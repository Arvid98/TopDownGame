using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class WarriorAttack : PlayerAttack
{
    [Header("Warrior Attack Settings")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackWidth = 1.5f;
    [SerializeField] private float attackAngle = 90f; // In degrees, for the swing arc
    [SerializeField] private float attackDuration = 0.3f;

    // Visual representation
    [SerializeField] private GameObject swingSprite;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Override default values for warrior
        attackDamage = 30f;
        attackCooldown = 1.2f;

        // Initialize swing sprite if provided
        if (swingSprite != null)
        {
            swingSprite.SetActive(false);
        }
    }

    protected override IEnumerator ExecuteAttack()
    {
        if (!IsServer) yield break;

        // Get facing direction (from animator or last movement)
        Vector2 facingDirection = GetFacingDirection();
        Vector2 attackPosition = (Vector2)transform.position + facingDirection * (attackRange / 2);

        // Show attack zone as a semi-circle in front of player
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Vector3 rotation = new Vector3(0, 0, angle);
        ShowAttackZoneClientRpc(new Vector3(attackRange / 2, 0, 0), new Vector3(attackRange, attackWidth, 1), rotation);

        // Show swing sprite
        ShowSwingClientRpc(true, angle);

        // Visual buildup
        float elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            float t = elapsedTime / attackDuration;
            UpdateAttackZoneColorClientRpc(t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Apply damage
        ApplyDamageInArea(attackPosition, attackRange / 2);

        // Visual completion
        UpdateAttackZoneColorClientRpc(1.0f);
        yield return new WaitForSeconds(0.1f);

        // Hide visuals
        HideAttackZoneClientRpc();
        ShowSwingClientRpc(false, angle);
    }

    private Vector2 GetFacingDirection()
    {
        // Try to get facing direction from animator parameters
        if (animator != null)
        {
            float lastHorizontal = animator.GetFloat("LastHorizontal");
            float lastVertical = animator.GetFloat("LastVertical");

            if (lastHorizontal != 0 || lastVertical != 0)
            {
                return new Vector2(lastHorizontal, lastVertical).normalized;
            }
        }

        // Default to right-facing if no animation data
        return Vector2.right;
    }

    [ClientRpc]
    private void ShowSwingClientRpc(bool show, float angle)
    {
        if (swingSprite != null)
        {
            swingSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
            swingSprite.SetActive(show);
        }
    }

    // More accurate visualization for melee attack
    protected override void ApplyDamageInArea(Vector2 center, float radius)
    {
        if (!IsServer) return;

        Vector2 facingDirection = GetFacingDirection();
        float attackAngleRadians = attackAngle * Mathf.Deg2Rad;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Check if enemy is within the attack arc
                Vector2 directionToEnemy = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                float angleToEnemy = Vector2.Angle(facingDirection, directionToEnemy);

                if (angleToEnemy <= attackAngle / 2)
                {
                    enemy.TakeDamage((int)attackDamage);
                }
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Draw attack range
        Vector3 position = transform.position;
        Vector2 facingDirection = Vector2.right; // Default in editor

        // Draw a semi-circle for the attack arc
        Vector3 attackCenter = position + new Vector3(facingDirection.x, facingDirection.y, 0) * (attackRange / 2);
        Gizmos.DrawWireSphere(attackCenter, attackRange / 2);

        // Visualize the attack arc
        float angleStep = 5f;
        Vector3 previousPoint = position;

        for (float angle = -attackAngle / 2; angle <= attackAngle / 2; angle += angleStep)
        {
            float radians = Mathf.Deg2Rad * angle;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * new Vector3(facingDirection.x, facingDirection.y, 0);
            Vector3 point = position + direction * attackRange;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}