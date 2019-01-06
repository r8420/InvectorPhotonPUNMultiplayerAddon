using System;
using System.Reflection;
using UnityEngine;

public static class PUN_Helpers
{
    public static void CopyComponentTo(Component orgComp, Component newComp)
    {
        Type type = orgComp.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);

        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                pinfo.SetValue(newComp, pinfo.GetValue(orgComp, null), null);
            }
        }

        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(newComp, finfo.GetValue(orgComp));
        }
    }
}


