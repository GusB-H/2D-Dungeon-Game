using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    [System.Serializable]
    public class ProjectileData
    {
        public Sprite sprite;
        public Damage damage;
        public AnimationCurve speedOverLife;
        public float lifeSpan, randomLifeSpanBonus;
        public int maxPierces;
        public Vector2 size;
    }

    public Sprite sprite;
    public int maxPierces;
    public List<EntityType> hitMask;
    public bool invertHitMask;
    public Damage damage;
    public Vector2 direction;
    public float maxLifeSpan;
    public float lifeSpanLeft;
    public AnimationCurve speedOverLife;
    // Start is called before the first frame update


    public void Setup(ProjectileData projectileData)
    {
        sprite = projectileData.sprite;
        damage = projectileData.damage;
        speedOverLife = projectileData.speedOverLife;
        maxLifeSpan = projectileData.lifeSpan + Random.value * projectileData.randomLifeSpanBonus;
        maxPierces = projectileData.maxPierces;
        size = projectileData.size;
    }

    new void Start()
    {
        GetComponent<BoxCollider>().size = size;
        lifeSpanLeft = maxLifeSpan;
        type = new EntityType[] { EntityType.Hitbox };
        base.Start();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider collider = collision.collider;
        if ((collider.gameObject.layer == 8 || collider.gameObject.layer == 11) && collider.GetComponent<Entity>() && !collider.GetComponent<Entity>().isDead)
        {
            if (CheckTypeIntersect(collider.GetComponent<Entity>().type, hitMask) == !invertHitMask)
            {
                print(collider.GetComponent<Entity>().TakeDamage(damage));
                if (maxPierces == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    maxPierces--;
                }
            }
        }
        else if (collider.gameObject.layer == 10)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        lifeSpanLeft -= Time.fixedDeltaTime;
        if(lifeSpanLeft <= 0 || MapGen.current.tiles[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)].gameObject.layer == 10)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)direction * speedOverLife.Evaluate(1 - lifeSpanLeft / maxLifeSpan) * Time.fixedDeltaTime;
        UpdateSortingOrder();
    }
}
