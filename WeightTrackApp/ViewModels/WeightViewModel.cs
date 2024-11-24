using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeightTrackApp.Data;
using WeightTrackApp.Models;
using System.Collections.ObjectModel;

namespace WeightTrackApp.ViewModels
{
    /// <summary>
    /// ViewModel for managing weight entries and providing data-binding for the UI.
    /// </summary>
    public partial class WeightViewModel : ObservableObject
    {
        private readonly DatabaseContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeightViewModel"/> class.
        /// </summary>
        /// <param name="context">The database context used for data operations.</param>
        public WeightViewModel(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Collection of all weight entries displayed in the UI.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Weights> _weights = new();

        /// <summary>
        /// The weight entry currently being operated on (create/edit).
        /// </summary>
        [ObservableProperty]
        private Weights _operatingWeights = new();

        /// <summary>
        /// Indicates whether a background operation is currently running.
        /// </summary>
        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// The text displayed to indicate the status of a background operation.
        /// </summary>
        [ObservableProperty]
        private string _busyText;

        /// <summary>
        /// Loads all weight entries from the database and populates the collection.
        /// </summary>
        public async Task LoadWeightASync()
        {
            await ExecuteAsync(async () =>
            {
                var weights = await _context.GetAllAsync<Weights>();
                if (weights is not null && weights.Any())
                {
                    Weights.Clear();
                    foreach (var weight in weights)
                    {
                        // Ensure all weight entries have valid dates
                        if (weight.Date == DateTime.MinValue)
                        {
                            weight.Date = DateTime.Now;
                            await _context.UpdateItemAsync(weight);
                        }
                        Weights.Add(weight);
                    }
                }
            }, "Fetching weight entries...");
        }

        /// <summary>
        /// Sets the operating weight to a new or existing weight entry.
        /// </summary>
        /// <param name="weight">The weight entry to operate on. If null, creates a new entry.</param>
        [RelayCommand]
        private void SetOperatingWeight(Weights? weight)
        {
            OperatingWeights = weight ?? new Weights { Id = 0, Date = DateTime.Now };
        }

        /// <summary>
        /// Saves the current operating weight to the database, either creating or updating it.
        /// </summary>
        [RelayCommand]
        private async Task SaveWeightAsync()
        {
            if (OperatingWeights is null)
                return;

            var (isValid, errorMessage) = OperatingWeights.Validate();
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Validation Error", errorMessage, "Ok");
                return;
            }

            var busyText = OperatingWeights.Id == 0 ? "Creating weight..." : "Updating Weight...";
            await ExecuteAsync(async () =>
            {
                if (OperatingWeights.Id == 0)
                {
                    // Ensure valid date
                    if (OperatingWeights.Date == DateTime.MinValue)
                    {
                        OperatingWeights.Date = DateTime.Now;
                    }
                    // Create new weight record
                    await _context.AddItemAsync<Weights>(OperatingWeights);
                    Weights.Add(OperatingWeights);
                }
                else
                {
                    // Update existing weight record
                    if (await _context.UpdateItemAsync<Weights>(OperatingWeights))
                    {
                        var weightCopy = OperatingWeights.Clone();

                        var index = Weights.IndexOf(OperatingWeights);
                        Weights.RemoveAt(index);

                        Weights.Insert(index, weightCopy);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Weight entry update failed", "Ok");
                        return;
                    }
                }
                SetOperatingWeightCommand.Execute(new());
            }, busyText);
        }

        /// <summary>
        /// Deletes a weight entry by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the weight entry to delete.</param>
        [RelayCommand]
        private async Task DeleteWeightAsync(int id)
        {
            await ExecuteAsync(async () =>
            {
                if (await _context.DeleteItemByKeyAsync<Weights>(id))
                {
                    var weight = Weights.FirstOrDefault(p => p.Id == id);
                    Weights.Remove(weight);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Delete Error", "Weight entry was not deleted", "Ok");
                }
            }, "Deleting weight...");
        }

        /// <summary>
        /// Executes an asynchronous operation while managing the busy state and status text.
        /// </summary>
        /// <param name="operation">The asynchronous operation to execute.</param>
        /// <param name="busyText">The text to display during the operation.</param>
        private async Task ExecuteAsync(Func<Task> operation, string? busyText = null)
        {
            IsBusy = true;
            BusyText = busyText ?? "Processing...";
            try
            {
                await operation?.Invoke();
            }
            catch (Exception ex)
            {
                // Handle exception as necessary
            }
            finally
            {
                IsBusy = false;
                BusyText = "Processing...";
            }
        }
    }
}
