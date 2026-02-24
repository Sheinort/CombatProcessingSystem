using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Combat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Resist
    {
        public float Value;
        public float CalculatedFraction;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CombatStatBlock
    {
        public float AttackPower;
        public float Haste;
        public float CritChance;
        public float CritMultiplier;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStat(CombatStatType type, float value, out float delta)
        {
            delta = value;
            switch (type)
            {
                case CombatStatType.AttackPower:
                    delta -= AttackPower;
                    AttackPower = value;
                    break;
                case CombatStatType.Haste:
                    delta -= Haste;
                    Haste = value;
                    break;
                case CombatStatType.CritChance:
                    delta -= CritChance;
                    CritChance = value;
                    break;
                case CombatStatType.CritMultiplier:
                    delta -= CritMultiplier;
                    CritMultiplier = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}