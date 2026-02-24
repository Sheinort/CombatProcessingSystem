using System;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Holds all DeathRequests and Corpses.
    /// </summary>
    public class DeathRegistry : IDisposable
    {
        public NativeList<DeathRequest> DeathRequestList;
        public NativeList<DeadEntity> Corpses;
        
        public DeathRegistry(int initialBufferSize)
        {
            DeathRequestList = new NativeList<DeathRequest>(initialBufferSize, Allocator.Persistent);
            Corpses = new NativeList<DeadEntity>(initialBufferSize, Allocator.Persistent);

        }

        public void AddCorpse(DeadEntity  deadEntity)
        {
            Corpses.Add(deadEntity);
        }
        public void Dispose()
        {
            if (DeathRequestList.IsCreated) DeathRequestList.Dispose();
            if (Corpses.IsCreated) Corpses.Dispose();

        }
    }
}