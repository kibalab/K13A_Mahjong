using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
/*
public class Card : UdonSharpBehaviour
{
    public string type;
    public int spriteNum;
    public int cardNum;
    public bool isDora = false;
    public GameObject sprites;
    public Card(string type, int spriteNum, bool isDora, GameObject sprites)
    {
        this.type = type;
        this.cardNum = spriteNum;
        this.isDora = isDora;
        this.sprites = sprites;

        GenerateCardData();
    }

    private int GenerateCardData()
    {
        int s = -1;
        if (type.Equals("���߹�"))
        {
            s = 42 + cardNum;
        } else if (type.Equals("��������")) 
        {
            s = 38 + cardNum;
        } else if (type.Equals("��"))
        {
            s = 11 + cardNum;
        } else if (type.Equals("��"))
        {
            s = 29 + cardNum;
        } else if (type.Equals("��"))
        {
            s = 20 + cardNum;
        }
        return s;
    }

    public GameObject SpawnObject(GameObject card, Transform point)
    {
        GameObject c = VRCInstantiate(card);
        c.transform.SetPositionAndRotation(point.position, point.rotation);
        c.transform.Find("Display").GetComponent<SpriteRenderer>().sprite = sprites.transform.Find(spriteNum.ToString()).GetComponent<SpriteRenderer>().sprite;
        return c;
    }

    public bool SetDora(bool b)
    {
        isDora = b;
        return isDora;
    }
}
*/