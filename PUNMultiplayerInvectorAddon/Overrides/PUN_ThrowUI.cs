using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using Photon.Pun;

public class PUN_ThrowUI : vThrowUI
{
    protected override void Start()
    {
        if (GetLocalPlayer())
        {
            base.Start();
        }
    }

    private void FixedUpdate()
    {
        if (throwManager == null && FindObjectOfType<vThrowObject>())
        {
            throwManager = GetLocalPlayer();
            throwManager.onCollectObject.AddListener(UpdateCount);
            throwManager.onThrowObject.AddListener(UpdateCount);
            UpdateCount();
        }
    }

    vThrowObject GetLocalPlayer()
    {
        vThrowObject script = null;
        foreach (vThrowObject player in FindObjectsOfType<vThrowObject>())
        {
            if (player.gameObject.GetComponent<PhotonView>().IsMine)
            {
                script = player;
                break;
            }
        }

        return script;
    }
}
