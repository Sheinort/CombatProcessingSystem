namespace Combat
{
    public enum StatCategory : byte
    {
        Resource,
        CombatStat,
        Resistance
    }

    public enum ResourceStatType : byte
    {
        MaxHealth,
        MaxArmor,
        MaxPrimaryResource,
        MaxSecondaryResource
    }

    public enum CombatStatType : byte
    {
        AttackPower,
        Haste,
        CritChance,
        CritMultiplier
    }

    public enum ResistanceStatType : byte
    {
        Physical,
        Magical,
        True
    }

    public enum StatChangeType : byte
    {
        Flat,
        MultiplierMultiplicative,
        MultiplierAdditive
    }
}