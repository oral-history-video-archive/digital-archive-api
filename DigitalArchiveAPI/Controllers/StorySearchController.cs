using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.Models;
using DigitalArchiveAPI.AzureServices;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provide data to the story search view.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StorySearchController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public StorySearchController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Performs full-text search of the stories index.
        /// </summary>
        /// <param name="query">The query terms.</param>
        /// <param name="pageSize">Number of results to return per page.</param>
        /// <param name="currentPage">Retreive the nth page of results based on given pageSize.</param>
        /// <param name="parentBiographyID">Only retrieve stories belonging to this biography (if the ID is non-empty).</param>
        /// <param name="searchFields">Comma-separated list of fields to be searched.</param>
        /// <param name="interviewYearFilterLowerBound">Only return stories with interview date on or after this year (if given).</param>
        /// <param name="interviewYearFilterUpperBound">Only return stories with interview date on or before this year (if given).</param>
        /// <param name="genderFacet">Filter results by the given gender.</param>
        /// <param name="yearFacet">Filter results by the given year of birth.</param>
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
        /// A StoryResultSet document containing the results of the search operation.
        /// </returns>        
        /// <remarks>
        /// Optional parameters are created by assigning them default values.
        /// If searchFields is "all" or not specified, it will be taken to be title,transcript.
        /// 
        /// Based on advice from https://docs.microsoft.com/en-us/rest/api/searchservice/simple-query-syntax-in-azure-search
        /// the search mode is locked down to "all".
        /// 
        /// Details: 
        /// Negated terms will not be understood with match any, e.g., that buffalo -soldier will return 10000s of
        /// stories, all those NOT containing soldier unioned with all the stories matching buffalo.  Instead, to get
        /// buffalo AND not soldier, the underlying searchMode should be all.  The same is true for when complex
        /// queries are given with precedence and with and (+) and or (|) as in buffalo+(soldier|bill).  This is interpreted 
        /// fine as long as the underlying searchMode is all (not any).          
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<StoryResultSet>> Get(
            string query = "",
            int? pageSize = 20,
            int? currentPage = 1,
            string parentBiographyID = "",
            string searchFields = "all",
            int? interviewYearFilterLowerBound = 0,
            int? interviewYearFilterUpperBound = 0,
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
            var results = await azureSearch.StorySearch(
                query ?? string.Empty,
                pageSize ?? 20,
                currentPage ?? 1,
                parentBiographyID ?? string.Empty,
                searchFields ?? "all",
                interviewYearFilterLowerBound ?? 0,
                interviewYearFilterUpperBound ?? 0,
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

            var stories = new StoryResultSet
            {
                Facets = results.Facets,
                Stories = results.Results,
                Count = results.Count
            };

            return stories;
        }
    }
}
