using Invector.vCharacterController;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Invector.vCharacterController.AI;
using Invector.vShooter;

public class ConvertScene : EditorWindow
{
    [MenuItem("Invector/Multiplayer/03. Convert Scene To Multiplayer")]
    private static void PUN_ConvertSceneWindow()
    {
        GetWindow<ConvertScene>("PUN - Convert Scene To Multiplayer");
    }

    #region Editor Variables
    GUISkin skin;
    Vector2 rect = new Vector2(400, 180);
    Vector2 maxrect = new Vector2(400, 400);
    private bool _scanned = false;
    private bool _executed = false;
    public enum M_FileAddtionType { Replace, NewLine, InsertLine }
    List<GameObject> modified = new List<GameObject>();
    List<GameObject> found = new List<GameObject>();
    Vector2 _scanScrollPos;
    Vector2 _modifiedScrollPos;
    bool _ignorePlayers = true;
    List<bool> buttonOn = new List<bool>();
    #endregion

    private void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.maxSize = maxrect;
        this.titleContent = new GUIContent("PUN: Multiplayer", null, "Converts Scene To Support Multiplayer.");
        GUILayout.BeginVertical("Add Multiplayer Compatiblity To Scene", "window");
        GUILayout.Space(35);

        GUILayout.BeginVertical("box");
        if (_scanned == false)
        {
            EditorGUILayout.HelpBox("First scan the scene and get a list of objects that will be modified. (Can be edited next)", MessageType.Info);
        }
        else if (_executed == false)
        {
            EditorGUILayout.HelpBox("Now select a button to see it in the hierarchy. A new button will appear that will allow you to remove it from the future modification list.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Now go to each gameobject and make sure the \"PhotonView\" is turned on to the setting you want. Note: You can select each button below to be directed to each gameobject.", MessageType.Info);
        }
        GUILayout.EndVertical();
        if (_scanned == false)
        {
            _ignorePlayers = EditorGUILayout.Toggle("Ignore Players/AI:",_ignorePlayers);
            if (GUILayout.Button("Scan Scene"))
            {
                _scanned = true;
                PUN_ScanScene();
            }
        }
        else if (_executed == false)
        {
            _scanScrollPos = EditorGUILayout.BeginScrollView(_scanScrollPos, GUILayout.Width(350), GUILayout.Height(250));
            for (int i = 0; i < found.Count; i++)
            {
                if (GUILayout.Button(found[i].name))
                {
                    Selection.activeGameObject = found[i];
                    buttonOn[i] = !buttonOn[i];
                }
                if (buttonOn[i] == true)
                {
                    if (GUILayout.Button(" !! REMOVE !!: " + found[i].name))
                    {
                        found.RemoveAt(i);
                        buttonOn.RemoveAt(i);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Convert Objects To Multiplayer"))
            {
                _executed = true;
                PUN_ConvertSceneToMultiplayer();
            }
        }
        else
        {
            _modifiedScrollPos = EditorGUILayout.BeginScrollView(_modifiedScrollPos, GUILayout.Width(350),GUILayout.Height(250), GUILayout.ExpandWidth(true));
            foreach(GameObject obj in modified)
            {
                if (GUILayout.Button(obj.name))
                {
                    Selection.activeGameObject = obj;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
    private void PUN_ScanScene()
    {
        found.Clear();
        //Find vThrowUI
        vThrowUI[] uis = FindObjectsOfType<vThrowUI>();
        foreach (vThrowUI ui in uis)
        {
            found.Add(ui.gameObject);
            buttonOn.Add(false);
        }

        //Find Control Aim Canvas
        vControlAimCanvas[] canvases = FindObjectsOfType<vControlAimCanvas>();
        foreach (vControlAimCanvas canvas in canvases)
        {
            found.Add(canvas.gameObject);
            buttonOn.Add(false);
        }

        //Find Rigidbodies
        Rigidbody[] bodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody body in bodies)
        {
            if (_ignorePlayers == true)
            {
                if (!body.transform.root.gameObject.GetComponent<PUN_ThirdPersonController>() && !body.transform.root.gameObject.GetComponent<vThirdPersonController>() &&
                    !body.transform.root.gameObject.GetComponent<v_AIController>())
                {
                    found.Add(body.gameObject);
                    buttonOn.Add(false);
                }
            }
            else
            {
                found.Add(body.gameObject);
                buttonOn.Add(false);
            }
        }
    }
    private void PUN_ConvertSceneToMultiplayer()
    {
        modified.Clear();
        foreach(GameObject obj in found)
        {
            if (obj.GetComponent<vThrowUI>() || obj.GetComponent<PUN_ThrowUI>())
            {
                PUN_ConvertThrowUI(obj);
            }
            else if (obj.GetComponent<vControlAimCanvas>() || obj.GetComponent<PUN_ControlAimCanvas>())
            {
                PUN_ConvertControlAimCanvas(obj);
            }
            else if (obj.GetComponent<Rigidbody>())
            {
                PUN_ConvertRigidbody(obj);
            }
        }
    }
    private void PUN_ConvertThrowUI(GameObject obj)
    {
        if (!obj.GetComponent<PUN_ThrowUI>())
        {
            obj.AddComponent<PUN_ThrowUI>();
        }
        if (obj.GetComponent<vThrowUI>())
        {
            obj.GetComponent<PUN_ThrowUI>().maxThrowCount = obj.GetComponent<vThrowUI>().maxThrowCount;
            obj.GetComponent<PUN_ThrowUI>().currentThrowCount = obj.GetComponent<vThrowUI>().currentThrowCount;
            vThrowUI ui = obj.GetComponent<vThrowUI>();
            if (ui.GetType() != typeof(PUN_ThrowUI))
            {
                DestroyImmediate(ui);
            }
        }
        obj.GetComponent<PUN_ThrowUI>().enabled = true;

        modified.Add(obj);
    }
    private void PUN_ConvertRigidbody(GameObject obj)
    {
        if (!obj.GetComponent<PhotonView>())
        {
            obj.AddComponent<PhotonView>();
        }
        if (!obj.GetComponent<PhotonRigidbodyView>())
        {
            obj.AddComponent<PhotonRigidbodyView>();
        }
        obj.GetComponent<PhotonRigidbodyView>().m_SynchronizeAngularVelocity = true;
        obj.GetComponent<PhotonRigidbodyView>().m_SynchronizeVelocity = true;
        List<Component> observe = new List<Component>();
        observe.Add(obj.GetComponent<PhotonRigidbodyView>());
        obj.GetComponent<PhotonView>().ObservedComponents = observe;

        modified.Add(obj);
    }
    private void PUN_ConvertControlAimCanvas(GameObject obj)
    {
        if (!obj.GetComponent<PUN_ControlAimCanvas>())
        {
            obj.AddComponent<PUN_ControlAimCanvas>();
        }
        if (obj.GetComponent<vControlAimCanvas>())
        {
            obj.GetComponent<PUN_ControlAimCanvas>().canvas = obj.GetComponent<vControlAimCanvas>().canvas;
            obj.GetComponent<PUN_ControlAimCanvas>().aimCanvasCollection = obj.GetComponent<vControlAimCanvas>().aimCanvasCollection;
            obj.GetComponent<PUN_ControlAimCanvas>().currentAimCanvas = obj.GetComponent<vControlAimCanvas>().currentAimCanvas;
            vControlAimCanvas canvas = obj.GetComponent<vControlAimCanvas>();
            if (canvas.GetType() != typeof(PUN_ControlAimCanvas))
            {
                DestroyImmediate(canvas);
            }
        }
        obj.GetComponent<PUN_ControlAimCanvas>().enabled = true;

        modified.Add(obj);
    }
}
