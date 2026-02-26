using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(menuName = "Combat/Interceptors/Shield Absorb")]
    public class ShieledAbsorb : InterceptorDefinitionSO
    {
        public float AbsorbValue;

        public override void  Register(EntityID entityID, CombatWorld combatWorld)
        {
            CombatCommands.AddToInterceptors<ShieldAbsorbInterceptor>(combatWorld, entityID, new InterceptorData{FloatValue1 = AbsorbValue});
        }
    }
    public sealed class ShieldAbsorbInterceptor : IResourceInterceptorBeforeResist
    {
        // This is the easiest to work with, but a direct map of array entityID to the array of flattened Data would be better at scale.
        NativeParallelMultiHashMap<EntityID, InterceptorData> map;

        public void Initialize(int initialSize)
        {
            map = new NativeParallelMultiHashMap<EntityID, InterceptorData>(initialSize, Allocator.Persistent);
        }

        public void Update(CombatWorld combatWorld)
        {
            var requests = combatWorld.ActionRegister.ResourceChangeRequests;
            ShieldAbsorbInterceptorSystem.Execute(ref map, ref requests);
        }

        public void Add(EntityID entityId, InterceptorData data) => map.Add(entityId, data);
        public void Remove(EntityID entityId) => map.Remove(entityId);

        public NativeList<InterceptorData> GetInterceptorData(EntityID entityID)
        {
            var result = new NativeList<InterceptorData>(Allocator.Temp);

            if (!map.TryGetFirstValue(entityID, out var data, out var iterator))
                return result;
            do
            {
                result.Add(data);
            }
            while (map.TryGetNextValue(out data, ref iterator));

            return result;
        }

        public void Dispose()
        {
            map.Dispose();
        }
    }

    [BurstCompile]
    internal static class ShieldAbsorbInterceptorSystem
    {
        [BurstCompile]
        public static void Execute(
            ref NativeParallelMultiHashMap<EntityID, InterceptorData> map,
            ref NativeList<ResourceChangeRequest> requests)
        {
            for (int i = 0; i < requests.Length; i++) {
                ref var request = ref requests.ElementAt(i);
                if (request.Value >= 0) continue; 
                if (!map.TryGetFirstValue(request.TargetID, out var data, out var iterator)) continue;

                data.FloatValue1 += request.Value;
                request.Value = math.min(data.FloatValue1, 0);
                data.FloatValue1 = math.max(data.FloatValue1, 0);
                map.SetValue(data, iterator);
                if (data.FloatValue1 <= 0) {
                    map.Remove(iterator);
                }
            }
        }
    }
     
}