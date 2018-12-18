using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vMelee;
using Photon.Pun;
using Invector.vItemManager;

public class PUN_MeleeManager : vMeleeManager
{
    public override void SetRightWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {
            base.SetRightWeapon(weaponObject);
            gameObject.GetComponent<PhotonView>().RPC("SetRightWeapon", RpcTarget.AllBuffered, weaponObject.name);
        }
    }
    public override void SetRightWeapon(vMeleeWeapon weapon)
    {
        if (weapon)
        {
            base.SetRightWeapon(weapon);
            gameObject.GetComponent<PhotonView>().RPC("SetRightWeapon", RpcTarget.AllBuffered, weapon.gameObject.name);
        }
    }

    public override void SetLeftWeapon(vMeleeWeapon weapon)
    {
        if (weapon)
        {
            base.SetLeftWeapon(weapon);
            gameObject.GetComponent<PhotonView>().RPC("SetLeftWeapon", RpcTarget.AllBuffered, weapon.gameObject.name);
        }
    }
    public override void SetLeftWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {
            base.SetLeftWeapon(weaponObject);
            gameObject.GetComponent<PhotonView>().RPC("SetLeftWeapon", RpcTarget.AllBuffered, weaponObject);
        }
    }
}
