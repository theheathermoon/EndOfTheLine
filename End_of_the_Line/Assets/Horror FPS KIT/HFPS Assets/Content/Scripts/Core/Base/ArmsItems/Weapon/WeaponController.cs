/*
 * WeaponController.cs - by ThunderWire Studio
 * Version 3.0
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Weapon Shooting Script
    /// </summary>
    public class WeaponController : SwitcherBehaviour, ISaveableArmsItem, IOnAnimatorState
    {
        public enum WeaponType { Semi, Auto, Shotgun }
        public enum BulletType { None, Bullet }
        public enum ReloadSound { None, Script, Animation }


        #region Structures
        [Serializable]
        public sealed class InventorySettings
        {
            [InventorySelector]
            public int weaponID;
            [InventorySelector]
            public int bulletsID;
        }

        [Serializable]
        public sealed class WeaponSettings
        {
            public int weaponDamage = 20;
            public float shootRange = 250.0f;
            public float hitforce = 20.0f;
            public float fireRate = 0.1f;
            public float recoil = 0.1f;
        }

        [Serializable]
        public sealed class AimingSettings
        {
            public bool enableAiming = true;
            public bool steadyAim = true;
            public Vector3 aimPosition;
            public float aimSpeed = 0.25f;
            public float zoomFOVSmooth = 10f;
            public float unzoomFOVSmooth = 5f;
            public int zoomFOV = 40;
        }

        [Serializable]
        public sealed class BulletSettings
        {
            public SurfaceID surfaceID = SurfaceID.Texture;
            public string FleshTag = "Flesh";
            public int defaultSurfaceID;
            public int bulletsInMag = 0;
            public int bulletsPerMag = 0;
            public int bulletsPerShot = 1;
            public bool keepReloadMagBullets;
            public bool soundOnImpact;
            public bool ejectShells;
        }

        [Serializable]
        public sealed class MuzzleFlashSettings
        {
            public bool enableMuzzleFlash;
            public Vector3 muzzleRotation;
            public Renderer muzzleFlash;
            public Light muzzleLight;
        }

        [Serializable]
        public sealed class KickbackSettings
        {
            public float kickUp = 1f;
            public float kickSideways = 1f;
            public float kickTime = 0.1f;
            public float kickReturnSpeed = 6f;
        }

        [Serializable]
        public sealed class AudioSettings
        {
            public ReloadSound reloadSound = ReloadSound.Script;
            [Space(5)]
            public AudioClip soundDraw;
            [Range(0, 2)] public float volumeDraw = 1f;
            public AudioClip soundFire;
            [Range(0, 2)] public float volumeFire = 1f;
            public AudioClip soundEmpty;
            [Range(0, 2)] public float volumeEmpty = 1f;
            public AudioClip soundReload;
            [Range(0, 2)] public float volumeReload = 1f;
            [Range(0, 2)] public float impactVolume = 1f;
        }

        [Serializable]
        public sealed class AnimationSettings
        {
            public string hideAnim = "Hide";
            public string fireAnim = "Fire";
            public string reloadAnim = "Reload";

            [Header("Shotgun Specific")]
            public string beforeReloadAnim = "BeforeReload";
            public string afterReloadAnim = "AfterReload";
            public string afterReloadEmptyAnim = "AfterReloadEmpty";
        }

        [Serializable]
        public sealed class NPCReactionSettings
        {
            public float soundReactionRadius = 20f;
            public bool enableSoundReaction;
        }

        [Serializable]
        public sealed class BulletModelSettings
        {
            public Transform barrelEndPosition;
            public GameObject bulletPrefab;
            public Vector3 bulletRotation;
            public float bulletForce = 10f;
        }

        [Serializable]
        public sealed class ShellEfectSettings
        {
            public Transform ejectPosition;
            public GameObject shellPrefab;
            public Vector3 shellRotation;
            public float ejectSpeed = 10f;
            [Tooltip("Eject shells automatically on fire or with an animation event?")]
            public bool ejectAutomatiacally = false;
        }
        #endregion

        #region Public Variables
        public WeaponType weaponType = WeaponType.Semi;
        public BulletType bulletType = BulletType.None;
        public LayerMask raycastMask;
        public LayerMask soundReactionMask;

        [Header("Referencing")]
        public SurfaceDetailsScriptable surfaceDetails;
        public StabilizeKickback kickback;
        public AudioSource audioSource;

        [Header("Weapon Configuration")]
        [ReadOnly] public int carryingBullets = 0;
        public InventorySettings inventorySettings = new InventorySettings();
        public WeaponSettings weaponSettings = new WeaponSettings();
        public AimingSettings aimingSettings = new AimingSettings();
        public BulletSettings bulletSettings = new BulletSettings();
        public KickbackSettings kickbackSettings = new KickbackSettings();
        public MuzzleFlashSettings muzzleFlashSettings = new MuzzleFlashSettings();
        public AudioSettings audioSettings = new AudioSettings();
        public AnimationSettings animationSettings = new AnimationSettings();
        public NPCReactionSettings npcReactionSettings = new NPCReactionSettings();
        public BulletModelSettings bulletModelSettings = new BulletModelSettings();
        public ShellEfectSettings shellEjectSettings = new ShellEfectSettings();
        #endregion

        #region Private Variables
        private ScriptManager scriptManager;
        private HFPS_GameManager gameManager;
        private PlayerFunctions playerFunctions;
        private PlayerController playerController;
        private Inventory inventory;
        private Transform Player;

        private Animator weaponAnim;
        private Transform weaponRoot;

        Coroutine shotgunReload = null;

        private RaycastHit hit;
        private Camera mainCamera;

        private bool fireControl;
        private bool reloadControl;
        private bool zoomControl;

        private float fireTime = 0;
        private float muzzleTime = 0;
        private float conflictTime = 0;

        private Vector3 hipPosition;
        private Vector3 distVector;

        private bool isHideAnim;
        private bool isSelected;
        private bool isBlocked;
        private bool isReloading;
        private bool isAiming;
        private bool isEmpty;
        private bool canFire;
        private bool fireOnce;
        private bool interruptReload;

        private bool muzzleShown;
        private bool wallHit;
        private bool uiShown;

        private string stateName = string.Empty;
        private float stateTime;

        private bool inputWait;
        private float waitTime;
        #endregion

        void Awake()
        {
            if (ScriptManager.HasReference)
                scriptManager = ScriptManager.Instance;

            if (HFPS_GameManager.HasReference)
                gameManager = HFPS_GameManager.Instance;

            if (Inventory.HasReference)
                inventory = Inventory.Instance;

            playerFunctions = scriptManager.C<PlayerFunctions>();

            if (GetComponentInParent<PlayerController>() is var p && p != null)
            {
                Player = p.transform;
                playerController = p;
            }

            mainCamera = scriptManager.MainCamera;
            weaponRoot = transform.GetChild(0);
            weaponAnim = weaponRoot.GetComponent<Animator>();

            hipPosition = weaponAnim.transform.localPosition;
            distVector = hipPosition;

            fireTime = weaponSettings.fireRate;

            if (muzzleFlashSettings.enableMuzzleFlash)
            {
                if (muzzleFlashSettings.muzzleFlash) muzzleFlashSettings.muzzleFlash.enabled = false;
                if (muzzleFlashSettings.muzzleLight) muzzleFlashSettings.muzzleLight.enabled = false;
            }

            if (weaponType != WeaponType.Shotgun)
            {
                bulletSettings.bulletsPerShot = 1;
            }
        }

        void OnEnable()
        {
            if (isReloading)
            {
                StartReload();
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
                fireControl = InputHandler.ReadButton("Fire");
                zoomControl = InputHandler.ReadButton("Zoom");

                if (InputHandler.IsCompositesSame("Reload", "Examine"))
                {
                    if (!scriptManager.IsExamineRaycast && !scriptManager.IsGrabRaycast)
                    {
                        if (conflictTime <= 0)
                        {
                            reloadControl = InputHandler.ReadButton("Reload");
                        }
                        else
                        {
                            conflictTime -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        conflictTime = 0.3f;
                    }
                }
                else
                {
                    reloadControl = InputHandler.ReadButton("Reload");
                }
            }

            if (inventory)
            {
                if (inventory.CheckItemInventory(inventorySettings.weaponID) && inventorySettings.weaponID != -1)
                {
                    inventory.SetItemAmount(inventorySettings.weaponID, bulletSettings.bulletsInMag);
                }

                if (inventory.CheckItemInventory(inventorySettings.bulletsID) && inventorySettings.bulletsID != -1)
                {
                    carryingBullets = inventory.GetItemAmount(inventorySettings.bulletsID);
                }
                else
                {
                    carryingBullets = 0;
                }
            }

            if (gameManager && uiShown)
            {
                gameManager.userInterface.BulletsText.text = bulletSettings.bulletsInMag.ToString();
                gameManager.userInterface.MagazinesText.text = carryingBullets.ToString();

                if (inventory.preventUse)
                {
                    gameManager.gamePanels.AmmoPanel.SetActive(false);
                }
                else
                {
                    gameManager.gamePanels.AmmoPanel.SetActive(true);
                }
            }

            if (!weaponRoot.gameObject.activeSelf) return;
            if (!scriptManager.ScriptEnabledGlobal) return;
            if (wallHit || isBlocked || !isSelected || isHideAnim) return;

            if (Cursor.lockState != CursorLockMode.None)
            {
                if (fireControl && !interruptReload)
                {
                    if (!isReloading)
                    {
                        if (weaponType == WeaponType.Auto)
                        {
                            if (canFire)
                            {
                                FireOneBullet();
                                fireTime = weaponSettings.fireRate;
                            }
                        }
                        else if (!fireOnce && canFire)
                        {
                            FireOneBullet();
                            fireTime = weaponSettings.fireRate;
                            fireOnce = true;
                        }
                    }
                    else if (weaponType == WeaponType.Shotgun)
                    {
                        interruptReload = true;
                        StopCoroutine(shotgunReload);
                        StartCoroutine(InterruptShotgunReloading());
                    }
                }
                else if (fireOnce)
                {
                    fireOnce = false;
                }

                if (fireTime > 0)
                {
                    fireTime -= Time.deltaTime;
                    canFire = false;
                }
                else
                {
                    fireTime = 0;
                    canFire = true;
                }

                if (reloadControl && !isReloading && carryingBullets > 0 && bulletSettings.bulletsInMag < bulletSettings.bulletsPerMag)
                {
                    StartReload();
                    isReloading = true;
                }

                if (zoomControl && !isReloading && playerFunctions.zoomEnabled)
                {
                    if (!isAiming && aimingSettings.enableAiming)
                    {
                        distVector = aimingSettings.aimPosition;
                        playerController.steadyArms = aimingSettings.steadyAim;
                        isAiming = true;
                    }
                    else
                    {
                        gameManager.userInterface.Crosshair.enabled = false;
                        gameManager.isWeaponZooming = true;
                    }
                }
                else
                {
                    if (isAiming && aimingSettings.enableAiming)
                    {
                        distVector = hipPosition;
                        playerController.steadyArms = false;
                        isAiming = false;
                    }
                    else
                    {
                        gameManager.userInterface.Crosshair.enabled = true;
                        gameManager.isWeaponZooming = false;
                    }
                }

                if (weaponAnim.transform.localPosition != distVector)
                {
                    Vector3 lerp = Vector3.LerpUnclamped(weaponRoot.localPosition, distVector, aimingSettings.aimSpeed * Time.deltaTime);
                    weaponRoot.localPosition = lerp;
                }

                if (muzzleFlashSettings.muzzleFlash && muzzleFlashSettings.enableMuzzleFlash)
                {
                    if (muzzleShown)
                    {
                        Vector3 muzzleRot = muzzleFlashSettings.muzzleFlash.transform.localEulerAngles;
                        muzzleRot += muzzleFlashSettings.muzzleRotation.normalized * (UnityEngine.Random.value * 360);
                        muzzleFlashSettings.muzzleFlash.transform.localEulerAngles = muzzleRot;

                        muzzleTime = 0f;
                        muzzleFlashSettings.muzzleFlash.enabled = true;
                        muzzleFlashSettings.muzzleLight.enabled = true;
                        muzzleShown = false;
                    }
                    else
                    {
                        if (muzzleTime <= 0.01f)
                        {
                            muzzleTime += Time.deltaTime;
                        }
                        else
                        {
                            muzzleFlashSettings.muzzleFlash.enabled = false;
                            muzzleFlashSettings.muzzleLight.enabled = false;
                            muzzleShown = false;
                        }
                    }
                }
            }
        }

        void StartReload()
        {
            if (weaponType != WeaponType.Shotgun)
            {
                StartCoroutine(Reload());
            }
            else
            {
                shotgunReload = StartCoroutine(ReloadShotgun());
            }

            if (audioSettings.reloadSound == ReloadSound.Script)
            {
                if (audioSettings.soundReload) PlaySound(audioSettings.soundReload, audioSettings.volumeReload);
            }
        }

        void FireOneBullet()
        {
            if (bulletSettings.bulletsInMag <= 0)
            {
                if (audioSettings.soundEmpty) PlaySound(audioSettings.soundEmpty, audioSettings.volumeEmpty);
                bulletSettings.bulletsInMag = 0;
                return;
            }

            weaponAnim.SetTrigger(animationSettings.fireAnim);

            for (int i = 0; i < bulletSettings.bulletsPerShot; i++)
            {
                float width = UnityEngine.Random.Range(-1f, 1f) * weaponSettings.recoil;
                float height = UnityEngine.Random.Range(-1f, 1f) * weaponSettings.recoil;

                Vector3 spray = mainCamera.transform.forward + mainCamera.transform.right * width + mainCamera.transform.up * height;
                Ray aim = new Ray(mainCamera.transform.position, spray.normalized);

                if (Physics.Raycast(aim, out hit, weaponSettings.shootRange, raycastMask))
                {
                    if (hit.rigidbody)
                    {
                        hit.rigidbody.AddForceAtPosition(aim.direction * weaponSettings.hitforce, hit.point);
                    }

                    ShowBulletMark(hit);
                }

                if (bulletModelSettings.bulletPrefab != null && bulletType == BulletType.Bullet)
                {
                    if (bulletModelSettings.bulletPrefab.GetComponent<Rigidbody>())
                    {
                        Vector3 direction = aim.direction;
                        GameObject bullet = Instantiate(bulletModelSettings.bulletPrefab, bulletModelSettings.barrelEndPosition.position, Quaternion.Euler(bulletModelSettings.bulletRotation));
                        bullet.GetComponent<Rigidbody>().AddForce(direction * bulletModelSettings.bulletForce, ForceMode.Impulse);
                    }
                    else
                    {
                        Quaternion rotation = Quaternion.LookRotation(aim.direction);
                        Instantiate(bulletModelSettings.bulletPrefab, bulletModelSettings.barrelEndPosition.position, rotation);
                    }
                }
            }

            bulletSettings.bulletsInMag--;
            isEmpty = bulletSettings.bulletsInMag <= 0;

            if (audioSettings.soundFire) PlaySound(audioSettings.soundFire, audioSettings.volumeFire, true);
            if (shellEjectSettings.ejectAutomatiacally) EjectShell();

            kickback.ApplyKickback(new Vector3(kickbackSettings.kickUp,
                UnityEngine.Random.Range(-kickbackSettings.kickSideways, kickbackSettings.kickSideways), 0),
                kickbackSettings.kickTime, kickbackSettings.kickReturnSpeed);

            muzzleShown = true;
        }

        void ShowBulletMark(RaycastHit hit)
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 hitPosition = hit.point;

            GameObject hitObject = hit.collider.gameObject;
            SurfaceDetails surface;
            Terrain terrain;

            if ((terrain = hitObject.GetComponent<Terrain>()) != null)
            {
                surface = surfaceDetails.GetTerrainSurfaceDetails(terrain, hit.point);
            }
            else
            {
                surface = surfaceDetails.GetSurfaceDetails(hitObject, bulletSettings.surfaceID);
            }

            if (surface == null || hitObject.CompareTag("Water") && (surface = surfaceDetails.GetSurfaceDetails("Water")) == null)
            {
                surface = surfaceDetails.surfaceDetails[bulletSettings.defaultSurfaceID];
            }

            if (surface.HasBulletmarks() && surface.SurfaceProperties.AllowImpactMark)
            {
                float rScale = UnityEngine.Random.Range(0.5f, 1.0f);

                if (surface.SurfaceProperties.SurfaceTag != bulletSettings.FleshTag)
                {
                    GameObject bulletMark = Instantiate(surface.Bulletmark(), hitPosition, hitRotation);
                    bulletMark.transform.localPosition += .02f * hit.normal;
                    bulletMark.transform.localScale = new Vector3(rScale, 1, rScale);
                    bulletMark.transform.parent = hit.transform;
                }
                else
                {
                    Instantiate(surface.Bulletmark(), hitPosition, hitRotation);
                }
            }

            if (surface.HasBulletImpacts() && bulletSettings.soundOnImpact)
            {
                AudioSource.PlayClipAtPoint(surface.BulletImpact(), hit.point, audioSettings.impactVolume);
            }

            hit.collider.SendMessage("ApplyDamage", weaponSettings.weaponDamage, SendMessageOptions.DontRequireReceiver);
        }

        public void OnStateEnter(AnimatorStateInfo state, string name)
        {
            stateName = name;
            stateTime = state.length;
        }

        public void EjectShell()
        {
            if (bulletSettings.ejectShells && shellEjectSettings.shellPrefab && shellEjectSettings.ejectPosition)
            {
                GameObject shell = Instantiate(shellEjectSettings.shellPrefab, shellEjectSettings.ejectPosition.position, Quaternion.Euler(shellEjectSettings.ejectPosition.eulerAngles + shellEjectSettings.shellRotation));
                Physics.IgnoreCollision(shell.GetComponent<Collider>(), Player.GetComponent<CharacterController>());
                shell.GetComponent<Rigidbody>().AddForce(shellEjectSettings.ejectPosition.transform.right * shellEjectSettings.ejectSpeed);
            }
        }

        IEnumerator Reload()
        {
            int bulletsToFullMag = bulletSettings.keepReloadMagBullets ? 
                bulletSettings.bulletsPerMag - bulletSettings.bulletsInMag :
                bulletSettings.bulletsPerMag;

            if (carryingBullets > 0 && bulletSettings.bulletsInMag != bulletSettings.bulletsPerMag)
            {
                weaponAnim.SetTrigger(animationSettings.reloadAnim);

                yield return new WaitUntil(() => stateName.Equals("Reload"));
                yield return new WaitForSeconds(stateTime);

                if (carryingBullets >= bulletsToFullMag)
                {
                    bulletSettings.bulletsInMag = bulletSettings.bulletsPerMag;
                }
                else
                {
                    if (bulletSettings.keepReloadMagBullets)
                    {
                        bulletSettings.bulletsInMag += carryingBullets;
                    }
                    else
                    {
                        bulletSettings.bulletsInMag = carryingBullets;
                    }
                }

                inventory.RemoveItemAmount(inventorySettings.bulletsID, bulletsToFullMag);
            }

            isReloading = false;
            canFire = true;
            fireOnce = false;
            fireTime = 0;

            stateName = string.Empty;
            stateTime = 0;
        }

        IEnumerator ReloadShotgun()
        {
            int shellsToFull = bulletSettings.bulletsPerMag - bulletSettings.bulletsInMag;

            if (carryingBullets > 0 && bulletSettings.bulletsInMag != bulletSettings.bulletsPerMag)
            {
                weaponAnim.SetTrigger(animationSettings.beforeReloadAnim);
                yield return new WaitUntil(() => stateName.Equals("Before"));
                yield return new WaitForSeconds(stateTime);

                int insertCycles = Mathf.Min(shellsToFull, carryingBullets);

                for (int i = 0; i < insertCycles; i++)
                {
                    if (interruptReload) break;

                    weaponAnim.SetTrigger(animationSettings.reloadAnim);

                    yield return new WaitUntil(() => stateName.Equals("Insert"));
                    yield return new WaitForSeconds(stateTime);

                    inventory.RemoveItemAmount(inventorySettings.bulletsID, 1);
                    bulletSettings.bulletsInMag++;

                    stateName = string.Empty;
                    stateTime = 0;
                }
            }

            weaponAnim.SetTrigger(isEmpty ? animationSettings.afterReloadEmptyAnim : animationSettings.afterReloadAnim);
            yield return new WaitUntil(() => stateName.Equals("After"));
            yield return new WaitForSeconds(stateTime);

            interruptReload = false;
            isReloading = false;
            canFire = true;
            fireOnce = false;
            fireTime = 0;

            stateName = string.Empty;
            stateTime = 0;
        }

        IEnumerator InterruptShotgunReloading()
        {
            weaponAnim.SetTrigger(isEmpty ? animationSettings.afterReloadEmptyAnim : animationSettings.afterReloadAnim);
            yield return new WaitUntil(() => stateName.Equals("After"));

            weaponAnim.ResetTrigger(animationSettings.reloadAnim);
            weaponAnim.ResetTrigger(animationSettings.beforeReloadAnim);
            weaponAnim.ResetTrigger(animationSettings.afterReloadAnim);
            weaponAnim.ResetTrigger(animationSettings.afterReloadEmptyAnim);

            yield return new WaitForSeconds(stateTime);

            interruptReload = false;
            isReloading = false;
            canFire = true;
            fireOnce = false;
            fireTime = 0;

            stateName = string.Empty;
            stateTime = 0;
        }

        void PlaySound(AudioClip clip, float volume, bool reaction = false)
        {
            if (audioSource)
            {
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.Play();
            }

            if (reaction && npcReactionSettings.enableSoundReaction)
            {
                Collider[] colliderHit = Physics.OverlapSphere(Player.position, npcReactionSettings.soundReactionRadius, soundReactionMask);

                foreach (var hit in colliderHit)
                {
                    INPCReaction m_reaction;

                    if ((m_reaction = hit.GetComponentInChildren<INPCReaction>()) != null)
                    {
                        m_reaction.SoundReaction(Player.position, false);
                    }
                }
            }
        }

        void SetZoomFOV(bool useDefault)
        {
            if (!useDefault)
            {
                playerFunctions.ZoomFOV = aimingSettings.zoomFOV;
                playerFunctions.ZoomSmooth = aimingSettings.zoomFOVSmooth;
                playerFunctions.UnzoomSmooth = aimingSettings.unzoomFOVSmooth;
            }
            else
            {
                playerFunctions.ResetDefaults();
                playerFunctions.wallHit = false;
                gameManager.userInterface.Crosshair.enabled = true;
                gameManager.isWeaponZooming = false;
            }
        }

        public override void OnSwitcherSelect()
        {
            if (isSelected || stateName.Equals("Draw")) return;

            StartCoroutine(SelectEvent());
        }

        public override void OnSwitcherDeselect()
        {
            if (!isSelected || isReloading || stateName.Equals("Hide")) return;

            isHideAnim = true;

            StartCoroutine(DeselectEvent());
            SetZoomFOV(true);
        }

        public override void OnSwitcherActivate()
        {
            weaponRoot.gameObject.SetActive(true);
            SetZoomFOV(false);

            if (gameManager)
            {
                gameManager.gamePanels.AmmoPanel.SetActive(true);
                uiShown = true;
            }

            isReloading = false;
            isSelected = true;

            stateName = string.Empty;
            stateTime = 0;
        }

        public override void OnSwitcherDeactivate()
        {
            isSelected = false;
            SetZoomFOV(true);
            weaponRoot.localPosition = hipPosition;
            weaponRoot.gameObject.SetActive(false);

            if (gameManager)
            {
                gameManager.gamePanels.AmmoPanel.SetActive(false);
                uiShown = false;
            }
        }

        public override void OnSwitcherWallHit(bool hit)
        {
            wallHit = hit;
            playerFunctions.wallHit = hit;
        }

        public override void OnSwitcherDisable(bool enabled)
        {
            isBlocked = enabled;
        }

        IEnumerator SelectEvent()
        {
            weaponRoot.gameObject.SetActive(true);

            yield return new WaitUntil(() => stateName.Equals("Draw"));

            SetZoomFOV(false);

            if (gameManager)
            {
                gameManager.gamePanels.AmmoPanel.SetActive(true);
                uiShown = true;
            }

            if (audioSettings.soundDraw)
            {
                PlaySound(audioSettings.soundDraw, audioSettings.volumeDraw);
            }

            yield return new WaitForSeconds(stateTime - 0.1f);

            interruptReload = false;
            isReloading = false;
            isSelected = true;
            canFire = true;
            fireOnce = false;
            fireTime = 0;

            stateName = string.Empty;
            stateTime = 0;
        }

        IEnumerator DeselectEvent()
        {
            if (gameManager)
            {
                gameManager.gamePanels.AmmoPanel.SetActive(false);
                uiShown = false;
            }

            weaponRoot.localPosition = hipPosition;

            weaponAnim.SetTrigger(animationSettings.hideAnim);

            yield return new WaitUntil(() => stateName.Equals("Hide"));
            yield return new WaitForSeconds(stateTime);

            isHideAnim = false;
            isSelected = false;
            stateName = string.Empty;
            stateTime = 0;

            weaponRoot.gameObject.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            if (npcReactionSettings.enableSoundReaction)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, npcReactionSettings.soundReactionRadius);
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
            {
                {"bulletsInMag", bulletSettings.bulletsInMag},
                {"uiShown", uiShown}
            };
        }

        public void OnLoad(JToken token)
        {
            bulletSettings.bulletsInMag = (int)token["bulletsInMag"];

            if ((bool)token["uiShown"])
            {
                if (gameManager)
                {
                    gameManager.gamePanels.AmmoPanel.SetActive(true);
                    uiShown = true;
                }
            }
        }
    }
}