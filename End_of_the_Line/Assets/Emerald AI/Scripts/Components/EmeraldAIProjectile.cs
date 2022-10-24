using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    public class EmeraldAIProjectile : MonoBehaviour
    {
        [HideInInspector]
        public EmeraldAIAbility ProjectileData;
        [HideInInspector]
        public int Damage;
        [HideInInspector]
        public EmeraldAISystem EmeraldComponent;
        [HideInInspector]
        public EmeraldAISystem TargetEmeraldSystem;
        [HideInInspector]
        public Transform StartingTarget;
        [HideInInspector]
        public EmeraldAIPlayerDamage m_EmeraldAIPlayerDamage;
        [HideInInspector]
        public Vector3 ProjectileDirection;
        [HideInInspector]
        public float CollisionTimer;
        [HideInInspector]
        public float TimeoutTimer;
        [HideInInspector]
        public bool Collided;
        SphereCollider ProjectileCollider;
        [HideInInspector]
        public float HeatSeekingTimer = 0;
        [HideInInspector]
        public bool HeatSeekingFinished = false;
        [HideInInspector]
        public Transform ProjectileCurrentTarget;
        [HideInInspector]
        public bool TargetInView = false;
        [HideInInspector]
        public bool CriticalHit = false;
        [HideInInspector]
        public bool AngleTooBig = false;
        [HideInInspector]
        public float AdjustedAngle;
        [HideInInspector]
        public EmeraldAISystem.TargetType TargetTypeRef = EmeraldAISystem.TargetType.Player;
        [HideInInspector]
        public GameObject ObjectToDisableOnCollision;

        Vector3 AdjustTargetPosition;       
        GameObject SpawnedEffect;
        GameObject CollisionSoundObject;
        float CollisionTime;
        Vector3 LastDirection;
        GameObject DamageOverTimeComponent;
        Vector3 m_PreviousPosition;
        Vector3 m_CurrentVelocity = new Vector3(4, 4, 4);
        bool ProjectileDirectionReceived = false;
        Vector3 ProjectileGravity;
        Rigidbody m_Rigidbody;
        bool TargetLost;

        //Setup our AI's projectile once on Awake
        void Awake()
        {
            gameObject.layer = 2;
            gameObject.AddComponent<SphereCollider>();
            ProjectileCollider = GetComponent<SphereCollider>();
            ProjectileCollider.center = Vector3.zero;
            ProjectileCollider.isTrigger = true;
            m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            gameObject.isStatic = false;
            CollisionSoundObject = Resources.Load("Emerald Collision Sound") as GameObject;
        }

        void Start()
        {
            gameObject.AddComponent<AudioSource>();
            DamageOverTimeComponent = Resources.Load<GameObject>("Damage Over Time Component");
            ProjectileCollider.radius = ProjectileData.ColliderRadius;
            InitailizeAudioSource();
        }

        void InitailizeAudioSource()
        {
            AudioSource m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.spatialBlend = EmeraldComponent.m_AudioSource.spatialBlend;
            m_AudioSource.minDistance = EmeraldComponent.m_AudioSource.minDistance;
            m_AudioSource.maxDistance = EmeraldComponent.m_AudioSource.maxDistance;
            m_AudioSource.rolloffMode = EmeraldComponent.m_AudioSource.rolloffMode;
        }

        public void InitializeProjectile (EmeraldAISystem SentEmeraldComponent, EmeraldAIAbility SentProjectileData)
        {
            EmeraldComponent = SentEmeraldComponent;
            ProjectileData = SentProjectileData;

            //Initialize the needeed local variables each time the projectile is created.
            AngleTooBig = false;
            TargetLost = false;
            HeatSeekingFinished = false;
            HeatSeekingTimer = 0;
            ProjectileCurrentTarget = EmeraldComponent.CurrentTarget;
            CollisionTimer = 0;
            TimeoutTimer = 0;
            CollisionTime = ProjectileData.CollisionTime;
            Collided = false;
            TargetInView = false;
            StartingTarget = EmeraldComponent.CurrentTarget;
            TargetEmeraldSystem = EmeraldComponent.TargetEmerald;
            TargetTypeRef = EmeraldComponent.TargetTypeRef;

            //If the user set the projectile to stick into targets, but left the Collision Time at 0, set it to 3 so the projectile doesn't despawn right away.
            if (CollisionTime == 0 && ProjectileData.ArrowProjectileRef == EmeraldAIAbility.Yes_No.Yes)
            {
                CollisionTime = 3;
            }

            if (ProjectileData.HeatSeekingRef == EmeraldAIAbility.Yes_No.Yes)
            {
                GetHeatSeekingAngle();
            }
            else if (ProjectileData.HeatSeekingRef == EmeraldAIAbility.Yes_No.No)
            {
                GetAngle();
            }

            if (ProjectileCurrentTarget != null)
            {
                Vector3 ProjectileFirePosition = ProjectileCurrentTarget.position;
                ProjectileDirection = (ProjectileFirePosition - transform.position);
            }

            if (ProjectileData.HighQualityCollisions == EmeraldAIAbility.Yes_No.Yes)
            {
                m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            ProjectileDirectionReceived = false;
            ProjectileCollider.enabled = true;
            if (ObjectToDisableOnCollision != null)
            {
                ObjectToDisableOnCollision.SetActive(true);
            }

            int AbilityDamage = 0;
            if (ProjectileData.UseRandomizedDamage == EmeraldAIAbility.Yes_No.Yes)
                AbilityDamage = Random.Range(ProjectileData.MinAbilityDamage, ProjectileData.MaxAbilityDamage+1);
            else if (ProjectileData.UseRandomizedDamage == EmeraldAIAbility.Yes_No.No)
                AbilityDamage = ProjectileData.AbilityDamage;
                

            if (ProjectileData.UseCriticalHits == EmeraldAIAbility.Yes_No.Yes && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.Instant)
            {
                float m_Odds = Random.Range(0.0f, 1.0f);
                m_Odds = Mathf.Round(m_Odds * 100f) / 100f * (100);

                float m_CritMultiplier = Random.Range(ProjectileData.CriticalHitMultiplierMin, ProjectileData.CriticalHitMultiplierMax);
                m_CritMultiplier = Mathf.Round(m_CritMultiplier * 100f) / 100f;

                if (ProjectileData.CriticalHitOdds >= m_Odds)
                {
                    CriticalHit = true;
                    Damage = Mathf.RoundToInt(AbilityDamage * m_CritMultiplier);
                }
                else
                {
                    CriticalHit = false;
                    Damage = AbilityDamage;
                }
            }
            else if (ProjectileData.UseCriticalHits == EmeraldAIAbility.Yes_No.No || ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.OverTime)
            {
                CriticalHit = false;
                Damage = AbilityDamage;
            }
        }

        //Get the angle when the projectile is created.
        public void GetHeatSeekingAngle()
        {
            if (ProjectileData.HeatSeekingRef == EmeraldAIAbility.Yes_No.Yes && ProjectileCurrentTarget != null && EmeraldComponent != null)
            {
                AdjustedAngle = EmeraldComponent.TargetAngle();

                if (AdjustedAngle <= (EmeraldComponent.MaxFiringAngle + 5))
                {
                    TargetInView = true;
                }
                else if (AdjustedAngle > (EmeraldComponent.MaxFiringAngle + 5))
                {
                    AngleTooBig = true;
                }
            }
        }

        public void GetAngle ()
        {
            if (ProjectileCurrentTarget != null && EmeraldComponent != null)
            {
                AdjustedAngle = EmeraldComponent.TargetAngle();

                if (AdjustedAngle <= (EmeraldComponent.MaxFiringAngle))
                {
                    TargetInView = true;
                }
                else if (AdjustedAngle > (EmeraldComponent.MaxFiringAngle))
                {
                    AngleTooBig = true;
                }
            }
        }

        void Update()
        {
            if (ProjectileData.UseGravity == EmeraldAIAbility.Yes_No.Yes)
            {
                ProjectileGravity = (Vector3.up * Time.deltaTime * ProjectileData.GravityAmount);
            }
            else
            {
                ProjectileGravity = Vector3.zero;
            }

            if (ProjectileCurrentTarget == null)
                TargetLost = true;

            //If the target exceeds the AI's firing angle, fire the projectile towards the last detected destination.
            if (AngleTooBig && !Collided || TargetLost)
            {               
                if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && !ProjectileDirectionReceived)
                {
                    AdjustTargetPosition = TargetEmeraldSystem.HitPointTransform.position - transform.position;
                    ProjectileDirectionReceived = true;
                }
                else if (TargetTypeRef != EmeraldAISystem.TargetType.AI && !ProjectileDirectionReceived)
                {
                    AdjustTargetPosition = EmeraldComponent.m_InitialTargetPosition - new Vector3(EmeraldComponent.transform.position.x, transform.position.y, EmeraldComponent.transform.position.z);
                    ProjectileDirectionReceived = true;
                }

                transform.position = transform.position + AdjustTargetPosition.normalized * Time.deltaTime * ProjectileData.ProjectileSpeed - ProjectileGravity;

                if (AdjustTargetPosition != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(AdjustTargetPosition);
                }
            }

            if (!AngleTooBig && !TargetLost)
            {
                //Continue to have our AI projectile follow the direction of its target until it collides with something
                if (!Collided && ProjectileData.HeatSeekingRef == EmeraldAIAbility.Yes_No.No && ProjectileDirection != Vector3.zero ||
                    !TargetInView && !Collided && ProjectileDirection != Vector3.zero)
                {
                    if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && !ProjectileDirectionReceived)
                    {
                        AdjustTargetPosition = TargetEmeraldSystem.HitPointTransform.position - transform.position;
                        ProjectileDirectionReceived = true;
                    }
                    else if (TargetTypeRef == EmeraldAISystem.TargetType.Player && !ProjectileDirectionReceived)
                    {
                        AdjustTargetPosition = ProjectileDirection + (ProjectileCurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        ProjectileDirectionReceived = true;
                    }
                    else if (TargetTypeRef == EmeraldAISystem.TargetType.NonAITarget && !ProjectileDirectionReceived)
                    {
                        AdjustTargetPosition = ProjectileDirection + (ProjectileCurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        ProjectileDirectionReceived = true;
                    }

                    transform.position += AdjustTargetPosition.normalized * Time.deltaTime * ProjectileData.ProjectileSpeed - ProjectileGravity;
                    transform.rotation = Quaternion.LookRotation(AdjustTargetPosition);  
                }

                if (!Collided && ProjectileData.HeatSeekingRef == EmeraldAIAbility.Yes_No.Yes && TargetInView)
                {
                    if (!HeatSeekingFinished)
                    {
                        if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null)
                        {
                            AdjustTargetPosition = TargetEmeraldSystem.HitPointTransform.position;
                        }
                        else if (TargetTypeRef == EmeraldAISystem.TargetType.Player)
                        {
                            AdjustTargetPosition = ProjectileCurrentTarget.position + (ProjectileCurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        }
                        else if (TargetTypeRef == EmeraldAISystem.TargetType.NonAITarget)
                        {
                            AdjustTargetPosition = ProjectileCurrentTarget.position + (ProjectileCurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        }

                        if (ProjectileCurrentTarget != null) 
                        {
                            transform.position = Vector3.MoveTowards(transform.position, AdjustTargetPosition, Time.deltaTime * ProjectileData.ProjectileSpeed);
                            transform.LookAt(AdjustTargetPosition);
                            HeatSeekingTimer += Time.deltaTime;

                            if (HeatSeekingTimer >= ProjectileData.HeatSeekingSeconds || TargetEmeraldSystem != null && TargetEmeraldSystem.CurrentHealth <= 0)
                            {
                                LastDirection = ProjectileCurrentTarget.position + (ProjectileCurrentTarget.up * EmeraldComponent.CurrentPositionModifier) - transform.position;
                                HeatSeekingFinished = true;                               
                            }
                        }
                    }
                    else if (HeatSeekingFinished && LastDirection != Vector3.zero || TargetEmeraldSystem != null && TargetEmeraldSystem.CurrentHealth <= 0)
                    {
                        if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && !ProjectileDirectionReceived)
                        {
                            AdjustTargetPosition = TargetEmeraldSystem.HitPointTransform.position - transform.position;
                            ProjectileDirectionReceived = true;
                        }
                        else if (TargetTypeRef != EmeraldAISystem.TargetType.AI && !ProjectileDirectionReceived)
                        {
                            AdjustTargetPosition = new Vector3(LastDirection.x, LastDirection.y, LastDirection.z);
                            ProjectileDirectionReceived = true;
                        }

                        transform.position = transform.position + AdjustTargetPosition.normalized * Time.deltaTime * ProjectileData.ProjectileSpeed - ProjectileGravity;

                        if (AdjustTargetPosition != Vector3.zero)
                        {
                            transform.rotation = Quaternion.LookRotation(AdjustTargetPosition);
                        }                      
                    }
                }
            }

            if (Collided)
            {
                CollisionTimer += Time.deltaTime;
                if (CollisionTimer >= CollisionTime)
                {
                    EmeraldAIObjectPool.Despawn(gameObject);
                }
            }
            else
            {
                TimeoutTimer += Time.deltaTime;
                if (TimeoutTimer >= ProjectileData.AbilityEffectTimeoutSeconds)
                {
                    EmeraldAIObjectPool.Despawn(gameObject);
                }
            }
        }

        //Handle all of our collision related calculations here. When this happens, effects and sound can be played before the object is despawned.
        void OnTriggerEnter(Collider C)
        {
            if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DebugLogProjectileCollisionsEnabled == EmeraldAISystem.YesOrNo.Yes && C.gameObject != EmeraldComponent.gameObject)
            {
                Debug.Log("<b>" + "<color=green>" + EmeraldComponent.name + "'s Projectile Hit: " + "</color>" + "<color=red>" + C.gameObject.name + "</color>" + "</b>");
            }

            if (!Collided && EmeraldComponent != null && ProjectileCurrentTarget != null && C.transform.IsChildOf(ProjectileCurrentTarget.transform) && !C.transform.IsChildOf(EmeraldComponent.transform) && C.gameObject.layer != 2 && !C.isTrigger)
            {
                if (ProjectileData.EffectOnCollisionRef == EmeraldAIAbility.Yes_No.Yes)
                {
                    if (ProjectileData.CollisionEffect != null && ProjectileData.CollisionEffectOnTargets == EmeraldAIAbility.Yes_No.Yes)
                    {
                        SpawnedEffect = EmeraldAIObjectPool.SpawnEffect(ProjectileData.CollisionEffect, transform.position, Quaternion.identity, ProjectileData.CollisionTimeout);
                        SpawnedEffect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                    }
                }

                PlayCollisionSound();

                //Damage AI target with damage based off of currently applied ability
                if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && TargetEmeraldSystem.LocationBasedDamageComp == null || 
                    TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && TargetEmeraldSystem.LocationBasedDamageComp != null && !C.GetComponent<LocationBasedDamageArea>())
                {
                    if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.Instant)
                    {
                        TargetEmeraldSystem.Damage(Damage, EmeraldAISystem.TargetType.AI, EmeraldComponent.transform, EmeraldComponent.SentRagdollForceAmount, CriticalHit);
                        EmeraldComponent.OnDoDamageEvent.Invoke();
                        if (CriticalHit)
                            EmeraldComponent.OnCriticalHitEvent.Invoke();
                    }
                    else if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.OverTime)
                    {
                        //Apply the initial damage to our target
                        TargetEmeraldSystem.Damage(ProjectileData.AbilityImpactDamage, EmeraldAISystem.TargetType.AI, EmeraldComponent.transform, EmeraldComponent.SentRagdollForceAmount);
                        EmeraldComponent.OnDoDamageEvent.Invoke();
                        if (ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.No && !TargetEmeraldSystem.ActiveEffects.Contains(ProjectileData.AbilityName) && ProjectileData.AbilityName != string.Empty || ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.Yes)
                        {
                            //Initialize the damage over time component
                            GameObject SpawnedDamageOverTimeComponent = EmeraldAIObjectPool.Spawn(DamageOverTimeComponent, ProjectileCurrentTarget.position, Quaternion.identity);
                            SpawnedDamageOverTimeComponent.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                            SpawnedDamageOverTimeComponent.GetComponent<EmeraldAIDamageOverTime>().Initialize(ProjectileData.AbilityName, ProjectileData.AbilityDamagePerIncrement, ProjectileData.AbilityDamageIncrement, ProjectileData.AbilityLength,
                                ProjectileData.DamageOverTimeEffect, ProjectileData.DamageOvertimeTimeout, ProjectileData.DamageOverTimeSound, null, m_EmeraldAIPlayerDamage, TargetEmeraldSystem, EmeraldComponent, EmeraldComponent.CurrentTarget, TargetTypeRef);
                            TargetEmeraldSystem.ActiveEffects.Add(ProjectileData.AbilityName);
                        }
                    }
                }
                else if (TargetTypeRef == EmeraldAISystem.TargetType.AI && TargetEmeraldSystem != null && TargetEmeraldSystem.LocationBasedDamageComp != null && C.GetComponent<LocationBasedDamageArea>())
                {
                    TargetEmeraldSystem.RagdollTransform = C.transform;

                    if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.Instant)
                    {
                        C.GetComponent<LocationBasedDamageArea>().DamageArea(Damage, EmeraldAISystem.TargetType.AI, EmeraldComponent.transform, EmeraldComponent.SentRagdollForceAmount, CriticalHit);
                        EmeraldComponent.OnDoDamageEvent.Invoke();
                        if (CriticalHit)
                            EmeraldComponent.OnCriticalHitEvent.Invoke();
                    }
                    else if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.OverTime)
                    {
                        //Apply the initial damage to our target
                        C.GetComponent<LocationBasedDamageArea>().DamageArea(ProjectileData.AbilityImpactDamage, EmeraldAISystem.TargetType.AI, EmeraldComponent.transform, EmeraldComponent.SentRagdollForceAmount);
                        EmeraldComponent.OnDoDamageEvent.Invoke();
                        if (ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.No && !TargetEmeraldSystem.ActiveEffects.Contains(ProjectileData.AbilityName) && ProjectileData.AbilityName != string.Empty || ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.Yes)
                        {
                            //Initialize the damage over time component
                            GameObject SpawnedDamageOverTimeComponent = EmeraldAIObjectPool.Spawn(DamageOverTimeComponent, ProjectileCurrentTarget.position, Quaternion.identity);
                            SpawnedDamageOverTimeComponent.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                            SpawnedDamageOverTimeComponent.GetComponent<EmeraldAIDamageOverTime>().Initialize(ProjectileData.AbilityName, ProjectileData.AbilityDamagePerIncrement, ProjectileData.AbilityDamageIncrement, ProjectileData.AbilityLength,
                                ProjectileData.DamageOverTimeEffect, ProjectileData.DamageOvertimeTimeout, ProjectileData.DamageOverTimeSound, null, m_EmeraldAIPlayerDamage, TargetEmeraldSystem, EmeraldComponent, EmeraldComponent.CurrentTarget, TargetTypeRef);
                            SpawnedDamageOverTimeComponent.GetComponent<EmeraldAIDamageOverTime>().m_LocationBasedDamageArea = C.GetComponent<LocationBasedDamageArea>();
                            TargetEmeraldSystem.ActiveEffects.Add(ProjectileData.AbilityName);
                        }
                    }
                }
                else if (TargetTypeRef == EmeraldAISystem.TargetType.Player) //Damage the Player with damage based off of currently applied ability
                {
                    if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.Instant)
                    {
                        DamagePlayer(Damage);
                    }
                    else if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.OverTime)
                    {
                        //Apply the initial damage to our target
                        DamagePlayer(ProjectileData.AbilityImpactDamage);
                        if (ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.No && !EmeraldComponent.CurrentTarget.GetComponent<EmeraldAIPlayerDamage>().ActiveEffects.Contains(ProjectileData.AbilityName) 
                            && ProjectileData.AbilityName != string.Empty || ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.Yes)
                        {
                            //Initialize the damage over time component
                            GameObject SpawnedDamageOverTimeComponent = EmeraldAIObjectPool.Spawn(DamageOverTimeComponent, ProjectileCurrentTarget.position, Quaternion.identity);
                            SpawnedDamageOverTimeComponent.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                            SpawnedDamageOverTimeComponent.GetComponent<EmeraldAIDamageOverTime>().Initialize(ProjectileData.AbilityName, ProjectileData.AbilityDamagePerIncrement, ProjectileData.AbilityDamageIncrement, ProjectileData.AbilityLength,
                                ProjectileData.DamageOverTimeEffect, ProjectileData.DamageOvertimeTimeout, ProjectileData.DamageOverTimeSound, null, m_EmeraldAIPlayerDamage, TargetEmeraldSystem, EmeraldComponent, EmeraldComponent.CurrentTarget, TargetTypeRef);
                            EmeraldComponent.CurrentTarget.GetComponent<EmeraldAIPlayerDamage>().ActiveEffects.Add(ProjectileData.AbilityName);
                        }
                    }
                }
                else if (TargetTypeRef == EmeraldAISystem.TargetType.NonAITarget) //Damage a non-AI target with damage based off of currently applied ability
                {
                    if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.Instant)
                    {
                        DamageNonAITarget(Damage);
                    }
                    else if (ProjectileData.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && ProjectileData.DamageType == EmeraldAIAbility.DamageTypeEnum.OverTime)
                    {
                        //Apply the initial damage to our target
                        DamageNonAITarget(ProjectileData.AbilityImpactDamage);

                        //Get a reference to the Non-AI Target component
                        EmeraldAINonAIDamage m_EmeraldAINonAIDamage = StartingTarget.GetComponent<EmeraldAINonAIDamage>();

                        if (ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.No && !m_EmeraldAINonAIDamage.ActiveEffects.Contains(ProjectileData.AbilityName) && ProjectileData.AbilityName != string.Empty || ProjectileData.AbilityStacksRef == EmeraldAIAbility.Yes_No.Yes)
                        {
                            //Initialize the damage over time component
                            GameObject SpawnedDamageOverTimeComponent = EmeraldAIObjectPool.Spawn(DamageOverTimeComponent, ProjectileCurrentTarget.position, Quaternion.identity);
                            SpawnedDamageOverTimeComponent.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                            SpawnedDamageOverTimeComponent.GetComponent<EmeraldAIDamageOverTime>().Initialize(ProjectileData.AbilityName, ProjectileData.AbilityDamagePerIncrement, ProjectileData.AbilityDamageIncrement, ProjectileData.AbilityLength,
                                ProjectileData.DamageOverTimeEffect, ProjectileData.DamageOvertimeTimeout, ProjectileData.DamageOverTimeSound, m_EmeraldAINonAIDamage, m_EmeraldAIPlayerDamage, TargetEmeraldSystem, EmeraldComponent, EmeraldComponent.CurrentTarget, TargetTypeRef);
                            m_EmeraldAINonAIDamage.ActiveEffects.Add(ProjectileData.AbilityName);
                        }
                    }
                }

                if (ObjectToDisableOnCollision != null)
                {
                    ObjectToDisableOnCollision.SetActive(false);
                }

                Collided = true;
                ProjectileCollider.enabled = false;
                AttachToCollider(C);
            }
            else if (!Collided && EmeraldComponent != null && ProjectileCurrentTarget != null && C.gameObject != ProjectileCurrentTarget.gameObject && C.gameObject != EmeraldComponent.gameObject && C.gameObject.layer != 2 && !C.transform.IsChildOf(EmeraldComponent.transform) && !C.isTrigger)
            {
                Collided = true;
                ProjectileCollider.enabled = false;

                if (ObjectToDisableOnCollision != null)
                {
                    ObjectToDisableOnCollision.SetActive(false);
                }

                if (ProjectileData.EffectOnCollisionRef == EmeraldAIAbility.Yes_No.Yes)
                {
                    if (ProjectileData.CollisionEffect != null)
                    {
                        SpawnedEffect = EmeraldAIObjectPool.SpawnEffect(ProjectileData.CollisionEffect, transform.position, Quaternion.identity, 2);
                        SpawnedEffect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                    }
                }

                PlayCollisionSound();

                //If colliding with another AI that is not the AI's target, despawn the projectile.
                if (C.GetComponent<EmeraldAISystem>() || C.GetComponent<LocationBasedDamageArea>())
                    EmeraldAIObjectPool.Despawn(gameObject);
                else
                    AttachToCollider(C);
            }

            if (gameObject.activeSelf)
            {
                m_Rigidbody.interpolation = RigidbodyInterpolation.None;
                m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }

        void DamagePlayer(int SentDamage)
        {
            if (StartingTarget.GetComponent<EmeraldAIPlayerDamage>() != null)
            {
                StartingTarget.GetComponent<EmeraldAIPlayerDamage>().SendPlayerDamage(SentDamage, EmeraldComponent.transform, EmeraldComponent, CriticalHit);                              
            }
            else
            {
                StartingTarget.gameObject.AddComponent<EmeraldAIPlayerDamage>();
                StartingTarget.GetComponent<EmeraldAIPlayerDamage>().SendPlayerDamage(SentDamage, EmeraldComponent.transform, EmeraldComponent, CriticalHit);
            }

            if (CriticalHit)
                EmeraldComponent.OnCriticalHitEvent.Invoke();

            EmeraldComponent.OnDoDamageEvent.Invoke();
            m_EmeraldAIPlayerDamage = StartingTarget.GetComponent<EmeraldAIPlayerDamage>();
        }

        void DamageNonAITarget(int SentDamage)
        {
            if (StartingTarget.GetComponent<EmeraldAINonAIDamage>() != null)
            {
                StartingTarget.GetComponent<EmeraldAINonAIDamage>().SendNonAIDamage(SentDamage, EmeraldComponent.transform, CriticalHit);
            }
            else
            {
                StartingTarget.gameObject.AddComponent<EmeraldAINonAIDamage>();
                StartingTarget.GetComponent<EmeraldAINonAIDamage>().SendNonAIDamage(SentDamage, EmeraldComponent.transform, CriticalHit);
            }

            if (CriticalHit)
                EmeraldComponent.OnCriticalHitEvent.Invoke();

            EmeraldComponent.OnDoDamageEvent.Invoke();
        }

        /// <summary>
        /// Attaches the projectile to the passed collider.
        /// </summary>
        void AttachToCollider (Collider C)
        {
            if (ProjectileData.ArrowProjectileRef == EmeraldAIAbility.Yes_No.Yes)
            {
                transform.SetParent(C.transform);
            }
        }

        /// <summary>
        /// Plays a random collision sound based on the AI's current projectile CollisionSoundsList. 
        /// </summary>
        void PlayCollisionSound ()
        {
            if (ProjectileData.SoundOnCollisionRef == EmeraldAIAbility.Yes_No.Yes && ProjectileData.CollisionSoundsList.Count > 0)
            {
                AudioClip RandomCollisionSound = ProjectileData.CollisionSoundsList[Random.Range(0, ProjectileData.CollisionSoundsList.Count)];

                if (RandomCollisionSound != null)
                {
                    GameObject CollisionSound = EmeraldAIObjectPool.SpawnEffect(CollisionSoundObject, transform.position, Quaternion.identity, 2);
                    CollisionSound.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                    AudioSource CollisionAudioSource = CollisionSound.GetComponent<AudioSource>();
                    CollisionAudioSource.PlayOneShot(RandomCollisionSound);
                }
            }
        }
    }
}
