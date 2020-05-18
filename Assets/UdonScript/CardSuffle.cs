
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;



    /*
public struct Card
{
    public string type;
    public int spriteNum;
    public int cardNum;
    public bool isDora_;
    public GameObject sprites;
    public Card(string type_, int spriteNum_, bool isDora_, GameObject sprites_)
        {
            type = type_;
            cardNum = spriteNum_;
            isDora_ = isDora_;
            sprites = sprites_;
            cardNum = 0;

        GenerateCardData();
        }

        private int GenerateCardData()
        {
            int s = -1;
            if (type.Equals("백중발"))
            {
                s = 42 + cardNum;
            }
            else if (type.Equals("동서남북"))
            {
                s = 38 + cardNum;
            }
            else if (type.Equals("만"))
            {
                s = 11 + cardNum;
            }
            else if (type.Equals("삭"))
            {
                s = 29 + cardNum;
            }
            else if (type.Equals("통"))
            {
                s = 20 + cardNum;
            }
            return s;
        }

        public GameObject SpawnObject(GameObject card, Transform point)
        {
            GameObject c = UdonSharpBehaviour.VRCInstantiate(card);
            c.transform.SetPositionAndRotation(point.position, point.rotation);
            c.transform.Find("Display").GetComponent<SpriteRenderer>().sprite = sprites.transform.Find(spriteNum.ToString()).GetComponent<SpriteRenderer>().sprite;
            return c;
        }

        public bool SetDora(bool b)
        {
            isDora_ = b;
            return isDora_;
        }
    }
    */


public class CardSuffle : UdonSharpBehaviour
{
        /*
    public CardManager cardManager1;
    public CardManager cardManager2;
    public CardManager cardManager3;
    public CardManager cardManager4;

    public GameObject sprites;

    public Card Card;

    
    public override void Interact()
    {
        genCard();
    }

    public Card[] genCard()
    {
        Card[] allCards = new Card[137];
        foreach (var type in new string[3] { "만", "삭", "통"})
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9})
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = (i == 3 ? true : false);
                    allCards[allCards.Length+1] = new Card(type, number, isDora, sprites);
                }
            }
        }
        for (int i = 0; i < 3; ++i)
        {
            allCards[allCards.Length + 1] = new Card("백중발", i, false, sprites);
        }
        for (int i = 0; i < 4; ++i)
        {
            allCards[allCards.Length + 1] = new Card("동서남북", i, false, sprites);
        }
        return ShuffleCards(allCards);
    }

    public Card[] ShuffleCards(Card[] cards)
    {
        var shuffledCards = new Card[136];
        var yetShuffledCount = 136 - 1;
        var shuffledIndex = 0;

        while (yetShuffledCount >= 0)
        {
            var picked = UnityEngine.Random.Range(0, yetShuffledCount + 1);
            shuffledCards[shuffledIndex] = cards[picked];
            cards[picked] = cards[yetShuffledCount];

            yetShuffledCount--;
            shuffledIndex++;
        }
        return shuffledCards;
    }

    public int GetTsumo()
    {

        int randArray;
        for (int i = 0; i < 1;)
        {
            int g = UnityEngine.Random.Range(11, 47);
            if (g == 15 || g == 25 || g == 35)
            {
                if (shuffleInt[g] <= 3)
                {
                    shuffleInt[g]++;
                    randArray = g;
                    i++;
                    return randArray;
                }
            }
            else if (g == 16 || g == 26 || g == 36)
            {
                if (shuffleInt[g] <= 1)
                {
                    shuffleInt[g]++;
                    randArray = g;
                    i++;
                    return randArray;
                }
            }
            else {
                if (shuffleInt[g] != 4)
                {
                    shuffleInt[g]++;
                    randArray = g;
                    i++;
                    return randArray;
                }
            }
            
        }
        return -1;
    }
    

    public int[] sortArray(int[] a, int x)
    {
        int i;
        int j;
        int temp;

        for (i = (x - 1); i >= 0; i--)
        {
            for (j = 1; j <= i; j++)
            {
                if (a[j - 1] > a[j])
                {
                    temp = a[j - 1];
                    a[j - 1] = a[j];
                    a[j] = temp;
                }
            }
        }
        return a;
    }*/
}