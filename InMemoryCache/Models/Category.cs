using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InMemoryCache.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; }

        [BsonElement("Id")]
        public int Id { get; set; }

        [BsonElement("Name")]
        public required string Name { get; set; }
    }
}

