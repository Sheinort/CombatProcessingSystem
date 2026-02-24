using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Combat
{
    /// <summary>
    /// Applies StatChange requests to Stats and Calculates + Updates values in corresponding storages.
    /// Holds CalculateStat and ApplyCalculatedStat static methods.
    /// </summary>
    [BurstCompile]
    public static class StatChangeSystem
    {
        public static void Update(CombatWorld world)
        {
            var requests = world.ActionRegister.StatChangeRequests;
            if (requests.Length == 0) return;

            world.ActionRegister.StatChangeResults.Resize(requests.Length, NativeArrayOptions.ClearMemory);

            var requestsArray = world.ActionRegister.StatChangeRequests.AsArray();
            var resultsArray = world.ActionRegister.StatChangeResults.AsArray();

            Execute(
                ref requestsArray,
                ref resultsArray,
                ref world.StatRegistry.ResourceStats,
                ref world.StatRegistry.CombatStats,
                ref world.StatRegistry.ResistanceStats,
                ref world.StatRegistry.Resources,
                ref world.StatRegistry.Resistances,
                ref world.StatRegistry.CombatStatBlocks
            );
        }

        [BurstCompile]
        private static void Execute(
            ref NativeArray<StatChangeRequest> requests,
            ref NativeArray<StatChangeResult> results,
            ref EnumIndexedArray<ResourceStatType, Stat> resourceStats,
            ref EnumIndexedArray<CombatStatType, Stat> combatStats,
            ref EnumIndexedArray<ResistanceStatType, Stat> resistanceStats,
            ref EnumIndexedArray<ResourceType, Resource> resources,
            ref EnumIndexedArray<DamageType, Resist> resistances,
            ref NativeArray<CombatStatBlock> combatStatBlocks)
        {
            int length = requests.Length;
            for (int i = 0; i < length; i++)
            {   
                var req = requests[i];
                int entityIdx = req.TargetEntityArrayIndex;
                if (entityIdx < 0) continue;
    
                ModifyAndApplyStat(
                    ref req.Target, entityIdx, req.ChangeType, req.Value,
                    ref resourceStats, ref combatStats, ref resistanceStats,
                    ref resources, ref resistances, ref combatStatBlocks,
                    out float delta);

                results[i] = new StatChangeResult {
                    TargetID         = req.TargetID,
                    Target           = req.Target,
                    TargetArrayIndex = entityIdx,
                    Delta            = delta
                };
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateStat(in Stat stat) {
            return (stat.BaseValue + stat.FlatBonus) * (1f + stat.MultiplierAdditive) * stat.MultiplierMultiplicative;
        }
         

        [BurstCompile]
        public static void ApplyCalculatedStat(
            ref float delta,
            ref StatTarget target,
            int index,
            float value,
            ref NativeArray<CombatStatBlock> blocks,
            ref EnumIndexedArray<DamageType, Resist> resists,
            ref EnumIndexedArray<ResourceType, Resource> resources)
        {
            switch (target.Category)
            {
                case StatCategory.Resource:
                    SetResourceMax(ref delta, ref resources.GetRef((ResourceType)target.SubIndex, index), value);
                    break;
                case StatCategory.CombatStat:
                    var b = blocks[index];
                    b.SetStat((CombatStatType)target.SubIndex, value, out delta);
                    blocks[index] = b;
                    break;
                case StatCategory.Resistance:
                    SetResistance(ref delta, ref resists.GetRef((DamageType)target.SubIndex, index), value);
                    break;
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ModifyAndApplyStat(
            ref StatTarget target,
            int entityIdx,
            StatChangeType changeType,
            float value,
            ref EnumIndexedArray<ResourceStatType, Stat> resourceStats,
            ref EnumIndexedArray<CombatStatType, Stat> combatStats,
            ref EnumIndexedArray<ResistanceStatType, Stat> resistanceStats,
            ref EnumIndexedArray<ResourceType, Resource> resources,
            ref EnumIndexedArray<DamageType, Resist> resistances,
            ref NativeArray<CombatStatBlock> combatStatBlocks,
            out float delta)
        {
            delta = 0f;
            switch (target.Category)
            {
                case StatCategory.Resource:
                {
                    var type  = (ResourceStatType)target.SubIndex;
                    var slice = resourceStats[type];
                    var stat  = slice[entityIdx];
                    ApplyChange(ref stat, changeType, value);
                    slice[entityIdx] = stat;
                    SetResourceMax(ref delta, ref resources.GetRef((ResourceType)target.SubIndex, entityIdx), CalculateStat(stat));
                    break;
                }
                case StatCategory.CombatStat:
                {
                    var type  = (CombatStatType)target.SubIndex;
                    var slice = combatStats[type];
                    var stat  = slice[entityIdx];
                    ApplyChange(ref stat, changeType, value);
                    slice[entityIdx] = stat;
                    var block = combatStatBlocks[entityIdx];
                    block.SetStat(type, CalculateStat(stat), out delta);
                    combatStatBlocks[entityIdx] = block;
                    break;
                }
                case StatCategory.Resistance:
                {
                    var type  = (ResistanceStatType)target.SubIndex;
                    var slice = resistanceStats[type];
                    var stat  = slice[entityIdx];
                    ApplyChange(ref stat, changeType, value);
                    slice[entityIdx] = stat;
                    SetResistance(ref delta, ref resistances.GetRef((DamageType)target.SubIndex, entityIdx), CalculateStat(stat));
                    break;
                }
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetResourceMax(ref float delta, ref Resource resource, float value)
        {
            delta = value - resource.MaxValue;
            resource.MaxValue = value;
            resource.Value    = math.min(resource.Value, value);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetResistance(ref float delta, ref Resist resist, float value)
        {
            delta = value - resist.Value;
            resist = new Resist {
                Value              = value,
                CalculatedFraction = value <= 0f ? 0f : value / (value + 100f)
            };
        }

     
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyChange(ref Stat stat, StatChangeType changeType, float value)
        {
            switch (changeType)
            {
                case StatChangeType.Flat:                     stat.FlatBonus += value;               break;
                case StatChangeType.MultiplierMultiplicative: stat.MultiplierMultiplicative *= value; break;
                case StatChangeType.MultiplierAdditive:       stat.MultiplierAdditive += value;       break;
            }
        }
    }
}