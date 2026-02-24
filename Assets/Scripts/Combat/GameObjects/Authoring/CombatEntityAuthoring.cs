using Combat;
using UnityEngine;

/// <summary>
/// Initializes entity into Combat World.
/// </summary>
public class CombatEntityAuthoring: MonoBehaviour
{
    [SerializeField] private CombatLoop combatLoop;
    [SerializeField] private EntityDefinitionSO defenition;
    [SerializeField] private InterceptorDefinitionSO[] interceptors;
    [HideInInspector]
    public EntityID ID;

    private void Start()
    {
        ID = CombatCommands.CreateEntity(combatLoop.combatWorld, defenition.ID);
        foreach (var defenitionSo in interceptors) {
            defenitionSo.Register(ID, combatLoop.combatWorld);
        }
    }
}