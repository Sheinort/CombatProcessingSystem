using System;

namespace Combat
{
    public readonly struct EntityID : IEquatable<EntityID>, IComparable<EntityID>
    {
        public EntityID(int value) { Value = value; }
        public int Value { get; }
        public static explicit operator EntityID(int value) => new(value);
        public static explicit operator int(EntityID id) => id.Value;
        public static EntityID operator ++(EntityID id) => new(id.Value + 1);
        public static EntityID operator --(EntityID id) => new(id.Value -  1);
        public bool Equals(EntityID other) => Value == other.Value;
        public override bool Equals(object obj) => obj is EntityID other && Equals(other);
        public static bool operator ==(EntityID left, EntityID right) => left.Value == right.Value;
        public static bool operator !=(EntityID left, EntityID right) => left.Value != right.Value;
        public override int GetHashCode() => Value;
        public override string ToString() => Value.ToString();

        public int CompareTo(EntityID other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}