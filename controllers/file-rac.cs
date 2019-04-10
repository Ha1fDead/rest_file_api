using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace maplarge_restapicore.controllers
{

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            return "hello world";
        }
    }
}