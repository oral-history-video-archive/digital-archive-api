using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.AzureServices;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorpusInfoController : ControllerBase
    {
        private readonly AzureSearch azureSearch;

        /// <summary>
        /// Provides an overview of the corpus to the home view.
        /// </summary>
        /// <param name="configuration"></param>
        public CorpusInfoController(IConfiguration configuration)
        {
            azureSearch = new AzureSearch(configuration?["AzureSearch:ServiceName"], configuration?["AzureSearch:ApiKey"]);
        }

        /// <summary>
        /// Retrieve information about the corpus useful to the home view.
        /// </summary>
        /// <returns>
        /// A JSON document containing the total count of indexed biographies and stories.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<CorpusInfo>> Get()
        {
            return await azureSearch.CorpusMetrics().ConfigureAwait(false);
        }
    }
}
