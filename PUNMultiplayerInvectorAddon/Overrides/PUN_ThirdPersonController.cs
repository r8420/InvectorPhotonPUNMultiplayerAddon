using UnityEngine;
using Invector.vCharacterController;
using Invector;
using Photon.Pun;

public class PUN_ThirdPersonController : vThirdPersonController
{

    public override void TakeDamage(vDamage damage)
    {

        if (GetComponent<PhotonView>().IsMine == true)
        {
            base.TakeDamage(damage);
        }
        else
        {
            GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.Others, JsonUtility.ToJson(damage));
        }
    }
}
