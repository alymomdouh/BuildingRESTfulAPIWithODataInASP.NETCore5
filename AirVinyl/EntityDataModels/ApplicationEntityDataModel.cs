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

            /// add functions
            var isHighRatedFunction = builder.EntityType<RecordStore>().Function("IsHighRated");
            isHighRatedFunction.Returns<bool>();
            isHighRatedFunction.Parameter<int>("minimumRating");
            isHighRatedFunction.Namespace = "AirVinyl.Functions";

            var areRatedByFunction = builder.EntityType<RecordStore>().Collection
                .Function("AreRatedBy");
            areRatedByFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            areRatedByFunction.CollectionParameter<int>("personIds");
            areRatedByFunction.Namespace = "AirVinyl.Functions";

            var getHighRatedRecordStoresFunction = builder.Function("GetHighRatedRecordStores");
            getHighRatedRecordStoresFunction.Parameter<int>("minimumRating");
            getHighRatedRecordStoresFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            getHighRatedRecordStoresFunction.Namespace = "AirVinyl.Functions";

            // add Actions
            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";


            return builder.GetEdmModel();
        }
    }
}
