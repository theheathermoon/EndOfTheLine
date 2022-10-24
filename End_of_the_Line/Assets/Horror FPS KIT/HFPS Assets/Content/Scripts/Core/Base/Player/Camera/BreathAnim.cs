using UnityEngine;

namespace HFPS.Player
{
    public class BreathAnim : MonoBehaviour
    {
        public Animation anim;
        public string breathAnim = "Breath";
        public string idleAnim = "BreathIdle";

        void Update()
        {
            if (!Input.GetButton("Fire2"))
                anim.Play(breathAnim);
            else
                anim.CrossFade(idleAnim);
        }
    }
}