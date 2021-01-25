using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.AzureServices;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides a list of biographies (people) born this month.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleBornThisMonthController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public PeopleBornThisMonthController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Retrieve a list of people born this month with optional filtering.
        /// </summary>
        /// <param name="pageSize">Number of results to return.</param>
        /// <param name="currentPage">Retreive the nth page of results based on given page size.</param>
        /// <param name="genderFacet">Filter results by the given gender.</param>
        /// <param name="yearFacet">Filter results by the given year.</param>
        /// <param name="makerFacet">Filter results by the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter results by the given comma-separated list of job types.</param>
        /// <param name="lastInitialFacet">Filter results by first letter of last name, e.g., C is filtering to all last names starting with C.</param>
        /// <param name="dateTodayFacet">Today's date.</param>
        /// <returns>
        /// A BiographyResultSet containing the results of the search operation.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<BiographyResultSet>> Get(
            int? pageSize = 20,
            int? currentPage = 1,
            string genderFacet = "",
            string yearFacet = "",
            string makerFacet = "",
            string jobFacet = "",
            string lastInitialFacet = "",
            string dateTodayFacet = "")
        {
            var results = await azureSearch.GetPeopleBornThisMonth(
                pageSize ?? 20,
                currentPage ?? 1,
                genderFacet ?? "",
                yearFacet ?? "",
                makerFacet ?? "",
                jobFacet ?? "",
                lastInitialFacet ?? "",
                dateTodayFacet ?? ""
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
