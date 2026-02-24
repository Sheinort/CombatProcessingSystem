using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Combat
{
    [BurstCompile]
    public static class ConvertResourceRequestsToFlatSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var requests = combatWorld.ActionRegister.ResourceChangeRequests;
            if (requests.Length == 0) return;
            var resources = combatWorld.StatRegistry.Resources;
            Execute(ref requests, ref resources);
        }

        [BurstCompile]
        private static void Execute(
            ref NativeList<ResourceChangeRequest> requests,
            ref EnumIndexedArray<ResourceType, Resource> resources)
        {
            for (int i = 0; i < requests.Length; i++) {
                ref var req = ref requests.ElementAt(i);
                if (req.ChangeType == ResourceChangeType.Flat) continue;
                var targetRes = resources[req.ChangeTypeTarget][req.TargetEntityArrayIndex];
                req.Value = req.ChangeType switch
                {
                    ResourceChangeType.FractionOfMax => req.Value * targetRes.MaxValue,
                    ResourceChangeType.FractionOfCurrent => req.Value * targetRes.Value,
                    _ => req.Value
                };
            }
        }
    }
    [BurstCompile]
    public static class ApplyResistsSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var requests = combatWorld.ActionRegister.ResourceChangeRequests;
            var resists = combatWorld.StatRegistry.Resistances;
            Execute(ref requests, ref resists);
        }

        [BurstCompile]
        private static void Execute(
            ref NativeList<ResourceChangeRequest> requests,
            ref EnumIndexedArray<DamageType, Resist> resists)
        {
            for (int i = 0; i < requests.Length; i++) {
                ref var req = ref requests.ElementAt(i);
                if (req.Value < 0)
                {
                    var resist = resists[req.DamageType][req.TargetEntityArrayIndex].CalculatedFraction;
                    req.Value *= (1f - resist);
                }
            }
        }
    }
    [BurstCompile]
    public static class ApplyResourceChangesSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var requests = combatWorld.ActionRegister.ResourceChangeRequests;
            var resources = combatWorld.StatRegistry.Resources;
            var results = combatWorld.ActionRegister.ResourceChangeResult;
            combatWorld.ActionRegister.ResourceChangeResult.Resize(requests.Length, NativeArrayOptions.UninitializedMemory);
            Execute(ref requests, ref resources, ref results);
        }

        [BurstCompile]
        private static void Execute(
            ref NativeList<ResourceChangeRequest> requests,
            ref EnumIndexedArray<ResourceType, Resource> resources,
            ref NativeList<ResourceChangeResult> results)
        {
            for (int i = 0; i < requests.Length; i++) {
                ref var req = ref requests.ElementAt(i);
                int entityIdx = req.TargetEntityArrayIndex;
                ref var result = ref results.ElementAt(i);

                float change = req.Value;
                for (int j = 0; j < req.OrderOfModification.Count; j++)
                {
                    if (change == 0) break;

                    var resType = req.OrderOfModification.Get(j);
                    var resourceList = resources[resType];
                    var resource = resourceList[entityIdx];
                    var beforeValue = resource.Value;

                    float newValue = resource.Value + change;
                    resource.Value = math.clamp(newValue, 0, resource.MaxValue);
                    change = newValue - resource.Value;
                    resourceList[entityIdx] = resource;

                    if (beforeValue != resource.Value)
                        result.AddResult(resType, beforeValue, resource.Value);
                }
            }
        }
    }
}