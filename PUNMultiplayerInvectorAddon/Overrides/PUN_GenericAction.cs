using Invector.vCharacterController.vActions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PUN_GenericAction : vGenericAction {
    public override void TriggerAnimation() {
        if (playingAnimation || actionStarted) return;
        doingAction = true;
        OnDoAction.Invoke(triggerAction);

        if (debugMode) Debug.Log("TriggerAnimation", gameObject);

        // trigger the animation behaviour & match target
        // if (!string.IsNullOrEmpty(triggerAction.playAnimation)) {
        //     playingAnimation = true;
        //     GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, triggerAction.playAnimation, 0.1f); // trigger the action animation clip
        // }



        if (triggerAction.animatorActionState != 0) {
            if (debugMode) Debug.Log("Applied ActionState: " + triggerAction.animatorActionState, gameObject);
            tpInput.cc.SetActionState(triggerAction.animatorActionState);
        }

        // trigger the animation behaviour & match target
        if (!string.IsNullOrEmpty(triggerAction.playAnimation)) {
            if (!actionStarted) {
                actionStarted = true;
                playingAnimation = true;
                // tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, 0.1f);    // trigger the action animation clip
                GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, triggerAction.playAnimation, 0.1f); // trigger the action animation clip
                if (!string.IsNullOrEmpty(triggerAction.customCameraState))
                    tpInput.ChangeCameraState(triggerAction.customCameraState, true);                 // change current camera state to a custom
            }
        } else {
            actionStarted = true;
        }
        // animationBehaviourDelay = 0.2f;






        // trigger OnDoAction Event, you can add a delay in the inspector   
        StartCoroutine(triggerAction.OnDoActionDelay(gameObject));

        // bool to limit the autoAction run just once
        // if (triggerAction.autoAction || triggerAction.destroyAfter) triggerActionOnce = true;
        // new animation triggers dont have the concept of once

        // destroy the triggerAction if checked with destroyAfter
        if (triggerAction.destroyAfter)
            StartCoroutine(DestroyActionDelay(triggerAction));
    }


    // public override void TriggerAnimation() {
    //     if (debugMode) Debug.Log("TriggerAnimation");

    //     // trigger the animation behaviour & match target
    //     if (!string.IsNullOrEmpty(triggerAction.playAnimation)) {
    //         playingAnimation = true;
    //         GetComponent<PhotonView>().RPC("CrossFadeInFixedTime", RpcTarget.All, triggerAction.playAnimation, 0.1f); // trigger the action animation clip
    //     }

    //     // trigger OnDoAction Event, you can add a delay in the inspector   
    //     StartCoroutine(triggerAction.OnDoActionDelay(gameObject));

    //     // bool to limit the autoAction run just once
    //     // if (triggerAction.autoAction || triggerAction.destroyAfter) triggerActionOnce = true;
    //     // new animation triggers dont have the concept of once

    //     // destroy the triggerAction if checked with destroyAfter
    //     if (triggerAction.destroyAfter)
    //         StartCoroutine(DestroyActionDelay(triggerAction));
    // }
}
