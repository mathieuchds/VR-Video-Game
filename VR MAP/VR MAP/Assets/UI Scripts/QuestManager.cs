using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestUIManager questUI;

    private Quest currentQuest;

    void Start()
    {
        StartQuest(
            "Première Vague",
            "Éliminer 10 ennemis",
            10
        );
    }

    public void StartQuest(string title, string description, int target)
    {
        currentQuest = new Quest(title, description, target);
        questUI.ShowQuest();
        UpdateUI();
    }

    public void AddProgress(int amount = 1)
    {
        if (currentQuest == null) return;

        currentQuest.AddProgress(amount);
        UpdateUI();

        if (currentQuest.IsCompleted)
        {
            CompleteQuest();
        }
    }

    void UpdateUI()
    {
        questUI.UpdateQuest(
            currentQuest.title,
            currentQuest.description,
            currentQuest.current,
            currentQuest.target
        );
    }

    void CompleteQuest()
    {
        Debug.Log("Quête terminée !");
        questUI.HideQuest();
    }
}
