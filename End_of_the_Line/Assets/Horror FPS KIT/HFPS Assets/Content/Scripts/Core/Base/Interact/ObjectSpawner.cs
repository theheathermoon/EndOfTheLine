using UnityEngine;

namespace HFPS.Systems
{
    public class ObjectSpawner : MonoBehaviour
    {
        private SaveGameHandler saveGameHandler;

        public enum SpawnType { OnStart, ByTrigger, ByEvent }

        public SpawnType spawnType = SpawnType.OnStart;
        public ObjectReference[] SpawnObjects;
        public Transform SpawnPosition;
        public bool spawnOnce = true;

        [HideInInspector, SaveableField]
        public bool isSpawned;

        private Transform spawnPoint;

        private void Awake()
        {
            saveGameHandler = SaveGameHandler.Instance;
        }

        void Start()
        {
            spawnPoint = SpawnPosition != null ? SpawnPosition : transform;
            Invoke(nameof(DelayedStart), 1f);
        }

        void DelayedStart()
        {
            if (!isSpawned && SpawnObjects.Length > 0 && spawnType == SpawnType.OnStart)
            {
                InstantiateSaveable();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isSpawned && SpawnObjects.Length > 0 && spawnType == SpawnType.ByTrigger)
            {
                InstantiateSaveable();
            }
        }

        public void Spawn()
        {
            if (!isSpawned && SpawnObjects.Length > 0 && spawnType == SpawnType.ByEvent)
            {
                InstantiateSaveable();
            }
        }

        void InstantiateSaveable()
        {
            GameObject go = saveGameHandler.InstantiateSaveableReference(SpawnObjects[Random.Range(0, SpawnObjects.Length)], spawnPoint.position, spawnPoint.eulerAngles);

            if (go.GetComponentsInChildren<InteractiveItem>(true).Length > 0)
            {
                foreach (var item in go.GetComponentsInChildren<InteractiveItem>(true))
                {
                    item.disableType = InteractiveItem.DisableType.Destroy;
                }
            }

            isSpawned = spawnOnce;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SpawnPosition.position, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(SpawnPosition.position, SpawnPosition.forward * 1f);
        }
    }
}