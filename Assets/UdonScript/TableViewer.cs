
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
                windTexts[0].sprite = CardSprites.FindSprite("��");
                windTexts[1].sprite = CardSprites.FindSprite("��");
                windTexts[2].sprite = CardSprites.FindSprite("��");
                windTexts[3].sprite = CardSprites.FindSprite("��");
                break;
            case 1:
                windTexts[0].sprite = CardSprites.FindSprite("��");
                windTexts[1].sprite = CardSprites.FindSprite("��");
                windTexts[2].sprite = CardSprites.FindSprite("��");
                windTexts[3].sprite = CardSprites.FindSprite("��");
                break;
            case 2:
                windTexts[0].sprite = CardSprites.FindSprite("��");
                windTexts[1].sprite = CardSprites.FindSprite("��");
                windTexts[2].sprite = CardSprites.FindSprite("��");
                windTexts[3].sprite = CardSprites.FindSprite("��");
                break;
            case 3:
                windTexts[0].sprite = CardSprites.FindSprite("��");
                windTexts[1].sprite = CardSprites.FindSprite("��");
                windTexts[2].sprite = CardSprites.FindSprite("��");
                windTexts[3].sprite = CardSprites.FindSprite("��");
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
