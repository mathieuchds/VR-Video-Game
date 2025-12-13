using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


public class PowerSelectionManager : MonoBehaviour
{
    [Header("UI Assets")]
    public UIDocument gameUIDocument;     // GAMEUI.uxml UIDocument
    public VisualTreeAsset powerCardAsset; // PowerCard.uxml
    public PowerDatabase database;         // PowerDatabase ScriptableObject

    [Header("Settings")]
    public int count = 3;                  // combien de cartes à afficher

    private VisualElement root;            // root de GAMEUI
    private VisualElement container;       // PowerSelectionContainer
    private VisualElement cardsRow;        // PowerCardsRow

    private Power[] selected;
    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }



    void OnEnable()
    {
        if (gameUIDocument == null)
        {
            Debug.LogError("GameUIDocument non assigné dans PowerSelectionManager.");
            enabled = false;
            return;
        }

        root = gameUIDocument.rootVisualElement;
        container = root.Q<VisualElement>("PowerSelectionContainer");
        cardsRow = root.Q<VisualElement>("PowerCards");

        if (container == null)
        {
            Debug.LogError("PowerSelectionContainer introuvable dans GAMEUI.uxml");
            enabled = false;
            return;
        }

        container.style.visibility = Visibility.Hidden;
    }

    public void ShowSelection()
    {
        if (database == null || database.powers == null || database.powers.Length == 0)
        {
            Debug.LogWarning("Base de pouvoirs vide.");
            return;
        }

        SelectRandom();
        PopulateCards();

        // Afficher UI
        container.style.visibility = Visibility.Visible;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;


    }

    public void HideSelection()
    {
        container.style.visibility = Visibility.Hidden;

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;


        // (Optionnel) réactiver le contrôle joueur
        // Example: PlayerController.Instance.enabled = true;
    }

    void SelectRandom()
    {
        List<Power> list = new List<Power>(database.powers);
        selected = new Power[Mathf.Min(count, list.Count)];

        for (int i = 0; i < selected.Length; i++)
        {
            int idx = Random.Range(0, list.Count);
            selected[i] = list[idx];
            list.RemoveAt(idx);
        }
    }

    void PopulateCards()
    {
        // clear previous cards
        cardsRow.Clear();

        for (int i = 0; i < selected.Length; i++)
        {
            var card = powerCardAsset.Instantiate();

            var title = card.Q<Label>("Title");
            var icon = card.Q<Image>("Icon");

            if (title != null) title.text = selected[i].name;
            if (icon != null && selected[i].icon != null) icon.sprite = selected[i].icon;

            int localIndex = i;
            card.RegisterCallback<ClickEvent>(_ =>
            {
                OnCardClicked(localIndex);
            });

            cardsRow.Add(card);
        }
    }

    void OnCardClicked(int index)
    {
        var power = selected[index];
        Debug.Log("[PowerSelection] Choisi : " + power.name);

        player.ApplyPowerUp(power.name);


        HideSelection();
    }
}
