using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using AirVinyl.Entities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
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
        //[EnableQuery] // for enable select and work 
        //get http://localhost:5000/odata/People?$select=Email
        //get http://localhost:5000/odata/People?$select=Email,FirstName

        //test with Expand
        // get http://localhost:5000/odata/People?$expand=VinylRecords
        //muilt layers Expand
        // get http://localhost:5000/odata/People?$expand=VinylRecords($expand=PressingDetail)
        //[EnableQuery(MaxExpansionDepth =1)]//this to not allowed to user make multi expand inside
        // merge select with expand 
        // get http://localhost:5000/odata/People?$select=Email,FirstName&$expand=VinylRecords
        // get http://localhost:5000/odata/People?$select=Email,FirstName&$expand=VinylRecords($select=Title)
        // get http://localhost:5000/odata/People?$select=Email,FirstName&$expand=VinylRecords($select=Title;$expand=PressingDetail($select=Grams))

        // test order by default is asc
        // get http://localhost:5000/odata/People?$orderby=Email
        // get http://localhost:5000/odata/People?$orderby=Email desc
        // get http://localhost:5000/odata/People?$orderby=Gender, Email
        // get http://localhost:5000/odata/People?$orderby=Gender desc, Email
        // merage order by with expand
        // get http://localhost:5000/odata/People?$orderby=Gender desc, Email desc&$expand=VinylRecords($orderby=Title)
        // merage order by with expand with select 
        // get http://localhost:5000/odata/People?$orderby=Gender desc, Email desc&$expand=VinylRecords($select=Title;$orderby=Title)
        // test Paging
        //get http://localhost:5000/odata/People?$top=2
        //get http://localhost:5000/odata/People?$top=2&$skip=15    // this will fail 
        //[EnableQuery(MaxTop =2,MaxSkip =2)]
        [EnableQuery(MaxTop = 2, MaxSkip = 2, PageSize = 4, MaxExpansionDepth = 2)]
        // get http://localhost:5000/odata/People?$top=2
        // get http://localhost:5000/odata/People?$top=5
        // get http://localhost:5000/odata/People?$count=true    /// this will inculde counter 
        // get http://localhost:5000/odata/People?$count=true&$skip=2   // this will include counter and skip 
        // include count in multi layers 
        // get http://localhost:5000/odata/People?$count=true&$expand=VinylRecords($count=true)
        // test filters 
        // get http://localhost:5000/odata/People?$filter=FirstName eq 'Kevin'                           /// equal 
        // get http://localhost:5000/odata/People?$filter=PersonId gt 3                                  /// greater than 
        // get http://localhost:5000/odata/People?$filter=DateOfBirth le 1981-05-05T00:00:00Z            /// less than  
        // get http://localhost:5000/odata/People?$filter=Gender eq AirVinyl.Gender'Female'       // this not working with me not know what is the erore
        // get http://localhost:5000/odata/People?$filter=PersonId lt 3 and FirstName eq 'Kevin'
        // get http://localhost:5000/odata/People?$filter=PersonId lt 3 or FirstName eq 'Kevin'
        // merge 
        // get http://localhost:5000/odata/People?$expand=VinylRecords($filter=Year ge 2000)
        // get http://localhost:5000/odata/People?$expand=VinylRecords($filter=Year eq null)
        // get http://localhost:5000/odata/People?$expand=VinylRecords($filter=PressingDetail/Grams ge 100)
        // get http://localhost:5000/odata/People?$filter=VinylRecords/any(vr:vr/Artist eq 'Arctic Monkeys')&$expand=VinylRecords
        // get http://localhost:5000/odata/People?$filter=VinylRecords/all(vr:vr/Artist eq 'Arctic Monkeys')&$expand=VinylRecords

        // test filter arithmetic operator ==>  boolean condition
        // get http://localhost:5000/odata/People?$filter=NumberOfRecordsOnWishList add 10 eq 20  ///that mean get all rows that make condition (((columnName)+10)==20)
        // get http://localhost:5000/odata/People?$filter=AmountOfCashToSpend div NumberOfRecordsOnWishList gt 10
        // get http://localhost:5000/odata/People?$filter=AmountOfCashToSpend add 1500 div 3 gt 600
        // get http://localhost:5000/odata/People?$filter=(AmountOfCashToSpend add 1500) div 3 gt 600

        // test filter with canonical function (can combined multi function )
        // get http://localhost:5000/odata/People?$filter=endswith(FirstName, 'n')
        // get http://localhost:5000/odata/People?$filter=length(Email) eq 26
        // get http://localhost:5000/odata/People?$filter=year(DateOfBirth) eq 1981
        // get http://localhost:5000/odata/People?$filter=length(Email) eq 26 and endswith(FirstName, 'n')
        // get 

        public async Task<IActionResult> Get()
        {
            return Ok(await dbContext.People.ToListAsync());
        }
        // People(1)
        //get http://localhost:5000/odata/People(1)
        [EnableQuery] // for enable select and work only sebfic properties
                      //get http://localhost:5000/odata/People(1)?$select=Email,FirstName
                      // expand
                      //get http://localhost:5000/odata/People(1)?$select=Email,FirstName&$expand=VinylRecords($select=Title)
                      // test order by default is asc
                      // get http://localhost:5000/odata/People(1)?$orderby=Gender desc, Email desc&$expand=VinylRecords($select=Title;$orderby=Title)
                      //get http://localhost:5000/odata/People(1)?$expand=VinylRecords($select=Title;$orderby=Title)

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
        [EnableQuery] // for enable select and work only sebfic properties
        //get http://localhost:5000/odata/People(1)/VinylRecords?$select=Title
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
            //return Ok(SingleResult.Create(person));
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
        //get http://localhost:5000/odata/People(1)/VinylRecords?$select=Title
        [EnableQuery] // for enable select and work only sebfic properties
        //
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
                //    dbContext.People.Add(person);
                //    await dbContext.SaveChangesAsync();
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
        public async Task<IActionResult> PartiallyUpdatePerson(int key, [FromBody] Delta<Person> patch)
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

        [HttpDelete("odata/People({key})")]
        public async Task<IActionResult> DeleteOnePerson(int key)
        {
            var currentPerson = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }
            dbContext.People.Remove(currentPerson);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        // get http://localhost:5000/odata/People(1)/VinylRecords

        [HttpGet("odata/People({key})/VinylRecords")]
        [EnableQuery]
        public IActionResult GetVinylRecordsForPerson(int key)
        {
            var person = dbContext.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            //return Ok(dbContext.VinylRecords.Where(v => v.Person.PersonId == key));
            return Ok(dbContext.VinylRecords.Include("DynamicVinylRecordProperties").Where(v => v.Person.PersonId == key));
        }

        // get http://localhost:5000/odata/People(1)/VinylRecords(1)

        [HttpGet("odata/People({key})/VinylRecords({vinylRecordKey})")]
        [EnableQuery]
        public IActionResult GetVinylRecordForPerson(int key, int vinylRecordKey)
        {
            var person = dbContext.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            var vinylRecord = dbContext.VinylRecords.Where(v => v.Person.PersonId == key
                && v.VinylRecordId == vinylRecordKey);

            if (!vinylRecord.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(vinylRecord));
        }

        [HttpPost("odata/People")]
        public async Task<IActionResult> CreatePerson1([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // add the person to the People collection
            dbContext.People.Add(person);
            await dbContext.SaveChangesAsync();

            // return the created person 
            return Created(person);
        }

        [HttpPut("odata/People({key})")]
        public async Task<IActionResult> UpdatePerson1(int key, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = await dbContext.People
              .FirstOrDefaultAsync(p => p.PersonId == key);

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
                //    dbContext.People.Add(person);
                //    await dbContext.SaveChangesAsync();
                //    return Created(person);
                //}

            }

            person.PersonId = currentPerson.PersonId;
            dbContext.Entry(currentPerson).CurrentValues.SetValues(person);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpPatch("odata/People({key})")]
        public async Task<IActionResult> PartiallyUpdatePerson1(int key, [FromBody] Delta<Person> patch)
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

        [HttpDelete("odata/People({key})")]
        public async Task<IActionResult> DeleteOnePerson1(int key)
        {
            var currentPerson = await dbContext.People.FirstOrDefaultAsync(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }
            dbContext.People.Remove(currentPerson);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        // post http://localhost:5000/odata/People(1)/VinylRecords
        /*
         body 
            {
                  "Title": "Bleach",
                  "Artist": "Nirvana",
                  "CatalogNumber": "ARC/101",
                  "Year": 1989,
                  "PressingDetailId": 1
            } 
         */

        [HttpPost("odata/People({key})/VinylRecords")]
        public async Task<IActionResult> CreateVinylRecordForPerson(int key, [FromBody] VinylRecord vinylRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // does the person exist?
            var person = await dbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }
            // link the person to the VinylRecord (also avoids an invalid person 
            // key on the passed-in record - key from the URI wins)
            vinylRecord.Person = person;
            // add the VinylRecord
            dbContext.VinylRecords.Add(vinylRecord);
            await dbContext.SaveChangesAsync();
            // return the created VinylRecord 
            return Created(vinylRecord);
        }

        //patch http://localhost:5000/odata/People(1)/VinylRecords(1)
        /*
         * 
               {
                "Artist": "Nirvana updated" 
                } 
         */

        [HttpPatch("odata/People({key})/VinylRecords({vinylRecordKey})")]
        public async Task<IActionResult> PartiallyUpdateVinylRecordForPerson(int key, int vinylRecordKey, [FromBody] Delta<VinylRecord> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // does the person exist?
            var person = await dbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            // find a matching vinyl record
            // var currentVinylRecord = await dbContext.VinylRecords.FirstOrDefaultAsync(p => p.VinylRecordId == vinylRecordKey&& p.Person.PersonId == key);

            var currentVinylRecord = await dbContext.VinylRecords
                .Include("DynamicVinylRecordProperties")
                .FirstOrDefaultAsync(p => p.VinylRecordId == vinylRecordKey && p.Person.PersonId == key);

            // return NotFound if the VinylRecord isn't found
            if (currentVinylRecord == null)
            {
                return NotFound();
            }

            // apply patch
            patch.Patch(currentVinylRecord);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // delete http://localhost:5000/odata/People(1)/VinylRecords(6)   //DELETE VinylRecord for Person
        [HttpDelete("odata/People({key})/VinylRecords({vinylRecordKey})")]
        public async Task<IActionResult> DeleteVinylRecordForPerson(int key, int vinylRecordKey)
        {
            var currentPerson = await dbContext.People
                .FirstOrDefaultAsync(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            // find a matching vinyl record  
            var currentVinylRecord = await dbContext.VinylRecords
                .FirstOrDefaultAsync(p => p.VinylRecordId == vinylRecordKey
                && p.Person.PersonId == key);

            if (currentVinylRecord == null)
            {
                return NotFound();
            }

            dbContext.VinylRecords.Remove(currentVinylRecord);
            await dbContext.SaveChangesAsync();

            // return No Content
            return NoContent();
        }
    }
}