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
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex.Infrastructure;
using Platformex.Web;
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

                        });
                });
        
    }
}
