using MongoDB.Bson;
using MongoDB.Driver;
using proyectoDivisas.Models;

namespace proyectoDivisas.Repositories
{
    public class AlertaDivisaCollection : IAlertaDivisasCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Alerta> Collection;

        public AlertaDivisaCollection()
        {
            Collection = _repository.db.GetCollection<Alerta>("Alertas");
        }
        public async Task CreateAlerta(Alerta alerta)
        {
            await Collection.InsertOneAsync(alerta);
        }

        public async Task DeleteAlerta(string id)
        {
            var filter = Builders<Alerta>.Filter.Eq(doc => doc.Id, id);
            await Collection.DeleteOneAsync(filter);
        }

        public async Task<Alerta> ReadAlertaPorId(string id)
        {
            var filter = Builders<Alerta>.Filter.Eq(doc => doc.Id, id);
            var cursor = await Collection.FindAsync(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task<List<Alerta>> ReadAllAlertas()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task UpdateAlerta(Alerta alerta)
        {
            var filter = Builders<Alerta>.Filter.Eq(s => s.Id, alerta.Id);
            await Collection.ReplaceOneAsync(filter,alerta);

        }
    }
}
