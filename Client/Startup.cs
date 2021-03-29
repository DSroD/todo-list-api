using System;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;

using Orleans;
using Orleans.Configuration;

using TodoListApi.Interfaces;
using TodoListApi.Client.Hubs;

namespace TodoListApi.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var orleansClient = ConnectClient().Result;
            var grain = orleansClient.GetGrain<INoteGrain>(0);

            var corsSettings = new ConfigurationBuilder()
                .AddJsonFile("corsSettings.json", false)
                .Build();

            services.AddCors(options => {
                //options.AddPolicy("AnyOrigin", builder =>
                //{
                //    builder
                //        .WithOrigins(corsSettings.GetSection("webDomain").Get<string[]>())
                //        .AllowAnyMethod()
                //        .AllowAnyHeader()
                //        .DisallowCredentials();
                //});

                options.AddPolicy("DomainCred", builder =>
                {
                    builder
                        .WithOrigins(corsSettings.GetSection("webDomain").Get<string[]>())
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST", "DELETE")
                        .AllowCredentials();
                });
            });
            
            services.AddControllers();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDo-List API", Version = "v1" });
            });

            services.AddSignalR();

            services.AddSingleton<IGrainFactory>(orleansClient);
            services.AddSingleton<IClusterClient>(orleansClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo-List API v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("DomainCred");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<UpdateHub>("/update");
            });
        }

        #region Orleans Client
        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dez-cluster";
                    options.ServiceId = "todo-list";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
        #endregion
    }
}