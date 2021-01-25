﻿namespace DigitalArchiveAPI.Models
{
    /// <summary>
    /// Interview subject's answers to questions about their favorite things.
    /// </summary>
    public class FavoritesSet
    {
        /// <summary>
        /// Subject's favorite color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Subject's favorite food.
        /// </summary>
        public string Food { get; set; }

        /// <summary>
        /// Subject's favorite quote.
        /// </summary>
        public string Quote { get; set; }

        /// <summary>
        /// Subject's favorite time of year.
        /// </summary>
        public string TimeOfYear { get; set; }

        /// <summary>
        /// Subject's favorite vacation spot.
        /// </summary>
        public string VacationSpot { get; set; }
    }
}
