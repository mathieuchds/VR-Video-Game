using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("UI (optionnel)")]
    public Image icon;              
    public TMP_Text cooldownText;

    [Header("State")]
    public bool isUnlocked = false;

    public void Unlock()
    {
        isUnlocked = true;
        cooldownText.text = "R";
        if (icon != null)
        {
            icon.color = Color.red;
            icon.fillAmount = 0f;
        }
    }

    public void Lock()
    {
        isUnlocked = false;

        if (icon != null)
        {
            icon.color = Color.gray;
            icon.fillAmount = 1f;
        }
    }

    public void UpdateCooldown(float remaining, float max)
    {
        if (!isUnlocked)
        {
            cooldownText.text = "L";
            return;
        }

        if (remaining > 0f)
        {
            cooldownText.text = remaining.ToString("F1") + "s";

            if (icon != null)
                icon.fillAmount = remaining / max;
        }
        else
        {
            cooldownText.text = "R";

            if (icon != null)
                icon.fillAmount = 0f;
        }
    }
}
