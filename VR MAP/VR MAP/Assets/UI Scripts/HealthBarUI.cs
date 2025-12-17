using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerStats stats;
    public Image fillImage;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponent<PlayerStats>();

        stats.currentHealth = stats.maxHealth;
        fillImage = GameObject.Find("HealthBar_Fill").GetComponent<Image>();

        stats.HealthUpdate += UpdateHealthUI;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (stats != null)
        {
            fillImage.fillAmount = stats.currentHealth / stats.maxHealth;
        }
    }


}
