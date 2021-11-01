using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public WeaponData weaponData;
    public int durability;

    public Weapon(WeaponData weaponData)
    {
        this.weaponData = weaponData;
        this.durability = weaponData.maxDurability;
    }
}
