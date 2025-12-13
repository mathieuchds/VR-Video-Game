using UnityEngine;

[RequireComponent(typeof(Light))]
public class Ghost : MonoBehaviour
{
    [Header("Référence Cible")]
    [SerializeField] private GameObject targetObject;
    private Transform target;

    [Header("Paramètres de Mouvement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float floatHeight = 1.5f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatFrequency = 1f;

    [Header("Paramètres de Dégâts")]
    [SerializeField] private float damageRadius = 2.5f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageInterval = 1f;
    private float nextDamageTime;

    [Header("Zone de Dégâts Visuelle")]
    [SerializeField] private Color damageZoneColor = new Color(1f, 0.3f, 0.1f);
    [SerializeField] private float lightIntensity = 3f;

    private Light damageLight;
    private float floatOffset;
    public EnemySpawner spawner;

    [Header("Santé")]
    [SerializeField] private EnemyHealthBar healthBar;
    public float maxHealth = 100f;
    public float health;
    public float flashTime = 0.1f;

    private Renderer rend;
    private Color baseColor;
    private float flashTimer = 0f;

    void Start()
    {
        spawner = FindObjectOfType<EnemySpawner>();

        damageLight = GetComponent<Light>();
        damageLight.type = LightType.Point;
        damageLight.color = damageZoneColor;
        damageLight.range = damageRadius;
        damageLight.intensity = lightIntensity;
        damageLight.shadows = LightShadows.None;

        if (targetObject != null)
            target = targetObject.transform;
        else
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        floatOffset = Random.Range(0f, Mathf.PI * 2f);

        // Health init
        health = maxHealth;
        if (healthBar != null)
            healthBar.SetHealth(1f);

        // Cherche un renderer sur l'objet ou ses enfants (utile si le mesh a été déplacé)
        rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;

        // Si le collider est en mode trigger mais qu'il n'y a pas de Rigidbody,
        // ajouter un Rigidbody kinematic pour permettre OnTriggerEnter/Exit/Stay
        Collider col = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        if (col != null && col.isTrigger && GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + Vector3.up * floatHeight;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float floatY = Mathf.Sin(Time.time * floatFrequency + floatOffset) * floatAmplitude;
        transform.position += Vector3.up * floatY * Time.deltaTime;

        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        CheckDamageZone();

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && rend != null)
                rend.material.color = baseColor;
        }
    }

    private void CheckDamageZone()
    {
        if (Time.time < nextDamageTime) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerStats ps = hitCollider.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    ps.TakeDamage(damageAmount);
                }
                else
                {
                    Debug.Log($"Dégâts appliqués au joueur : {damageAmount} (PlayerStats non trouvé)");
                }

                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    // Permet au fantôme de recevoir des dégâts depuis d'autres scripts (ex : ton arme/projectile doit appeler TakeDamage)
    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (rend != null) rend.material.color = Color.black;
        flashTimer = flashTime;

        if (healthBar != null) healthBar.SetHealth(health / maxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (spawner != null) spawner.EnemyDied();
        Destroy(gameObject);
    }

    // Si tu veux gérer les contacts physiques avec le joueur
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(damageAmount);
            }
        }

        // Note : pour que des triggers fonctionnent, au moins un des deux objets doit avoir un Rigidbody.
        // Nous avons ajouté un Rigidbody kinematic si le collider du fantôme est en trigger.
        // Pour les armes/projectiles : il est préférable que ton script d'arme appelle directement ghost.TakeDamage(dmg)
        // lors d'un impact.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, damageRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

}