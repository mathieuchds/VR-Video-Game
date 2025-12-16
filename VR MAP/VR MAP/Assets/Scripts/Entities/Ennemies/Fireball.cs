using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float damage = 10f;
    public float speed = 15f;
    private Vector3 direction;

    // ✅ Méthode pour initialiser la direction
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        // ✅ Déplacer manuellement le projectile (ignore toutes les physiques)
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FireBall] Trigger avec {other.name} (tag: {other.tag})");

        // ✅ Ne réagir QUE si c'est le joueur
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(damage);
                Debug.Log($"[FireBall] ✅ {damage} dégâts infligés au joueur !");
            }
            Destroy(gameObject);
        }

        // ✅ Ignorer tout le reste
    }
}