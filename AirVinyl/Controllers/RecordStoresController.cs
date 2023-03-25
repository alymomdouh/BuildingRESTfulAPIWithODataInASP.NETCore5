using AirVinyl.API.DbContexts;
using AirVinyl.API.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirVinyl.Controllers
{
    [Route("odata")]
    public class RecordStoresController : ODataController
    {
        private readonly AirVinylDbContext _airVinylDbContext;

        public RecordStoresController(AirVinylDbContext airVinylDbContext)
        {
            _airVinylDbContext = airVinylDbContext
                ?? throw new ArgumentNullException(nameof(airVinylDbContext));
        }


        [EnableQuery]
        [HttpGet("RecordStores")]
        public IActionResult GetAllRecordStores()
        {
            return Ok(_airVinylDbContext.RecordStores);
        }


        [EnableQuery]
        [HttpGet("RecordStores({key})")]
        public IActionResult GetOneRecordStore(int key)
        {
            var recordStores = _airVinylDbContext.RecordStores.Where(p => p.RecordStoreId == key);

            if (!recordStores.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(recordStores));
        }

        [HttpGet("RecordStores({key})/Tags")]
        [EnableQuery]
        public IActionResult GetRecordStoreTagsProperty(int key)
        {
            // no Include necessary for EF Core - "Tags" isn't a navigation property 
            // in the entity model.  
            var recordStore = _airVinylDbContext.RecordStores
                .FirstOrDefault(p => p.RecordStoreId == key);

            if (recordStore == null)
            {
                return NotFound();
            }

            var collectionPropertyToGet = new Uri(HttpContext.Request.GetEncodedUrl())
                .Segments.Last();
            var collectionPropertyValue = recordStore.GetValue(collectionPropertyToGet);

            // return the collection of tags
            return Ok(collectionPropertyValue);
        }

        // get http://localhost:5000/odata/RecordStores(1)/AirVinyl.Functions.IsHighRated(minimumRating=1)
        [HttpGet("RecordStores({id})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
        public async Task<bool> IsHighRated(int id, int minimumRating)
        {
            // get the RecordStore
            var recordStore = await _airVinylDbContext.RecordStores
                .FirstOrDefaultAsync(p => p.RecordStoreId == id
                    && p.Ratings.Any()
                    && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating);

            return (recordStore != null);
        }

        // this make function that take list of person Ids and return recordStores
        // get http://localhost:5000/odata/RecordStores/AirVinyl.Functions.AreRatedBy(personIds=[1,2])
        // get http://localhost:5000/odata/RecordStores/AirVinyl.Functions.AreRatedBy(personIds=[7])
        // get http://localhost:5000/odata/RecordStores/AirVinyl.Functions.AreRatedBy(personIds=[4,5])
        [HttpGet("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={people})")]
        public async Task<IActionResult> AreRatedBy([FromODataUri] IEnumerable<int> people)
        {
            var recordStores = await _airVinylDbContext.RecordStores
                .Where(p => p.Ratings.Any(r => people.Contains(r.RatedBy.PersonId)))
                .ToListAsync();
            return Ok(recordStores);
        }

        // GetHighRatedRecordStored - unbound function
        // get http://localhost:5000/odata/GetHighRatedRecordStores(minimumRating=3)
        [HttpGet("GetHighRatedRecordStores(minimumRating={minimumRating})")]
        public async Task<IActionResult> GetHighRatedRecordStores(int minimumRating)
        {
            var recordStores = await _airVinylDbContext.RecordStores
                .Where(p => p.Ratings.Any()
                    && (p.Ratings.Sum(r => r.Value) / p.Ratings.Count) >= minimumRating)
                .ToListAsync();
            return Ok(recordStores);
        }
    }
}
