
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KList : UdonSharpBehaviour
{
    private const int jump = 256;

    public bool IgnoreTests = false;

    private object[] components = new object[jump];
    private int scaled = 1;
    private int index = -1;

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

    public void Sort()
    {
        if (index == -1) { return; }

        var type = (At(0)).GetType().Name;
        switch (type)
        {
            case "Int32": Sort_Int(); break;
            default:
                if ((CardComponent)At(0) != null)
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
                var val1 = (CardComponent)components[j - 1];
                var val2 = (CardComponent)components[j];
                if (val1.GlobalIndex > val2.GlobalIndex)
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
        // primitive type�̸� ������ ��������� ��
        // �ϴ� ���� �� �� ���� �ΰ��� ����
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
        for (var i = 0; i <= 2000; ++i)
        {
            if (Count() != i) Debug.Log("error at 1");
            if (index != i - 1) Debug.Log("error at 1");
            if (components.Length != TEST__EstimatedLength(index)) Debug.Log("error");
            Add(new object());
        }
    }

    void Test2()
    {
        for (var i = 0; i <= 20; ++i)
        {
            Add(new object());
        }

        for (var i = 20; i >= 0; --i)
        {
            if (Count() != i + 1) Debug.Log("error");
            if (index != i) Debug.Log("error");
            if (components.Length != TEST__EstimatedLength(index)) Debug.Log("error");

            RemoveLast();
        }
    }

    void Test3()
    {
        for (var i = 0; i <= 20; ++i) { Add(i); }
        for (var i = 0; i <= 20; ++i)
        {
            if (IndexOf(i) != i) Debug.Log("indexOf Error");
        }

        for (var i = 20; i >= 0; --i)
        {
            if (IndexOf(i) != i) Debug.Log("indexOf Error");
            var value = (int)RemoveLast();
            if (value != i) Debug.Log("error");
        }
    }

    void Test4()
    {
        for (var i = 0; i <= 20; ++i) { Add(i.ToString()); }
        for (var i = 20; i >= 0; --i)
        {
            var value = (string)RemoveLast();
            if (value != i.ToString()) Debug.Log($"ValueError {value}, {i}");
        }
    }

    void Test5()
    {
        Add(3);
        Add(1);
        Add(2);
        Sort();
        if ((int)At(0) != 1) Debug.Log("error on test 5 1");
        if ((int)At(1) != 2) Debug.Log("error on test 5 2");
        if ((int)At(2) != 3) Debug.Log("error on test 5 3");
    }

    int TEST__EstimatedLength(int i)
    {
        return ((i / jump) + 1) * jump;
    }

    public void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log("--- KList TEST ---");

        Test1(); Clear();
        Test2(); Clear();
        Test3(); Clear();
        Test4(); Clear();
        Test5(); Clear();

        Add("hello world");
        if (IndexOf("hello world") != 0) Debug.Log("indexOf error 3");

        Debug.Log("if nothing appeared above, test success");
    }
}