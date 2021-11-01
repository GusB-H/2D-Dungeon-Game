using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : Interactable
{
    public static GameObject CreatePickup(ScriptableObject itemData, Vector3 position)
    {
        GameObject prefabPickup = Resources.Load("Pickup") as GameObject;
        GameObject newPickup = Instantiate(prefabPickup, position, Quaternion.identity);
        newPickup.GetComponent<Pickup>().itemData = itemData;
        newPickup.GetComponent<Pickup>().Setup();
        return newPickup;
    }
    public static GameObject CreatePickup(Item item, Vector3 position)
    {
        GameObject prefabPickup = Resources.Load("Pickup") as GameObject;
        GameObject newPickup = Instantiate(prefabPickup, position, Quaternion.identity);
        newPickup.GetComponent<Pickup>().item = item;
        newPickup.GetComponent<Pickup>().itemType = ItemType.Consumable;
        newPickup.GetComponent<Pickup>().Setup();
        return newPickup;
    }
    public static GameObject CreatePickup(Weapon weapon, Vector3 position)
    {
        GameObject prefabPickup = Resources.Load("Pickup") as GameObject;
        GameObject newPickup = Instantiate(prefabPickup, position, Quaternion.identity);
        newPickup.GetComponent<Pickup>().itemData = weapon.weaponData;
        newPickup.GetComponent<Pickup>().itemType = ItemType.Weapon;
        newPickup.GetComponent<Pickup>().Setup();
        return newPickup;
    }

    public ScriptableObject itemData; //The ScriptableObject for the itemData. Do not edit this, it will change the prefab
    public enum ItemType { Weapon, Consumable };
    public ItemType itemType;
    public Item item; //The data for this instance of the item
    public Weapon weapon;
    bool isSetUp = false;
    bool isPickedUp = false;

    void Setup()
    {
        if (isSetUp) return;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (item.displayName == "" && weapon.weaponData == null)
        {
            if (itemData.GetType() == typeof(ConsumableData))
            {
                item = new Item((ConsumableData)itemData);
                spriteRenderer.sprite = item.sprite;
            }
            else if (itemData.GetType() == typeof(WeaponData))
            {
                weapon = new Weapon((WeaponData)itemData);
                spriteRenderer.sprite = weapon.weaponData.sprite;
            }
            else
            {
                Debug.LogError("Attempted to create a pickup that wasn't an item. Please do not do this");
                gameObject.SetActive(false);
                return;
            }
        }
        else
        {
            if(item != null)
            {
                spriteRenderer.sprite = item.sprite;
            }
        }

        isSetUp = true;
    }


    protected override void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        Setup();
    }

    public override void Interact(Creature user)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        switch (itemType) 
        {
            case ItemType.Consumable:
                user.AddToInventory(item);
                break;

            case ItemType.Weapon:
                user.AddWeapon(weapon);
                break;
        }
        Destroy(gameObject);

    }
}
