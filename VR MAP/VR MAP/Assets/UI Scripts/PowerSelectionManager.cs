
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerSelectionManager : MonoBehaviour
{
    public GameObject panel;
    public Button[] powerButtons;
    public TMP_Text[] powerTexts;


    public PlayerController player;

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


    void Start()
    {
        panel = GameObject.Find("PowerSelectionPanel");
        panel.SetActive(false);
        player = FindObjectOfType<PlayerController>();
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

            powerButtons[i].onClick.RemoveAllListeners();
            powerButtons[i].onClick.AddListener(() => SelectPower(power));
        }
    }

    void SelectPower(string power)
    {
        player.ApplyPowerUp(power);

        panel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
