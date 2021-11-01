using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSwing : Entity
{
    public List<EntityType> hitMask;
    public bool invertHitMask;
    public LayerMask targetMask;
    public Transform swinger;
    public List<Transform> hitTransforms;
    public Vector3 hitboxDimensions;
    public int framesLeft;
    int startFrames;
    public Vector2 swingDirection;
    public WeaponData.Hitbox hitbox;
    public List<ParticleSystem> particleSystems;
    public AudioSource audioSource;

    // Start is called before the first frame update
    new void Start()
    {
        if (hitMask == null)
        {
            hitMask = new List<EntityType>();
            invertHitMask = true;
        }
        startFrames = framesLeft;
        hitboxDimensions = hitbox.dimensions;
        //gameObject.AddComponent<BoxCollider>().size = hitboxDimensions; //debug for checking hitbox size. ToDo: make this part of the thing, and use Physics.SweepTest instead of OverlapBox to prevent fast swings from missing targets

        //GetComponent<BoxCollider>().size = hitboxDimensions;

        hitTransforms = new List<Transform>();
        hitTransforms.Add(transform); //Just in case we want the hitbox to collide with hitboxes, this stops it from detecting itself
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        if (!swinger.GetComponent<Creature>().isAttacking)
        {
            Destroy(gameObject);
        }


        foreach (Collider collider in Physics.OverlapBox((Vector2)transform.position, hitboxDimensions, transform.rotation, targetMask))
        {
            if (collider.GetComponent<Entity>() && collider.transform != swinger && (CheckTypeIntersect(collider.GetComponent<Entity>().type, hitMask) != invertHitMask) && !hitTransforms.Contains(collider.transform) && collider.GetComponent<Entity>().canTakeDamage)
            {
                collider.GetComponent<Entity>().TakeHit(hitbox.hit);
                collider.GetComponent<Entity>().velocity += swingDirection * hitbox.knockback;

                if (hitbox.selfHitstopOverride < 0)
                {
                    swinger.GetComponent<Entity>().SetHitstop(hitbox.hit.hitstop);
                }
                else
                {
                    swinger.GetComponent<Entity>().SetHitstop(hitbox.selfHitstopOverride);
                }

                if (hitbox.hitSound.sound)
                {
                    if (!audioSource.isPlaying) audioSource.pitch = Random.Range(hitbox.hitSound.minPitch, hitbox.hitSound.maxPitch);
                    audioSource.PlayOneShot(hitbox.hitSound.sound, hitbox.hitSound.volume);
                }

                if (swinger.GetComponent<Entity>().myShakeCamera)
                {
                    swinger.GetComponent<Entity>().myShakeCamera.AddShake(hitbox.attackerShake.Copy(swingDirection));
                }
                if (collider.GetComponent<Entity>().myShakeCamera)
                {
                    collider.GetComponent<Entity>().myShakeCamera.AddShake(hitbox.targetShake.Copy(-swingDirection));
                }
                

                hitTransforms.Add(collider.transform);
            }
        }
        if(framesLeft <= 0)
        {
            Destroy(gameObject);
        }
        if (swinger.GetComponent<Entity>().hitstop == 0)
        {
            framesLeft--;
        }
        else
        {

        }
    }
}
