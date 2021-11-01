using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewConsumable", menuName = "ScriptableObjects/ConsumableItem", order = 3)]

public class ConsumableData : ItemData
{
    public class Healing
    {
        public int health;
        public int frequency;
        public int numberHeals;
    }

    public Entity.HealingListObject healing;

    public override void OnUse(Entity user)
    {
        base.OnUse(user);

        user.healingList.Add(healing.Clone());
    }
}
