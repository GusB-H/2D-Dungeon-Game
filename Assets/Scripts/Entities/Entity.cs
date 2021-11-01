using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [System.Serializable]
    public class Hit
    {
        public Damage damage;
        public int hitstun;
        public int hitstop;
        public int invulFrames = 30;
        public bool ignoreInvul;
        public bool triggerCurse;
        public float speedMult = 1; //multiply the speed of the entity being hit. Use 0-1 to reduce the current momentum of creatures being hit

        public Hit(Damage damage, int hitstop = 0, int hitstun = 0, int invulFrames = 30, bool ignoreInvul = false, bool triggerCurse = false)
        {
            this.damage =damage;
            this.hitstun =hitstun;
            this.hitstop =hitstop;
            this.invulFrames = invulFrames;
            this.ignoreInvul = ignoreInvul;
            this.triggerCurse =triggerCurse;
        }
    }

    [System.Serializable]
    public class Injury
    {
        public enum InjuryType { None, Bleed, Poison, Burn, Curse, Corruption };
        public InjuryType type;
        public float amount;

        public Injury(InjuryType type, float amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
    [System.Serializable]
    public class Damage
    {
        public float damage;
        public DamageType[] damageTypes;
        public Injury[] injuries;

        public enum DamageType { Piercing, Slashing, Bludgeoning, Fire, Water, Earth, Air, Elecrtic, Poison, Nature, Frost, Shadow, Eldritch };

        public Damage(float damage, List<DamageType> damageTypes)
        {
            this.damage = damage;
            this.damageTypes = damageTypes.ToArray();
        }
        public Damage(float damage, DamageType[] damageTypes)
        {
            this.damage = damage;
            this.damageTypes = damageTypes;
        }
        public Damage(float damage, DamageType damageType)
        {
            this.damage = damage;
            this.damageTypes = new DamageType[] { damageType };
        }
        public Damage Copy()
        {
            return new Damage(damage, damageTypes);
        }
    }

    [System.Serializable]
    public class Defense
    {
        public enum Resistance { Immune, Resistant, Neutral, Vulnerable };

        public Resistance piercingResistance, slashingResistance, bludgeoningResistance, fireResistance, waterResistance, earthResistance, airResistance, elecrticResistance, poisonResistance, natureResistance, frostResistance, shadowResistance, eldritchResistance;

        private void Setup()
        {
            piercingResistance = Resistance.Neutral;
            slashingResistance = Resistance.Neutral;
            bludgeoningResistance = Resistance.Neutral;
            fireResistance = Resistance.Neutral;
            waterResistance = Resistance.Neutral;
            earthResistance = Resistance.Neutral;
            airResistance = Resistance.Neutral;
            elecrticResistance = Resistance.Neutral;
            poisonResistance = Resistance.Neutral;
            natureResistance = Resistance.Neutral;
            frostResistance = Resistance.Neutral;
            shadowResistance = Resistance.Neutral;
            eldritchResistance = Resistance.Neutral;
        }

        private float CalculateResistance(Resistance resistance, float damage)
        {
            switch (resistance)
            {
                case Resistance.Immune:
                    return 0;
                case Resistance.Resistant:
                    return damage * 0.5f;
                case Resistance.Neutral:
                    return damage;
                case Resistance.Vulnerable:
                    return damage * 2;
            }

            return 0;
        }

        public Resistance GetResistance(Damage.DamageType damageType)
        {
            switch (damageType)
            {
                case Damage.DamageType.Piercing:
                    return piercingResistance;

                case Damage.DamageType.Slashing:
                    return slashingResistance;

                case Damage.DamageType.Bludgeoning:
                    return bludgeoningResistance;

                case Damage.DamageType.Fire:
                    return fireResistance;

                case Damage.DamageType.Water:
                    return waterResistance;

                case Damage.DamageType.Earth:
                    return earthResistance;

                case Damage.DamageType.Air:
                    return airResistance;

                case Damage.DamageType.Elecrtic:
                    return elecrticResistance;

                case Damage.DamageType.Poison:
                    return poisonResistance;

                case Damage.DamageType.Nature:
                    return natureResistance;

                case Damage.DamageType.Frost:
                    return frostResistance;

                case Damage.DamageType.Shadow:
                    return shadowResistance;

                case Damage.DamageType.Eldritch:
                    return eldritchResistance;
            }

            return Resistance.Neutral;
        }

        public Damage CalculateDamage(Damage damage)
        {
            if (damage.damageTypes.Length == 0) return damage;
            foreach (Damage.DamageType damageType in damage.damageTypes)    //Being vulnerable to any type in the damage dealt means taking double damage.
            { 
                if(GetResistance(damageType) == Resistance.Vulnerable)
                {
                    return new Damage(damage.damage * 2, damage.damageTypes);
                }
            }

            foreach (Damage.DamageType damageType in damage.damageTypes)    //If you are immune to any type in the damage dealt, you take no damage.
            {                                                               //(unless you were vulnerable)
                if (GetResistance(damageType) == Resistance.Immune)
                {
                    return new Damage(0, damage.damageTypes);
                }
            }

            foreach (Damage.DamageType damageType in damage.damageTypes)    //Resistance has the lowest priority, and only takes effect if there are no
            {                                                               //vulnerabilities or immunities present
                if (GetResistance(damageType) == Resistance.Resistant)
                {
                    return new Damage(damage.damage * 0.5f, damage.damageTypes);
                }
            }

            return damage;
        }

        public Defense()
        {
            Setup();
        }

        [System.Serializable]
        public struct ResistanceOfType
        {
            public Resistance resistance;
            public Damage.DamageType damageType;

            public ResistanceOfType(Resistance resistance, Damage.DamageType damageType)
            {
                this.resistance = resistance;
                this.damageType = damageType;
            }
        }

        public Defense(ResistanceOfType resistanceOfType) : this(resistanceOfType.resistance, resistanceOfType.damageType)
        {

        }

        public Defense(Resistance resistance, Damage.DamageType damageType)
        {
            Setup();
            switch (damageType)
            {
                case Damage.DamageType.Piercing:
                    piercingResistance = resistance;
                    break;
                case Damage.DamageType.Slashing:
                    slashingResistance = resistance;
                    break;
                case Damage.DamageType.Bludgeoning:
                    bludgeoningResistance = resistance;
                    break;
                case Damage.DamageType.Fire:
                    fireResistance = resistance;
                    break;
                case Damage.DamageType.Water:
                    waterResistance = resistance;
                    break;
                case Damage.DamageType.Earth:
                    earthResistance = resistance;
                    break;
                case Damage.DamageType.Air:
                    airResistance = resistance;
                    break;
                case Damage.DamageType.Elecrtic:
                    elecrticResistance = resistance;
                    break;
                case Damage.DamageType.Poison:
                    poisonResistance = resistance;
                    break;
                case Damage.DamageType.Nature:
                    natureResistance = resistance;
                    break;
                case Damage.DamageType.Frost:
                    frostResistance = resistance;
                    break;
                case Damage.DamageType.Shadow:
                    shadowResistance = resistance;
                    break;
                case Damage.DamageType.Eldritch:
                    eldritchResistance = resistance;
                    break;
            }
        }


    }

    [System.Serializable]
    public class HealingListObject
    {
        public bool isRegeneration;
        public int healingLeft;
        public int framesPerTick;
        public int tickCountown;
        public Sprite sprite;

        public HealingListObject(int totalHealing, int framesPerTick, bool isRegeneration, Sprite sprite)
        {
            this.isRegeneration = isRegeneration;
            healingLeft = totalHealing;
            this.framesPerTick = framesPerTick;
            tickCountown = framesPerTick;
            this.sprite = sprite;
        }

        public HealingListObject Clone()
        {
            return new HealingListObject(healingLeft, framesPerTick, isRegeneration, sprite);
        }
    }

    public List<HealingListObject> healingList;

    public int inventoryMax;
    public List<Item> inventory;
    public Weapon weapon;
    public WeaponData weaponData;

    public enum EntityType { Hitbox, Player, Mushroom, Bandit, Prop, Animal };

    public bool canBeBled = true;
    public float bleedMultiplier = 1;
    public float bleedRate = 15;
    public int bleed;
    public int bleedCooldown;
    public bool canBePoisoned = true;
    public float poisonMultiplier = 1;
    public int poisonRate = 10;
    public float poison;
    public int poisonCountdown;
    public bool canBeBurned = true;
    public float burnMultiplier = 1;
    public float burn;
    public bool canBeCursed = true;
    public float curseMultiplier = 1;
    public int curse;
    public bool canBeCorrupted = true;
    public float corruptionMultiplier = 1;
    public float corruption;
    public static Tile[,] tiles;
    public Vector2 size;
    public EntityType[] type;
    public float health;
    public int maxHealth;
    public Defense defense;
    public float maxSpeed;
    public float acceleration;
    public int hitstun;
    public int hitstop;
    public bool hasInvulnerabilityFrames = true;
    public int invulnerabilityFrames;
    public float invulnerabilityAmount;
    public float friction = 1;
    public float hitstunFriction = 1;
    public bool canTakeDamage;
    public bool canDie;
    public bool isDead;
    public float attackSpeedPercent = 100;
    public Vector2 shuffleVelocity;
    public int shuffleFramesLeft;
    public int movementOverrideDuration;
    public Vector2 velocity;
    public Vector2 curAcceleration;
    public SpriteRenderer spriteRenderer;
    public bool isVisible;
    public float visibility;
    public int glowBrightness;
    public CameraFeedback myShakeCamera;
    public List<Interactable> nearbyInteractables;



    protected void Start()
    {
        if (weaponData)
        {
            weapon = new Weapon(weaponData);
        }

        if (defense == null)
        {
            defense = new Defense();
        }
        health = maxHealth;

        if (!spriteRenderer)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        healingList = new List<HealingListObject>();
        nearbyInteractables = new List<Interactable>();
    }

    public void Accelerate(Vector2 vector, bool useAccelerationMult = true, bool useTerrainModifiers = true)
    {
        float multiplier = 1;
        if (useAccelerationMult) multiplier *= acceleration;
        if (useTerrainModifiers) multiplier *= MapGen.current.GetTile(transform.position).GetTraction();

        curAcceleration += vector * multiplier;
    }

    protected virtual void FixedUpdate()
    {
        if (!isDead)
        {
            UpdateHealth();
        }
        UpdateSortingOrder();
    }

    public void AddToInventory(Item item)
    {
        if (inventoryMax < 1) return;

        if(inventory.Count < inventoryMax)
        {
            inventory.Insert(0, item);
            return;
        }
        else
        {
            inventory.Insert(0, item);
            Pickup.CreatePickup(inventory[1], transform.position);
            inventory.RemoveAt(1);
            return;
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        if (this.weapon.weaponData)
        {
            Pickup.CreatePickup(this.weapon, transform.position);
            this.weapon = weapon;
            this.weaponData = weapon.weaponData;
            SetWeaponSprite();
            return;
        }
        this.weapon = weapon;
        this.weaponData = weapon.weaponData;
        SetWeaponSprite();
        return;
    }

    public virtual void SetWeaponSprite()
    {

    }

    public int UseItem(int inventorySlot = 0)
    {

        if (inventory.Count > inventorySlot && inventory[inventorySlot] != null)
        {
            int useReturn = inventory[inventorySlot].OnUse(this);
            
            if (inventory[inventorySlot].useParticles)
            {
                Instantiate(inventory[inventorySlot].useParticles, transform.position, Quaternion.identity);
            }

            //print(useReturn);

            switch (useReturn)
            {
                case 0:
                    break; //The item was used, but not destroyed. Keep it where it is in the inventory.

                case 1:
                    inventory.RemoveAt(0); //The item was used up all the way. Since it's gone, remove the now empty reference from the inventory.
                    break;
            }

            return useReturn;
        }

        return -1; //Something went wrong...
    }

    public bool CheckTypeIntersect(EntityType[] typeToCheck, EntityType[] listToCheck)
    {
        foreach (EntityType entityType1 in listToCheck)
        {
            foreach (EntityType entityType2 in typeToCheck)
            {
                if (entityType1 == entityType2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckTypeIntersect(EntityType[] typeToCheck, List<EntityType> listToCheck)
    {
        foreach (EntityType entityType in typeToCheck)
        {
            if (listToCheck.Contains(entityType))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void SetHitstop(int value, bool cancelHigherValues = false)
    {
        if (cancelHigherValues)
        {
            hitstop = value;
        }
        else
        {
            hitstop = Mathf.Max(value, hitstop);
        }
    }

    public virtual void SetHitstun(int value, bool cancelHigherValues = false)
    {
        if (cancelHigherValues)
        {
            hitstun = value;
        }
        else
        {
            hitstun = Mathf.Max(value, hitstun);
        }
    }

    public virtual void TakeHit(Hit hit)
    {
        SetHitstun(hit.hitstun); //even if we have hit invulnerability, still take hitstun and hitstop to make multihits work and prevent immediate counterattacks
        SetHitstop(hit.hitstop); //things that don't take damage still get hitstop and hitstun; if this isn't wanted they can override this function
        velocity *= hit.speedMult;
        if (!canTakeDamage) return; 

        if (canBeCursed && curse > 0 && hit.triggerCurse)
        {
            TakeDamage(new Damage(curse, Damage.DamageType.Shadow));
            curse = 0;
        }

        if(hit.ignoreInvul || hit.damage.damage > invulnerabilityAmount)
        {
            foreach (Injury injury in hit.damage.injuries)
            {
                TakeInjury(injury);
            }
        }

        float incomingDamage = defense.CalculateDamage(hit.damage).damage;
        if (!hit.ignoreInvul && invulnerabilityFrames > 0)
        {
            if (incomingDamage > invulnerabilityAmount || hit.ignoreInvul)
            {
                TakeDamage(hit.damage, invulnerabilityAmount);

                if (hasInvulnerabilityFrames && hit.invulFrames > 0)
                {
                    invulnerabilityFrames = hit.invulFrames;
                    invulnerabilityAmount = incomingDamage;
                }
            }
        }
        else
        {
            TakeDamage(hit.damage);

            if (hasInvulnerabilityFrames && hit.invulFrames > 0)
            {
                invulnerabilityFrames = hit.invulFrames;
                invulnerabilityAmount = incomingDamage;
            }
        }
    }

    public virtual void TakeHit(Damage damage, int hitstun, int hitstop, bool triggerInvul = true, bool ignoreInvul = false)
    {
        SetHitstun(hitstun);
        SetHitstop(hitstop);
        if (!canTakeDamage) return;

        if (canBeCursed && curse > 0)
        {
            TakeDamage(new Damage(curse, Damage.DamageType.Shadow));
            curse = 0;
        }
        foreach(Injury injury in damage.injuries)
        {
            TakeInjury(injury);
        }

        float incomingDamage = defense.CalculateDamage(damage).damage;
        if (!ignoreInvul && invulnerabilityFrames > 0)
        {
            if (!ignoreInvul && invulnerabilityFrames > 0)
            {
                if (incomingDamage > invulnerabilityAmount || ignoreInvul)
                {
                    TakeDamage(damage, invulnerabilityAmount);

                    if (hasInvulnerabilityFrames && triggerInvul)
                    {
                        invulnerabilityFrames = 30;
                        invulnerabilityAmount = incomingDamage;
                    }
                }
            }
        }
        else
        {
            TakeDamage(damage);

            if (hasInvulnerabilityFrames && triggerInvul)
            {
                invulnerabilityFrames = 30;
                invulnerabilityAmount = incomingDamage;
            }
        }
    }

    public virtual float TakeDamage(Damage damage)
    {
        return TakeDamage(damage, 0);
    }

    public virtual float TakeDamage(Damage damage, float reduction)
    {
        if (canTakeDamage)
        {
            float damageTaken = defense.CalculateDamage(damage).damage;
            if (reduction > damageTaken) return 0;

            health -= damageTaken;

            if (health <= 0 && canDie && !isDead)
            {
                Die();
            }

            return damageTaken;
        }
        else
        {
            return 0;
        }
    }

    public virtual bool TakeInjury(Injury injury)
    {
        switch (injury.type)
        {
            case Injury.InjuryType.Bleed:
                if (bleed == 0) bleedCooldown = Mathf.FloorToInt(bleedRate * 60);
                if (canBeBled) bleed += Mathf.RoundToInt(injury.amount);
                else return false;
                break;

            case Injury.InjuryType.Poison:
                if(canBePoisoned) poison += injury.amount;
                else return false;
                break;

            case Injury.InjuryType.Burn:
                if(canBeBurned) burn += injury.amount;
                else return false;
                break;

            case Injury.InjuryType.Curse:
                if (canBeCursed) curse += Mathf.RoundToInt(injury.amount);
                else return false;
                break;

            case Injury.InjuryType.Corruption:
                if(canBeCorrupted) corruption += injury.amount;
                else return false;
                break;
        }

        return true;
    }

    public virtual void UpdateInjuries()
    {
        if(canBeBled && health > maxHealth - bleed)
        {
            bleedCooldown--;
            if(bleedCooldown <= 0)
            {
                bleedCooldown = Mathf.FloorToInt(bleedRate * 60);
                health--;
                if (health <= 0)
                {
                    Die();
                }
            }
        }

        if(canBePoisoned && poison > 0)
        {
            TakeDamage(new Damage(Time.fixedDeltaTime * 0.25f, Damage.DamageType.Poison)); //make these rates adjustable
        }

        
    }

    public virtual bool CalculateMapCollisions()
    {
        Vector2Int testPos = Vector2Int.RoundToInt((Vector2)transform.position - size * 0.5f);
        if(tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position + size * 0.5f);
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position - size * 0.5f * new Vector2(1, -1));
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position - size * 0.5f * new Vector2(-1, 1));
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }

        return false;
    }
    public virtual bool CalculateMapCollisions(Vector2 offset)
    {
        Vector2Int testPos = Vector2Int.RoundToInt((Vector2)transform.position + offset - size * 0.5f);
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position + offset + size * 0.5f);
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position + offset - size * 0.5f * new Vector2(1, -1));
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }
        testPos = Vector2Int.RoundToInt((Vector2)transform.position + offset - size * 0.5f * new Vector2(-1, 1));
        if (tiles[testPos.x, testPos.y].type == Tile.TileType.wall ||
           tiles[testPos.x, testPos.y].type == Tile.TileType.edgeWall)
        {
            return true;
        }

        return false;
    }

    public float[] GetTileWeights()
    {
        float xWeight = transform.position.x % 1;
        float yWeight = transform.position.y % 1;


        float[] returnValues = new float[4] {
        (1 - xWeight) * (1 - yWeight),  //  Bottom Left
        xWeight       * (1 - yWeight),  //  Bottom Right
        (1 - xWeight) * yWeight,        //  Top Left
        xWeight       * yWeight         //  Top Right
        };

        return returnValues;
    }
    public float[] GetTileDistances()
    {
        float xDist = transform.position.x % 1;
        float yDist = transform.position.y % 1;


        float[] returnValues = new float[4] {
        Mathf.Sqrt(     xDist  *      xDist  +      yDist  *      yDist ),
        //Bottom Left
        Mathf.Sqrt((1 - xDist) * (1 - xDist) +      yDist  *      yDist ),
        //Bottom Right
        Mathf.Sqrt(     xDist  *      xDist  + (1 - yDist) * (1 - yDist)), 
        //Top Left
        Mathf.Sqrt((1 - xDist) * (1 - xDist) + (1 - yDist) * (1 - yDist)) 
        //Top Right
        };

        return returnValues;
    }

    public void EmitLight()
    {
        float[] tileLights = GetTileDistances();
        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)].SetLightLevel(glowBrightness + Mathf.FloorToInt((1f - tileLights[0]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y)].SetLightLevel(glowBrightness + Mathf.FloorToInt((1f - tileLights[1]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y + 1)].SetLightLevel(glowBrightness + Mathf.FloorToInt((1f - tileLights[2]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y + 1)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y + 1)].SetLightLevel(glowBrightness + Mathf.FloorToInt((1f - tileLights[3]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y + 1)].lightingUpdateFlag = true;
    }
    public void EmitLight(int lightLevel)
    {
        float[] tileLights = GetTileDistances();
        /*print((Mathf.FloorToInt((1f - tileLights[0]) * 100)).ToString() + ", " 
            + (Mathf.FloorToInt((1f - tileLights[1]) * 100)) + ", " 
            + (Mathf.FloorToInt((1f - tileLights[2]) * 100)) + ", " 
            + (Mathf.FloorToInt((1f - tileLights[3]) * 100)));*/

        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)].SetLightLevel(lightLevel + Mathf.FloorToInt((1f - tileLights[0]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y)].SetLightLevel(lightLevel + Mathf.FloorToInt((1f - tileLights[1]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y + 1)].SetLightLevel(lightLevel + Mathf.FloorToInt((1f - tileLights[2]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y + 1)].lightingUpdateFlag = true;

        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y + 1)].SetLightLevel(lightLevel + Mathf.FloorToInt((1f - tileLights[3]) * 100));
        tiles[Mathf.FloorToInt(transform.position.x + 1), Mathf.FloorToInt(transform.position.y + 1)].lightingUpdateFlag = true;
    }

    public virtual void Die()
    {
        if (isDead)
        {
            return;
        }
        else
        {
            isDead = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Interactable>())
        {
            nearbyInteractables.Add(other.GetComponent<Interactable>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>())
        {
            nearbyInteractables.Remove(other.GetComponent<Interactable>());
        }
    }

    public Interactable CheckNearestInteractable()
    {
        Interactable nearestItem = null;
        float nearestDist = float.PositiveInfinity;
        for (int i = 0; i < nearbyInteractables.Count; i++)
        {
            Interactable item = nearbyInteractables[i];
            if (item == null)
            {
                i--;
                nearbyInteractables.Remove(item);
                continue;
            }

            
            if(Vector3.SqrMagnitude(item.transform.position - transform.position) < nearestDist)
            {
                nearestItem = item;
            }
        }
        return nearestItem;
    }

    public void CalculateEnvironmentEffects()
    {
        if (MapGen.current.GetTile(transform.position).isBurning)
        {
            TakeDamage(new Damage(1, Damage.DamageType.Fire));
        }
    }

    public void UpdateSortingOrder()
    {
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y) + 1;
    }

    public void UpdateIntangibilityFrames()
    {
        if (invulnerabilityFrames > 0)
        {
            invulnerabilityFrames--;
            if (invulnerabilityFrames == 0)
            {
                invulnerabilityAmount = 0;
            }
        }
    }

    public virtual void UpdateHealth()
    {
        UpdateIntangibilityFrames();
        UpdateInjuries();

        for(int i = 0; i < healingList.Count; i++)
        {
            HealingListObject listObject = healingList[i];


            if(listObject.tickCountown <= 1)
            {
                if ((listObject.isRegeneration && health < maxHealth - bleed) 
                   || health < maxHealth)
                {
                    health++;
                }

                if (!listObject.isRegeneration && bleed > 0)
                {
                    bleedCooldown = Mathf.FloorToInt(bleedRate * 60);
                }

                listObject.tickCountown = listObject.framesPerTick;
                listObject.healingLeft--;

                if (listObject.healingLeft <= 0)
                {
                    healingList.Remove(listObject); //If we remove the item, subtract from i so the index we check stays constant as to not skip over the next item.
                    i--;
                }
            }
            else
            {
                listObject.tickCountown--;
            }
        }
    }
}
