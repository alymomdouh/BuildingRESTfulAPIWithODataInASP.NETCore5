using AirVinyl.API.DbContexts;
using AirVinyl.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    [Route("odata")]
    public class SingletonController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbContext;
        public SingletonController(AirVinylDbContext airVinylDbContext)
        {
            _airVinylDbContext = airVinylDbContext ?? throw new ArgumentNullException(nameof(airVinylDbContext));
        }

        // GET Singleton
        // get http://localhost:5000/odata/Tim
        // get http://localhost:5000/odata/Tim/Email
        // get http://localhost:5000/odata/Tim/Email/$value
        // get http://localhost:5000/odata/Tim/VinylRecords
        [HttpGet("Tim")]
        public async Task<IActionResult> GetSingletonTim()
        {
            // find Tim - he's got id 5
            var personTim = await _airVinylDbContext.People.FirstOrDefaultAsync(p => p.PersonId == 5);
            return Ok(personTim);
        }

        // patch http://localhost:5000/odata/Tim
        /*
            {
                "NumberOfRecordsOnWishList": 30,
                "AmountOfCashToSpend": 200
            }
         */
        [HttpPatch("Tim")]
        public async Task<IActionResult> PartiallyUpdateTim([FromBody] Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // find Tim
            var currentPerson = await _airVinylDbContext.People.FirstOrDefaultAsync(p => p.PersonId == 5);
            // apply the patch, and save the changes
            patch.Patch(currentPerson);
            await _airVinylDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
