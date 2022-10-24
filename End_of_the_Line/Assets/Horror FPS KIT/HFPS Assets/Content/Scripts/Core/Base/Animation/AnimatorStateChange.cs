using System.Linq;
using UnityEngine;

namespace HFPS.Systems
{
    public class AnimatorStateChange : StateMachineBehaviour
    {
        public string Name = "";

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            IOnAnimatorState[] states = GetStates(animator.gameObject) ?? GetStates(animator.transform.parent.gameObject);

            if(states.Length > 0)
            {
                foreach (var state in states)
                {
                    state.OnStateEnter(stateInfo, Name);
                }
            }

            /*
            if (scripts.Length <= 0)
            {
                scripts = GetStates(animator.transform.parent.gameObject);
            }

            foreach (var istate in scripts)
            {
                istate.OnStateEnter(stateInfo, Name);
            }
            */
        }

        IOnAnimatorState[] GetStates(GameObject obj)
        {
            var states = (from script in obj.GetComponents<MonoBehaviour>()
                        where typeof(IOnAnimatorState).IsAssignableFrom(script.GetType())
                        select script as IOnAnimatorState).ToArray();

            return states.Length > 0 ? states : null;
        }
    }
}