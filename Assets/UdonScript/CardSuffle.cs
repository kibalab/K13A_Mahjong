
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
        int[] a = Shuffle();
        int[] b = Shuffle();
        int[] c = Shuffle();
        int[] d = Shuffle();
        for (int i = 0; i < 13; i++)
        {
            cardManager1.SetCard(a[i].ToString(), i);
            cardManager2.SetCard(b[i].ToString(), i);
            cardManager3.SetCard(c[i].ToString(), i);
            cardManager4.SetCard(d[i].ToString(), i);
        }
    }
    public int[] Shuffle()
    {

        int[] randArray = new int[13];
        int top = 0;
        for (int i = 0; i < 13;) {
            int g = UnityEngine.Random.Range(11, 47);
            if (shuffleInt[g - 1] != 4)
            {
                shuffleInt[g - 1] ++;
                randArray[top++] = g;
                i++;
            }
        }
        sortArray(randArray, 13);
        return randArray;
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
