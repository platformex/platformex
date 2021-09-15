using Microsoft.Extensions.DependencyInjection;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Platformex.Infrastructure
{
    public interface IInitializer : IGrainWithStringKey
    {
        Task InitAsync();
    }
    public class Initializer : Grain, IInitializer
    {
        public async Task InitAsync()
        {
            DeactivateOnIdle();

            var streamProvider = GetStreamProvider("EventBusProvider");
            var eventStream = streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions");
            await eventStream.OnNextAsync("start");
        }
        public static async Task InitAsync(IServiceProvider provider)
        {
            await provider
                .GetRequiredService<IGrainFactory>()
                .GetGrain<IInitializer>("IInitializer")
                .InitAsync();
        }


    }
}
