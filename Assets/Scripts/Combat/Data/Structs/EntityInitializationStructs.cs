using Unity.Collections;

namespace Combat
{
    public struct PendingEntityInit
    {
        public int ArrayIndex;
        public int TypeIndex;
    }
    
    public struct EntityTypeData
    {
        public FixedString64Bytes Name;
        public FixedList32Bytes<float> ResourceStats;
        public FixedList32Bytes<float> CombatStats;
        public FixedList32Bytes<float> ResistanceStats;
        public FixedList32Bytes<float> StartingResources;
        public DeathFlags DeathFlags;
    }
}