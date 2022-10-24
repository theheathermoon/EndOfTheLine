/*
 * NPCBodyPart.cs - by ThunderWire Studio
 * ver. 1.0
*/

using UnityEngine;

namespace HFPS.Systems
{
    /// <summary>
    /// NPC Damage Caller (Sends Damage Event to Main Health script)
    /// </summary>
    public class NPCBodyPart : DamageBehaviour
    {
        [HideInInspector]
        public NPCHealth health;

        public bool isHead;

        public override void ApplyDamage(int damageAmount)
        {
            health.Damage(isHead ? health.headshotDamage : damageAmount);
        }

        public override bool IsAlive()
        {
            return health.Health > 0;
        }
    }
}