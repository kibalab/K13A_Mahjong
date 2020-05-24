
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KList : UdonSharpBehaviour
{
    private const int jump = 8;

    public bool IgnoreTests = false;

    private object[] components = new object[jump];
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
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
            if (Count() == i) Debug.Log(i + "countError");
        }

        Clear();

        for (var i = 0; i <= 20; ++i)
        {
            Add(new object());
            if (index != i) Debug.Log(i + "IndexError at test 2");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError at test 2");
            if (Count() == i) Debug.Log(i + "countError at test 2");
        }

        for (var i = 20; i >= 0; --i)
        {
            RemoveLast();
            if (index != i - 1) Debug.Log(i + "IndexError");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
            if (Count() == i - 1) Debug.Log(i + "countError");
        }

        for (var i = 0; i <= 20; ++i)
        {
            Add(i);
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
        }

        if (IndexOf(2) != 2) Debug.Log("indexOf Error 0");
        if (IndexOf(5) != 5) Debug.Log("indexOf Error 1");
        if (IndexOf(-99) != -1) Debug.Log("indexOf Error 2");

        for (var i = 19; i >= 0; --i)
        {
            var value = (int)RemoveLast();
            if (value != i + 1) Debug.Log($"ValueError {value}, {i}");
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
        }

        for (var i = 1; i <= 20; ++i)
        {
            Add(i.ToString());
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
        }

        for (var i = 19; i >= 0; --i)
        {
            var value = (string)RemoveLast();
            if (value != (i + 1).ToString()) Debug.Log($"ValueError {value}, {i}");
            if (index != i) Debug.Log(i + "IndexError");
            if (components.Length != EstimatedLength(index)) Debug.Log(i + "lengthError");
        }

        Clear();
        Add("hello world");
        if (IndexOf("hello world") != 0) Debug.Log("indexOf error 3");

        Debug.Log("if nothing appeared above, test success");
    }

    int EstimatedLength(int i)
    {
        return ((i / jump) + 1) * jump;
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
        if (!isAdd && (index - 1 < (scaled - 1) * jump) && scaled != 1)
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

    public int IndexOf_Int(int number)
    {
        for (var i = 0; i <= index; ++i)
        {
            if (number == (int)components[i])
            {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf_String(string str)
    {
        for (var i = 0; i <= index; ++i)
        {
            if (str == (string)components[i])
            {
                return i;
            }
        }
        return -1;
    }

    public int IndexOf(object obj)
    {
        // premitive type이면 별도로 생성해줘야 함
        // 일단 자주 쓸 것 같은 두개만 만듬
        var typeName = obj.GetType().Name;
        switch (typeName)
        {
            case "Int32": return IndexOf_Int((int)obj);
            case "String": return IndexOf_String((string)obj);
            default: break;
        }

        for (var i = 0; i <= index; ++i)
        {
            if (obj == components[i])
            {
                return i;
            }
        }
        return -1;
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