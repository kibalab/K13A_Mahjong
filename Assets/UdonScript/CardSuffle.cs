
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSuffle : UdonSharpBehaviour
{
    public CardManager cardManager1;
    public CardManager cardManager2;
    public CardManager cardManager3;
    public CardManager cardManager4;
    public int[] shuffleInt = new int[52];
    public override void Interact()
    {
        shuffleInt = new int[52];
        int[] a = Shuffle();
        int[] b = Shuffle();
        int[] c = Shuffle();
        int[] d = Shuffle();
        for (int i = 0; i < 13; i++)
        {
            //Debug.Log(d[i]);
            cardManager1.SetCard(a[i].ToString(), i);
            cardManager2.SetCard(b[i].ToString(), i);
            cardManager3.SetCard(c[i].ToString(), i);
            cardManager4.SetCard(d[i].ToString(), i);
        }
        cardManager1.SetCard(GetTsumo().ToString(), 13);
        cardManager2.SetCard(GetTsumo().ToString(), 13);
        cardManager3.SetCard(GetTsumo().ToString(), 13);
        cardManager4.SetCard(GetTsumo().ToString(), 13);

        cardManager1.Pickupable(true);
        cardManager2.Pickupable(true);
        cardManager3.Pickupable(true);
        cardManager4.Pickupable(true);
    }
    public int[] Shuffle()
    {

        int[] randArray = new int[13];
        int top = 0;
        for (int i = 0; i < 13;) {
            int g = UnityEngine.Random.Range(11, 47);
            if (shuffleInt[g] <= 4)
            {
                shuffleInt[g] ++;
                randArray[top++] = g;
                i++;
            }
        }
        sortArray(randArray, 13);
        return randArray;
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
    }
}
