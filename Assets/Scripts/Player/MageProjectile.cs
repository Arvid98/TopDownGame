using System.Collections;
using UnityEngine;

// Visual representation of magic projectiles
public class MagicProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private float distanceTraveled;
    private Vector3 startPosition;

    public void Initialize(Vector2 direction, float speed, float maxDistance)
    {
        this.direction = direction;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.startPosition = transform.position;

        // Set rotation to match direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Start movement
        StartCoroutine(MoveProjectile());
    }

    private IEnumerator MoveProjectile()
    {
        while (distanceTraveled < maxDistance)
        {
            Vector3 previousPosition = transform.position;
            transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;

            distanceTraveled += Vector3.Distance(previousPosition, transform.position);

            // Optional: Check for visual-only obstacles like walls
            RaycastHit2D hit = Physics2D.Linecast(previousPosition, transform.position, LayerMask.GetMask("Obstacle"));
            if (hit.collider != null)
            {
                // Show impact effect and destroy
                CreateImpactEffect(hit.point);
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        // Reached max distance
        CreateImpactEffect(transform.position);
        Destroy(gameObject);
    }

    private void CreateImpactEffect(Vector3 position)
    {
        // Create simple impact effect
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.transform.position = position;
        impact.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add material
        Renderer renderer = impact.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = new Color(0.3f, 0.5f, 1f, 0.8f);

        // Remove collider
        Destroy(impact.GetComponent<Collider>());

        // Animate and destroy
        StartCoroutine(AnimateImpact(impact));
    }

    private IEnumerator AnimateImpact(GameObject impact)
    {
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            impact.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1f, 1f, 1f), t);

            Renderer renderer = impact.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1 - t;
                renderer.material.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(impact);
    }
}