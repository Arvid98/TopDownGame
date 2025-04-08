using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private float avoidanceWeight = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack Settings")]
    [SerializeField] private AttackPattern attackPattern = AttackPattern.Basic;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackWidthSetting = 2f;
    [SerializeField] private float attackLengthSetting = 1f;

    // Networked variables
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false);

    // Private variables
    private Transform currentTarget;
    private bool canAttack = true;
    private GameObject attackZone;
    private Material attackZoneMaterial;
    private Color startColor = new Color(1, 0, 0, 0.1f); // Much more transparent start
    private Color endColor = new Color(1, 0, 0, 0.8f);   // Not fully opaque end

    // Attack dimensions used by attack methods
    private float attackWidthValue = 2f;
    private float attackLengthValue = 1f;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        // Store serialized values
        attackWidthValue = attackWidthSetting;
        attackLengthValue = attackLengthSetting;

        // Create attack zone on all clients after network spawn
        SetupAttackZone();
    }

    private void SetupAttackZone()
    {
        attackZone = GameObject.CreatePrimitive(PrimitiveType.Quad);
        attackZone.transform.SetParent(transform);
        attackZone.SetActive(false);

        // Set up attack zone material with transparency
        attackZoneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (attackZoneMaterial == null)
        {
            // Fallback for built-in render pipeline
            attackZoneMaterial = new Material(Shader.Find("Standard"));
            if (attackZoneMaterial == null)
            {
                // Ultimate fallback
                attackZoneMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        // Set material properties for transparency
        attackZoneMaterial.color = startColor;
        attackZoneMaterial.SetFloat("_Mode", 3); // Transparent mode
        attackZoneMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        attackZoneMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        attackZoneMaterial.SetInt("_ZWrite", 0);
        attackZoneMaterial.DisableKeyword("_ALPHATEST_ON");
        attackZoneMaterial.EnableKeyword("_ALPHABLEND_ON");
        attackZoneMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        attackZoneMaterial.renderQueue = 3000;

        // Apply material to the renderer
        var renderer = attackZone.GetComponent<Renderer>();
        renderer.material = attackZoneMaterial;

        // Remove collider from the attack zone (we'll handle hits manually)
        Destroy(attackZone.GetComponent<Collider>());

        Debug.Log($"Attack zone created on {(IsServer ? "server" : "client")}: {(attackZone != null)}");
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsServer) return;

        FindClosestPlayer();

        if (currentTarget != null)
        {
            // Update the networked position to sync with clients
            targetPosition.Value = currentTarget.position;

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // If in attack range and can attack, initiate attack
            if (distanceToTarget <= attackRange && canAttack)
            {
                StartCoroutine(PerformAttack());
            }
            // Otherwise move towards the target
            else if (distanceToTarget <= detectionRange && !isAttacking.Value)
            {
                MoveTowardsTarget();
            }
        }
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        currentTarget = closestPlayer;
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;

        // Raycast for obstacle avoidance
        Vector3 avoidanceDirection = CalculateAvoidanceDirection();

        // Combine the target direction with avoidance
        Vector3 finalDirection = (direction + avoidanceDirection * avoidanceWeight).normalized;

        // Move the enemy
        transform.position += finalDirection * moveSpeed * Time.deltaTime;

        // Store direction for attack orientation (without changing rotation)
        targetPosition.Value = currentTarget.position;
    }

    private Vector3 CalculateAvoidanceDirection()
    {
        Vector3 avoidanceDirection = Vector3.zero;

        // Forward raycast
        Vector3 forwardDir = transform.right; // In 2D, right is forward for sprites
        if (Physics2D.Raycast(transform.position, forwardDir, raycastDistance, obstacleLayer))
        {
            avoidanceDirection -= forwardDir;
        }

        // Left diagonal raycast
        Vector3 leftDir = Quaternion.Euler(0, 0, 30) * forwardDir;
        if (Physics2D.Raycast(transform.position, leftDir, raycastDistance, obstacleLayer))
        {
            avoidanceDirection -= leftDir;
        }

        // Right diagonal raycast
        Vector3 rightDir = Quaternion.Euler(0, 0, -30) * forwardDir;
        if (Physics2D.Raycast(transform.position, rightDir, raycastDistance, obstacleLayer))
        {
            avoidanceDirection -= rightDir;
        }

        return avoidanceDirection.normalized;
    }

    private IEnumerator PerformAttack()
    {
        if (!IsServer) yield break;

        // Set attack state for network sync
        isAttacking.Value = true;

        switch (attackPattern)
        {
            case AttackPattern.Basic:
                yield return BasicAttack();
                break;
            case AttackPattern.AreaOfEffect:
                yield return AreaOfEffectAttack();
                break;
            case AttackPattern.MultiStrike:
                yield return MultiStrikeAttack();
                break;
        }

        // Reset attack state
        isAttacking.Value = false;
    }

    private IEnumerator BasicAttack()
    {
        canAttack = false;

        // Store target position at attack initiation
        Vector3 attackTargetPosition = targetPosition.Value;
        Vector3 directionToTarget = (attackTargetPosition - transform.position).normalized;

        // Make zone visible via ClientRpc with fixed target position
        ShowAttackZoneClientRpc(transform.position, new Vector3(attackLengthValue, attackWidthValue, 1f), Vector3.zero, attackTargetPosition);

        // Warning phase
        float elapsedTime = 0f;
        while (elapsedTime < warningDuration)
        {
            float t = elapsedTime / warningDuration;
            UpdateAttackZoneColorClientRpc(t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Damage phase
        UpdateAttackZoneColorClientRpc(1.0f); // Full red

        // Get the actual position of the attack zone for damage calculation
        Vector3 attackPosition = attackZone.transform.position;
        ApplyDamageInZone(attackPosition, attackWidthValue / 2f);

        // Clean up
        yield return new WaitForSeconds(0.2f);
        HideAttackZoneClientRpc();

        // Cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator AreaOfEffectAttack()
    {
        canAttack = false;

        // Store target position at attack initiation
        Vector3 attackTargetPosition = targetPosition.Value;

        // Make zone visible via ClientRpc
        ShowAttackZoneClientRpc(transform.position, new Vector3(attackLengthValue * 1.5f, attackWidthValue * 1.5f, 1f), Vector3.zero, attackTargetPosition);

        // Warning phase
        float elapsedTime = 0f;
        float extendedWarningDuration = warningDuration * 1.5f; // Longer warning for AoE
        while (elapsedTime < extendedWarningDuration)
        {
            float t = elapsedTime / extendedWarningDuration;
            UpdateAttackZoneColorClientRpc(t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Damage phase
        UpdateAttackZoneColorClientRpc(1.0f); // Full red
        ApplyDamageInZone(attackZone.transform.position, Mathf.Max(attackWidthValue, attackLengthValue) * 0.75f);

        // Clean up
        yield return new WaitForSeconds(0.3f);
        HideAttackZoneClientRpc();

        // Cooldown
        yield return new WaitForSeconds(attackCooldown * 1.5f); // Longer cooldown for AoE
        canAttack = true;
    }

    private IEnumerator MultiStrikeAttack()
    {
        canAttack = false;

        // Store target position at attack initiation
        Vector3 attackTargetPosition = targetPosition.Value;

        // Perform three quick strikes
        for (int i = 0; i < 3; i++)
        {
            // Show attack zone via ClientRpc
            ShowAttackZoneClientRpc(transform.position, new Vector3(attackLengthValue * 0.75f, attackWidthValue * 0.75f, 1f), Vector3.zero, attackTargetPosition);

            // Quick warning phase
            float elapsedTime = 0f;
            float quickWarningDuration = warningDuration * 0.6f;
            while (elapsedTime < quickWarningDuration)
            {
                float t = elapsedTime / quickWarningDuration;
                UpdateAttackZoneColorClientRpc(t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Damage phase
            UpdateAttackZoneColorClientRpc(1.0f); // Full red
            ApplyDamageInZone(attackZone.transform.position, attackWidthValue * 0.4f);

            // Clean up
            yield return new WaitForSeconds(0.15f);
            HideAttackZoneClientRpc();

            // Delay between strikes
            if (i < 2) yield return new WaitForSeconds(0.3f);
        }

        // Cooldown
        yield return new WaitForSeconds(attackCooldown * 1.2f);
        canAttack = true;
    }

    private void ApplyDamageInZone(Vector3 center, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Notify the player they've been hit using a ClientRPC
                NetworkObject playerNetObj = hit.GetComponent<NetworkObject>();
                if (playerNetObj != null)
                {
                    ApplyDamageClientRpc(playerNetObj.OwnerClientId, attackDamage);
                }
            }
        }
    }

    [ClientRpc]
    private void ApplyDamageClientRpc(ulong clientId, float damage)
    {
        // This will run on all clients
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Get player health component and apply damage
            // Example: hit.GetComponent<PlayerHealth>().TakeDamage(damage);
            Debug.Log($"Player {clientId} took {damage} damage!");
        }
    }

    [ClientRpc]
    private void ShowAttackZoneClientRpc(Vector3 position, Vector3 scale, Vector3 rotation, Vector3 targetPos)
    {
        if (attackZone == null)
        {
            Debug.LogError("Attack zone is null on client");
            return;
        }

        // Calculate direction to face the fixed target position
        Vector3 direction = targetPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation = new Vector3(0, 0, angle);

        // Set position in front of the enemy, aligned with the forward direction
        // The attack zone's pivot is now at the enemy's position, extending forward only
        attackZone.transform.position = position;
        attackZone.transform.localScale = scale;
        attackZone.transform.rotation = Quaternion.Euler(rotation);

        // Move the zone forward so it's fully in front of the enemy
        Vector3 forwardOffset = Quaternion.Euler(0, 0, rotation.z) * Vector3.right * scale.x * 0.5f;
        attackZone.transform.position += forwardOffset;

        attackZone.SetActive(true);

        // Reset color to starting color
        if (attackZoneMaterial != null)
        {
            attackZoneMaterial.color = startColor;
        }
    }

    [ClientRpc]
    private void UpdateAttackZoneColorClientRpc(float t)
    {
        if (attackZoneMaterial != null)
        {
            // Ensure we're interpolating properly in the alpha channel
            Color newColor = Color.Lerp(startColor, endColor, t);
            attackZoneMaterial.color = newColor;
        }
    }

    [ClientRpc]
    private void HideAttackZoneClientRpc()
    {
        if (attackZone != null)
        {
            attackZone.SetActive(false);
        }
    }

    private void UpdateAttackZoneDirection()
    {
        if (!IsServer || currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 scale = attackZone.transform.localScale;
        Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * scale.x * 0.5f;

        // Update attack zone orientation
        attackZone.transform.position = transform.position + offset;
        attackZone.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // For testing purposes only
    private IEnumerator TestColorTransition()
    {
        yield return new WaitForSeconds(2f);
        Vector3 testTarget = transform.position + Vector3.right * 5f; // Dummy target position
        ShowAttackZoneClientRpc(transform.position, new Vector3(3f, 2f, 1f), Vector3.zero, testTarget);

        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            UpdateAttackZoneColorClientRpc(t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        UpdateAttackZoneColorClientRpc(1.0f);
        yield return new WaitForSeconds(1f);
        HideAttackZoneClientRpc();
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!IsServer) return;

        // Notify all clients this enemy is dead
        NotifyDeathClientRpc();

        // Destroy the network object
        Destroy(gameObject, 0.1f);
    }

    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        // Play death effects on all clients
        Debug.Log("Enemy defeated!");
        // Add visual effects, sound, etc.
    }

    // Attack patterns enum
    public enum AttackPattern
    {
        Basic,
        AreaOfEffect,
        MultiStrike
    }

    // Draw gizmos for visualization in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw obstacle detection rays
        Gizmos.color = Color.blue;
        Vector3 forwardDir = transform.right;
        Vector3 leftDir = Quaternion.Euler(0, 0, 30) * forwardDir;
        Vector3 rightDir = Quaternion.Euler(0, 0, -30) * forwardDir;

        Gizmos.DrawRay(transform.position, forwardDir * raycastDistance);
        Gizmos.DrawRay(transform.position, leftDir * raycastDistance);
        Gizmos.DrawRay(transform.position, rightDir * raycastDistance);
    }
}