using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using DigitalArchiveAPI.Models;
using DigitalArchiveAPI.AzureServices;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides data to the biography details view.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BiographyDetailsController : ControllerBase
    {
        private readonly AzureStorage azureStorage;

        public BiographyDetailsController(IConfiguration configuration)
        {
            azureStorage = new AzureStorage(configuration?["AzureStorage:ConnectionString"]);
        }

        /// <summary>
        /// Gets the full details for the specified biography.
        /// </summary>
        /// <param name="accession">The unique biography identifier.</param>
        /// <returns>A JSON document containing the requested biography details.</returns>
        [HttpGet]
        public async Task<ActionResult<BiographyDetails>> Get(string accession)
        {
            if (accession is null)
            {
                return BadRequest();
            }

            var biographyDetails = await azureStorage.GetBiographyDetails(accession).ConfigureAwait(false);

            if (biographyDetails == null)
            {
                return NotFound();
            }

            return biographyDetails;
        }
    }
}