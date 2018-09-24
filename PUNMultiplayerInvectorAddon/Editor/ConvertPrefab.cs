using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConvertPrefab : EditorWindow
{
    [MenuItem("Invector/Multiplayer/(Optional) Convert Prefab To Multiplayer")]
    private static void PUN_ConvertSceneWindow()
    {
        GetWindow<ConvertPrefab>("PUN - Convert Prefab To Multiplayer");
    }

    #region Editor Variables
    GUISkin skin;
    Vector2 rect = new Vector2(400, 180);
    Vector2 maxrect = new Vector2(400, 400);
    GameObject _prefab = null;
    bool _executed = false;
    Editor prefabPreview;
    #endregion

    void PrefabPreview()
    {
        GUILayout.FlexibleSpace();

        if (_prefab != null)
        {
            prefabPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(360, 300), "window");
        }
    }
    private void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.maxSize = maxrect;
        this.titleContent = new GUIContent("PUN: Multiplayer", null, "Converts Prefab To Support Multiplayer.");
        GUILayout.BeginVertical("Add Multiplayer Compatiblity To Prefab", "window");
        GUILayout.Space(35);

        GUILayout.BeginVertical("box");
        if (_prefab == false)
        {
            EditorGUILayout.HelpBox("Input the prefab you want to make multiplayer compatible.", MessageType.Info);
        }
        else if (_prefab == true && _executed == false)
        {
            EditorGUILayout.HelpBox("Now click \"Convert\" to convert this prefab and put a copy of it in the \"Resources\" folder.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("A copy of this prefab that is multiplayer compatible should have been placed into the resources folder.", MessageType.Info);
        }
        _prefab = EditorGUILayout.ObjectField("Target Prefab", _prefab, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        if (_prefab != null)
        {
            prefabPreview = Editor.CreateEditor(_prefab);
        }
        GUILayout.EndVertical();
        if (_prefab != null)
        {
            if (GUILayout.Button("Convert"))
            {
                _executed = true;
                ConvertTargetPrefab();
            }
        }
    }
    void ConvertTargetPrefab()
    {

    }
}
