using System;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;

namespace DigitalArchiveAPI.Models
{
    /// <summary>
    /// The set of biographies returned by an Azure Search query.
    /// </summary>
    public class BiographyResultSet
    {
        /// <summary>
        /// The list of facets for the current result set as returned by Azure Search.
        /// </summary>
        public IDictionary<string, IList<FacetResult>> Facets { get; set; }

        /// <summary>
        /// The list Biography documents matching the search criteria.
        /// </summary>
        public IList<SearchResult<BiographySearchDocument>> Biographies { get; set; }

        /// <summary>
        /// Total number of biographies matching the search criteria.
        /// </summary>
        public long? Count { get; set; }
    }

    /// <summary>
    /// A streamlined Biography document as returned by a biography search.
    /// </summary>
    public class BiographySearchDocument
    {
        /// <summary>
        /// Document key analogous to CollectionID.
        /// </summary>
        public string BiographyID { get; set; }

        /// <summary>
        /// Accession number, the preferred unique identifier for an interview subject
        /// </summary>
        public string Accession { get; set; }

        /// <summary>
        /// Date of birth, may be null
        /// </summary>
        public DateTimeOffset? BirthDate { get; set; }

        /// <summary>
        /// Interview subject's full preferred name
        /// </summary>
        public string PreferredName { get; set; }
    }
}
