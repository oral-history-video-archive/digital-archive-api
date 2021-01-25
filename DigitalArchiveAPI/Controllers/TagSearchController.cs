using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.AzureServices;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides data to the topic search view.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TagSearchController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public TagSearchController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Search for stories containing the given list of tags.
        /// </summary>
        /// <param name="csvTagList">A comma-separated list of tag identifiers.</param>
        /// <param name="pageSize">Number of results to return.</param>
        /// <param name="currentPage">Retreive the nth page of results based on given page size.</param>
        /// <param name="genderFacet">Filter results by the given gender.</param>
        /// <param name="yearFacet">Filter results by the given year.</param>
        /// <param name="makerFacet">Filter results by the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter results by the given comma-separated list of job types.</param>
        /// <param name="entityStateFacet">Filter results to the given comma-separated list of mentioned U.S. states.</param>
        /// <param name="entityCountryFacet">Filter results to the given comma-separated list of mentioned countries.</param>
        /// <param name="entityOrgFacet">Filter results to the given comma-separated list of mentioned organizations.</param>
        /// <param name="entityDecadeFacet">Filter results to the given comma-separated list of mentioned decades.</param>
        /// <param name="entityYearFacet">Filter results to the given comma-separated list of mentioned years.</param>
        /// <param name="sortField">Sort results by this field, e.g., interviewDate; default is empty (meaning query relevance ranking is used to sort).</param>
        /// <param name="sortInDescendingOrder">If sortField is given, sort results in descending order if true; default is false so default sort order is ascending.</param>

        /// <returns>
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<StoryResultSet>> Get(
            string csvTagList,
            int? pageSize = 20,
            int? currentPage = 1,
            string genderFacet = "",
            string yearFacet = "",
            string makerFacet = "",
            string jobFacet = "",
            string entityStateFacet = "",
            string entityCountryFacet = "",
            string entityOrgFacet = "",
            string entityDecadeFacet = "",
            string entityYearFacet = "",
            string sortField = "",
            bool? sortInDescendingOrder = false)
        {
            var results = await azureSearch.StorySearchByTags(
                csvTagList ?? "",
                pageSize ?? 20,
                currentPage?? 1,
                genderFacet ?? string.Empty,
                yearFacet ?? string.Empty,
                makerFacet ?? string.Empty,
                jobFacet ?? string.Empty,
                entityStateFacet ?? string.Empty,
                entityCountryFacet ?? string.Empty,
                entityOrgFacet ?? string.Empty,
                entityDecadeFacet ?? string.Empty,
                entityYearFacet ?? string.Empty,
                sortField ?? "",
                sortInDescendingOrder ?? false
            ).ConfigureAwait(false);

            return new StoryResultSet
            {
                Facets = results.Facets,
                Stories = results.Results,
                Count = results.Count
            };
        }
    }
}