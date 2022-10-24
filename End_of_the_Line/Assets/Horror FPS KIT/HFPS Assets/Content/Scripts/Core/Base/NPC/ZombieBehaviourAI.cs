/*
 * ZombieBehaviourAI.cs - by ThunderWire Studio
 * Version 3.1
*/

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using ThunderWire.Utility;
using Newtonsoft.Json.Linq;
using HFPS.Player;

namespace HFPS.Systems
{
    /// <summary>
    /// Zombie AI System Script
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(CapsuleCollider))]
    [RequireComponent(typeof(NavMeshAgent), typeof(NPCHealth), typeof(NPCFootsteps))]
    public class ZombieBehaviourAI : NPCEntity, INPCReaction, IOnAnimatorState, ISaveable
    {
        #region Structures
        [System.Serializable]
        public sealed class BehaviourSettings
        {
            public bool enableScream = true;
            public bool enableAgony = true;
            public bool enableHunger = true;
            public bool soundReaction = true;
            public bool runToThreat = true;
            public bool attackOthers = false;
            public bool hungerRecoverHealth = true;
            public bool randomWaypoint = true;
            public bool waypointsReassign = true;
            public bool lowHPIgnorePlayer = false;
            public bool playerInvisible = false;
            public bool damageThreat = true;
            public bool wakeByThreat = false;

            [MinMax(1, 30)]
            public Vector2 patrolTime = new Vector2(5, 10);
            [MinMax(10, 240)]
            public Vector2 screamNext = new Vector2(120, 150);
            [MinMax(10, 240)]
            public Vector2 agonyNext = new Vector2(60, 120);

            public float playerLostPatrol = 5f;
            public float hungerTime = 30f;
        }

        [System.Serializable]
        public sealed class AISettings
        {
            public float walkSpeed = 0.4f;
            public float runSpeed = 5.5f;
            public float eatingStoppingDist = 3f;
            public float agentRotationSpeed = 5f;
            public float speedChangeSpeed = 1f;
            public bool rotateManually = true;
            public bool accelerateManually = true;
            public bool walkRootMotion = true;
        }

        [System.Serializable]
        public sealed class DamageSettings
        {
            [MinMax(1, 100)]
            public Vector2 damageValue = new Vector2(20, 40);
            public Vector2 damageKickback;
            public float kickbackTime;
        }

        [System.Serializable]
        public sealed class SensorSettings
        {
            public Vector3 headOffset;
            public int reactionAngleTurn = 40;
            public float soundReactRange = 20f;
            [Range(0, 179)] public float sightsFOV = 110;
            [Range(0, 179)] public float attackFOV = 30;
            public float sightsDistance = 15;
            public float attackDistance = 5f;
            public float idleHearRange = 10f;
            public float veryCloseRange = 4f;
            public float chaseTimeHide = 2f;
            public float threatRadius = 10f;
            public float lowHP = 30f;
        }

        [System.Serializable]
        public sealed class GizmosSettings
        {
            public bool gizmosEnabled;
            public bool soundReactionGizmos;
        }

        [System.Serializable]
        public sealed class NPCSounds
        {
            public AudioClip ScreamSound;
            public AudioClip EatingSound;
            public AudioClip AgonizeSound;
            public AudioClip TakeDamageSound;
            public AudioClip DieSound;
            public AudioClip[] IdleSounds;
            public AudioClip[] ReactionSounds;
            public AudioClip[] AttackSounds;
        }

        [System.Serializable]
        public sealed class NPCSoundsVolume
        {
            public float ScreamVolume = 1f;
            public float EatingVolume = 1f;
            public float AgonizeVolume = 1f;
            public float TakeDamageVolume = 1f;
            public float DieVolume = 1f;
            public float IdleVolume = 1f;
            public float ReactionVolume = 1f;
            public float AttackVolume = 1f;
        }

        [System.Serializable]
        public sealed class ZombieEvents
        {
            public UnityEvent OnReactionEvent;
            public UnityEvent OnSleepStandupEvent;
            public UnityEvent OnScreamEvent;
            public UnityEvent OnPatrolEvent;
            public UnityEvent OnEatEvent;
            public UnityEvent OnAgonyEvent;
        }

        struct WaypointsData
        {
            public WaypointGroup waypointGroup;
            public float closestDistance;

            public WaypointsData(WaypointGroup wg, float dist)
            {
                waypointGroup = wg;
                closestDistance = dist;
            }
        }

        [System.Serializable]
        struct Behaviour
        {
            public Coroutine behaviour;
            public bool isRunning;

            public Behaviour(Coroutine bh)
            {
                behaviour = bh;
                isRunning = true;
            }
        }
        #endregion

        public enum PrimaryBehaviour { Sleep, Chase, Attracted }
        public enum SecondaryBehaviour { Normal, Agony, Scream, Eating, Patrol, Reaction }
        public enum ReactionTrigger { None, Hit, Sound }
        public enum GeneralBehaviour { Waypoint2Waypoint, W2WPatrol, W2WPatrolIdle }
        public enum StartingSleep { StandUpBack, StandUpFront, Idle, None }

        private AudioSource audioSource;
        private NavMeshAgent agent;
        private NPCHealth health;

        private WaypointGroup waypoints;
        private HungerPoint[] hungerPositions;
        private HungerPoint closestHungerPoint;

        private PlayerController playerController;
        private HealthManager playerHealth;
        private Transform playerObject;
        private Transform priorityThreat;
        private Transform playerCam;

        private Waypoint nextWaypoint;
        private Vector3 reactionPosition;

        #region Public Variables
        [Header("Behaviour Main")]
        [ReadOnly, SerializeField] private PrimaryBehaviour primaryBehaviour = PrimaryBehaviour.Sleep;
        [ReadOnly, SerializeField] private SecondaryBehaviour secondaryBehaviour = SecondaryBehaviour.Normal;

        [Space(5)]
        [Tooltip("Starting Zombie Behaviour")]
        public StartingSleep sleepBehaviour = StartingSleep.StandUpBack;
        [Tooltip("General Zombie Behaviour")]
        public GeneralBehaviour zombieBehaviour = GeneralBehaviour.W2WPatrol;

        [Header("Main Setup")]
        public Animator animator;
        public LayerMask sightMask;
        public LayerMask threatMask;
        public int attackAnimations = 5;

        [Header("AI Preferences")]
        public BehaviourSettings behaviourSettings = new BehaviourSettings();
        public AISettings aiSettings = new AISettings();
        public DamageSettings damageSettings = new DamageSettings();
        public SensorSettings sensorSettings = new SensorSettings();
        public GizmosSettings gizmosSettings = new GizmosSettings();
        public ZombieEvents zombieEvents = new ZombieEvents();

        [Header("Sounds")]
        public NPCSounds m_NPCSounds;
        public NPCSoundsVolume m_NPCSoundsVolume;

        [Space(5)]
        [Tooltip("Play Sounds when zombie is in attracted state.")]
        public bool playAttractedSounds = true;
        public bool eventPlayAttackSound;
        public bool eventPlayScreamSound;
        public bool eventPlayAgonySound;
        public bool eventPlayEatSound;

        [HideInInspector]
        public bool isDead = false;
        #endregion

        #region Private Variables
        private Vector3 npcHead;
        private Vector3 playerHead;
        private Vector3 lastWaypointPos;
        private Vector3 lastThreatPosition;
        private Vector3 lastCorrectDestination;

        private int lastAttack;
        private int lastSound;
        private float defaultStopping;
        private float targetAgentSpeed;

        private float screamTime;
        private float chaseTime;
        private float agonyTime;

        private bool enableRootPosition;
        private bool enableRootRotation;

        private bool secondaryPending;
        private bool waypointsAssigned;
        private bool rotateTransform;

        private bool shouldMove;
        private bool canChasePlayer;
        private bool canScream;
        private bool canAttack = true;
        private bool canReact = true;
        private bool runToDestination = false;
        private bool ignorePlayer = false;
        private bool canIgnorePlayer = true;

        private bool enableNPCSights = true;
        private bool setAgonyTime = true;
        private bool setHungerTime = true;

        private bool isAgonyOrEating;
        private bool isReaction;
        private bool isLerpSpeed;
        private bool isWaypointSet;
        private bool isNPCAwake;
        private bool isIdle;

        private bool screamTriggered;
        private bool waitToChase;
        private bool veryCloseAttract;
        private bool goToLastWaypoint;

        //Behaviours
        Behaviour b_Patrol = new Behaviour();
        Behaviour b_Reaction = new Behaviour();
        Behaviour b_Eating = new Behaviour();
        #endregion

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<NPCHealth>();

            playerController = PlayerController.Instance;
            playerObject = playerController.transform;
            playerHealth = playerController.gameObject.GetComponent<HealthManager>();

            playerCam = ScriptManager.Instance.MainCamera.transform;
            hungerPositions = FindObjectsOfType<HungerPoint>();

            defaultStopping = agent.stoppingDistance;
        }

        void Start()
        {
            agent.updateRotation = !aiSettings.rotateManually;
            agent.updatePosition = false;
            agent.isStopped = false;
            enableNPCSights = true;

            canScream = behaviourSettings.enableScream;
            canAttack = true;
            canReact = true;

            setHungerTime = behaviourSettings.hungerTime <= 0;

            if (!behaviourSettings.waypointsReassign)
            {
                WaypointGroup nextWaypoints = FindClosestWaypoints();

                if (nextWaypoints != waypoints)
                {
                    waypoints = FindClosestWaypoints();
                    goToLastWaypoint = false;
                }

                waypointsAssigned = true;
            }

            Invoke(nameof(LateStart), 1f);

            if (!agent.isOnNavMesh)
            {
                Debug.LogError("[Zombie AI] Create NavMesh first or move AI to NavMesh!");
            }
        }

        void LateStart()
        {
            if (sleepBehaviour != StartingSleep.None)
            {
                primaryBehaviour = PrimaryBehaviour.Sleep;
                animator.SetInteger("IdleState", (int)sleepBehaviour);
                animator.SetBool("Idle", true);
                animator.SetBool("Patrol", false);
                isNPCAwake = false;
            }
            else if (primaryBehaviour == PrimaryBehaviour.Sleep)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Patrol", true);
                isNPCAwake = true;
                isIdle = false;
            }
            else
            {
                secondaryPending = false;
                isNPCAwake = true;
                isIdle = false;
            }
        }

        void OnAnimatorMove()
        {
            if (enableRootPosition)
            {
                Vector3 position = animator.rootPosition;
                position.y = agent.nextPosition.y;
                transform.position = position;
                agent.nextPosition = position;
            }

            if (enableRootRotation)
            {
                transform.rotation = animator.rootRotation;
            }
        }

        void Update()
        {
            //Stop update after death
            if (isDead) return;

            //Set correct head positions
            Vector3 _playerCam = playerCam.position;
            playerHead = new Vector3(playerObject.position.x, _playerCam.y, playerObject.position.z);
            npcHead = transform.position + sensorSettings.headOffset;

            //Should we allow NavMeshAgent update transform position, when Root Motion is enabled?
            agent.updatePosition = !enableRootPosition;

            //Get player velocity magnitude
            Vector3 pvelocity = playerController.CharacterControl.velocity;
            float pmagnitude = new Vector3(pvelocity.x, 0, pvelocity.z).magnitude;

            //Is player currently moving?
            bool playerMoving = DistanceOf(_playerCam) > agent.stoppingDistance && pmagnitude > 1;

            //Check if zombie is in agent destination
            shouldMove = !PathCompleted();

            //Check if Eating or Agony is pending
            isAgonyOrEating = secondaryBehaviour == SecondaryBehaviour.Eating || secondaryBehaviour == SecondaryBehaviour.Agony;

            //Ignore Player if Zombie has Low HP
            ignorePlayer = health.Health <= sensorSettings.lowHP && behaviourSettings.lowHPIgnorePlayer && canIgnorePlayer;

            if (behaviourSettings.lowHPIgnorePlayer && ignorePlayer && health.Health > sensorSettings.lowHP)
            {
                canIgnorePlayer = true;
            }

            //Update NPC rotation
            if (rotateTransform)
            {
                if (aiSettings.rotateManually)
                {
                    RotateTowards(agent.steeringTarget);
                }
                else
                {
                    agent.updateRotation = true;
                }
            }
            else
            {
                agent.updateRotation = false;
            }

            //Update NPC agent speed
            if (isLerpSpeed)
            {
                agent.speed = Mathf.MoveTowards(agent.speed, targetAgentSpeed, Time.deltaTime * aiSettings.speedChangeSpeed);
            }

            if (sleepBehaviour != StartingSleep.None)
            {
                Transform wakeThreat = playerObject;

                //Wake zombie by threat presence
                if (behaviourSettings.wakeByThreat)
                {
                    Collider[] threats = ThreatsInRadius(sensorSettings.threatRadius);
                    Transform newWakeThreat = GetClosestVisibleThreat(threats);

                    if (newWakeThreat != null)
                        wakeThreat = newWakeThreat;
                }

                //Set zombie animation to a sleep behaviour
                if (wakeThreat && InDistance(sensorSettings.idleHearRange, wakeThreat.position))
                {
                    PlaySoundRandom(m_NPCSounds.ReactionSounds, m_NPCSoundsVolume.ReactionVolume);
                    animator.SetInteger("IdleState", (int)StartingSleep.None);
                    animator.SetBool("Idle", false);
                    enableNPCSights = false;

                    sleepBehaviour = StartingSleep.None;
                    primaryBehaviour = PrimaryBehaviour.Chase;
                    isIdle = behaviourSettings.enableScream;
                }
                else
                {
                    if (sleepBehaviour != StartingSleep.Idle)
                    {
                        secondaryPending = true;
                        isNPCAwake = false;
                    }
                    else
                    {
                        isIdle = true;
                        isNPCAwake = true;
                    }
                }
            }
            else if (SearchForThreat())
            {
                agent.stoppingDistance = defaultStopping;
                enableNPCSights = isAgonyOrEating;
                waypointsAssigned = false;
                canReact = true;

                //Destroy the pending Patrol or Reaction behaviour
                DestroyBehaviour(true, true, true);

                //Can player be chased?
                if (!canChasePlayer)
                {
                    //Set last player position
                    SetAgentDestination(lastThreatPosition, aiSettings.runSpeed, true);

                    if (behaviourSettings.enableScream && canScream)
                    {
                        SetAnimatorState(scream: true);
                        waitToChase = true;
                        screamTriggered = true;
                    }
                    else if (!screamTriggered && !isReaction)
                    {
                        //Trigger Move Animation
                        enableRootPosition = !behaviourSettings.runToThreat && aiSettings.walkRootMotion;
                        SetAnimatorState(!behaviourSettings.runToThreat, behaviourSettings.runToThreat);
                        canChasePlayer = !secondaryPending;
                        secondaryPending = isIdle || !isNPCAwake || isAgonyOrEating;
                        waitToChase = false;
                    }
                    else
                    {
                        secondaryPending = false;
                        isReaction = false;
                    }
                }
                else if (!secondaryPending)
                {
                    //Should we enable root motion?
                    rotateTransform = true;
                    enableRootRotation = false;
                    enableRootPosition = !behaviourSettings.runToThreat && aiSettings.walkRootMotion;
                    isIdle = false;

                    //Go to player position
                    SetAgentDestination(lastThreatPosition, aiSettings.runSpeed, false, aiSettings.accelerateManually);

                    if (behaviourSettings.runToThreat)
                    {
                        SetAnimatorState(false, shouldMove || playerMoving, !shouldMove, scream: false);
                    }
                    else
                    {
                        SetAnimatorState(shouldMove || playerMoving, false, !shouldMove, scream: false);
                    }

                    //Attack Player
                    if (priorityThreat && InDistance(sensorSettings.attackDistance, priorityThreat.position) && IsObjectInSights(sensorSettings.attackFOV, priorityThreat.position) && canAttack)
                    {
                        if (!eventPlayAttackSound) PlaySoundRandom(m_NPCSounds.AttackSounds, m_NPCSoundsVolume.AttackVolume);
                        animator.SetInteger("AttackState", lastAttack = Utilities.RandomUnique(0, attackAnimations, lastAttack));
                        animator.SetTrigger("Attack");
                        canAttack = false;
                    }

                    secondaryBehaviour = SecondaryBehaviour.Normal;
                }

                if (!isAgonyOrEating)
                {
                    primaryBehaviour = PrimaryBehaviour.Chase;
                }
            }
            else if (secondaryBehaviour != SecondaryBehaviour.Scream && !screamTriggered)
            {
                //Enable NPC sights and set required variables
                screamTriggered = false;
                canChasePlayer = false;
                enableNPCSights = true;
                waitToChase = false;
                isIdle = false;

                //If Agony or Eating is running and the player is not in the sights, it will return to the default state
                if (isAgonyOrEating)
                {
                    if (!isReaction)
                    {
                        SetAnimatorState(true, false, scream: false);
                        ResetAgentForSecondary();
                    }

                    canReact = true;
                    goToLastWaypoint = true;
                }

                if (shouldMove && primaryBehaviour == PrimaryBehaviour.Chase)
                {
                    //Should we enable root motion?
                    rotateTransform = true;
                    enableRootRotation = false;
                    enableRootPosition = !behaviourSettings.runToThreat && aiSettings.walkRootMotion;

                    if (health.Health > sensorSettings.lowHP || !behaviourSettings.lowHPIgnorePlayer)
                    {
                        //Continue to last player destination
                        SetAgentDestination(lastThreatPosition, behaviourSettings.runToThreat ? aiSettings.runSpeed : aiSettings.walkSpeed, false, aiSettings.accelerateManually);
                        SetAnimatorState(!behaviourSettings.runToThreat, behaviourSettings.runToThreat, false, false, true);

                        //Start Patrol Behaviour
                        b_Patrol = new Behaviour(StartCoroutine(Patrol(behaviourSettings.playerLostPatrol)));
                        secondaryPending = true;
                    }
                    else
                    {
                        //Low Zombie HP Settings

                        //Reset Pending Path
                        agent.ResetPath();

                        //Set Destination to First Waypoint
                        SetAgentDestination(waypoints.Waypoints[0].transform.position, aiSettings.runSpeed, false, aiSettings.accelerateManually);

                        runToDestination = true; //Set Running Speed
                        secondaryPending = false; //Enable Secondary Behaviour
                        enableNPCSights = true; //Enable Zombie Sights
                        behaviourSettings.hungerTime = 2f; //Set Hunger Time to low number
                    }

                    secondaryBehaviour = SecondaryBehaviour.Normal;
                }
                else if (!secondaryPending)
                {
                    canReact = true;
                    SecondaryUpdate();
                }

                primaryBehaviour = PrimaryBehaviour.Attracted;
            }
            else
            {
                //Destroy the pending Reaction behaviour
                DestroyBehaviour(false, true);

                //If Agony or Eating is running and the player is not in the sights, it will return to the default state
                if (isAgonyOrEating)
                {
                    if (!isReaction)
                    {
                        SetAnimatorState(true, false, scream: false);
                        ResetAgentForSecondary();
                    }

                    canReact = true;
                    waitToChase = false;
                    screamTriggered = false;
                    goToLastWaypoint = true;
                }
            }
        }

        /// <summary>
        /// Function to update Secondary Behaviours
        /// </summary>
        void SecondaryUpdate()
        {
            agent.stoppingDistance = defaultStopping;

            //Assign new Waypoints Group after chase
            if (!waypointsAssigned && behaviourSettings.waypointsReassign)
            {
                WaypointGroup nextWaypoints = FindClosestWaypoints();

                if (nextWaypoints != waypoints)
                {
                    waypoints = FindClosestWaypoints();
                    goToLastWaypoint = false;
                }

                waypointsAssigned = true;
            }

            //Next Scream Timer
            if (behaviourSettings.enableScream)
            {
                if (!canScream)
                {
                    if (screamTime > 0)
                    {
                        //Next Scream Countdown
                        screamTime -= Time.deltaTime;
                    }
                    else
                    {
                        //Set next scream time
                        screamTime = behaviourSettings.screamNext.Random();
                        canScream = true;
                    }
                }
            }

            //Hunger Behaviour
            if (behaviourSettings.enableHunger)
            {
                if (setHungerTime && behaviourSettings.hungerTime <= 0)
                {
                    if (behaviourSettings.hungerTime < 0) behaviourSettings.hungerTime = 0;

                    //Set first hunger time to +5s
                    behaviourSettings.hungerTime += 5f;
                    setHungerTime = false;
                }
                else
                {
                    if (behaviourSettings.hungerTime > 0)
                    {
                        //Hunger Countdown
                        behaviourSettings.hungerTime -= Time.deltaTime;
                    }
                    else
                    {
                        closestHungerPoint = FindClosestHungerPoint();

                        if (closestHungerPoint && SeesObject(sensorSettings.sightsDistance, closestHungerPoint.transform.position))
                        {
                            //Should we enable root motion?
                            rotateTransform = true;
                            enableRootRotation = false;
                            enableRootPosition = false;

                            //Set position to closest Hunger Point position
                            SetAgentDestination(closestHungerPoint.transform.position, aiSettings.runSpeed, false, aiSettings.accelerateManually);
                            SetAnimatorState(false, true);
                            agent.stoppingDistance = aiSettings.eatingStoppingDist;

                            //Start Eat Behaviour Trigger
                            b_Eating = new Behaviour(StartCoroutine(EatOrAgony(0, SecondaryBehaviour.Eating, true)));

                            //Set required variables
                            goToLastWaypoint = true;
                            isWaypointSet = false;
                            secondaryPending = true;
                            behaviourSettings.hungerTime = 0;

                            return;
                        }
                    }
                }
            }

            //Agony Behaviour
            if (behaviourSettings.enableAgony)
            {
                if (setAgonyTime)
                {
                    //Set next agony time
                    agonyTime = behaviourSettings.agonyNext.Random();
                    setAgonyTime = false;
                }
                else
                {
                    if (agonyTime > 0)
                    {
                        //Agony Countdown
                        agonyTime -= Time.deltaTime;
                    }
                    else
                    {
                        //Stop other and trigger Agony animation
                        SetAnimatorState(scream: false);
                        animator.SetTrigger("Agonize");

                        //Set required variables
                        agent.isStopped = true;
                        goToLastWaypoint = true;
                        isWaypointSet = false;
                        secondaryPending = true;
                        behaviourSettings.hungerTime += 5;
                        agonyTime = 0;

                        return;
                    }
                }
            }

            //Waypoints Behaviour
            if (!shouldMove)
            {
                //Should we enable root motion?
                rotateTransform = true;
                enableRootRotation = false;
                enableRootPosition = aiSettings.walkRootMotion;

                if (zombieBehaviour == GeneralBehaviour.Waypoint2Waypoint)
                {
                    //Set Waypoint Destination
                    SetAnimatorState(true);
                    SetAgentDestination(goToLastWaypoint ? lastWaypointPos : NextWaypoint(), runToDestination ? aiSettings.runSpeed : aiSettings.walkSpeed, false, false);
                }
                else if (zombieBehaviour == GeneralBehaviour.W2WPatrol || zombieBehaviour == GeneralBehaviour.W2WPatrolIdle)
                {
                    if (!isWaypointSet)
                    {
                        //Set Waypoint Destination
                        SetAnimatorState(true);
                        SetAgentDestination(goToLastWaypoint ? lastWaypointPos : NextWaypoint(), runToDestination ? aiSettings.runSpeed : aiSettings.walkSpeed, false, false);
                        isWaypointSet = true;
                    }
                    else
                    {
                        if (zombieBehaviour == GeneralBehaviour.W2WPatrol)
                        {
                            SetAnimatorState(false, false, true, false, true);
                        }
                        else if (zombieBehaviour == GeneralBehaviour.W2WPatrolIdle)
                        {
                            SetAnimatorState(false, false, false, true, true);
                        }

                        //Start Patrol Behaviour
                        b_Patrol = new Behaviour(StartCoroutine(Patrol(behaviourSettings.patrolTime.Random())));

                        agent.isStopped = true;
                        secondaryPending = true;
                        isWaypointSet = false;
                    }
                }

                if (behaviourSettings.lowHPIgnorePlayer && ignorePlayer)
                {
                    canIgnorePlayer = false;
                }

                goToLastWaypoint = false;
                runToDestination = false;
            }
        }

        /// <summary>
        /// Get next possible waypoint path
        /// </summary>
        Vector3? NextWaypoint()
        {
            if (waypoints && waypoints.Waypoints.Count > 1)
            {
                if (behaviourSettings.randomWaypoint)
                {
                    Waypoint[] possibleWaypoints = waypoints.Waypoints.Where(x => !x.isOccupied && IsPathPossible(x.transform.position) && x != nextWaypoint).ToArray();

                    if (possibleWaypoints.Length > 0)
                    {
                        if (nextWaypoint)
                        {
                            nextWaypoint.isOccupied = false;
                            nextWaypoint.occupiedBy = null;
                        }

                        nextWaypoint = possibleWaypoints[Random.Range(0, possibleWaypoints.Length - 1)];
                        nextWaypoint.isOccupied = true;
                        nextWaypoint.occupiedBy = gameObject;

                        return lastWaypointPos = nextWaypoint.transform.position;
                    }
                    else
                    {
                        return transform.position;
                    }
                }
                else
                {
                    List<Waypoint> possibleWaypoints = waypoints.Waypoints.Where(x => !x.isOccupied && IsPathPossible(x.transform.position)).ToList();

                    if (nextWaypoint)
                    {
                        nextWaypoint.isOccupied = false;
                        nextWaypoint.occupiedBy = null;
                    }

                    int next = nextWaypoint != null ? possibleWaypoints.IndexOf(nextWaypoint) == possibleWaypoints.Count - 1 ? 0 : possibleWaypoints.IndexOf(nextWaypoint) + 1 : 0;

                    nextWaypoint = possibleWaypoints[next];
                    nextWaypoint.isOccupied = true;
                    nextWaypoint.occupiedBy = gameObject;

                    return lastWaypointPos = nextWaypoint.transform.position;
                }
            }

            Debug.LogError("[AI Waypoint] Could not set next waypoint!");
            return null;
        }

        #region Interface Callbacks
        public void OnStateEnter(AnimatorStateInfo state, string name)
        {
            if (name == "AttackIdle")
            {
                canAttack = true;
            }
            else if (name == "Sleep")
            {
                StartCoroutine(SleepStandup(state.length));

                //Invoke the Scream Event
                zombieEvents?.OnSleepStandupEvent.Invoke();
            }
            else if (name == "Scream")
            {
                secondaryBehaviour = SecondaryBehaviour.Scream;
                StartCoroutine(Scream(state.length));

                //Invoke the Scream Event
                zombieEvents?.OnScreamEvent.Invoke();
            }
            else if (name == "Eat")
            {
                secondaryBehaviour = SecondaryBehaviour.Eating;
                StartCoroutine(EatOrAgony(state.length, SecondaryBehaviour.Eating, false));

                //Invoke the Eat Event
                zombieEvents?.OnEatEvent.Invoke();
            }
            else if (name == "Agony")
            {
                secondaryBehaviour = SecondaryBehaviour.Agony;
                StartCoroutine(EatOrAgony(state.length, SecondaryBehaviour.Agony, false));

                //Invoke the Agony Event
                zombieEvents?.OnAgonyEvent.Invoke();
            }
            else if (name == "Turn")
            {
                secondaryBehaviour = SecondaryBehaviour.Reaction;
                b_Reaction = new Behaviour(StartCoroutine(Reaction(state.length)));
            }
        }

        public void HitReaction()
        {
            if (isDead) return;

            //Wake NPC
            if (sleepBehaviour != StartingSleep.None)
            {
                animator.SetInteger("IdleState", (int)StartingSleep.None);
                animator.SetBool("Idle", false);
                sleepBehaviour = StartingSleep.None;
            }

            //Trigger Hit Animation
            if (secondaryBehaviour != SecondaryBehaviour.Eating && isNPCAwake)
            {
                animator.SetTrigger("Hit");
            }

            PlaySound(m_NPCSounds.TakeDamageSound, m_NPCSoundsVolume.TakeDamageVolume);

            if (primaryBehaviour != PrimaryBehaviour.Chase && canReact)
            {
                //Play Reaction Sound
                PlaySoundRandom(m_NPCSounds.ReactionSounds, m_NPCSoundsVolume.ReactionVolume);

                //Destroy all pending Behaviours
                StopAllCoroutines();
                b_Reaction.isRunning = false;

                //Set required parameters
                primaryBehaviour = PrimaryBehaviour.Attracted;
                isReaction = true;

                //Set reaction position
                reactionPosition = playerObject.position;

                //Enable Sights
                enableNPCSights = true;

                //Start Reaction Behaviour
                int angle = transform.RealSignedAngle(playerObject.position);

                if (Mathf.Abs(angle) >= sensorSettings.reactionAngleTurn)
                {
                    SetAnimatorState(scream: false);
                    animator.SetFloat("TurnAngle", angle);
                    animator.SetTrigger("Turn");
                }
                else
                {
                    b_Reaction = new Behaviour(StartCoroutine(Reaction(0)));
                }

                agent.ResetPath();
                agent.isStopped = true;
                rotateTransform = false;
                goToLastWaypoint = true;
                secondaryPending = true;
                veryCloseAttract = false;
                isNPCAwake = isIdle ? true : isNPCAwake;
                canReact = false;
            }
        }

        public void SoundReaction(Vector3 pos, bool closeSound)
        {
            if (isDead) return;

            float distance = Vector3.Distance(transform.position, pos);

            if (primaryBehaviour != PrimaryBehaviour.Chase && distance <= sensorSettings.soundReactRange && canReact)
            {
                //Wake NPC
                if (sleepBehaviour != StartingSleep.None)
                {
                    animator.SetInteger("IdleState", (int)StartingSleep.None);
                    animator.SetBool("Idle", false);
                    sleepBehaviour = StartingSleep.None;

                    //Play Reaction Sound
                    PlaySoundRandom(m_NPCSounds.ReactionSounds, m_NPCSoundsVolume.ReactionVolume);
                }

                //Destroy all pending Behaviours
                StopAllCoroutines();
                b_Reaction.isRunning = false;

                //Set required parameters
                primaryBehaviour = PrimaryBehaviour.Attracted;
                isReaction = true;

                //Invoke the Attract Ęvent
                zombieEvents?.OnReactionEvent.Invoke();

                //Set reaction position
                reactionPosition = pos;

                //Enable Sights
                enableNPCSights = true;

                //Start Reaction Behaviour
                int angle = transform.RealSignedAngle(pos);

                if (Mathf.Abs(angle) >= sensorSettings.reactionAngleTurn)
                {
                    SetAnimatorState(scream: false);
                    animator.SetFloat("TurnAngle", angle);
                    animator.SetTrigger("Turn");
                }
                else
                {
                    b_Reaction = new Behaviour(StartCoroutine(Reaction(0)));
                }

                agent.ResetPath();
                agent.isStopped = true;
                rotateTransform = false;
                goToLastWaypoint = true;
                secondaryPending = true;
                veryCloseAttract = closeSound;
                isNPCAwake = isIdle ? true : isNPCAwake;
                canReact = false;
            }
        }
        #endregion

        #region Behaviour Trees
        IEnumerator SleepStandup(float time)
        {
            canChasePlayer = false;
            secondaryPending = true;

            if (!behaviourSettings.playerInvisible && !isReaction)
            {
                if (behaviourSettings.enableScream && canScream)
                {
                    SetAnimatorState(scream: true);
                }
                else
                {
                    SetAnimatorState(true, scream: false);
                }
            }

            yield return new WaitForSeconds(time);

            if (!isReaction)
            {
                canChasePlayer = !screamTriggered;
                secondaryPending = false;
            }

            isNPCAwake = true;
        }

        IEnumerator Scream(float time)
        {
            //Play Scream Sound
            if (!eventPlayScreamSound) PlaySound(m_NPCSounds.ScreamSound, m_NPCSoundsVolume.ScreamVolume);

            canScream = false;
            rotateTransform = true;
            SetAnimatorState(!behaviourSettings.runToThreat, behaviourSettings.runToThreat);
            SetAgentDestination(lastThreatPosition, behaviourSettings.runToThreat ? aiSettings.runSpeed : aiSettings.walkSpeed, true);

            yield return new WaitForSeconds(time);

            secondaryBehaviour = SecondaryBehaviour.Normal;
            secondaryPending = false;
            screamTriggered = false;
            canChasePlayer = true;
            isReaction = false;
        }

        IEnumerator Patrol(float time)
        {
            yield return new WaitUntil(() => !shouldMove);

            //Invoke the Patrol Event
            zombieEvents?.OnPatrolEvent.Invoke();

            rotateTransform = false;
            agent.isStopped = true;

            SetAnimatorState(false, false, true, scream: false);
            secondaryBehaviour = SecondaryBehaviour.Patrol;

            yield return new WaitForSeconds(time);

            if (playAttractedSounds) PlaySoundRandom(m_NPCSounds.IdleSounds, m_NPCSoundsVolume.IdleVolume);

            secondaryPending = false;
            secondaryBehaviour = SecondaryBehaviour.Normal;

            b_Patrol.isRunning = false;
        }

        IEnumerator EatOrAgony(float time, SecondaryBehaviour behaviour, bool trigger)
        {
            if (behaviour == SecondaryBehaviour.Agony || !trigger && behaviour == SecondaryBehaviour.Eating)
            {
                if (behaviour == SecondaryBehaviour.Agony)
                {
                    //Play Agony Sound
                    if (!eventPlayAgonySound) PlaySound(m_NPCSounds.AgonizeSound, m_NPCSoundsVolume.AgonizeVolume);

                    setAgonyTime = true;
                    rotateTransform = false;
                }

                SetAnimatorState(true, scream: false);
                yield return new WaitForSeconds(time);

                if (!isReaction)
                {
                    secondaryBehaviour = SecondaryBehaviour.Normal;
                    canChasePlayer = !waitToChase && !screamTriggered;
                    agent.isStopped = false;
                    secondaryPending = false;
                }

                agent.stoppingDistance = defaultStopping;
                b_Eating.isRunning = false;
            }
            else if (behaviour == SecondaryBehaviour.Eating)
            {
                yield return new WaitUntil(() => !shouldMove);

                //Should we enable root motion?
                rotateTransform = false;
                enableRootRotation = false;
                enableRootPosition = false;

                SetAnimatorState(scream: false);
                animator.SetTrigger("Eat");

                HungerPoint.HungerPoints hunger_points = closestHungerPoint.GetHungerPoints();
                behaviourSettings.hungerTime = hunger_points.hungerPoints;

                if (behaviourSettings.hungerRecoverHealth)
                {
                    health.Health += hunger_points.healthPoints;

                    if (behaviourSettings.lowHPIgnorePlayer && health.Health > sensorSettings.lowHP)
                    {
                        canIgnorePlayer = true;
                    }
                }
            }
        }

        IEnumerator Reaction(float time)
        {
            yield return new WaitUntil(() => isNPCAwake && !isAgonyOrEating);

            secondaryBehaviour = SecondaryBehaviour.Reaction;

            //Invoke the Agony Event
            zombieEvents?.OnReactionEvent.Invoke();

            if (time > 0)
            {
                //Should we enable root motion?
                enableRootRotation = true;
                enableRootPosition = false;

                //Clamp wait time for sound reaction
                yield return new WaitForSeconds(time - 1f);
            }

            //Should we enable root motion?
            rotateTransform = true;
            enableRootRotation = false;
            enableRootPosition = !behaviourSettings.runToThreat && aiSettings.walkRootMotion;
            canReact = true;

            if (!veryCloseAttract)
            {
                SetAnimatorState(!behaviourSettings.runToThreat, behaviourSettings.runToThreat, scream: false);
                SetAgentDestination(reactionPosition, aiSettings.runSpeed, false, aiSettings.accelerateManually);

                yield return new WaitForSeconds(1);
                yield return new WaitUntil(() => !shouldMove);
            }

            SetAnimatorState(false, false, true, scream: false);

            //Should we enable root motion?
            rotateTransform = false;
            enableRootRotation = false;
            enableRootPosition = false;

            yield return new WaitForSeconds(behaviourSettings.patrolTime.Random());

            PlaySoundRandom(m_NPCSounds.IdleSounds, m_NPCSoundsVolume.IdleVolume);

            isReaction = false;
            secondaryPending = false;
            veryCloseAttract = false;

            secondaryBehaviour = SecondaryBehaviour.Normal;
            b_Reaction.isRunning = false;
        }
        #endregion

        #region Other Functions
        /// <summary>
        /// Wake up NPC
        /// </summary>
        public void WakeUp()
        {
            if (sleepBehaviour != StartingSleep.None)
            {
                animator.SetInteger("IdleState", (int)StartingSleep.None);
                animator.SetBool("Idle", false);
                sleepBehaviour = StartingSleep.None;
            }
        }

        /// <summary>
        /// Main Search Function
        /// </summary>
        bool SearchForThreat()
        {
            bool sightsResult;

            if (playerHealth.isDead || behaviourSettings.playerInvisible || ignorePlayer)
            {
                priorityThreat = null;
                return false;
            }

            if (priorityThreat)
            {
                sightsResult = priorityThreat == playerObject ?
                    GetSightsResult(playerObject.position, playerHead) :
                    GetSightsResult(priorityThreat.position, priorityThreat.position);

                if (sightsResult)
                {
                    chaseTime = 0;
                    lastThreatPosition = priorityThreat.position;
                    return true;
                }
                else if (chaseTime < sensorSettings.chaseTimeHide)
                {
                    chaseTime += Time.deltaTime;
                    lastThreatPosition = priorityThreat.position;
                    return true;
                }
                else
                {
                    priorityThreat = null;
                }

                return false;
            }

            if (behaviourSettings.attackOthers)
            {
                Collider[] threats = ThreatsInRadius(sensorSettings.threatRadius);

                if(threats.Length > 0)
                {
                    // Prioritize Player
                    foreach (var threat in threats)
                    {
                        if(threat.transform == playerObject)
                        {
                            if (GetSightsResult(playerObject.position, playerHead))
                            {
                                priorityThreat = threat.transform;
                            }

                            break;
                        }
                    }

                    // Pick random visible threat
                    if (!priorityThreat)
                    {
                        priorityThreat = GetClosestVisibleThreat(threats);
                    }
                }
            }
            else if(!playerHealth.isDead && !behaviourSettings.playerInvisible && !ignorePlayer)
            {
                // Player as single threat
                sightsResult = GetSightsResult(playerObject.position, playerHead);

                if (sightsResult)
                {
                    priorityThreat = playerObject;
                    return true;
                }
            }

            return false;
        }

        Transform GetClosestVisibleThreat(Collider[] threats)
        {
            Transform closestThreat = null;
            Transform[] visibleThreats = threats.Where(x => GetSightsResult(x.transform.position, x.transform.position))
                                        .Where(x => !x.gameObject.HasComponent(out DamageBehaviour damage) || damage.IsAlive())
                                        .Select(x => x.transform).ToArray();

            if (visibleThreats.Length > 0)
            {
                float lastDistance = 0;
                foreach (var threat in visibleThreats)
                {
                    float distance = DistanceOf(threat.position);

                    if (distance < lastDistance || lastDistance == 0)
                    {
                        lastDistance = distance;
                        closestThreat = threat;
                    }
                }
            }

            return closestThreat;
        }

        bool GetSightsResult(Vector3 threatPos, Vector3 headPos)
        {
            bool sightsResult = false;

            if (enableNPCSights)
            {
                if (SeesObject(sensorSettings.sightsDistance, headPos) && IsObjectInSights(sensorSettings.sightsFOV, threatPos))
                {
                    sightsResult = true;
                }
                else if (SeesObject(sensorSettings.veryCloseRange, headPos) && !veryCloseAttract && !isAgonyOrEating)
                {
                    SoundReaction(threatPos, true);
                    veryCloseAttract = true;
                }
            }
            else if (SeesObject(sensorSettings.sightsDistance, headPos))
            {
                sightsResult = true;
            }

            return sightsResult;
        }

        /// <summary>
        /// Play Single Sound
        /// </summary>
        void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (audioSource && clip != null)
            {
                audioSource.volume = volume;
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Play Random Sounds
        /// </summary>
        void PlaySoundRandom(AudioClip[] clips, float volume = 1f)
        {
            if (audioSource && clips.Length > 0)
            {
                lastSound = Utilities.RandomUnique(0, clips.Length, lastSound);
                audioSource.volume = volume;
                audioSource.clip = clips[lastSound];
                audioSource.Play();
            }
        }

        /// <summary>
        /// Play Sound from Animation or Apply Damage to Threat Event
        /// </summary>
        public void PlaySoundOrDamageEvent(int type)
        {
            if (type == 0)
            {
                // Attack Sound
                if (eventPlayAttackSound)
                {
                    PlaySoundRandom(m_NPCSounds.AttackSounds, m_NPCSoundsVolume.AttackVolume);
                }

                if (priorityThreat != null)
                {
                    // Send Damage to Threat
                    if (behaviourSettings.damageThreat && IsObjectInSights(sensorSettings.attackFOV, priorityThreat.position) && InDistance(sensorSettings.attackDistance, priorityThreat.position))
                    {
                        if (priorityThreat == playerObject)
                            StartCoroutine(playerController.ApplyKickback(new Vector3(damageSettings.damageKickback.x, damageSettings.damageKickback.y, 0), damageSettings.kickbackTime));

                        if (priorityThreat.HasComponent(out DamageBehaviour damage))
                        {
                            damage.ApplyDamage((int)Random.Range(damageSettings.damageValue.x, damageSettings.damageValue.y));

                            if (!damage.IsAlive())
                                priorityThreat = null;
                        }
                    }
                }
            }
            else if (type == 1 && eventPlayScreamSound)
            {
                //Scream
                PlaySound(m_NPCSounds.ScreamSound, m_NPCSoundsVolume.ScreamVolume);
            }
            else if (type == 2 && eventPlayAgonySound)
            {
                //Agony
                PlaySound(m_NPCSounds.AgonizeSound, m_NPCSoundsVolume.AgonizeVolume);
            }
            else if (type == 3 && eventPlayEatSound)
            {
                //Eat
                PlaySound(m_NPCSounds.EatingSound, m_NPCSoundsVolume.EatingVolume);
            }
        }

        /// <summary>
        /// Callback for Death Event
        /// </summary>
        public override void OnDeath()
        {
            PlaySound(m_NPCSounds.DieSound, m_NPCSoundsVolume.DieVolume);
            SetAnimatorState(scream: false);
            StopAllCoroutines();
            agent.enabled = false;
            animator.enabled = false;
            isDead = true;
        }

        /// <summary>
        /// Search for all threats in the given radius.
        /// </summary>
        Collider[] ThreatsInRadius(float radius)
        {
            return Physics.OverlapSphere(transform.position, radius, threatMask, QueryTriggerInteraction.Collide);
        }

        /// <summary>
        /// Does the NPC see the object from the head position?
        /// </summary>
        bool SeesObject(float distance, Vector3 position)
        {
            if (Vector3.Distance(transform.position, position) <= distance)
            {
                return !Physics.Linecast(npcHead, position, sightMask, QueryTriggerInteraction.Collide);
            }

            return false;
        }

        /// <summary>
        /// Is the object in the NPC Field of View?
        /// </summary>
        bool IsObjectInSights(float FOV, Vector3 position)
        {
            Vector3 dir = position - transform.position;
            return Vector3.Angle(transform.forward, dir) <= FOV * 0.5;
        }

        /// <summary>
        /// Is the object in the Distance?
        /// </summary>
        bool InDistance(float distance, Vector3 position)
        {
            return DistanceOf(position) <= distance;
        }

        /// <summary>
        /// Distance from transform to target
        /// </summary>
        float DistanceOf(Vector3 target)
        {
            return Vector3.Distance(transform.position, target);
        }

        /// <summary>
        /// Destroy specific Behaviour
        /// </summary>
        void DestroyBehaviour(bool patrol = false, bool reaction = false, bool eating = false)
        {
            if (patrol && b_Patrol.isRunning)
            {
                StopCoroutine(b_Patrol.behaviour);
                secondaryPending = false;
                secondaryBehaviour = SecondaryBehaviour.Normal;

                b_Patrol.isRunning = false;
            }

            if (reaction && b_Reaction.isRunning)
            {
                StopCoroutine(b_Reaction.behaviour);
                canReact = true;
                isReaction = false;
                secondaryPending = false;
                secondaryBehaviour = SecondaryBehaviour.Normal;

                b_Reaction.isRunning = false;
            }

            if (eating && b_Eating.isRunning)
            {
                StopCoroutine(b_Eating.behaviour);
                b_Eating.isRunning = false;
                secondaryPending = false;
            }
        }

        void ResetAgentForSecondary()
        {
            agent.ResetPath();
            agent.speed = aiSettings.walkSpeed;
        }

        /// <summary>
        /// Set Reachable Agent Destination
        /// </summary>
        void SetAgentDestination(Vector3? destination, float speed, bool stopAgent = false, bool lerpSpeed = false)
        {
            NavMeshPath path = new NavMeshPath();
            Vector3 dest = destination.Value;

            agent.isStopped = stopAgent;

            if (!lerpSpeed)
            {
                agent.speed = speed;
                isLerpSpeed = false;
            }
            else
            {
                targetAgentSpeed = speed;
                isLerpSpeed = true;
            }

            if (agent.CalculatePath(dest, path))
            {
                agent.SetDestination(dest);
                lastCorrectDestination = dest;
            }
            else
            {
                agent.SetDestination(lastCorrectDestination);
            }
        }

        /// <summary>
        /// Is Agent Path Completed?
        /// </summary>
        bool PathCompleted()
        {
            if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude <= 0.1f && !agent.pathPending)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Can Agent reach the destination?
        /// </summary>
        bool IsPathPossible(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(destination, path);
            return path.status != NavMeshPathStatus.PathPartial && path.status != NavMeshPathStatus.PathInvalid;
        }

        /// <summary>
        /// Trigger Animation State
        /// </summary>
        void SetAnimatorState(bool walk = false, bool run = false, bool patrol = false, bool idle = false, bool scream = false)
        {
            animator.SetBool("Walking", walk);
            animator.SetBool("Running", run);
            animator.SetBool("Patrol", patrol);
            animator.SetBool("Idle", idle);
            animator.SetBool("Scream", scream);
        }

        /// <summary>
        /// Rotate Agent Manually.
        /// </summary>
        void RotateTowards(Vector3 target)
        {
            if (target != Vector3.zero || target != null)
            {
                //Make sure that agent.updateRotation is false
                agent.updateRotation = false;

                //Rotate transform towards target
                Vector3 direction = (target - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, lookRotation, Time.deltaTime * aiSettings.agentRotationSpeed);
            }
        }

        /// <summary>
        /// Find Waypoints by closest point distance.
        /// </summary>
        WaypointGroup FindClosestWaypoints()
        {
            WaypointsData[] waypoints = (from w in FindObjectsOfType<WaypointGroup>()
                                         select new WaypointsData(w, 0)).ToArray();

            for (int i = 0; i < waypoints.Length; i++)
            {
                float distance = 0;

                foreach (var point in waypoints[i].waypointGroup.Waypoints)
                {
                    float newDistance = 0;

                    if ((newDistance = Vector3.Distance(transform.position, point.transform.position)) < distance || distance == 0)
                    {
                        distance = newDistance;
                    }
                }

                waypoints[i].closestDistance = distance;
            }

            return waypoints.OrderBy(x => x.closestDistance).FirstOrDefault().waypointGroup;
        }

        /// <summary>
        /// Find Closest Hunger Point based on NavMeshPath.
        /// </summary>
        HungerPoint FindClosestHungerPoint()
        {
            if (hungerPositions.Length > 0)
            {
                HungerPoint closest = hungerPositions[0];
                float closest_length = CalculatePathLength(closest.transform.position);

                foreach (var point in hungerPositions)
                {
                    float length = 0;

                    if ((length = CalculatePathLength(point.transform.position)) > closest_length)
                    {
                        closest_length = length;
                        closest = point;
                    }
                }

                return closest;
            }

            return null;
        }

        /// <summary>
        /// Calculate NavMeshPath Length to target
        /// </summary>
        float CalculatePathLength(Vector3 targetPosition)
        {
            float pathLength = 0;
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(targetPosition, path);

            Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];
            allWayPoints[0] = transform.position;
            allWayPoints[allWayPoints.Length - 1] = targetPosition;

            for (int i = 0; i < path.corners.Length; i++)
            {
                allWayPoints[i + 1] = path.corners[i];
            }

            for (int i = 0; i < allWayPoints.Length - 1; i++)
            {
                pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
            }

            return pathLength;
        }
        #endregion

        void OnDrawGizmosSelected()
        {
            if (!gizmosSettings.gizmosEnabled) return;

            if (Application.isPlaying)
            {
                Vector3 dir = npcHead - playerHead;
                Gizmos.DrawRay(npcHead, -dir);
            }

            Vector3 pos = transform.position;
            pos += sensorSettings.headOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, 0.05f);

            Vector3 leftRayDirection = Quaternion.AngleAxis(-(sensorSettings.sightsFOV / 2), Vector3.up) * transform.forward;
            Vector3 rightRayDirection = Quaternion.AngleAxis(sensorSettings.sightsFOV / 2, Vector3.up) * transform.forward;

            if (Application.isPlaying && playerObject)
            {
                Gizmos.color = IsObjectInSights(sensorSettings.sightsFOV, playerObject.position) && SeesObject(sensorSettings.sightsDistance, playerHead) ? Color.green : Color.yellow;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawRay(transform.position, leftRayDirection * sensorSettings.sightsDistance);
            Gizmos.DrawRay(transform.position, rightRayDirection * sensorSettings.sightsDistance);

            Vector3 leftAttackRay = Quaternion.AngleAxis(-(sensorSettings.attackFOV / 2), Vector3.up) * transform.forward;
            Vector3 rightAttackRay = Quaternion.AngleAxis(sensorSettings.attackFOV / 2, Vector3.up) * transform.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, leftAttackRay * sensorSettings.attackDistance);
            Gizmos.DrawRay(transform.position, rightAttackRay * sensorSettings.attackDistance);

            if (!gizmosSettings.soundReactionGizmos)
            {
                if (sleepBehaviour != StartingSleep.None)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(transform.position, sensorSettings.idleHearRange);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.position, sensorSettings.veryCloseRange);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, sensorSettings.soundReactRange);
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "position", transform.position },
                { "rotation_y", transform.eulerAngles.y },
                { "sleep_behaviour", (int)sleepBehaviour },
                { "agony_set", setAgonyTime },
                { "hunger_set", setHungerTime },
                { "scream", canScream },
                { "awake", isNPCAwake },
                { "idle", isIdle },
                { "npc_dead", isDead },
                { "scream_time", screamTime },
                { "agony_time", agonyTime },
                { "hunger_time", behaviourSettings.hungerTime },
                { "last_waypoint", lastWaypointPos }
            };
        }

        public void OnLoad(JToken token)
        {
            GetComponent<NavMeshAgent>().Warp(token["position"].ToObject<Vector3>());
            Vector3 rotation = transform.eulerAngles;
            rotation.y = (float)token["rotation_y"];
            transform.eulerAngles = rotation;

            if ((bool)token["npc_dead"])
            {
                isDead = true;
                gameObject.SetActive(false);
            }
            else
            {
                sleepBehaviour = token["sleep_behaviour"].ToObject<StartingSleep>();
                setAgonyTime = (bool)token["agony_set"];
                setHungerTime = (bool)token["hunger_set"];
                canScream = (bool)token["scream"];
                isNPCAwake = (bool)token["awake"];
                isIdle = (bool)token["idle"];

                screamTime = (float)token["scream_time"];
                agonyTime = (float)token["agony_time"];
                behaviourSettings.hungerTime = (float)token["hunger_time"];
                lastWaypointPos = token["last_waypoint"].ToObject<Vector3>();
            }

            goToLastWaypoint = true;
            secondaryPending = false;
        }
    }
}