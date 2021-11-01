using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : Entity
{
    public int attackDelay = 360;
    public int attackVariation = 60;
    public int attackCooldown;
    public Vector2Int attackProjectilesMinMax;
    public Vector2Int hurtProjectilesMinMax;
    public Vector2Int deathProjectilesMinMax;
    public Vector2 projectileSpeedMinMax;
    public Vector2 projectileLifespanMinMax;
    public GameObject projectilePrefab;
    public AnimationCurve projectileSpeedOverLife;
    public int nextAttackCount;


    new void Start()
    {
        canTakeDamage = true;
        canDie = true;
        type = new EntityType[] { EntityType.Mushroom };
        base.Start();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        attackCooldown = Random.Range(0, attackDelay);
    }

    public override void Die()
    {
        nextAttackCount += Random.Range(deathProjectilesMinMax.x, deathProjectilesMinMax.y);
        isDead = true;
    }
    private void Attack(int numProjectiles, float projectileSpeedMult = 1)
    {
        for(int i = 0; i < numProjectiles; i++)
        {
            Projectile newProjectile = Instantiate(projectilePrefab, transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity).GetComponent<Projectile>();
            float angle = Random.Range(0, 2 * Mathf.PI / numProjectiles) + 2 * Mathf.PI * i / numProjectiles;
            newProjectile.direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            newProjectile.acceleration = Random.Range((float)projectileSpeedMinMax.x, projectileSpeedMinMax.y) * projectileSpeedMult;
            newProjectile.spriteRenderer = newProjectile.GetComponentInChildren<SpriteRenderer>();
            newProjectile.canTakeDamage = false;
            newProjectile.damage = new Damage(2, Damage.DamageType.Poison);
            newProjectile.hitMask = new List<EntityType>() { EntityType.Player };
            newProjectile.size = new Vector2(0.5f, 0.5f);
            newProjectile.speedOverLife = projectileSpeedOverLife;
            newProjectile.maxLifeSpan = Random.Range((float)projectileLifespanMinMax.x, projectileLifespanMinMax.y);
            newProjectile.lifeSpanLeft = newProjectile.maxLifeSpan;
        }
    }

    public override float TakeDamage(Damage damage)
    {
        nextAttackCount += Random.Range(hurtProjectilesMinMax.x, hurtProjectilesMinMax.y);
        attackCooldown = attackDelay + Random.Range(-attackVariation, attackVariation);
        
        base.TakeDamage(damage);
        return 0;
    }

    protected override void FixedUpdate()
    {
        UpdateSortingOrder();
        if (hitstop == 0)
        {
            if(nextAttackCount > 0)
            {
                if (isDead)
                {
                    Attack(nextAttackCount, 1.5f);
                    Destroy(gameObject);
                }
                else
                {
                    Attack(nextAttackCount);
                }
                nextAttackCount = 0;
            }
            if (isDead)
            {
                Destroy(gameObject);
            }

            if (attackCooldown <= 0)
            {
                Attack(Random.Range(attackProjectilesMinMax.x, attackProjectilesMinMax.y), 0.5f);
                attackCooldown = attackDelay + Random.Range(-attackVariation, attackVariation);
            }
            else
            {
                attackCooldown--;
            }
        }
        else if(hitstop > 0)
        {
            hitstop--;
        }
        else
        {
            hitstun--;

            if (nextAttackCount > 0)
            {
                if (isDead)
                {
                    Attack(nextAttackCount, 1.5f);
                    Destroy(gameObject);
                }
                else
                {
                    Attack(nextAttackCount);
                }
                nextAttackCount = 0;
            }
            if (isDead)
            {
                Destroy(gameObject);
            }
        }
    }
}
