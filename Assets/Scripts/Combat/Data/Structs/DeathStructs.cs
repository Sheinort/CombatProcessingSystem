using UnityEngine;
using System.Runtime.InteropServices;

namespace Combat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DeadEntity
    {
        public EntityID ID;
        public int OriginalArrayIndex;
        public float MaxHealth;
        public float MaxArmor;
        public Vector3 Position;
        public Quaternion Rotation;
        public float TimeOfDeath;
    }

    public struct DeathRequest
    {
        public EntityID EntityID;
        public int ArrayIndex;
        public DeathFlags Flags;
    }

    public struct RemovedEntity
    {
        public EntityID ID;
        public int RemovedIndex;
        public int MovedFrom;
    }
}