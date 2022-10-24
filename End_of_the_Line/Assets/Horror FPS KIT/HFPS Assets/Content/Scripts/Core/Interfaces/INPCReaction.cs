using UnityEngine;

namespace HFPS.Systems
{
    public interface INPCReaction
    {
        void HitReaction();
        void SoundReaction(Vector3 pos, bool closeSound);
    }
}