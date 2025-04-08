using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class MageAttack : PlayerAttack
{
    [Header("Mage Attack Settings")]
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float maxRange = 10f;
    [SerializeField] private GameObject magicBoltPrefab;
    [SerializeField] private Transform spellCastPoint;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Override default values for mage
        attackDamage = 15f;
        attackCooldown = 0.8f;

        // Create cast point if not assigned
        if (spellCastPoint == null)
        {
            GameObject castPoint = new GameObject("SpellCastPoint");
            castPoint.transform.SetParent(transform);
            castPoint.transform.localPosition = new Vector3(0.5f, 0, 0); // Slightly in front of player
            spellCastPoint = castPoint.transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        // Additional input for aiming if needed
        if (IsOwner)
        {
            // Can implement mouse aiming here if desired
        }
    }

    protected override IEnumerator ExecuteAttack()
    {
        if (!IsServer) yield break;

        // Get aim direction - can be mouse direction or character facing
        Vector2 aimDirection = GetAimDirection();

        // Visual charge-up effect
        Vector3 castPosition = spellCastPoint.position;
        Vector3 rotation = new Vector3(0, 0, Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg);
        ShowAttackZoneClientRpc(new Vector3(0.5f, 0, 0), new Vector3(1f, 1f, 1), rotation);

        float chargeDuration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            float t = elapsedTime / chargeDuration;
            UpdateAttackZoneColorClientRpc(t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hide charge effect
        HideAttackZoneClientRpc();

        // Spawn projectile
        SpawnProjectileClientRpc(castPosition, aimDirection);

        // Server-side projectile for damage calculation
        StartCoroutine(ProjectileTravel(castPosition, aimDirection));
    }

    private IEnumerator ProjectileTravel(Vector3 startPos, Vector2 direction)
    {
        float distance = 0;
        Vector3 position = startPos;
        bool hitSomething = false;

        // Move projectile until it hits max range
        while (distance < maxRange && !hitSomething)
        {
            Vector3 previousPosition = position;
            position += new Vector3(direction.x, direction.y, 0) * projectileSpeed * Time.deltaTime;
            distance += Vector3.Distance(previousPosition, position);

            // Check for collisions
            RaycastHit2D hit = Physics2D.Linecast(previousPosition, position);
            if (hit.collider != null)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Apply damage to enemy
                    enemy.TakeDamage((int)attackDamage);
                    hitSomething = true;

                    // Visual effect at hit point
                    ShowImpactEffectClientRpc(hit.point);
                }
            }

            yield return null;
        }

        if (!hitSomething)
        {
            // Visual effect for max range
            ShowImpactEffectClientRpc(position);
        }
    }

    private Vector2 GetAimDirection()
    {
        // For mouse aiming
        if (IsOwner)
        {
            // Use aim direction from InputManager
            return InputManager.AimDirection;
        }

        // Fallback to facing direction from animator
        if (animator != null)
        {
            float lastHorizontal = animator.GetFloat("LastHorizontal");
            float lastVertical = animator.GetFloat("LastVertical");

            if (lastHorizontal != 0 || lastVertical != 0)
            {
                return new Vector2(lastHorizontal, lastVertical).normalized;
            }
        }

        // Default right-facing
        return Vector2.right;
    }

    [ClientRpc]
    private void SpawnProjectileClientRpc(Vector3 position, Vector2 direction)
    {
        // Instantiate visual projectile
        if (magicBoltPrefab != null)
        {
            GameObject projectile = Instantiate(magicBoltPrefab, position, Quaternion.identity);
            MagicProjectile projectileScript = projectile.AddComponent<MagicProjectile>();
            projectileScript.Initialize(direction, projectileSpeed, maxRange);
        }
        else
        {
            // Fallback if no prefab is assigned
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            projectile.transform.position = position;

            // Add custom component for visual movement
            MagicProjectile projectileScript = projectile.AddComponent<MagicProjectile>();
            projectileScript.Initialize(direction, projectileSpeed, maxRange);

            // Add trail renderer for effect
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.color = new Color(0.3f, 0.5f, 1f);

            // Remove collider (handled on server)
            Destroy(projectile.GetComponent<Collider>());
        }
    }

    [ClientRpc]
    private void ShowImpactEffectClientRpc(Vector2 position)
    {
        // Create impact effect
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.transform.position = position;
        impact.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add material
        Renderer renderer = impact.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = new Color(0.3f, 0.5f, 1f, 0.8f);

        // Remove collider
        Destroy(impact.GetComponent<Collider>());

        // Destroy after animation
        float duration = 0.3f;
        StartCoroutine(ScaleAndDestroy(impact, duration));
    }

    private IEnumerator ScaleAndDestroy(GameObject obj, float duration)
    {
        float elapsed = 0;
        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = startScale * 2;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade out
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color color = renderer.material.color;
                color.a = 1 - t;
                renderer.material.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        // Draw attack range
        Gizmos.DrawWireSphere(transform.position, maxRange);

        // Draw aimed direction if in play mode
        if (Application.isPlaying && IsOwner)
        {
            Vector2 direction = GetAimDirection();
            Gizmos.DrawRay(transform.position, direction * maxRange);
        }
    }
}