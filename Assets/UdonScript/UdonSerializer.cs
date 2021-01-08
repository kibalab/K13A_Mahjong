
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UdonSerializer : UdonSharpBehaviour
{
    private void Start()
    {
        Debug.LogWarning("[UdonSerializer]=========Test=========");
        Debug.LogWarning(this.GetType());
        Debug.LogWarning(GetUdonTypeName<Card>());
    }

    public string Serialize(object obj)
    {
        var type = obj.GetType().Name;

        switch(type)
        {
            case "Vector2" :
                var vector2 = ((Vector2) obj).x.ToString() + ",";
                vector2 += ((Vector2)obj).y.ToString();
                return vector2;

            case "Vector3":
                var vector3 = ((Vector3)obj).x.ToString() + ",";
                vector3 += ((Vector3)obj).y.ToString() + ",";
                vector3 += ((Vector3)obj).z.ToString();
                return vector3;

            case "Vector4":
                var vector4 = ((Vector4)obj).x.ToString() + ",";
                vector4 += ((Vector4)obj).y.ToString() + ",";
                vector4 += ((Vector4)obj).z.ToString() + ",";
                vector4 += ((Vector4)obj).w.ToString();
                return vector4;

            case "Quaternion":
                var quaternion = ((Quaternion)obj).x.ToString() + ",";
                quaternion += ((Quaternion)obj).y.ToString() + ",";
                quaternion += ((Quaternion)obj).z.ToString() + ",";
                quaternion += ((Quaternion)obj).w.ToString();
                return quaternion;

            
        }

        return "";
    }
}
