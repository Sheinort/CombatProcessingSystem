using System;
using Unity.Burst;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Static class for burst entity removal method.
    /// </summary>
    [BurstCompile]
    public static class EntityMapExtensions
    {
        [BurstCompile]
        public static void RemoveEntiiesFromMap( ref NativeArray<DeathRequest> DeathRequests,
            ref NativeArray<int> _idToIndex,
            ref NativeArray<EntityID> _indexToId,
            ref  NativeReference<int> _count,
            ref NativeArray<RemovedEntity> RemovedEntities)
        {
            for (int i = 0; i < DeathRequests.Length; i++) {
                int removedIndex = _idToIndex[DeathRequests[i].EntityID.Value];
                int lastIndex = _count.Value - 1;
                int movedFrom = -1;

                if (removedIndex != lastIndex)
                {
                    EntityID lastId = _indexToId[lastIndex];
                    _indexToId[removedIndex] = lastId;
                    _idToIndex[lastId.Value] = removedIndex;
                    movedFrom = lastIndex;
                }

                _idToIndex[DeathRequests[i].EntityID.Value] = -1;
                _indexToId[lastIndex] = default;
                _count.Value--;
                if (movedFrom >= 0)
                    RemovedEntities[i] = new RemovedEntity {
                        ID = DeathRequests[i].EntityID,
                        RemovedIndex = removedIndex,
                        MovedFrom = movedFrom
                    };
            }
        }
    }
    /// <summary>
    /// Maps EntityIDs to indices in contiguous arrays and reverse of that.
    /// </summary>
    public sealed class EntityMap : IDisposable
    {
        private NativeArray<int> _idToIndex;
        private NativeArray<EntityID> _indexToId;
        private NativeList<EntityID> _freeIds;
        private NativeReference<int> _count;
        private int _idCapacity;
        private int _entityCapacity;
        private EntityID _nextId;

        public int Count => _count.Value;

        public EntityMap(int capacity)
        {
            _entityCapacity = capacity;
            _idCapacity = capacity * 2;
            _idToIndex = new NativeArray<int>(_idCapacity, Allocator.Persistent);
            _indexToId = new NativeArray<EntityID>(_entityCapacity, Allocator.Persistent);
            _freeIds = new NativeList<EntityID>(_entityCapacity, Allocator.Persistent);
            _count = new NativeReference<int>(0, Allocator.Persistent);
            _nextId = new EntityID(-1);
        }

        public int GetIndex(EntityID id)
        {
            if (id.Value < 0 || id.Value >= _idCapacity) return -1;
            return _idToIndex[id.Value];
        }

        public EntityID GetID(int index)
        {
            if (index < 0 || index >= _count.Value) return new EntityID(-1);
            return _indexToId[index];
        }

        public (EntityID id, int arrayIndex) Add()
        {
            if (_count.Value >= _entityCapacity) return (new EntityID(-1),-1);
            
            EntityID id;
            if (_freeIds.Length > 0)
            {
                id = _freeIds[^1];
                _freeIds.RemoveAt(_freeIds.Length - 1);
            }
            else
            {
                
                _nextId++;
                id = _nextId;
                if (id.Value >= _idCapacity) {
                    _nextId--;
                    return (new EntityID(-1),-1);
                }
            }
            
            
            int arrayIndex = _count.Value;
            _idToIndex[id.Value] = arrayIndex;
            _indexToId[arrayIndex] = id;
            _count.Value++;

            return (id, arrayIndex);
        }

        public (int removedArrayIndex, int movedFromArrayIndex) Remove(EntityID id)
        {
            if (_count.Value == 0) return (-1, -1);
            int removedIndex = _idToIndex[id.Value];
            int lastIndex = _count.Value - 1;
            int movedFrom = -1;

            if (removedIndex != lastIndex)
            {
                EntityID lastId = _indexToId[lastIndex];
                _indexToId[removedIndex] = lastId;
                _idToIndex[lastId.Value] = removedIndex;
                movedFrom = lastIndex;
            }

            _idToIndex[id.Value] = -1;
            _indexToId[lastIndex] = default;
            _count.Value--;

            return (removedIndex, movedFrom);
        }

        public void RemoveEntitiesBunch(NativeArray<DeathRequest> DeathRequests, NativeArray<RemovedEntity> RemovedEntities)
        {
            EntityMapExtensions.RemoveEntiiesFromMap(ref DeathRequests, ref _idToIndex, ref _indexToId, ref _count, ref RemovedEntities);
        }
        
        
        public void RecycleID(EntityID id)
        {
            _freeIds.Add(id);
        }

        public JobView AsJobView() => new JobView(_idToIndex, _indexToId, _count.Value);
        public NativeList<EntityID> GetFreeIds() => _freeIds;
        public void Dispose()
        {
            if (_idToIndex.IsCreated) _idToIndex.Dispose();
            if (_indexToId.IsCreated) _indexToId.Dispose();
            if (_freeIds.IsCreated) _freeIds.Dispose();
            if (_count.IsCreated) _count.Dispose();
        }
        
        public struct JobView
        {
            [ReadOnly] private NativeArray<int> _idToIndex;
            [ReadOnly] private NativeArray<EntityID> _indexToId;
            private readonly int _count;

            internal JobView(
                NativeArray<int> idToIndex,
                NativeArray<EntityID> indexToId,
                int count)
            {
                _idToIndex = idToIndex;
                _indexToId = indexToId;
                _count = count;
            }

            public int Count => _count;

            public int GetIndex(EntityID id)
            {
                if (id.Value < 0 || id.Value >= _idToIndex.Length) return -1;
                return _idToIndex[id.Value];
            }

            public EntityID GetID(int index)
            {
                if (index < 0 || index >= _count) return new EntityID(-1);
                return _indexToId[index];
            }
        }
    }
}