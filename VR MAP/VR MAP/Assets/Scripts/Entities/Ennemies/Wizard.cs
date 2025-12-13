using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ennemi "Sorcier" : se déplace vers le joueur mais s'arrête à une distance minimale.
/// Tire des boules de feu en direction du joueur quand il est à portée (<= stopDistance).
/// Le spawn du projectile DOIT être effectué depuis l'Animation Event qui appelle ShootAtTarget().
/// </summary>
[RequireComponent(typeof(NavMeshAgent))
]
public class Wizard : Enemy
{
    [Header("Références")]
    [SerializeField] private GameObject targetObject;
    private Transform target;
    private NavMeshAgent agent;

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Animator animator;

    [Header("Distances")]
    [SerializeField] private float stopDistance = 12f; // distance à garder (tir quand <=)

    [Header("Attaque")]
    [SerializeField] private float attackCooldown = 2f;
    [Tooltip("Cooldown entre attaques (le spawn est déclenché par l'Animation Event).")]
    [SerializeField] private float attackDelayAfterTrigger = 0.15f;

    [Header("Projectile par défaut")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileDamage = 15f;
    [SerializeField] private float projectileLifetime = 6f;

    private static readonly int ParamIsRunning = Animator.StringToHash("IsRunning");
    private static readonly int ParamShoot = Animator.StringToHash("Shoot"); // trigger

    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    // logs / petite marge anti-oscillation
    private const float epsilon = 0.05f;
    [Header("Debug")]
    [SerializeField] private float debugLogInterval = 2f;
    private float debugLogTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targetObject != null)
            target = targetObject.transform;
        else
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (target == null)
            Debug.LogWarning($"[Wizard:{name}] Aucun target trouvé (tag Player ou targetObject).");
        else
            Debug.Log($"[Wizard:{name}] Target trouvé : {target.name}");

        Debug.Log($"[Wizard:{name}] stopDistance={stopDistance}, attackCooldown={attackCooldown}");

        if (firePoint == null)
            firePoint = transform;

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        debugLogTimer = debugLogInterval;
    }

    void Update()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        // Déplacement : avancer seulement si on est plus loin que stopDistance (+ epsilon)
        bool shouldRun = dist > (stopDistance + epsilon);
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (shouldRun)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            else
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        // Mettre à jour l'Animator (Run/Idle)
        if (animator != null)
            animator.SetBool(ParamIsRunning, shouldRun);

        // Attaque : tirer quand le joueur est à stopDistance ou plus proche
        bool canAttack = dist <= (stopDistance + epsilon);

        if (canAttack && !isAttacking)
        {
            Debug.Log($"[Wizard:{name}] Entrée zone d'attaque (dist={dist:F2}). Démarrage AttackLoop.");
            attackCoroutine = StartCoroutine(AttackLoop());
        }
        else if (!canAttack && isAttacking)
        {
            Debug.Log($"[Wizard:{name}] Sortie zone d'attaque (dist={dist:F2}). Arrêt AttackLoop.");
            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            isAttacking = false;
        }

        // Rotation vers le joueur (douce)
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 6f);

        // Debug périodique
        if (debugLogInterval > 0f)
        {
            debugLogTimer -= Time.deltaTime;
            if (debugLogTimer <= 0f)
            {
                Debug.Log($"[WizardStatus:{name}] pos={transform.position} dist={dist:F2} shouldRun={shouldRun} canAttack={canAttack} isAttacking={isAttacking} attackCoroutine={(attackCoroutine!=null)}");
                debugLogTimer = debugLogInterval;
            }
        }
    }

    private System.Collections.IEnumerator AttackLoop()
    {
        isAttacking = true;
        while (true)
        {
            if (animator != null)
            {
                Debug.Log($"[Wizard:{name}] Déclenche trigger Shoot sur Animator.");
                animator.SetTrigger(ParamShoot);

                // Ne pas spawner depuis le script : le projectile DOIT être instancié par l'Animation Event
                // On attend un petit délai pour laisser l'animation démarrer (optionnel)
                yield return new WaitForSeconds(attackDelayAfterTrigger);
            }
            else
            {
                // Si pas d'Animator (fallback) on déclenche directement le spawn
                ShootAtTarget();
            }

            // cooldown entre attaques
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    // Méthode publique appelée depuis un Animation Event sur le clip d'attaque.
    // L'Animation Event doit appeler exactement "ShootAtTarget" sur le GameObject qui contient ce script.
    public void ShootAtTarget()
    {
        Debug.Log($"[Wizard:{name}] ShootAtTarget() appelé (via Animation Event).");
        if (target == null || fireballPrefab == null)
        {
            Debug.LogWarning($"[Wizard:{name}] Impossible de tirer : target={(target==null)}, fireballPrefab={(fireballPrefab==null)}");
            return;
        }

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + transform.forward * 1f + Vector3.up * 1f;
        Vector3 aimPoint = target.position + Vector3.up * 1f;
        Vector3 dir = (aimPoint - spawnPos).normalized;

        GameObject fb = Instantiate(fireballPrefab, spawnPos, Quaternion.LookRotation(dir));
        Fireball fbScript = fb.GetComponent<Fireball>();
        if (fbScript != null)
        {
            fbScript.Initialize(dir, projectileSpeed, projectileDamage, projectileLifetime);
        }
        else
        {
            var rb = fb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
                Debug.Log($"[Wizard:{name}] Projectile fallback: applied rb.velocity.");
            }
            else
            {
                Debug.LogWarning($"[Wizard:{name}] Projectile instancié sans Rigidbody ni Fireball script.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}