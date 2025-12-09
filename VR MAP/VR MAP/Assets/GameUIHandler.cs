using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public PlayerStats stats;
    public UIDocument UIDoc;

    private Label m_HealthLabel;
    private VisualElement m_HealthBarMask;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponent<PlayerStats>();
        UIDoc  =GetComponent<UIDocument>();
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("HealthLabel");
        m_HealthBarMask = UIDoc.rootVisualElement.Q<VisualElement>("HealthBarMask");

        stats.HealthUpdate += UpdateHealthUI;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if(stats!=null && m_HealthLabel != null)
        {
            m_HealthLabel.text = $"{stats.currentHealth}/{stats.maxHealth}";
            float healthRatio = (float)stats.currentHealth / stats.maxHealth;
            float healthPercent = Mathf.Lerp(0, 100, healthRatio);
            m_HealthBarMask.style.width = Length.Percent(healthPercent);
        }
    }


}
