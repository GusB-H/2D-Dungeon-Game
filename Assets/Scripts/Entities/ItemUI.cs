using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    public List<Item> playerInventory;
    public Sprite emptySprite;
    public Transform itemBoxPrimary;
    public Image itemDisplayPrimary;
    public Transform itemBoxSecondary;
    public Image itemDisplaySecondary;

    public Transform secondInventoryBoxPrimary;
    public Transform secondInventoryBoxSecondary; //use these for chars with two inventories

    private void Start()
    {
        playerInventory = PlayerController.current.inventory;
        itemDisplayPrimary = itemBoxPrimary.GetChild(0).GetComponent<Image>();
        itemDisplaySecondary = itemBoxSecondary.GetChild(0).GetComponent<Image>();
    }

    private void FixedUpdate()
    {
        itemDisplayPrimary.sprite = emptySprite;
        itemDisplaySecondary.sprite = emptySprite;
        if (playerInventory.Count > 0)
        {
            if (playerInventory[0] != null)
            {
                itemDisplayPrimary.sprite = playerInventory[0].sprite;
            }

            if(playerInventory.Count > 1)
            {
                if (playerInventory[1] != null)
                {
                    itemDisplaySecondary.sprite = playerInventory[1].sprite;
                }
            }
        }
    }


}
