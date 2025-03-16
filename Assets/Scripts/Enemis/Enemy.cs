using UnityEngine;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;
    public float detectionRadius = 10f;
    public float attackRadius = 2f;
    public float attackCooldown = 1.5f;
    public float patrolChangeDirectionTime = 3f;

    private Vector2 patrolDirection;
    private float patrolTimer;
    private float attackTimer;
    private Transform targetPlayer;
    private EnemyAnimationController animController;

    void Start()
    {
        patrolTimer = patrolChangeDirectionTime;
        ChooseNewPatrolDirection();
        attackTimer = 0f;
        animController = GetComponent<EnemyAnimationController>();
    }

    void Update()
    {
        if (!IsServer)
            return;

        attackTimer -= Time.deltaTime;
        targetPlayer = FindClosestPlayer();

        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            if (distance <= attackRadius)
            {
                Attack();
                if (animController != null)
                    animController.SetAttack();
            }
            else if (distance <= detectionRadius)
            {
                Chase();
                if (animController != null)
                    animController.SetChase();
            }
            else
            {
                Patrol();
                if (animController != null)
                    animController.SetPatrol();
            }
        }
        else
        {
            Patrol();
            if (animController != null)
                animController.SetPatrol();
        }
    }

    Transform FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;
        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }
        return closest;
    }

    void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            ChooseNewPatrolDirection();
            patrolTimer = patrolChangeDirectionTime;
        }
        transform.Translate(patrolDirection * patrolSpeed * Time.deltaTime);
    }

    void Chase()
    {
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        transform.Translate(direction * chaseSpeed * Time.deltaTime);
    }

    void Attack()
    {
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
        }
    }

    void ChooseNewPatrolDirection()
    {
        patrolDirection = Random.insideUnitCircle.normalized;
    }
}
