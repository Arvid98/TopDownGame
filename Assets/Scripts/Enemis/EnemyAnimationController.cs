using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetPatrol()
    {
        animator.SetBool("isPatrolling", true);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
    }

    public void SetChase()
    {
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isChasing", true);
        animator.SetBool("isAttacking", false);
    }

    public void SetAttack()
    {
        animator.SetBool("isPatrolling", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", true);
    }
}
