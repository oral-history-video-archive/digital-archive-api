using System;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;

namespace DigitalArchiveAPI.Models
{
    /// <summary>
    /// The set of stories returned by an Azure Search query.
    /// </summary>
    public class StoryResultSet
    {
        /// <summary>
        /// The facets for the current result set as returned by Azure Search.
        /// </summary>
        public IDictionary<string, IList<FacetResult>> Facets { get; set; }

        /// <summary>
        /// The list story documents matching the search criteria.
        /// </summary>
        public IList<SearchResult<StorySearchDocument>> Stories { get; set; }

        /// <summary>
        /// Total number of stories matching the search criteria.
        /// </summary>
        public long? Count { get; set; }
    }

    /// <summary>
    /// A streamlined Story document as returned by a story search.
    /// </summary>
    public class StorySearchDocument
    {
        /// <summary>
        /// Document key analogous to SegmentID.
        /// </summary>
        public string StoryID { get; set; }

        /// <summary>
        /// The parent biography's accession identifier.
        /// </summary>
        public string BiographyID { get; set; }

        /// <summary>
        /// 1-based session order within biography.
        /// </summary>
        public int? SessionOrder { get; set; }

        /// <summary>
        /// 1-based tape order.
        /// </summary>
        public int? TapeOrder { get; set; }

        /// <summary>
        /// 1-based order within the story set
        /// </summary>
        public int? StoryOrder { get; set; }

        /// <summary>
        /// Story duration in milliseconds.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Date interview was conducted.
        /// </summary>
        public DateTimeOffset? InterviewDate { get; set; }

        /// <summary>
        /// Human generated story title.
        /// </summary>
        public string Title { get; set; }
    }
}
