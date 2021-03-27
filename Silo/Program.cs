using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

using TodoListApi.Interfaces;
using TodoListApi.Grains;
using TodoListApi.Silo.GrainStorage;

namespace Silo
{
    class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync() {
            try {

                var host = await StartSilo();

                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;

            } 
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo() {

            var mongoDbConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = "dez-cluster";
                    options.ServiceId = "todo-list";
                })
                .AddMongoDBGrainStorage("mongo_db", options => {
                    options.databaseName = mongoDbConfiguration.GetValue<string>("databaseName");
                    options.atlasConnectionString = mongoDbConfiguration.GetValue<string>("atlasConnectionString");
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;

        }
    }
}
