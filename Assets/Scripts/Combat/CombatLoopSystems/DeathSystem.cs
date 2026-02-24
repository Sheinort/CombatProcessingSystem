using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Detects Deaths based on ResourceChangeRequests in the current frame.
    /// Deletes dead entities from every list and writes into a Corpse list if the entity needs to leave it.
    /// </summary>
    [BurstCompile]
    public static class DeathSystem
    {
        public static void Update(CombatWorld world)
        {
            var requests        = world.DeathRegistry.DeathRequestList;
            var resourceChanges = world.ActionRegister.ResourceChangeRequests;
            if (resourceChanges.Length == 0 && requests.Length == 0) return;
            var resourceChangesArray = resourceChanges.AsArray();
            var healths              = world.StatRegistry.Resources[ResourceType.Health];
            var armors               = world.StatRegistry.Resources[ResourceType.Armor];
            float time               = Time.realtimeSinceStartup;

            ProcessDeaths(
                ref resourceChangesArray,
                ref healths,
                ref armors,
                ref world.StatRegistry.DeathFlags,
                ref requests,
                ref world.DeathRegistry.Corpses,
                time);

            if (requests.Length == 0) return;

            var requestsArray = requests.AsArray();
            requestsArray.Sort(new ReverseIndexComparer());
            var freeIds       = world.EntityMap.GetFreeIds();

            if (freeIds.Capacity < freeIds.Length + requests.Length)
                freeIds.Capacity = freeIds.Length + requests.Length;

            var removedEntitiesFromMap = new NativeArray<RemovedEntity>(requestsArray.Length, Allocator.TempJob);

            
            world.EntityMap.RemoveEntitiesBunch(requestsArray, removedEntitiesFromMap);
            world.StatRegistry.GetRemoveEntitiesBunchJob(removedEntitiesFromMap);

            FreeEntityIds(ref freeIds, ref requestsArray);

            removedEntitiesFromMap.Dispose();

            foreach (var entity in requestsArray)
            {
                CombatCommands.RemoveTemporaryStatChangesForEntity(world, entity.EntityID);
            }
        }

        
        [BurstCompile]
        private static void ProcessDeaths(
            ref NativeArray<ResourceChangeRequest> resourceChanges,
            ref NativeSlice<Resource> healths,
            ref NativeSlice<Resource> armors,
            ref NativeArray<DeathFlags> deathFlags,
            ref NativeList<DeathRequest> deathRequests,
            ref NativeList<DeadEntity> corpses,
            float timeSinceStartup)
        {
            var seen = new NativeHashSet<int>(resourceChanges.Length, Allocator.Temp);

            for (int i = 0; i < resourceChanges.Length; i++)
            {
                var req = resourceChanges[i];
                int idx = req.TargetEntityArrayIndex;
                var health = healths[idx];
                if (health.Value > 0 || !seen.Add(idx)) continue;

                var flags = deathFlags[idx];

                deathRequests.Add(new DeathRequest {
                    ArrayIndex = idx,
                    EntityID   = req.TargetID,
                    Flags      = flags
                });

                if ((flags & DeathFlags.LeavesCorpse) != 0)
                {
                    corpses.Add(new DeadEntity {
                        ID                 = req.TargetID,
                        OriginalArrayIndex = idx,
                        MaxHealth          = health.MaxValue,
                        MaxArmor           = armors[idx].MaxValue,
                        Position           = default,
                        Rotation           = default,
                        TimeOfDeath        = timeSinceStartup
                    });
                }
            }
            seen.Dispose();
        }

        [BurstCompile]
        private static void FreeEntityIds(ref NativeList<EntityID> freeIds, ref NativeArray<DeathRequest> requests)
        {
            for (int i = 0; i < requests.Length; i++) {
                var request = requests[i];
                if ((request.Flags & DeathFlags.LeavesCorpse) == 0)
                    freeIds.AddNoResize(request.EntityID);
            }
        }
        [BurstCompile]
        private struct ReverseIndexComparer : IComparer<DeathRequest>
        {
            public int Compare(DeathRequest x, DeathRequest y) => y.ArrayIndex.CompareTo(x.ArrayIndex);
        }
    }
}