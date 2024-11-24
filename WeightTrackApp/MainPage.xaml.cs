using WeightTrackApp.ViewModels;

namespace WeightTrackApp
{
    /// <summary>
    /// Represents the main page of the WeightTrackApp application.
    /// Provides the user interface for managing weight entries.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly WeightViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        /// <param name="viewModel">The ViewModel used for binding data to the page.</param>
        public MainPage(WeightViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Invoked when the page appears on the screen.
        /// Loads weight data asynchronously and updates the UI.
        /// </summary>
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadWeightASync();
        }
    }
}
