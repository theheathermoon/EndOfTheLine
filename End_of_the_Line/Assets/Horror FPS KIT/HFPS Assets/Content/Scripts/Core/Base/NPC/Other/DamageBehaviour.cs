using UnityEngine;

namespace HFPS.Systems
{
    public abstract class DamageBehaviour : MonoBehaviour
    {
        public abstract void ApplyDamage(int damageAmount);
        public abstract bool IsAlive();
    }
}