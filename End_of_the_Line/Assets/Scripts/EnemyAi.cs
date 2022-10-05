using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsground, whatIsplayer;
    //Differing Behavior
    public bool Wandering;

    private int index = 1;


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

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

    }

    private void Update()
    {
        //Check if player is withing sight range or attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsplayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsplayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) Chasing();
        if (playerInSightRange && playerInAttackRange) Attacking();

    }
    private void Patrolling()
    {

        //If no predetermined walkpoint is set, search for one
        if (!walkPointset && Wandering)
        {
            SearchWalkPoint();
            Debug.Log("Searching");
        }

        if (!walkPointset && !Wandering)
        {
            walkPointset = true;
            walkPoint = (patrolPoints[index].transform.position);
            Debug.Log("Found");
        }


        if (walkPointset)
        {
            Debug.Log("WalkPointSet");
            agent.SetDestination(walkPoint);
        }
            

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointset = false;
        }
            
    }
    //Find a random walkpoint within the range
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointrange, walkPointrange);
        float randomX = Random.Range(-walkPointrange, walkPointrange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsground))
            Debug.Log("WalkpointFound");
            walkPointset = true;
    }

    private void Chasing()
    {
        agent.SetDestination(player.position);
    }

    private void Attacking()
    {
        //Makesure enemy doesnt move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            ///Attack Code here
            
            
            
            
            
            
            ///

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.DrawWireCube(walkPoint, new Vector3(1,1,1));
    }
}
