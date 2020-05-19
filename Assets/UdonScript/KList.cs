
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KList : UdonSharpBehaviour
{
    public int Count = 0;

    private object[] cards1 = new object[0];
    private object[] cards2 = new object[0];


    public object[] Add(object component)
    {
        cards2 = cards1;
        cards1 = new object[cards2.Length + 1];
        for (var i = 0; i < cards2.Length; i++)
        {
            cards1[i] = cards2[i];
        }
        cards1[cards2.Length] = component;
        Count++;
        cards2 = new object[0];
        return cards1;
    }

    public object Remove()
    {
        cards2 = cards1;
        cards1 = new object[cards2.Length - 1];
        for (var i = 0; i < cards2.Length - 1; i++)
        {
            cards1[i] = cards2[i];
        }

        Count--;
        object r = cards1[cards1.Length - 1];
        cards1[cards1.Length - 1] = null;
        cards2 = new object[0];
        return r;
    }

    public bool Clear()
    {
        cards1 = new object[0];
        cards2 = new object[0];
        Count = 0;

        return true;
    }

    public object[] Clone()
    {
        return cards1;
    }

    public object[] GetRange(int startIndex, int endIndex)
    {
        object[] l = new object[endIndex - startIndex + 1];
        for(int i = startIndex, j = 0; i <= endIndex; i++, j++)
        {
            l[j] = cards1[i];
        }
        return l;
    }

    public object[] insert(int index, object data)
    {
        cards2 = cards1;
        cards1 = new object[cards2.Length + 1];
        for(int i = 0, j = 0; i < cards2.Length + 1; i++, j++)
        {
            if(i == index)
            {
                cards1[i] = data;
                i++;
            }
            else
            {
                cards1[i] = cards2[j];
            }
        }
        Count++;
        cards2 = new object[0];
        return cards1;
    }

    public object[] RemoveAt(int index)
    {
        cards2 = cards1;
        cards1 = new object[cards2.Length - 1];
        for (int i = 0, j = 0; i < cards2.Length; i++, j++)
        {
            if (i == index)
            {
                i++;
            }
            else
            {
                cards1[j] = cards2[i];
            }
        }
        Count--;
        cards2 = new object[0];
        return cards1;
    }
}
