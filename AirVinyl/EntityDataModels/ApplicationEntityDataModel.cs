using AirVinyl.Entities;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
namespace AirVinyl.EntityDataModels
{
    public class ApplicationEntityDataModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "AlyNamespace";
            builder.ContainerName = "AlyContainerName";

            builder.EntitySet<Person>("People");
           // builder.EntitySet<VinylRecord>("VinylRecords");
            builder.EntitySet<RecordStore>("RecordStores");
            builder.EntityType<Person>().Ignore(r => r.Photo);
            builder.EntityType<Person>().Property(r => r.Base64String);
            return builder.GetEdmModel();
        }
    }
}
