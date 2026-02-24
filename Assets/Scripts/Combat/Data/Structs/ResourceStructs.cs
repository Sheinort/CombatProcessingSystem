using System;
using System.Runtime.InteropServices;


namespace Combat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Resource
    {
        public float Value;
        public float MaxValue;
    }
    [Serializable]
    public struct ResourceChangeRequest : IComparable<ResourceChangeRequest>
    {
        public EntityID OriginID;
        public EntityID TargetID;
        public int TargetEntityArrayIndex;
        public float Value;
        public OrderOfModification OrderOfModification;
        public ResourceChangeType ChangeType;
        public ResourceType ChangeTypeTarget;
        public DamageType DamageType;
        public ResourceChangeFlags DamageFlags;
        public int CompareTo(ResourceChangeRequest other)
        {
            return TargetID.Value.CompareTo(TargetID.Value);
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ResourceChangeOverTime
    {
        public float FrequencyPerSecond;
        public float TimeSinceApplied;
        public float Duration;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct OrderOfModification
    {
        private uint _packedData;
        private byte _count;
        public byte Count => _count;

        public void Add(ResourceType resourceType)
        {
            if (_count >= 4) return;
            _packedData |= (uint)resourceType << (_count * 8);
            _count++;
        }

        public ResourceType Get(int index)
            => (ResourceType)((_packedData >> (index * 8)) & 0xFF);
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ResourceChangeResult
    {
        public struct ResourceEntry
        {
            public ResourceType Type;
            public float From;
            public float To;
        }

        public EntityID OriginID;
        public EntityID TargetID;
        public int TargetIndex;
        private ResourceEntry _r0, _r1, _r2;
        private byte _count;
        public byte Count => _count;

        public void AddResult(ResourceType resourceType, float from, float to)
        {
            if (_count >= 3) return;
            var entry = new ResourceEntry { Type = resourceType, From = from, To = to };
            if (_count == 0) _r0 = entry;
            else if (_count == 1) _r1 = entry;
            else _r2 = entry;
            _count++;
        }

        public ResourceEntry GetResult(int index) => index switch
        {
            0 => _r0,
            1 => _r1,
            2 => _r2,
            _ => default
        };
    }
    
    [Flags]
    public enum DeathFlags : byte
    {
        None        = 0,
        LeavesCorpse = 1 << 0,
        WasCritical  = 1 << 1,
        WasExecuted  = 1 << 2,
    }
}