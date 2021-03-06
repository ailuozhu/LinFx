﻿using LinFx.Utils;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LinFx.Extensions.MongoDB
{
    public class MongoModelBuilder : IMongoModelBuilder
    {
        private readonly Dictionary<Type, object> _entityModelBuilders;

        private static readonly object SyncObj = new object();

        public MongoModelBuilder()
        {
            _entityModelBuilders = new Dictionary<Type, object>();
        }

        public MongoDbContextModel Build()
        {
            var entityModels = _entityModelBuilders
                .Select(x => x.Value)
                .Cast<IMongoEntityModel>()
                .ToImmutableDictionary(x => x.EntityType, x => x);

            foreach (var entityModel in entityModels.Values)
            {
                var map = entityModel.As<IHasBsonClassMap>().GetMap();
                lock (SyncObj)
                {
                    if (!BsonClassMap.IsClassMapRegistered(map.ClassType))
                    {
                        BsonClassMap.RegisterClassMap(map);
                    }
                }
            }

            return new MongoDbContextModel(entityModels);
        }

        public virtual void Entity<TEntity>(Action<IMongoEntityModelBuilder<TEntity>> buildAction = null)
        {
            var model = (IMongoEntityModelBuilder<TEntity>)_entityModelBuilders.GetOrAdd(
                typeof(TEntity),
                () => new MongoEntityModelBuilder<TEntity>()
            );

            buildAction?.Invoke(model);
        }

        public virtual void Entity(Type entityType, Action<IMongoEntityModelBuilder> buildAction = null)
        {
            Check.NotNull(entityType, nameof(entityType));

            var model = (IMongoEntityModelBuilder)_entityModelBuilders.GetOrAdd(
                entityType,
                () => (IMongoEntityModelBuilder)Activator.CreateInstance(
                    typeof(MongoEntityModelBuilder<>).MakeGenericType(entityType)
                )
            );

            buildAction?.Invoke(model);
        }

        public virtual IReadOnlyList<IMongoEntityModel> GetEntities()
        {
            return _entityModelBuilders.Values.Cast<IMongoEntityModel>().ToImmutableList();
        }
    }
}