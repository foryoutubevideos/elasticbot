using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Bot : ControllerBase
    {
        readonly Service _service = null;

        public Bot(IConfiguration configuration)
        {
            _service = new Service(configuration);
        }

        [HttpGet("{query}")]
        public async Task<IActionResult> SearchAsync(string query)
        {
            if (string.IsNullOrEmpty(query)) return BadRequest();

            var result = await _service.Search(query);

            if (result == null) return StatusCode(StatusCodes.Status500InternalServerError);

            return Ok(result);
        }
    }
}
