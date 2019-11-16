using Invector.vShooter;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class PUN_ShooterWeapon : vShooterWeapon {

    public override void Shoot(Transform _sender = null, UnityAction<bool> successfulShot = null) {
        base.Shoot(_sender);
        if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PUN_SyncPlayer>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
            _sender.GetComponent<PhotonView>().RPC("SendShootEffect", RpcTarget.Others, transform.parent.name, transform.name);
        }
        // print("1 Shoot " + ammo + "  " + ammoCount);
    }
    public override void Shoot(Vector3 aimPosition, Transform _sender = null, UnityAction<bool> successfulShot = null) {
        base.Shoot(aimPosition, _sender);
        if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PUN_SyncPlayer>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
            _sender.GetComponent<PhotonView>().RPC("SendShootEffect", RpcTarget.Others, JsonUtility.ToJson(aimPosition), transform.parent.name, transform.name);
        }
        // print("2 Shoot " + ammo + "  " + ammoCount);
    }
    public override void ReloadEffect() {
        base.ReloadEffect();
        if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PhotonView>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
            transform.root.GetComponent<PhotonView>().RPC("SendReloadEffect", RpcTarget.Others, transform.parent.name, transform.name);
        }
    }
    public override void FinishReloadEffect() {
        base.ReloadEffect();
        if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PhotonView>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
            print("Finish Reload " + ammo + "  " + ammoCount);
            transform.root.GetComponent<PhotonView>().RPC("SendFinishReloadEffect", RpcTarget.Others, transform.parent.name, transform.name, ammoCount);
        }
    }
    // protected override void EmptyClipEffect() {
    //     base.EmptyClipEffect();
    //     if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PhotonView>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
    //         transform.root.GetComponent<PhotonView>().RPC("SendEmptyClipEffect", RpcTarget.Others, transform.parent.name, transform.name);
    //     }
    // }
    // protected override void StopSound() {
    //     base.StopSound();
    //     if (PhotonNetwork.IsConnected == true && transform.root.GetComponent<PhotonView>() && transform.root.GetComponent<PhotonView>().IsMine == true) {
    //         transform.root.GetComponent<PhotonView>().RPC("SendStopSound", RpcTarget.Others, transform.parent.name, transform.name);
    //     }
    // }
}
