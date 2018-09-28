using UnityEngine;
using Photon.Pun;
using Invector.vCharacterController;            
using Invector.vShooter;
using Invector.vMelee;
using Invector.vItemManager;
using Invector.vCharacterController.vActions;
using Invector;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
//[RequireComponent(typeof(PhotonTransformView))]
//[RequireComponent(typeof(PhotonAnimatorView))]
[RequireComponent(typeof(PhotonRigidbodyView))]
public class PUN_SyncPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Sync Components
    private Transform local_head, local_neck, local_spine, local_chest = null;
    private Quaternion correctBoneHeadRot, correctBoneNeckRot, correctBoneSpineRot, correctBoneChestRot = Quaternion.identity;
    private Vector3 correctPlayerPos = Vector3.zero;
    private Quaternion correctPlayerRot = Quaternion.identity;
    private Dictionary<string, AnimatorControllerParameterType> animParams = new Dictionary<string, AnimatorControllerParameterType>();
    
    PhotonView view;
    Animator animator;
    //private float lag = 0.0f;
    #endregion

    #region Modifiables
    [Tooltip("This will sync the bone positions. Makes it so players on the network can see where this player is looking.")]
    [SerializeField] private bool _syncBones = false;
    [Tooltip("How fast to move bones of network player version when it receives an update from the server.")]
    [SerializeField] private float _boneLerpRate = 5.0f;
    [Tooltip("How fast to move to new position when the networked player receives and update from the server.")]
    [SerializeField] private float _positionLerpRate = 5.0f;
    [Tooltip("How fast to move to new rotation when the networked player receives and update from the server.")]
    [SerializeField] private float _rotationLerpRate = 5.0f;
    [Tooltip("If this is not a locally controller version of this player change the objects tag to be this.")]
    public string noneLocalTag = "Enemy";
    [Tooltip("If this is not a locally controller version of this player change the objects layer to be this. (ONLY SELECT ONE!)")]
    public int _nonAuthoritativeLayer = 9;
    #endregion

    #region Initializations 
    void Start()
    {
        animator = GetComponent<Animator>();
        view = GetComponent<PhotonView>();

        if (GetComponent<PUN_ThirdPersonController>()) GetComponent<PUN_ThirdPersonController>().enabled = true;
        if (GetComponent<vHitDamageParticle>()) GetComponent<vHitDamageParticle>().enabled = true;

        if (view.IsMine == true && PhotonNetwork.IsConnected == true)
        {
            if (GetComponent<PUN_MeleeCombatInput>()) GetComponent<PUN_MeleeCombatInput>().enabled = true;
            if (GetComponent<vMeleeManager>()) GetComponent<vMeleeManager>().enabled = true;
            if (GetComponent<PUN_ShooterMeleeInput>()) GetComponent<PUN_ShooterMeleeInput>().enabled = true;
            if (GetComponent<vShooterManager>()) GetComponent<vShooterManager>().enabled = true;
            if (GetComponent<vAmmoManager>()) GetComponent<vAmmoManager>().enabled = true;
            if (GetComponent<vHeadTrack>()) GetComponent<vHeadTrack>().enabled = true;
            if (GetComponent<vItemManager>()) GetComponent<vItemManager>().enabled = true;
            if (GetComponent<vWeaponHolderManager>()) GetComponent<vWeaponHolderManager>().enabled = true;
            if (GetComponent<vGenericAction>()) GetComponent<vGenericAction>().enabled = true;
            if (GetComponent<vLadderAction>()) GetComponent<vLadderAction>().enabled = true;
            if (GetComponent<vThrowObject>()) GetComponent<vThrowObject>().enabled = true;
            if (GetComponent<vItemManager>()) GetComponent<vItemManager>().enabled = true;
            if (GetComponent<vLockOn>()) GetComponent<vLockOn>().enabled = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(noneLocalTag))
            {
                this.tag = noneLocalTag;
            }
            SetLayer();
            SetTags(animator.GetBoneTransform(HumanBodyBones.Hips).transform);
        }
        if (_syncBones == true)
        {
            SetBones();
        }
        BuildAnimatorParamsDict();
    }
    void SetBones()
    {
        if (local_head == null)
        {
            try
            {
                local_head = animator.GetBoneTransform(HumanBodyBones.Head).transform;
                correctBoneHeadRot = local_head.localRotation;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        if (local_neck == null)
        {
            try
            {
                local_neck = animator.GetBoneTransform(HumanBodyBones.Neck).transform;
                correctBoneNeckRot = local_neck.localRotation;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        if (local_spine == null)
        {
            try
            {
                local_spine = animator.GetBoneTransform(HumanBodyBones.Spine).transform;
                correctBoneSpineRot = local_spine.localRotation;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        if (local_chest == null)
        {
            try
            {
                local_chest = animator.GetBoneTransform(HumanBodyBones.Chest).transform;
                correctBoneChestRot = local_chest.localRotation;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    void SetLayer()
    {
        gameObject.layer = _nonAuthoritativeLayer;
        animator.GetBoneTransform(HumanBodyBones.Hips).transform.parent.gameObject.layer = _nonAuthoritativeLayer;
    }
    void SetTags(Transform target)
    {
        target.tag = noneLocalTag;
        foreach(Transform child in target)
        {
            if (child.tag == "Untagged" || child.tag == "Player")
            {
                child.tag = noneLocalTag;
            }
            SetTags(child);
        }
    }
    void BuildAnimatorParamsDict()
    {
        if (GetComponent<Animator>())
        {
            foreach (var param in GetComponent<Animator>().parameters)
            {
                if (param.type != AnimatorControllerParameterType.Trigger) //Syncing triggers this way is unreliable, send trigger events via RPC
                {
                    animParams.Add(param.name, param.type);
                }
            }
        }
    }
    #endregion

    #region Server Sync Logic
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
    {
        if (stream.IsWriting)   //Authoritative player sending data to server
        {
            if (_syncBones == true)
            {
                //Send Bone Rotations
                stream.SendNext(local_head.localRotation);
                stream.SendNext(local_neck.localRotation);
                stream.SendNext(local_spine.localRotation);
                stream.SendNext(local_chest.localRotation);
            }

            //Send Player Position and rotation
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            //Send Player Animations
            foreach (var item in animParams)
            {
                switch (item.Value)
                {
                    case AnimatorControllerParameterType.Bool:
                        stream.SendNext(animator.GetBool(item.Key));
                        break;
                    case AnimatorControllerParameterType.Float:
                        stream.SendNext(animator.GetFloat(item.Key));
                        break;
                    case AnimatorControllerParameterType.Int:
                        stream.SendNext(animator.GetInteger(item.Key));
                        break;
                }
            }
        }
        else  //Network player copies receiving data from server
        {
            if (_syncBones == true)
            {
                //Receive Bone Rotations
                this.correctBoneHeadRot = (Quaternion)stream.ReceiveNext();
                this.correctBoneNeckRot = (Quaternion)stream.ReceiveNext();
                this.correctBoneSpineRot = (Quaternion)stream.ReceiveNext();
                this.correctBoneChestRot = (Quaternion)stream.ReceiveNext();
            }

            //Receive Player Position and rotation
            this.correctPlayerPos = (Vector3)stream.ReceiveNext();
            this.correctPlayerRot = (Quaternion)stream.ReceiveNext();

            //Receive Player Animations
            foreach (var item in animParams)
            {
                switch (item.Value)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(item.Key,(bool)stream.ReceiveNext());
                        break;
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(item.Key, (float)stream.ReceiveNext());
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(item.Key, (int)stream.ReceiveNext());
                        break;
                }
            }
        }
        //lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
    }

    public void SendTrigger(string name)
    {
        GetComponent<PhotonView>().RPC("SetTrigger", RpcTarget.All, name);
    }

    [PunRPC]
    public void ApplyDamage(string amount)
    {
        if (GetComponent<PhotonView>().IsMine == true)
        {
            vDamage damage = JsonUtility.FromJson<vDamage>(amount);
            GetComponent<vThirdPersonController>().TakeDamage(damage);
        }
    }
    [PunRPC]
    public void ResetTrigger(string name)
    {
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().ResetTrigger(name);
        }
    }
    [PunRPC]
    public void SetTrigger(string name)
    {
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().SetTrigger(name);
        }
    }
    [PunRPC]
    public void CrossFadeInFixedTime(string name, float time)
    {
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().CrossFadeInFixedTime(name, time);
        }
    }
    #endregion

    #region Local Actions Based on Server Changes
    void Update()
    {
        if (photonView.IsMine == false)
        {
            float distance = Vector3.Distance(transform.position, this.correctPlayerPos);
            if (distance < 2f)
            {
                transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * _positionLerpRate);
                transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * _rotationLerpRate);
            }
            else
            {
                transform.position = this.correctPlayerPos;
                transform.rotation = this.correctPlayerRot;
            }
            if (_syncBones == true)
            {
                SyncBoneRotation();
            }
        }
    }
    void LateUpdate()
    {
        if (_syncBones == true && GetComponent<PhotonView>().IsMine == false)
        {
            SyncBoneRotation();
        }
    }
    void SyncBoneRotation()
    {
        //local_head.localRotation = this.correctBoneHeadRot;
        //local_neck.localRotation = this.correctBoneNeckRot;
        //local_spine.localRotation = this.correctBoneSpineRot;
        //local_chest.localRotation = this.correctBoneChestRot;
        local_head.localRotation = Quaternion.Lerp(local_head.localRotation, this.correctBoneHeadRot, Time.deltaTime * _boneLerpRate);
        local_neck.localRotation = Quaternion.Lerp(local_neck.localRotation, this.correctBoneNeckRot, Time.deltaTime * _boneLerpRate);
        local_spine.localRotation = Quaternion.Lerp(local_spine.localRotation, this.correctBoneSpineRot, Time.deltaTime * _boneLerpRate);
        local_chest.localRotation = Quaternion.Lerp(local_chest.localRotation, this.correctBoneChestRot, Time.deltaTime * _boneLerpRate);
    }
    bool notNan(Quaternion value)
    {
        if (!float.IsNaN(value.x) && !float.IsNaN(value.y) && !float.IsNaN(value.z) && !float.IsNaN(value.w))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
