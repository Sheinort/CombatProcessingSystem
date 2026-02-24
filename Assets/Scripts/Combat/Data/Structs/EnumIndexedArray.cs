using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Combat
{
    public struct EnumIndexedArray<TEnum, TData> : IDisposable
        where TEnum : unmanaged, Enum
        where TData : unmanaged
    {
        [ReadOnly] private readonly int _entityCapacity;
        [ReadOnly] private readonly int _enumCount;
        public int EnumCount => _enumCount;
        private NativeArray<TData> _data;

        public EnumIndexedArray(int entityCapacity, Allocator allocator)
        {
            _entityCapacity = entityCapacity;
            _enumCount = Enum.GetValues(typeof(TEnum)).Length;
            _data = new NativeArray<TData>(_enumCount * _entityCapacity, allocator);
        }

        public NativeSlice<TData> this[TEnum key]
        {
            get
            {
                int offset = UnsafeUtility.As<TEnum, byte>(ref key) * _entityCapacity;
                return _data.GetSubArray(offset, _entityCapacity);
            }
        }

        public unsafe ref TData GetRef(TEnum key, int entityIndex)
        {
            int offset = UnsafeUtility.As<TEnum, byte>(ref key) * _entityCapacity + entityIndex;
            return ref UnsafeUtility.ArrayElementAsRef<TData>(_data.GetUnsafePtr(), offset);
        }

        public void SwapBack(int removedIndex, int movedFromIndex)
        {
            for (int i = 0; i < _enumCount; i++)
            {
                int baseIdx = i * _entityCapacity;
                _data[baseIdx + removedIndex] = _data[baseIdx + movedFromIndex];
            }
        }

        public void ClearSlot(int index)
        {
            if (index < 0 || index >= _entityCapacity) return;
            for (int i = 0; i < _enumCount; i++)
            {
                _data[(i * _entityCapacity) + index] = default;
            }
        }

        public void Dispose()
        {
            if (_data.IsCreated) _data.Dispose();
        }

        private static int UnsafeEnumToInt(TEnum value)
            => UnsafeUtility.As<TEnum, int>(ref value);

        public bool IsCreated => _data.IsCreated;
    }
}