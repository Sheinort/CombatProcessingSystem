using Unity.Burst;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Writes array index into requests based on the target ID.
    /// </summary>
    [BurstCompile]
    public static class IndexerSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var entityMap        = combatWorld.EntityMap.AsJobView();
            var requestsResource = combatWorld.ActionRegister.ResourceChangeRequests;
            if (requestsResource.Length > 0) {
                ExecuteResources(ref requestsResource, ref entityMap);
            }
            var requestsStats    = combatWorld.ActionRegister.StatChangeRequests;
            if (requestsStats.Length == 0) return;
            ExecuteStats(ref requestsStats, ref entityMap);
        }

        [BurstCompile]
        private static void ExecuteResources(ref NativeList<ResourceChangeRequest> requests, ref EntityMap.JobView entityMap)
        {
            for (int i = requests.Length-1; i >= 0 ; i--) {
                ref var req = ref requests.ElementAt(i);
                req.TargetEntityArrayIndex = entityMap.GetIndex(req.TargetID);
                if (req.TargetEntityArrayIndex < 0) requests.RemoveAtSwapBack(i);
            }
        }

        [BurstCompile]
        private static void ExecuteStats(ref NativeList<StatChangeRequest> requests, ref EntityMap.JobView entityMap)
        {
            for (int i = requests.Length-1; i >= 0 ; i--) {
                ref var req = ref requests.ElementAt(i);
                req.TargetEntityArrayIndex = entityMap.GetIndex(req.TargetID);
                if (req.TargetEntityArrayIndex < 0) requests.RemoveAtSwapBack(i);
            }
        }
    }
}