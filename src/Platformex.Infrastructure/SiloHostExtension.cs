using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;
using Platformex.Domain;

namespace Platformex.Infrastructure
{
    public static class SiloHostBuilderExtension
    {
        public static ISiloHostBuilder ConfigurePlatformex(this ISiloHostBuilder builder, Action<PlatformBuilder> configureAction)
        {
            var platform = new Platform();

            var platformBuilder = new PlatformBuilder(platform); 
            configureAction(platformBuilder);

            foreach (var asm in platform.Definitions.Assemblies)
            {
                builder.ConfigureApplicationParts(manager =>
                {
                    manager.AddApplicationPart(new AssemblyPart(asm));
                });
            }
            foreach (var data in platform.Definitions.Aggregates.Values)
            {
                var stateImpl = data.StateType;
                var stateInterface = stateImpl.GetInterfaces()
                    .FirstOrDefault(i => !i.IsGenericType && i.GetInterfaces()
                        .Any(j => j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregateState<>)));
                if (stateInterface != null)
                    builder.ConfigureServices(s => s.AddTransient(stateInterface,stateImpl));
            }

            builder.ConfigureServices(collection =>
            {
                foreach (var action in platformBuilder.ConfigureServicesActions)
                {
                    action(collection);
                }
                collection.AddSingleton<IPlatform>(provider =>
                {
                    platform.SetServiceProvider(provider);
                    return platform;
                });
            });

            builder
                .AddStartupTask((provider, _) => Initializer.InitAsync(provider))
                .ConfigureApplicationParts(manager =>
                    manager.AddApplicationPart(typeof(Initializer).Assembly).WithReferences());

            //�������� ������-������ �� ������� �������
            builder.AddOutgoingGrainCallFilter(async context =>
            {
                if (context.Arguments != null ? context.Arguments.Length == 1 : false)
                {
                    var argument = context.Arguments[0];
                    var rulesAttribute = argument.GetType().GetCustomAttribute<RulesAttribute>();
                    if (rulesAttribute != null)
                    {
                        var rules = (IRules)Activator.CreateInstance(rulesAttribute.RulesType);
                        var result = rules.Validate(argument);
                        if (!result.IsValid)
                        {
                            context.Result = new Result(result);
                            return;
                        }
                    }
                }

                await context.Invoke();
            });

            return builder;
        }

        public static ISiloBuilder ConfigurePlatformex(this ISiloBuilder builder, Action<PlatformBuilder> configureAction)
        {
            var platform = new Platform();
            var platformBuilder = new PlatformBuilder(platform); 
            configureAction(platformBuilder);

            foreach (var asm in platform.Definitions.Assemblies)
            {
                builder.ConfigureApplicationParts(manager =>
                {
                    manager.AddApplicationPart(new AssemblyPart(asm));
                });
            }
            foreach (var data in platform.Definitions.Aggregates.Values)
            {
                var stateImpl = data.StateType;
                var stateInterface = stateImpl.GetInterfaces()
                    .FirstOrDefault(i => !i.IsGenericType && i.GetInterfaces()
                        .Any(j => j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregateState<>)));
                if (stateInterface != null)
                    builder.ConfigureServices(s => s.AddTransient(stateInterface,stateImpl));
            }

            builder.ConfigureServices(collection =>
            {
                foreach (var action in platformBuilder.ConfigureServicesActions)
                {
                    action(collection);
                }
                collection.AddSingleton<IPlatform>(provider =>
                {
                    platform.SetServiceProvider(provider);
                    return platform;
                });
            });

            builder.AddStartupTask(async (provider, _) =>
            {
                foreach (var action in platformBuilder.ConfigureStartupActions)
                {
                    await action(provider);
                }
            });

            builder
                .AddStartupTask((provider, _) => Initializer.InitAsync(provider))
                .ConfigureApplicationParts(manager =>
                    manager.AddApplicationPart(typeof(Initializer).Assembly).WithReferences());

            //�������� ������-������ �� ������� �������
            builder.AddOutgoingGrainCallFilter(async context =>
            {
                if (context.Arguments != null ? context.Arguments.Length == 1 : false)
                {
                    var argument = context.Arguments[0];
                    var rulesAttribute = argument.GetType().GetCustomAttribute<RulesAttribute>();
                    if (rulesAttribute != null)
                    {
                        var rules = (IRules)Activator.CreateInstance(rulesAttribute.RulesType);
                        var result = rules.Validate(argument);
                        if (!result.IsValid)
                        {
                            context.Result = new Result(result);
                            return;
                        }
                    }
                }

                await context.Invoke();
            });

            return builder;
        }
    }
}