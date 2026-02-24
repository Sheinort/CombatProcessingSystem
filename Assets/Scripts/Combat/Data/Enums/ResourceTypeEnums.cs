namespace Combat
{
    public enum ResourceType : byte
    {
        Health,
        Armor,
        PrimaryResource,
        SecondaryResource
    }

    public enum ResourceChangeType : byte
    {
        Flat,
        FractionOfMax,
        FractionOfCurrent
    }

    [System.Flags]
    public enum  ResourceChangeFlags : byte
    {
        IsReflectionChange = 1 << 0,
        BypassReductions   = 1 << 1,
        InstantKill        = 1 << 2
    }
}