using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Platformex.Domain
{
    public abstract class ServiceBase : Grain, IIncomingGrainCallFilter
    {
        protected SecurityContext SecurityContext { get; private set; }

        protected Task<Result> ExecuteAsync<TIdentity>(ICommand<TIdentity> command) 
            where TIdentity : Identity<TIdentity>
        {
            var platform = ServiceProvider.GetService<IPlatform>();
            return platform != null ? platform.ExecuteAsync(command.Id.Value, command) : null;
        }
        
        private ILogger _logger;
        protected ServiceMetadata Metadata { get; private set; } = new ServiceMetadata();
        protected ILogger Logger => GetLogger();
        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>() != null ? ServiceProvider.GetService<ILoggerFactory>().CreateLogger(GetType()) : null;

        public TDomainService Service<TDomainService>() where TDomainService : IService
        // ReSharper disable once PossibleNullReferenceException
            => ServiceProvider.GetService<IPlatform>().Service<TDomainService>();

        protected virtual string GetPrettyName() => $"{GetJobName()}:{IdentityString}";
        protected virtual string GetJobName() => GetType().Name;

        public override Task OnActivateAsync()
        {
            Logger.LogInformation($"(Service [{GetPrettyName()}] activated.");

            return base.OnDeactivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"(Service [{GetPrettyName()}] deactivated.");

            return base.OnDeactivateAsync();
        }


        public Task SetMetadata(ServiceMetadata metadata)
        {
            Metadata = metadata;
            return  Task.CompletedTask;
        }

        public async Task<object> Invoke(string methodName, Dictionary<string, object> parameters)
        {
            var method = GetType().GetMethod(methodName);
            if (method == null)
                throw new MissingMethodException($"Не найден метод {methodName} у сервиса {GetType().Name}");

            var param = method.GetParameters();
            var paramsList = new List<object>();
            foreach (var p in param)
            {
                if (!parameters.ContainsKey(p.Name))
                    throw new ArgumentException(
                        $"Не указан параметр {p.Name} для метода {methodName} у сервиса {GetType().Name}");

                object val;
                try
                {
                    val = Convert.ChangeType(parameters[p.Name], p.ParameterType);

                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"Неверный тип параметра {p.Name} для метода {methodName} у сервиса {GetType().Name}", ex);
                }

                paramsList.Add(val);
            }

            var task = (Task)method.Invoke(this, paramsList.ToArray());
            await task.ConfigureAwait(false);

            var props = task.GetType().GetProperty(nameof(Task<object>.Result));
            if (props != null)
            {
                var result = task.GetType().GetProperty(nameof(Task<object>.Result)) != null
                    ? props.GetValue(task)
                    : null;
                return result;
            }

            return null;

        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.InterfaceMethod.Name != "SetMetdadta")
            {
                Logger.LogInformation($"(Service method [{context.InterfaceMethod.Name}] invoking...");
            
                
                try
                {
                    var sc = new SecurityContext(Metadata);
                    //Проверим права
                    var requiredUser = SecurityContext.IsUserRequiredFrom(this);
                    if (requiredUser && !sc.IsAuthorized)
                        throw new UnauthorizedAccessException();
            
                    var requiredRole = SecurityContext.GetRolesFrom(this);
                    if (requiredRole != null)
                        sc.HasRoles(requiredRole);

                    SecurityContext = sc;
                    
                    await context.Invoke();
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.LogInformation($"Unauthorized Access in Service{GetPrettyName()} method {context.InterfaceMethod.Name}" , e);
                    context.Result = Result.Unauthorized($"Необходима аутентификация для доступа к сервису {GetPrettyName()} при выполнении метода {context.InterfaceMethod.Name}. {e.Message}");
                    return;
                }
                catch (ForbiddenException e)
                {
                    _logger.LogInformation($"Request Forbidden in Service{GetPrettyName()} method {context.InterfaceMethod.Name}" , e);
                    context.Result = Result.Forbidden($"Недостаточно прав доступа для доступа к сервису {GetPrettyName()} при выполнении метода {context.InterfaceMethod.Name}. {e.Message}");
                }            
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    context.Result = Result.Fail(e.Message);
                    return;
                }
               
                Logger.LogInformation($"(Service method [{context.InterfaceMethod.Name}] invoked...");
            }
            else
            {
                await context.Invoke();
            }
        }
    }
}