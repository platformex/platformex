
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Platformex.Infrastructure")]
[assembly: InternalsVisibleTo("Platformex.Tests")]

namespace Platformex.Domain
{
    public abstract class Aggregate<TIdentity, TState, TAggregate> : Grain, IAggregate<TIdentity>, IIncomingGrainCallFilter
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        private static readonly IReadOnlyDictionary<Type, Func<TAggregate, ICommand, Task<Result>>> DoCommands;
        private ICommand _pinnedCommand;

        protected SecurityContext SecurityContext { get; private set; }
        static Aggregate()
        {
            DoCommands = typeof(TAggregate).GetAggregateDoMethods<TIdentity, TAggregate>();
        }
        public async Task<Result> DoAsync(ICommand command)
        {
            if (!DoCommands.TryGetValue(command.GetType(), out var applier))
            {
                throw new MissingMethodException($"missing HandleAsync({command.GetType().Name})");
            }

            try
            {
                await BeforeApplyingCommand(command);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogInformation($"Unauthorized Access in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                return Result.Unauthorized($"Необходима аутентификация для доступа к агрегату {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
            }
            catch (ForbiddenException e)
            {
                _logger.LogInformation($"Request Forbidden in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                return Result.Forbidden($"Недостаточно прав доступа для агрегата {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
            }
            Result result;

            try
            {
                result = await applier((TAggregate)(object)this, command);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogInformation($"Unauthorized Access in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                return Result.Unauthorized($"Необходима аутентификация для доступа к агрегату {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
            }
            catch (ForbiddenException e)
            {
                _logger.LogInformation($"Request Forbidden in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                return Result.Forbidden($"Недостаточно прав доступа для агрегата {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                await RollbackApplyingCommand();
                result = Result.Fail(e.Message);
                return result;
            }

            await AfterApplyingCommand();

            return result;

        }

        private async Task RollbackApplyingCommand()
        {
            _uncommitedEvents.Clear();
            _pinnedCommand = null;
            SecurityContext = null;
            await State.RollbackTransaction();
        }

        private async Task AfterApplyingCommand()
        {

            //Завершаем транзакцию
            await State.CommitTransaction();

            /*
             * TODO:В это месте необходимо обеспечить синхронизацию транзакций и отправки событий в шину.
             * Если после сохранения состояния и до отправки сооющения произошел сбой, то при восстановления
             * нужно повтроно отпавить события в шину 
             */

            //Отправяем все события в шину
            foreach (var ent in _uncommitedEvents)
            {
                await _platform.PublishEvent(ent);
            }

            _uncommitedEvents.Clear();
            _pinnedCommand = null;
            SecurityContext = null;
        }

        private async Task BeforeApplyingCommand(ICommand command)
        {
            var sc = new SecurityContext(command.Metadata);
            //Проверим права
            var requiredUser = SecurityContext.IsUserRequiredFrom(command);
            if (requiredUser && !sc.IsAuthorized)
                throw new UnauthorizedAccessException();

            var requiredRole = SecurityContext.GetRolesFrom(command);
            if (requiredRole != null)
                sc.HasRoles(requiredRole);

            SecurityContext = sc;
            _pinnedCommand = command;

            //Наичнаем транзакцию
            await State.BeginTransaction();
        }


        protected TIdentity AggregateId => State != null ? State.Identity != null ? State.Identity : this.GetId<TIdentity>() : this.GetId<TIdentity>();
        protected TState State { get; private set; }
        internal void TestOnlySetState(TState newState) => State = newState;
        internal TState TestOnlyGetState() => State;

        private IPlatform _platform;

        private ILogger _logger;
        protected ILogger Logger => GetLogger();

        private ILogger GetLogger()
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>() != null ? ServiceProvider.GetService<ILoggerFactory>().CreateLogger(GetType()) : null;

        protected virtual string GetAggregateName() => GetType().Name.Replace("Aggregate", "");
        protected string GetPrettyName() => $"{GetAggregateName()}:{this.GetPrimaryKeyString()}";

        public override async Task OnActivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activating...");
            try
            {
                _platform = (IPlatform)ServiceProvider.GetService(typeof(IPlatform));

                var stateType = _platform.Definitions.Aggregate<TIdentity>() != null ? _platform.Definitions.Aggregate<TIdentity>().StateType : null;

                if (stateType == null)
                    throw new Exception($"Definitions on aggregate {typeof(TIdentity).Name} not found");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loading...");

                State = ServiceProvider.GetService<TState>() != null ? ServiceProvider.GetService<TState>() : Activator.CreateInstance<TState>();
                if (State != null) await State.LoadState(this.GetId<TIdentity>());

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loaded.");

                await base.OnActivateAsync();
            }
            catch (Exception e)
            {
                Logger.LogError($"Aggregate [{GetPrettyName()}] activation error: {e.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
            return base.OnDeactivateAsync();
        }

        private readonly List<IDomainEvent> _uncommitedEvents = new();

        protected async Task Emit<TEvent>(TEvent e) where TEvent : class, IAggregateEvent<TIdentity>
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] preparing to emit event {e.GetPrettyName()}...");
            var metadata = CreateEventMetadata(e);
            var domainEvent = new DomainEvent<TIdentity, TEvent>(e.Id, e, DateTimeOffset.Now, 1,
                metadata);
            try
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changes state ...");
                await State.Apply(e);
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changed state ...");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] fires event {e.GetPrettyName()}...");

                _uncommitedEvents.Add(domainEvent);

            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] error fires event: {domainEvent.GetPrettyName()} : {ex.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] fired event {e.GetPrettyName()}...");
        }

        private IEventMetadata CreateEventMetadata(IAggregateEvent @event)
        {
            var now = DateTimeOffset.UtcNow;
            var eventId = EventId.NewDeterministic(GuidFactories.Deterministic.Namespaces.Events, $"{AggregateId.Value}-v{now.ToUnixTime()}");
            var eventMetadata = new EventMetadata()
            {
                Timestamp = now,
                AggregateSequenceNumber = 0,
                AggregateName = GetAggregateName(),
                AggregateId = AggregateId.Value,
                EventId = eventId,
                EventName = @event.GetPrettyName(),
                EventVersion = 1
            };

            eventMetadata.Merge(_pinnedCommand.Metadata);

            eventMetadata.AddOrUpdateValue(MetadataKeys.TimestampEpoch, now.ToUnixTime().ToString());
            return eventMetadata;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (context.InterfaceMethod.Name == "Do" && context.Arguments.Length == 1 &&
                context.Arguments[0].GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(ICommand<>)))
            {
                var command = (ICommand)context.Arguments[0];
                try
                {
                    await BeforeApplyingCommand(command);
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.LogInformation(
                        $"Unauthorized Access in Aggregate{GetPrettyName()} handle command {command.GetType().Name}",
                        e);
                    context.Result = Result.Unauthorized(
                        $"Необходима аутентификация для доступа к агрегату {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
                    return;
                }
                catch (ForbiddenException e)
                {
                    _logger.LogInformation(
                        $"Request Forbidden in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                    context.Result = Result.Forbidden(
                        $"Недостаточно прав доступа для агрегата {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
                    return;
                }

                try
                {
                    await context.Invoke();
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.LogInformation(
                        $"Unauthorized Access in Aggregate{GetPrettyName()} handle command {command.GetType().Name}",
                        e);
                    context.Result = Result.Unauthorized(
                        $"Необходима аутентификация для доступа к агрегату {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
                    return;
                }
                catch (ForbiddenException e)
                {
                    _logger.LogInformation(
                        $"Request Forbidden in Aggregate{GetPrettyName()} handle command {command.GetType().Name}", e);
                    context.Result = Result.Forbidden(
                        $"Недостаточно прав доступа для агрегата {GetPrettyName()} при выполнении команды {command.GetType().Name}. {e.Message}");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    await RollbackApplyingCommand();
                    context.Result = Result.Fail(e.Message);
                    return;
                }

                await AfterApplyingCommand();

            }
            else
            {
                await context.Invoke();
            }
        }
    }
}