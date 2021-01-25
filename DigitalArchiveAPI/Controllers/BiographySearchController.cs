using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.Models;
using DigitalArchiveAPI.AzureServices;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides data to the biography search (aka HistoryMakers) view.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BiographySearchController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public BiographySearchController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Performs a full text search of the biography index.
        /// </summary>
        /// <param name="query">The query terms.</param>
        /// <param name="pageSize">Number of results to return per page.</param>
        /// <param name="currentPage">Retreive the nth page of results based on given pageSize.</param>
        /// <param name="searchFields">A comma-separated list of fields to be searched.</param>
        /// <param name="genderFacet">Filter results by the given gender ['M' | 'F'].</param>
        /// <param name="stateFacet">Filter results by the given U.S. state of birth.</param>
        /// <param name="yearFacet">Filter results by the given year of birth.</param>
        /// <param name="makerFacet">Filter results by the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter results by the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by the first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="sortField">Sort results by this field, e.g., lastName;  default is empty (meaning query relevance ranking is used to sort)</param>
        /// <param name="sortInDescendingOrder">Sort results in descending order if true; default is false so default sort order is ascending.</param>
        /// <returns>
        /// A BiographyResultSet document containing the results of the search operation.
        /// </returns>
        /// <remarks>
        /// If searchFields is "all" or not specified, it will be taken to be descriptionShort, accession, lastName, preferredName.
        /// If query is "all" via "*" wildcard and sortField is not specified, query relevance ranking will be taken to be "lastName ascending".
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<BiographyResultSet>> Get(
            string query = "",
            int? pageSize = 20,
            int? currentPage = 1,
            string searchFields = "all",
            string genderFacet = "",
            string stateFacet = "",
            string yearFacet = "",
            string makerFacet = "",
            string jobFacet = "",
            string lastInitialFacet = "",
            string sortField = "",
            bool? sortInDescendingOrder = false )
        {
            // NOTE: One override: to add in a sortField of "lastName" if the query is empty or just the wildcard * for all results,
            // and the sortField is also not originally specified, with sorting on last name to be in ascending order.
            var sortFieldOverride = sortField;
            var sortInDescendingOrderOverride = sortInDescendingOrder;
            if ((string.IsNullOrEmpty(query) || query.Trim() == "*") && string.IsNullOrEmpty(sortField))
            {
                sortFieldOverride = "lastName";
                sortInDescendingOrderOverride = false;
            }

            var results = await azureSearch.BiographySearch(
                query ?? "",
                pageSize ?? 20,
                currentPage ?? 1,
                searchFields ?? "all",
                genderFacet ?? "",
                stateFacet ?? "",
                yearFacet ?? "",
                makerFacet ?? "",
                jobFacet ?? "",
                lastInitialFacet ?? "",
                sortFieldOverride ?? "",
                sortInDescendingOrderOverride ?? false
            ).ConfigureAwait(false);

            return new BiographyResultSet
            {
                Facets = results.Facets,
                Biographies = results.Results,
                Count = results.Count
            };
        }
    }
}