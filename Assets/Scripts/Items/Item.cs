using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData itemData;
    public ItemData.ItemSize size;
    public string displayName;
    public string displayDesc;
    public Sprite sprite;
    public bool destroyOnUse;
    public GameObject useParticles;

    public Item(ItemData itemData)
    {
        this.itemData = itemData;
        size = itemData.size;
        displayName = itemData.displayName;
        displayDesc = itemData.displayDesc;
        sprite = itemData.sprite;
        destroyOnUse = itemData.destroyOnUse;
        useParticles = itemData.useParticles;
    }
    
    public int OnUse(Entity user)
    {
        if (itemData)
        {
            itemData.OnUse(user);
            if(destroyOnUse) return 1; //The Item was fully used up
            return 0; //The item was used, but not destroyed
        }

        Debug.LogError("Gus, you need to implement a generic OnUse function instead of relying on overriding ItemData.OnUse() because dropped/instantiated items don't always have an ItemData!");
        return -1; //???
    }
}
