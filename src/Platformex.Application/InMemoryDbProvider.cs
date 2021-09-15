using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Platformex.Application
{
    public class InMemoryDbProvider<TModel> : IDbProvider<TModel> 
        where TModel : IModel, new()
    {
        private static readonly Dictionary<Guid, TModel> Items = new();
        private static readonly object _loc = new();
        private Dictionary<Guid, TModel> _transactionalItems;
        
        public InMemoryDbProvider(Dictionary<Guid, TModel> items = null)
        {
            if (items == null) return;
            foreach (var (key, value) in items)
                Items.Add(key, value);
        }

        private Task<TModel> FindAsync(Guid id)
        {
            lock (_loc)
            {
                return Task.FromResult(Items.ContainsKey(id) ? Items[id] : default);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private TModel Create(Guid id)
        {
            var model = new TModel();
            return model;
        }

        public async Task<(TModel model, bool isCreated)> LoadOrCreate(Guid id)
        {
            var model = await FindAsync(id);
            var isCreated = true;
            if (model == null)
            {
                isCreated = false;
                model = Create(id);
            }

            return (model, isCreated);
        }

        public Task SaveChangesAsync(Guid id, TModel model)
        {
            lock (_loc)
            {

                if (_transactionalItems.ContainsKey(id))
                    _transactionalItems[id] = model;
                else
                {
                    _transactionalItems.Add(id, model);
                }
            }

            return Task.CompletedTask;
        }

        public Task BeginTransaction()
        {
            lock (_loc)
            {
                _transactionalItems = Items.ToDictionary(entry => entry.Key, 
                    entry => CreateDeepCopy(entry.Value));
            }
            return Task.CompletedTask;
        }

        private static T CreateDeepCopy<T>(T obj)
        {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T) formatter.Deserialize(ms);
        }

        public Task CommitTransaction()
        {
            lock (_loc)
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
            }

            return Task.CompletedTask;
        }

        public Task RollbackTransaction()
        {
            lock (_loc)
            {
                _transactionalItems = null;
            }

            return Task.CompletedTask;
        }
    }
}