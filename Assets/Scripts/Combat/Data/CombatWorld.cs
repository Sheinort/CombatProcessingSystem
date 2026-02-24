using System;
using Unity.Collections;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Holds all native arrays and registries.
    /// Systems iterate over these containers using indexes mapped by entity map.
    /// </summary>
    public sealed class CombatWorld : IDisposable
    {
        public EntityMap EntityMap;
        public InterceptorRegistry InterceptorRegistry;
        public OverTimeEffectsRegistry OverTimeEffectsRegistry;
        public ActionRegister ActionRegister;
        public StatRegistry StatRegistry;
        public DeathRegistry DeathRegistry;
        public EntityTypeRegistry EntityTypeRegistry;
        public NativeList<PendingEntityInit> PendingInits;

        public CombatWorld(
            int entityCapacity       = 64,
            int initialBufferSize    = 5,
            EntityDefinitionSO[] entityDefinitions = null)
        {
            EntityMap = new EntityMap(entityCapacity);
            InterceptorRegistry = new InterceptorRegistry(entityCapacity);
            OverTimeEffectsRegistry = new OverTimeEffectsRegistry(initialBufferSize);
            ActionRegister = new ActionRegister(initialBufferSize);
            StatRegistry = new StatRegistry(entityCapacity);
            DeathRegistry = new DeathRegistry(initialBufferSize);
            EntityTypeRegistry = new EntityTypeRegistry();
            if (entityDefinitions != null)
                EntityTypeRegistry.Build(entityDefinitions);
            
            PendingInits = new NativeList<PendingEntityInit>(initialBufferSize, Allocator.Persistent);
        }

        public void Dispose()
        {
            OverTimeEffectsRegistry.Dispose();
            InterceptorRegistry.Dispose();
            StatRegistry.Dispose();
            DeathRegistry.Dispose();
            EntityMap.Dispose();
            EntityTypeRegistry.Dispose();
            if (PendingInits.IsCreated) PendingInits.Dispose();
        }
    }
}