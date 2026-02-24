using System.Runtime.InteropServices;

namespace Combat
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InterceptorData 
    {
        [FieldOffset(0)] public float FloatValue1;
        [FieldOffset(0)] public int IntValue1;
        [FieldOffset(4)] public float FloatValue2;
        [FieldOffset(4)] public int IntValue2;
        [FieldOffset(8)] public float FloatValue3;
        [FieldOffset(8)] public int IntValue3;
        
        [FieldOffset(12)] public DamageType DamageType;
        [FieldOffset(12)] public StatChangeType StatChangeType;
        
        [FieldOffset(16)] public StatTarget StatTarget;
        [FieldOffset(16)] public ResourceType ResourceType;

        
    }
}