using UnityEngine;
using UnityEditor;
using Invector.vMelee;
using Invector.vShooter;
using Invector.vCharacterController;
using Invector;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using System;

public class SetupNetworking : EditorWindow
{

    [MenuItem("Invector/Multiplayer/(Optional) Create Network Manager")]
    private static void M_NetworkManager()
    {
        if (!FindObjectOfType<PUN_NetworkManager>())
        {
            GameObject _networkManager = new GameObject("NetworkManager");
            _networkManager.AddComponent<PUN_NetworkManager>();
            _networkManager.AddComponent<PUN_LobbyUI>();
            _networkManager.GetComponent<PUN_NetworkManager>()._spawnPoint = _networkManager.transform;
        }
        else
        {
            PUN_NetworkManager _networkManager = FindObjectOfType<PUN_NetworkManager>();
            _networkManager._spawnPoint = _networkManager.gameObject.transform;
        }
    }

    //# ------------------------------------------------------------ #

    GameObject _player = null;
    GUISkin skin;
    Vector2 rect = new Vector2(400, 180);
    Vector2 max_rect = new Vector2(400, 500);
    Editor playerPreview;
    bool generated = false;
    bool paramSynced = false;
    float timer = 0.0f;
    GameObject _prefab = null;
    [MenuItem("Invector/Multiplayer/02. Make Player Multiplayer Compatible")]
    private static void M_MakePlayerMultiplayer()
    {
        GetWindow<SetupNetworking>("Photon PUN - Make Player Multiplayer Compatible");
    }

    void PlayerPreview()
    {
        GUILayout.FlexibleSpace();

        if (_player != null)
        {
            playerPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(360, 300), "window");
        }
    }

    private void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.maxSize = max_rect;
        this.titleContent = new GUIContent("Photon PUN: Multiplayer", null, "Adds multiplayer support to your player.");
        GUILayout.BeginVertical("Add Multiplayer Compatiblity", "window");
        GUILayout.Space(35);

        GUILayout.BeginVertical("box");

        if (!_player)
        {
            generated = false;
            EditorGUILayout.HelpBox("Input your player gameobject you want to make multiplayer compatible. Will copy, modify, and save that gameobject as a resource (in the \"Assets/Resources\" folder).", MessageType.Info);
        }

        _player = EditorGUILayout.ObjectField("Player Prefab", _player, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

        if (GUI.changed && _player != null)
        {
            playerPreview = Editor.CreateEditor(_player);
        }
        if (paramSynced == false && _player != null && generated == true)
        {
            EditorGUILayout.HelpBox("Waiting for animator params to be found and synced (Note You need to click on this editor window to update it)...", MessageType.Info);
        }
        if (_player != null && generated == true && paramSynced == true)
        {
            EditorGUILayout.HelpBox("Last manual steps! Do these on the gameobject prefab (or gameobject and save it to the prefab)... \n\n 1. Open the \"Photon View\" component and set the \"Observe option\" to \"Reliable Delta Compressed\". \n\n 2. Make sure prefab is assigned to the \"NetworkManager\" \"_playerPrefab\" input.\n\n NOTE: You can find the prefab in the \"Assets/Resources\" folder", MessageType.Info);
        }
        GUILayout.EndVertical();

        if (_player != null)
        {
            GUILayout.BeginHorizontal("box");
            PlayerPreview();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add Multiplayer Support"))
            {
                generated = true;
                M_SetupMultiplayer();
            }
        }
    }

    private void Update()
    {
        if (paramSynced == false)
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                timer = 0;
                SetParamSync(_prefab);
            }
        }
    }
    void M_SetupMultiplayer()
    {
        GameObject prefab = GameObject.Instantiate(_player, _player.transform.position+Vector3.forward, Quaternion.identity) as GameObject;
        prefab.name = "PUN_" + prefab.name.Replace("(Clone)", "");
        Selection.activeGameObject = prefab;

        if (prefab == null)
            return;
        foreach (MonoBehaviour script in prefab.GetComponents(typeof(MonoBehaviour)))
        {
            script.enabled = false;
        }
        if (!prefab.GetComponent<PhotonAnimatorView>()) prefab.AddComponent<PhotonAnimatorView>();
        if (prefab.GetComponent<PUN_SyncPlayer>() == null)
        {
            prefab.AddComponent<PUN_SyncPlayer>();
        }
        _prefab = prefab;
        ModifyComponents(prefab);
        AssignDamageReceivers(prefab);
        MakeAndAssignPrefab(prefab);
    }
    void ModifyComponents(GameObject prefab)
    {
        //Sync Rigidbody
        prefab.GetComponent<PhotonRigidbodyView>().m_SynchronizeVelocity = true;
        prefab.GetComponent<PhotonRigidbodyView>().m_SynchronizeAngularVelocity = true;
        prefab.GetComponent<PhotonRigidbodyView>().m_TeleportEnabled = false;

        //Sync Transform
        prefab.GetComponent<PhotonTransformView>().m_SynchronizePosition = true;
        prefab.GetComponent<PhotonTransformView>().m_SynchronizeRotation = true;
        prefab.GetComponent<PhotonTransformView>().m_SynchronizeScale = false;

        //Sync Animator Params
        SetParamSync(prefab);

        //Add Photon Components To Photon View To Sync Them over network
        prefab.GetComponent<PhotonView>().ObservedComponents = null;
        List<Component> observables = new List<Component>();
        observables.Add(prefab.GetComponent<PhotonTransformView>());
        observables.Add(prefab.GetComponent<PhotonRigidbodyView>());
        observables.Add(prefab.GetComponent<PUN_SyncPlayer>());
        observables.Add(prefab.GetComponent<PhotonAnimatorView>());
        prefab.GetComponent<PhotonView>().ObservedComponents = observables;
        //(Observe Options) https://doc.photonengine.com/en-us/pun/current/getting-started/feature-overview

        //Enable multiplayer compatiable components
        if (prefab.GetComponent<vThirdPersonController>() || prefab.GetComponent<PUN_ThirdPersonController>())
        {
            if (prefab.GetComponent<PUN_ThirdPersonController>()) prefab.AddComponent<PUN_ThirdPersonController>();
            if (!prefab.GetComponent<PUN_ThirdPersonController>()) prefab.AddComponent<PUN_ThirdPersonController>();
            prefab.GetComponent<PUN_ThirdPersonController>().enabled = true;
            prefab.GetComponent<PUN_ThirdPersonController>().useInstance = false;
        }
        if (prefab.GetComponent<vShooterMeleeInput>() || prefab.GetComponent<PUN_ShooterMeleeInput>())
        {
            if (!prefab.GetComponent<PUN_ShooterMeleeInput>()) prefab.AddComponent<PUN_ShooterMeleeInput>();
            prefab.GetComponent<PUN_ShooterMeleeInput>().enabled = false;
        }
        if (!prefab.GetComponent<PUN_ThirdPersonCameraVerify>()) prefab.AddComponent<PUN_ThirdPersonCameraVerify>();

        //Destroy Non Multiplayer Compatible Components
        if (prefab.GetComponent<vThirdPersonController>()) DestroyImmediate(prefab.GetComponent<vThirdPersonController>());
        if (prefab.GetComponent<vShooterMeleeInput>()) DestroyImmediate(prefab.GetComponent<vShooterMeleeInput>());

        //Assign Observable components to PhotonView component
        if (prefab.GetComponent<vRagdoll>()) prefab.GetComponent<vRagdoll>().enabled = true;
        if (prefab.GetComponent<vFootStep>()) prefab.GetComponent<vFootStep>().enabled = true;
        prefab.GetComponent<PUN_ThirdPersonCameraVerify>().enabled = true;
        prefab.GetComponent<PUN_SyncPlayer>().enabled = true;
        prefab.GetComponent<PhotonRigidbodyView>().enabled = true;
        prefab.GetComponent<PhotonTransformView>().enabled = true;
        prefab.GetComponent<PhotonAnimatorView>().enabled = true;
        if (prefab.GetComponent<vFootStep>()) prefab.GetComponent<vFootStep>().enabled = true;

        //Enable Ragdoll colliders to shooter weapons can hit you
        prefab.GetComponent<vRagdoll>().disableColliders = false;
    }
    void MakeAndAssignPrefab(GameObject prefab)
    {
        try
        {
            //Create the Prefab 
            if (!Directory.Exists("Assets/Resources"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory("Assets/Resources");
            }

            if (AssetDatabase.LoadAssetAtPath("Assets/Resources/" + prefab.name + ".prefab", typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                            "The prefab already exists. Do you want to overwrite it?",
                            "Yes",
                            "No"))
                //If the user presses the yes button, create the Prefab
                {
                    CreatePrefab(prefab, "Assets/Resources/" + prefab.name + ".prefab");
                }
                //If the name doesn't exist, create the new Prefab
                else
                {
                    Debug.Log("The prefab for this gameobject \"" + prefab.name + "\" was not made. Make one manually and assign that prefab to the \"_playerPrefab\" on the NetworkManager gameobject.");
                }
            }
            else
            {
                CreatePrefab(prefab, "Assets/Resources/" + prefab.name + ".prefab");
            }
            //Application.dataPath
            M_NetworkManager();
            PUN_NetworkManager nm = FindObjectOfType<PUN_NetworkManager>();
            nm._playerPrefab = (GameObject)Resources.Load(prefab.name);
        }
        catch (Exception e)
        {
            Debug.Log("An error occured. Make sure the prefab was made and that prefab gets assigned to the \"NetworkManager\"");
            Debug.LogError(e);
        }
    }
    void CreatePrefab(GameObject obj, string location)
    {
        Debug.Log("Saving prefab to: " + location);
        UnityEngine.Object newPrefab = PrefabUtility.CreatePrefab(location, obj);
        PrefabUtility.ReplacePrefab(obj, newPrefab, ReplacePrefabOptions.ConnectToPrefab);
        Debug.Log("Prefab successfully made and saved!");
    }
    void SetParamSync(GameObject prefab)
    {
        if (prefab == null)
            return;
        if (!prefab.GetComponent<PhotonAnimatorView>()) prefab.AddComponent<PhotonAnimatorView>();
        if (prefab.GetComponent<PhotonAnimatorView>().GetSynchronizedParameters().Count > 0)
        {
            paramSynced = true;
        }
        foreach (var param in prefab.GetComponent<PhotonAnimatorView>().GetSynchronizedParameters())
        {
            prefab.GetComponent<PhotonAnimatorView>().SetParameterSynchronized(param.Name, param.Type, PhotonAnimatorView.SynchronizeType.Discrete);
        }
        //Sync All Layers
        for (int index = 0; index < prefab.GetComponent<PhotonAnimatorView>().GetSynchronizedLayers().Count; index++)
        {
            prefab.GetComponent<PhotonAnimatorView>().SetLayerSynchronized(index, PhotonAnimatorView.SynchronizeType.Discrete);
        }
        if (paramSynced == true)
        {
            PrefabUtility.ReplacePrefab(prefab, PrefabUtility.GetCorrespondingObjectFromSource(prefab), ReplacePrefabOptions.ConnectToPrefab);
        }
    }
    void AssignDamageReceivers(GameObject prefab)
    {
        TraverseChildren(prefab.transform.root);
        foreach (Transform child in prefab.transform.root)
        {
            if (child.GetComponent<vDamageReceiver>())
            {
                vDamageReceiver original = child.GetComponent<vDamageReceiver>();

                PUN_DamageReceiver pun = child.gameObject.AddComponent<PUN_DamageReceiver>();
                pun.damageMultiplier = child.GetComponent<vDamageReceiver>().damageMultiplier;
                pun.overrideReactionID = child.GetComponent<vDamageReceiver>().overrideReactionID;
                
                DestroyImmediate(original);
            }
        }
    }
    void TraverseChildren(Transform target)
    {
        if (target.GetComponent<vDamageReceiver>())
        {
            vDamageReceiver original = target.GetComponent<vDamageReceiver>();

            PUN_DamageReceiver pun = target.gameObject.AddComponent<PUN_DamageReceiver>();
            pun.damageMultiplier = target.GetComponent<vDamageReceiver>().damageMultiplier;
            pun.overrideReactionID = target.GetComponent<vDamageReceiver>().overrideReactionID;

            DestroyImmediate(original);
        }
        foreach(Transform child in target)
        {
            TraverseChildren(child);
        }
    }
}

