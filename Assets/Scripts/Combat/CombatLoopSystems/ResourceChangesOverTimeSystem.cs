using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Process change over time effects, updates their timers, and deletes when duration runs out.
    /// </summary>
    public static class ResourceChangesOverTimeSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var statusEffectRegistry = combatWorld.OverTimeEffectsRegistry;
            var behaviour = statusEffectRegistry.ResourceChangeOverTime;
            var data = statusEffectRegistry.ResourceChangeOverTimeRequests;
            for (int i = behaviour.Length-1; i >= 0; i--)
            {
                var timer = behaviour[i];
                timer.Duration -= Time.deltaTime;
                timer.TimeSinceApplied += Time.deltaTime;
                if (timer.TimeSinceApplied >= 1f/timer.FrequencyPerSecond)
                {
                    combatWorld.ActionRegister.ResourceChangeRequests.Add(data[i]);
                    timer.TimeSinceApplied -= 1f/timer.FrequencyPerSecond;
                }
                behaviour[i] = timer;
                if (timer.Duration <= 0)
                    statusEffectRegistry.RemoveResourceChangeOverTime(i);
            }
            
        }

    }

}