using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.Models;
using DigitalArchiveAPI.AzureServices;

namespace DigitalArchiveAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StorySetController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public StorySetController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Retrieve an ordered set of stories.
        /// </summary>
        /// <param name="csvStoryIDs">Comma-separated list of story identifiers.</param>
        /// <param name="genderFacet">Filter results by the given gender.</param>
        /// <param name="yearFacet">Filter results by the given year.</param>
        /// <param name="makerFacet">Filter results by the given comma-separated list of maker categories.</param>
        /// <param name="jobFacet">Filter results by the given comma-separated list of job types.</param>
        /// <param name="entityStateFacet">Filter results to the given comma-separated list of mentioned U.S. states.</param>
        /// <param name="entityCountryFacet">Filter results to the given comma-separated list of mentioned countries.</param>
        /// <param name="entityOrgFacet">Filter results to the given comma-separated list of mentioned organizations.</param>
        /// <param name="entityDecadeFacet">Filter results to the given comma-separated list of mentioned decades.</param>
        /// <param name="entityYearFacet">Filter results to the given comma-separated list of mentioned years.</param>
        /// <returns>
        /// A StoryResultSet containing the stories specified by the given list.
        /// </returns>        
        [HttpGet]
        public async Task<ActionResult<StoryResultSet>> Get(
            string csvStoryIDs = "",
            string genderFacet = "",
            string yearFacet = "",
            string makerFacet = "",
            string jobFacet = "",
            string entityStateFacet = "",
            string entityCountryFacet = "",
            string entityOrgFacet = "",
            string entityDecadeFacet = "",
            string entityYearFacet = "")
        {
            csvStoryIDs ??= string.Empty;

            var storyList = new List<string>();

            foreach (var item in csvStoryIDs.Split(','))
            {
                var id = item.Trim();
                if (id.Length != 0) storyList.Add(id);
            }

            var results = await azureSearch.StorySet(
                storyList,
                genderFacet ?? string.Empty,
                yearFacet ?? string.Empty,
                makerFacet ?? string.Empty,
                jobFacet ?? string.Empty,
                entityStateFacet ?? string.Empty,
                entityCountryFacet ?? string.Empty,
                entityOrgFacet ?? string.Empty,
                entityDecadeFacet ?? string.Empty,
                entityYearFacet ?? string.Empty
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