using System;
using System.Threading.Tasks;
using Demo.Application;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Platformex.Application;

namespace Demo.Infrastructure
{
    public class DocumentModel : IDocumentModel
    {
        [BsonId]
        public ObjectId ObjectId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        
        public DateTime CreateDate { get; set; }
    }
    public class DocumentDbProvider : IDbProvider<IDocumentModel>
    {
        private readonly IMongoClient _client;

        public DocumentDbProvider(IMongoClient client)
        {
            _client = client;
        }
        public async Task<IDocumentModel> FindAsync(string id)
        {
            return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        }

        private IMongoCollection<DocumentModel> Collection 
            => _client.GetDatabase("demo").GetCollection<DocumentModel>("document");

        public IDocumentModel Create(string id)
        {
            return new DocumentModel{Id = id};
        }

        public async Task SaveChangesAsync(IDocumentModel model)
        {
            var m = (DocumentModel) model;
            if (m.ObjectId == default)
                await Collection.InsertOneAsync((DocumentModel) model);
            else
                await Collection.ReplaceOneAsync(e => e.ObjectId == m.ObjectId, m);
        }
    }
}