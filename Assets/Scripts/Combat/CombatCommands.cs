using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Combat
{
    /// <summary>
    /// Performs operations across different data. All one-off changes should be made through this class.
    /// </summary>
    public static class CombatCommands
    {

        public static EntityID CreateEntity(CombatWorld world)
        {
            var (id, arrayIndex) = world.EntityMap.Add();
            if (id == new EntityID(-1)) return id;
            world.StatRegistry.ClearSlot(arrayIndex);
            return id;
        }

        public static EntityID CreateEntity(CombatWorld world, int entityTypeIndex)
        {
            var id = CreateEntity(world);
            if (id == new EntityID(-1)) return id;

            world.PendingInits.Add(new PendingEntityInit
            {
                ArrayIndex = world.EntityMap.GetIndex(id),
                TypeIndex  = entityTypeIndex
            });
            return id;
        }
        
        public static void ResolvePendingInits(CombatWorld world)
        {
            if (world.PendingInits.Length == 0) return;

            int batchCount = Mathf.Max(1,
                world.PendingInits.Length / (Environment.ProcessorCount * 2));

            new InitEntityStatsJob
            {
                Pending          = world.PendingInits.AsArray(),
                EntityTypes      = world.EntityTypeRegistry.EntityTypes,
                ResourceStats    = world.StatRegistry.ResourceStats,
                CombatStats      = world.StatRegistry.CombatStats,
                ResistanceStats  = world.StatRegistry.ResistanceStats,
                CombatStatBlocks = world.StatRegistry.CombatStatBlocks,
                Resistances      = world.StatRegistry.Resistances,
                Resources        = world.StatRegistry.Resources,
                DeathFlags       = world.StatRegistry.DeathFlags,
            }.Run();

            world.PendingInits.Clear();
        }

        public static void CombatLoop(CombatWorld combatWorld)
        {
            Profiler.BeginSample("CombatLoop.EntityInit");
            ResolvePendingInits(combatWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("CombatLoop.IndexerSystem");
            IndexerSystem.Update(combatWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("CombatLoop.TemporaryStatChangesSystem");
            TemporaryStatChangesSystem.Update(combatWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("CombatLoop.Stat");
            UpdateStatSystems(combatWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("CombatLoop.ResourceChangesOverTimeSystem");
            ResourceChangesOverTimeSystem.Update(combatWorld);
            Profiler.EndSample();
            
            Profiler.BeginSample("CombatLoop.Resource");
            UpdateResourceSystems(combatWorld);
            Profiler.EndSample();
            


            Profiler.BeginSample("CombatLoop.Death");
            DeathSystem.Update(combatWorld);
            Profiler.EndSample();
            Profiler.BeginSample("CombatLoop.EndFrameCleanup");
            EndFrameCleanup(combatWorld);
            Profiler.EndSample();
        }

        public static void UpdateStatSystems(CombatWorld combatWorld)
        {
            foreach (var i in combatWorld.InterceptorRegistry.GetInterceptors<IStatInterceptorBeforeApplication>()) {
                i.Update(combatWorld);
            }
            StatChangeSystem.Update(combatWorld);
            foreach (var i in combatWorld.InterceptorRegistry.GetInterceptors<IStatInterceptorAfterApplication>()) {
                i.Update(combatWorld);
            }
        }
        public static void UpdateResourceSystems(CombatWorld combatWorld)
        {
            ConvertResourceRequestsToFlatSystem.Update(combatWorld);
            foreach (var i in combatWorld.InterceptorRegistry.GetInterceptors<IResourceInterceptorBeforeResist>()) {
                i.Update(combatWorld);
            }
            ApplyResistsSystem.Update(combatWorld);
            foreach (var i in combatWorld.InterceptorRegistry.GetInterceptors<IResourceInterceptorBeforeApplication>()) {
                i.Update(combatWorld);
            }
            ApplyResourceChangesSystem.Update(combatWorld);
            foreach (var i in combatWorld.InterceptorRegistry.GetInterceptors<IResourceInterceptorAfterApplication>()) {
                i.Update(combatWorld);
            }
        }
        public static void DestroyEntity(CombatWorld world, EntityID id)
        {
            if (id.Value == -1) return;

            var (removedIndex, movedFromIndex) = world.EntityMap.Remove(id);
            if (movedFromIndex < 0) return;

            world.StatRegistry.RemoveEntity(removedIndex, movedFromIndex);
            RemoveTemporaryStatChangesForEntity(world, id);

        }

        public static IReadOnlyList<TInterceptor> GetInterceptors<TInterceptor>(CombatWorld combatWorld) where TInterceptor : IInterceptor
        {
            return combatWorld.InterceptorRegistry.GetInterceptors<TInterceptor>();
        }

        public static void AddToInterceptors<TInterceptor>(CombatWorld combatWorld, EntityID entityID, InterceptorData data) where TInterceptor : IInterceptor
        {
            combatWorld.InterceptorRegistry.AddToInterceptors<TInterceptor>(entityID, data);
        }

        public static void RemoveFromInterceptors<TInterceptor>(CombatWorld combatWorld,EntityID entityID) where TInterceptor : IInterceptor
        {
            combatWorld.InterceptorRegistry.RemoveFromInterceptors<TInterceptor>(entityID);
        }

        public static TInterceptor GetInterceptor<TInterceptor>(CombatWorld combatWorld) where TInterceptor : IInterceptor
        {
           return combatWorld.InterceptorRegistry.GetInterceptor<TInterceptor>();
        }
        public static void AddTemporaryStatChange(CombatWorld world, StatChangeRequest request, float duration)
        {
            world.ActionRegister.StatChangeRequests.Add(request);
            world.OverTimeEffectsRegistry.AddTemporaryStatChange(request, duration);
        }

        public static void RemoveTemporaryStatChangesForEntity(CombatWorld world, EntityID entityID)
        {
            var allEffects = world.OverTimeEffectsRegistry.TemporaryStatChange;

            for (int i = allEffects.Length - 1; i >= 0; i--)
            {
                if (allEffects[i].TargetID == entityID)
                    RemoveTemporaryStatChange(world, i);
            }
        }

        public static void RemoveTemporaryStatChange(CombatWorld world, int index)
        {
            var inverse = world.OverTimeEffectsRegistry.TemporaryStatChange[index];
            inverse.Invert();
            world.ActionRegister.StatChangeRequests.Add(inverse);
            world.OverTimeEffectsRegistry.RemoveTemporaryStatChange(index);
        }

        public static void EndFrameCleanup(CombatWorld world)
        {
            world.ActionRegister.ClearBuffers();
            world.DeathRegistry.DeathRequestList.Clear();
        }



        [BurstCompile]
        private struct InitEntityStatsJob : IJob
        {
            [NativeDisableParallelForRestriction]  [ReadOnly] public NativeArray<PendingEntityInit> Pending;
            [NativeDisableParallelForRestriction]  [ReadOnly] public NativeArray<EntityTypeData> EntityTypes;
            [NativeDisableParallelForRestriction] public EnumIndexedArray<ResourceStatType, Stat> ResourceStats;
            [NativeDisableParallelForRestriction] public EnumIndexedArray<CombatStatType, Stat> CombatStats;
            [NativeDisableParallelForRestriction] public EnumIndexedArray<ResistanceStatType, Stat> ResistanceStats;
            [NativeDisableParallelForRestriction] public EnumIndexedArray<ResourceType, Resource> Resources;
            [NativeDisableParallelForRestriction]  public EnumIndexedArray<DamageType, Resist> Resistances;
            [NativeDisableParallelForRestriction] public NativeArray<CombatStatBlock> CombatStatBlocks;
            [NativeDisableParallelForRestriction] public NativeArray<DeathFlags> DeathFlags;

            public void Execute()
            {
                for (int i = 0; i < Pending.Length; i++) {
                     var init  = Pending[i];
                    var index = init.ArrayIndex;
                    var data  = EntityTypes[init.TypeIndex];

                    for (int s = 0; s < data.ResourceStats.Length; s++)
                    {
                        var value = data.ResourceStats[s];
                        if (value <= 0) continue;
                        var type = (ResourceStatType)s;
                        var slice = ResourceStats[type];
                        var newStat = Stat.Create(value);
                        slice[index] = newStat;
                        var delta = 0f;
                        value = StatChangeSystem.CalculateStat(newStat);
                        var t = new StatTarget(type);
                        StatChangeSystem.ApplyCalculatedStat(ref delta, ref t, index, value, ref CombatStatBlocks, ref Resistances, ref Resources);
                    }

                    for (int s = 0; s < data.CombatStats.Length; s++)
                    {
                        var value = data.CombatStats[s];
                        if (value <= 0) continue;
                        var type = (CombatStatType)s;
                        var slice = CombatStats[type];
                        var newStat = Stat.Create(value);
                        slice[index] = newStat;
                        var delta = 0f;
                        value = StatChangeSystem.CalculateStat(newStat);
                        var t = new StatTarget(type);
                        StatChangeSystem.ApplyCalculatedStat(ref delta, ref t, index, value, ref CombatStatBlocks, ref Resistances, ref Resources);
                    }

                    for (int s = 0; s < data.ResistanceStats.Length; s++)
                    {
                        var value = data.ResistanceStats[s];
                        if (value <= 0) continue;
                        var type = (ResistanceStatType)s;
                        var slice = ResistanceStats[type];
                        var newStat = Stat.Create(value);
                        slice[index] = newStat;
                        var delta = 0f;
                        value = StatChangeSystem.CalculateStat(newStat);
                        var t = new StatTarget(type);
                        StatChangeSystem.ApplyCalculatedStat(ref delta, ref t, index, value, ref CombatStatBlocks, ref Resistances, ref Resources);
                    }

                    for (int r = 0; r < data.StartingResources.Length; r++)
                    {
                        var value = data.StartingResources[r];
                        if (value <= 0) continue;
                        var slice = Resources[(ResourceType)r];
                        slice[index] = new Resource { MaxValue = slice[index].MaxValue, Value = value };
                    }

                    DeathFlags[index] = data.DeathFlags;
                }
               
            }
        }
    }
}