using MongoDB.Driver;

namespace proyectoDivisas.Repositories
{
    public class MongoDBRepository
    {
        public MongoClient client; //coneccion entre la app y la db
        public IMongoDatabase db;
        public MongoDBRepository()
        {
            client = new MongoClient("mongodb://localhost:27017"); //conection string
            db = client.GetDatabase("Divisas");
        }
    }
}
