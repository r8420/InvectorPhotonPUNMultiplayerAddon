using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PUN_ThrowObject : vThrowObject
{
    protected override void UpdateInput()
    {
        if (aimThrowInput.GetButtonDown() && !isAiming && !inThrow)
        {
            isAiming = true;
            tpInput.cc.lockInStrafe = true;
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, holdingAnimation, 0.2f);
            onEnableAim.Invoke();
        }
        if (aimThrowInput.GetButtonUp())
        {
            isAiming = false;
            tpInput.cc.lockInStrafe = false;
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, cancelAnimation, 0.2f);
            onCancelAim.Invoke();
        }
        if (throwInput.GetButtonDown() && isAiming && !inThrow)
        {
            isAiming = false;
            isThrowInput = true;
        }
    }

    protected override void UpdateThrow()
    {
        if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction)
        {
            isAiming = false;
            inThrow = false;
            isThrowInput = false;
            if (lineRenderer && lineRenderer.enabled) lineRenderer.enabled = false;
            if (throwEnd && throwEnd.activeSelf) throwEnd.SetActive(false);
            return;
        }

        if (isAiming)
            DrawTrajectory();
        else
        {
            if (lineRenderer && lineRenderer.enabled) lineRenderer.enabled = false;
            if (throwEnd && throwEnd.activeSelf) throwEnd.SetActive(false);
        }

        if (isThrowInput)
        {
            inThrow = true;
            isThrowInput = false;
            GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, throwAnimation, 0.2f);
            currentThrowObject -= 1;
            StartCoroutine(Launch());
        }
    }
}
