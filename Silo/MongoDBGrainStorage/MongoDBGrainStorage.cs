using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

using Orleans;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Configuration;
using Orleans.Serialization;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using MongoDB.Driver;
using MongoDB.Bson;

namespace TodoListApi.Silo.GrainStorage {

    public class MongoDBGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle> {

        private readonly string _storageName;
        private readonly MongoDBGrainStorageOptions _options;
        private readonly ClusterOptions _clusterOptions;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        private MongoClient _dbClient;

        public MongoDBGrainStorage(string storageName, MongoDBGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions, IGrainFactory grainFactory, ITypeResolver typeResolver) {
            _storageName = storageName;
            _options = options;
            _clusterOptions = clusterOptions.Value;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) {
            var db = _dbClient.GetDatabase(_options.databaseName);
            bool collectionExists = await IsCollectionExistsAsync(db, GetCollectionString(grainType));
            var filter = Builders<BsonDocument>.Filter.Eq("grain_key", GetKeyString(grainReference));
            if(!collectionExists) {
                return;
            }

            var grainData = (await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).FindAsync(filter)).FirstOrDefault();
            if(grainData != null) {
                await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).DeleteOneAsync(filter);
            }
            return;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) {
            var db = _dbClient.GetDatabase(_options.databaseName);
            bool collectionExists = await IsCollectionExistsAsync(db, GetCollectionString(grainType));
            var filter = Builders<BsonDocument>.Filter.Eq("grain_key", GetKeyString(grainReference));
            if(!collectionExists) {
                db.CreateCollection(grainType);
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                return;
            }
            var grainData = (await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).FindAsync(filter)).FirstOrDefault();
            
            if(grainData != null) {
                grainData.Remove("grain_key");
                grainData.Remove("_id");
                grainState.State = JsonConvert.DeserializeObject(grainData.ToJson(), grainState.Type, _jsonSettings);
                
            }
            else {
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) {
            var json = grainState.State.ToJson();
            var storedData = BsonDocument.Parse(json).AddRange(new BsonDocument {{"grain_key", GetKeyString(grainReference)}} );
            var db = _dbClient.GetDatabase(_options.databaseName);
            bool collectionExists = await IsCollectionExistsAsync(db, GetCollectionString(grainType));
            storedData.Remove("_t");
            var filter = Builders<BsonDocument>.Filter.Eq("grain_key", GetKeyString(grainReference));
            if(!collectionExists) {
                db.CreateCollection(grainType);
                await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).InsertOneAsync(storedData);
                return;
            }

            var grainData = (await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).FindAsync(filter)).FirstOrDefault(); //Does it already exist?

            // This is ugly hack for now
            if(grainData != null) {
                await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).DeleteOneAsync(filter);
            }
            await db.GetCollection<BsonDocument>(GetCollectionString(grainType)).InsertOneAsync(storedData);
        }

        public void Participate(ISiloLifecycle lifecycle) {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<MongoDBGrainStorage>(_storageName), ServiceLifecycleStage.ApplicationServices, Init);
        }

        private Task Init(CancellationToken ct) {
            _jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(OrleansJsonSerializer.GetDefaultSerializerSettings(_typeResolver, _grainFactory), false, false, TypeNameHandling.None);

            _dbClient = new MongoClient(_options.atlasConnectionString);

            return Task.CompletedTask;

        }

        private string GetKeyString(GrainReference grainReference) {
            return $"{grainReference.ToShortKeyString()}";
        }

        private string GetCollectionString(string grainType) {
            return $"{_clusterOptions.ServiceId}_{grainType}";
        }

        private Task<bool> IsCollectionExistsAsync(IMongoDatabase database, string collectionName) {

            IMongoCollection<BsonDocument> mongoCollection = database.GetCollection<BsonDocument>(collectionName);
                    
            if (mongoCollection != null)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    public class MongoDBGrainStorageOptions {
        public string atlasConnectionString { get; set; }
        public string databaseName { get; set; }
    }

}