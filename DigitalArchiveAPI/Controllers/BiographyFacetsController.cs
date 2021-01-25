using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BiographyFacetsController : ControllerBase
    {
        /// <summary>
        /// Gets a list mapping biography search facet identifiers to readable descriptions.
        /// </summary>
        /// <returns>A JSON document containing the mapping.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(BiographyFacets), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return File("~/biographyFacets.json", "application/json; charset=utf-8");
        }
    }
}