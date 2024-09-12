using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace proyectoDivisas.Models
{
    public class Alerta
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DivisaBase { get; set; }
        public string DivisaContraparte { get; set; }
        public float Minimo { get; set; }
        public float Maximo { get; set; }
        public float ValorActual { get; set; }
        public bool LimiteMinimoAlcanzado { get; set; }
        public bool LimiteMaximoAlcanzado { get; set; }

    }
}
