using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        if (!spriteRenderer)
        {
            if (GetComponent<SpriteRenderer>())
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            else if (GetComponentInChildren<SpriteRenderer>())
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        UpdateSortingOrder();
    }

    public void UpdateSortingOrder()
    {
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y) + 1;
    }

    public virtual void Interact(Creature user)
    {

    }
}
