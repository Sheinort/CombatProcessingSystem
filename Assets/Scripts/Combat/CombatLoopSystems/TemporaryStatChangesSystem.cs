using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Controls the lifetime of temporary stat changes.
    /// </summary>
    public static class TemporaryStatChangesSystem
    {
        public static void Update(CombatWorld combatWorld)
        {
            var timers = combatWorld.OverTimeEffectsRegistry.TemporaryStatBuffsTimers;
            for (int i = timers.Length-1; i >= 0; i--)
            {
                timers[i]-= Time.deltaTime;
                if (timers[i] <= 0)
                    CombatCommands.RemoveTemporaryStatChange(combatWorld, i);
            }
        }
    }
}