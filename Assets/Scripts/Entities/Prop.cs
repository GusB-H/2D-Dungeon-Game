using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : Entity
{
    public LootTable lootTable;
    public int drops;
    public override void Die()
    {
        base.Die();
        isDead = true;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(hitstop > 0)
        {
            hitstop--;
        }
        else if (isDead)
        {
            if (drops > 0)
            {
                foreach (GameObject gameObject in lootTable.PullDrops(drops))
                {
                    Instantiate(gameObject, transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0), Quaternion.identity);
                }
            }
            Destroy(gameObject);
        }
    }
}
