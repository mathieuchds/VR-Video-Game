
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerSelectionManager : MonoBehaviour
{
    public GameObject panel;
    public Button[] powerButtons;
    public TMP_Text[] powerTexts;
    public TMP_Text[] descriptionTexts;


    public PlayerController player;

    private bool canSelect = false;

    private List<string> allPowerUps = new List<string>()
    {
        "Stun",
        "SpeedBoost",
        "Shockwave",
        "Bomba",
        "FlameThrower",
        "PoisonBullets",
        "IceRay"
    };

    List<string> powerDescriptions = new List<string>()
{
    "Étourdit les ennemis proches",
    "Augmente temporairement la vitesse",
    "Onde de choc lors du saut",
    "Lance une bombe repoussant les enemis",
    "Lance-flammes continu",
    "Dégâts sur la durée",
    "Ralentit les ennemis"
};



    void Awake()
    {
        panel = GameObject.Find("PowerSelectionPanel");
        panel.SetActive(false);
        player = FindObjectOfType<PlayerController>(true);
    }

    public void ShowPowerSelection()
    {
        Time.timeScale = 0f; // pause
        panel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        List<string> randomPowers = GetRandomPowers(3);

        for (int i = 0; i < powerButtons.Length; i++)
        {
            string power = randomPowers[i];
            powerTexts[i].text = power;
            descriptionTexts[i].text = powerDescriptions[allPowerUps.IndexOf(power)];

            powerButtons[i].onClick.RemoveAllListeners();
            powerButtons[i].onClick.AddListener(() => SelectPower(power));
        }
        StartCoroutine(EnableSelectionAfterDelay());

    }

    System.Collections.IEnumerator EnableSelectionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        canSelect = true;
    }


    void SelectPower(string power)
    {
        if (!canSelect) return;
        player.ApplyPowerUp(power);

        panel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canSelect = false;
    }

    List<string> GetRandomPowers(int count)
    {
        List<string> tempList = new List<string>(allPowerUps);
        List<string> result = new List<string>();

        for (int i = 0; i < count; i++)
        {
            
            int index = Random.Range(0, tempList.Count);
 

            result.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return result;
    }
}
