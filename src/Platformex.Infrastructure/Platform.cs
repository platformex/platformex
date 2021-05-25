using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans;
using Platformex.Application;

namespace Platformex.Infrastructure
{
    public class Platform : IPlatform
    {
        public Definitions Definitions { get; } = new Definitions();

        private IGrainFactory _grainFactory;


        public TAggregate GetAggregate<TAggregate>(string id) where TAggregate : IAggregate => _grainFactory.GetGrain<TAggregate>(id);



        internal void SetServiceProvider(IServiceProvider provider)
        {
            _grainFactory = provider.GetService<IGrainFactory>();
        }


        private string GenerateQueryId(object query)
        {
            string CalculateMd5Hash(string input)
            {
                var md5 = MD5.Create();
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
            
            var json = JsonConvert.SerializeObject(query);
            return CalculateMd5Hash(json);
        }
        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query) 
        {
            var id = GenerateQueryId(query);
            var queryGarin = _grainFactory.GetGrain<IQueryHandler<TResult>>(id);
            return queryGarin.QueryAsync(query);
        }

        public Task<CommandResult> ExecuteAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}