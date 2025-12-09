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

    public event Action HealthUpdate;

    public void TakeDamage(float amount)
    {
        float finalDamage = Mathf.Max(amount - defense, 0f);
        currentHealth -= finalDamage;
        HealthUpdate?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HealthUpdate?.Invoke();
    }
}
