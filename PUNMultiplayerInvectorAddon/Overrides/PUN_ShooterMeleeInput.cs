using UnityEngine;
using Invector.vShooter;                //to access "vShooterMeleeInput"
using Invector;                        //to access "vDamage"
using Invector.vEventSystems;          //to access "vIMeleeFighter"
using Photon.Pun;

public class PUN_ShooterMeleeInput : vShooterMeleeInput
{
    protected override void Start()
    {
        //if (GetComponent<PhotonView>().IsMine == true)
        //{
        //    GameObject.FindObjectOfType<vControlAimCanvas>().GetComponent<vControlAimCanvas>().SetCharacterController(GetComponent<M_ThirdPersonController>());
        //}
        base.Start();
    }

    protected override void MeleeWeakAttackInput()
    {
        if (cc.animator == null) return;
        base.MeleeWeakAttackInput();
    }

    protected override void MeleeStrongAttackInput()
    {
        if (cc.animator == null) return;
        base.MeleeStrongAttackInput();
    }
    public override void OnEnableAttack()
    {
        if (transform.root.GetComponent<PhotonView>().IsMine == true)
        {
            base.OnEnableAttack();
        }
    }
    public override void OnDisableAttack()
    {
        if (transform.root.GetComponent<PhotonView>().IsMine == true)
        {
            base.OnDisableAttack();
        }
    }
    public override void ResetAttackTriggers()
    {
        if (GetComponent<PhotonView>().IsMine == false) return;
        GetComponent<PhotonView>().RPC("ResetTrigger", RpcTarget.All, "WeakAttack");
        GetComponent<PhotonView>().RPC("ResetTrigger", RpcTarget.All, "StrongAttack");
    }
    public override void OnRecoil(int recoilID)
    {
        cc.animator.SetInteger("RecoilID", recoilID);
        GetComponent<PhotonView>().RPC("SetTrigger", RpcTarget.All, "TriggerRecoil");
        GetComponent<PhotonView>().RPC("SetTrigger", RpcTarget.All, "ResetState");
        GetComponent<PhotonView>().RPC("ResetTrigger", RpcTarget.All, "WeakAttack");
        GetComponent<PhotonView>().RPC("ResetTrigger", RpcTarget.All, "StrongAttack");
    }
    public override void TriggerWeakAttack()
    {
        cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
        GetComponent<PhotonView>().RPC("SetTrigger", RpcTarget.All, "WeakAttack");
    }
    public override void TriggerStrongAttack()
    {
        cc.animator.SetInteger("AttackID", meleeManager.GetAttackID());
        GetComponent<PhotonView>().RPC("SetTrigger", RpcTarget.All, "StrongAttack");
    }
    
    public override void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
    {
        //character is blocking
        if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(damage.sender.position))
        {
            var damageReduction = meleeManager.GetDefenseRate();
            if (damageReduction > 0)
                damage.ReduceDamage(damageReduction);
            if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                attacker.BreakAttack(meleeManager.GetDefenseRecoilID());
            meleeManager.OnDefense();
            cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
            cc.currentStamina -= damage.staminaBlockCost;
        }
        //apply damage
        damage.hitReaction = !isBlocking;
        if (GetComponent<PhotonView>().IsMine == true)
        {
            cc.TakeDamage(damage);
        }
        else
        {   
            GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.Others, JsonUtility.ToJson(damage));
        }
    }

    protected override void UpdateAimHud()
    {
        if (cc == null)
        {
            cc = GetComponent<PUN_ThirdPersonController>();
        }
        if (GetComponent<PhotonView>().IsMine == false) return;
        base.UpdateAimHud();
    }
}
