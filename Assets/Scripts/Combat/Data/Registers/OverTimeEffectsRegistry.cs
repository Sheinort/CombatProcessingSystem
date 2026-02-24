using System;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Holds Time based changes to stats and resources.
    /// </summary>
    public class OverTimeEffectsRegistry : IDisposable
    {

        public NativeList<StatChangeRequest> TemporaryStatChange { get; }
        public NativeList<float> TemporaryStatBuffsTimers{ get; }
        
        public NativeList<ResourceChangeRequest> ResourceChangeOverTimeRequests{ get; }
        public NativeList<ResourceChangeOverTime> ResourceChangeOverTime{ get; }

        public void AddResourceChangeOverTime(ResourceChangeRequest request, ResourceChangeOverTime behaviour)
        {
            ResourceChangeOverTimeRequests.Add(request);
            ResourceChangeOverTime.Add(behaviour);
        }
        public void RemoveResourceChangeOverTime(int index)
        {
            ResourceChangeOverTimeRequests.RemoveAtSwapBack(index);
            ResourceChangeOverTime.RemoveAtSwapBack(index);
        }
        
        public OverTimeEffectsRegistry(int initialBufferSize = 5)
        {
            TemporaryStatChange = new NativeList<StatChangeRequest>(initialBufferSize, Allocator.Persistent);
            TemporaryStatBuffsTimers = new NativeList<float>(initialBufferSize, Allocator.Persistent);
            ResourceChangeOverTime = new NativeList<ResourceChangeOverTime>(initialBufferSize, Allocator.Persistent);
            ResourceChangeOverTimeRequests = new NativeList<ResourceChangeRequest>(initialBufferSize, Allocator.Persistent);
        }
        

        public void RemoveTemporaryStatChange(int index)
        {
            TemporaryStatChange.RemoveAtSwapBack(index);
            TemporaryStatBuffsTimers.RemoveAtSwapBack(index);
        }
        public void AddTemporaryStatChange(StatChangeRequest request, float time)
        {
            TemporaryStatChange.Add(request);
            TemporaryStatBuffsTimers.Add(time);
        }
        public void Dispose()
        {
            if (TemporaryStatChange.IsCreated) TemporaryStatChange.Dispose();
            if (TemporaryStatBuffsTimers.IsCreated) TemporaryStatBuffsTimers.Dispose();
            
            if  (ResourceChangeOverTimeRequests.IsCreated) ResourceChangeOverTimeRequests.Dispose();
            if  (ResourceChangeOverTime.IsCreated) ResourceChangeOverTime.Dispose();
        }
    }
}