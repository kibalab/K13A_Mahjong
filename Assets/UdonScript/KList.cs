
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class KList : UdonSharpBehaviour
{
    public bool IgnoreTests = false;

    private object[] components = new object[8];
    private const int jump = 8;
    private int scaled = 1;
    private int index = -1;

    public void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log("--- KList TEST ---");
        for (var i = 0; i <= 2000; ++i)
        {
            Add(new object());
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
            if (Count() == i) Debug.Log(i + "countError");
        }

        Clear();

        for (var i = 0; i <= 20; ++i)
        {
            Add(new object());
            if (index != i) Debug.Log(i + "IndexError at test 2");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError at test 2");
            if (Count() == i) Debug.Log(i + "countError at test 2");
        }

        for (var i = 19; i >= 0; --i)
        {
            RemoveLast();
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
            if (Count() == i) Debug.Log(i + "countError");
        }

        for (var i = 1; i <= 20; ++i)
        {
            Add(i);
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
        }

        for (var i = 19; i >= 0; --i)
        {
            var value = (int)RemoveLast();
            if (value != i + 1) Debug.Log($"ValueError {value}, {i}");
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
        }

        for (var i = 1; i <= 20; ++i)
        {
            Add(i.ToString());
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
        }

        for (var i = 19; i >= 0; --i)
        {
            var value = (string)RemoveLast();
            if (value != (i + 1).ToString()) Debug.Log($"ValueError {value}, {i}");
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != ((index / 8) + 1) * 8) Debug.Log(i + "lengthError");
        }

        Debug.Log("if nothing appeared, test success");
    }

    public object[] Add(object newComponent)
    {
        ResizeIfNeeded(true);
        components[++index] = newComponent;
        return components;
    }

    public object RemoveLast()
    {
        var comp = components[index];
        components[index] = null;
        ResizeIfNeeded(false);
        --index;
        return comp;
    }

    void ResizeIfNeeded(bool isAdd)
    {
        // scale up needed
        if (isAdd && (index == components.Length - 1))
        {
            ++scaled;
            var temp = components;

            components = new object[scaled * jump];
            for (var i = 0; i < temp.Length; i++)
            {
                components[i] = temp[i];
            }
        }

        // scale down needed
        if (!isAdd && (index - 1 < (scaled - 1) * jump))
        {
            --scaled;
            var temp = components;
            components = new object[scaled * jump];
            for (var i = 0; i < components.Length; i++)
            {
                components[i] = temp[i];
            }
        }
    }

    public object At(int i)
    {
        return components[i];
    }

    public void Clear()
    {
        components = new object[8];
        scaled = 1;
        index = -1;
    }

    public object[] Clone()
    {
        var copied = new object[components.Length];
        for (var i = 0; i < components.Length; ++i)
        {
            copied[i] = components[i];
        }
        return copied;
    }

    public object[] GetRange(int startIndex, int endIndex)
    {
        var l = new object[endIndex - startIndex + 1];
        for (int i = startIndex, j = 0; i <= endIndex; i++, j++)
        {
            l[j] = components[i];
        }
        return l;
    }

    public object RemoveAt(int removeIndex)
    {
        ResizeIfNeeded(false);

        var r = components[removeIndex];
        for (var i = removeIndex; i < components.Length - 1; ++i)
        {
            components[i] = components[i + 1];
        }
        --index;
        return r;
    }

    public int Count()
    {
        return index + 1;
    }
}