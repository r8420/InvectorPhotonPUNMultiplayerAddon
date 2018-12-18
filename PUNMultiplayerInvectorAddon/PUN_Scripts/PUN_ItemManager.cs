using Invector.vItemManager;
using Invector.vShooter;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PUN_ItemManager : MonoBehaviour {

    [SerializeField] private vItemListData allItems = null;
    public enum EquipSide { Left, Right }
    public enum WeaponType { shooter, melee }

    public GameObject createItem(string itemName, EquipSide side, GameObject calledFrom)
    {
        itemName = itemName.Replace("(Clone)", "").Trim();
        if (itemName[0] == 'v')
        {
            itemName = itemName.Remove(0, 1);
        }
        GameObject newitem = null;
        string temp = "";
        foreach (var item in allItems.items)
        {
            temp = item.name;
            if (temp[0] == 'v')
            {
                temp = temp.Remove(0, 1);
            }
            if (temp == itemName)
            {
                newitem = Instantiate(item.originalObject, this.transform.position, this.transform.rotation);
                switch (item.type)
                {
                    case vItemType.Shooter:
                        SetWeapon(newitem, calledFrom, side, WeaponType.shooter);
                        break;
                    case vItemType.MeleeWeapon:
                        SetWeapon(newitem, calledFrom, side, WeaponType.melee);
                        break;
                }
            }
        }

        return newitem;
    }

    public void DestroyWeapon(GameObject owner, string weaponName, EquipSide side)
    {
        List<Transform> handlers = GetHandlers(owner);
        Transform weapon = null;
        foreach (Transform handler in handlers)
        {
            weapon = FindWithName(handler, weaponName);
            if (weapon != null)
            {
                Destroy(weapon.gameObject);
            }
        }
    }

    void SetWeapon(GameObject weapon, GameObject owner, EquipSide side, WeaponType type)
    {
        //vShooterWeapon shooterWeapon = weapon.GetComponent<vShooterWeapon>();
        //PUN_ShooterManager manager = owner.GetComponent<PUN_ShooterManager>();
        Transform handler = GetHandler(owner.transform, side, type);
        weapon.transform.position = handler.position;
        weapon.transform.rotation = handler.rotation;
        weapon.transform.SetParent(handler);
    }

    List<Transform> GetHandlers(GameObject owner)
    {
        List<Transform> handlers = new List<Transform>();
        handlers.Add(GetHandler(owner.transform, EquipSide.Left, WeaponType.melee));
        handlers.Add(GetHandler(owner.transform, EquipSide.Right, WeaponType.melee));
        handlers.Add(GetHandler(owner.transform, EquipSide.Left, WeaponType.shooter));
        handlers.Add(GetHandler(owner.transform, EquipSide.Right, WeaponType.shooter));
        return handlers;
    }

    Transform GetHandler(Transform owner, EquipSide side, WeaponType type)
    {
        string foundHandler = "";
        string searchParent = "";
        switch (side)
        {
            case EquipSide.Left:
                searchParent = "LeftHandlers";
                break;
            case EquipSide.Right:
                searchParent = "RightHandlers";
                break;
        }
        switch (type)
        {
            case WeaponType.melee:
                foundHandler = "meleeHandler";
                break;
            case WeaponType.shooter:
                foundHandler = "defaultHandler";
                break;
        }

        Transform rootHandler = FindWithName(owner, searchParent);
        Transform handler = FindWithName(rootHandler, foundHandler);

        return handler;
    }

    Transform FindWithName(Transform root, string Name)
    {
        Transform retVal = null;
        if (root.name == Name)
        {
            retVal = root;
        }
        retVal = root.Find(Name);
        if (retVal == null)
        {
            foreach (Transform child in root)
            {
                retVal = FindWithName(child, Name);
                if (retVal != null)
                {
                    break;
                }
            }
        }

        return retVal;
    }
}
