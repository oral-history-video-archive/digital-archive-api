using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalArchiveAPI.Models;

namespace DigitalArchiveAPI.Controllers
{
    /// <summary>
    /// Provides information about the tags (facet) returned by the search engine.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TagListController : ControllerBase
    {

        /// <summary>
        /// Gets a list mapping tag identifiers to readable descriptions.
        /// </summary>
        /// <returns>A JSON document containing the mapping.</returns>
        /// <remarks>
        /// Formerly known as api/TagList
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(TagTree), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return File("~/tagTree.json", "application/json; charset=utf-8");
        }
    }
}