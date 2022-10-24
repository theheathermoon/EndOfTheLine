using UnityEngine.InputSystem;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem.Editor;
using UnityEditor;
#endif

namespace ThunderWire.Input.Interactions
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class NotRebindableInteraction : IInputInteraction
    {
        private bool m_WaitingForRelease;

        public void Process(ref InputInteractionContext context)
        {
            var isActuated = context.ControlIsActuated(InputSystem.settings.defaultButtonPressPoint);

            if (m_WaitingForRelease)
            {
                if (!isActuated)
                {
                    m_WaitingForRelease = false;
                    context.Canceled();
                }
            }
            else if (isActuated)
            {
                m_WaitingForRelease = true;
                context.PerformedAndStayPerformed();
            }
        }

        public void Reset()
        {
            
        }

        static NotRebindableInteraction()
        {
            InputSystem.RegisterInteraction<NotRebindableInteraction>();
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            
        }
    }

#if UNITY_EDITOR
    internal class NotRebindableInteractionEditor : InputParameterEditor<NotRebindableInteraction>
    {
        public override void OnGUI()
        {
            EditorGUILayout.LabelField(EditorGUIUtility.TrTextContentWithIcon("This is just a tag that determines that the action is not rebindable.", "console.infoicon.sml"), EditorStyles.helpBox);
        }
    }
#endif
}