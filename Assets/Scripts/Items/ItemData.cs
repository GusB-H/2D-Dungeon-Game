using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public enum ItemSize { Tiny, Small, Medium, Large };
    public ItemSize size;
    public string displayName;
    public string displayDesc;
    public Sprite sprite;
    public bool destroyOnUse;
    public GameObject useParticles;


    public virtual void OnUse(Entity user)
    {

    }

    public virtual void OnPickup(Entity user)
    {

    }

    public virtual void OnDrop(Entity user)
    {

    }
}
