using Combat;
using UnityEngine;

public abstract class InterceptorDefinitionSO : ScriptableObject
{
    public abstract void Register(EntityID entityID, CombatWorld combatWorld);
}