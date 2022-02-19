
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KList : UdonSharpBehaviour
{
    public DebugHelper DebugHelper;
    private const int jump = 256;

    private object[] components = new object[jump];
    private int scaled = 1;
    private int index = -1;

    public object[] Add(object newComponent)
    {
        ResizeIfNeeded(true);
        components[++index] = newComponent;
        return components;
    }

    public object[] AddArray(object[] newComponents)
    {
        foreach(object newComponent in newComponents)
        {
            Add(newComponent);
        }
        return components;
    }

    public object[] Insert(int insertIndex, object component)
    {
        ResizeIfNeeded(true);
        index++;
        for (var i = index; i > insertIndex; --i)
        {
            components[i] = components[i - 1];
        }
        components[insertIndex] = component;

        return components;
    }

    public object RemoveLast()
    {
        if (index >= components.Length || index < 0)
        {
            Debug.LogError($"[KList] Cannot Remove value by index : {index}");
            return null;
        }

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

    public void Sort()
    {
        if (index == -1) { return; }

        var type = (At(0)).GetType().Name;
        switch (type)
        {
            case "Int32": Sort_Int(); break;
            default:
                if (((Card)At(0)) != null)
                {
                    Sort_Cards();
                }
                else
                {
                    Debug.Log($"can't sort object type {type}");
                }
                break;
        }
    }

    void Sort_Int()
    {
        for (var i = index; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                var val1 = (int)components[j - 1];
                var val2 = (int)components[j];

                if (val1 > val2)
                {
                    var temp = val1;
                    components[j - 1] = val2;
                    components[j] = temp;
                }
            }
        }
    }

    void Sort_Cards()
    {
        for (var i = index; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                var val1 = (Card)components[j - 1];
                var val2 = (Card)components[j];

                if (val1.GlobalOrder > val2.GlobalOrder)
                {
                    var temp = val1;
                    components[j - 1] = val2;
                    components[j] = temp;
                }
            }
        }
    }

    int IndexOf_Int(int number)
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

    int IndexOf_String(string str)
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

    public bool Contains(object obj)
    {
        return IndexOf(obj) != -1;
    }

    public int IndexOf(object obj)
    {
        // primitive type이면 별도로 생성해줘야 함
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
        if (i >= components.Length)
        {
            Debug.LogError($"[KList] Cannot load value by index : {index}");
            return null;
        }

        return components[i];
    }

    public void Clear()
    {
        components = new object[jump];
        scaled = 1;
        index = -1;
    }

    public object[] Clone()
    {
        var copied = new object[Count()];
        for (var i = 0; i < Count(); ++i)
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

        Debug.Log("RemoveIndex : " + removeIndex);
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


    void Test1()
    {
        DebugHelper.SetTestName("Test1");

        for (var i = 0; i <= 2000; ++i)
        {
            DebugHelper.Equal(Count(), i, 1);
            DebugHelper.Equal(index, i - 1, 2);
            DebugHelper.Equal(components.Length, EstimatedLength(index), 3);
            Add(new object());
        }
    }

    void Test2()
    {
        DebugHelper.SetTestName("Test2");

        for (var i = 0; i <= 20; ++i)
        {
            Add(new object());
        }

        for (var i = 21; i > 0; --i)
        {
            DebugHelper.Equal(Count(), i, 1);
            DebugHelper.Equal(index, i - 1, 2);
            DebugHelper.Equal(components.Length, EstimatedLength(index), 3);
            RemoveLast();
        }
    }

    void Test3()
    {
        DebugHelper.SetTestName("Test3");

        for (var i = 0; i <= 20; ++i)
        {
            Add(i);
            DebugHelper.Equal(IndexOf(i), i, 1);
        }

        for (var i = 20; i >= 0; --i)
        {
            DebugHelper.Equal(IndexOf(i), i, 2);
            var value = (int)RemoveLast();
            DebugHelper.Equal(value, i, 3);
        }
    }

    void Test4()
    {
        DebugHelper.SetTestName("Test4");

        for (var i = 0; i <= 20; ++i) { Add(i.ToString()); }
        for (var i = 20; i >= 0; --i)
        {
            var value = (string)RemoveLast();
            DebugHelper.IsTrue(value == i.ToString(), 1);
        }
    }

    void Test5()
    {
        DebugHelper.SetTestName("Test5");

        Add(3);
        Add(1);
        Add(2);
        Sort();

        DebugHelper.Equal((int)At(0), 1, 1);
        DebugHelper.Equal((int)At(1), 2, 2);
        DebugHelper.Equal((int)At(2), 3, 3);
    }

    void Test6()
    {
        DebugHelper.SetTestName("Test6");

        Add("hello world");
        Add("nice to meet you");

        var index1 = IndexOf("hello world");
        var index2 = IndexOf("nice to meet you");

        DebugHelper.Equal(index1, 0, 1);
        DebugHelper.Equal(index2, 1, 2);
    }

    int EstimatedLength(int i)
    {
        return ((i / jump) + 1) * jump;
    }

    public void Start()
    {
        if (DebugHelper != null && Networking.LocalPlayer == null) 
        {
            DebugHelper.SetClassName("KList");
            Test1(); Clear();
            Test2(); Clear();
            Test3(); Clear();
            Test4(); Clear();
            Test5(); Clear();
            Test6();
        }
    }
}