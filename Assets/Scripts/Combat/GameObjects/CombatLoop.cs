using UnityEngine;
using Combat;

/// <summary>
/// Initializes a combat world and updates all systems through Combat Commands.
/// </summary>
public class CombatLoop: MonoBehaviour
{
    public CombatWorld combatWorld;
    
    private void Awake()
    {
        var definitions = Resources.LoadAll<EntityDefinitionSO>("ScriptableObjects");
        combatWorld = new CombatWorld(10001,10001, definitions);
    }

    private void Update()
    {
        CombatCommands.CombatLoop(combatWorld);
    }

    private void OnDestroy()
    {
        combatWorld?.Dispose();
    }
}
