using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Platformex
{
    public interface IService : IGrainWithGuidKey
    {
        Task SetMetadata(ServiceMetadata metadata);
        
        //Используется для вызова сервиса по API
        Task<object> Invoke(string methodName, Dictionary<string, object> parameters);
    }
}