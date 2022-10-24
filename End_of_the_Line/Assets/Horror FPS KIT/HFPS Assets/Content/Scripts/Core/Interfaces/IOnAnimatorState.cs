using UnityEngine;

namespace HFPS.Systems
{
    public interface IOnAnimatorState
    {
        void OnStateEnter(AnimatorStateInfo state, string name);
    }
}