using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace JcdMockApi.Controllers
{
    /// <summary> 
    /// A controller for wombats, who quickly get out of hand if left to their own devices.
    /// </summary> 
    [Route("[controller]")]
    [ApiController]
    public class WombatsController : ControllerBase
    {
        private readonly string[] wombats = { "Northern Hairy-nosed Wombat", "Southern Hairy-nosed Wombat", "Plain-nosed Wombat"};

        /* For a method called GetEdit that takes an id as an argument
        [HttpGet("edit/{id}")]
        [ProducesResponseType(typeof(HttpResult), (int) HttpStatusCode.OK)]

        For a Get method:
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HttpResult), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Models.NpiGroup>> Get(int id)


        [HttpGet("search")]
        [ProducesResponseType(typeof(HttpResult), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<NpiSearch>>> Search(string search)
        */


        /// <summary> 
        /// Get me the wombats. All of them.
        /// </summary> 
        [HttpGet]
        [Produces("application/json", Type = typeof(string))]
        [SwaggerOperation(
        Summary = "Gets all the wombats",
        Description = "Just like the summary. If you want all the wombats, look here. If you want fewer than that, try something else.",
        OperationId = "GetAll",
        Tags = new[] { "Wombats", "GET" }
        )]
        public ActionResult<string> Get()
        {
            string jsonString;
            jsonString = JsonSerializer.Serialize(wombats);
            return jsonString;
        }

        /// <summary> 
        /// Get me a wombat. A specific one.
        /// </summary> 
        /// <param name="id"> The id of the wombat we want.</param>
        [HttpGet("{id}")]
        [Produces("application/json", Type = typeof(string))]
        [SwaggerOperation(
        Summary = "Get just one wombat, by ID.",
        Description = "Just one wombat, but you have to know exactly the one you want.",
        OperationId = "GetOne",
        Tags = new[] { "Wombats", "GET" }
        )]
        public ActionResult<string> Get(int id)
        {
            if (id >= wombats.Length || id < 0)
                throw new IndexOutOfRangeException("That's not a valid wombat index. Try a number between 0 and the max number of wombats.");

            return JsonSerializer.Serialize("one wombat");
        }

        /// <summary> 
        /// Create brand new wombats
        /// </summary> 
        /// <param name="wombats">The information about the wombats we want to create</param>
        [HttpPost]
        [SwaggerOperation(
        Summary = "Create new wombats",
        OperationId = "Post",
        Tags = new[] { "Wombats", "POST" }
        )]
        public void Post([FromBody] string wombats)
        {
        }

        /// <summary> 
        /// Update a wombat
        /// </summary> 
        /// <param name="id">The id of the wombat to update</param>
        /// <param name="wombatValue">The value to use in updating the wombat</param>
        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Update a known wombat",
        OperationId = "PutWombat",
        Tags = new[] { "Wombats", "PUT" }
        )]
        public void Put(int id, [FromBody] string wombatValue)
        {
        }

        /// <summary> 
        /// Delete a wombat
        /// </summary> 
        /// <param name="id">The id of the wombat to delete</param>
        [HttpDelete("{id}")]
        [SwaggerOperation(
        Summary = "Delete a known wombat",
        OperationId = "DeleteWombat",
        Tags = new[] { "Wombats", "DELETE" }
        )]
        public void Delete(int id)
        {
        }
    }
}
