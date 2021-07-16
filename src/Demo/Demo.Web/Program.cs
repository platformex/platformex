using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using Demo.Application;
using Demo.Application.Queries;
using Demo.Cars;
using Demo.Cars.Domain;
using Demo.Documents;
using Demo.Documents.Domain;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex;
using Platformex.Infrastructure;
using Platformex.Web;
using Platformex.Web.GraphQL;
using Platformex.Web.Swagger;

namespace Demo.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseOrleans(builder =>
                {
                    //Конфигурация кластера
                    builder.UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "HelloWorldApp";
                        })
                        .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)

                        //Конфигурация шины событий
                        .AddSimpleMessageStreamProvider("EventBusProvider")
                        .AddMemoryGrainStorage("PubSubStore")

                        //Конфигурация журналирования
                        .ConfigureLogging(logging =>
                        {
                            logging.AddConsole();
                            logging.SetMinimumLevel(LogLevel.Information);
                        })

                        //Конфигурация приложения
                        .ConfigurePlatformex(p =>
                        {
                            p.RegisterAggregate<CarId, CarAggregate, CarState>().WithCommands();
                            p.RegisterAggregate<DocumentId, DocumentAggregate, DocumentState>().WithCommands();

                            p.RegisterApplicationParts<DocumentInfoReadModel>();

                            //Конфигурация WebAPI
                            p.ConfigureWebApi()
                                .WithSwagger(options =>
                            {
                                options.Name = "Test";
                                options.Url = "swagger";

                            });
                            p.ConfigureGraphQl(options => options.BasePath = "graphql")
                                .WithConsole(options => options.BasePath = "graphql-console");

                        }).AddStartupTask(async (provider, _) =>
                        {
                            var platform = provider.GetService<IPlatform>();

                            var carId = CarId.New;
                            var car = await platform.CreateCar(carId, "test");

                            await platform.CreateCar(CarId.New, "test");

                            await car.RenameCar("new-name");

                            var docId = DocumentId.New;
                            var doc = await platform.CreateDocument(docId, "doc");

                            await doc.RenameDocument("doc-new-name");

                            // ReSharper disable once PossibleNullReferenceException
                            var result = await platform.QueryAsync(new TotalObjectsQuery());
                            Console.WriteLine($">> Total count: {result.Count}");

                            var res = await platform.QueryAsync(new ObjectsNamesQuery());
                            Console.WriteLine($">> Names: {string.Join(",", res.Names)}");

                            var items = await platform.QueryAsync(new DocumentInfoQuery { Take = 10});
                            foreach (var c in items)
                            {
                                Console.WriteLine($">> DOCUMENT INFO: ID:{c.Id} Name:{c.Name} Changes:{c.ChangesCount}");
                            }
                        });
                });
        
    }
}
