using UnityEngine;

namespace HFPS.Systems 
{
    public class NextLevelTrigger : MonoBehaviour
    {
        private HFPS_GameManager gameManager;

        public string nextLevelName;
        public bool simpleChange = false;
        public bool triggerChange = false;

        private bool isChanging;

        private void Awake()
        {
            if (HFPS_GameManager.HasReference)
            {
                gameManager = HFPS_GameManager.Instance;
            }
            else
            {
                throw new System.NullReferenceException("Could not find HFPS_GameManager reference!");
            }
        }

        public void UseObject()
        {
            if (!isChanging)
            {
                ChangeScene();
                isChanging = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isChanging && triggerChange)
            {
                ChangeScene();
                isChanging = true;
            }
        }

        void ChangeScene()
        {
            if (gameManager)
            {
                if (!simpleChange)
                {
                    gameManager.LoadNextScene(nextLevelName);
                }
                else
                {
                    gameManager.ChangeScene(nextLevelName);
                }
            }
            else
            {
                Debug.LogError("[NextLevelTrigger] GameManager script does not found!");
            }
        }
    }
}