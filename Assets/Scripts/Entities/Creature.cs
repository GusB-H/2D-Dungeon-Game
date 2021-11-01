using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Entity
{
    public enum CreatureState { Idle, Asleep, Exploring, Chasing, Strafing, Striking, Searching, Attacking, Fleeing, Dead };

    public float visionRange;
    public float attackRange;
    public float minStrafeDistance;
    public float maxStrafeDistance;
    public float minStrafeDuration;
    public float maxStrafeDuration;
    public float curStrafeTimer;
    protected int strafeDirection;
    public int minAttackCooldown;
    public int maxAttackCooldown;
    public int minAttackDelay;
    public int maxAttackDelay;
    public LayerMask attackLayermask;
    public List<EntityType> attackHitMask;
    public bool invertHitMask;

    public CreatureState state;
    public List<Transform> hostiles;
    public Transform target;
    public Vector2 lastSeenTargetPos;

    public WeaponData.Attack curAttack;
    public bool isAttacking;
    protected int curAttackCooldown;
    protected int nextAttackDelay;
    protected int curAttackFrame;
    public bool naturalWeapon;
    public GameObject weaponObject;
    public GameObject weaponPivot;
    public List<ParticleSystem> curSwingParticles;
    public int weaponSwingFrame;
    protected int swingMultiplier;
    protected float swingRotation;
    protected float swingRotationOffset;
    protected Vector2 weaponPivotOffset;

    public Interactable nearestInteractable;

    public bool hasFootstepSounds = true;
    protected int footstepTimer;
    public AudioSource audioSource;

    new protected void Start()
    {
        if (GetComponent<AudioSource>())
        {
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        base.Start();
        SetWeaponSprite();
        curSwingParticles = new List<ParticleSystem>();
    }

    public bool Attack(WeaponData.AttackType attackType)
    {
        if (isAttacking) return false;
        switch (attackType)
        {
            case WeaponData.AttackType.Normal:
                curAttack = weapon.weaponData.normalAttack;
                break;

            case WeaponData.AttackType.Charged:
                curAttack = weapon.weaponData.chargedAttack;
               break;

            case WeaponData.AttackType.Dash:
                curAttack = weapon.weaponData.dashAttack;
               break;

            case WeaponData.AttackType.EmptyNormal:
                curAttack = weapon.weaponData.emptyNormalAttack;
               break;

            case WeaponData.AttackType.EmptyCharged:
                curAttack = weapon.weaponData.emptyChargedAttack;
               break;

            case WeaponData.AttackType.EmptyDash:
                curAttack = weapon.weaponData.emptyDashAttack;
               break;
        }
        return true;
    }

    public bool Attack()
    {
        if (weapon.durability > 0)
        {
            return Attack(WeaponData.AttackType.Normal);
        }
        else
        {
            return Attack(WeaponData.AttackType.EmptyNormal);
        }
    }

    public override void SetWeaponSprite()
    {
        if (!weaponPivot)
        {
            weaponPivot = transform.GetChild(1).gameObject;
        }
        if (!weaponObject)
        {
            weaponObject = weaponPivot.transform.GetChild(0).gameObject;
        }
        weaponObject.GetComponent<SpriteRenderer>().sprite = weaponData.sprite;
        base.SetWeaponSprite();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Die()
    {
        foreach(ParticleSystem particles in curSwingParticles)
        {
            Destroy(particles);
        }
        state = CreatureState.Dead;
        canTakeDamage = false;
        base.Die();
    }

    public override void SetHitstun(int value, bool cancelHigherValues = false)
    {
        if(value > 0)
        {
            foreach (ParticleSystem particles in curSwingParticles)
            {
                Destroy(particles);
            }
        }
        base.SetHitstun(value, cancelHigherValues);
    }

    public virtual void UpdateWeapon(Vector2 targetVector, bool overrideDirection = false)
    {
        weaponObject.GetComponent<SpriteRenderer>().sortingOrder = -Mathf.RoundToInt(weaponObject.transform.position.y - 0.25f) + 1;
        if (isAttacking)
        {
            if (overrideDirection)
            {
                if (targetVector.x >= 0)
                {
                    swingMultiplier = 1;
                    weaponObject.GetComponent<SpriteRenderer>().flipX = false;
                }
                else
                {
                    swingMultiplier = -1;
                    weaponObject.GetComponent<SpriteRenderer>().flipX = true;
                }
                swingRotation = Mathf.Rad2Deg * Mathf.Atan(targetVector.y / targetVector.x);
            }
            UpdateAttack();
        }
        else
        {
            if (targetVector.x >= 0)
            {
                swingMultiplier = 1;
                weaponObject.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                swingMultiplier = -1;
                weaponObject.GetComponent<SpriteRenderer>().flipX = true;
            }
            swingRotation = Mathf.Rad2Deg * Mathf.Atan(targetVector.y / targetVector.x) + weaponData.naturalRotation;
            if (weaponData.naturalOffset != Vector2.zero) 
            {
                targetVector.Normalize();
            }
            weaponPivotOffset = weaponData.naturalOffset.x * targetVector + weaponData.naturalOffset.y * new Vector2(targetVector.y, -targetVector.x);
        }

        weaponPivot.transform.localPosition = weaponPivotOffset;
        weaponObject.transform.rotation = Quaternion.Euler(0, 0, swingMultiplier * swingRotationOffset + swingRotation);

        for (int i = 0; i < curSwingParticles.Count; i++)
        {
            if (!curSwingParticles[i])
            {
                curSwingParticles.RemoveAt(i);
                i--;
            }
            else
            {
                curSwingParticles[i].Simulate(Time.fixedDeltaTime, false, false);
                if(curSwingParticles[i].particleCount == 0)
                {
                    Destroy(curSwingParticles[i].gameObject);
                    curSwingParticles.RemoveAt(i);
                    i--;
                }
            }
        }

    }

    public virtual void Move()
    {
        Vector2 thisFrameAcceleration;

        if (isAttacking)
        {
            thisFrameAcceleration = curAcceleration * curAttack.movementLockCurve.Evaluate((float)(weaponSwingFrame * (attackSpeedPercent / 100)) / curAttack.attackDuration);
        }
        else
        {
            thisFrameAcceleration = curAcceleration;
        }

        velocity += thisFrameAcceleration;
        Vector2 moveDir = velocity;

        if (CalculateMapCollisions(moveDir * Time.fixedDeltaTime * Vector2.up))
        {
            velocity *= Vector2.right;
        }
        else
        {
            transform.position += (Vector3)(moveDir * Vector2.up) * Time.fixedDeltaTime;
        }

        if (CalculateMapCollisions(moveDir * Time.fixedDeltaTime * Vector2.right))
        {
            velocity *= Vector2.up;
        }
        else
        {
            transform.position += (Vector3)(moveDir * Vector2.right) * Time.fixedDeltaTime;
        }
        if (hitstun > 0)
        {
            velocity /= hitstunFriction;
        }
        else
        {
            velocity /= friction;
        }

        curAcceleration = Vector2.zero;
    }

    public virtual void UpdateAttack()
    {
        Vector2 swingDirection = new Vector2(Mathf.Cos(swingRotation * Mathf.Deg2Rad) * swingMultiplier, Mathf.Sin(swingRotation * Mathf.Deg2Rad) * swingMultiplier); //Get the exact direction the weapon was swung in. Cannot use the weapon's current orientation because it will rotate throughout a swing.
        Vector2 perpendicularSwingDirection = new Vector2(swingDirection.y, -swingDirection.x) * swingMultiplier;

        if (curAttack.hitboxes != null)
        {
            foreach (WeaponData.Hitbox hitbox in curAttack.hitboxes)
            {
                if (weaponSwingFrame * (attackSpeedPercent / 100) >= hitbox.startFrame
                    && (weaponSwingFrame - 1) * (attackSpeedPercent / 100) < hitbox.startFrame) //check if the hitbox should start this frame
                {
                    AttackSwing attackSwing = Instantiate(Resources.Load("SwingPrefab") as GameObject, weaponObject.transform).GetComponent<AttackSwing>(); //Instantiate the hitbox
                    attackSwing.swinger = transform;
                    attackSwing.targetMask = attackLayermask;
                    attackSwing.hitMask = attackHitMask;
                    attackSwing.invertHitMask = invertHitMask; //Populate the new hitbox gameobject with its hitmask

                    attackSwing.transform.localPosition = hitbox.offset * swingMultiplier; //Because the hitbox is a child of our weapon, we set its with localPosition

                    attackSwing.framesLeft = Mathf.CeilToInt(hitbox.duration * (100 / attackSpeedPercent)); //Calculate how many frames the hitbox should last based on our attack speed
                    attackSwing.swingDirection = swingDirection;
                    attackSwing.hitbox = hitbox;
                }
            }
        }

        if (curAttack.projectiles != null)
        {
            foreach (WeaponData.ProjectileAttack projectileAttack in curAttack.projectiles)
            {
                if (weaponSwingFrame * (attackSpeedPercent / 100) >= projectileAttack.startFrame
                    && (weaponSwingFrame - 1) * (attackSpeedPercent / 100) < projectileAttack.startFrame) //check if the hitbox should start this frame
                {
                    Projectile projectile = Instantiate(Resources.Load("Projectile") as GameObject, transform.position + projectileAttack.distance * (Vector3)swingDirection, transform.rotation).GetComponent<Projectile>();
                    projectile.hitMask = attackHitMask;
                    projectile.invertHitMask = invertHitMask;
                    projectile.direction = swingDirection;

                    projectile.Setup(projectileAttack.projectileData);
                }
            }
        }

        if(curAttack.soundEffects != null)
        foreach (WeaponData.SoundEffect soundEffect in curAttack.soundEffects)
        {
            if (weaponSwingFrame * (attackSpeedPercent / 100) >= soundEffect.startFrame
                && (weaponSwingFrame - 1) * (attackSpeedPercent / 100) < soundEffect.startFrame) //check if the hitbox should start this frame
            {
                weaponObject.GetComponent<AudioSource>().pitch = Random.Range(soundEffect.minPitch, soundEffect.maxPitch);
                weaponObject.GetComponent<AudioSource>().PlayOneShot(soundEffect.sound, soundEffect.volume);
            }   
        }

        if (curAttack.swingParticles != null)
        {
            foreach (WeaponData.SwingParticle swingParticle in curAttack.swingParticles)
            {
                if (weaponSwingFrame * (attackSpeedPercent / 100) >= swingParticle.startFrame
                && (weaponSwingFrame - 1) * (attackSpeedPercent / 100) < swingParticle.startFrame)
                {
                    ParticleSystem attackParticle = Instantiate(swingParticle.particleSystem, weaponObject.transform).GetComponent<ParticleSystem>();

                    curSwingParticles.Add(attackParticle);
                    attackParticle.transform.localPosition = swingParticle.offset * swingMultiplier; //Similar to the hitboxes, particles are children of our weapon

                    attackParticle.Pause(); //Pause the particle system to update its values. Do this because Unity doesn't like updating the duration of an active particle system
                    var m = attackParticle.main;
                    m.duration = swingParticle.duration * Time.fixedDeltaTime * (100 / attackSpeedPercent); //Increase or decrease the particle system's duration to match how long our attack will take
                                                                                                            //attackParticle.Play(); //Resume partile system.
                }
            }
        }

        if (curAttack.swingForces != null)
        {
            foreach (WeaponData.SwingForce swingForce in curAttack.swingForces)
            {
                if (weaponSwingFrame * (attackSpeedPercent / 100) >= swingForce.startFrame
                && weaponSwingFrame * (attackSpeedPercent / 100) <= swingForce.endFrame)
                {
                    float forceFrameMultiplier = (attackSpeedPercent / 100) / ((swingForce.endFrame - swingForce.startFrame)); //forces get weaker for slower attacks

                    velocity += swingDirection * swingForce.totalForce.x * forceFrameMultiplier;
                    velocity += new Vector2(swingDirection.y, -swingDirection.x) * swingForce.totalForce.y * forceFrameMultiplier;
                }
            }
        }

        if (myShakeCamera)
        {
            if (curAttack.cameraShakes != null)
            {
                foreach (WeaponData.CameraShake cameraShake in curAttack.cameraShakes)
                {
                    if (weaponSwingFrame * (attackSpeedPercent / 100) >= cameraShake.startFrame
                    && (weaponSwingFrame - 1) * (attackSpeedPercent / 100) < cameraShake.startFrame)
                    {
                        myShakeCamera.AddShake(cameraShake.screenshake.Copy(-swingDirection));
                    }
                }
            }
        }



        if (weaponSwingFrame * (attackSpeedPercent / 100) >= curAttack.attackDuration)
        {
            isAttacking = false;
            foreach(ParticleSystem particles in curSwingParticles)
            {
                Destroy(particles);
            }
        }
        else
        {
            swingRotationOffset = curAttack.swingRotationCurve.Evaluate((float)(weaponSwingFrame * (attackSpeedPercent / 100)) / curAttack.attackDuration);
            weaponPivotOffset = swingDirection * curAttack.offsetCurveX.Evaluate((float)(weaponSwingFrame * (attackSpeedPercent / 100)) / curAttack.attackDuration)
                + perpendicularSwingDirection * curAttack.offsetCurveY.Evaluate((float)(weaponSwingFrame * (attackSpeedPercent / 100)) / curAttack.attackDuration);
            //Update the swing's position. This offset is added to the weapon object's 'true' rotation, to make it swing relative to the direction it's swinging in
            weaponSwingFrame++;
        }
    }


    public override float TakeDamage(Damage damage)
    {
        return base.TakeDamage(damage);
    }
}
