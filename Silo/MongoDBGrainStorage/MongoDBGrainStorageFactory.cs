using System;

using Orleans.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;


namespace TodoListApi.Silo.GrainStorage {

    public static class MongoDBGrainStorageFactory{
        internal static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<MongoDBGrainStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<MongoDBGrainStorageOptions>>();
            return ActivatorUtilities.CreateInstance<MongoDBGrainStorage>(services, name, optionsSnapshot.Get(name), services.GetProviderClusterOptions(name));
        }
    }
}