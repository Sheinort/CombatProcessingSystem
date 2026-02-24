using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Combat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Stat
    {
        public float BaseValue;
        public float FlatBonus;
        public float MultiplierMultiplicative;
        public float MultiplierAdditive;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stat Create(float baseValue) => new()
        {
            BaseValue = baseValue,
            FlatBonus = 0f,
            MultiplierMultiplicative = 1f,
            MultiplierAdditive = 0f
        };
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct StatChangeRequest : IComparable<StatChangeRequest>
    {
        public EntityID OriginID;
        public EntityID TargetID;
        public int TargetEntityArrayIndex;
        public float Value;
        public StatTarget Target;
        public StatChangeType ChangeType;

        [BurstCompile]
        public void Invert()
        {
            if (ChangeType == StatChangeType.MultiplierMultiplicative)
                Value = 1 / Value;
            else
                Value *= -1;
        }

        public int CompareTo(StatChangeRequest other)
        {
            return TargetID.Value.CompareTo(other.TargetID.Value);
        }
    }
    public readonly struct StatTarget
    {
        public readonly StatCategory Category;
        public readonly byte SubIndex;

        public StatTarget(CombatStatType stat)     { Category = StatCategory.CombatStat; SubIndex = (byte)stat; }
        public StatTarget(ResourceStatType stat)   { Category = StatCategory.Resource;   SubIndex = (byte)stat; }
        public StatTarget(ResistanceStatType stat) { Category = StatCategory.Resistance; SubIndex = (byte)stat; }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct StatChangeResult
    {
        public EntityID TargetID;
        public StatTarget Target;
        public int TargetArrayIndex;
        public float Delta;
    }
}