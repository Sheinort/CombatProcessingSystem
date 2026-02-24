using System;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Holds request and result buffers for stat and resource changes.
    /// </summary>
    public class ActionRegister : IDisposable
    {
        public NativeList<StatChangeRequest> StatChangeRequests { get; }
        public NativeList<StatChangeResult> StatChangeResults{ get; }
        public NativeList<ResourceChangeRequest> ResourceChangeRequests{ get; }
        public NativeList<ResourceChangeResult> ResourceChangeResult{ get; }

        public ActionRegister(int initialBufferSize)
        {
            StatChangeRequests = new NativeList<StatChangeRequest>(initialBufferSize, Allocator.Persistent);
            StatChangeResults = new NativeList<StatChangeResult>(initialBufferSize, Allocator.Persistent);
            ResourceChangeRequests = new NativeList<ResourceChangeRequest>(initialBufferSize, Allocator.Persistent);
            ResourceChangeResult = new NativeList<ResourceChangeResult>(initialBufferSize, Allocator.Persistent);

        }

        public void ClearBuffers()
        {
            StatChangeRequests.Clear();
            StatChangeResults.Clear();
            ResourceChangeRequests.Clear();
            ResourceChangeResult.Clear();
        }
        public void Dispose()
        {
            if (StatChangeRequests.IsCreated) StatChangeRequests.Dispose();
            if (StatChangeResults.IsCreated) StatChangeResults.Dispose();
            if (ResourceChangeRequests.IsCreated) ResourceChangeRequests.Dispose();
            if (ResourceChangeResult.IsCreated) ResourceChangeResult.Dispose();
        }
    }
}