using System;
using Unity.Collections;

namespace Combat
{
    /// <summary>
    /// Holds least of all possible entity definitions.
    /// When a new combat entity spawns, it uses index into EntityTypes to get Base values for stats and resources.
    /// </summary>
    public sealed class EntityTypeRegistry : IDisposable
    {
        public NativeArray<EntityTypeData> EntityTypes { get; private set; }

        public void Build(EntityDefinitionSO[] definitions)
        {
            EntityTypes = new NativeArray<EntityTypeData>(definitions.Length, Allocator.Persistent);
            for (int i = 0; i < definitions.Length; i++)
            {
                var entityTypes = EntityTypes;
                entityTypes[i] = Compile(definitions[i]);
                definitions[i].ID = i;
            }
        }

        private static EntityTypeData Compile(EntityDefinitionSO so)
        {
            DeathFlags flags = DeathFlags.None;
            if (so.LeavesCorpse)
                flags |= DeathFlags.LeavesCorpse;

            return new EntityTypeData
            {
                Name             = so.Name,
                ResourceStats    = ToFixedList(so.ResourceStats),
                CombatStats      = ToFixedList(so.CombatStats),
                ResistanceStats  = ToFixedList(so.ResistanceStats),
                StartingResources = ToFixedList(so.StartingResources),
                DeathFlags       = flags
            };
        }

        private static FixedList32Bytes<float> ToFixedList(float[] source)
        {
            var list = new FixedList32Bytes<float>();
            for (int i = 0; i < source.Length; i++)
                list.Add(source[i]);
            return list;
        }

        public void Dispose()
        {
            if (EntityTypes.IsCreated) EntityTypes.Dispose();
        }
    }
}