using System;
using System.Linq;
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


        private static string GenerateQueryId(object query)
        {
            static string CalculateMd5Hash(string input)
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

        public async Task<object> QueryAsync(IQuery query)
        {
            var id = GenerateQueryId(query);

            var type = query.GetType();

            var queryInterface = type.GetInterfaces().FirstOrDefault(j => j.IsGenericType
                                                                            && j.GetGenericTypeDefinition() ==
                                                                            typeof(IQuery<>));
            if (queryInterface == null) throw new InvalidOperationException();
                    
            var resultType = queryInterface.GetGenericArguments()[0];

            var handlerInterface = typeof(IQueryHandler<>).MakeGenericType(resultType);

            var queryGarin = (IQueryHandler) _grainFactory.GetGrain(handlerInterface, id);
            
            return await queryGarin.QueryAsync(query);
        }

        public async Task<CommandResult> ExecuteAsync(string aggragateId, ICommand command)
        {

            var type = command.GetType();

            var commandType = type.GetInterfaces().FirstOrDefault(j => j.IsGenericType
                                                                          && j.GetGenericTypeDefinition() ==
                                                                          typeof(ICommand<>));
            if (commandType == null) throw new InvalidOperationException();
                    
            var identityType = commandType.GetGenericArguments()[0];

            if (!Definitions.Aggregates.TryGetValue(identityType, out var aggregateDefinition))
                throw new InvalidOperationException();

            var grain = (IAggregate) _grainFactory.GetGrain(aggregateDefinition.InterfaceType, aggragateId);

            return await grain.DoAsync(command);
        }
    }
}