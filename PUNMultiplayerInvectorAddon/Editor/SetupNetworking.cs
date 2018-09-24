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
using Invector.vItemManager;
using Invector.vCharacterController.vActions;
using System.Reflection;
using UnityEngine.Events;
using UnityEditor.Events;

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
        if (_player != null && generated == true)
        {
            EditorGUILayout.HelpBox("Last manual steps! Do these on the gameobject prefab (or gameobject and save it to the prefab)... \n\n1. Open the \"Photon View\" component and set the \"Observe option\" to \"Reliable Delta Compressed\". \n\n 2. Make sure prefab is assigned to the \"NetworkManager\" \"_playerPrefab\" input.\n\n 3. Any components with custom events will need to be verified. If the component wasn't able to be easily copied it will say \"Missing Component\" on the UnityEvent. Look at the original object and re-add any of the missing components. \n\n 4. When done be sure to apply your changes to the prefab! \n\nNOTE: You can find the prefab in the \"Assets/Resources\" folder", MessageType.Info);
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
        
        ////Add Photon Components To Photon View To Sync Them over network
        prefab.GetComponent<PhotonView>().ObservedComponents = null;
        List<Component> observables = new List<Component>();
        observables.Add(prefab.GetComponent<PhotonRigidbodyView>());
        observables.Add(prefab.GetComponent<PUN_SyncPlayer>());
        prefab.GetComponent<PhotonView>().ObservedComponents = observables;
        //(Observe Options) https://doc.photonengine.com/en-us/pun/current/getting-started/feature-overview

        //Enable multiplayer compatiable components
        Setup_GenericAction(prefab);
        Setup_GenericAnimation(prefab);
        Setup_ThirdPersonController(prefab);
        Setup_ShooterMeleeInput(prefab);
        Setup_MeleeCombatInput(prefab);
        Setup_ThrowObject(prefab);
        Setup_LadderAction(prefab);
        Setup_CameraVerify(prefab);


        //Destroy Non Multiplayer Compatible Components
        DestroyComponents(prefab);

        EnableComponents(prefab);

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
        foreach (Transform child in target)
        {
            TraverseChildren(child);
        }
    }

    #region Component Setups
    void Setup_LadderAction(GameObject prefab)
    {
        if (prefab.GetComponent<vLadderAction>() || prefab.GetComponent<PUN_LadderAction>())
        {
            if (!prefab.GetComponent<PUN_LadderAction>()) prefab.AddComponent<PUN_LadderAction>();
            prefab.GetComponent<PUN_LadderAction>().enabled = false;
            if (prefab.GetComponent<vLadderAction>())
            {
                prefab.GetComponent<PUN_LadderAction>().actionEnter = _player.GetComponent<vLadderAction>().actionEnter;
                prefab.GetComponent<PUN_LadderAction>().actionStay = _player.GetComponent<vLadderAction>().actionStay;
                prefab.GetComponent<PUN_LadderAction>().actionExit = _player.GetComponent<vLadderAction>().actionExit;
                prefab.GetComponent<PUN_LadderAction>().actionTag = _player.GetComponent<vLadderAction>().actionTag;
                prefab.GetComponent<PUN_LadderAction>().debugMode = _player.GetComponent<vLadderAction>().debugMode;

                prefab.GetComponent<PUN_LadderAction>().OnDoAction = prefab.GetComponent<vLadderAction>().OnDoAction;
                prefab.GetComponent<PUN_LadderAction>().OnEnterLadder = prefab.GetComponent<vLadderAction>().OnEnterLadder;
                prefab.GetComponent<PUN_LadderAction>().OnExitLadder = prefab.GetComponent<vLadderAction>().OnExitLadder;
            }
        }
    }
    void Setup_ThrowObject(GameObject prefab)
    {
        if (prefab.GetComponent<vThrowObject>() || prefab.GetComponent<PUN_ThrowObject>())
        {
            if (prefab.GetComponent<PUN_ThrowObject>()) prefab.AddComponent<PUN_ThrowObject>();
            if (!prefab.GetComponent<PUN_ThrowObject>()) prefab.AddComponent<PUN_ThrowObject>();
            prefab.GetComponent<PUN_ThrowObject>().enabled = false;
            if (prefab.GetComponent<vThrowObject>())
            {
                prefab.GetComponent<PUN_ThrowObject>().throwStartPoint = prefab.transform.Find(prefab.GetComponent<vThrowObject>().throwStartPoint.name);
                prefab.GetComponent<PUN_ThrowObject>().throwEnd = prefab.transform.Find(prefab.GetComponent<vThrowObject>().throwEnd.name).gameObject;
                prefab.GetComponent<PUN_ThrowObject>().objectToThrow = _player.GetComponent<vThrowObject>().objectToThrow;
                prefab.GetComponent<PUN_ThrowObject>().obstacles = _player.GetComponent<vThrowObject>().obstacles;
                prefab.GetComponent<PUN_ThrowObject>().throwMaxForce = _player.GetComponent<vThrowObject>().throwMaxForce;
                prefab.GetComponent<PUN_ThrowObject>().throwDelayTime = _player.GetComponent<vThrowObject>().throwDelayTime;
                prefab.GetComponent<PUN_ThrowObject>().lineStepPerTime = _player.GetComponent<vThrowObject>().lineStepPerTime;
                prefab.GetComponent<PUN_ThrowObject>().lineMaxTime = _player.GetComponent<vThrowObject>().lineMaxTime;
                prefab.GetComponent<PUN_ThrowObject>().exitStrafeModeDelay = _player.GetComponent<vThrowObject>().exitStrafeModeDelay;
                prefab.GetComponent<PUN_ThrowObject>().throwAnimation = _player.GetComponent<vThrowObject>().throwAnimation;
                prefab.GetComponent<PUN_ThrowObject>().holdingAnimation = _player.GetComponent<vThrowObject>().holdingAnimation;
                prefab.GetComponent<PUN_ThrowObject>().cancelAnimation = _player.GetComponent<vThrowObject>().cancelAnimation;
                prefab.GetComponent<PUN_ThrowObject>().maxThrowObjects = _player.GetComponent<vThrowObject>().maxThrowObjects;
                prefab.GetComponent<PUN_ThrowObject>().currentThrowObject = _player.GetComponent<vThrowObject>().currentThrowObject;
                prefab.GetComponent<PUN_ThrowObject>().debug = _player.GetComponent<vThrowObject>().debug;
                
                prefab.GetComponent<PUN_ThrowObject>().onEnableAim = prefab.GetComponent<vThrowObject>().onEnableAim;
                prefab.GetComponent<PUN_ThrowObject>().onCancelAim = prefab.GetComponent<vThrowObject>().onCancelAim;
                prefab.GetComponent<PUN_ThrowObject>().onThrowObject = prefab.GetComponent<vThrowObject>().onThrowObject;
                prefab.GetComponent<PUN_ThrowObject>().onCollectObject = prefab.GetComponent<vThrowObject>().onCollectObject;
            }
        }
    }
    void Setup_CameraVerify(GameObject prefab)
    {
        if (!prefab.GetComponent<PUN_ThirdPersonCameraVerify>()) prefab.AddComponent<PUN_ThirdPersonCameraVerify>();
    }
    void Setup_MeleeCombatInput(GameObject prefab)
    {
        if (!prefab.GetComponent<vShooterMeleeInput>() && (prefab.GetComponent<vMeleeCombatInput>() || prefab.GetComponent<PUN_MeleeCombatInput>()))
        {
            if (!prefab.GetComponent<PUN_MeleeCombatInput>()) prefab.AddComponent<PUN_MeleeCombatInput>();
            prefab.GetComponent<PUN_MeleeCombatInput>().enabled = false;
        }
    }
    void Setup_ShooterMeleeInput(GameObject prefab)
    {
        if (prefab.GetComponent<vShooterMeleeInput>() || prefab.GetComponent<PUN_ShooterMeleeInput>())
        {
            if (!prefab.GetComponent<PUN_ShooterMeleeInput>()) prefab.AddComponent<PUN_ShooterMeleeInput>();
            prefab.GetComponent<PUN_ShooterMeleeInput>().enabled = false;
        }
    }
    void Setup_ThirdPersonController(GameObject prefab)
    {
        if (prefab.GetComponent<vThirdPersonController>() || prefab.GetComponent<PUN_ThirdPersonController>())
        {
            if (prefab.GetComponent<PUN_ThirdPersonController>()) prefab.AddComponent<PUN_ThirdPersonController>();
            if (!prefab.GetComponent<PUN_ThirdPersonController>()) prefab.AddComponent<PUN_ThirdPersonController>();
            prefab.GetComponent<PUN_ThirdPersonController>().enabled = true;
            prefab.GetComponent<PUN_ThirdPersonController>().useInstance = false;
            //foreach (var prop in prefab.GetComponent<vThirdPersonAnimator>().GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ))
            //{
            //    if (prop.FieldType == typeof(UnityEvent))
            //    {
            //        Debug.Log("NAME: " + prop.Name);
            //    }
            //}

            //MethodInfo info = UnityEventBase.GetValidMethodInfo(OnReceiveDamage.GetPersistentTarget(0), OnReceiveDamage.GetPersistentMethodName(0), new Type[] { typeof(float) });
            //methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), audioSrc, "Play") as UnityAction;
            //UnityEditor.Events.UnityEventTools.AddPersistentListener(events, methodDelegate);
        }
    }
    void Setup_GenericAnimation(GameObject prefab)
    {
        if (prefab.GetComponent<vGenericAnimation>() || prefab.GetComponent<PUN_GenericAnimation>())
        {
            if (prefab.GetComponent<PUN_GenericAnimation>()) prefab.AddComponent<PUN_GenericAnimation>();
            if (!prefab.GetComponent<PUN_GenericAnimation>()) prefab.AddComponent<PUN_GenericAnimation>();
            prefab.GetComponent<PUN_GenericAnimation>().enabled = false;
            if (prefab.GetComponent<vGenericAnimation>())
            {
                prefab.GetComponent<PUN_GenericAnimation>().GetType().GetProperty("OnDoAction").SetValue(prefab, prefab.GetComponent<vGenericAnimation>().GetType().GetProperty("OnDoAction"), null);
                prefab.GetComponent<PUN_GenericAnimation>().GetType().GetProperty("OnStartAction").SetValue(prefab, prefab.GetComponent<vGenericAnimation>().GetType().GetProperty("OnStartAction"), null);
                prefab.GetComponent<PUN_GenericAnimation>().GetType().GetProperty("OnEndAction").SetValue(prefab, prefab.GetComponent<vGenericAnimation>().GetType().GetProperty("OnEndAction"), null);
            }
        }
    }
    void Setup_GenericAction(GameObject prefab)
    {
        if (prefab.GetComponent<vGenericAction>() || prefab.GetComponent<PUN_GenericAction>())
        {
            if (prefab.GetComponent<PUN_GenericAction>()) prefab.AddComponent<PUN_GenericAction>();
            if (!prefab.GetComponent<PUN_GenericAction>()) prefab.AddComponent<PUN_GenericAction>();
            prefab.GetComponent<PUN_GenericAction>().enabled = false;
            if (prefab.GetComponent<vGenericAction>())
            {
                prefab.GetComponent<PUN_GenericAction>().actionEnter = _player.GetComponent<vGenericAction>().actionEnter;
                prefab.GetComponent<PUN_GenericAction>().actionStay = _player.GetComponent<vGenericAction>().actionStay;
                prefab.GetComponent<PUN_GenericAction>().actionExit = _player.GetComponent<vGenericAction>().actionExit;
                prefab.GetComponent<PUN_GenericAction>().actionInput = _player.GetComponent<vGenericAction>().actionInput;
                prefab.GetComponent<PUN_GenericAction>().actionTag = _player.GetComponent<vGenericAction>().actionTag;
                prefab.GetComponent<PUN_GenericAction>().useRootMotion = _player.GetComponent<vGenericAction>().useRootMotion;
                prefab.GetComponent<PUN_GenericAction>().triggerAction = _player.GetComponent<vGenericAction>().triggerAction;
                prefab.GetComponent<PUN_GenericAction>().debugMode = _player.GetComponent<vGenericAction>().debugMode;
                prefab.GetComponent<PUN_GenericAction>().canTriggerAction = _player.GetComponent<vGenericAction>().canTriggerAction;
                prefab.GetComponent<PUN_GenericAction>().isPlayingAnimation = _player.GetComponent<vGenericAction>().isPlayingAnimation;
                prefab.GetComponent<PUN_GenericAction>().triggerActionOnce = _player.GetComponent<vGenericAction>().triggerActionOnce;

                prefab.GetComponent<PUN_GenericAction>().OnDoAction = prefab.GetComponent<vGenericAction>().OnDoAction;
                prefab.GetComponent<PUN_GenericAction>().OnStartAction = prefab.GetComponent<vGenericAction>().OnStartAction;
                prefab.GetComponent<PUN_GenericAction>().OnEndAction = prefab.GetComponent<vGenericAction>().OnEndAction;
            }
        }
    }
    void EnableComponents(GameObject prefab)
    {
        if (prefab.GetComponent<vRagdoll>()) prefab.GetComponent<vRagdoll>().enabled = true;
        if (prefab.GetComponent<vFootStep>()) prefab.GetComponent<vFootStep>().enabled = true;
        prefab.GetComponent<PUN_ThirdPersonCameraVerify>().enabled = true;
        prefab.GetComponent<PUN_SyncPlayer>().enabled = true;
        prefab.GetComponent<PhotonRigidbodyView>().enabled = true;
        if (prefab.GetComponent<vFootStep>()) prefab.GetComponent<vFootStep>().enabled = true;
    }
    #endregion

    #region Destroy Components
    void DestroyComponents(GameObject prefab)
    {
        if (prefab.GetComponent<vThirdPersonController>()) DestroyImmediate(prefab.GetComponent<vThirdPersonController>());
        if (prefab.GetComponent<vShooterMeleeInput>()) DestroyImmediate(prefab.GetComponent<vShooterMeleeInput>());
        if (!prefab.GetComponent<vShooterMeleeInput>() && prefab.GetComponent<vMeleeCombatInput>()) DestroyImmediate(prefab.GetComponent<vMeleeCombatInput>());
        if (prefab.GetComponent<vGenericAction>()) DestroyImmediate(prefab.GetComponent<vGenericAction>());
        if (prefab.GetComponent<vGenericAnimation>()) DestroyImmediate(prefab.GetComponent<vGenericAnimation>());
        if (prefab.GetComponent<vThrowObject>()) DestroyImmediate(prefab.GetComponent<vThrowObject>());
        if (prefab.GetComponent<vLadderAction>()) DestroyImmediate(prefab.GetComponent<vLadderAction>());
    }
    #endregion

    #region Copying UnityEvents
    //void RebuildMissingComponents(GameObject prefab, UnityEvent prefabsEvent, UnityEvent originalEvent)
    //{
    //    for (int i = 0; i < prefabsEvent.GetPersistentEventCount(); i++)
    //    {
    //        if (prefabsEvent.GetPersistentTarget(i) == null)
    //        {
    //            if (prefabsEvent.GetPersistentTarget(i).GetType() == typeof(Component))
    //            {
    //                Component val = GetValidComponent(prefab.transform, prefabsEvent.GetPersistentTarget(i).name);
    //                MethodInfo info = UnityEventBase.GetValidMethodInfo(prefabsEvent.GetPersistentTarget(i), prefabsEvent.GetPersistentMethodName(i), new Type[] { typeof(Component) });
    //                UnityAction<Component> execute = (Component obj) => info.Invoke(val, new UnityEngine.Component[] { val });
    //                UnityEventTools.AddPersistentListener(execute);
    //                UnityEventTools.AddObjectPersistentListener<GameObject>(dest, execute, arg.gameObject);
    //            }
    //        }
    //    }
    //}
    //Component GetValidComponent(Transform target, string componentName)
    //{
    //    Component val = null;
    //    foreach (Component comp in target)
    //    {
    //        if (comp.name == componentName)
    //        {
    //            return comp;
    //        }
    //    }
    //    foreach (Transform child in target)
    //    {
    //        foreach(Component comp in child)
    //        {
    //            if (comp.name == componentName)
    //            {
    //                return comp;
    //            }
    //        }
    //        val = GetValidComponent(child, componentName);
    //        if (val != null)
    //        {
    //            return val;
    //        }
    //    }
    //    return null;
    //}
    //void CopyUnityEvent(GameObject prefab, UnityEvent source, UnityEvent dest)
    //{
    //    dest = source;
    //    for (int i = 0; i < source.GetPersistentEventCount(); i++)
    //    {
    //        if (source.GetPersistentTarget(i).GetType() == typeof(GameObject))
    //        {
    //            Transform arg = FindValidTarget(_player, prefab, source, i);
    //            MethodInfo info = UnityEventBase.GetValidMethodInfo(source.GetPersistentTarget(i), source.GetPersistentMethodName(i), new Type[] { typeof(GameObject) });
    //            UnityAction<GameObject> execute = (GameObject obj) => info.Invoke(arg, new UnityEngine.Object[] { arg });
    //            UnityEventTools.AddObjectPersistentListener<GameObject>(dest, execute, arg.gameObject);
    //        }
    //    }
    //    //for (int i = 0; i < source.GetPersistentEventCount(); i++)
    //    //{
    //    //    Type inputType = source.GetPersistentTarget(i).GetType();
    //    //    MethodInfo info;
    //    //    if (inputType == typeof(bool))
    //    //    {
    //    //        info = UnityEventBase.GetValidMethodInfo(source.GetPersistentTarget(i), source.GetPersistentMethodName(i), new Type[] { typeof(bool) });
    //    //        //execute = () => info.Invoke(source.GetPersistentTarget(i), new object[] { source.GetPersistentTarget(i) });
    //    //    }
    //    //    else if (inputType == typeof(float))
    //    //    {
    //    //        info = UnityEventBase.GetValidMethodInfo(source.GetPersistentTarget(i), source.GetPersistentMethodName(i), new Type[] { typeof(float) });
    //    //        //execute = () => info.Invoke(source.GetPersistentTarget(i), new object[] { source.GetPersistentTarget(i) });
    //    //    }
    //    //    else if (inputType == typeof(int))
    //    //    {
    //    //        info = UnityEventBase.GetValidMethodInfo(source.GetPersistentTarget(i), source.GetPersistentMethodName(i), new Type[] { typeof(int) });
    //    //        //execute = () => info.Invoke(source.GetPersistentTarget(i), new object[] { source.GetPersistentTarget(i) });
    //    //    }
    //    //    else
    //    //    {
    //    //        UnityEngine.Object arg = (UnityEngine.Object)FindValidTarget(_player, prefab, source, i);
    //    //        //MethodInfo function = GetFunction(prefab, source.GetPersistentMethodName(i));
    //    //        //Debug.Log(source.GetPersistentMethodName(i));
    //    //        //Debug.Log("FUNCTION:" + function);

    //        //        UnityAction<UnityEngine.Object> action = System.Delegate.CreateDelegate(typeof(UnityAction), source.GetPersistentMethodName(i), eventName) as UnityAction<UnityEngine.Object>;
    //        //        UnityEventTools.AddObjectPersistentListener(dest, action, arg);
    //        //        //action 
    //        //        //UnityAction<UnityEngine.Object> execute = (UnityEngine.Object obj) => info.Invoke(arg, new UnityEngine.Object[] { arg });
    //        //        //UnityEventTools.AddObjectPersistentListener(dest, execute, arg);
    //        //    }
    //        //}
    //}
    //Transform FindValidTarget(GameObject owner, GameObject prefab, UnityEvent target, int index)
    //{
    //    Transform retVal = null;
    //    var argument = target.GetPersistentTarget(index);
    //    if (argument.GetType() == typeof(GameObject) || argument.GetType() == typeof(Transform))
    //    {
    //        retVal = GetObjectWithName(owner.transform, argument.name);
    //        if (retVal != null)
    //        {
    //            retVal = GetObjectWithName(prefab.transform, argument.name);
    //        }
    //    }

    //    return retVal;
    //}
    //Transform GetObjectWithName(Transform parent, string nameToSearchFor)
    //{
    //    Transform retVal = null;
    //    foreach(Transform child in parent)
    //    {
    //        if (child.name == nameToSearchFor)
    //        {
    //            retVal = child;
    //            break;
    //        }
    //        else
    //        {
    //            retVal = GetObjectWithName(child, nameToSearchFor);
    //            if (retVal != null)
    //            {
    //                break;
    //            }
    //        }
    //    }
    //    return retVal;
    //}
    //MethodInfo GetFunction(GameObject target, string functionName)
    //{
    //    MethodInfo retVal = null;
        
    //    foreach (var component in target.GetComponents<Component>())
    //    {
    //        foreach (var method in component.GetType().GetMethods())
    //        {
    //            if (method.Name == functionName)
    //            {
    //                retVal = component.GetType().GetMethod(functionName);
    //                break;
    //            }
    //        }
    //        if (retVal != null)
    //        {
    //            break;
    //        }
    //    }

    //    return retVal;
    //}
    #endregion
}
