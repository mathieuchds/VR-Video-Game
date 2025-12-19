using System;
using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField]  public float maxHealth = 100f;
    [SerializeField]  public float currentHealth = 100f;

    [SerializeField]  public int attackDamage = 50;
    [SerializeField]  public float attackSpeed = 1.5f;

    [SerializeField]  public int defense = 5;
    [SerializeField]  public float moveSpeed = 1f;

    //[SerializeField] public int mana = 50;
    //[SerializeField] public int maxMana = 50;


    [Header("Power Up")]

    [Header("Shockwave")]
    [SerializeField] public float shockwaveRadius = 5f;
    [SerializeField] public float shockwaveDamage = 20f;

    [Header("Stun")]
    [SerializeField] public float stunDuration = 2f;


    [Header("SpeedBoost")]
    [SerializeField] public float speedBoostMultiplier = 3f;
    [SerializeField] public float speedBoostDuration = 3f;

    [Header("Bomba")]
    [SerializeField] public float explosionRadius = 3f;
    [SerializeField] public float explosionDamage = 20f;

    [Header("FlameThrower")]
    [SerializeField] public float flameDamagePerSecond = 4f;
    [SerializeField] public float flameDuration = 2f;

    [Header("PoisonBullets")]
    [SerializeField] public float poisonDamage = 10f;
    [SerializeField] public float poisonDuration = 3f;

    [Header("IceRay")]
    [SerializeField] public float iceDuration = 2f;


    public event Action HealthUpdate;

    private GameStateManager gameStateManager;
    private bool isDead = false; // ✅ NOUVEAU : Empêcher de mourir plusieurs fois

    private void Awake()
    {
        gameStateManager = FindObjectOfType<GameStateManager>();
        
        if (gameStateManager == null)
        {
            Debug.LogError("[PlayerStats] GameStateManager introuvable ! Le Game Over ne pourra pas se déclencher.");
        }

        ResetStats();
    }

    // ✅ NOUVEAU : Méthode pour réinitialiser les stats
    public void ResetStats()
    {
        currentHealth = maxHealth;
        isDead = false;
        HealthUpdate?.Invoke();
        
        Debug.Log($"[PlayerStats] ✅ Stats réinitialisées : {currentHealth}/{maxHealth} HP");
    }

    public void TakeDamage(float amount)
    {
        // ✅ Ne pas prendre de dégâts si déjà mort
        if (isDead)
            return;

        float finalDamage = Mathf.Max(amount - defense, 0f);
        currentHealth -= finalDamage;
        
        currentHealth = Mathf.Max(currentHealth, 0f);
        
        HealthUpdate?.Invoke();

        if (currentHealth <= 0f && !isDead)
        {
            OnPlayerDeath();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HealthUpdate?.Invoke();
    }

    private void OnPlayerDeath()
    {
        isDead = true; // ✅ Empêcher de mourir plusieurs fois
        
        Debug.Log("[PlayerStats] 💀 JOUEUR MORT ! Déclenchement du Game Over...");

        if (gameStateManager != null)
        {
            gameStateManager.TriggerGameOver(false);
        }
        else
        {
            Debug.LogError("[PlayerStats] Impossible de déclencher le Game Over : GameStateManager introuvable !");
        }
    }
}
