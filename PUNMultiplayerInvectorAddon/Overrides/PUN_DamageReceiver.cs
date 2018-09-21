using Invector;
using Invector.vCharacterController;
using Invector.vEventSystems;
using Photon.Pun;
using UnityEngine;

public class PUN_DamageReceiver : vDamageReceiver
{
    public override void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
    {
        Debug.Log("RECEIVED DAMAGE");
        if (overrideReactionID)
            damage.reaction_id = reactionID;

        if (ragdoll && !ragdoll.iChar.isDead)
        {
            var _damage = new vDamage(damage);
            var value = (float)_damage.damageValue;
            _damage.damageValue = (int)(value * damageMultiplier);
            if (ragdoll.gameObject.transform.root.GetComponent<PhotonView>().IsMine == true)
            {
                ragdoll.gameObject.ApplyDamage(_damage, attacker);
            }
            else
            {
                healthController.gameObject.transform.root.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.All, JsonUtility.ToJson(_damage));
            }
            onReceiveDamage.Invoke(_damage);
        }
        else
        {
            if (healthController == null)
                healthController = GetComponentInParent<vIHealthController>();

            if (healthController != null)
            {
                var _damage = new vDamage(damage);
                var value = (float)_damage.damageValue;
                _damage.damageValue = (int)(value * damageMultiplier);
                try
                {
                    if (healthController.gameObject.transform.root.GetComponent<PhotonView>().IsMine == true)
                    {
                        healthController.gameObject.ApplyDamage(_damage, attacker);
                    }
                    else if (healthController.gameObject.transform.root.GetComponent<PhotonView>())
                    {
                        healthController.gameObject.transform.root.GetComponent<PhotonView>().RPC("ApplyDamage", RpcTarget.All, JsonUtility.ToJson(_damage));
                    }
                    onReceiveDamage.Invoke(_damage);
                }
                catch
                {
                    this.enabled = false;
                }
            }
        }
    }
}
