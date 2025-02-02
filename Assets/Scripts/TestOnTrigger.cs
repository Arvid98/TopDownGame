using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("TriggerTest: OnTriggerEnter kallad av " + collision.gameObject.name);
    }
    
}
