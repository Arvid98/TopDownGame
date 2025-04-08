using System.Collections;
using UnityEngine;
using Unity.Netcode;

// Abstract base class for different player attack types
public abstract class PlayerAttack : NetworkBehaviour
{
    [Header("Base Attack Settings")]
    [SerializeField] protected float attackCooldown = 1.0f;
    [SerializeField] protected float attackDamage = 20f;

    // Attack state tracking
    protected NetworkVariable<bool> isAttacking = new NetworkVariable<bool>(false);
    protected NetworkVariable<bool> canAttack = new NetworkVariable<bool>(true);

    // Visual attack zone (similar to enemy implementation)
    protected GameObject attackZone;
    protected Material attackZoneMaterial;
    protected Color startColor = new Color(1, 1, 0, 0.1f);
    protected Color endColor = new Color(1, 1, 0, 0.8f);

    // Input handling
    protected bool attackInput;

    // References
    protected Animator animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        animator = GetComponent<Animator>();

        if (IsServer)
        {
            canAttack.Value = true;
        }

        // Set up attack visuals
        SetupAttackZone();
    }

    protected virtual void SetupAttackZone()
    {
        // Base implementation - can be overridden by specific attack types
        attackZone = GameObject.CreatePrimitive(PrimitiveType.Quad);
        attackZone.transform.SetParent(transform);
        attackZone.SetActive(false);

        // Set up material with transparency (same approach as Enemy.cs)
        attackZoneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (attackZoneMaterial == null)
        {
            attackZoneMaterial = new Material(Shader.Find("Standard"));
            if (attackZoneMaterial == null)
            {
                attackZoneMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        attackZoneMaterial.color = startColor;
        attackZoneMaterial.SetFloat("_Mode", 3);
        attackZoneMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        attackZoneMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        attackZoneMaterial.SetInt("_ZWrite", 0);
        attackZoneMaterial.DisableKeyword("_ALPHATEST_ON");
        attackZoneMaterial.EnableKeyword("_ALPHABLEND_ON");
        attackZoneMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        attackZoneMaterial.renderQueue = 3000;

        var renderer = attackZone.GetComponent<Renderer>();
        renderer.material = attackZoneMaterial;

        Destroy(attackZone.GetComponent<Collider>());
    }

    protected virtual void Update()
    {
        // Only process input on owner client
        if (!IsOwner) return;

        // Check for attack input using InputManager
        attackInput = InputManager.Attack;

        if (attackInput && canAttack.Value && !isAttacking.Value)
        {
            RequestAttackServerRpc();
        }
    }

    [ServerRpc]
    protected virtual void RequestAttackServerRpc()
    {
        if (!canAttack.Value || isAttacking.Value) return;

        // Begin attack process
        StartCoroutine(PerformAttack());
    }

    protected virtual IEnumerator PerformAttack()
    {
        if (!IsServer) yield break;

        // Set attack states
        isAttacking.Value = true;
        canAttack.Value = false;

        // Play animation
        TriggerAttackAnimationClientRpc();

        // Execute specific attack type implementation
        yield return ExecuteAttack();

        // Reset cooldown
        yield return new WaitForSeconds(attackCooldown);

        // Reset states
        isAttacking.Value = false;
        canAttack.Value = true;
    }

    // To be implemented by specific attack types
    protected abstract IEnumerator ExecuteAttack();

    [ClientRpc]
    protected virtual void TriggerAttackAnimationClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    [ClientRpc]
    protected virtual void ShowAttackZoneClientRpc(Vector3 position, Vector3 scale, Vector3 rotation)
    {
        if (attackZone == null) return;

        attackZone.transform.localPosition = position;
        attackZone.transform.localScale = scale;
        attackZone.transform.localRotation = Quaternion.Euler(rotation);
        attackZone.SetActive(true);

        if (attackZoneMaterial != null)
        {
            attackZoneMaterial.color = startColor;
        }
    }

    [ClientRpc]
    protected virtual void UpdateAttackZoneColorClientRpc(float t)
    {
        if (attackZoneMaterial != null)
        {
            attackZoneMaterial.color = Color.Lerp(startColor, endColor, t);
        }
    }

    [ClientRpc]
    protected virtual void HideAttackZoneClientRpc()
    {
        if (attackZone != null)
        {
            attackZone.SetActive(false);
        }
    }

    // Apply damage to enemies in a given area
    protected virtual void ApplyDamageInArea(Vector2 center, float radius)
    {
        if (!IsServer) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)attackDamage);
            }
        }
    }

    // For drawing debug info in the editor
    protected virtual void OnDrawGizmosSelected()
    {
        // Can be implemented by derived classes to visualize attack range
    }
}