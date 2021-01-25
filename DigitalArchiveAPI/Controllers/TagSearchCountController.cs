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
    public class TagSearchCountController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        public TagSearchCountController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Return the number of stories matching each tag filtered by the given list of tags.
        /// </summary>
        /// <param name="csvTagList">A comma-separated list of tag identifiers.</param>
        /// <returns>A StoryResultSet containing the results of the search operation.</returns>
        [HttpGet]
        public async Task<ActionResult<StoryResultSet>> Get(string csvTagList)
        {
            var results = await azureSearch.StorySearchTagCounts(csvTagList).ConfigureAwait(false);

            return new StoryResultSet
            {
                Facets = results.Facets,
                Stories = null,
                Count = results.Count
            };
        }
    }
}