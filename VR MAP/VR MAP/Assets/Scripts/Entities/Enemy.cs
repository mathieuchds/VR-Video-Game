using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyHealthBar healthBar;
    public float maxHealth = 100f;
    public float health;
    public float contactDamage = 10f;

    public float flashTime = 0.1f;

    private Renderer rend;
    private Color baseColor;
    private float flashTimer = 0f;
    public Rigidbody rb;

    public EnemySpawner spawner;

    UnityEngine.AI.NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Si l'ennemi est pilot� par NavMeshAgent, on pr�f�re que le Rigidbody soit kinematic
        // (�vite que l'agent "passe � travers" des obstacles g�r�s par la NavMesh).
        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
            if (rb != null)
                rb.isKinematic = true;
        }

        health = maxHealth;
        healthBar.SetHealth(1f);

        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0 && rend != null)
                rend.material.color = baseColor;
        }
    }

    public void Knockback(Vector3 direction, float force, float duration)
    {
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector3 dir, float force, float duration)
    {
        dir.y = 0f;
        dir.Normalize();

        if (rb == null)
            yield break;

        // D�sactiver temporairement l'agent pour permettre la physique,
        // rendre le Rigidbody non-kinematic, appliquer la force puis restaurer.
        bool hadAgent = agent != null && agent.enabled;
        if (hadAgent) agent.enabled = false;

        rb.isKinematic = false;
        rb.AddForce(dir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (hadAgent) agent.enabled = true;
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunRoutine(agent != null ? agent.speed : 0f, duration));
    }

    private IEnumerator StunRoutine(float baseSpeed, float duration)
    {
        if (agent != null)
            agent.speed = 0f;

        yield return new WaitForSeconds(duration);

        if (agent != null)
            agent.speed = baseSpeed;
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        // flash
        if (rend != null) rend.material.color = Color.black;
        flashTimer = flashTime;

        if (healthBar != null) healthBar.SetHealth(health / maxHealth);

        if (health <= 0f)
        {
            if (spawner != null) spawner.EnemyDied();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(contactDamage);
            }
        }
    }
}
