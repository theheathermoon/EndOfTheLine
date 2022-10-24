using UnityEngine;

namespace ThunderWire.Helpers
{
    /// <summary>
    /// Class to get a correct DeltaTime
    /// </summary>
    public class Timekeeper
    {
        private float prevRealTime;
        private float thisRealTime;

        public void UpdateTime()
        {
            prevRealTime = thisRealTime;
            thisRealTime = Time.realtimeSinceStartup;
        }

        public float deltaTime
        {
            get
            {
                if (Time.timeScale > 0f)
                {
                    return Time.deltaTime / Time.timeScale;
                }
                else
                {
                    return Time.realtimeSinceStartup - prevRealTime;
                }
            }
        }
    }
}