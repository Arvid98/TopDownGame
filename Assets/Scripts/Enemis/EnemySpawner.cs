using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnRadius = 10f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.GetComponent<NetworkObject>().Spawn();
        }
    }
}
