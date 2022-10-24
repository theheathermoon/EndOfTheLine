using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HFPS.Systems
{
    public class BreakableObject : DamageBehaviour, ISaveable
    {
        private SaveGameHandler saveGameHandler;

        [Header("Object")]
        public ObjectReference[] ObjectsInside;
        [Space(5)]
        public Transform Center;
        public bool SpawnObjectInside;
        public bool RandomObject;
        public bool AddFloatingIcon;

        [Header("Object Health")]
        public GameObject BrokenPrefab;
        public float ObjectHealth;
        [Space(5)]
        public bool DestroyPieces;
        [MinMax(10, 40)]
        public Vector2Int PiecesKeepRange = new Vector2Int(10, 20);

        [Header("Rotations")]
        public Vector3 BrokenRotation;
        public Vector3 SpawnedRotation;

        [Header("Pieces Explosion")]
        public bool ExplosionEffect;
        public float UpwardsModifer = 1.5f;
        public float ExplosionPower = 200;
        public float ExplosionRadius = 0.5f;

        [Header("Sounds")]
        public AudioClip BreakSound;
        public float BreakVolume = 1f;

        private bool isBroken;

        void Awake()
        {
            saveGameHandler = SaveGameHandler.Instance;
        }

        public override void ApplyDamage(int damageAmount)
        {
            if (isBroken) return;

            ObjectHealth -= damageAmount;

            if (ObjectHealth <= 0)
            {
                BreakObject();
                ObjectHealth = 0;
                isBroken = true;
            }
        }

        public override bool IsAlive()
        {
            return !isBroken;
        }

        void BreakObject()
        {
            if (!BrokenPrefab)
            {
                Debug.LogError("[Breakable Object] Broken Prefab is missing!");
                return;
            }

            gameObject.SetActive(false);

            Vector3 rotation = transform.eulerAngles + BrokenRotation;
            GameObject brokenObj = Instantiate(BrokenPrefab, transform.position, Quaternion.Euler(rotation));
            int destroyTime = 0;

            if (SpawnObjectInside && ObjectsInside.Length > 0)
            {
                int random = RandomObject ? Random.Range(0, ObjectsInside.Length) : 0;
                GameObject go = saveGameHandler.InstantiateSaveableReference(ObjectsInside[random], Center.position, SpawnedRotation);
                FloatingIconManager.Instance.FloatingIcons.Add(go);
            }

            foreach (Collider piece in brokenObj.GetComponentsInChildren<Collider>())
            {
                if (DestroyPieces)
                {
                    int random = Random.Range(PiecesKeepRange.x, PiecesKeepRange.y);
                    piece.gameObject.AddComponent<DestroyAfterCall>().DestroyObject(random);

                    if (random > destroyTime)
                    {
                        destroyTime = random;
                    }
                }

                if (ExplosionEffect && Center)
                {
                    Rigidbody rb = piece.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        rb.AddExplosionForce(ExplosionPower, Center.position, ExplosionRadius, UpwardsModifer);
                    }
                }
            }

            if (DestroyPieces)
                brokenObj.AddComponent<DestroyAfterCall>().DestroyObject(destroyTime);

            if (BreakSound)
                AudioSource.PlayClipAtPoint(BreakSound, transform.position, BreakVolume);
        }

        void OnDrawGizmosSelected()
        {
            if (ExplosionEffect && Center)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Center.position, ExplosionRadius);
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                {"breakable_health", ObjectHealth},
                {"is_broken", isBroken}
            };
        }

        public void OnLoad(JToken token)
        {
            ObjectHealth = (float)token["breakable_health"];

            if (isBroken = (bool)token["is_broken"])
            {
                gameObject.SetActive(false);
            }
        }
    }
}