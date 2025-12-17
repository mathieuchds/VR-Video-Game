using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Petit démon volant :  se déplace rapidement vers le joueur. 
/// Quand proche, dash à travers lui en infligeant des dégâts, puis se repose (chill).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Devil : Enemy
{
    [Header("Références")]
    [SerializeField] private GameObject targetObject;
    private Transform target;
    [SerializeField] private Animator animator;

    [Header("Distances & Zones")]
    [SerializeField] private float outerCircleRadius = 15f; // Rayon du grand cercle (zone d'approche)
    [SerializeField] private float innerCircleRadius = 5f;  // Rayon du petit cercle (zone de charge)

    [Header("Déplacement")]
    [SerializeField] private float normalSpeed = 6f;        // Vitesse normale d'approche
    [SerializeField] private float dashSpeed = 25f;         // Vitesse du dash

    [Header("Combat")]
    [SerializeField] private float dashDamage = 15f;        // Dégâts du dash
    [SerializeField] private float chillDuration = 1.5f;    // Temps de repos après dash
    [SerializeField] private float dashHitRadius = 1.5f;    // Rayon de détection pendant le dash

    [Header("Cooldowns")]
    [SerializeField] private float dashCooldown = 3f;       // Cooldown entre deux dashs

    // États pour l'Animator
    private static readonly int ParamIsRunning = Animator.StringToHash("IsRunning");
    private static readonly int ParamIsCharging = Animator.StringToHash("IsCharging");
    private static readonly int ParamIsChilling = Animator.StringToHash("IsChilling");
    private static readonly int TriggerHit = Animator.StringToHash("Hit");

    // État interne
    private enum DevilState
    {
        Idle,           // Repos
        Approaching,    // Se déplace vers le joueur (IsRunning)
        Charging,       // En train de dasher (IsCharging)
        Chilling        // Repos après dash (IsChilling)
    }

    private DevilState currentState = DevilState.Idle;
    private bool canDash = true;
    private bool hasHitPlayerThisDash = false;
    private Vector3 dashDirection;
    private Vector3 dashEndPosition;
    private Vector3 dashStartPosition;
    private float dashProgress = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (agent != null)
        {
            agent.speed = normalSpeed;
            agent.updatePosition = true;
            agent.updateRotation = false; // On gère la rotation manuellement

            if (rb != null)
                rb.isKinematic = true;
        }

        health = maxHealth;

        if (healthBar != null)
            healthBar.SetHealth(1f);

        // Trouver le joueur
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
            Debug.LogWarning($"[Devil:{name}] Aucun joueur trouvé !");
        else
            Debug.Log($"[Devil:{name}] Target trouvé : {target.name}");

        currentState = DevilState.Approaching;
    }

    void Update()
    {
        if (target == null || isStunned) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case DevilState.Approaching:
                HandleApproaching(distanceToPlayer);
                break;

            case DevilState.Charging:
                HandleCharging();
                break;

            case DevilState.Chilling:
                // État géré par coroutine
                break;
        }

        // Rotation vers la direction de mouvement (sauf en chill)
        if (currentState != DevilState.Chilling)
        {
            Vector3 lookDirection = (currentState == DevilState.Charging) ? dashDirection : (target.position - transform.position);
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void HandleApproaching(float distanceToPlayer)
    {
        // Se déplacer vers le joueur
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = normalSpeed;
            agent.SetDestination(target.position);
        }

        // Mettre à jour l'Animator
        UpdateAnimatorState(isRunning: true, isCharging: false, isChilling: false);

        // Si on entre dans le cercle intérieur ET qu'on peut dasher → DASH ! 
        if (distanceToPlayer <= innerCircleRadius && canDash)
        {
            StartDash();
        }
    }

    private void HandleCharging()
    {
        // ✅ Vérifier si on touche le joueur pendant le dash (sécurité si coroutine rate)
        if (!hasHitPlayerThisDash)
        {
            float distToPlayer = Vector3.Distance(transform.position, target.position);
            if (distToPlayer <= dashHitRadius)
            {
                HitPlayer();
            }
        }
    }

    private void StartDash()
    {
        Debug.Log($"[Devil:{name}] 🔥 DASH DÉMARRÉ !");

        currentState = DevilState.Charging;
        canDash = false;
        hasHitPlayerThisDash = false;
        dashProgress = 0f;

        // Arrêter le NavMeshAgent pendant le dash
        if (agent != null && agent.enabled)
            agent.isStopped = true;

        // Calculer la direction du dash :  du Devil vers le joueur
        Vector3 toPlayer = (target.position - transform.position).normalized;
        toPlayer.y = 0f; // Rester au même niveau Y (volant)
        dashDirection = toPlayer;

        // Point de départ :  position actuelle
        dashStartPosition = transform.position;

        // Point d'arrivée : de l'autre côté du cercle extérieur
        // On passe par le joueur (centre) et on continue jusqu'au bord opposé
        dashEndPosition = target.position + dashDirection * outerCircleRadius;

        // Si le NavMesh ne peut pas atteindre ce point, on ajuste
        if (agent != null && agent.isOnNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(dashEndPosition, out navHit, outerCircleRadius, NavMesh.AllAreas))
            {
                dashEndPosition = navHit.position;
            }
        }

        UpdateAnimatorState(isRunning: false, isCharging: true, isChilling: false);

        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        Vector3 startPos = transform.position;
        float dashDuration = Vector3.Distance(startPos, dashEndPosition) / dashSpeed;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            dashProgress = elapsed / dashDuration;

            // Interpolation linéaire (dash rapide)
            transform.position = Vector3.Lerp(startPos, dashEndPosition, dashProgress);

            // Vérifier si on touche le joueur pendant le dash
            if (!hasHitPlayerThisDash)
            {
                float distToPlayer = Vector3.Distance(transform.position, target.position);
                if (distToPlayer <= dashHitRadius)
                {
                    HitPlayer();
                }
            }

            yield return null;
        }

        // Fin du dash
        transform.position = dashEndPosition;

        Debug.Log($"[Devil:{name}] ✅ Dash terminé, entrée en chill.");
        StartChill();
    }

    private void HitPlayer()
    {
        hasHitPlayerThisDash = true;

        Debug.Log($"[Devil:{name}] 💥 HIT JOUEUR pendant le dash !");

        // Trigger l'animation Hit
        if (animator != null)
            animator.SetTrigger(TriggerHit);

        // Infliger des dégâts
        PlayerStats ps = target.GetComponent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(dashDamage);
            Debug.Log($"[Devil:{name}] ✅ {dashDamage} dégâts infligés !");
        }
    }

    private void StartChill()
    {
        currentState = DevilState.Chilling;
        UpdateAnimatorState(isRunning: false, isCharging: false, isChilling: true);

        StartCoroutine(ChillCoroutine());
    }

    private IEnumerator ChillCoroutine()
    {
        // Repos pendant chillDuration secondes
        yield return new WaitForSeconds(chillDuration);

        Debug.Log($"[Devil:{name}] 😎 Chill terminé, retour en approche.");

        // Retour en mode approche
        currentState = DevilState.Approaching;
        UpdateAnimatorState(isRunning: true, isCharging: false, isChilling: false);

        // Réactiver le dash après le cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        Debug.Log($"[Devil:{name}] ⚡ Dash à nouveau disponible.");
    }

    private void UpdateAnimatorState(bool isRunning, bool isCharging, bool isChilling)
    {
        if (animator == null) return;

        animator.SetBool(ParamIsRunning, isRunning);
        animator.SetBool(ParamIsCharging, isCharging);
        animator.SetBool(ParamIsChilling, isChilling);
    }

    // Gizmos pour visualiser les zones
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || target == null) return;

        // Cercle extérieur (zone d'approche)
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(target.position, outerCircleRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, outerCircleRadius);

        // Cercle intérieur (zone de dash)
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(target.position, innerCircleRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, innerCircleRadius);

        // Rayon de hit pendant le dash
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);

        // Ligne du dash (si en charge)
        if (currentState == DevilState.Charging)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, dashEndPosition);
        }
    }
}