using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Creature
{
    public bool useChargedAttack;

    public static PlayerController current;
    public RunType runType;

    public Sprite stand;
    public Sprite stun;
    public Sprite walk;
    public Sprite dead;
    private int walkAnimationCounter;
    public bool canMove = true;
    public Animator gameOverAnimator;
    public int currentTorchLevel;

    Vector2 halfScreenSize;
    Vector2 movementInput;
    Vector2 lookInput;
    int attackBuffer;
    bool isCharging;
    int chargeFrame;



    private void Awake()
    {
        current = this;
    }

    new protected void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        halfScreenSize = new Vector2(393, 256);
        type = new EntityType[] { EntityType.Player };
        canMove = true;
        size = new Vector2(0.5f, 0.5f);
        if (defense == null)
        {
            defense = new Defense(0, 0);
        }
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        health = maxHealth;
    }


    public override void Die()
    {
        if (isDead) return;

        GetComponent<AudioSource>().Play();
        canMove = false;
        gameOverAnimator.SetBool("dead", true);
        base.Die();
    }

    public override float TakeDamage(Damage damage)
    {
        return base.TakeDamage(damage);
    }

    protected override void FixedUpdate()
    {
        nearestInteractable = CheckNearestInteractable();
        if (tiles[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)].type == Tile.TileType.exit && !isDead)
        {
            Run.current.LevelUp(0);
        }
        UpdateSortingOrder();
        UpdateHealth();
        weaponObject.GetComponent<SpriteRenderer>().color = spriteRenderer.color;
        if (isCharging)
        {
            chargeFrame++;
            if(chargeFrame >= 12)
            {
                attackBuffer = 1;
            }
        }

        if (hitstun == 0 && hitstop == 0)
        {
            if (canMove)
            {
                if (attackBuffer > 0 && !isAttacking)
                {
                    attackBuffer = 0;
                    isAttacking = true;
                    weaponSwingFrame = 0;
                    if (chargeFrame >= 12)
                    {
                        curAttack = weapon.weaponData.chargedAttack;
                    }
                    else
                    {
                        curAttack = weapon.weaponData.normalAttack;
                    }
                    UpdateWeapon(lookInput - halfScreenSize, true);
                }
                else
                {
                    UpdateWeapon(lookInput - halfScreenSize);
                }

                //move
                Accelerate(movementInput * Time.fixedDeltaTime);

            }

            if (velocity.x < -0.2f)
            {
                spriteRenderer.flipX = true;
            }
            else if (velocity.x > 0.2f)
            {
                spriteRenderer.flipX = false;
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
                        
                        walkAnimationCounter = 0;
                    }
                    walkAnimationCounter++;

                    if (hasFootstepSounds)
                    {
                        if (footstepTimer >= 20)
                        {
                            footstepTimer = 0;
                            if (MapGen.current.GetTile(transform.position).GetFootstepSound())
                            {
                                audioSource.PlayOneShot(MapGen.current.GetTile(transform.position).GetFootstepSound(), 0.025f);
                            }
                        }
                        else
                        {
                            footstepTimer++;
                        }
                    }
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
            }

            Move();
        }
        else if (hitstop > 0)
        {
            hitstop--;
        }
        else
        {
            isAttacking = false;
            hitstun--;
            Move();
        }


        EmitLight(currentTorchLevel);
        if(attackBuffer > 0)
        {
            attackBuffer--;
        }
    }

    void OnAttack(InputValue value)
    {
        if (value.Get<float>() > 0.5f)
        {
            isCharging = true;
            chargeFrame = 0;
        }
        else
        {
            isCharging = false;
            attackBuffer = 12;
        }
    }

    void OnLook(InputValue lookValue)
    {
        lookInput = lookValue.Get<Vector2>();
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnInteract()
    {
        if (!canMove || isAttacking) return;

        if (nearestInteractable)
        {
            nearestInteractable.Interact(this);
        }
        else
        {
            if(inventory.Count > 1)
            {
                inventory.Insert(2, inventory[0]);
                inventory.RemoveAt(0);
            }
        }
    }

    void OnUse()
    {
        if (!canMove) return;
        UseItem(0);
    }
}
