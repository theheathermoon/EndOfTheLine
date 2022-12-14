using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySystem
{
    public class EnemyAi : MonoBehaviour
    {
        //Reference the the NavMeshAgent component's agent (Which is this object just throguh the lens of the navmesh agent)
        public NavMeshAgent agent;
        //Player Transform
        public Transform player;
        //How to detect what can be walked on
        public LayerMask whatIsground, whatIsplayer;

        //PlayerDamageValues
        public int Damage;

        //Differing Behavior
        public bool Wandering;
        private int index = 0;
        public bool InFlashLight;
        Rigidbody m_Rigidbody;
        public float DarknessRange;

        ///Patrolling
        //An array of gameobjects that act as a path of objects for the enemy to follow
        public List<GameObject> patrolPoints;
        public Vector3 walkPoint;
        bool walkPointset;
        public float walkPointrange;


        //Attacking
        public float timeBetweenAttacks;
        bool alreadyAttacked;

        //States
        public float sightRange, attackRange;
        public bool playerInSightRange, playerInAttackRange;

        //Called when the objhect first appears
        private void Awake()
        {
            //Set player to the Player Character
            player = GameObject.Find("Player").transform;
            //Set the NavMeshAgent as the Enemy
            agent = GetComponent<NavMeshAgent>();
            //Default being within the Flashlight as false
            InFlashLight = false;
        }
        //Called every Frame
        private void Update()
        {

            //Check if player is withing sight range or attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsplayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsplayer);



            //If outside of the aggro range, The enemy will keep patrolling
            if (!playerInSightRange && !playerInAttackRange && !InFlashLight) Patrolling();
            //If inside the aggro range, the enemy will chase the player
            if (playerInSightRange && !playerInAttackRange && !InFlashLight) Chasing();
            //If within the attack range, the enemy will attack the player
            if (playerInSightRange && playerInAttackRange && !InFlashLight) Attacking();


        }
        //When a collision is detected
        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Trigger");
            //If the colliding object is the Light Cone
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                //The enemy is now within the flashlight
                InFlashLight = true;
                Debug.Log("InFlashlight");

            }


        }
        //When exiting collision
        private void OnTriggerExit(Collider other)
        {
            //If the object was the Light Cone
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                //the enemy is no longer in the light
                Debug.Log("Outofflashlight");
                InFlashLight = false;
            }
        }
    
    ///This governs all behavior while the enemy is Patrolling
    ///Essentially, the code sees if there is a destination for the enemy. If there is, the enemy moves to it
    ///If there isnt, the enemy finds a random walkpoint
    ///If Wandering is enable, the walkpoints are all random
    ///If Wandering is disabled, the Enemy will follow its patrol paths unless interupted by the player
    public void Patrolling()
        {

            //If no predetermined destination is set, search for one
            if (!walkPointset && Wandering)
            {
                //When there is no destination set, search for a random one within the range
                SearchWalkPoint();

            }
            //If the enemy has predetermined patrolpoint, they will move between them contuously
            if (!walkPointset && !Wandering)
            {
                //Make sure the enemy knows there is a destination set
                walkPointset = true;
                //The destination is set to where the next Patrol point is 
                walkPoint = (patrolPoints[index].transform.position);

            }

            //If there is a destination set
            if (walkPointset)
            {
                //Move to the walkpoint
                agent.SetDestination(walkPoint);

            }

            //Calculate the distance between the current position and the destination
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //This occurs when the destination is reached
            if (distanceToWalkPoint.magnitude < 1f)
            {
                //Reset the destination
                walkPointset = false;

                //This moves to the next point in the Patrol Index if it is available
                if (index >= patrolPoints.Count - 1)
                {
                    index = 0;
                }
                else
                {
                    index = index + 1;
                }
            }

        }
        ///This code finds a random walkpoint within the specified range
        public void SearchWalkPoint()
        {

            //Calculate random point in range
            float randomZ = Random.Range(-walkPointrange, walkPointrange);
            float randomX = Random.Range(-walkPointrange, walkPointrange);

            //Set the walkpoint to this random location
            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            //If the position is a valid place, and the enemy wont get stuck, confirm the destination 
            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsground))
            {
                //Confirms the destination is set
                walkPointset = true;
            }
        }
        //Very simple, When within range, the destination is set tot he player's position so as to continue chasing them
        private void Chasing()
        {
            agent.SetDestination(player.position);
        }
        //This occurs when the enemy is within range to deal damage
        private void Attacking()
        {
            //Makesure enemy doesnt keep moving
            agent.SetDestination(transform.position);

            //Make it so the enemy faces the player
            transform.LookAt(player);

            ///When an attack is available, the enemy will deal damage.
            ///If the enemy hjas recently attacked, it must wait for the specified time before it can attack again
            if (!alreadyAttacked)
            {
                ///Attack Code here
                //GameObject.Find("Player").GetComponent<FirstPersonController>().Health -= 1;





                ///

                //Make sure the enemy knows it has recently attacked
                alreadyAttacked = true;
                //Start the attack reset once the time specified is up
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }
        //Set it so the enemy can attack again
        private void ResetAttack()
        {
            alreadyAttacked = false;
        }

        //Wireframe drawing so you can easily tell the range and dimensions of the enemy
        private void OnDrawGizmosSelected()
        {
            //Draw a red sphere to show the attack radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            //draw a yellow sphere to show the chase radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            //Draw a cube to show the next destination, This one is only really used for bug fixing.
            Gizmos.DrawWireCube(walkPoint, new Vector3(1, 1, 1));

        }
    }
}
