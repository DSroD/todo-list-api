using System;

using Orleans;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Hosting;

using Microsoft.Extensions.DependencyInjection;



namespace TodoListApi.Silo.GrainStorage{
    public static class MongoDBSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddMongoDBGrainStorage(this ISiloHostBuilder builder, string providerName, Action<MongoDBGrainStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddMongoDBGrainStorage(providerName, options));
        }

        public static IServiceCollection AddMongoDBGrainStorage(this IServiceCollection services, string providerName, Action<MongoDBGrainStorageOptions> options)
        {
            services.AddOptions<MongoDBGrainStorageOptions>(providerName).Configure(options);
            return services
                .AddSingletonNamedService(providerName, MongoDBGrainStorageFactory.Create)
                .AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }
}