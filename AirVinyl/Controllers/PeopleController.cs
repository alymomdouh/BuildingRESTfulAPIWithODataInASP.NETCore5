using AirVinyl.API.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    public class PeopleController : ODataController
    {
        private readonly AirVinylDbContext dbContext;

        public PeopleController(AirVinylDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        // #RequestInPostman  http://localhost:5000/odata/People
        public async Task<IActionResult> Get()
        {
            return Ok(await dbContext.People.ToListAsync());
        }
        // People(1)
        //get http://localhost:5000/odata/People(1)
        public async Task<IActionResult> Get(int key)
        {
            var person = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person);
        }
    }
}
