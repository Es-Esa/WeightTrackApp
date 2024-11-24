using SQLite;

namespace WeightTrackApp.Models
{
    /// <summary>
    /// Represents a weight entry with details such as ID, weight value, and date.
    /// </summary>
    public class Weights
    {
        /// <summary>
        /// Gets or sets the unique identifier for the weight entry.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the weight value in kilograms.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Gets or sets the date of the weight entry.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="Weights"/> object.
        /// </summary>
        /// <returns>A new <see cref="Weights"/> object with the same values.</returns>
        public Weights Clone() => MemberwiseClone() as Weights;

        /// <summary>
        /// Validates the current weight entry to ensure all required properties are valid.
        /// </summary>
        /// <returns>
        /// A tuple where <c>IsValid</c> is true if the weight is valid; otherwise, false.
        /// <c>ErrorMessage</c> contains the validation error message, if any.
        /// </returns>
        public (bool IsValid, string? ErrorMessage) Validate()
        {
            if (Weight <= 0)
            {
                return (false, $"{nameof(Weight)}: {Weight}");
            }
            return (true, null);
        }
    }
}
