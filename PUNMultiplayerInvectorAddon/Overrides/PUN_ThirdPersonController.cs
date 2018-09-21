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

    protected override void TriggerDamageReaction(vDamage damage)
    {
        if (animator != null && animator.enabled && !damage.activeRagdoll && currentHealth > 0)
        {
            if (damage.sender && hitDirectionHash.isValid) animator.SetInteger(hitDirectionHash, (int)transform.HitAngle(damage.sender.position));
            // trigger hitReaction animation
            if (damage.hitReaction)
            {
                // set the ID of the reaction based on the attack animation state of the attacker - Check the MeleeAttackBehaviour script
                if (reactionIDHash.isValid) animator.SetInteger(reactionIDHash, damage.reaction_id);
                if (triggerReactionHash.isValid) animator.SetTrigger(triggerReactionHash);
                if (triggerResetStateHash.isValid) animator.SetTrigger(triggerResetStateHash);
            }
            else
            {
                if (recoilIDHash.isValid) animator.SetInteger(recoilIDHash, damage.recoil_id);
                if (triggerRecoilHash.isValid) animator.SetTrigger(triggerRecoilHash);
                if (triggerResetStateHash.isValid) animator.SetTrigger(triggerResetStateHash);
            }
        }
        if (damage.activeRagdoll)
            onActiveRagdoll.Invoke();
    }
}
