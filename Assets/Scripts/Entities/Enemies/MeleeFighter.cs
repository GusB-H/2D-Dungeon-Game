using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeFighter : Creature
{
    public Sprite stand;
    public Sprite stun;
    public Sprite walk;
    public Sprite dead;
    private int walkAnimationCounter;
    public bool useChargedAttack;

    new private void Start()
    {
        base.Start();
        hostiles.Add(PlayerController.current.transform);
    }

    protected override void FixedUpdate()
    {
        if (hitstun <= 0 && hitstop == 0)
        {
            switch (state)
            {
                case CreatureState.Idle:
                    float curClosestDist = visionRange * visionRange;
                    foreach (Transform hostile in hostiles)
                    {
                        if (Vector3.SqrMagnitude(hostile.position - transform.position) < curClosestDist)
                        {
                            curClosestDist = Vector3.SqrMagnitude(hostile.position - transform.position);
                            target = hostile;
                            state = CreatureState.Chasing;
                        }
                    }
                    break;

                case CreatureState.Chasing:
                    curAcceleration += (Vector2)(target.position - transform.position).normalized * acceleration * Time.fixedDeltaTime;
                    if ((target.position - transform.position).sqrMagnitude <= maxStrafeDistance * maxStrafeDistance)
                    {
                        strafeDirection = Random.Range(0, 2);
                        curStrafeTimer = Random.Range(minStrafeDuration, maxStrafeDuration);
                        state = CreatureState.Strafing;
                    }

                    UpdateWeapon(new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y));

                    break;

                case CreatureState.Strafing:
                    Vector2 targetDir = (Vector2)(target.position - transform.position);
                    Vector2 strafeDir;
                    if (strafeDirection == 1)
                    {
                        strafeDir = new Vector2(targetDir.y, -targetDir.x);
                    }
                    else
                    {
                        strafeDir = new Vector2(-targetDir.y, targetDir.x);
                    }
                    float approachStrength = Mathf.InverseLerp(minStrafeDistance * minStrafeDistance, maxStrafeDistance * maxStrafeDistance, (target.position - transform.position).sqrMagnitude);
                    if (approachStrength >= 0.25f && approachStrength <= 0.75f)
                    {
                        
                    }
                    else
                    {
                        approachStrength = approachStrength * 2 - 1;
                    }
                    targetDir = (targetDir * approachStrength + (1 - Mathf.Abs(approachStrength)) * strafeDir).normalized;

                    curAcceleration += targetDir * acceleration * Time.fixedDeltaTime;
                    if ((target.position - transform.position).sqrMagnitude <= attackRange * attackRange)
                    {
                        state = CreatureState.Striking;
                    }
                    else
                    {
                        curStrafeTimer -= Time.fixedDeltaTime;
                        if (curStrafeTimer <= 0)
                        {
                            state = CreatureState.Striking;
                        }
                    }

                    UpdateWeapon(new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y));

                    break;

                case CreatureState.Striking:
                    curAcceleration += (Vector2)(target.position - transform.position).normalized * acceleration * Time.fixedDeltaTime;
                    if ((target.position - transform.position).sqrMagnitude <= attackRange * attackRange)
                    {
                        state = CreatureState.Attacking;
                    }


                    UpdateWeapon(new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y));

                    break;

                case CreatureState.Attacking:

                    if (!isAttacking)
                    {
                        if (curAttackCooldown > 0)
                        {
                            curAttackCooldown--;
                        }
                        else if (nextAttackDelay > 0)
                        {
                            if ((target.position - transform.position).sqrMagnitude > attackRange * attackRange)
                            {
                                strafeDirection = Random.Range(0, 2);
                                curStrafeTimer = Random.Range(minStrafeDuration, maxStrafeDuration);
                                state = CreatureState.Strafing;
                                break;
                            }
                            nextAttackDelay--;
                        }
                        else
                        {
                            UpdateWeapon(new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y));
                            isAttacking = true;
                            if (useChargedAttack)
                            {
                                curAttack = weapon.weaponData.chargedAttack;
                            }
                            else
                            {
                                curAttack = weapon.weaponData.normalAttack;
                            }
                            weaponSwingFrame = 0;
                            curAttackCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
                            nextAttackDelay = Random.Range(minAttackDelay, maxAttackDelay);
                        }
                    }


                    UpdateWeapon(new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y));

                    break;

                case CreatureState.Dead:
                    break;
            }


            if (!isDead)
            {
                if (velocity.sqrMagnitude > 1f)
                {
                    if (walkAnimationCounter < 10)
                    {
                        spriteRenderer.sprite = walk;
                    }
                    else
                    {
                        spriteRenderer.sprite = stand;
                    }
                    if (walkAnimationCounter == 20)
                    {
                        if (hasFootstepSounds)
                        {
                            audioSource.PlayOneShot(MapGen.current.GetTile(transform.position).GetFootstepSound());
                        }
                        walkAnimationCounter = 0;
                    }
                    walkAnimationCounter++;
                }
                else
                {
                    walkAnimationCounter = 0;
                    spriteRenderer.sprite = stand;
                }
            }
            else
            {
                spriteRenderer.sprite = dead;
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
            }


            Move();
        }
        else if (hitstop > 0)
        {
            hitstop--;
        }
        else
        {
            if (isDead)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                state = CreatureState.Chasing;
                swingRotationOffset = 0;
            }

            isAttacking = false;
            hitstun--;
            Move();
            velocity /= hitstunFriction;
        }
        weaponObject.GetComponent<SpriteRenderer>().color = spriteRenderer.color;
        base.FixedUpdate();
    }
}
