using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Combat
{
    /// <summary>
    /// Holds burstable method for mass entity removal.
    /// </summary>
    [BurstCompile]
    public static class StatRegistryExtensions
    {
        [BurstCompile]
        public static void RemoveEntitiesBunch(
            ref NativeArray<RemovedEntity> RemovedEntities,
         ref EnumIndexedArray<ResourceStatType, Stat> ResourceStats,
        ref EnumIndexedArray<CombatStatType, Stat> CombatStats,
        ref EnumIndexedArray<ResistanceStatType, Stat> ResistanceStats,
        ref EnumIndexedArray<ResourceType, Resource> Resources,
        ref  EnumIndexedArray<DamageType, Resist> Resistances,
        ref NativeArray<CombatStatBlock> CombatStatBlocks,
        ref NativeArray<DeathFlags> DeathFlags)
        {
            for (int i = 0; i < RemovedEntities.Length; i++) {
                var movedFromIndex = RemovedEntities[i].MovedFrom;
                var removedIndex = RemovedEntities[i].RemovedIndex;
                ResourceStats.SwapBack(removedIndex, movedFromIndex);
                CombatStats.SwapBack(removedIndex, movedFromIndex);
                ResistanceStats.SwapBack(removedIndex, movedFromIndex);
                Resources.SwapBack(removedIndex, movedFromIndex);
                Resistances.SwapBack(removedIndex, movedFromIndex);
                var blocks = CombatStatBlocks;
                blocks[removedIndex] = blocks[movedFromIndex];
                var deathResolutionTypes = DeathFlags;
                deathResolutionTypes[removedIndex] = deathResolutionTypes[movedFromIndex];    
            }
        }
    }
    /// <summary>
    /// Holds all permutations of stats and resources.
    /// </summary>
    public class StatRegistry : IDisposable
    {
        // Raw stat values — split to match the new enum structure
        public EnumIndexedArray<ResourceStatType, Stat> ResourceStats;
        public EnumIndexedArray<CombatStatType, Stat> CombatStats;
        public EnumIndexedArray<ResistanceStatType, Stat> ResistanceStats;

        // Computed outputs — unchanged, these are derived from stats above
        public EnumIndexedArray<ResourceType, Resource> Resources;
        public EnumIndexedArray<DamageType, Resist> Resistances;
        public NativeArray<CombatStatBlock> CombatStatBlocks;
        public NativeArray<DeathFlags> DeathFlags;

        public StatRegistry(int entityCapacity)
        {
            ResourceStats = new EnumIndexedArray<ResourceStatType, Stat>(entityCapacity, Allocator.Persistent);
            CombatStats = new EnumIndexedArray<CombatStatType, Stat>(entityCapacity, Allocator.Persistent);
            ResistanceStats = new EnumIndexedArray<ResistanceStatType, Stat>(entityCapacity, Allocator.Persistent);
            Resources = new EnumIndexedArray<ResourceType, Resource>(entityCapacity, Allocator.Persistent);
            Resistances = new EnumIndexedArray<DamageType, Resist>(entityCapacity, Allocator.Persistent);
            CombatStatBlocks = new NativeArray<CombatStatBlock>(entityCapacity, Allocator.Persistent);
            DeathFlags = new NativeArray<DeathFlags>(entityCapacity, Allocator.Persistent);
        }

        public void RemoveEntity(int removedIndex, int movedFromIndex)
        {
            if (movedFromIndex == -1) return;
            ResourceStats.SwapBack(removedIndex, movedFromIndex);
            CombatStats.SwapBack(removedIndex, movedFromIndex);
            ResistanceStats.SwapBack(removedIndex, movedFromIndex);
            Resources.SwapBack(removedIndex, movedFromIndex);
            Resistances.SwapBack(removedIndex, movedFromIndex);
            var blocks = CombatStatBlocks;
            blocks[removedIndex] = blocks[movedFromIndex];
            var deathResolutionTypes = DeathFlags;
            deathResolutionTypes[removedIndex] = deathResolutionTypes[movedFromIndex];
        }

        public void GetRemoveEntitiesBunchJob(NativeArray<RemovedEntity> RemovedEntities)
        {
            StatRegistryExtensions.RemoveEntitiesBunch(ref RemovedEntities, ref ResourceStats, ref CombatStats, ref ResistanceStats, ref Resources, ref Resistances, ref CombatStatBlocks, ref DeathFlags);
        }
        public void ClearSlot(int index)
        {
            ResourceStats.ClearSlot(index);
            CombatStats.ClearSlot(index);
            ResistanceStats.ClearSlot(index);
            Resources.ClearSlot(index);
            Resistances.ClearSlot(index);
            var blocks = CombatStatBlocks;
            blocks[index] = default;
            var deathResolutionTypes = DeathFlags;
            deathResolutionTypes[index] |= Combat.DeathFlags.LeavesCorpse;
        }

        public void Dispose()
        {
            ResourceStats.Dispose();
            CombatStats.Dispose();
            ResistanceStats.Dispose();
            Resources.Dispose();
            Resistances.Dispose();
            if (CombatStatBlocks.IsCreated) CombatStatBlocks.Dispose();
            if (DeathFlags.IsCreated) DeathFlags.Dispose();
        }
    }
}