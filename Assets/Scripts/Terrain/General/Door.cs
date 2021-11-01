using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Tile, IDoorInterface
{
    public Sprite openSprite;
    public Sprite closedSprite;
    public GameObject openParticles;
    public GameObject closeParticles;
    public bool isOpen;
    
    public bool Open()
    {
        if (isOpen) return false;

        type = TileType.floor;
        spriteRenderer.sprite = openSprite;
        if (openParticles)
        {
            Instantiate(openParticles, transform.position, Quaternion.identity);
        }
        isOpen = true;
        return true;
    }

    public bool Close()
    {
        if (!isOpen) return false;

        type = TileType.wall;
        spriteRenderer.sprite = closedSprite;
        if (closeParticles)
        {
            Instantiate(closeParticles, transform.position, Quaternion.identity);
        }
        isOpen = false;
        return true;
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Close();
            return;
        }

        Open();
        return;
    }
}
