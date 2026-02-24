using System;
using Unity.Collections;
using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(menuName = "Combat/Entity Definition")]
    public sealed class EntityDefinitionSO : ScriptableObject
    {
        public FixedString64Bytes Name;
        public bool LeavesCorpse;

        [EnumNamedArray(typeof(ResourceStatType))]
        public float[] ResourceStats = new float[EnumLength<ResourceStatType>()];

        [EnumNamedArray(typeof(CombatStatType))]
        public float[] CombatStats = new float[EnumLength<CombatStatType>()];

        [EnumNamedArray(typeof(ResistanceStatType))]
        public float[] ResistanceStats = new float[EnumLength<ResistanceStatType>()];

        [EnumNamedArray(typeof(ResourceType))]
        public float[] StartingResources = new float[EnumLength<ResourceType>()];

        [HideInInspector] public int ID;

        private void OnValidate()
        {
            EnsureSize(ref ResourceStats,    EnumLength<ResourceStatType>());
            EnsureSize(ref CombatStats,      EnumLength<CombatStatType>());
            EnsureSize(ref ResistanceStats,  EnumLength<ResistanceStatType>());
            EnsureSize(ref StartingResources, EnumLength<ResourceType>());
        }

        private static int EnumLength<T>() where T : Enum
            => Enum.GetValues(typeof(T)).Length;

        private static void EnsureSize<T>(ref T[] array, int size)
        {
            if (array == null || array.Length != size)
                Array.Resize(ref array, size);
        }
    }
}