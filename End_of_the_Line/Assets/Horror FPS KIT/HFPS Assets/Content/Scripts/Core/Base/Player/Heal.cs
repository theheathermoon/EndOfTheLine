using UnityEngine;
using ThunderWire.Utility;

namespace HFPS.Player
{
    public class Heal : MonoBehaviour
    {
        public float HealAmout;
        public AudioClip HealSound;
        public float HealVolume = 1f;

        public void UseObject()
        {
            if (PlayerController.HasReference)
            {
                if (PlayerController.Instance.gameObject.HasComponent(out HealthManager health))
                {
                    health.ApplyHeal(HealAmout);

                    if (!health.isMaximum)
                    {
                        if (HealSound)
                        {
                            AudioSource.PlayClipAtPoint(HealSound, transform.position, HealVolume);
                        }

                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}