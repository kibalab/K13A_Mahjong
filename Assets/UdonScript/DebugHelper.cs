
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DebugHelper : UdonSharpBehaviour
{
    string className;
    string testName;

    public void SetClassName(string className)
    {
        this.className = className;
    }

    public void SetTestName(string testName)
    {
        this.testName = testName;
    }

    public void Equal(int a, int b, int lineNumber)
    {
        if (a != b)
        {
            Print($"두 값이 같지 않습니다. {a} {b}", lineNumber);
        }
    }

    public void IsTrue(bool cond, int lineNumber)
    {
        if (!cond)
        {
            Print("참이어야 하는데 참이 아닙니다.", lineNumber);
        }
    }

    public void IsFalse(bool cond, int lineNumber)
    {
        if (cond)
        {
            Print("거짓이어야 하는데 거짓이 아닙니다.", lineNumber);
        }
    }

    void Print(string errorMsg, int lineNumber)
    {
        Debug.Log($"{className} {testName} line:{lineNumber} {errorMsg}");
    }
}