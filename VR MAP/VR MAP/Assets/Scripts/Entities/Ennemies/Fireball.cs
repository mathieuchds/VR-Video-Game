using UnityEngine;


/// <summary>
/// Projectile de boule de feu : gère la trajectoire, dégâts au joueur et destruction après durée.
/// Le prefab doit avoir un Collider (isTrigger ou non) et un Rigidbody (kinematic = false).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Fireball : MonoBehaviour
{
    private Rigidbody rb;
    private float damage = 10f;
    private float lifetime = 6f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, float speed, float dmg, float life)
    {
        damage = dmg;
        lifetime = life;

        Debug.Log($"[Fireball:{name}] Initialize dir={direction} speed={speed} dmg={dmg} life={life}");

        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed; // corrigé : velocity
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning($"[Fireball:{name}] Pas de Rigidbody trouvé !");
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Fireball:{name}] OnTriggerEnter with {other.name} (tag={other.tag})");
        if (other.CompareTag("Player"))
        {
            var ps = other.GetComponent<PlayerStats>();
            if (ps != null)
                ps.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        Debug.Log($"[Fireball:{name}] OnCollisionEnter with {other.name} (tag={other.tag})");
        if (other.CompareTag("Player"))
        {
            var ps = other.GetComponent<PlayerStats>();
            if (ps != null)
                ps.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}