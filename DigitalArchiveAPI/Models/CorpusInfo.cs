using System;

namespace DigitalArchiveAPI.Models
{
    /// <summary>
    /// Summary information used in home view and tag view blurbs
    /// </summary>
    public class CorpusInfo
    {
        /// <summary>
        /// TODO: Use today's date until a mechanism is put into place for tracking Azure updates.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Total number of indexed biographies and the subset of those with tags.
        /// </summary>
        public SetCounts Biographies { get; set; }

        /// <summary>
        /// Total number of indexed stories and the subset of those with tags.
        /// </summary>
        public SetCounts Stories { get; set; }
    }

    /// <summary>
    /// Integer counts for the full set of All and the subset of Tagged
    /// Biographies or Stories.
    /// </summary>
    public class SetCounts
    {
        public long? All { get; set; }
        public long? Tagged { get; set; }
        public long? ScienceMakerCount { get; set; }
    }
}
