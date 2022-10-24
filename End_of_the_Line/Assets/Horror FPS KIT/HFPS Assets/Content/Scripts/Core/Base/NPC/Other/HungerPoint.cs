using UnityEngine;

namespace HFPS.Systems
{
    public class HungerPoint : MonoBehaviour
    {
        [MinMax(10, 320)]
        public Vector2 hungerPoints = new Vector2(60, 120);

        [MinMax(1, 100)]
        public Vector2Int healthRecover = new Vector2Int(20, 40);


        public HungerPoints GetHungerPoints()
        {
            return new HungerPoints()
            {
                hungerPoints = Random.Range(hungerPoints.x, hungerPoints.y),
                healthPoints = Random.Range(healthRecover.x, healthRecover.y)
            };
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.05f);
        }

        public struct HungerPoints
        {
            public float hungerPoints;
            public int healthPoints;
        }
    }
}