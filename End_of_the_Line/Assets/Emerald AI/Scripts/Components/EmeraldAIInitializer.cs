using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace EmeraldAI.Utility
{
    public class EmeraldAIInitializer : MonoBehaviour
    {
        EmeraldAISystem EmeraldComponent;
        [HideInInspector] public bool CustomIKFaded;

        public void Initialize()
        {
            SetupEmeraldAISettings();
            SetupEmeraldAIObjectPool();
            SetupNavMeshAgent();
            SetupAdditionalComponents();
            SetupOptimizationSettings();
            SetupHealthBar();
            SetupCombatText();
            if (GetComponent<LocationBasedDamage>() == null)
                DisableRagdoll();
            else if (GetComponent<LocationBasedDamage>() != null)
                GetComponent<LocationBasedDamage>().InitializeLocationBasedDamage();
            SetupAudio();
            if (EmeraldComponent.DebugLogMissingAnimationsRef == EmeraldAISystem.YesOrNo.Yes)
            {
                CheckAnimationEvents();
                CheckForMissingAnimations();
            }
            InitializeWeaponTypeAnimationAndSettings();
            SetupAnimator();
            IntializeLookAtController();
            IniializeHandIK();
            InitializeDroppableWeapon();
            Invoke("CheckFactionRelations", 0.1f);
        }

        void Start()
        {
            //Invoke our AI's On Start Event
            EmeraldComponent.OnStartEvent.Invoke();
        }

        void CheckFactionRelations()
        {
            if (EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.CurrentFaction) &&
                EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.CurrentFaction)] == 0)
            {
                Debug.LogError("The AI '" + gameObject.name + "' contains an Enemy Faction Relation of its own Faction '" + EmeraldComponent.EmeraldEventsManagerComponent.GetFaction() +
                    "'. Please remove the faction from the AI Faction Relation List or change it to Friendly to avoid incorrect target detection.");
            }
        }

        void OnEnable()
        {
            //When the AI is enabled, and it has been killed, reset the AI to its default settings. 
            //This is intended for being used with Object Pooling or spawning systems such as Crux.
            if (EmeraldComponent != null && EmeraldComponent.IsDead)
            {
                EmeraldComponent.EmeraldEventsManagerComponent.ResetAI();
            }

            //Invoke the OnEnabledEvent.
            if (EmeraldComponent != null) EmeraldComponent.OnEnabledEvent.Invoke();
        }

        public void AlignOnStart()
        {
            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.25f, transform.localPosition.z), -transform.up, out HitDown, 2, EmeraldComponent.AlignmentLayerMask))
            {
                if (HitDown.transform != this.transform)
                {
                    Vector3 Normal = HitDown.normal;
                    Normal.x = Mathf.Clamp(Normal.x, -EmeraldComponent.MaxNormalAngle, EmeraldComponent.MaxNormalAngle);
                    Normal.z = Mathf.Clamp(Normal.z, -EmeraldComponent.MaxNormalAngle, EmeraldComponent.MaxNormalAngle);

                    transform.rotation = Quaternion.FromToRotation(transform.up, Normal) * transform.rotation;
                }
            }
        }

        public void IntializeLookAtController()
        {
            EmeraldComponent.EmeraldLookAtComponent.Initialize();
        }


        public void DisableRagdoll()
        {
            foreach (Rigidbody R in transform.GetComponentsInChildren<Rigidbody>())
            {
                R.isKinematic = true;
            }

            if (EmeraldComponent.LocationBasedDamageComp != null)
                return;

            foreach (Collider C in transform.GetComponentsInChildren<Collider>())
            {
                C.enabled = false;
            }

            GetComponent<BoxCollider>().enabled = true;
        }

        public void EnableRagdoll()
        {
            EmeraldComponent.AIBoxCollider.enabled = false;

            if (EmeraldComponent.LocationBasedDamageComp == null)
            {
                foreach (Collider C in transform.GetComponentsInChildren<Collider>())
                {
                    if (C.transform != this.transform)
                    {
                        C.tag = EmeraldComponent.RagdollTag;
                        C.enabled = true;
                    }
                }

                foreach (Rigidbody R in transform.GetComponentsInChildren<Rigidbody>())
                {
                    R.isKinematic = false;
                }
            }
            else
            {
                for (int i = 0; i < EmeraldComponent.LocationBasedDamageComp.ColliderList.Count; i++)
                {
                    EmeraldComponent.LocationBasedDamageComp.ColliderList[i].ColliderObject.tag = EmeraldComponent.RagdollTag;
                    EmeraldComponent.LocationBasedDamageComp.ColliderList[i].ColliderObject.enabled = true;
                }

                for (int i = 0; i < EmeraldComponent.LocationBasedDamageComp.ColliderList.Count; i++)
                {
                    EmeraldComponent.LocationBasedDamageComp.ColliderList[i].ColliderObject.GetComponent<Rigidbody>().isKinematic = false;
                    EmeraldComponent.LocationBasedDamageComp.ColliderList[i].ColliderObject.enabled = true;
                }
            }
        }

        void SetupEmeraldAIObjectPool ()
        {
            if (EmeraldAISystem.ObjectPool == null)
            {
                EmeraldAISystem.ObjectPool = new GameObject();
                EmeraldAISystem.ObjectPool.name = "Emerald Object Pool";
            }
        }

        void SetupAudio()
        {
            EmeraldComponent.m_AudioSource = GetComponent<AudioSource>();

            EmeraldComponent.m_SecondaryAudioSource = gameObject.AddComponent<AudioSource>();
            EmeraldComponent.m_SecondaryAudioSource.spatialBlend = EmeraldComponent.m_AudioSource.spatialBlend;
            EmeraldComponent.m_SecondaryAudioSource.minDistance = EmeraldComponent.m_AudioSource.minDistance;
            EmeraldComponent.m_SecondaryAudioSource.maxDistance = EmeraldComponent.m_AudioSource.maxDistance;
            EmeraldComponent.m_SecondaryAudioSource.rolloffMode = EmeraldComponent.m_AudioSource.rolloffMode;

            EmeraldComponent.m_EventAudioSource = gameObject.AddComponent<AudioSource>();
            EmeraldComponent.m_EventAudioSource.spatialBlend = EmeraldComponent.m_AudioSource.spatialBlend;
            EmeraldComponent.m_EventAudioSource.minDistance = EmeraldComponent.m_AudioSource.minDistance;
            EmeraldComponent.m_EventAudioSource.maxDistance = EmeraldComponent.m_AudioSource.maxDistance;
            EmeraldComponent.m_EventAudioSource.rolloffMode = EmeraldComponent.m_AudioSource.rolloffMode;
        }

        void SetupOptimizationSettings ()
        {
            if (EmeraldComponent.DisableAIWhenNotInViewRef == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.DisableAIWhenNotInViewRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HasMultipleLODsRef == EmeraldAISystem.YesOrNo.No)
                {
                    if (EmeraldComponent.AIRenderer != null && EmeraldComponent.UseDeactivateDelayRef == EmeraldAISystem.YesOrNo.No &&
                        EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                    {
                        EmeraldComponent.AIRenderer.gameObject.AddComponent<VisibilityCheck>();
                        GetComponentInChildren<VisibilityCheck>().EmeraldComponent = GetComponentInChildren<EmeraldAISystem>();
                    }
                    else if (EmeraldComponent.AIRenderer != null && EmeraldComponent.UseDeactivateDelayRef == EmeraldAISystem.YesOrNo.Yes &&
                        EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                    {
                        EmeraldComponent.AIRenderer.gameObject.AddComponent<VisibilityCheckDelay>();
                        GetComponentInChildren<VisibilityCheckDelay>().EmeraldComponent = GetComponentInChildren<EmeraldAISystem>();
                    }
                    else if (EmeraldComponent.HasMultipleLODsRef == EmeraldAISystem.YesOrNo.No && EmeraldComponent.AIRenderer == null)
                    {
                        EmeraldComponent.DisableAIWhenNotInViewRef = EmeraldAISystem.YesOrNo.No;
                    }
                }

                if (EmeraldComponent.HasMultipleLODsRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    if (EmeraldComponent.TotalLODsRef == EmeraldAISystem.TotalLODsEnum.Two)
                    {
                        if (EmeraldComponent.Renderer1 == null || EmeraldComponent.Renderer2 == null)
                        {
                            EmeraldComponent.DisableAIWhenNotInViewRef = EmeraldAISystem.YesOrNo.No;
                            EmeraldComponent.HasMultipleLODsRef = EmeraldAISystem.YesOrNo.No;
                        }
                    }
                    else if (EmeraldComponent.TotalLODsRef == EmeraldAISystem.TotalLODsEnum.Three)
                    {
                        if (EmeraldComponent.Renderer1 == null || EmeraldComponent.Renderer2 == null || EmeraldComponent.Renderer3 == null)
                        {
                            EmeraldComponent.DisableAIWhenNotInViewRef = EmeraldAISystem.YesOrNo.No;
                            EmeraldComponent.HasMultipleLODsRef = EmeraldAISystem.YesOrNo.No;
                        }
                    }
                    else if (EmeraldComponent.TotalLODsRef == EmeraldAISystem.TotalLODsEnum.Four)
                    {
                        if (EmeraldComponent.Renderer1 == null || EmeraldComponent.Renderer2 == null ||
                            EmeraldComponent.Renderer3 == null || EmeraldComponent.Renderer4 == null)
                        {
                            EmeraldComponent.DisableAIWhenNotInViewRef = EmeraldAISystem.YesOrNo.No;
                            EmeraldComponent.HasMultipleLODsRef = EmeraldAISystem.YesOrNo.No;
                        }
                    }
                }
            }
            else if (EmeraldComponent.DisableAIWhenNotInViewRef == EmeraldAISystem.YesOrNo.No)
            {
                EmeraldComponent.OptimizedStateRef = EmeraldAISystem.OptimizedState.Inactive;
            }

            if (EmeraldComponent.AlignmentQualityRef == EmeraldAISystem.AlignmentQuality.Low)
            {
                EmeraldComponent.RayCastUpdateSeconds = 0.3f;
            }
            else if (EmeraldComponent.AlignmentQualityRef == EmeraldAISystem.AlignmentQuality.Medium)
            {
                EmeraldComponent.RayCastUpdateSeconds = 0.2f;
            }
            else if (EmeraldComponent.AlignmentQualityRef == EmeraldAISystem.AlignmentQuality.High)
            {
                EmeraldComponent.RayCastUpdateSeconds = 0.1f;
            }

            if (EmeraldComponent.ObstructionDetectionQualityRef == EmeraldAISystem.ObstructionDetectionQuality.Low)
            {
                EmeraldComponent.ObstructionDetectionUpdateSeconds = 0.6f;
            }
            else if (EmeraldComponent.ObstructionDetectionQualityRef == EmeraldAISystem.ObstructionDetectionQuality.Medium)
            {
                EmeraldComponent.ObstructionDetectionUpdateSeconds = 0.3f;
            }
            else if (EmeraldComponent.ObstructionDetectionQualityRef == EmeraldAISystem.ObstructionDetectionQuality.High)
            {
                EmeraldComponent.ObstructionDetectionUpdateSeconds = 0.1f;
            }

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
            {
                EmeraldComponent.OptimizedStateRef = EmeraldAISystem.OptimizedState.Inactive;
            }
        }

        void SetupFactions ()
        {
            for (int i = 0; i < EmeraldComponent.FactionRelationsList.Count; i++)
            {
                EmeraldComponent.AIFactionsList.Add(EmeraldComponent.FactionRelationsList[i].FactionIndex);
                EmeraldComponent.FactionRelations.Add((int)EmeraldComponent.FactionRelationsList[i].RelationTypeRef);
            }
        }

        void SetupAdditionalComponents ()
        {
            EmeraldComponent.EmeraldDetectionComponent = GetComponent<EmeraldAIDetection>();
            EmeraldComponent.EmeraldEventsManagerComponent = GetComponent<EmeraldAIEventsManager>();
            EmeraldComponent.EmeraldBehaviorsComponent = GetComponent<EmeraldAIBehaviors>();

            EmeraldComponent.EmeraldLookAtComponent = gameObject.GetComponent<EmeraldAILookAtController>();

            if (EmeraldComponent.EmeraldLookAtComponent == null)
                EmeraldComponent.EmeraldLookAtComponent = gameObject.AddComponent<EmeraldAILookAtController>();
        }

        void SetupNavMeshAgent ()
        {
            if (GetComponent<Rigidbody>())
            {
                Rigidbody RigidbodyComp = GetComponent<Rigidbody>();
                RigidbodyComp.isKinematic = true;
                RigidbodyComp.useGravity = false;
            }

            if (EmeraldComponent.m_NavMeshAgent == null)
            {
                gameObject.AddComponent<NavMeshAgent>();
                EmeraldComponent.m_NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            EmeraldComponent.AIPath = new NavMeshPath();
            EmeraldComponent.m_NavMeshAgent.CalculatePath(transform.position, EmeraldComponent.AIPath);

            if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;
            }
            else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
            {               
                if (EmeraldComponent.CurrentTarget == null)
                {
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.FollowingStoppingDistance;
                }
            }

            EmeraldComponent.m_NavMeshAgent.radius = EmeraldComponent.AgentRadius;
            EmeraldComponent.m_NavMeshAgent.baseOffset = EmeraldComponent.AgentBaseOffset;
            EmeraldComponent.m_NavMeshAgent.angularSpeed = EmeraldComponent.StationaryTurningSpeedNonCombat;
            EmeraldComponent.m_NavMeshAgent.acceleration = EmeraldComponent.AgentAcceleration;
            EmeraldComponent.m_NavMeshAgent.updateUpAxis = false;

            if (EmeraldComponent.AvoidanceQualityRef == EmeraldAISystem.AvoidanceQuality.None)
            {
                EmeraldComponent.m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            }
            else if (EmeraldComponent.AvoidanceQualityRef == EmeraldAISystem.AvoidanceQuality.Low)
            {
                EmeraldComponent.m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            }
            else if (EmeraldComponent.AvoidanceQualityRef == EmeraldAISystem.AvoidanceQuality.Medium)
            {
                EmeraldComponent.m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            }
            else if (EmeraldComponent.AvoidanceQualityRef == EmeraldAISystem.AvoidanceQuality.Good)
            {
                EmeraldComponent.m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
            }
            else if (EmeraldComponent.AvoidanceQualityRef == EmeraldAISystem.AvoidanceQuality.High)
            {
                EmeraldComponent.m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            }

            if (EmeraldComponent.m_NavMeshAgent.enabled)
            {
                if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.CurrentFollowTarget == null)
                {
                    if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Destination)
                    {
                        EmeraldComponent.m_NavMeshAgent.autoBraking = false;
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.SingleDestination;
                        EmeraldComponent.CheckPath(EmeraldComponent.SingleDestination);
                    }
                    else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Waypoints)
                    {
                        if (EmeraldComponent.WaypointTypeRef != EmeraldAISystem.WaypointType.Random)
                        {
                            if (EmeraldComponent.WaypointsList.Count > 0)
                            {
                                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.1f;
                                EmeraldComponent.m_NavMeshAgent.autoBraking = false;
                                StartCoroutine(SetDelayedDestination(EmeraldComponent.WaypointsList[EmeraldComponent.WaypointIndex]));
                            }
                        }
                        else if (EmeraldComponent.WaypointTypeRef == EmeraldAISystem.WaypointType.Random)
                        {
                            if (EmeraldComponent.WaypointsList.Count > 0)
                            {
                                EmeraldComponent.WaypointIndex = Random.Range(0, EmeraldComponent.WaypointsList.Count);
                                StartCoroutine(SetDelayedDestination(EmeraldComponent.WaypointsList[EmeraldComponent.WaypointIndex]));
                                EmeraldComponent.m_NavMeshAgent.autoBraking = false;
                            }
                        }

                        if (EmeraldComponent.WaypointsList.Count == 0)
                        {
                            EmeraldComponent.WanderTypeRef = EmeraldAISystem.WanderType.Stationary;
                            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;
                        }
                    }
                    else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Stationary || EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Dynamic)
                    {
                        EmeraldComponent.m_NavMeshAgent.autoBraking = false;
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.StartingDestination;
                    }
                }
            }

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
            {
                if (EmeraldComponent.CurrentFollowTarget != null)
                {
                    EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
                    EmeraldComponent.StartingMovementState = EmeraldAISystem.MovementState.Run;
                }
                EmeraldComponent.UseAIAvoidance = EmeraldAISystem.YesOrNo.No;
            }
        }

        IEnumerator SetDelayedDestination (Vector3 Destination)
        {
            
            yield return new WaitForSeconds(1f);
            EmeraldComponent.LockTurning = false;
            EmeraldComponent.m_NavMeshAgent.destination = Destination;
            EmeraldComponent.WaypointIndex = 0;
        }

        void SetupAnimator ()
        {
            EmeraldComponent.AIAnimator = GetComponent<Animator>();

            if (EmeraldComponent.AIAnimator.layerCount >= 2)
                EmeraldComponent.AIAnimator.SetLayerWeight(1, 1);

            EmeraldComponent.StartingLookAtPosition = transform.position + transform.forward;

            SetupFactions();

            if (EmeraldComponent.ReverseWalkAnimation && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", -1f);
            }
            else if (!EmeraldComponent.ReverseWalkAnimation && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", 1);
            }

            if (EmeraldComponent.ReverseRangedWalkAnimation && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", -1f);
            }
            else if (!EmeraldComponent.ReverseRangedWalkAnimation && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", 1);
            }

            if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", true);
            }
            else if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.No)
            {
                EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", false);
            }

            if (EmeraldComponent.UseHitAnimations == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AIAnimator.SetBool("Use Hit", true);
            }
            else
            {
                EmeraldComponent.AIAnimator.SetBool("Use Hit", false);
            }

            if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.RootMotion)
            {
                EmeraldComponent.m_NavMeshAgent.speed = 0;
                EmeraldComponent.AIAnimator.applyRootMotion = true;
            }
            else
            {
                EmeraldComponent.AIAnimator.applyRootMotion = false;
            }

            if (EmeraldComponent.AIAnimator.layerCount >= 2)
            {
                EmeraldComponent.AIAnimator.SetLayerWeight(1, 1);
            }

            if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes || 
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.PullOutWeaponAnimation == null || EmeraldComponent.PutAwayWeaponAnimation == null)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", false);
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.RangedPullOutWeaponAnimation == null || EmeraldComponent.RangedPutAwayWeaponAnimation == null)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", false);
                    }
                }
            }

            EmeraldComponent.AIAnimator.SetInteger("Idle Index", Random.Range(1, EmeraldComponent.TotalIdleAnimations + 1));
        }

        void SetupHealthBar ()
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HealthBarCanvas == null || EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HealthBarCanvas == null)
            {
                EmeraldComponent.HealthBarCanvas = Resources.Load("AI Health Bar Canvas") as GameObject;
            }

            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HealthBarCanvas != null || EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HealthBarCanvas != null)
            {
                EmeraldComponent.HealthBar = Instantiate(EmeraldComponent.HealthBarCanvas, Vector3.zero, Quaternion.identity) as GameObject;
                GameObject HealthBarParent = new GameObject();
                HealthBarParent.name = "HealthBarParent";
                HealthBarParent.transform.SetParent(this.transform);
                HealthBarParent.transform.localPosition = new Vector3(0, 0, 0);

                EmeraldComponent.HealthBar.transform.SetParent(HealthBarParent.transform);
                EmeraldComponent.HealthBar.transform.localPosition = EmeraldComponent.HealthBarPos;
                EmeraldComponent.HealthBar.AddComponent<EmeraldAIHealthBar>();
                EmeraldAIHealthBar HealthBarScript = EmeraldComponent.HealthBar.GetComponent<EmeraldAIHealthBar>();
                EmeraldComponent.m_HealthBarComponent = HealthBarScript;
                HealthBarScript.canvas = EmeraldComponent.HealthBar.GetComponent<Canvas>();
                HealthBarScript.EmeraldComponent = GetComponent<EmeraldAISystem>();
                EmeraldComponent.HealthBar.name = "AI Health Bar Canvas";

                GameObject HealthBarChild = EmeraldComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                HealthBarChild.transform.localScale = EmeraldComponent.HealthBarScale;

                Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<Image>();
                HealthBarRef.color = EmeraldComponent.HealthBarColor;

                Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<Image>();
                HealthBarBackgroundImageRef.color = EmeraldComponent.HealthBarBackgroundColor;

                EmeraldComponent.HealthBarCanvasRef = EmeraldComponent.HealthBar.GetComponent<Canvas>();

                if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet || EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.No)
                {
                    HealthBarChild.GetComponent<Image>().enabled = false;
                    HealthBarRef.gameObject.SetActive(false);
                }

                if (EmeraldComponent.CustomizeHealthBarRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.HealthBarBackgroundImage != null && EmeraldComponent.HealthBarImage != null)
                {
                    HealthBarBackgroundImageRef.sprite = EmeraldComponent.HealthBarBackgroundImage;
                    HealthBarRef.sprite = EmeraldComponent.HealthBarImage;
                }

                //Displays and colors our AI's name text, if enabled.
                if (EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    EmeraldComponent.AINameUI = EmeraldComponent.HealthBar.transform.Find("AI Name Text").gameObject.GetComponent<Text>();

                    if (EmeraldComponent.UseAINameUIOutlineEffect == EmeraldAISystem.YesOrNo.Yes)
                    {
                        Outline AINameOutline = EmeraldComponent.AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = EmeraldComponent.AINameUIOutlineSize;
                        AINameOutline.effectColor = EmeraldComponent.AINameUIOutlineColor;
                    }
                    else
                    {
                        EmeraldComponent.AINameUI.GetComponent<Outline>().enabled = false;
                    }

                    if (EmeraldComponent.DisplayAITitleRef == EmeraldAISystem.YesOrNo.Yes)
                    {                       
                        EmeraldComponent.AIName = EmeraldComponent.AIName + "\\n" + EmeraldComponent.AITitle;
                        EmeraldComponent.AIName = EmeraldComponent.AIName.Replace("\\n", "\n");
                        EmeraldComponent.AINamePos.y += 0.25f;

                        if (EmeraldComponent.UseAINameUIOutlineEffect == EmeraldAISystem.YesOrNo.Yes)
                            EmeraldComponent.AINameUI.lineSpacing = EmeraldComponent.AINameLineSpacing;
                    }

                    EmeraldComponent.AINameUI.transform.localPosition = new Vector3(EmeraldComponent.AINamePos.x, EmeraldComponent.AINamePos.y - EmeraldComponent.HealthBarPos.y, EmeraldComponent.AINamePos.z);
                    EmeraldComponent.AINameUI.text = EmeraldComponent.AIName;
                    EmeraldComponent.AINameUI.fontSize = EmeraldComponent.NameTextFontSize;
                    EmeraldComponent.AINameUI.color = EmeraldComponent.NameTextColor;

                    if (EmeraldComponent.UseCustomFontAIName == EmeraldAISystem.YesOrNo.Yes)
                        EmeraldComponent.AINameUI.font = EmeraldComponent.AINameFont;
                }

                //Displays and colors our AI's level text, if enabled.
                if (EmeraldComponent.DisplayAILevelRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    EmeraldComponent.AILevelUI = EmeraldComponent.HealthBar.transform.Find("AI Level Text").gameObject.GetComponent<Text>();
                    EmeraldComponent.AILevelUI.text = "   "+EmeraldComponent.AILevel.ToString();
                    EmeraldComponent.AILevelUI.color = EmeraldComponent.LevelTextColor;
                    EmeraldComponent.AILevelUI.transform.localPosition = new Vector3(EmeraldComponent.AILevelPos.x, EmeraldComponent.AILevelPos.y, EmeraldComponent.AILevelPos.z);

                    if (EmeraldComponent.UseCustomFontAILevel == EmeraldAISystem.YesOrNo.Yes)
                        EmeraldComponent.AILevelUI.font = EmeraldComponent.AILevelFont;

                    if (EmeraldComponent.UseAINameUIOutlineEffect == EmeraldAISystem.YesOrNo.Yes)
                    {
                        Outline AINameOutline = EmeraldComponent.AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = EmeraldComponent.AINameUIOutlineSize;
                        AINameOutline.effectColor = EmeraldComponent.AINameUIOutlineColor;
                    }
                    else
                    {
                        EmeraldComponent.AILevelUI.GetComponent<Outline>().enabled = false;
                    }
                }

                //Add disable to return to start and slight delay
                if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                {
                    EmeraldComponent.HealthBarCanvasRef.enabled = false;
                    if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.No)
                    {
                        HealthBarBackgroundImageRef.gameObject.SetActive(false);
                    }
                    if (EmeraldComponent.AINameUI != null && EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        EmeraldComponent.AINameUI.gameObject.SetActive(false);
                    }
                    if (EmeraldComponent.AILevelUI != null && EmeraldComponent.DisplayAILevelRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        EmeraldComponent.AILevelUI.gameObject.SetActive(false);
                    }
                }
                else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
                {
                    EmeraldComponent.SetUI(true);
                }
            }
        }

        void SetupCombatText ()
        {
            if (EmeraldAISystem.CombatTextSystemObject == null)
            {
                GameObject m_CombatTextSystem = Instantiate((GameObject)Resources.Load("Combat Text System") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextSystem.name = "Combat Text System";
                GameObject m_CombatTextCanvas = Instantiate((GameObject)Resources.Load("Combat Text Canvas") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextCanvas.name = "Combat Text Canvas";
                EmeraldAISystem.CombatTextSystemObject = m_CombatTextCanvas;
                CombatTextSystem.Instance.CombatTextCanvas = m_CombatTextCanvas;
                CombatTextSystem.Instance.Initialize();
            }
        }

        void SetupEmeraldAISettings ()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Both)
                EmeraldComponent.EnableBothWeaponTypes = EmeraldAISystem.YesOrNo.Yes;
            else
                EmeraldComponent.EnableBothWeaponTypes = EmeraldAISystem.YesOrNo.No;
            EmeraldComponent.HitPointTransform = new GameObject("AI Hit Transform").transform;
            EmeraldComponent.HitPointTransform.SetParent(transform);
            EmeraldComponent.HitPointTransform.localPosition = new Vector3(0, EmeraldComponent.ProjectileCollisionPointY/transform.localScale.y, 0);
            EmeraldComponent.m_NavMeshAgent = GetComponent<NavMeshAgent>();
            EmeraldComponent.StartingRunSpeed = EmeraldComponent.RunSpeed;
            EmeraldComponent.StartingRunAnimationSpeed = EmeraldComponent.RunAnimationSpeed;
            EmeraldComponent.TargetObstructed = true;
            EmeraldComponent.StartingTag = gameObject.tag;
            EmeraldComponent.StartingLayer = gameObject.layer;
            EmeraldComponent.fieldOfViewAngleRef = EmeraldComponent.fieldOfViewAngle;
            EmeraldComponent.StartingMovementState = EmeraldComponent.CurrentMovementState;
            EmeraldComponent.StartingDetectionRadius = EmeraldComponent.DetectionRadius;
            EmeraldComponent.StartingDestination = transform.position;
            EmeraldComponent.StartingChaseDistance = EmeraldComponent.MaxChaseDistance;
            EmeraldComponent.BackingUpSeconds = Random.Range(EmeraldComponent.BackingUpSecondsMin, EmeraldComponent.BackingUpSecondsMax + 1);
            EmeraldComponent.WaitTime = Random.Range((float)EmeraldComponent.MinimumWaitTime, EmeraldComponent.MaximumWaitTime + 1);
            EmeraldComponent.IdleSoundsSeconds = Random.Range(EmeraldComponent.IdleSoundsSecondsMin, EmeraldComponent.IdleSoundsSecondsMax + 1);
            EmeraldComponent.StationaryIdleSeconds = Random.Range(EmeraldComponent.StationaryIdleSecondsMin, EmeraldComponent.StationaryIdleSecondsMax + 1);
            EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
            EmeraldComponent.AIBoxCollider = GetComponent<BoxCollider>();
            EmeraldComponent.m_AudioSource = GetComponent<AudioSource>();
            EmeraldComponent.DeathDelay = Random.Range((float)EmeraldComponent.DeathDelayMin, EmeraldComponent.DeathDelayMax + 1f);
            EmeraldComponent.GeneratedBlockOdds = Random.Range(1, 101);
            EmeraldComponent.GeneratedBackupOdds = Random.Range(1, 101);
            EmeraldComponent.CombatStateRef = EmeraldAISystem.CombatState.NotActive;
            EmeraldComponent.StartingBehaviorRef = (int)EmeraldComponent.BehaviorRef;
            EmeraldComponent.StartingConfidenceRef = (int)EmeraldComponent.ConfidenceRef;
            EmeraldComponent.StartingWanderingTypeRef = (int)EmeraldComponent.WanderTypeRef;
            EmeraldComponent.AttackTimer = 0;
            EmeraldComponent.RunAttackSpeed = Random.Range(EmeraldComponent.MinimumRunAttackSpeed, EmeraldComponent.MaximumRunAttackSpeed + 1);
            EmeraldComponent.TargetDetectionActive = true;
            EmeraldComponent.FirstTimeInCombat = true;
            EmeraldComponent.BackupDistance = (int)EmeraldComponent.StoppingDistance + 2;
            EmeraldComponent.SwitchWeaponTime = Random.Range((float)EmeraldComponent.SwitchWeaponTimeMin, EmeraldComponent.SwitchWeaponTimeMax+1);
            EmeraldComponent.AIAnimator.updateMode = AnimatorUpdateMode.Normal;

            //Ensure the LineOfSight Detection Type is being used if the SoundDetector component is present.
            if (GetComponent<SoundDetection.SoundDetector>() != null)
                EmeraldComponent.DetectionTypeRef = EmeraldAISystem.DetectionType.LineOfSight;

            if (EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.Yes)
                gameObject.layer = EmeraldComponent.NonCombatAILayerIndex;

            if (EmeraldComponent.IKType == EmeraldAISystem.IKTypes.EmeraldIK)
                EmeraldComponent.HeadLookWeightCombat = 0.6f;

            if (EmeraldComponent.AnimatorCullingMode == EmeraldAISystem.AnimatorCullingModes.AlwaysAnimate)
                EmeraldComponent.AIAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            else if (EmeraldComponent.AnimatorCullingMode == EmeraldAISystem.AnimatorCullingModes.CullUpdateTransforms)
                EmeraldComponent.AIAnimator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            if (EmeraldComponent.MeleeAttacks.Count > 0)
            {
                EmeraldComponent.GetDamageAmount();
            }

            if (EmeraldComponent.UseHitAnimations == EmeraldAISystem.YesOrNo.No && EmeraldComponent.UseBlockingRef == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.UseBlockingRef = EmeraldAISystem.YesOrNo.No;
            }

            if (EmeraldComponent.ConfidenceRef == EmeraldAISystem.ConfidenceType.Coward)
            {
                EmeraldComponent.UseEquipAnimation = EmeraldAISystem.YesOrNo.No;
            }

            if (EmeraldComponent.SummonsMultipleAIRef == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.MaxAllowedSummonedAI = 1;
            }

            //If the user forgot to add a head transform, create a temporary one to avoid an error and to still allow the AI to function.
            if (EmeraldComponent.HeadTransform == null)
            {
                Transform TempHeadTransform = new GameObject("AI Head Transform").transform;
                TempHeadTransform.SetParent(transform);
                TempHeadTransform.localPosition = new Vector3(0,1,0);
                EmeraldComponent.HeadTransform = TempHeadTransform;
            }

            if (EmeraldComponent.AttackSpeed < 1)
            {
                EmeraldComponent.AttackSpeed = 1;
            }

            if (EmeraldComponent.RunAttackSpeed < 1)
            {
                EmeraldComponent.RunAttackSpeed = 1;
            }

            if (EmeraldComponent.RangedAttackTransform == null)
            {
                EmeraldComponent.RangedAttackTransform = this.transform;
            }

            EmeraldComponent.MaxNormalAngle = ((float)EmeraldComponent.MaxNormalAngleEditor / 9 * 0.1f);

            //Companion AI cannot use the Line of Sight Detection Type. This is due to them possibly missing targets.
            if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.LineOfSight && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion
                || EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.LineOfSight && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive)
            {
                EmeraldComponent.DetectionTypeRef = EmeraldAISystem.DetectionType.Trigger;
            }             
            
            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion)
            {
                EmeraldComponent.PlayerFaction[0].RelationTypeRef = EmeraldAISystem.PlayerFactionClass.RelationType.Friendly;
            }

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Aggressive && EmeraldComponent.ConfidenceRef == EmeraldAISystem.ConfidenceType.Coward)
            {
                EmeraldComponent.ConfidenceRef = EmeraldAISystem.ConfidenceType.Brave;
            }

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
            {
                if (gameObject.tag != "Untagged")
                {
                    gameObject.tag = "Untagged";
                }
                if (gameObject.layer != 0)
                {
                    gameObject.layer = 0;
                }
                EmeraldComponent.PlayerFaction[0].RelationTypeRef = EmeraldAISystem.PlayerFactionClass.RelationType.Friendly;
            }

            if (EmeraldComponent.UseRandomRotationOnStartRef == EmeraldAISystem.YesOrNo.Yes)
            {
                transform.rotation = Quaternion.AngleAxis(Random.Range(5, 360), Vector3.up);
            }

            if (EmeraldComponent.AlignAIOnStartRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
            {
                AlignOnStart();
            }
        }

        /// <summary>
        /// Double check all essential animations to esnure they have the proper events. If not, notify the user which animations 
        /// are missing as well as which events are needed. This is to avoid confusion as to why some functionality may not be wroking correctly.
        /// </summary>
        void CheckAnimationEvents ()
        {
            if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet || EmeraldComponent.ConfidenceRef != EmeraldAISystem.ConfidenceType.Coward)
            {
                //Attack Animations
                for (int l = 0; l < EmeraldComponent.AttackAnimationList.Count; l++)
                {
                    bool AttackAnimationEventFound = false;
                    bool AnimationEventErrorDisplayed = false;

                    if (EmeraldComponent.AttackAnimationList[l].AnimationClip != null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        for (int i = 0; i < EmeraldComponent.AttackAnimationList[l].AnimationClip.events.Length; i++)
                        {
                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee &&
                                EmeraldComponent.AttackAnimationList[l].AnimationClip.events[i].functionName == "SendEmeraldDamage" || 
                                EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee &&
                                EmeraldComponent.AttackAnimationList[l].AnimationClip.events[i].functionName == "EmeraldAttackEvent")
                            {
                                AttackAnimationEventFound = true;
                            }
                        }

                        if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && !AttackAnimationEventFound && !AnimationEventErrorDisplayed)
                        {
                            Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Attack " + (l + 1) + " is missing a EmeraldAttackEvent Animation Event. " +
                                    "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                            AnimationEventErrorDisplayed = true;
                        }
                    }
                }

                //Ranged Attack Animations
                for (int l = 0; l < EmeraldComponent.RangedAttackAnimationList.Count; l++)
                {
                    bool AttackAnimationEventFound = false;
                    bool AnimationEventErrorDisplayed = false;

                    if (EmeraldComponent.RangedAttackAnimationList[l].AnimationClip != null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        for (int i = 0; i < EmeraldComponent.RangedAttackAnimationList[l].AnimationClip.events.Length; i++)
                        {
                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged &&
                                EmeraldComponent.RangedAttackAnimationList[l].AnimationClip.events[i].functionName == "CreateEmeraldProjectile" ||
                                EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged &&
                                EmeraldComponent.RangedAttackAnimationList[l].AnimationClip.events[i].functionName == "EmeraldAttackEvent")
                            {
                                AttackAnimationEventFound = true;
                            }
                        }

                        if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && !AttackAnimationEventFound && !AnimationEventErrorDisplayed)
                        {
                            Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Attack " + (l + 1) + " is missing a EmeraldAttackEvent Animation Event. " +
                                "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                            AnimationEventErrorDisplayed = true;
                        }
                    }
                }

                //Run Attack Animations
                if (EmeraldComponent.UseRunAttacks == EmeraldAISystem.YesOrNo.Yes)
                {
                    for (int l = 0; l < EmeraldComponent.RunAttackAnimationList.Count; l++)
                    {
                        bool RunAttackAnimationEventFound = false;
                        bool RunAnimationEventErrorDisplayed = false;

                        if (EmeraldComponent.RunAttackAnimationList[l].AnimationClip != null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            for (int i = 0; i < EmeraldComponent.RunAttackAnimationList[l].AnimationClip.events.Length; i++)
                            {
                                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee &&
                                    EmeraldComponent.RunAttackAnimationList[l].AnimationClip.events[i].functionName == "SendEmeraldDamage")
                                {
                                    RunAttackAnimationEventFound = true;
                                }
                                else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee &&
                                    EmeraldComponent.RunAttackAnimationList[l].AnimationClip.events[i].functionName == "EmeraldAttackEvent")
                                {
                                    RunAttackAnimationEventFound = true;
                                }
                            }

                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && !RunAttackAnimationEventFound && !RunAnimationEventErrorDisplayed ||
                                EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes && !RunAttackAnimationEventFound && !RunAnimationEventErrorDisplayed)
                            {
                                Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Run Attack " + (l + 1) + " is missing a EmeraldAttackEvent Animation Event. " +
                                        "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                                RunAnimationEventErrorDisplayed = true;
                            }
                        }
                    }
                }

                //Equip Animations
                if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                {
                    //Check Melee
                    if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Both)
                    {
                        if (EmeraldComponent.PullOutWeaponAnimation != null)
                        {
                            bool EquipAnimationEventFound = false;
                            bool AnimationEventErrorDisplayed = false;

                            for (int i = 0; i < EmeraldComponent.PullOutWeaponAnimation.events.Length; i++)
                            {
                                if (EmeraldComponent.PullOutWeaponAnimation.events[i].functionName == "EquipWeapon")
                                {
                                    EquipAnimationEventFound = true;
                                }
                            }

                            if (!EquipAnimationEventFound && !AnimationEventErrorDisplayed)
                            {
                                Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Equip Animation is missing an EquipWeapon Animation Event. " +
                                    "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                                AnimationEventErrorDisplayed = true;
                            }
                        }

                        if (EmeraldComponent.PutAwayWeaponAnimation != null)
                        {
                            bool UnequipAnimationEventFound = false;
                            bool AnimationEventErrorDisplayed = false;

                            for (int i = 0; i < EmeraldComponent.PutAwayWeaponAnimation.events.Length; i++)
                            {
                                if (EmeraldComponent.PutAwayWeaponAnimation.events[i].functionName == "UnequipWeapon")
                                {
                                    UnequipAnimationEventFound = true;
                                }
                            }

                            if (!UnequipAnimationEventFound && !AnimationEventErrorDisplayed)
                            {
                                Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Ranged Unequip Animation is missing a UnequipWeapon Animation Event. " +
                                    "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                                AnimationEventErrorDisplayed = true;
                            }
                        }
                    }
                    else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Both)
                    {
                        if (EmeraldComponent.RangedPullOutWeaponAnimation != null && EmeraldComponent.HeldRangedWeaponObject != null)
                        {
                            bool RangedEquipAnimationEventFound = false;
                            bool AnimationEventErrorDisplayed = false;

                            for (int i = 0; i < EmeraldComponent.RangedPullOutWeaponAnimation.events.Length; i++)
                            {
                                if (EmeraldComponent.RangedPullOutWeaponAnimation.events[i].functionName == "EquipWeapon")
                                {
                                    RangedEquipAnimationEventFound = true;
                                }
                            }

                            if (!RangedEquipAnimationEventFound && !AnimationEventErrorDisplayed)
                            {
                                Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Ranged Equip Animation is missing an EquipWeapon Animation Event. " +
                                    "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                                AnimationEventErrorDisplayed = true;
                            }
                        }

                        if (EmeraldComponent.RangedPutAwayWeaponAnimation != null && EmeraldComponent.HeldRangedWeaponObject != null)
                        {
                            bool RangedUnequipAnimationEventFound = false;
                            bool AnimationEventErrorDisplayed = false;

                            for (int i = 0; i < EmeraldComponent.RangedPutAwayWeaponAnimation.events.Length; i++)
                            {
                                if (EmeraldComponent.RangedPutAwayWeaponAnimation.events[i].functionName == "UnequipWeapon")
                                {
                                    RangedUnequipAnimationEventFound = true;
                                }
                            }

                            if (!RangedUnequipAnimationEventFound && !AnimationEventErrorDisplayed)
                            {
                                Debug.Log("<b>" + "<color=red>" + gameObject.name + "'s Unequip Animation is missing a UnequipWeapon Animation Event. " +
                                    "Please add one or see Emerald AI's Documentation for a guide on how to do so." + "</color>" + "</b>");
                                AnimationEventErrorDisplayed = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check all of an AI's animations. If there are any missing. Debug.Log an error message notifying the user which animation is missing.
        /// </summary>
        public void CheckForMissingAnimations()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
            bool MissingAnimationFound = false;

            //Check animations Lists
            for (int l = 0; l < EmeraldComponent.IdleAnimationList.Count; l++)
            {
                if (EmeraldComponent.IdleAnimationList[l].AnimationClip == null)
                {
                    Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Idle Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                    MissingAnimationFound = true;
                }
            }
            
            if (EmeraldComponent.UseHitAnimations == EmeraldAISystem.YesOrNo.Yes)
            {
                for (int l = 0; l < EmeraldComponent.HitAnimationList.Count; l++)
                {
                    if (EmeraldComponent.HitAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Hit Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    for (int l = 0; l < EmeraldComponent.CombatHitAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.CombatHitAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Hit Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    for (int l = 0; l < EmeraldComponent.RangedCombatHitAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.RangedCombatHitAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Hit Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }
            }

            if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
            {
                for (int l = 0; l < EmeraldComponent.RangedAttackAnimationList.Count; l++)
                {
                    if (EmeraldComponent.RangedAttackAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Attack Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                for (int l = 0; l < EmeraldComponent.RangedRunAttackAnimationList.Count; l++)
                {
                    if (EmeraldComponent.RangedRunAttackAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Run Attack Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }
            }

            if (EmeraldComponent.DeathTypeRef == EmeraldAISystem.DeathType.Animation)
            {
                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Both)
                {
                    for (int l = 0; l < EmeraldComponent.DeathAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.DeathAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Death Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }

                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Both)
                {
                    for (int l = 0; l < EmeraldComponent.RangedDeathAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.RangedDeathAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Death Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }
            }

            if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet || EmeraldComponent.ConfidenceRef != EmeraldAISystem.ConfidenceType.Coward)
            {
                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.CombatIdleAnimation == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Idle Combat Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.RangedCombatIdleAnimation == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Idle Combat Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    for (int l = 0; l < EmeraldComponent.AttackAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.AttackAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Attack Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        if (EmeraldComponent.CombatWalkStraightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatWalkLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Walk Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatWalkRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatRunStraightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatRunLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Run Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatRunRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatTurnLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Turn Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.CombatTurnRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Combat Turn Right Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                    {
                        if (EmeraldComponent.RangedCombatWalkStraightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatWalkLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Walk Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatWalkRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatRunStraightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatRunLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Run Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatRunRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatTurnLeftAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Turn Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }

                        if (EmeraldComponent.RangedCombatTurnRightAnimation == null)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Combat Turn Right Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }
            }

            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || 
                EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.UseRunAttacks == EmeraldAISystem.YesOrNo.Yes)
                {
                    for (int l = 0; l < EmeraldComponent.RunAttackAnimationList.Count; l++)
                    {
                        if (EmeraldComponent.RunAttackAnimationList[l].AnimationClip == null && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
                        {
                            Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Run Attack Animation " + (l + 1) + " is missing. Please assign an animation to this slot or remove it to avoid errors." + "</color>" + "</b>");
                            MissingAnimationFound = true;
                        }
                    }
                }
            }

            //Double Check Single Animations
            if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
            {
                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes || 
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.PullOutWeaponAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Equip Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }

                    if (EmeraldComponent.PutAwayWeaponAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Unequip Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.RangedPullOutWeaponAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Equip Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }

                    if (EmeraldComponent.RangedPutAwayWeaponAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Unequip Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }
            }

            if (EmeraldComponent.NonCombatIdleAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Idle Non-Combat Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.UseWarningAnimationRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.NonCombatAI == EmeraldAISystem.YesOrNo.No)
            {
                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.IdleWarningAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Warning Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }

                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes ||
                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.RangedIdleWarningAnimation == null)
                    {
                        Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Ranged Warning Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                        MissingAnimationFound = true;
                    }
                }
            }

            if (EmeraldComponent.WalkStraightAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.WalkLeftAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Walk Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.WalkRightAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Walk Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.RunStraightAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.RunLeftAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Run Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.RunRightAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Run Straight Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.NonCombatTurnLeftAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Non-Combat Turn Left Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (EmeraldComponent.NonCombatTurnRightAnimation == null)
            {
                Debug.LogError("<b>" + "<color=red>" + gameObject.name + "'s Non-Combat Turn Right Animation is missing. Please assign an animation to this slot to avoid errors." + "</color>" + "</b>");
                MissingAnimationFound = true;
            }

            if (!MissingAnimationFound && !Application.isPlaying)
            {
                Debug.Log("<b>" + "<color=green>" + gameObject.name + " is NOT missing any animations." + "</color>" + "</b>");
            }
        }

        public void InitializeWeaponTypeAnimationAndSettings ()
        {
            if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No)
            {
                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 0);
                    EmeraldComponent.AttackDistance = EmeraldComponent.MeleeAttackDistance;
                    EmeraldComponent.AttackSpeed = Random.Range((float)EmeraldComponent.MinMeleeAttackSpeed, EmeraldComponent.MaxMeleeAttackSpeed + 1);
                    EmeraldComponent.TooCloseDistance = EmeraldComponent.MeleeTooCloseDistance;
                }
                else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 1);
                    EmeraldComponent.AttackDistance = EmeraldComponent.RangedAttackDistance;
                    EmeraldComponent.AttackSpeed = Random.Range((float)EmeraldComponent.MinRangedAttackSpeed, EmeraldComponent.MaxRangedAttackSpeed + 1);
                    EmeraldComponent.TooCloseDistance = EmeraldComponent.RangedTooCloseDistance;
                }
            }
            else
            {
                EmeraldComponent.WeaponTypeRef = EmeraldAISystem.WeaponType.Ranged;
                EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 1);
                EmeraldComponent.AttackDistance = EmeraldComponent.RangedAttackDistance;
                EmeraldComponent.AttackSpeed = Random.Range((float)EmeraldComponent.MinRangedAttackSpeed, EmeraldComponent.MaxRangedAttackSpeed + 1);
                EmeraldComponent.TooCloseDistance = EmeraldComponent.RangedTooCloseDistance;
            }  
        }

        /// <summary>
        /// Intializes the AI's Hand IK.
        /// </summary>
        void IniializeHandIK ()
        {
            if (GetComponent<EmeraldAIHandIK>() != null)
            {
                EmeraldComponent.EmeraldHandIKComp = GetComponent<EmeraldAIHandIK>();

                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.No)
                    EmeraldComponent.EmeraldHandIKComp.InstantlyFadeInWeights();
            }
        }

        /// <summary>
        /// Intializes the AI's droppable weapon.
        /// </summary>
        void InitializeDroppableWeapon ()
        {
            if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.UseDroppableWeapon == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    if (EmeraldComponent.HeldMeleeWeaponObject != null)
                    {
                        EmeraldComponent.DroppableMeleeWeapon = EmeraldAIObjectPool.Spawn(EmeraldComponent.HeldMeleeWeaponObject, EmeraldComponent.HeldMeleeWeaponObject.transform.position, EmeraldComponent.HeldMeleeWeaponObject.transform.rotation);
                        EmeraldComponent.DroppableMeleeWeapon.transform.localScale = EmeraldComponent.HeldMeleeWeaponObject.transform.lossyScale;
                        EmeraldComponent.DroppableMeleeWeapon.gameObject.SetActive(false);
                        EmeraldComponent.DroppableMeleeWeapon.transform.SetParent(EmeraldComponent.HeldMeleeWeaponObject.transform.parent);
                        EmeraldComponent.DroppableMeleeWeapon.transform.localPosition = EmeraldComponent.HeldMeleeWeaponObject.transform.localPosition;
                        EmeraldComponent.DroppableMeleeWeapon.gameObject.name = EmeraldComponent.HeldMeleeWeaponObject.gameObject.name + " (Droppable Copy)";
                    }
                }
                else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    if (EmeraldComponent.HeldRangedWeaponObject != null)
                    {
                        TransferAbilityEffects();
                        EmeraldComponent.DroppableRangedWeapon = EmeraldAIObjectPool.Spawn(EmeraldComponent.HeldRangedWeaponObject, EmeraldComponent.HeldRangedWeaponObject.transform.position, EmeraldComponent.HeldRangedWeaponObject.transform.rotation);
                        EmeraldComponent.DroppableRangedWeapon.transform.localScale = EmeraldComponent.HeldRangedWeaponObject.transform.lossyScale;
                        EmeraldComponent.DroppableRangedWeapon.gameObject.SetActive(false);
                        EmeraldComponent.DroppableRangedWeapon.transform.SetParent(EmeraldComponent.HeldRangedWeaponObject.transform.parent);
                        EmeraldComponent.DroppableRangedWeapon.transform.localPosition = EmeraldComponent.HeldRangedWeaponObject.transform.localPosition;
                        EmeraldComponent.DroppableRangedWeapon.gameObject.name = EmeraldComponent.HeldRangedWeaponObject.gameObject.name + " (Droppable Copy)";
                    }
                }
            }
            else if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.UseDroppableWeapon == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.HeldMeleeWeaponObject != null)
                {
                    EmeraldComponent.DroppableMeleeWeapon = EmeraldAIObjectPool.Spawn(EmeraldComponent.HeldMeleeWeaponObject, EmeraldComponent.HeldMeleeWeaponObject.transform.position, EmeraldComponent.HeldMeleeWeaponObject.transform.rotation);
                    EmeraldComponent.DroppableMeleeWeapon.transform.localScale = EmeraldComponent.HeldMeleeWeaponObject.transform.lossyScale;
                    EmeraldComponent.DroppableMeleeWeapon.gameObject.SetActive(false);
                    EmeraldComponent.DroppableMeleeWeapon.transform.SetParent(EmeraldComponent.HeldMeleeWeaponObject.transform.parent);
                    EmeraldComponent.DroppableMeleeWeapon.transform.localPosition = EmeraldComponent.HeldMeleeWeaponObject.transform.localPosition;
                    EmeraldComponent.DroppableMeleeWeapon.gameObject.name = EmeraldComponent.HeldMeleeWeaponObject.gameObject.name + " (Droppable Copy)";
                }

                if (EmeraldComponent.HeldRangedWeaponObject != null)
                {
                    TransferAbilityEffects();
                    EmeraldComponent.DroppableRangedWeapon = EmeraldAIObjectPool.Spawn(EmeraldComponent.HeldRangedWeaponObject, EmeraldComponent.HeldRangedWeaponObject.transform.position, EmeraldComponent.HeldRangedWeaponObject.transform.rotation);
                    EmeraldComponent.DroppableRangedWeapon.transform.localScale = EmeraldComponent.HeldRangedWeaponObject.transform.lossyScale;
                    EmeraldComponent.DroppableRangedWeapon.gameObject.SetActive(false);
                    EmeraldComponent.DroppableRangedWeapon.transform.SetParent(EmeraldComponent.HeldRangedWeaponObject.transform.parent);
                    EmeraldComponent.DroppableRangedWeapon.transform.localPosition = EmeraldComponent.HeldRangedWeaponObject.transform.localPosition;
                    EmeraldComponent.DroppableRangedWeapon.gameObject.name = EmeraldComponent.HeldRangedWeaponObject.gameObject.name + " (Droppable Copy)";
                }
            }
        }

        /// <summary>
        /// Transfers any remaining ability effects within the HeldRangedWeaponObject back to the ObjectPool before creating a copy of the weapon.
        /// </summary>
        void TransferAbilityEffects()
        {
            var TimedDespawns = EmeraldComponent.HeldRangedWeaponObject.GetComponentsInChildren<EmeraldAITimedDespawn>();
            for (int i = 0; i < TimedDespawns.Length; i++)
            {
                TimedDespawns[i].transform.SetParent(EmeraldAISystem.ObjectPool.transform);
            }
        }

        /// <summary>
        /// Initialize an AI's death
        /// </summary>
        public void InitializeAIDeath ()
        {
            //Crux support
            #if CRUX_PRESENT
            if (EmeraldComponent.UseMagicEffectsPackRef == EmeraldAISystem.YesOrNo.Yes)
            {
                Crux.CruxSystem.Instance.RemoveObjectFromPopulation(gameObject);
            }
            #endif

            //Used by AI who are not using the custom IK system.
            if (EmeraldComponent.DeathTypeRef == EmeraldAISystem.DeathType.Ragdoll && EmeraldComponent.EmeraldLookAtComponent.BodyWeight == 0)
            {
                EnableRagdoll();
                Invoke("RagdollDeath", 0.01f);
            }
            else if (EmeraldComponent.DeathTypeRef == EmeraldAISystem.DeathType.Animation)
            {
                StartCoroutine(AnimationDeath());
            }

            EmeraldComponent.AIBoxCollider.enabled = false;      
        }

        private void Update()
        {
            //Only used by AI who's BodyWeight is greater than 0, which is any AI using the custom IK system. This needs to be faded out and blended
            //in order to have smooth transitions when enabling ragdolls.
            if (EmeraldComponent.IsDead && !CustomIKFaded && EmeraldComponent.EmeraldLookAtComponent.BodyWeight > 0 && EmeraldComponent.DeathTypeRef == EmeraldAISystem.DeathType.Ragdoll)
            {
                EmeraldComponent.EmeraldLookAtComponent.UnityHeadIK = Mathf.LerpAngle(EmeraldComponent.EmeraldLookAtComponent.UnityHeadIK, 0.5f, Time.deltaTime * 60);
                EmeraldComponent.EmeraldLookAtComponent.UnityBodyIK = Mathf.LerpAngle(EmeraldComponent.EmeraldLookAtComponent.UnityBodyIK, 1f, Time.deltaTime * 60);
                EmeraldComponent.EmeraldLookAtComponent.HeadFade = Mathf.LerpAngle(EmeraldComponent.EmeraldLookAtComponent.HeadFade, 0.0f, Time.deltaTime * 60);               
                EmeraldComponent.EmeraldLookAtComponent.BodyWeight = Mathf.LerpAngle(EmeraldComponent.EmeraldLookAtComponent.BodyWeight, 0, Time.deltaTime * 58);

                if (EmeraldComponent.EmeraldLookAtComponent.BodyWeight <= 0.25f)
                {
                    EmeraldComponent.EmeraldLookAtComponent.UnityHeadIK = 0.5f;
                    EmeraldComponent.EmeraldLookAtComponent.UnityBodyIK = 1f;
                    EmeraldComponent.EmeraldLookAtComponent.HeadFade = 0.0f;
                    EmeraldComponent.EmeraldLookAtComponent.BodyWeight = 0;
                    EnableRagdoll();
                    RagdollDeath();
                    CustomIKFaded = true;
                }
            }
        }

        void RagdollDeath ()
        {
            if (EmeraldComponent.RagdollTransform == null)
            {
                if (EmeraldComponent.HeadTransform.GetComponent<Rigidbody>() != null)
                    EmeraldComponent.RagdollTransform = EmeraldComponent.HeadTransform;
                else
                {
                    Rigidbody[] RandomRagdollComponent = transform.GetComponentsInChildren<Rigidbody>();
                    EmeraldComponent.RagdollTransform = RandomRagdollComponent[Random.Range(0, RandomRagdollComponent.Length)].transform;
                }
            }

            if (EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != null && EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != EmeraldComponent.CurrentTarget)
            {
                EmeraldComponent.RagdollTransform.GetComponent<Rigidbody>().AddForce((EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker().position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
            }
            else
            {
                EmeraldComponent.RagdollTransform.GetComponent<Rigidbody>().AddForce((EmeraldComponent.CurrentTarget.position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
            }

            EmeraldComponent.EmeraldEventsManagerComponent.PlayDeathSound();
            DisableComponents();
        }

        IEnumerator AnimationDeath ()
        {
            EmeraldComponent.AIAnimator.SetInteger("Death Index", Random.Range(1, EmeraldComponent.TotalDeathAnimations + 1));
            EmeraldComponent.EmeraldLookAtComponent.enabled = false;
            if (gameObject.GetComponent<EmeraldAIHandIK>() != null)
                gameObject.GetComponent<EmeraldAIHandIK>().enabled = false;
            EmeraldComponent.EmeraldEventsManagerComponent.PlayDeathSound();
            EmeraldComponent.AIAnimator.SetTrigger("Dead");
            yield return new WaitForSeconds(EmeraldComponent.SecondsToDisable);
            DisableComponents();

        }

        /// <summary>
        /// Disables an AI's components.
        /// </summary>
        void DisableComponents ()
        {
            if (gameObject.GetComponent<EmeraldAIHandIK>() != null)
                gameObject.GetComponent<EmeraldAIHandIK>().enabled = false;

            if (gameObject.GetComponent<SoundDetection.SoundDetector>() != null)
                gameObject.GetComponent<SoundDetection.SoundDetector>().enabled = false;

            EmeraldComponent.AIAnimator.enabled = false;
            GetComponent<EmeraldAISystem>().enabled = false;
            EmeraldComponent.EmeraldDetectionComponent.enabled = false;
            EmeraldComponent.EmeraldEventsManagerComponent.enabled = false;
            EmeraldComponent.EmeraldLookAtComponent.enabled = false;
            EmeraldComponent.EmeraldLookAtComponent.ResetSettings();

            if (EmeraldComponent.UseDroppableWeapon == EmeraldAISystem.YesOrNo.Yes)
                EmeraldComponent.EmeraldEventsManagerComponent.CreateDroppableWeapon();
        }
    }
}