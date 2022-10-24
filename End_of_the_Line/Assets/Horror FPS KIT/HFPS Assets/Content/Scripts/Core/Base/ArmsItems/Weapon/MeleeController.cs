/*
 * MeleeController.cs - by ThunderWire Studio
 * version 1.0
*/

using System.Collections;
using UnityEngine;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    public class MeleeController : SwitcherBehaviour
    {
        private ScriptManager scriptManager;
        private PlayerController playerControl;
        private Inventory Inventory;
        private Animation anim;
        private Camera cam;

        [Header("Inventory")]
        [InventorySelector]
        public int MeleeItemID;

        [Header("Meleemarks")]
        public SurfaceID surfaceID = SurfaceID.Texture;
        public SurfaceDetailsScriptable surfaceDetails;
        public int defaultSurfaceID;

        [Header("Main")]
        public LayerMask HitLayer;
        public float HitDistance;
        public float HitForce;
        public float HitWaitDelay;
        public Vector2Int AttackDamage;

        [Header("Kickback")]
        public Vector3 SwayKickback;
        public float SwaySpeed = 0.1f;

        [Header("Animation")]
        public GameObject MeleeGO;
        public string DrawAnim;
        [Range(0, 5)] public float DrawSpeed = 1f;
        public string HideAnim;
        [Range(0, 5)] public float HideSpeed = 1f;
        public string AttackAnim;
        [Range(0, 5)] public float AttackSpeed = 1f;

        [Header("Sounds")]
        public AudioSource audioSource;

        [Space(5)]
        public AudioClip DrawSound;
        [Range(0, 1)] public float DrawVolume = 1f;

        [Space(5)]
        public AudioClip HideSound;
        [Range(0, 1)] public float HideVolume = 1f;

        [Space(5)]
        public AudioClip SwaySound;
        [Range(0, 1)] public float SwayVolume = 1f;

        private bool AttackKey;

        private bool isHideAnim;
        private bool isSelected;
        private bool isBlocked;
        private bool wallHit;

        private bool inputWait;
        private float waitTime;

        void Awake()
        {
            anim = MeleeGO.GetComponent<Animation>();
            scriptManager = transform.root.GetComponentInChildren<ScriptManager>();
            playerControl = transform.root.GetComponent<PlayerController>();
        }

        void Start()
        {
            Inventory = Inventory.Instance;
            cam = ScriptManager.Instance.MainCamera;

            anim[DrawAnim].speed = DrawSpeed;
            anim[HideAnim].speed = HideSpeed;
            anim[AttackAnim].speed = AttackSpeed;
        }

        public override void OnSwitcherSelect()
        {
            if (anim.isPlaying || isSelected) return;

            MeleeGO.SetActive(true);
            anim.Play(DrawAnim);
            PlaySound(DrawSound, DrawVolume);

            StartCoroutine(SelectCoroutine());
        }

        public override void OnSwitcherDeselect()
        {
            if (anim.isPlaying || !isSelected) return;

            isHideAnim = true;

            anim.Play(HideAnim);
            PlaySound(HideSound, HideVolume);
            StartCoroutine(DeselectCoroutine());
        }

        public override void OnSwitcherActivate()
        {
            MeleeGO.SetActive(true);
            isSelected = true;
        }

        public override void OnSwitcherDeactivate()
        {
            isSelected = false;
            MeleeGO.SetActive(false);
        }

        public override void OnSwitcherWallHit(bool hit)
        {
            wallHit = hit;
        }

        public override void OnSwitcherDisable(bool enabled)
        {
            isBlocked = enabled;
        }

        IEnumerator SelectCoroutine()
        {
            yield return new WaitUntil(() => !anim.isPlaying);
            isSelected = true;
        }

        IEnumerator DeselectCoroutine()
        {
            yield return new WaitUntil(() => !anim.isPlaying);
            MeleeGO.SetActive(false);
            isSelected = false;
            isHideAnim = false;
        }

        void PlaySound(AudioClip clip, float volume)
        {
            if (clip)
            {
                audioSource.volume = volume;
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        void Update()
        {
            if (!scriptManager.ScriptGlobalState)
            {
                waitTime = 0.5f;
                inputWait = true;
            }
            else if (inputWait && waitTime > 0)
            {
                waitTime -= Time.deltaTime;
            }
            else
            {
                inputWait = false;
            }

            if (InputHandler.InputIsInitialized && !inputWait)
            {
                AttackKey = InputHandler.ReadButton("Fire");
            }

            if (Inventory.CheckItemInventory(MeleeItemID) && scriptManager.ScriptGlobalState && isSelected && !wallHit && !isBlocked && !isHideAnim)
            {
                if (AttackKey && !anim.isPlaying)
                {
                    PlaySound(SwaySound, SwayVolume);
                    anim.Play(AttackAnim);
                    StartCoroutine(SwayMelee(SwayKickback, SwaySpeed));

                    Ray playerAim = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                    if (Physics.Raycast(playerAim, out RaycastHit hit, HitDistance, HitLayer))
                    {
                        StartCoroutine(Hit(hit, playerAim.direction));
                    }
                }
            }
        }

        IEnumerator Hit(RaycastHit hit, Vector3 dir)
        {
            yield return new WaitForSeconds(HitWaitDelay);

            hit.collider.SendMessageUpwards("ApplyDamage", Random.Range(AttackDamage.x, AttackDamage.y), SendMessageOptions.DontRequireReceiver);

            if (hit.rigidbody)
            {
                hit.rigidbody.AddForceAtPosition(dir * (HitForce * 10), hit.point);
            }

            Terrain terrain;
            if ((terrain = hit.collider.GetComponent<Terrain>()) != null)
            {
                SurfaceDetails surface = surfaceDetails.GetTerrainSurfaceDetails(terrain, hit.point);

                if (surface != null)
                {
                    SpawnHitmark(surface, hit);
                }
                else
                {
                    SpawnHitmark(surfaceDetails.surfaceDetails[defaultSurfaceID], hit);
                }
            }
            else
            {
                SurfaceDetails surface = surfaceDetails.GetSurfaceDetails(hit.collider.gameObject, surfaceID);

                if (surface != null)
                {
                    SpawnHitmark(surface, hit);
                }
                else
                {
                    SpawnHitmark(surfaceDetails.surfaceDetails[defaultSurfaceID], hit);
                }
            }
        }

        void SpawnHitmark(SurfaceDetails surface, RaycastHit hit)
        {
            bool canSpawn = true;

            if (hit.collider.GetComponent<InteractiveItem>() && hit.collider.GetComponent<InteractiveItem>().examineType != InteractiveItem.ExamineType.None)
            {
                canSpawn = false;
            }

            if (canSpawn && surface.HasMeleemarks() && surface.SurfaceProperties.AllowImpactMark)
            {
                GameObject mark = Instantiate(surface.Meleemark(), hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                mark.transform.SetParent(hit.collider.transform);

                Vector3 relative = mark.transform.InverseTransformPoint(cam.transform.position);
                int angle = Mathf.RoundToInt(Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg);

                mark.transform.RotateAround(hit.point, hit.normal, angle);

                if (surface.HasMeleeImpacts() && mark.GetComponent<AudioSource>())
                {
                    AudioSource markAS = mark.GetComponent<AudioSource>();
                    markAS.clip = surface.MeleeImpact();
                    markAS.volume = surface.ImpactProperties.ImpactVolume;
                    markAS.Play();
                }
            }
            else if (surface.HasMeleeImpacts())
            {
                AudioClip clip = surface.MeleeImpact();
                AudioSource.PlayClipAtPoint(clip, hit.transform.position, surface.ImpactProperties.ImpactVolume);
            }
        }

        IEnumerator SwayMelee(Vector3 pos, float time)
        {
            Quaternion s = playerControl.baseKickback.transform.localRotation;
            Quaternion sw = playerControl.weaponKickback.transform.localRotation;
            Quaternion e = playerControl.baseKickback.transform.localRotation * Quaternion.Euler(pos);
            float r = 1.0f / time;
            float t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime * r;
                playerControl.baseKickback.transform.localRotation = Quaternion.Slerp(s, e, t);
                playerControl.weaponKickback.transform.localRotation = Quaternion.Slerp(sw, e, t);
                yield return null;
            }
        }
    }
}