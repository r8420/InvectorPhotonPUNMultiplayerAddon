using UnityEngine;
using Photon.Pun;
using Invector.vCharacterController;            
using Invector.vShooter;
using Invector.vMelee;
using Invector.vItemManager;
using Invector.vCharacterController.vActions;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(PhotonAnimatorView))]
[RequireComponent(typeof(PhotonRigidbodyView))]
public class PUN_SyncPlayer : MonoBehaviour, IPunObservable
{
    #region Reference Components
    public string noneLocalTag;
    private Transform local_head, local_neck, local_spine, local_chest = null;
    private Quaternion server_head, server_neck, server_spine, server_chest = Quaternion.identity;
    private Quaternion potential_head, potential_neck, potential_spine, potential_chest = Quaternion.identity;
    PhotonView view;
    Animator animator;
    #endregion

    #region Modifiables
    [Tooltip("This will sync the bone positions. Makes it so players on the network can see where this player is looking.")]
    [SerializeField] private bool _syncBones = true;
    [Tooltip("How fast to move bones of network player version when it receives an update from the server.")]
    [SerializeField] private float _boneLerpRate = 90.0f;
    #endregion

    #region Initializations
    void Start()
    {
        animator = GetComponent<Animator>();
        view = GetComponent<PhotonView>();
        if (view.IsMine == true && PhotonNetwork.IsConnected == true)
        {
            if (GetComponent<vShooterMeleeInput>()) GetComponent<vShooterMeleeInput>().enabled = true;
            if (GetComponent<vShooterManager>()) GetComponent<vShooterManager>().enabled = true;
            if (GetComponent<vMeleeManager>()) GetComponent<vMeleeManager>().enabled = true;
            if (GetComponent<vAmmoManager>()) GetComponent<vAmmoManager>().enabled = true;
            if (GetComponent<vHeadTrack>()) GetComponent<vHeadTrack>().enabled = true;
            if (GetComponent<vItemManager>()) GetComponent<vItemManager>().enabled = true;
            if (GetComponent<vWeaponHolderManager>()) GetComponent<vWeaponHolderManager>().enabled = true;
            if (GetComponent<vGenericAction>()) GetComponent<vGenericAction>().enabled = true;
            if (GetComponent<vLadderAction>()) GetComponent<vLadderAction>().enabled = true;
            if (GetComponent<vThrowObject>()) GetComponent<vThrowObject>().enabled = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(noneLocalTag))
            {
                this.tag = noneLocalTag;
            }
        }
        if (_syncBones == true)
        {
            SetBones();
        }
    }
    void SetBones()
    {
        if (local_head == null)
        {
            try
            {
                local_head = animator.GetBoneTransform(HumanBodyBones.Head).transform;
                server_head = local_head.localRotation;
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
                server_neck = local_neck.localRotation;
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
                server_spine = local_spine.localRotation;
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
                server_chest = local_chest.localRotation;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    #endregion

    #region Server Sync Logic
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //this function called by Photon View component
    {
        if (_syncBones == true)
        {
            if (stream.IsWriting)   //Authoritative player sending data to server
            {
                stream.SendNext(local_head.localRotation);
                stream.SendNext(local_neck.localRotation);
                stream.SendNext(local_spine.localRotation);
                stream.SendNext(local_chest.localRotation);
            }
            else if(stream.IsReading) //Network player copies receiving data from server
            {
                this.potential_head = (Quaternion)stream.ReceiveNext();
                this.potential_neck = (Quaternion)stream.ReceiveNext();
                this.potential_spine = (Quaternion)stream.ReceiveNext();
                this.potential_chest = (Quaternion)stream.ReceiveNext();

                server_head = (notNan(potential_head) && potential_head != Quaternion.identity) ? potential_head : server_head;
                server_neck = (notNan(potential_neck) && potential_neck != Quaternion.identity) ? potential_neck : server_neck;
                server_spine = (notNan(potential_spine) && potential_spine != Quaternion.identity) ? potential_spine : server_spine;
                server_chest = (notNan(potential_chest) && potential_chest != Quaternion.identity) ? potential_chest : server_chest;
            }
        }
    }
    #endregion

    #region Local Actions Based on Server Changes
    void LateUpdate()
    {
        if (GetComponent<PhotonView>().IsMine == false)
        {
            SyncBoneRotation();
        }
    }
    void SyncBoneRotation()
    {
        local_head.localRotation = Quaternion.Lerp(local_head.localRotation, server_head, Time.deltaTime * _boneLerpRate);
        local_neck.localRotation = Quaternion.Lerp(local_neck.localRotation, server_neck, Time.deltaTime * _boneLerpRate);
        local_spine.localRotation = Quaternion.Lerp(local_spine.localRotation, server_spine, Time.deltaTime * _boneLerpRate);
        local_chest.localRotation = Quaternion.Lerp(local_chest.localRotation, server_chest, Time.deltaTime * _boneLerpRate);
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
