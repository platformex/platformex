using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Platformex.Application
{
    public class InMemoryDbProvider<TModel> : IDbProvider<TModel> where TModel : new()
    {
        private static readonly Dictionary<string, TModel> Items = new();
        private Dictionary<string, TModel> _transactionalItems;

        public InMemoryDbProvider(Dictionary<string, TModel> items = null)
        {
            if (items == null) return;
            foreach (var (key, value) in items)
                Items.Add(key, value);
        }
        public Task<TModel> FindAsync(string id) 
            => Task.FromResult(Items.ContainsKey(id) ? Items[id] : default);

        public TModel Create(string id)
        {
            var model = new TModel();
            return model;
        }

        public Task SaveChangesAsync(string id, TModel model)
        {
            if (_transactionalItems.ContainsKey(id))
                _transactionalItems[id] = model;
            else
            {
                _transactionalItems.Add(id, model);
            }
            return Task.CompletedTask;
        }

        public Task BeginTransaction()
        {
            _transactionalItems = Items.ToDictionary(entry => entry.Key, 
                entry => CreateDeepCopy(entry.Value));
            return Task.CompletedTask;
        }

        private static T CreateDeepCopy<T>(T obj)
        {
            var serializer = new XmlSerializer(obj.GetType());
            using var ms = new MemoryStream();
            serializer.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)serializer.Deserialize(ms);
        }

        public Task CommitTransaction()
        {
            foreach (var (key, value) in _transactionalItems)
            {
                if (Items.ContainsKey(key))
                    Items[key] = value;
                else
                {
                    Items.Add(key, value);
                }
            }
            _transactionalItems = null;
            return Task.CompletedTask;
        }

        public Task RollbackTransaction()
        {
            _transactionalItems = null;
            return Task.CompletedTask;
        }
    }
}