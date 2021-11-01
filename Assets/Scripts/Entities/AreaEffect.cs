using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AreaEffect : MonoBehaviour
{
    public Vector2 range;
    public Vector2 offset;
    public int tickDelay;
    int timer;
    public Entity.Hit hit;
    public bool damageOnEntrance;
    List<Entity> currentOccupants;
    List<int> occupantTimers;

    public GameObject hitEffect;



    private void Start()
    {
        currentOccupants = new List<Entity>();
        occupantTimers = new List<int>();
    }

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + (Vector3)offset, new Vector3(range.x * 0.5f, range.y * 0.5f, 1));
        for (int i = 0; i < currentOccupants.Count; i++)
        {
            if(currentOccupants[i] == null)
            {
                currentOccupants.RemoveAt(i);
                occupantTimers.RemoveAt(i);
                i--;
                continue;
            }

            if(occupantTimers[i] >= tickDelay)
            {
                if(colliders.Contains(currentOccupants[i].GetComponent<Collider>()) && !currentOccupants[i].isDead)
                {
                    if (hitEffect)
                    {
                        Instantiate(hitEffect, currentOccupants[i].transform.position, Quaternion.identity);
                    }
                    currentOccupants[i].TakeHit(hit);
                    occupantTimers[i] = 0;
                }
                else
                {
                    currentOccupants.RemoveAt(i);
                    occupantTimers.RemoveAt(i);
                    i--;
                }
            }
            else
            {
                occupantTimers[i]++;
            }


        }

        foreach(Collider collider in colliders)
        {
            if (collider.transform.root == transform.root) continue;

            if (collider.GetComponent<Entity>() && !currentOccupants.Contains(collider.GetComponent<Entity>()) && !collider.GetComponent<Entity>().isDead)
            {
                if (damageOnEntrance)
                {
                    if (hitEffect)
                    {
                        Instantiate(hitEffect, collider.transform.position, Quaternion.identity);
                    }
                    collider.GetComponent<Entity>().TakeHit(hit);
                }
                currentOccupants.Add(collider.GetComponent<Entity>());
                occupantTimers.Add(0);
            }
        }
    }
}
