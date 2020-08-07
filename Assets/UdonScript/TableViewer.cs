
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


    public void setRound(int round)
    {
        
        switch(round){
            case 0:
                windTexts[0].sprite = CardSprites.FindSprite("µ¿");
                windTexts[1].sprite = CardSprites.FindSprite("³²");
                windTexts[2].sprite = CardSprites.FindSprite("¼­");
                windTexts[3].sprite = CardSprites.FindSprite("ºÏ");
                break;
            case 1:
                windTexts[0].sprite = CardSprites.FindSprite("³²");
                windTexts[1].sprite = CardSprites.FindSprite("¼­");
                windTexts[2].sprite = CardSprites.FindSprite("ºÏ");
                windTexts[3].sprite = CardSprites.FindSprite("µ¿");
                break;
            case 2:
                windTexts[0].sprite = CardSprites.FindSprite("¼­");
                windTexts[1].sprite = CardSprites.FindSprite("ºÏ");
                windTexts[2].sprite = CardSprites.FindSprite("µ¿");
                windTexts[3].sprite = CardSprites.FindSprite("³²");
                break;
            case 3:
                windTexts[0].sprite = CardSprites.FindSprite("ºÏ");
                windTexts[1].sprite = CardSprites.FindSprite("µ¿");
                windTexts[2].sprite = CardSprites.FindSprite("³²");
                windTexts[3].sprite = CardSprites.FindSprite("¼­");
                break;
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
}
