using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using AirVinyl.Entities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
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

        //get http://localhost:5000/odata/People(1)/Email/$value
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

        //get http://localhost:5000/odata/People(1)/VinylRecords
        [HttpGet("odata/People({key})/VinylRecords")] // this for include navagation property 
        //[HttpGet("People({key})/Friends")]
        //[HttpGet("People({key})/Addresses")]
        public async Task<IActionResult> GetPersonCollectionProperty(int key)
        {
            var collectionPopertyToGet = new Uri(HttpContext.Request.GetEncodedUrl()).Segments.Last();
            var person = await dbContext.People.Include(collectionPopertyToGet).FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            if (!person.HasProperty(collectionPopertyToGet))
            {
                return NotFound();
            }
            return Ok(person.GetValue(collectionPopertyToGet));
        }

        /* data in post man 
         * headers
         *          Accept:application/json
         *          Content-Type:application/json
         * body 
         *       row  json 
         *                          {
                                        "FirstName":"hussam",
                                        "LastName":"Smith",
                                        "Email": "hussam.smith@someprovider.com",    
                                        "Gender":"Male",
                                        "DateOfBirth": "1980-01-25",
                                        "AmountOfCashToSpend":12.13,
                                        "NumberOfRecordsOnWishList":15
                                    } 
         */

        [HttpPost("odata/People")]
        public async Task<IActionResult> CreatePerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // add the person to the People collection
            await dbContext.People.AddAsync(person);
            await dbContext.SaveChangesAsync();
            // return the created person 
            return Created(person);
        }
        // put //http://localhost:5000/odata/People(3)
        /*   // request like a create 
         *  headers
         *          Accept:application/json
         *          Content-Type:application/json
         * body row json 
             {    
                "FirstName": "Nick",
                "LastName": "Missorten",
                "DateOfBirth": "1983-05-18T00:00:00+02:00",
                "Gender": "Male",
                "NumberOfRecordsOnWishList": 23,
                "AmountOfCashToSpend": 2500
            }
         */
        [HttpPut("odata/People({key})")]
        public async Task<IActionResult> UpdatePerson(int key, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentPerson = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();

                // Alternative: if the person isn't found: Upsert.  This must only
                // be used if the responsibility for creating the key isn't at 
                // server-level.  In our case, we're using auto-increment fields,
                // so this isn't allowed - code is for illustration purposes only!
                //if (currentPerson == null)
                //{
                //    // the key from the URI is the key we should use
                //    person.PersonId = key;
                //    _airVinylDbContext.People.Add(person);
                //    await _airVinylDbContext.SaveChangesAsync();
                //    return Created(person);
                //}

            }
            person.PersonId = currentPerson.PersonId;
            dbContext.Entry(currentPerson).CurrentValues.SetValues(person);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
        //patch http://localhost:5000/odata/People(3)
        /* headers a same preve request 
         * body row json 
         * {   
                "FirstName": "Nick",
                "Email": "nick@someprovider.com"
            }
         */
        [HttpPatch("odata/People({key})")]
        public async Task<IActionResult> PartiallyUpdatePerson(int key,[FromBody] Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            var currentPerson = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key); 
            if (currentPerson == null)
            {
                return NotFound();
            } 
            patch.Patch(currentPerson);
            await dbContext.SaveChangesAsync(); 
            return NoContent();
        }
    }
}
