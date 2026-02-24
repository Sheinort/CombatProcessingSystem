using System;
using System.Linq;
using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestDamageScript : MonoBehaviour
{
    public CombatLoop combatLoop;
    public CombatEntityAuthoring targetEntity;
    public TMP_Text healthText;
    private CombatWorld _combatWorld;

    private void Start()
    {
        _combatWorld = combatLoop.combatWorld;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            DealDamage();
        }

        var index = _combatWorld.EntityMap.GetIndex(targetEntity.ID);
        if (index == -1) {
            healthText.text = "Dead";
            return;
        };
        var res = _combatWorld.StatRegistry.Resources[ResourceType.Health][index];
        var shieldInterceptor = CombatCommands.GetInterceptor<ShieldAbsorbInterceptor>(_combatWorld);
        
        // Display method for demonstration and debug purposes. Intended use is to have a UI system that maps displays method
        // to specific Interceptors and updates by iterating over them. 
        if (shieldInterceptor == null) return;
        var shields = shieldInterceptor.GetInterceptorData(targetEntity.ID);
        var shieldValue = 0f;
        foreach (var shield in shields) {
            shieldValue += shield.FloatValue1;
        }
        healthText.text = res.Value + "/" + res.MaxValue;
        if (shields.Length > 0) {
            healthText.text += " + (" + shieldValue + ")";
        }
    }

    private void DealDamage()
    {
        Debug.Log("Damage!");
        var o = new OrderOfModification();
        o.Add(ResourceType.Health);
        _combatWorld.ActionRegister.ResourceChangeRequests.Add(new ResourceChangeRequest {
            ChangeType = ResourceChangeType.Flat,
            OriginID = targetEntity.ID,
            TargetID = targetEntity.ID,
            Value = -1,
            DamageType = DamageType.True,
            OrderOfModification = o
        });
    }
}
