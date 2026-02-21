using HomeControl.Models.ServicesInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Controllers
{
    [ApiController]
    [Route("database/structure")]
    public class DatabaseStructureController(IDatabaseConnectionService databaseConnectionService) : Controller
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateDatabaseStructure()
        {
            await databaseConnectionService.CreateDatabaseStructure();

            return Ok();
        }
    }
}
