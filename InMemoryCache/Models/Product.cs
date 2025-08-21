using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InMemoryCache.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; }

        [BsonElement("Id")]
        public int Id { get; set; }

        [BsonElement("Name")]
        public required string Name { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("CategoryId")]
        public int CategoryId { get; set; }
    }
}
