using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryFacetsController : ControllerBase
    {
        /// <summary>
        /// Gets a list mapping story search facet identifiers to readable descriptions.
        /// </summary>
        /// <returns>A JSON document containing the mapping.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(StoryFacets), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return File("~/storyFacets.json", "application/json; charset=utf-8");
        }
    }
}