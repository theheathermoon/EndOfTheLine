using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]  
    [RequireComponent(typeof(EmeraldAIDetection))]
    [RequireComponent(typeof(EmeraldAIInitializer))]
    [RequireComponent(typeof(EmeraldAIBehaviors))]
    [RequireComponent(typeof(EmeraldAIEventsManager))]
    [RequireComponent(typeof(EmeraldAILookAtController))]
    [SelectionBase]

    public class EmeraldAISystem : MonoBehaviour
    {
        #region Variables
        public float ForceWalkDistance = 2.5f;
        public bool LockTurning;
        public float MovementTurningSensitivity = 2f;
        //3.0 Variables
        public bool NotifiedOfNewVersion = false;
        float TimeSinceStart;
        public bool ReachedDestination;
        float DelayTimer;
        Coroutine SwitchWeaponCoroutine;
        public enum SwitchWeaponTypes { Distance, Timed};
        public SwitchWeaponTypes SwitchWeaponType = SwitchWeaponTypes.Timed;
        public int SwitchWeaponTimeMin = 10;
        public int SwitchWeaponTimeMax = 20;
        public float SwitchWeaponTime = 0;
        public float SwitchWeaponTimer = 0;        
        public EmeraldAIHandIK EmeraldHandIKComp;
        public GameObject HeldMeleeWeaponObject; 
        public GameObject HeldRangedWeaponObject;
        public GameObject HolsteredMeleeWeaponObject;       
        public GameObject HolsteredRangedWeaponObject;
        public bool TurnDirectionMet;
        public float TurnSpeedTimer;
        public EmeraldAIPlayerDamage PlayerDamageComponent;
        public static List<Transform> IgnoredTargetsList = new List<Transform>();
        public EmeraldAINonAIDamage NonAIDamageComponent;
        public Vector3 LBDImpactPosition;
        public LocationBasedDamage LocationBasedDamageComp;
        public TargetPositionModifier CurrentTargetPositionModifierComp;
        public float CurrentPositionModifier = 0;

        //Melee Attacks
        public int MeleeAttackIndex = 0;
        public enum MeleeAttackPickTypeEnum { Odds, Order, Random };
        public MeleeAttackPickTypeEnum MeleeAttackPickType = MeleeAttackPickTypeEnum.Order;
        public List<MeleeAttackClass> MeleeAttacks = new List<MeleeAttackClass>();
        public int MeleeAttacksListIndex = 0;

        //Melee Run Attacks
        public int MeleeRunAttackIndex = 0;
        public enum MeleeRunAttackPickTypeEnum { Odds, Order, Random };
        public MeleeRunAttackPickTypeEnum MeleeRunAttackPickType = MeleeRunAttackPickTypeEnum.Order;
        public List<MeleeAttackClass> MeleeRunAttacks = new List<MeleeAttackClass>();
        public int MeleeRunAttacksListIndex = 0;

        [System.Serializable]
        public class MeleeAttackClass
        {
            public int AttackAnimaition;
            public int MinDamage = 5;
            public int MaxDamage = 5;
            public int AttackOdds = 25;
            public GameObject AttackImpactEffect;
        }

        //Offensive Abilities
        public int OffensiveAbilityIndex = 0;
        public enum OffensiveAbilityPickTypeEnum { Odds, Order, Random };
        public OffensiveAbilityPickTypeEnum OffensiveAbilityPickType = OffensiveAbilityPickTypeEnum.Order;
        public List<OffensiveAbilitiesClass> OffensiveAbilities = new List<OffensiveAbilitiesClass>();

        [System.Serializable]
        public class OffensiveAbilitiesClass
        {
            public EmeraldAIAbility OffensiveAbility;
            public int AbilityAnimaition;
            public int AbilityOdds = 25;
        }

        //Support Abilities
        public int SupportAbilityIndex = 0;
        public enum SupportAbilityPickTypeEnum { Odds, Order, Random };
        public SupportAbilityPickTypeEnum SupportAbilityPickType = SupportAbilityPickTypeEnum.Order;
        public List<SupportAbilitiesClass> SupportAbilities = new List<SupportAbilitiesClass>();

        [System.Serializable]
        public class SupportAbilitiesClass
        {
            public EmeraldAIAbility SupportAbility;
            public int AbilityAnimaition;
            public int AbilityOdds = 25;
        }

        //Summoning Abilities
        public int SummoningAbilityIndex = 0;
        public enum SummoningAbilityPickTypeEnum { Odds, Order, Random };
        public SummoningAbilityPickTypeEnum SummoningAbilityPickType = SummoningAbilityPickTypeEnum.Order;
        public List<SummoningAbilitiesClass> SummoningAbilities = new List<SummoningAbilitiesClass>();

        [System.Serializable]
        public class SummoningAbilitiesClass
        {
            public EmeraldAIAbility SummoningAbility;
            public int AbilityAnimaition;
            public int AbilityOdds = 25;
        }


        public AnimatorStateInfo CurrentStateInfo;
        float AggroDelay;
        public bool CriticalHit = false;
        public bool TargetInAngleLimit = false;
        public int CurrentAnimationIndex = 0;
        public int CurrentRunAttackAnimationIndex = 0;
        public List<string> ActiveEffects = new List<string>();
        public EmeraldAIProjectile CurrentlyCreatedAbility;        
        public int MaxAllowedSummonedAI = 1;
        public int TotalSummonedAI = 0;
        public EmeraldAISystem CurrentSummoner;
        public Coroutine HealingOverTimeCoroutine;
        public int HealthPercentageToHeal = 30;
        public bool HealingCooldownActive;
        public int HealingCooldownSeconds = 8;
        float HealingCooldownTimer;
        public bool m_AbilityPicked = false;
        public Vector3 m_InitialTargetPosition;
        public bool TargetDetectionActive;
        public bool IdleActivated;
        public float DetectionFrequency = 1;
        public int PlayerDetectionEventCooldown = 5;
        public float RangedAttackOffset = 0;
        public Transform HitPointTransform;
        public int SwitchWeaponTypesCooldown = 10;
        public int SwitchWeaponTypesDistance = 8;
        public string CameraTag = "MainCamera";
        public EmeraldAIHealthBar m_HealthBarComponent;
        
        public float AttackHeight = 5;
        public EmeraldAIAbility m_EmeraldAIAbility;
        public bool RotateTowardsTarget = false;
        public bool m_AttackAnimationClipMissing = false;
        public float ProjectileCollisionPointY = 1.5f;
        public Vector3 m_ProjectileCollisionPoint;
        public float ObstructionTimer;
        public int ObstructionSeconds = 4;
        float m_ObstructedTimer;

        //Volumes
        public float IdleVolume = 1;
        public float WalkFootstepVolume = 0.1f;
        public float RunFootstepVolume = 0.1f;
        public float BlockVolume = 0.65f;
        public float ImpactVolume = 1;
        public float CriticalHitVolume = 1;
        public float InjuredVolume = 1;
        public float AttackVolume = 1;
        public float WarningVolume = 1;
        public float DeathVolume = 0.7f;
        public float EquipVolume = 1;
        public float UnequipVolume = 1;
        public float RangedEquipVolume = 1;
        public float RangedUnequipVolume = 1;

        //Head Look
        public float HeadLookWeightCombat = 0.75f;
        public float BodyLookWeightCombat = 0.75f;
        public float HeadLookWeightNonCombat = 0.85f;
        public float BodyLookWeightNonCombat = 0.1f;
        public int MaxLookAtDistance = 15;
        public float NonCombatLookSpeedMultiplier = 1f;
        public float CombatLookSpeedMultiplier = 1f;
        public int NonCombatLookAtLimit = 75;
        public int CombatLookAtLimit = 45;
        public float lookWeight;
        public int LookAtLimit;
        public AnimatorStateInfo m_LayerCurrentState;
        public Transform HeadTransform;
        public Vector3 StartingLookAtPosition;
        public List<HumanBodyBones> BoneObjects = new List<HumanBodyBones>();

        public int CurrentAggroHits = 0;
        public int TotalAggroHits = 5;
        public enum AggroAction {LastAttacker = 0, RandomAttacker, ClosestAttacker};
        public AggroAction AggroActionRef = AggroAction.LastAttacker;
        public bool ReturnToStationaryPosition;

        public EmeraldAIDetection EmeraldDetectionComponent;
        public EmeraldAIInitializer EmeraldInitializerComponent;
        public EmeraldAIEventsManager EmeraldEventsManagerComponent;
        public EmeraldAIBehaviors EmeraldBehaviorsComponent;
        public EmeraldAILookAtController EmeraldLookAtComponent;

        //Blocking
        float BlockTimer;
        float AdjustedBlockAngle;
        public int MitigationAmount = 50;
        public int MaxBlockAngle = 75;
        public int MaxDamageAngle = 75;
        public int MaxFiringAngle = 45;
        public Vector3 SurfaceNormal;
        Quaternion NormalRotation;
        public float MaxNormalAngle;
        public int MaxNormalAngleEditor = 25;
        public int BlockOdds = 80;
        public int GeneratedBlockOdds;

        public bool m_AnimationsChanged = false;
        public int m_LastAnimatorType;
        public int BackupOdds = 50;
        public int GeneratedBackupOdds;
        public int BackupDistance = 12;
        public Vector3 BackupDestination;

        //Animation States
        public bool IsEmoting;
        public bool IsIdling;
        public bool IsAttacking;
        public bool IsGettingHit;
        public bool IsEquipping;
        public bool IsBlocking; 
        public bool IsBackingUp;
        public bool IsTurning; 
        public bool IsTurningLeft, IsTurningRight;
        public bool IsSwitchingWeapons;
        public bool IsMoving;
        public bool IsDead;

        //Combat
        public float BackingUpTimer;
        public float BackingUpSeconds;
        public int BackingUpSecondsMin = 3;
        public int BackingUpSecondsMax = 4;
        public float RunAttackTimer;
        public int RunAttackSpeed;
        public int MinMeleeAttackSpeed = 0;
        public int MaxMeleeAttackSpeed = 0;
        public int MinRangedAttackSpeed = 0;
        public int MaxRangedAttackSpeed = 0;
        public float AttackSpeed = 1;
        public int MinimumRunAttackSpeed = 2;
        public int MaximumRunAttackSpeed = 4;
        public float AttackTimer;
        public bool HasTarget;
        public bool SwitchWeaponTypeTriggered = false;

        public int TotalIdleAnimations = 1;
        public int TotalHitAnimations = 1;
        public int TotalCombatHitAnimations = 1;
        public int TotalRangedCombatHitAnimations = 1;
        public int TotalEmoteAnimations = 1;
        public int TotalAttackAnimations = 1;
        public int TotalRangedAttackAnimations = 1;
        public int TotalRunAttackAnimations = 1;
        public int TotalRangedRunAttackAnimations = 1;
        public int TotalDeathAnimations = 1;
        public int TotalRangedDeathAnimations = 1;
        public bool m_IdleAnimaionIndexOverride = false;
        public bool MirrorWalkLeft = false;
        public bool MirrorWalkRight = false;
        public bool MirrorRunLeft = false;
        public bool MirrorRunRight = false;
        public bool MirrorCombatWalkLeft = false;
        public bool MirrorCombatWalkRight = false;
        public bool MirrorCombatRunLeft = false;
        public bool MirrorCombatRunRight = false;
        public bool MirrorCombatTurnLeft = false;
        public bool MirrorCombatTurnRight = false;
        public bool MirrorRangedCombatWalkLeft = false;
        public bool MirrorRangedCombatWalkRight = false;
        public bool MirrorRangedCombatRunLeft = false;
        public bool MirrorRangedCombatRunRight = false;
        public bool MirrorRangedCombatTurnLeft = false;
        public bool MirrorRangedCombatTurnRight = false;
        public bool MirrorTurnLeft = false;
        public bool MirrorTurnRight = false;
        public bool ReverseWalkAnimation = false;
        public bool ReverseRangedWalkAnimation = false;

        public int AngleToTurn;
        public int NonCombatAngleToTurn = 20;
        public int CombatAngleToTurn = 20;
        public float StationaryTurningSpeedNonCombat = 50;
        public float MovingTurnSpeedNonCombat = 2.5f;
        public int StationaryTurningSpeedCombat = 50;
        public float MovingTurnSpeedCombat = 2.5f;
        public float CurrentMovingTurnSpeed = 2.5f;
        public int BackupTurningSpeed = 100;
        public int MaxSlopeLimit = 30;
        public float Acceleration = 1;
        public float Deceleration = 0.5f;
        public float TurnAngle = 30;
        public int AlignmentSpeed;
        public int NonCombatAlignmentSpeed = 15;
        public int CombatAlignmentSpeed = 25;
        public float DirectionDampTime = 0.25f;
        public NavMeshAgent m_NavMeshAgent;
        public AudioSource m_AudioSource;
        public AudioSource m_SecondaryAudioSource;
        public AudioSource m_EventAudioSource;
        public float WaypointTimer;
        float IdleAnimationTimer;
        public int IdleAnimationSeconds = 3;
        public Vector3 NewDestination;
        float DistanceFromDestination;
        float Velocity;
        float AdjustedSpeed;
        Quaternion TargetQ;
        Quaternion GroundQ;
        float PreviousAngle;
        bool m_WeaponTypeSwitchDelay;
        float AddForceTimer;
        float DeathTimer;
        public Transform ForceTransform;
        public int ReceivedRagdollForceAmount;
        public int SentRagdollForceAmount = 200;
        public Transform HitTransform;
        public Transform RagdollTransform;
        public Vector3 TargetDestination;
        Quaternion AligmentAngle;
        public float DestinationAdjustedAngle;
        Vector3 DestinationDirection;
        public GameObject WeaponTrail;

        //Emerald AI Events
        public UnityEvent DeathEvent;
        public UnityEvent DamageEvent;
        public UnityEvent OnDoDamageEvent;
        public UnityEvent ReachedDestinationEvent;
        public UnityEvent OnStartEvent;
        public UnityEvent OnEnabledEvent;
        public UnityEvent OnPlayerDetectionEvent;
        public UnityEvent OnDetectTargetEvent;
        public UnityEvent OnAttackEvent;
        public UnityEvent OnAttackEndEvent;
        public UnityEvent OnFleeEvent;
        public UnityEvent OnStartCombatEvent;
        public UnityEvent OnKillTargetEvent;
        public UnityEvent OnHealEvent;
        public UnityEvent OnCriticalHitEvent;

        public int IdleSoundsSeconds;
        public int IdleSoundsSecondsMin = 5;
        public int IdleSoundsSecondsMax = 10;
        public float IdleSoundsTimer;
        public int CurrentAttacks = 0;
        public int ExpandedChaseDistance = 80;
        public int StartingChaseDistance;
        public int ExpandedDetectionRadius = 40;
        public int StartingDetectionRadius;
        public bool IsRunAttack = false;
        public int RandomDirection;
        public bool TargetInView = false;

        [SerializeField]
        public int CurrentFaction;
        [SerializeField]
        public static List<string> StringFactionList = new List<string>();
        public enum RelationType { Enemy = 0, Neutral = 1, Friendly = 2 };
        public RelationType RelationTypeRef;
        public List<int> FactionRelations = new List<int>();

        [SerializeField]
        public List<FactionsList> FactionRelationsList = new List<FactionsList>();

        [System.Serializable]
        public class FactionsList
        {
            public int FactionIndex;
            public enum RelationType { Enemy = 0, Neutral = 1, Friendly = 2 };
            public RelationType RelationTypeRef;

            public FactionsList(int m_FactionIndex, int m_RelationTypeRef)
            {
                FactionIndex = m_FactionIndex;
                RelationTypeRef = (RelationType)m_RelationTypeRef;
            }
        }

        [SerializeField]
        public List<PlayerFactionClass> PlayerFaction = new List<PlayerFactionClass> { new PlayerFactionClass("Player", 0) };

        [System.Serializable]
        public class PlayerFactionClass
        {
            public string PlayerTag = "Player";
            public enum RelationType { Enemy = 0, Neutral = 1, Friendly = 2 };
            public RelationType RelationTypeRef;

            public PlayerFactionClass(string m_PlayerTag, int m_RelationTypeRef)
            {
                PlayerTag = m_PlayerTag;
                RelationTypeRef = (RelationType)m_RelationTypeRef;
            }
        }

        public static GameObject ObjectPool;
        public static GameObject CombatTextSystemObject;
        public float StartingRunSpeed;
        public float StartingRunAnimationSpeed;
        public float StationaryIdleTimer = 0;
        public int StationaryIdleSeconds;
        public int StationaryIdleSecondsMin = 3;
        public int StationaryIdleSecondsMax = 6;
        public float BloodEffectTimeoutSeconds = 3f;
        public List<GameObject> BloodEffectsList = new List<GameObject>();
        public GameObject CurrentProjectile;
        public Vector3 ProjectileDirection;

        public List<GameObject> potentialTargets = new List<GameObject>();
        public List<Collider> LineOfSightTargets = new List<Collider>(); //3.1.2 Updated
        //public List<Transform> LineOfSightTargets = new List<Transform>();

        [System.Serializable]
        public class InteractSoundClass
        {
            public int SoundEffectID = 1;
            public AudioClip SoundEffectClip;
        }
        public List<InteractSoundClass> InteractSoundList = new List<InteractSoundClass>();

        [System.Serializable]
        public class ItemClass
        {
            public int ItemID = 1;
            public GameObject ItemObject;
        }
        public List<ItemClass> ItemList = new List<ItemClass>();

        [System.Serializable]
        public class EmoteAnimationClass
        {
            public EmoteAnimationClass(int NewAnimationID, AnimationClip NewEmoteAnimationClip)
            {
                AnimationID = NewAnimationID;
                EmoteAnimationClip = NewEmoteAnimationClip;
            }

            public int AnimationID = 1;
            public AnimationClip EmoteAnimationClip;
        }
        public List<EmoteAnimationClass> EmoteAnimationList = new List<EmoteAnimationClass>();

        [System.Serializable]
        public class AnimationClass
        {
            public AnimationClass(float NewAnimationSpeed, AnimationClip NewAnimationClip)
            {
                AnimationSpeed = NewAnimationSpeed;
                AnimationClip = NewAnimationClip;
            }

            public float AnimationSpeed = 1;
            public AnimationClip AnimationClip;
        }
        public List<AnimationClass> IdleAnimationList = new List<AnimationClass>();
        public List<AnimationClass> AttackAnimationList = new List<AnimationClass>();
        public List<AnimationClass> RunAttackAnimationList = new List<AnimationClass>();
        public List<AnimationClass> DeathAnimationList = new List<AnimationClass>();
        public List<AnimationClass> CombatHitAnimationList = new List<AnimationClass>();
        public List<AnimationClass> HitAnimationList = new List<AnimationClass>();

        public List<AnimationClass> RangedAttackAnimationList = new List<AnimationClass>();
        public List<AnimationClass> RangedCombatHitAnimationList = new List<AnimationClass>();
        public List<AnimationClass> RangedRunAttackAnimationList = new List<AnimationClass>();
        public List<AnimationClass> RangedDeathAnimationList = new List<AnimationClass>();

        public AnimationClip Attack1Animation, Attack2Animation, Attack3Animation, Attack4Animation, Attack5Animation, Attack6Animation, RunAttack1Animation, RunAttack2Animation, RunAttack3Animation;
        public AnimationClip Idle1Animation, Idle2Animation, Idle3Animation, Idle4Animation, Idle5Animation, Idle6Animation, IdleWarningAnimation;
        public AnimationClip NonCombatTurnLeftAnimation, NonCombatTurnRightAnimation, CombatTurnLeftAnimation, CombatTurnRightAnimation;
        public AnimationClip NonCombatIdleAnimation, WalkLeftAnimation, WalkStraightAnimation, WalkRightAnimation, RunLeftAnimation, RunStraightAnimation, RunRightAnimation;
        public AnimationClip Emote1Animation, Emote2Animation, Emote3Animation, Emote4Animation, Emote5Animation, Emote6Animation, Emote7Animation, Emote8Animation, Emote9Animation, Emote10Animation;
        public AnimationClip CombatIdleAnimation, CombatWalkLeftAnimation, CombatWalkStraightAnimation, CombatWalkBackAnimation, CombatWalkRightAnimation, CombatRunLeftAnimation, CombatRunStraightAnimation, CombatRunRightAnimation;
        public AnimationClip Hit1Animation, Hit2Animation, Hit3Animation, Hit4Animation, Hit5Animation, Hit6Animation, CombatHit1Animation, CombatHit2Animation, CombatHit3Animation, CombatHit4Animation, 
            CombatHit5Animation, CombatHit6Animation, BlockIdleAnimation, BlockHitAnimation, PutAwayWeaponAnimation, PullOutWeaponAnimation;
        public AnimationClip Death1Animation, Death2Animation, Death3Animation, Death4Animation, Death5Animation, Death6Animation;

        public AnimationClip RangedCombatIdleAnimation, RangedCombatWalkLeftAnimation, RangedCombatWalkStraightAnimation, RangedCombatWalkBackAnimation, RangedCombatWalkRightAnimation, 
            RangedCombatRunLeftAnimation, RangedCombatRunStraightAnimation, RangedCombatRunRightAnimation, RangedIdleWarningAnimation, RangedPutAwayWeaponAnimation, RangedPullOutWeaponAnimation;
        public AnimationClip RangedAttack1Animation, RangedAttack2Animation, RangedAttack3Animation, RangedAttack4Animation, RangedAttack5Animation, RangedAttack6Animation, RangedRunAttack1Animation, 
            RangedRunAttack2Animation, RangedRunAttack3Animation;
        public AnimationClip RangedCombatHit1Animation, RangedCombatHit2Animation, RangedCombatHit3Animation, RangedCombatHit4Animation, RangedCombatHit5Animation, RangedCombatHit6Animation, 
            RangedCombatTurnLeftAnimation, RangedCombatTurnRightAnimation;
        public AnimationClip RangedDeath1Animation, RangedDeath2Animation, RangedDeath3Animation, RangedDeath4Animation, RangedDeath5Animation, RangedDeath6Animation;

        [SerializeField]
        public bool AnimatorControllerGenerated = false;
        public bool AnimationListsChanged = false;
        public string FilePath;
        public bool MissingRuntimeController = false;
        public string EmeraldTag = "Respawn";
        public string UITag = "Player";
        public string RagdollTag = "Untagged";
        public bool AnimationsUpdated = false;

        public float RayCastUpdateSeconds = 0.1f;
        float RayCastUpdateTimer;
        public float ObstructionDetectionUpdateSeconds = 0.1f;
        public float ObstructionDetectionUpdateTimer;
        public int CurrentHealth;
        public int StartingHealth = 15;
        public int DetectionRadius = 18;
        public int CurrentDamageAmount = 5;

        public int DamageAmountRun = 5;
        public int WanderRadius = 25;
        public int TabNumber = 0;
        public int TemperamentTabNumber = 0;
        public int DetectionTagsTabNumber = 0;
        public int AnimationTabNumber = 0;
        public int SoundTabNumber = 0;
        public int MovementTabNumber = 0;
        public int WeaponTypeTabNumber = 0;
        public int WeaponTypeCombatTabNumber = 0;
        public int WeaponTypeControlTabNumber = 0;
        public int AttackAnimationNumber = 1;
        public int RunAttackAnimationNumber = 1;
        public int IdleAnimationNumber = 1;
        public int CombatTabNumber = 0;
        public int FactionsAndTagTabNumber = 0;
        public int EventTabNumber = 0;

        public int MinimumWaitTime = 3;
        public int MaximumWaitTime = 6;

        public int MinimumFollowWaitTime = 1;
        public int MaximumFollowWaitTime = 2;

        public float DeathDelay;
        public bool DeathDelayActive;
        public float DeathDelayTimer;
        public int DeathDelayMin = 1;
        public int DeathDelayMax = 3;
        public int MinimumDamageAmountRun = 2;
        public int MaximumDamageAmountRun = 5;
        public int CritChance = 15;
        public float CritMultiplier = 2;

        public float WalkBackwardsSpeed = 1;
        public float WalkSpeed = 2;
        public float RunSpeed = 5;
        public int ExpandedFieldOfViewAngle = 300;
        public int fieldOfViewAngle = 180;
        public int fieldOfViewAngleRef;
        public int MaxChaseDistance = 30;
        public int AngleNeededToTurn = 10;
        public int AngleNeededToTurnRunning = 30;
        public float TooCloseDistance = 1;
        public float MeleeTooCloseDistance = 1;
        public float RangedTooCloseDistance = 5;
        public float AttackDistance = 2.5f;
        public float MeleeAttackDistance = 2.5f;
        public float RangedAttackDistance = 25;
        public float RunAttackDistance = 3f;
        public int CautiousSeconds = 8;
        public int DeactivateDelay = 5;
        float DeactivateTimer;
        public int SecondsToDisable = 6;
        public float HealthRegRate = 1;
        public int RegenerateAmount = 1;
        public float RegenerateTimer = 0;
        public int AILevel = 1;
        public string AIName = "AI Name";
        public string AITitle = "AI Title";

        public float StoppingDistance = 2;
        public float FollowingStoppingDistance = 5;
        public float DistanceOffset = 1;
        public float DistanceFromTarget;
        public float AgentRadius = 0.5f;
        public float AgentBaseOffset = 0;
        public float AgentAcceleration = 75;
        public float MaxXAngle = 15;
        public float MaxZAngle = 5;
        public int HealthPercentageToFlee = 10;

        public float Idle1AnimationSpeed = 1, Idle2AnimationSpeed = 1, Idle3AnimationSpeed = 1, Idle4AnimationSpeed = 1, Idle5AnimationSpeed = 1, Idle6AnimationSpeed = 1;
        public float IdleWarningAnimationSpeed = 1;
        public float RangedIdleWarningAnimationSpeed = 1;
        public float IdleCombatAnimationSpeed = 1;
        public float RangedIdleCombatAnimationSpeed = 1;
        public float IdleNonCombatAnimationSpeed = 1;
        public float Attack1AnimationSpeed = 1, Attack2AnimationSpeed = 1, Attack3AnimationSpeed = 1, Attack4AnimationSpeed = 1, Attack5AnimationSpeed = 1, Attack6AnimationSpeed = 1;
        public float RangedAttack1AnimationSpeed = 1, RangedAttack2AnimationSpeed = 1, RangedAttack3AnimationSpeed = 1, RangedAttack4AnimationSpeed = 1, RangedAttack5AnimationSpeed = 1, RangedAttack6AnimationSpeed = 1;
        public float RunAttack1AnimationSpeed = 1, RunAttack2AnimationSpeed = 1, RunAttack3AnimationSpeed = 1;
        public float RangedRunAttack1AnimationSpeed = 1, RangedRunAttack2AnimationSpeed = 1, RangedRunAttack3AnimationSpeed = 1;
        public float TurnLeftAnimationSpeed = 1;
        public float TurnRightAnimationSpeed = 1;
        public float CombatTurnLeftAnimationSpeed = 1;
        public float CombatTurnRightAnimationSpeed = 1;
        public float RangedCombatTurnLeftAnimationSpeed = 1;
        public float RangedCombatTurnRightAnimationSpeed = 1;
        public float Death1AnimationSpeed = 1, Death2AnimationSpeed = 1, Death3AnimationSpeed = 1, Death4AnimationSpeed = 1, Death5AnimationSpeed = 1, Death6AnimationSpeed = 1;
        public float RangedDeath1AnimationSpeed = 1, RangedDeath2AnimationSpeed = 1, RangedDeath3AnimationSpeed = 1, RangedDeath4AnimationSpeed = 1, RangedDeath5AnimationSpeed = 1, RangedDeath6AnimationSpeed = 1;
        public float Emote1AnimationSpeed = 1, Emote2AnimationSpeed = 1, Emote3AnimationSpeed = 1, Emote4AnimationSpeed = 1, Emote5AnimationSpeed = 1, Emote6AnimationSpeed = 1, 
            Emote7AnimationSpeed = 1, Emote8AnimationSpeed = 1, Emote9AnimationSpeed = 1, Emote10AnimationSpeed = 1;
        public float WalkAnimationSpeed = 1;
        public float RunAnimationSpeed = 1;
        public float NonCombatWalkAnimationSpeed = 1;
        public float NonCombatRunAnimationSpeed = 1;
        public float CombatWalkAnimationSpeed = 1;
        public float CombatRunAnimationSpeed = 1;
        public float RangedCombatWalkAnimationSpeed = 1;
        public float RangedCombatRunAnimationSpeed = 1;
        public float Hit1AnimationSpeed = 1, Hit2AnimationSpeed = 1, Hit3AnimationSpeed = 1, Hit4AnimationSpeed = 1, Hit5AnimationSpeed = 1, Hit6AnimationSpeed = 1;
        public float CombatHit1AnimationSpeed = 1, CombatHit2AnimationSpeed = 1, CombatHit3AnimationSpeed = 1, CombatHit4AnimationSpeed = 1, CombatHit5AnimationSpeed = 1, CombatHit6AnimationSpeed = 1;
        public float RangedCombatHit1AnimationSpeed = 1, RangedCombatHit2AnimationSpeed = 1, RangedCombatHit3AnimationSpeed = 1, 
            RangedCombatHit4AnimationSpeed = 1, RangedCombatHit5AnimationSpeed = 1, RangedCombatHit6AnimationSpeed = 1;

        public int StartingBehaviorRef;
        public int StartingConfidenceRef;
        public int StartingWanderingTypeRef;

        public Vector3 SingleDestination;

        public Renderer Renderer1;
        public Renderer Renderer2;
        public Renderer Renderer3;
        public Renderer Renderer4;

        public enum DeathType {Animation = 0, Ragdoll};
        public DeathType DeathTypeRef = DeathType.Animation;

        public enum CurrentBehavior { Passive = 0, Cautious = 1, Companion = 2, Aggressive = 3, Pet = 4 };
        public CurrentBehavior BehaviorRef = CurrentBehavior.Passive;

        public enum DetectionType { Trigger = 0, LineOfSight = 1 };
        public DetectionType DetectionTypeRef = DetectionType.Trigger;

        public enum MaxChaseDistanceType { TargetDistance = 0, StartingDistance = 1 };
        public MaxChaseDistanceType MaxChaseDistanceTypeRef = MaxChaseDistanceType.TargetDistance;

        public enum ConfidenceType { Coward = 0, Brave = 1, Foolhardy = 2 };
        public ConfidenceType ConfidenceRef = ConfidenceType.Brave;

        public enum OptimizedState { Active = 0, Inactive = 1 };
        public OptimizedState OptimizedStateRef = OptimizedState.Inactive;

        public enum CurrentDetection { Alert = 0, Unaware = 1 };
        public CurrentDetection CurrentDetectionRef = CurrentDetection.Unaware;

        public enum TargetType { Player = 0, AI = 1, NonAITarget = 2 };
        public TargetType TargetTypeRef = TargetType.Player;

        public enum CombatState { NotActive = 0, Active = 1 };
        public CombatState CombatStateRef = CombatState.NotActive;       

        public enum CombatType { Offensive = 0, Defensive = 1 };
        public CombatType CombatTypeRef = CombatType.Offensive;    

        public enum RefillHealth {Disable = 0, Instantly = 1, OverTime = 2};
        public RefillHealth RefillHealthType = RefillHealth.OverTime;

        public enum WanderType { Dynamic = 0, Waypoints = 1, Stationary = 2, Destination = 3 };
        public WanderType WanderTypeRef = WanderType.Dynamic;

        public enum WaypointType { Loop = 0, Reverse = 1, Random = 2 };
        public WaypointType WaypointTypeRef = WaypointType.Loop;        

        public enum AlignmentQuality { Low = 0, Medium = 1, High = 2 };
        public AlignmentQuality AlignmentQualityRef = AlignmentQuality.Medium;

        public enum ObstructionDetectionQuality { Low = 0, Medium = 1, High = 2 };
        public ObstructionDetectionQuality ObstructionDetectionQualityRef = ObstructionDetectionQuality.Medium;

        public enum ActionAnimation { Eat = 0, Sleep = 1, Talk = 2, Work = 3, Interact = 4 };
        public ActionAnimation ActionAnimationRef = ActionAnimation.Talk;

        public enum PickTargetMethod { Closest = 0, FirstDetected = 1, Random = 2 };
        public PickTargetMethod PickTargetMethodRef = PickTargetMethod.Closest;

        public enum WeaponType { Melee = 0, Ranged = 1, Both = 2};
        public WeaponType WeaponTypeRef = WeaponType.Melee;

        public enum StartingWeaponType { Melee = 0, Ranged = 1};
        public StartingWeaponType StartingWeaponTypeRef = StartingWeaponType.Melee;

        public enum AvoidanceQuality { None = 0, Low = 1, Medium = 2, Good = 3, High = 4 };
        public AvoidanceQuality AvoidanceQualityRef = AvoidanceQuality.Medium;

        public enum TargetObstructedAction { StayStationary = 0, MoveCloserImmediately, MoveCloserAfterSetSeconds };
        public TargetObstructedAction TargetObstructedActionRef = TargetObstructedAction.StayStationary;

        public AnimatorTypeState AnimatorType = AnimatorTypeState.NavMeshDriven;
        public enum AnimatorTypeState { RootMotion, NavMeshDriven };

        public MovementState StartingMovementState = MovementState.Run;
        public MovementState CurrentMovementState = MovementState.Walk;
        public enum MovementState { Walk = 0, Run };

        public BlockingState CurrentBlockingState = BlockingState.NotBlocking;
        public enum BlockingState { Blocking, NotBlocking };

        public BackupType BackupTypeRef = BackupType.Instant;
        public enum BackupType { Off = 0, Instant, Odds };

        public enum AnimatorCullingModes { AlwaysAnimate, CullUpdateTransforms };
        public AnimatorCullingModes AnimatorCullingMode = AnimatorCullingModes.AlwaysAnimate;

        public enum CurrentMeleeAttackTypes { StationaryAttack, RunAttack };
        public CurrentMeleeAttackTypes CurrentMeleeAttackType = CurrentMeleeAttackTypes.StationaryAttack;

        public enum TotalLODsEnum { Two = 0, Three = 1, Four = 2 };
        public TotalLODsEnum TotalLODsRef = TotalLODsEnum.Three;

        public enum IKTypes { UnityIK, EmeraldIK };
        public IKTypes IKType = IKTypes.UnityIK;

        public enum YesOrNo { No = 0, Yes = 1 };
        public YesOrNo UseRandomRotationOnStartRef = YesOrNo.No;
        public YesOrNo DisableAIWhenNotInViewRef = YesOrNo.No;
        public YesOrNo UseDeactivateDelayRef = YesOrNo.No;
        public YesOrNo UseWarningAnimationRef = YesOrNo.No;
        public YesOrNo AlignAIOnStartRef = YesOrNo.No;
        public YesOrNo UseAIAvoidance = YesOrNo.No;
        public YesOrNo UseEquipAnimation = YesOrNo.No;
        public YesOrNo UseHitAnimations = YesOrNo.Yes;
        public YesOrNo UseDroppableWeapon = YesOrNo.No;
        public YesOrNo EnableDebugging = YesOrNo.No;
        public YesOrNo DrawRaycastsEnabled = YesOrNo.Yes;
        public YesOrNo DrawLookAtPointsEnabled = YesOrNo.Yes;
        public YesOrNo DebugLogTargetsEnabled = YesOrNo.Yes;
        public YesOrNo DebugLogObstructionsEnabled = YesOrNo.Yes;
        public YesOrNo DebugLogProjectileCollisionsEnabled = YesOrNo.Yes;
        public YesOrNo UseAggro = YesOrNo.Yes;
        public YesOrNo EnableBothWeaponTypes = YesOrNo.No;
        public YesOrNo UseAINameUIOutlineEffect = YesOrNo.Yes;
        public YesOrNo UseAILevelUIOutlineEffect = YesOrNo.Yes;
        public YesOrNo UseCustomFontAIName = YesOrNo.No;
        public YesOrNo UseCustomFontAILevel = YesOrNo.No;   
        public YesOrNo AlignAIWithGroundRef = YesOrNo.No;
        public YesOrNo CreateHealthBarsRef = YesOrNo.No;
        public YesOrNo UseNonAITagRef = YesOrNo.No;
        public YesOrNo UseMagicEffectsPackRef = YesOrNo.No;
        public YesOrNo RandomizeDamageRef = YesOrNo.Yes;
        public YesOrNo CustomizeHealthBarRef = YesOrNo.No;
        public YesOrNo UseCustomFontRef = YesOrNo.No;
        public YesOrNo DisplayAINameRef = YesOrNo.No;
        public YesOrNo DisplayAITitleRef = YesOrNo.No;
        public YesOrNo DisplayAILevelRef = YesOrNo.No;
        public YesOrNo HasMultipleLODsRef = YesOrNo.No;
        public YesOrNo UseHitEffect = YesOrNo.No;
        public YesOrNo UseRunAttacks = YesOrNo.No;
        public YesOrNo UseHeadLookRef = YesOrNo.No;
        public YesOrNo UseBlockingRef = YesOrNo.No;
        public YesOrNo UseCriticalHits = YesOrNo.No;
        public YesOrNo DebugLogMissingAnimationsRef = YesOrNo.Yes;
        public YesOrNo SummonsMultipleAIRef = YesOrNo.No;
        public YesOrNo AttackOnArrivalRef = YesOrNo.Yes;
        public YesOrNo NonCombatAI = YesOrNo.No;

        public Transform RangedAttackTransform;
        public Transform CurrentTarget;
        public Transform CurrentFollowTarget;

        public string PlayerTag = "Player";
        public string FollowTag = "Player";
        public string NonAITag = "Water";

        public bool ReturningToStartInProgress = false;
        public bool AIReachedDestination = false;
        public bool AIAgentActive = false;

        public bool HealthBarsFoldout = true;
        public bool CombatTextFoldout = true;
        public bool NameTextFoldout = true;
        public bool WaypointsFoldout = true;

        public bool WalkFoldout = true;
        public bool RunFoldout = true;
        public bool TurnFoldout = true;
        public bool CombatWalkFoldout = true;
        public bool CombatRunFoldout = true;
        public bool CombatTurnFoldout = true;

        public bool BehaviorFoldout = true;
        public bool ConfidenceFoldout = true;
        public bool WanderFoldout = true;
        public bool CombatStyleFoldout = true;
        public bool CurrentlyPlayingActionAnimation = false;
        public bool DamageSoundInhibitor = false;
        public bool DamageEffectInhibitor = false;

        public AudioClip SheatheWeapon;
        public AudioClip UnsheatheWeapon;
        public AudioClip RangedSheatheWeapon;
        public AudioClip RangedUnsheatheWeapon;
        public List<AudioClip> IdleSounds = new List<AudioClip>();
        public List<AudioClip> AttackSounds = new List<AudioClip>();
        public List<AudioClip> InjuredSounds = new List<AudioClip>();
        public List<AudioClip> WarningSounds = new List<AudioClip>();
        public List<AudioClip> DeathSounds = new List<AudioClip>();
        public List<AudioClip> FootStepSounds = new List<AudioClip>();
        public List<AudioClip> BlockingSounds = new List<AudioClip>();
        public List<AudioClip> ImpactSounds = new List<AudioClip>();
        public List<AudioClip> CriticalHitSounds = new List<AudioClip>();

        [SerializeField]
        public List<int> AIFactionsList = new List<int>();
        public int TargetTagsIndex;

        public Vector3 BloodPosOffset;
        public enum BloodEffectPositionType {BaseTransform = 0, HitTransform};
        public BloodEffectPositionType BloodEffectPositionTypeRef = BloodEffectPositionType.HitTransform;

        public GameObject HealthBarCanvas;
        public Sprite HealthBarImage;
        public Sprite HealthBarBackgroundImage;
        public Vector3 HealthBarPos = new Vector3(0, 1.75f, 0);
        public Vector3 CombatTextPos = new Vector3(0, 1.75f, 0);
        public Color HealthBarColor = new Color32(197, 41, 41, 255);
        public Color HealthBarBackgroundColor = new Color32(36, 36, 36, 255);
        public Color NameTextColor = new Color32(255, 255, 255, 255);
        public Color LevelTextColor = new Color32(255, 255, 255, 255);
        public Vector3 HealthBarScale = new Vector3(0.75f, 1, 1);
        public int NameTextFontSize = 7;
        public GameObject WaypointParent;
        public string WaypointOrigin;
        public Vector3 AINamePos = new Vector3(0, 3, 0);
        public Vector3 AILevelPos = new Vector3(1.5f, 0, 0);

        public bool FirstTimeInCombat = true;
        public bool WarningAnimationTriggered = false;
        public bool TargetObstructed = false;

        float CurrentHealthFloat;
        float CurrentVelocity;
        public float WaitTime = 5;
        public int DamageReceived;

        public GameObject DroppableMeleeWeapon;
        public GameObject DroppableRangedWeapon;
        public Transform LastAttacker;
        public Transform LookAtTarget;
        public Transform LineOfSightRef;
        public Transform PassiveTargetRef;
        public Transform CompanionTargetRef;
        public NavMeshPath AIPath;
        public Animator AIAnimator;
        public Vector3 StartingDestination;
        public float CautiousTimer;
        public BoxCollider AIBoxCollider;
        public Renderer AIRenderer;
        public EmeraldAISystem TargetEmerald;
        public GameObject HealthBar;
        public int MaxUIScaleSize = 16;
        public Text AINameUI;
        public Font AINameFont;
        public float AINameLineSpacing = 0.75f;
        public Vector2 AINameUIOutlineSize = new Vector2(0.35f, -0.35f);
        public Color AINameUIOutlineColor = Color.black;
        public Text AILevelUI;
        public Font AILevelFont;
        public Vector2 AILevelUIOutlineSize = new Vector2(0.35f, -0.35f);
        public Color AILevelUIOutlineColor = Color.black;
        public Canvas HealthBarCanvasRef;
        public List<Vector3> WaypointsList = new List<Vector3>();
        public int WaypointIndex = 0;
        int m_LastWaypointIndex = 0;
        public GameObject WaypointHolder;
        Vector3 TurnDirection;
        Vector3 TurnDirectionOther;
        Quaternion TargetDirection;
        NavMeshHit NMH;
        GameObject HitObject;
        public Collider[] CollidersInArea;
        public int ReceivedFaction;
        public Vector3 distanceBetween;
        public string StartingTag;
        public int StartingLayer;
        public AnimationProfile m_AnimationProfile;
        public EmeraldAIWaypointObject m_WaypointObject;
        public bool FinalImpactSound;
        bool WaypointReverseActive;

        public int NonCombatAILayerIndex = 2;
        public LayerMask DetectionLayerMask = 3;
        public LayerMask ObstructionDetectionLayerMask = 4;
        public LayerMask UILayerMask = 0;
        public LayerMask AlignmentLayerMask = 1;
        public LayerMask AIAvoidanceLayerMask;
        public LayerMask DynamicWanderLayerMask = ~0;
        public LayerMask BackupLayerMask = ~0;
        #endregion

        //Initialize Emerald AI and its components
        void Awake()
        {
            EmeraldInitializerComponent = GetComponent<EmeraldAIInitializer>();
            EmeraldInitializerComponent.Initialize();
        }

        /// <summary>
        /// Check our AI's path to ensure if it is reachable. If it isn't, regenerate, depending on the Wander Type.
        /// </summary>
        public void CheckPath(Vector3 Destination)
        {
            NavMeshPath path = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(Destination, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Path is valid
            }
            else if (path.status == NavMeshPathStatus.PathPartial)
            {
                if (WanderTypeRef == WanderType.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderTypeRef = WanderType.Stationary;
                }
                else if (WanderTypeRef == WanderType.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderTypeRef = WanderType.Stationary;
                }
            }
            else if (path.status == NavMeshPathStatus.PathInvalid)
            {
                if (WanderTypeRef == WanderType.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderTypeRef = WanderType.Stationary;
                }
                else if (WanderTypeRef == WanderType.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderTypeRef = WanderType.Stationary;
                }
            }
            else
            {
                Debug.Log("Path Invalid");
            }
        }

        void CheckAIRenderers()
        {
            if (OptimizedStateRef == OptimizedState.Inactive)
            {
                if (!Renderer1.isVisible && !Renderer2.isVisible && TotalLODsRef == TotalLODsEnum.Two)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (UseDeactivateDelayRef == YesOrNo.Yes && DeactivateTimer >= DeactivateDelay || UseDeactivateDelayRef == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!Renderer1.isVisible && !Renderer2.isVisible && !Renderer3.isVisible && TotalLODsRef == TotalLODsEnum.Three)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (UseDeactivateDelayRef == YesOrNo.Yes && DeactivateTimer >= DeactivateDelay || UseDeactivateDelayRef == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!Renderer1.isVisible && !Renderer2.isVisible && !Renderer3.isVisible && !Renderer4.isVisible && TotalLODsRef == TotalLODsEnum.Four)
                {
                    DeactivateTimer += Time.deltaTime;
                    
                    if (UseDeactivateDelayRef == YesOrNo.Yes && DeactivateTimer >= DeactivateDelay || UseDeactivateDelayRef == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
            }
            else if (OptimizedStateRef == OptimizedState.Active)
            {
                if (TotalLODsRef == TotalLODsEnum.Two)
                {
                    if (Renderer1.isVisible || Renderer2.isVisible)
                    {
                        Activate();
                    }
                }
                else if (TotalLODsRef == TotalLODsEnum.Three)
                {
                    if (Renderer1.isVisible || Renderer2.isVisible || Renderer3.isVisible)
                    {
                        Activate();
                    }
                }
                else if (TotalLODsRef == TotalLODsEnum.Four)
                {
                    if (Renderer1.isVisible || Renderer2.isVisible || Renderer3.isVisible || Renderer4.isVisible)
                    {
                        Activate();
                    }
                }
            }
        }

        void Update()
        {
            TimeSinceStart += Time.deltaTime;

            if (DisableAIWhenNotInViewRef == YesOrNo.Yes && HasMultipleLODsRef == YesOrNo.Yes)
            {
                CheckAIRenderers();
            }

            if (OptimizedStateRef == OptimizedState.Inactive || DisableAIWhenNotInViewRef == YesOrNo.No)
            {
                CurrentStateInfo = AIAnimator.GetCurrentAnimatorStateInfo(0);

                //Use CustomRotation
                CustomRotation();

                if (IsMoving)
                {
                    IsTurning = false;
                    if (CombatStateRef == CombatState.NotActive)
                        CurrentMovingTurnSpeed = MovingTurnSpeedNonCombat;
                    else if (CombatStateRef == CombatState.Active)
                        CurrentMovingTurnSpeed = MovingTurnSpeedCombat;
                }
                else
                {
                    m_NavMeshAgent.angularSpeed = StationaryTurningSpeedNonCombat;
                }

                if (CombatStateRef == CombatState.Active && !DeathDelayActive)
                {
                    UpdateWeaponTypeState();
                }

                if (DeathDelayActive)
                {
                    DeathDelayTimer += Time.deltaTime;

                    if (DeathDelayTimer > DeathDelay)
                    {                       
                        EmeraldBehaviorsComponent.DefaultState();
                    }
                }

                AIAgentActive = m_NavMeshAgent.enabled;
                AggroDelay += Time.deltaTime;

                EmeraldBehaviorsComponent.CheckAnimationStates();

                //If our AI is not in combat, wander according to its Wander Type.
                if (AIAgentActive && CombatStateRef == CombatState.NotActive && !DeathDelayActive && BehaviorRef != CurrentBehavior.Companion && BehaviorRef != CurrentBehavior.Pet)
                {        
                    Wander();
                }
                else if (AIAgentActive && CombatStateRef == CombatState.NotActive && !DeathDelayActive)
                {                 
                    if (BehaviorRef == CurrentBehavior.Companion || BehaviorRef == CurrentBehavior.Pet)
                    {
                        if (CurrentFollowTarget != null)
                        {
                            FollowCompanionTarget();
                        }
                        else
                        {
                            Wander();
                        }
                    }
                }
                else
                {
                    if (m_NavMeshAgent.hasPath && BehaviorRef == CurrentBehavior.Cautious && ConfidenceRef > 0)
                    {
                        m_NavMeshAgent.ResetPath();
                    }
                }

                //If our AI is not moving, align it with the current surface
                if (!IsMoving && !IsDead && !RotateTowardsTarget)
                {
                    AlignAIStationary();
                }

                //Healing cool down to avoid an AI healing too often
                if (HealingCooldownActive)
                {
                    HealingCooldownTimer += Time.deltaTime;

                    if (HealingCooldownTimer >= HealingCooldownSeconds && AttackTimer < AttackSpeed)
                    {
                        HealingCooldownTimer = 0;
                        HealingCooldownActive = false;
                    }
                }

                //Controls looking angle based on non-combat or during combat
                if (AIAgentActive)
                {
                    if (CombatStateRef == CombatState.NotActive)
                    {
                        AngleToTurn = NonCombatAngleToTurn;
                        LookAtLimit = NonCombatLookAtLimit;
                        AlignmentSpeed = NonCombatAlignmentSpeed;
                        WalkAnimationSpeed = NonCombatWalkAnimationSpeed;
                        RunAnimationSpeed = NonCombatRunAnimationSpeed;
                    }
                    else if (CombatStateRef == CombatState.Active)
                    {
                        AngleToTurn = CombatAngleToTurn;
                        LookAtLimit = CombatLookAtLimit;
                        AlignmentSpeed = CombatAlignmentSpeed;
                        WalkAnimationSpeed = CombatWalkAnimationSpeed;
                        RunAnimationSpeed = CombatRunAnimationSpeed;
                    }
                }

                //Handles our AI's Aggressive Behavior using the EmeraldBehaviorsComponent
                if (BehaviorRef == CurrentBehavior.Aggressive && CombatStateRef == CombatState.Active && AIAgentActive)
                {
                    EmeraldBehaviorsComponent.AggressiveBehavior();
                }
                //Handles our AI's Coward Behavior using the EmeraldBehaviorsComponent
                else if (BehaviorRef == CurrentBehavior.Cautious && ConfidenceRef == ConfidenceType.Coward && CombatStateRef == CombatState.Active && AIAgentActive)
                {
                    EmeraldBehaviorsComponent.CowardBehavior();
                }
                //Handles our AI's Cautious Behavior using the EmeraldBehaviorsComponent
                else if (BehaviorRef == CurrentBehavior.Cautious && ConfidenceRef != ConfidenceType.Coward && CombatStateRef == CombatState.Active && AIAgentActive)
                {
                    EmeraldBehaviorsComponent.CautiousBehavior();
                }
                //Handles our AI's Companion Behavior using the EmeraldBehaviorsComponent
                else if (BehaviorRef == CurrentBehavior.Companion && AIAgentActive)
                {
                    EmeraldBehaviorsComponent.CompanionBehavior();
                }

                //Calculates an AI's movement speed when using Root Motion
                if (AnimatorType == AnimatorTypeState.RootMotion && !IsDead)
                {
                    MoveAIRootMotion();
                }
                //Calculates an AI's movement speed when using NavMesh
                else if (AnimatorType == AnimatorTypeState.NavMeshDriven && !IsDead)
                {
                    MoveAINavMesh();
                }

                //If our target is obstructed for 3 seconds, search for a new closer target which will most likely be the object obstructing the AI's LOS.
                if (TargetObstructed && WeaponTypeRef == WeaponType.Ranged && CombatStateRef == CombatState.Active)
                {
                    if (EmeraldDetectionComponent.CurrentObstruction != null && EmeraldDetectionComponent.CurrentObstruction.CompareTag(EmeraldTag))
                    {
                        m_ObstructedTimer += Time.deltaTime;

                        if (m_ObstructedTimer >= 2.5f)
                        {
                            int m_ReceivedFaction = EmeraldDetectionComponent.CurrentObstruction.GetComponent<EmeraldAISystem>().CurrentFaction;
                            if (AIFactionsList.Contains(m_ReceivedFaction) && FactionRelations[AIFactionsList.IndexOf(m_ReceivedFaction)] == 0)
                            {
                                EmeraldDetectionComponent.SearchForTarget();
                            }
                            else
                            {
                                EmeraldDetectionComponent.SearchForRandomTarget = true;
                                EmeraldDetectionComponent.SearchForTarget();
                            }                           
                            
                            m_ObstructedTimer = 0;
                        }
                    }
                }
                else
                {
                    m_ObstructedTimer = 0;
                }
            }
        }

        //Controls the AI's weapon type switching from happening too often
        void WeaponSwitchCooldown ()
        {
            m_WeaponTypeSwitchDelay = false;
        }

        /// <summary>
        /// Updates the AI's weapon type to switch between ranged and melee.
        /// </summary>
        void UpdateWeaponTypeState ()
        {
            if (SwitchWeaponTypeTriggered && !m_WeaponTypeSwitchDelay)
            {
                SwitchWeaponTypeTriggered = false;
                m_WeaponTypeSwitchDelay = true;
                Invoke("WeaponSwitchCooldown", SwitchWeaponTypesCooldown);
            }

            if (EnableBothWeaponTypes == YesOrNo.Yes)
            {
                //Switches the current weapon type based on distance.
                if (SwitchWeaponType == SwitchWeaponTypes.Distance && CurrentTarget != null && !m_WeaponTypeSwitchDelay && !IsSwitchingWeapons && !IsAttacking && !IsMoving && !IsBackingUp && !IsTurning)
                {
                    if (CurrentTarget != null && Vector3.Distance(CurrentTarget.position, transform.position) > SwitchWeaponTypesDistance && WeaponTypeRef == WeaponType.Melee && !SwitchWeaponTypeTriggered)
                    {
                        SwitchWeaponCoroutine = StartCoroutine(SwitchCurrentWeaponType("Ranged")); //Switch to the Ranged Weapon Type
                        SwitchWeaponTypeTriggered = true;
                    }

                    if (CurrentTarget != null && Vector3.Distance(CurrentTarget.position, transform.position) <= SwitchWeaponTypesDistance && WeaponTypeRef == WeaponType.Ranged && !SwitchWeaponTypeTriggered)
                    {
                        //Fade out the look at controller.
                        if (UseHeadLookRef == YesOrNo.Yes && EmeraldLookAtComponent.BodyWeight > 0)
                            EmeraldLookAtComponent.FadeOutBodyIK();

                        SwitchWeaponCoroutine = StartCoroutine(SwitchCurrentWeaponType("Melee")); //Switch to the Melee Weapon Type
                        SwitchWeaponTypeTriggered = true;
                    }
                }
                //Switches the current weapon type based on a random time of SwitchWeaponTimeMin and SwitchWeaponTimeMax.
                else if (SwitchWeaponType == SwitchWeaponTypes.Timed)
                {
                    if (!IsSwitchingWeapons && !IsEquipping && !IsMoving)
                        SwitchWeaponTimer += Time.deltaTime;

                    if (CurrentTarget != null && SwitchWeaponTimer >= SwitchWeaponTime && !IsSwitchingWeapons && !IsAttacking && !IsMoving && !IsBackingUp && !IsTurning)
                    {
                        if (WeaponTypeRef == WeaponType.Melee)
                            SwitchWeaponCoroutine = StartCoroutine(SwitchCurrentWeaponType("Ranged")); //Switch to the Ranged Weapon Type
                        else if (WeaponTypeRef == WeaponType.Ranged)
                        {
                            //Fade out the look at controller.
                            if (UseHeadLookRef == YesOrNo.Yes && EmeraldLookAtComponent.BodyWeight > 0)
                                EmeraldLookAtComponent.FadeOutBodyIK();
                            SwitchWeaponCoroutine = StartCoroutine(SwitchCurrentWeaponType("Melee")); //Switch to the Melee Weapon Type
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Switches the Weapon Type based on the WeaponTypeName parameter.
        /// </summary>
        IEnumerator SwitchCurrentWeaponType(string WeaponTypeName)
        {
            IsSwitchingWeapons = true;
            yield return new WaitForSeconds(0.5f);
            EmeraldEventsManagerComponent.CancelAttackAnimation();

            SwitchWeaponTime = Random.Range((float)SwitchWeaponTimeMin, (float)SwitchWeaponTimeMax +1);
            SwitchWeaponTimer = 0;
            BackingUpTimer = 0;
            AIAnimator.SetBool("Blocking", false);
            CurrentAnimationIndex = 1;
            AIAnimator.SetInteger("Attack Index", 1);

            if (ReverseRangedWalkAnimation || ReverseWalkAnimation)
                AIAnimator.SetFloat("Backup Speed", -1);
            else
                AIAnimator.SetFloat("Backup Speed", 1);

            //Switching to Melee
            if (WeaponTypeName == "Melee")
            {
                yield return new WaitForSeconds(0.1f);

                while (!CurrentStateInfo.IsName("Combat Movement (Ranged)") || IsTurning || IsBackingUp)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);

                if (DeathDelayActive)
                {
                    IsSwitchingWeapons = false;
                    yield break;
                }

                AIAnimator.SetInteger("Weapon Type State", 0);

                //Fade out the AI's hand IK if it was active
                if (EmeraldHandIKComp != null && EmeraldHandIKComp.RightHandPosWeight > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                    EmeraldHandIKComp.FadeOutHandWeights();
                }

                yield return new WaitForSeconds(1f);

                while (IsEquipping)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);
                WeaponTypeRef = WeaponType.Melee;

                //Add the new weapon type's settings.
                AIAnimator.ResetTrigger("Attack");
                AttackDistance = MeleeAttackDistance;
                AttackSpeed = Random.Range((float)MinMeleeAttackSpeed, MaxMeleeAttackSpeed + 1);
                TooCloseDistance = MeleeTooCloseDistance;
                m_NavMeshAgent.stoppingDistance = MeleeAttackDistance;
            }
            else if (WeaponTypeName == "Ranged")
            {
                yield return new WaitForSeconds(0.1f);

                while (!CurrentStateInfo.IsName("Combat Movement") || IsTurning || IsBackingUp)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);

                if (DeathDelayActive)
                {
                    IsSwitchingWeapons = false;
                    yield break;
                }

                AIAnimator.SetInteger("Weapon Type State", 1);

                yield return new WaitForSeconds(1f);

                while (IsEquipping)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);
                WeaponTypeRef = WeaponType.Ranged;

                //Add the new weapon type's settings.
                AIAnimator.ResetTrigger("Attack");
                AttackDistance = RangedAttackDistance;
                AttackSpeed = Random.Range((float)MinRangedAttackSpeed, MaxRangedAttackSpeed + 1);
                TooCloseDistance = RangedTooCloseDistance;
                m_NavMeshAgent.stoppingDistance = RangedAttackDistance;
            }

            IsSwitchingWeapons = false;
        }

        /// <summary>
        /// Moves our AI using Unity's NavMesh Agent 
        /// </summary>
        void MoveAINavMesh ()
        {
            float speed = m_NavMeshAgent.desiredVelocity.magnitude;
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for NavMesh movement.
            if (AIAgentActive && !IsBackingUp)
            {
                AIAnimator.SetFloat("Direction", angle, DirectionDampTime, Time.deltaTime);
                AlignAIMoving();

                if (IsAttacking || IsEquipping || IsEmoting || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && !m_NavMeshAgent.pathPending || IsBlocking || IsTurning)
                {
                    AIAnimator.SetFloat("Speed", 0, 0.3f, Time.deltaTime);
                    m_NavMeshAgent.speed = 0;
                }
                else if (m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance && !m_NavMeshAgent.pathPending && !IsGettingHit && !IsAttacking && !IsSwitchingWeapons && !IsBackingUp && !IsEmoting && !IsEquipping && !IsBlocking && !IsTurning)
                {
                    AIAnimator.SetBool("Idle Active", false);
                    AIAnimator.SetFloat("Speed", speed, 0.3f, Time.deltaTime);

                    if (CurrentMovementState == MovementState.Run)
                    {
                        m_NavMeshAgent.speed = RunSpeed;
                    }
                    else if (CurrentMovementState == MovementState.Walk)
                    {
                        m_NavMeshAgent.speed = WalkSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// Moves our AI when using Root Motion
        /// </summary>
        void MoveAIRootMotion()
        {
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for Root Motion
            if (AIAgentActive)
            {
                AIAnimator.SetFloat("Direction", angle * MovementTurningSensitivity, DirectionDampTime, Time.deltaTime);
                AlignAIMoving();

                if (IsAttacking || IsEquipping || IsEmoting || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance || IsBlocking || IsTurning)
                {
                    DelayTimer = 0;
                    AIAnimator.SetFloat("Speed", 0, 0.2f, Time.deltaTime);
                    m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, 0f, Time.deltaTime * 30);

                    if (!ReachedDestination && !ReturningToStartInProgress && !IsTurning)
                    {
                        if (CombatStateRef == CombatState.NotActive && WanderTypeRef == WanderType.Dynamic)
                        {
                            m_NavMeshAgent.destination = transform.position;
                            ReachedDestination = true;
                        }
                    }
                }
                else if (m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance  && !IsGettingHit && !IsAttacking && !IsSwitchingWeapons && !IsBackingUp && !IsEmoting && !IsEquipping && !IsBlocking && !IsTurning)
                {
                    DelayTimer = Mathf.Lerp(DelayTimer, 1, Time.deltaTime);

                    if (DelayTimer >= 0.2f)
                    {
                        ReachedDestination = false;
                        m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, 0.06f, Time.deltaTime * 3);
                        AIAnimator.SetBool("Idle Active", false);

                        //Force walk movement if getting close to an AI's destination. This helps prevent the AI from running into its target as Root Motion with NavMesh needs a bit of time to stop.
                        if (m_NavMeshAgent.remainingDistance < (m_NavMeshAgent.stoppingDistance + ForceWalkDistance) && BehaviorRef != CurrentBehavior.Cautious && ConfidenceRef != ConfidenceType.Coward)
                        {
                            AIAnimator.SetFloat("Speed", 0.1f, 0.3f, Time.deltaTime);
                        }
                        else
                        {
                            if (CurrentMovementState == MovementState.Run)
                            {
                                AIAnimator.SetFloat("Speed", 1f, 2f, Time.deltaTime);
                            }
                            else if (CurrentMovementState == MovementState.Walk)
                            {
                                AIAnimator.SetFloat("Speed", 0.1f, 0.4f, Time.deltaTime);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uses a custom solution for rotating the AI while moving as it allows for much better turning control and increased speed if needed.
        /// </summary>
        void CustomRotation()
        {
            if (!IsMoving)
                return;

            m_NavMeshAgent.updateRotation = false;
            Vector3 SteeringDirection = (m_NavMeshAgent.steeringTarget - transform.position).normalized;

            if (SteeringDirection != Vector3.zero)
            {
                Quaternion SteeringRotation = Quaternion.LookRotation(SteeringDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, SteeringRotation.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime * CurrentMovingTurnSpeed);
            }
        }

        /// <summary>
        /// Aligns our AI to the current surface while moving
        /// </summary>
        void AlignAIMoving()
        {
            if (m_NavMeshAgent.desiredVelocity.magnitude >= 0.025f)
            {
                if (!IsMoving)
                {
                    AIAnimator.SetBool("Turn Right", false);
                    AIAnimator.SetBool("Turn Left", false);
                    AIAnimator.ResetTrigger("Hit");
                }

                IsMoving = true;

                if (AlignAIWithGroundRef == YesOrNo.Yes)
                {
                    RayCastUpdateTimer += Time.deltaTime;

                    if (RayCastUpdateTimer >= RayCastUpdateSeconds)
                    {
                        RaycastHit HitDown;
                        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), -transform.up, out HitDown, 2, AlignmentLayerMask))
                        {
                            if (HitDown.transform != this.transform)
                            {
                                SurfaceNormal = HitDown.normal;
                                SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -MaxNormalAngle, MaxNormalAngle);
                                SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -MaxNormalAngle, MaxNormalAngle);
                                RayCastUpdateTimer = 0;
                            }
                        }
                    }

                    NormalRotation = Quaternion.FromToRotation(transform.up, SurfaceNormal) * transform.rotation;
                    float angle = Quaternion.Angle(transform.rotation, NormalRotation);

                    //Only align the AI if the angle threshold is greater than 1.
                    if (angle > 1f)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, NormalRotation, Time.deltaTime * AlignmentSpeed);
                    }
                }
            }
            else
            {
                IsMoving = false;
            }
        }

        /// <summary>
        /// Aligns our AI to the current surface while stationary
        /// </summary>
        void AlignAIStationary()
        {
            RayCastUpdateTimer += Time.deltaTime;

            if (CombatStateRef == CombatState.Active && CurrentTarget && ConfidenceRef != ConfidenceType.Coward)
            {
                if (RayCastUpdateTimer >= RayCastUpdateSeconds && AlignAIWithGroundRef == YesOrNo.Yes)
                {
                    RaycastHit HitDown;
                    if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), -transform.up, out HitDown, 2, AlignmentLayerMask))
                    {
                        if (HitDown.transform != this.transform)
                        {
                            SurfaceNormal = HitDown.normal;
                            SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -MaxNormalAngle, MaxNormalAngle);
                            SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -MaxNormalAngle, MaxNormalAngle);
                            RayCastUpdateTimer = 0;
                        }
                    }
                }
                RotateAIStationary();
            }
            else if (CombatStateRef == CombatState.NotActive || ConfidenceRef == ConfidenceType.Coward)
            {
                //Once our AI has returned to its stantionary positon, adjust its position so it rotates to its original rotation.
                if (ReturnToStationaryPosition && AIAgentActive && m_NavMeshAgent.remainingDistance <= StoppingDistance)
                {
                    ReturnToStationaryPosition = false;
                }

                RotateAIStationary();
            }
        }

        /// <summary>
        /// Rotates our AI towards its target or destination
        /// </summary>
        void RotateAIStationary()
        {
            if (CombatStateRef == CombatState.Active && CurrentTarget && ConfidenceRef != ConfidenceType.Coward && !IsGettingHit)
            {
                //Get the angle between the current target and the AI. If using the alignment feature,
                //adjust the angle to include the rotation difference of the AI's current surface angle.
                Vector3 Direction = new Vector3(CurrentTarget.position.x, 0, CurrentTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                float angle = Vector3.Angle(transform.forward, Direction);
                DestinationDirection = Direction;

                if (AlignAIWithGroundRef == YesOrNo.Yes)
                {
                    float RotationDifference = transform.localEulerAngles.x;
                    RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
                    DestinationAdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
                }
                else if (AlignAIWithGroundRef == YesOrNo.No)
                {
                    DestinationAdjustedAngle = Mathf.Abs(angle);
                }

                if (DestinationAdjustedAngle >= AngleToTurn && DestinationDirection != Vector3.zero && !IsAttacking)
                {
                    if (AlignAIWithGroundRef == YesOrNo.Yes)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        Quaternion qGround = Quaternion.FromToRotation(Vector3.up, SurfaceNormal) * qTarget;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qGround, Time.deltaTime * StationaryTurningSpeedCombat);
                    }
                    else if (AlignAIWithGroundRef == YesOrNo.No)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qTarget, Time.deltaTime * StationaryTurningSpeedCombat);
                    }
                }
            }
            else //Non-Comabt rotations
            {
                //Get the angle between the current destination and the AI. If using the alignment feature,
                //adjust the angle to include the rotation difference of the AI's current surface angle.
                Vector3 Direction = new Vector3(m_NavMeshAgent.destination.x, 0, m_NavMeshAgent.destination.z) - new Vector3(transform.position.x, 0, transform.position.z);
                float angle = Vector3.Angle(transform.forward, Direction);
                DestinationDirection = Direction;

                if (AlignAIWithGroundRef == YesOrNo.Yes)
                {
                    float RotationDifference = transform.localEulerAngles.x;
                    RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
                    DestinationAdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
                }
                else if (AlignAIWithGroundRef == YesOrNo.No)
                {
                    DestinationAdjustedAngle = Mathf.Abs(angle);
                }

                if (DestinationAdjustedAngle >= AngleToTurn && DestinationDirection != Vector3.zero && !RotateTowardsTarget && IsTurning && !IsEmoting)
                {
                    if (AlignAIWithGroundRef == YesOrNo.Yes)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        Quaternion qGround = Quaternion.FromToRotation(Vector3.up, SurfaceNormal) * qTarget;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qGround, Time.deltaTime * StationaryTurningSpeedNonCombat);
                    }
                    else if (AlignAIWithGroundRef == YesOrNo.No)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qTarget, Time.deltaTime * StationaryTurningSpeedNonCombat);
                    }

                    TurnDirectionMet = false;
                }
                else
                {
                    if (!TurnDirectionMet && !LockTurning && CombatStateRef == CombatState.NotActive|| !TurnDirectionMet && CombatStateRef == CombatState.Active)
                    {
                        IsTurning = false;
                        IsTurningLeft = false;
                        IsTurningRight = false;
                        AIAnimator.SetBool("Idle Active", false);
                        AIAnimator.SetBool("Turn Left", false);
                        AIAnimator.SetBool("Turn Right", false);
                        LockTurning = true;
                        TurnDirectionMet = true;
                    }
                }
            }

            if (CombatStateRef == CombatState.NotActive && DestinationAdjustedAngle >= AngleToTurn && DestinationDirection != Vector3.zero && AIAgentActive &&  m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance
                || CombatStateRef == CombatState.Active && DestinationAdjustedAngle >= AngleToTurn && DestinationDirection != Vector3.zero && AIAgentActive && !IsAttacking && !IsGettingHit)
            {
                if (TimeSinceStart < 0.5f || LockTurning && CombatStateRef == CombatState.NotActive)
                    return;

                Vector3 cross = Vector3.Cross(transform.forward, Quaternion.LookRotation(DestinationDirection, Vector3.up) * Vector3.forward);

                if (cross.y > 0f) //Right
                {
                    IsTurning = true;
                    IsTurningRight = true;
                    IsTurningLeft = false;
                    AIAnimator.SetBool("Idle Active", false);
                    AIAnimator.SetBool("Turn Right", true);
                    AIAnimator.SetBool("Turn Left", false);
                    m_NavMeshAgent.angularSpeed = StationaryTurningSpeedNonCombat;
                }
                else if (cross.y < 0f) //Left
                {
                    IsTurning = true;
                    IsTurningLeft = true;
                    IsTurningRight = false;
                    AIAnimator.SetBool("Idle Active", false);
                    AIAnimator.SetBool("Turn Left", true);
                    AIAnimator.SetBool("Turn Right", false);
                    m_NavMeshAgent.angularSpeed = StationaryTurningSpeedNonCombat;
                }
                
            }
            else if (CombatStateRef == CombatState.Active)
            {
                IsTurning = false;
                IsTurningLeft = false;
                IsTurningRight = false;
                AIAnimator.SetBool("Turn Left", false);
                AIAnimator.SetBool("Turn Right", false);
            }
        }

        /// <summary>
        /// Generates a random waypoint, once an AI reaches its destination
        /// </summary>
        public void Wander ()
        {
            if (WanderTypeRef == WanderType.Dynamic && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && !m_NavMeshAgent.pathPending && !IsEquipping)
            {
                if (WaypointTimer == 0)
                {
                    AIAnimator.SetBool("Idle Active", true);
                }
                
                WaypointTimer += Time.deltaTime;

                if (WaypointTimer >= WaitTime)
                {
                    AIAnimator.SetBool("Idle Active", false);
                    GenerateWaypoint();
                    if (!m_IdleAnimaionIndexOverride)
                    {
                        AIAnimator.SetInteger("Idle Index", Random.Range(1, TotalIdleAnimations + 1));
                    }

                    WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                    WaypointTimer = 0;
                }
            }
            else if (WanderTypeRef == WanderType.Destination && m_NavMeshAgent.destination != SingleDestination && !AIReachedDestination && !IsEquipping)
            {
                if (m_NavMeshAgent.remainingDistance <= StoppingDistance && !m_NavMeshAgent.pathPending && m_NavMeshAgent.velocity.magnitude != 0)
                {
                    AIReachedDestination = true;
                    LockTurning = false;
                    ReachedDestinationEvent.Invoke();
                }
            }
            else if (WanderTypeRef == WanderType.Waypoints && !WaypointReverseActive && m_NavMeshAgent.destination != WaypointsList[WaypointIndex] && !IsEquipping)
            {
                if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance+0.1f && !m_NavMeshAgent.pathPending && TimeSinceStart > 1.05f)
                {
                    if (WaypointTypeRef == WaypointType.Random)
                    {
                        if (WaypointTimer <= 0.01f)
                        {
                            AIAnimator.SetBool("Idle Active", true);
                        }

                        WaypointTimer += Time.deltaTime;

                        if (WaypointTimer >= WaitTime && !IdleActivated)
                        {
                            LockTurning = false;
                            IdleActivated = true;
                            AIAnimator.SetBool("Idle Active", false);
                            if (!m_IdleAnimaionIndexOverride)
                            {
                                AIAnimator.SetInteger("Idle Index", Random.Range(1, TotalIdleAnimations + 1));
                            }
                            WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                            Invoke("ResetWanderTimer", 0.1f);
                            NextWaypoint();
                        }
                    }
                    else if (WaypointTypeRef != WaypointType.Random)
                    {
                        NextWaypoint();
                    }
                }
            }
            else if (WanderTypeRef == WanderType.Stationary && !IsMoving && !IsEquipping)
            {
                StationaryIdleTimer += Time.deltaTime;
                if (StationaryIdleTimer >= StationaryIdleSeconds)
                {
                    AIAnimator.SetBool("Idle Active", true);
                    if (!m_IdleAnimaionIndexOverride)
                    {
                        AIAnimator.SetInteger("Idle Index", Random.Range(1, TotalIdleAnimations + 1));
                    }
                    StationaryIdleSeconds = Random.Range(StationaryIdleSecondsMin, StationaryIdleSecondsMax);
                    StationaryIdleTimer = 0;
                }
            }

            //Play an idle sound if the AI is not moving and the Idle Seconds have been met. 
            if (!IsMoving)
            {
                IdleSoundsTimer += Time.deltaTime;
                if (IdleSoundsTimer >= IdleSoundsSeconds)
                {
                    EmeraldEventsManagerComponent.PlayIdleSound();
                    IdleSoundsTimer = 0;
                }
            }
        }

        void ResetWanderTimer ()
        {
            WaypointTimer = 0;
            IdleActivated = false;
        }

        /// <summary>
        /// Allows our Companion and Pet AI to follow their Follow Target
        /// </summary>
        public void FollowCompanionTarget ()
        {
            float DistanceFromFollower = Vector3.Distance(CurrentFollowTarget.position, transform.position);
            if (DistanceFromFollower > FollowingStoppingDistance && !IsEmoting)
            {
                m_NavMeshAgent.destination = CurrentFollowTarget.position;
            }

            if (DistanceFromFollower < 7)
            {
                CurrentMovementState = MovementState.Walk;
            }
            else
            {
                CurrentMovementState = MovementState.Run;
            }
        }

        void ReverseDelay ()
        {
            //If there's a target while in the process of this function, return as the stoppingDistance does not need to be set to 0.1.
            if (CurrentTarget != null)
            {
                WaypointReverseActive = false;
                return;
            }

            m_NavMeshAgent.stoppingDistance = 0.1f;
            WaypointReverseActive = false;
        }

        /// <summary>
        /// Handles our AI's waypoints when using the Waypoint Wander Type
        /// </summary>
        public void NextWaypoint()
        {
            if (WaypointsList.Count == 0)
                return;

            if (WaypointTypeRef != WaypointType.Random && WaypointsList.Count > 1 && !WaypointReverseActive)
            {
                WaypointIndex++;

                if (WaypointIndex == WaypointsList.Count)
                {
                    WaypointIndex = 0;
                    ReachedDestinationEvent.Invoke();

                    if (WaypointTypeRef == WaypointType.Reverse)
                    {                     
                        m_NavMeshAgent.destination = WaypointsList[WaypointsList.Count-1];
                        WaypointsList.Reverse();
                        m_NavMeshAgent.stoppingDistance = 10;
                        WaypointReverseActive = true;
                        LockTurning = false;
                        Invoke("ReverseDelay", 4);
                    }
                }

                if (m_NavMeshAgent.enabled && !WaypointReverseActive)
                {
                    m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                }
            }
            else if (WaypointTypeRef == WaypointType.Random && WaypointsList.Count > 1)
            {
                m_LastWaypointIndex = WaypointIndex;

                do
                {                  
                    WaypointIndex = Random.Range(0, WaypointsList.Count);
                } while (m_LastWaypointIndex == WaypointIndex);

                if (m_NavMeshAgent.enabled)
                {
                    m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                }
            }

            //Check that our AI's path is valid.
            CheckPath(m_NavMeshAgent.destination);
        }

        /// <summary>
        /// If enabled, checks our dynamically generated waypoint to ensure it is far 
        /// enough way from the AI, as well as ensuring that the angle limit is met
        /// </summary>
        public void GenerateWaypoint()
        {
            int GenerateIndex = 0;
            LockTurning = false;

            while (DistanceFromDestination <= (m_NavMeshAgent.stoppingDistance + 2))
            {
                //Attempt to generate a new destination 10 times, if no new destination can be found, stop trying.
                if (GenerateIndex > 10)
                {
                    GenerateIndex = 0;
                    break;
                }

                NewDestination = StartingDestination + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * WanderRadius;

                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 100, NewDestination.z), -transform.up, out HitDown, 200, DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (HitDown.transform != this.transform)
                    {
                        NewDestination = new Vector3(HitDown.point.x, HitDown.point.y, HitDown.point.z);

                        if (Vector3.Angle(Vector3.up, HitDown.normal) <= MaxSlopeLimit)
                        {
                            NewDestination = new Vector3(HitDown.point.x, HitDown.point.y, HitDown.point.z);
                            DistanceFromDestination = Vector3.Distance(NewDestination, transform.position);

                            if (DistanceFromDestination > m_NavMeshAgent.stoppingDistance + 2 && AIAgentActive)
                            {
                                NavMeshHit hit;

                                if (NavMesh.SamplePosition(NewDestination, out hit, 4, m_NavMeshAgent.areaMask))
                                {
                                    AIAnimator.SetBool("Idle Active", false);
                                    m_NavMeshAgent.SetDestination(hit.position);
                                }

                                DistanceFromDestination = 0;
                                GenerateIndex = 0;
                                break;
                            }
                        }
                    }
                }

                GenerateIndex++;
            }
        }

        /// <summary>
        /// Used for triggering a Ranged Attack. Do NOT call directly, use EmeraldAttackEvent instead.
        /// </summary>
        void CreateEmeraldProjectile()
        {
            if (CurrentTarget != null && m_EmeraldAIAbility != null)
            {
                float AdjustedAngle = TargetAngle();

                if (AdjustedAngle <= 180 && !EmeraldLookAtComponent.LookAtInProgress)
                {
                    if (m_EmeraldAIAbility.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Damage && OffensiveAbilities.Count > 0) //Offensive
                    {
                        OnAttackEvent.Invoke(); //Invoke the AttackStart event

                        if (TargetEmerald == null)
                            CurrentProjectile = EmeraldAIObjectPool.Spawn(m_EmeraldAIAbility.AbilityEffect, RangedAttackTransform.position, Quaternion.LookRotation((CurrentTarget.position + (CurrentTarget.up * CurrentPositionModifier)) - RangedAttackTransform.position, transform.up));
                        else
                            CurrentProjectile = EmeraldAIObjectPool.Spawn(m_EmeraldAIAbility.AbilityEffect, RangedAttackTransform.position, Quaternion.LookRotation(TargetEmerald.HitPointTransform.position - RangedAttackTransform.position, transform.up));

                        if (m_EmeraldAIAbility.UseCreateEffect == EmeraldAIAbility.Yes_No.Yes)
                        {
                            GameObject G = EmeraldAIObjectPool.SpawnEffect(m_EmeraldAIAbility.CreateEffect, RangedAttackTransform.position, RangedAttackTransform.rotation, m_EmeraldAIAbility.CastEffectTimeoutSeconds);
                            G.transform.SetParent(RangedAttackTransform);
                        }

                        if (CurrentProjectile.GetComponent<EmeraldAIProjectile>() == null)
                        {
                            CurrentlyCreatedAbility = CurrentProjectile.AddComponent<EmeraldAIProjectile>();
                        }
                        else
                        {
                            CurrentlyCreatedAbility = CurrentProjectile.GetComponent<EmeraldAIProjectile>();
                        }

                        if (m_EmeraldAIAbility.UseCreateSound == EmeraldAIAbility.Yes_No.Yes && m_EmeraldAIAbility.CreateSoundsList.Count > 0)
                        {
                            AudioClip RandomCreateSound = m_EmeraldAIAbility.CreateSoundsList[Random.Range(0, m_EmeraldAIAbility.CreateSoundsList.Count)];
                            if (RandomCreateSound != null)
                                EmeraldEventsManagerComponent.PlaySoundClip(RandomCreateSound);
                        }
                        InitializeAbility(CurrentlyCreatedAbility, m_EmeraldAIAbility);
                        CurrentProjectile.transform.SetParent(ObjectPool.transform);
                        CurrentProjectile.GetComponent<EmeraldAIProjectile>().InitializeProjectile(this, m_EmeraldAIAbility); //Initialize the created projectile with the needed data
                    }
                    else if (m_EmeraldAIAbility.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Support && SupportAbilities.Count > 0 && !HealingCooldownActive) //Support
                    {
                        GameObject AbilityEffect = EmeraldAIObjectPool.SpawnEffect(m_EmeraldAIAbility.AbilityEffect, HitPointTransform.position, Quaternion.identity, m_EmeraldAIAbility.AbilityEffectTimeoutSeconds);
                        if (m_EmeraldAIAbility.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Support && m_EmeraldAIAbility.SupportType == EmeraldAIAbility.SupportTypeEnum.OverTime)
                        {
                            EmeraldEventsManagerComponent.HealOverTimeAbility(m_EmeraldAIAbility);                           
                        }
                        else if (m_EmeraldAIAbility.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Support && m_EmeraldAIAbility.SupportType == EmeraldAIAbility.SupportTypeEnum.Instant)
                        {
                            CurrentHealth += m_EmeraldAIAbility.AbilitySupportAmount;
                            
                            if (CurrentHealth > StartingHealth)
                            {
                                CurrentHealth = StartingHealth;
                            }

                            if (CombatTextSystem.Instance.m_EmeraldAICombatTextData.CombatTextTargets != EmeraldAICombatTextData.CombatTextTargetEnum.PlayerOnly)
                            {
                                CombatTextSystem.Instance.CreateCombatTextAI(m_EmeraldAIAbility.AbilitySupportAmount, HitPointTransform.position, false, true);
                            }
                        }
                        OnHealEvent.Invoke();
                        AbilityEffect.transform.SetParent(ObjectPool.transform);                        
                        HealingCooldownSeconds = m_EmeraldAIAbility.AbilityCooldown;
                        HealingCooldownTimer = 0;
                        HealingCooldownActive = true;
                        if (m_EmeraldAIAbility.UseCreateSound == EmeraldAIAbility.Yes_No.Yes && m_EmeraldAIAbility.CreateSoundsList.Count > 0)
                        {
                            AudioClip RandomCreateSound = m_EmeraldAIAbility.CreateSoundsList[Random.Range(0, m_EmeraldAIAbility.CreateSoundsList.Count)];
                            if (RandomCreateSound != null)
                                EmeraldEventsManagerComponent.PlaySoundClip(RandomCreateSound);
                        }
                    }
                    else if (m_EmeraldAIAbility.AbilityType == EmeraldAIAbility.AbilityTypeEnum.Summon && SummoningAbilities.Count > 0 && TotalSummonedAI < MaxAllowedSummonedAI) //Summon
                    {
                        TotalSummonedAI++;                     
                        Vector3 m_SummonRadius = transform.position + m_EmeraldAIAbility.SummonRadius * new Vector3(Random.insideUnitSphere.x, 0, Random.insideUnitSphere.z);
                        GameObject m_SummonedObject = EmeraldAIObjectPool.Spawn(m_EmeraldAIAbility.SummonObject, m_SummonRadius + Vector3.up, Quaternion.identity);                      
                        EmeraldAISystem m_SummonedAI = m_SummonedObject.GetComponent<EmeraldAISystem>();
                        m_SummonedAI.m_NavMeshAgent.isStopped = true;
                        if (m_EmeraldAIAbility.UseCreateSound == EmeraldAIAbility.Yes_No.Yes && m_EmeraldAIAbility.CreateSoundsList.Count > 0)
                        {
                            AudioClip RandomCreateSound = m_EmeraldAIAbility.CreateSoundsList[Random.Range(0, m_EmeraldAIAbility.CreateSoundsList.Count)];
                            if (RandomCreateSound != null)
                                m_SummonedAI.EmeraldEventsManagerComponent.PlaySoundClip(RandomCreateSound);
                        }

                        if (m_SummonedObject.GetComponent<SummonedAIComponent>() == null)
                        {
                            m_SummonedObject.AddComponent<SummonedAIComponent>().IntitializeSummon(m_EmeraldAIAbility.AbilityLength);
                        }
                        else
                        {
                            m_SummonedObject.GetComponent<SummonedAIComponent>().IntitializeSummon(m_EmeraldAIAbility.AbilityLength);
                        }

                        GameObject SummoningEffect = EmeraldAIObjectPool.SpawnEffect(m_EmeraldAIAbility.SummonEffect, m_SummonedAI.transform.position, Quaternion.identity, m_EmeraldAIAbility.AbilityEffectTimeoutSeconds);
                        SummoningEffect.transform.SetParent(m_SummonedObject.transform);
                        StartCoroutine(InitializeSummonedAI(m_SummonedAI));
                    }

                    m_AbilityPicked = false;
                }
                else
                {
                    DestinationAdjustedAngle = 100;
                    AIAnimator.ResetTrigger("Attack");
                    AIAnimator.SetTrigger("Attack Cancelled");
                    return;
                }
            }
            else
            {
                DestinationAdjustedAngle = 100;
                AIAnimator.ResetTrigger("Attack");
                AIAnimator.SetTrigger("Attack Cancelled");
                return;
            }
        }

        IEnumerator InitializeSummonedAI (EmeraldAISystem AIObject)
        {
            yield return new WaitForSeconds(0.1f);
            AIObject.EmeraldEventsManagerComponent.ResetAI();
            AIObject.CurrentSummoner = this;
            AIObject.EmeraldEventsManagerComponent.SetFollowerTarget(transform);
            AIObject.AIFactionsList = new List<int>(AIFactionsList);
            AIObject.CurrentFaction = CurrentFaction;
            AIObject.PlayerFaction[0].RelationTypeRef = PlayerFaction[0].RelationTypeRef;
            AIObject.EmeraldDetectionComponent.SearchForRandomTarget = true;
            AIObject.EmeraldDetectionComponent.SearchForTarget();
            AIObject.EmeraldDetectionComponent.DetectTargetType(AIObject.CurrentTarget);
            AIObject.LookAtTarget = AIObject.CurrentTarget;
            AIObject.CompanionTargetRef = AIObject.CurrentTarget;
            AIObject.EmeraldBehaviorsComponent.ActivateCombatState();
            AIObject.m_NavMeshAgent.stoppingDistance = AIObject.AttackDistance;
            yield return new WaitForSeconds(0.8f);           
            AIObject.m_NavMeshAgent.isStopped = false;
        }

        void InitializeAbility (EmeraldAIProjectile CurrentlyCreatedAbility, EmeraldAIAbility m_EmeraldAIAbility)
        {
            CurrentlyCreatedAbility.EmeraldComponent = this;
        }
       
        /// <summary>
        /// This function is used for creating Attack Events for both Ranged and Melee attacks. See the Emerald AI Tutorial - Attack Animation Events video on YouTube.
        /// </summary>
        public void EmeraldAttackEvent()
        {
            if (WeaponTypeRef == WeaponType.Melee)
            {
                SendEmeraldDamage();
            }
            else if (WeaponTypeRef == WeaponType.Ranged)
            {
                CreateEmeraldProjectile();
            }
        }

        /// <summary>
        /// Sends damage to the player or another AI. Do NOT call directly, use EmeraldAttackEvent instead.
        /// </summary>
        public void SendEmeraldDamage()
        {
            AIAnimator.ResetTrigger("Hit");

            if (CurrentTarget != null && m_NavMeshAgent.enabled)
            {
                if (TargetObstructedActionRef != TargetObstructedAction.StayStationary || WeaponTypeRef == WeaponType.Melee)
                {
                    m_NavMeshAgent.destination = CurrentTarget.position;
                }

                //Get the distance between the AI and the target. If the distance exceeds the AI's attack distance, do not send damage to the target.
                float CurrentAttackDistance = 1;
                Vector3 m_TargetPos = CurrentTarget.position;
                m_TargetPos.y = 0;
                Vector3 m_SelfPos = transform.position;
                m_SelfPos.y = 0;
                float DistanceCheck = Vector3.Distance(m_TargetPos, m_SelfPos);
                float CurrentTargetHeight = 0;

                //Get the distance between the target and the AI. Negate the x and z axes to get the y axis height between the two objects.
                //This is used to stop AI from being able to trigger attacks that exceed the AI's Attack Height.
                Vector3 m_TargetHeight = CurrentTarget.position;
                m_TargetHeight.x = 0;
                m_TargetHeight.z = 0;
                Vector3 m_CurrentHeight = HitPointTransform.position;
                m_CurrentHeight.x = 0;
                m_CurrentHeight.z = 0;
                CurrentTargetHeight = Vector3.Distance(m_TargetHeight, m_CurrentHeight);

                GetDamageAmount();

                if (WeaponTypeRef == WeaponType.Melee)
                {
                    CurrentAttackDistance = MeleeAttackDistance;                 
                }
                else if (WeaponTypeRef == WeaponType.Ranged)
                {
                    CurrentAttackDistance = RangedAttackDistance;
                }

                if (WeaponTypeRef == WeaponType.Melee && DistanceCheck <= (CurrentAttackDistance + 0.25f) && !SwitchWeaponTypeTriggered && CurrentTargetHeight <= AttackHeight
                    || WeaponTypeRef == WeaponType.Ranged && !SwitchWeaponTypeTriggered)
                {
                    float AdjustedAngle = TargetAngle();

                    if (AdjustedAngle <= MaxDamageAngle || WeaponTypeRef == WeaponType.Ranged)
                    {
                        if (TargetTypeRef == TargetType.Player)
                        {
                            //If the player attacks without being detected, add the EmeraldAIPlayerDamage script so it can receive damage.
                            if (PlayerDamageComponent == null)
                            {
                                PlayerDamageComponent = CurrentTarget.gameObject.AddComponent<EmeraldAIPlayerDamage>();
                            }

                            if (PlayerDamageComponent != null)
                            {
                                PlayerDamageComponent.SendPlayerDamage(CurrentDamageAmount, this.transform, GetComponent<EmeraldAISystem>(), CriticalHit);
                            }

                            if (CriticalHit)
                                OnCriticalHitEvent.Invoke();

                            OnDoDamageEvent.Invoke();
                            if (!CriticalHit || CriticalHitSounds.Count == 0)
                                EmeraldEventsManagerComponent.PlayImpactSound();
                            else if (CriticalHit && CriticalHitSounds.Count > 0)
                                EmeraldEventsManagerComponent.PlayCriticalHitSound();
                        }
                        else if (TargetTypeRef == TargetType.AI && TargetEmerald != null)
                        {                          
                            TargetEmerald.Damage(CurrentDamageAmount, TargetType.AI, this.transform, SentRagdollForceAmount, CriticalHit);
                            OnDoDamageEvent.Invoke();

                            if (!IsBlocking || TargetEmerald.TransformAngle(TargetEmerald.LastAttacker) > TargetEmerald.MaxBlockAngle)
                            {
                                if (CriticalHit)
                                    OnCriticalHitEvent.Invoke();

                                if (TargetEmerald.FinalImpactSound)
                                    return;

                                if (!CriticalHit || CriticalHitSounds.Count == 0)
                                    EmeraldEventsManagerComponent.PlayImpactSound();
                                else if (CriticalHit && CriticalHitSounds.Count > 0)
                                    EmeraldEventsManagerComponent.PlayCriticalHitSound();

                                if (TargetEmerald.IsDead && !TargetEmerald.FinalImpactSound)
                                    TargetEmerald.FinalImpactSound = true;
                            }
                        }
                        else if (TargetTypeRef == TargetType.NonAITarget)
                        {
                            if (NonAIDamageComponent != null)
                            {
                                NonAIDamageComponent.SendNonAIDamage(CurrentDamageAmount, this.transform, CriticalHit);
                            }

                            if (CriticalHit)
                                OnCriticalHitEvent.Invoke();

                            OnDoDamageEvent.Invoke();
                            if (!CriticalHit || CriticalHitSounds.Count == 0)
                                EmeraldEventsManagerComponent.PlayImpactSound();
                            else if (CriticalHit && CriticalHitSounds.Count > 0)
                                EmeraldEventsManagerComponent.PlayCriticalHitSound();                          
                        }

                        if (WeaponTypeRef == WeaponType.Melee)
                        {
                            if (MeleeAttacks[MeleeAttackIndex].AttackImpactEffect != null && CurrentTarget != null)
                            {
                                if (TargetTypeRef == TargetType.Player)
                                {
                                    GameObject ImpactEffect = EmeraldAIObjectPool.SpawnEffect(MeleeAttacks[MeleeAttackIndex].AttackImpactEffect, CurrentTarget.position + (CurrentTarget.up * CurrentPositionModifier), Quaternion.identity, 2);
                                    ImpactEffect.transform.SetParent(CurrentTarget.transform);
                                }
                                else if (TargetTypeRef == TargetType.AI && TargetEmerald != null && !TargetEmerald.IsDead)
                                {
                                    GameObject ImpactEffect = EmeraldAIObjectPool.SpawnEffect(MeleeAttacks[MeleeAttackIndex].AttackImpactEffect, TargetEmerald.HitPointTransform.position, Quaternion.identity, 2);
                                    ImpactEffect.transform.SetParent(CurrentTarget.transform);
                                }
                                else if (TargetTypeRef == TargetType.NonAITarget)
                                {
                                    GameObject ImpactEffect = EmeraldAIObjectPool.SpawnEffect(MeleeAttacks[MeleeAttackIndex].AttackImpactEffect, CurrentTarget.position + (CurrentTarget.up * CurrentPositionModifier), CurrentTarget.rotation, 2);
                                    ImpactEffect.transform.SetParent(CurrentTarget.transform);
                                }
                            }
                        }   
                    }
                }
            }

            Invoke("EndAttackEvent", CurrentStateInfo.length / 2f);
        }

        void EndAttackEvent()
        {
            OnAttackEndEvent.Invoke();
        }

        /// <summary>
        /// Damages our AI and allows an AI to block and mitigate damage, if enabled. To use the ragdoll feature, all
        /// parameters need to be used where AttackerTransform is the current attacker.
        /// </summary>
        /// <param name="DamageAmount">Amount of damage caused during attack.</param>
        /// <param name="TypeOfTarget">The type of target who is causing the damage.</param>
        /// <param name="AttackerTransform">The transform of the current attacker.</param>
        /// <param name="RagdollForce">The amount of force to apply to this AI when they die. (Use Ragdoll must be enabled on this AI)</param>
        public void Damage (int DamageAmount, TargetType? TypeOfTarget = null, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            LastAttacker = AttackerTransform;
            
            if (CombatStateRef == CombatState.Active && AttackerTransform != CurrentTarget)
            {
                if (UseAggro == YesOrNo.Yes)
                {
                    if (AggroDelay >= 0.5f)
                    {
                        CurrentAggroHits++;
                        AggroDelay = 0;
                    }

                    if (CurrentAggroHits >= TotalAggroHits && !IsAttacking && !IsBackingUp)
                    {
                        if (IsBlocking && UseBlockingRef == YesOrNo.Yes || IsMoving || IsIdling && UseBlockingRef == YesOrNo.No)
                        {
                            if (AggroActionRef == AggroAction.LastAttacker && AttackerTransform != CurrentTarget)
                            {
                                EmeraldEventsManagerComponent.CancelAttackAnimation();
                                EmeraldDetectionComponent.SetDetectedTarget(AttackerTransform);
                            }
                            else if (AggroActionRef == AggroAction.RandomAttacker)
                            {
                                EmeraldEventsManagerComponent.CancelAttackAnimation();
                                EmeraldEventsManagerComponent.SearchForRandomTarget();
                            }
                            else if (AggroActionRef == AggroAction.ClosestAttacker)
                            {
                                EmeraldEventsManagerComponent.CancelAttackAnimation();
                                EmeraldDetectionComponent.SearchForTarget();
                            }

                            if (CurrentTarget == null)
                            {
                                EmeraldDetectionComponent.SearchForTarget();
                            }

                            CurrentAggroHits = 0;
                        }
                    }
                }
            }

            //If our AI is hit and does not have a target, expand its detection to look for the damage source.
            if (CurrentTarget == null && CombatStateRef == CombatState.NotActive || DeathDelayActive || BehaviorRef == CurrentBehavior.Cautious && CurrentTarget != null && ConfidenceRef != ConfidenceType.Coward)
            {
                //Return, if our attacker is equal to the AI's follow target.
                if (CurrentFollowTarget != null && AttackerTransform == CurrentFollowTarget)
                {
                    if (BehaviorRef == CurrentBehavior.Companion || BehaviorRef == CurrentBehavior.Pet)
                        return;               
                }

                if (BehaviorRef == CurrentBehavior.Aggressive ||
                    BehaviorRef == CurrentBehavior.Cautious && ConfidenceRef != ConfidenceType.Coward ||
                    BehaviorRef == CurrentBehavior.Passive && ConfidenceRef != ConfidenceType.Coward)
                {
                    if (AttackerTransform) 
                    {
                        if (TypeOfTarget != null)
                        {                           
                            TargetTypeRef = (TargetType)TypeOfTarget;
                        }

                        if (TargetTypeRef == TargetType.AI || TargetTypeRef == TargetType.Player && PlayerFaction[0].RelationTypeRef != PlayerFactionClass.RelationType.Friendly || TargetTypeRef == TargetType.NonAITarget)
                        {
                            DeathDelayActive = false;
                            CurrentTarget = AttackerTransform;
                            m_NavMeshAgent.destination = CurrentTarget.position;
                            m_NavMeshAgent.stoppingDistance = AttackDistance;

                            if (BehaviorRef == CurrentBehavior.Passive || BehaviorRef == CurrentBehavior.Cautious)
                            {
                                if (ConfidenceRef != ConfidenceType.Coward)
                                {
                                    BehaviorRef = CurrentBehavior.Aggressive;
                                }
                            }

                            if (EnableDebugging == EmeraldAISystem.YesOrNo.Yes && DebugLogTargetsEnabled == EmeraldAISystem.YesOrNo.Yes && !DeathDelayActive)
                            {
                                if (CurrentTarget != null)
                                {
                                    Debug.Log("<b>" + "<color=green>" + gameObject.name + "'s Current Target: " + "</color>" + "<color=red>" + CurrentTarget.gameObject.name + "</color>" + "</b>");
                                }
                            }

                            if (TargetTypeRef == TargetType.AI)
                            {
                                TargetEmerald = CurrentTarget.GetComponent<EmeraldAISystem>();
                            }
                            else
                            {
                                TargetEmerald = null;
                            }

                            MaxChaseDistance = ExpandedChaseDistance;
                            EmeraldLookAtComponent.SetPreviousLookAtInfo();
                            EmeraldBehaviorsComponent.ActivateCombatState();
                        }
                    }
                    else
                    {
                        MaxChaseDistance = ExpandedChaseDistance;
                        DetectionRadius = ExpandedDetectionRadius;
                        fieldOfViewAngle = ExpandedFieldOfViewAngle;
                        EmeraldDetectionComponent.SearchForTarget();
                    }
                }
                else if (BehaviorRef == CurrentBehavior.Cautious && ConfidenceRef == ConfidenceType.Coward || BehaviorRef == CurrentBehavior.Passive && ConfidenceRef == ConfidenceType.Coward)
                {
                    if (AttackerTransform.CompareTag(PlayerTag))
                    {
                        PlayerFaction[0].RelationTypeRef = PlayerFactionClass.RelationType.Enemy;
                    }
                    BehaviorRef = CurrentBehavior.Cautious;
                    MaxChaseDistance = ExpandedChaseDistance;
                    DetectionRadius = ExpandedDetectionRadius;
                    fieldOfViewAngle = ExpandedFieldOfViewAngle;
                }
            }

            DamageReceived = DamageAmount;

            if (AttackerTransform != null)
            {
                ForceTransform = AttackerTransform;

                if (CurrentTarget != null)
                {
                    AdjustedBlockAngle = TransformAngle(AttackerTransform);
                }
            }

            //Don't allow a Companion to receive damage from its own follower
            if (AttackerTransform != CurrentFollowTarget && BehaviorRef == CurrentBehavior.Companion && !IsDead || BehaviorRef != CurrentBehavior.Companion && BehaviorRef != CurrentBehavior.Pet && !IsDead)
            {
                if (!IsBlocking || AdjustedBlockAngle > MaxBlockAngle)
                {
                    CurrentHealth -= DamageAmount;
                    if (TypeOfTarget != TargetType.Player)
                    {
                        if (CombatTextSystem.Instance.m_EmeraldAICombatTextData.CombatTextTargets != EmeraldAICombatTextData.CombatTextTargetEnum.PlayerOnly && !IsDead)
                        {
                            CombatTextSystem.Instance.CreateCombatTextAI(DamageReceived, HitPointTransform.position, CriticalHit, false);
                        }
                    }
                    DamageEvent.Invoke(); //Invoke our AI's Damage Event

                    if (UseHitEffect == YesOrNo.Yes && !DamageEffectInhibitor && BloodEffectsList.Count > 0 && !IsDead)
                    {
                        GameObject RandomBloodEffect = BloodEffectsList[Random.Range(0, BloodEffectsList.Count)];
                        if (RandomBloodEffect != null)
                        {
                            GameObject SpawnedBlood = EmeraldAIObjectPool.SpawnEffect(RandomBloodEffect, Vector3.zero, transform.rotation, BloodEffectTimeoutSeconds) as GameObject;
                            SpawnedBlood.transform.SetParent(transform);

                            if (LBDImpactPosition == Vector3.zero)
                            {
                                if (BloodEffectPositionTypeRef == BloodEffectPositionType.BaseTransform)
                                {
                                    SpawnedBlood.transform.position = transform.position + BloodPosOffset;
                                }
                                else if (BloodEffectPositionTypeRef == BloodEffectPositionType.HitTransform)
                                {
                                    SpawnedBlood.transform.position = HitPointTransform.position + BloodPosOffset;
                                }
                            }

                            LBDImpactPosition = Vector3.zero;
                        }
                    }

                    EmeraldEventsManagerComponent.PlayInjuredSound();

                    if (CombatStateRef == CombatState.NotActive || ConfidenceRef == ConfidenceType.Coward)
                    {
                        AIAnimator.SetInteger("Hit Index", Random.Range(1, TotalHitAnimations + 1));
                    }
                    else if (CombatStateRef == CombatState.Active && ConfidenceRef != ConfidenceType.Coward)
                    {
                        if (WeaponTypeRef == WeaponType.Melee)
                        {
                            AIAnimator.SetInteger("Hit Index", Random.Range(1, TotalCombatHitAnimations + 1));
                        }
                        else if (WeaponTypeRef == WeaponType.Ranged)
                        {
                            AIAnimator.SetInteger("Hit Index", Random.Range(1, TotalRangedCombatHitAnimations + 1));
                        }
                    }

                    if (UseHitAnimations == YesOrNo.Yes && !IsGettingHit && !IsMoving && !IsBackingUp && !IsAttacking 
                        && !IsTurning && !IsEquipping && AdjustedBlockAngle <= MaxBlockAngle) 
                    {
                        AIAnimator.SetTrigger("Hit");
                        EmeraldEventsManagerComponent.Invoke("CancelAttackAnimation", 0.25f);
                    }
                    else 
                        AIAnimator.ResetTrigger("Hit");
                }
                else if (IsBlocking && AdjustedBlockAngle <= MaxBlockAngle)
                {
                    float BlockDamage = Mathf.Abs((DamageAmount * ((MitigationAmount - 1) * 0.01f)) - DamageAmount);
                    CurrentHealth -= (int)BlockDamage;
                    if (TypeOfTarget != TargetType.Player)
                    {
                        CombatTextSystem.Instance.CreateCombatTextAI((int)BlockDamage, HitPointTransform.position, CriticalHit, false);
                    }

                    EmeraldEventsManagerComponent.PlayBlockSound();
                    EmeraldEventsManagerComponent.Invoke("CancelAttackAnimation", 0.25f);
                    AIAnimator.SetTrigger("Hit");
                }
            }

            //Make our Brave AI flee if its health reaches the proper amount
            if (BehaviorRef == CurrentBehavior.Aggressive && ConfidenceRef == ConfidenceType.Brave)
            {
                if (((float)CurrentHealth / (float)StartingHealth) <= ((float)HealthPercentageToFlee * 0.01f))
                {
                    m_NavMeshAgent.updateRotation = true;
                    m_NavMeshAgent.ResetPath();
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    IsBackingUp = false;
                    AIAnimator.SetBool("Walk Backwards", false);
                    EmeraldEventsManagerComponent.ChangeBehavior(CurrentBehavior.Cautious);
                    EmeraldEventsManagerComponent.ChangeConfidence(ConfidenceType.Coward);
                }
            }

            //The AI has died, initialize its death state.
            if (CurrentHealth <= 0 && !IsDead)
            {
                ReceivedRagdollForceAmount = RagdollForce;
                EmeraldBehaviorsComponent.DeadState();
            }
        }

        /// <summary>
        /// Controls when an AI is deactivated while using the optimization system
        /// </summary>
        public void Deactivate()
        {
            if (CurrentTarget == null && !ReturningToStartInProgress &&
                BehaviorRef != CurrentBehavior.Companion && BehaviorRef != CurrentBehavior.Pet && CurrentDetectionRef == CurrentDetection.Unaware)
            {
                TargetDetectionActive = false;
                AIBoxCollider.enabled = false;
                EmeraldDetectionComponent.enabled = false;
                OptimizedStateRef = OptimizedState.Active;
                DeactivateTimer = 0;

                if (HealthBarCanvas != null && BehaviorRef != CurrentBehavior.Companion && BehaviorRef != CurrentBehavior.Pet)
                {
                    SetUI(false);
                }
            }
        }

        /// <summary>
        /// Controls when an AI is activated while using the optimization system
        /// </summary>
        public void Activate()
        {
            if (CurrentHealth > 0)
            {
                TargetDetectionActive = true;
                AIBoxCollider.enabled = true;
                EmeraldDetectionComponent.enabled = true;
                OptimizedStateRef = OptimizedState.Inactive;
            }
        }

        /// <summary>
        /// Gets our AI's damage amount depending on the current attack animation
        /// </summary>
        public void GetDamageAmount()
        {
            if (RandomizeDamageRef == YesOrNo.Yes)
            {
                if (CurrentMeleeAttackType == CurrentMeleeAttackTypes.StationaryAttack)
                {
                    CurrentDamageAmount = Random.Range(MeleeAttacks[MeleeAttacksListIndex].MinDamage, MeleeAttacks[MeleeAttacksListIndex].MaxDamage);
                }
                else if (CurrentMeleeAttackType == CurrentMeleeAttackTypes.RunAttack)
                {
                    CurrentDamageAmount = Random.Range(MeleeRunAttacks[MeleeRunAttacksListIndex].MinDamage, MeleeRunAttacks[MeleeRunAttacksListIndex].MaxDamage);
                }
            }
            else if (RandomizeDamageRef == YesOrNo.No)
            {
                if (CurrentMeleeAttackType == CurrentMeleeAttackTypes.StationaryAttack)
                {
                    CurrentDamageAmount = MeleeAttacks[MeleeAttacksListIndex].MinDamage;
                }
                else if (CurrentMeleeAttackType == CurrentMeleeAttackTypes.RunAttack)
                {
                    CurrentDamageAmount = MeleeRunAttacks[MeleeRunAttacksListIndex].MinDamage;
                }
            }

            //Calculates the AI's critical hits
            if (UseCriticalHits == YesOrNo.Yes && WeaponTypeRef == WeaponType.Melee)
            {
                CriticalHit = GenerateCritOdds();
                if (CriticalHit)
                {
                    CurrentDamageAmount = Mathf.RoundToInt(CurrentDamageAmount * CritMultiplier);
                }
            }
        }

        bool GenerateCritOdds()
        {
            bool CriticalHit = false;
            float m_GeneratedOdds = Random.Range(0.0f, 1.0f);
            m_GeneratedOdds = Mathf.RoundToInt(m_GeneratedOdds * 100);

            if (m_GeneratedOdds <= CritChance)
            {
                CriticalHit = true;
            }

            return CriticalHit;
        }

        public void SetUI(bool Enabled)
        {
            m_HealthBarComponent.CalculateUI();

            if (IsDead)
                return;

            HealthBarCanvasRef.enabled = Enabled;
            if (CreateHealthBarsRef == YesOrNo.Yes)
            {
                HealthBar.SetActive(Enabled);

                if (DisplayAILevelRef == YesOrNo.Yes)
                {
                    AILevelUI.gameObject.SetActive(Enabled);
                }
            }

            if (DisplayAINameRef == YesOrNo.Yes)
            {
                AINameUI.gameObject.SetActive(Enabled);
            }
        }

        /// <summary>
        /// Returns the current angle from the AI's target.
        /// </summary>
        public float TargetAngle ()
        {
            if (CurrentTarget == null)
            {
                return 360;
            }

            Vector3 Direction = new Vector3(CurrentTarget.position.x, 0, CurrentTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float angle = Vector3.Angle(transform.forward, Direction);
            float RotationDifference = transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);

            if (WeaponTypeRef == WeaponType.Ranged)
            {
                if (AdjustedAngle <= MaxFiringAngle)
                {
                    TargetInAngleLimit = true;
                }
                else
                {
                    TargetInAngleLimit = false;
                }
            }

            return AdjustedAngle;
        }

        /// <summary>
        /// Returns the current angle from the AI's destination.
        /// </summary>
        public float DestinationAngle ()
        {
            Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float angle = Vector3.Angle(transform.forward, Direction);
            float RotationDifference = transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
            return AdjustedAngle;
        }

        /// <summary>
        /// Returns the current angle from the passed Transform.
        /// </summary>
        public float TransformAngle(Transform Target)
        {
            if (Target == null)
                return 180;

            Vector3 Direction = new Vector3(Target.position.x, 0, Target.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float angle = Vector3.Angle(transform.forward, Direction);
            float RotationDifference = transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
            return AdjustedAngle;
        }

        /// <summary>
        /// Returns the current angle from the AI's head look target. (Player Targets Only)
        /// </summary>
        public float HeadLookAngle()
        {
            if (LookAtTarget == null)
                return 180;

            Vector3 Direction = new Vector3(LookAtTarget.position.x, 0, LookAtTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float angle = Vector3.Angle(transform.forward, Direction);
            float RotationDifference = transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
            return AdjustedAngle;
        }
    }
}
 