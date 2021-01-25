using System.Collections.Generic;

namespace DigitalArchiveAPI.Models
{
    public class BiographyFacets
    {
        /// <summary>
        /// Full list of maker category id:description pairs.
        /// </summary>
        public List<FacetIdLabel> MakerCategories { get; set; }

        /// <summary>
        /// Full list of occupation type id:description pairs.
        /// </summary>
        public List<FacetIdLabel> OccupationTypes { get; set; }
    }

    public class StoryFacets
    {
        /// <summary>
        /// Full list of maker category id:description pairs.
        /// </summary>
        public List<FacetIdLabel> MakerCategories { get; set; }

        /// <summary>
        /// Full list of occupation type id:description pairs.
        /// </summary>
        public List<FacetIdLabel> OccupationTypes { get; set; }

        /// <summary>
        /// Full list of named entity country id:description pairs.
        /// </summary>
        public List<FacetIdLabel> EntityCountries { get; set; }

        /// <summary>
        /// Full list of named entity organization id:description pairs.
        /// </summary>
        public List<FacetIdLabel> EntityOrganizations { get; set; }
    }


    /// <summary>
    /// Maps a single facet id to a description.
    /// </summary>
    public class FacetIdLabel
    {
        /// <summary>
        /// The identifier for this facet used by the search index.
        /// </summary>
        public string ID { get; set; }


        /// <summary>
        /// The full description of this value.
        /// </summary>
        public string Label { get; set; }
    }
}
