using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    // Networked variables
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(false);

    // Visual feedback
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine damageFlashCoroutine;

    public bool IsDead => currentHealth.Value <= 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(int damage, ulong attackerClientId)
    {
        if (!IsServer || IsDead || isInvulnerable.Value) return;

        currentHealth.Value -= damage;
        isInvulnerable.Value = true;

        // Visual feedback for all clients
        DamageVisualClientRpc();

        StartCoroutine(ResetInvulnerability());

        if (currentHealth.Value <= 0)
        {
            Die(attackerClientId);
        }
    }

    [ClientRpc]
    private void DamageVisualClientRpc()
    {
        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine = StartCoroutine(FlashDamage());
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer == null) yield break;

        // Flash red
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        // Return to original color
        spriteRenderer.color = originalColor;

        damageFlashCoroutine = null;
    }

    private IEnumerator ResetInvulnerability()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable.Value = false;
    }

    private void Die(ulong attackerClientId)
    {
        // Notify all clients this player died
        PlayerDiedClientRpc(attackerClientId);

        // Additional death logic can be added here
        // For example, respawn timer, score update, etc.
    }

    [ClientRpc]
    private void PlayerDiedClientRpc(ulong attackerClientId)
    {
        if (IsOwner)
        {
            Debug.Log("You died!");
            // Handle local player death (UI, input blocking, etc.)
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} was defeated by player {attackerClientId}!");
        }

        // Play death animation or effects
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
    }

    // For healing items or abilities
    public void Heal(int amount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
    }

    // For UI to display current health
    public int GetCurrentHealth()
    {
        return currentHealth.Value;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}