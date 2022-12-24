using AirVinyl.API.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System; 
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    [Route("odata")]// this from Microsoft.AspNetCore.Mvc
    public class VinylRecordsController : ODataController
    {
        private readonly AirVinylDbContext dbContext;

        public VinylRecordsController(AirVinylDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet("VinylRecords")]
        // Older version
        //[HttpGet]
        //[ODataRoute("VinylRecords")]
        public async Task<IActionResult> GetAllVinylRecords()
        {
            return Ok(await dbContext.VinylRecords.ToListAsync());
        }
        //get http://localhost:5000/odata/VinylRecords(1)
        [HttpGet("VinylRecords({key})")]
        public async Task<IActionResult> GetOneVinylRecord(int key)
        {
            var vinylRecord = await dbContext.VinylRecords.FirstOrDefaultAsync(p => p.VinylRecordId == key);
            if (vinylRecord == null)
            {
                return NotFound();
            }
            return Ok(vinylRecord);
        } 
    }
}
