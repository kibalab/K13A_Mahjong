
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class TableViewer : UdonSharpBehaviour
{
    public CardSprites CardSprites;

    public Text roundText;
    public Image[] windTexts;
    public Text[] namePlates;

    public GameObject endDisplay;


    public void setWInd(int playerIndex, string wind)
    {
        /*
        switch (wind)
        {
            case "East":
                windTexts[playerIndex].sprite = CardSprites.FindSprite("µ¿");
                break;
            case "South":
                windTexts[playerIndex].sprite = CardSprites.FindSprite("³²");
                break;
            case "West":
                windTexts[playerIndex].sprite = CardSprites.FindSprite("¼­");
                break;
            case "North":
                windTexts[playerIndex].sprite = CardSprites.FindSprite("ºÏ");
                break;
        }
        */

    }

    public void activeDisplay(string type, bool active)
    {
        var display = endDisplay.transform.Find(type);

        display.gameObject.SetActive(active);
    }

    public void hideDisplay()
    {
        for (var i = 0; i < endDisplay.transform.childCount; i++)
        {
            endDisplay.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void setChin(int chin)
    {
        switch (chin)
        {
            case 0:
                windTexts[0].color = Color.red;
                windTexts[1].color = Color.white; 
                windTexts[2].color = Color.white;
                windTexts[3].color = Color.white;
                break;
            case 1:
                windTexts[0].color = Color.white;
                windTexts[1].color = Color.red;
                windTexts[2].color = Color.white;
                windTexts[3].color = Color.white;
                break;
            case 2:
                windTexts[0].color = Color.white;
                windTexts[1].color = Color.white;
                windTexts[2].color = Color.red;
                windTexts[3].color = Color.white;
                break;
            case 3:
                windTexts[0].color = Color.white;
                windTexts[1].color = Color.white;
                windTexts[2].color = Color.white;
                windTexts[3].color = Color.red;
                break;
        }
    }

    public void setPlayerName(string name, int wind)
    {
        namePlates[wind].text = name;
    }
}
