using UnityEngine;

public class FlameHitbox : MonoBehaviour
{


    private void OnTriggerStay(Collider other)
    {
        Debug.Log("FlameHitbox detected collision with: " + other.name);

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.ApplyBurn();
    }
}
