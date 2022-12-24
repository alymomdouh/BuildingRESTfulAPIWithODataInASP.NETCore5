using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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
        //http://localhost:5000/odata/People(1)/Email
        [HttpGet("odata/People({key})/Email")]
        [HttpGet("odata/People({key})/FirstName")]
        [HttpGet("odata/People({key})/LastName")]
        [HttpGet("odata/People({key})/DateOfBirth")]
        [HttpGet("odata/People({key})/Gender")]
        public async Task<IActionResult> GetPersonProperty(int key)
        {
            var person = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            var propertyToGet = new Uri(HttpContext.Request.GetEncodedUrl()).Segments.Last();
            // from AirVinyl.API.Helpers =>class PropertyValueHelpers
            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }
            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }
            return Ok(propertyValue);
        }

        [HttpGet("odata/People({key})/Email/$value")]
        [HttpGet("odata/People({key})/FirstName/$value")]
        [HttpGet("odata/People({key})/LastName/$value")]
        [HttpGet("odata/People({key})/DateOfBirth/$value")]
        [HttpGet("odata/People({key})/Gender/$value")]
        public async Task<IActionResult> GetPersonPropertyRawValue(int key)
        {
            var person = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            var url = HttpContext.Request.GetEncodedUrl();
            var propertyToGet = new Uri(url).Segments[^2].TrimEnd('/');
            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }
            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                // null = no content
                return NoContent();
            }
            return Ok(propertyValue.ToString());
        }
    }
}
