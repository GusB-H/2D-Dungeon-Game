using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponData : ScriptableObject
{
    [System.Serializable]
    public class Attack
    {
        public int attackDuration;

        public AnimationCurve movementLockCurve;
        public AnimationCurve offsetCurveX, offsetCurveY, swingRotationCurve, weaponDistanceCurve;
        public SoundEffect[] soundEffects;
        public Hitbox[] hitboxes;
        public ProjectileAttack[] projectiles;
        public SwingParticle[] swingParticles;
        public SwingForce[] swingForces;
        public CameraShake[] cameraShakes;

    }


    [System.Serializable]
    public class Hitbox
    {
        public int startFrame;
        public int duration;
        public Vector2 offset;
        public bool parentToWeapon = true;
        public Vector3 dimensions;
        public Entity.Hit hit;
        public float knockback;
        public int selfHitstopOverride = -1;

        public SoundEffect hitSound;
        public CameraFeedback.Screenshake attackerShake;
        public CameraFeedback.Screenshake targetShake;
    }
    [System.Serializable]
    public class SoundEffect
    {
        public int startFrame;
        public AudioClip sound;
        public float volume = 1;
        public float minPitch = 1;
        public float maxPitch = 1;
    }
    [System.Serializable]
    public class SwingParticle
    {
        public int startFrame;
        public int duration;
        public Vector2 offset;
        public GameObject particleSystem;
    }
    [System.Serializable]
    public class SwingForce
    {
        public int startFrame;
        public int endFrame;
        public Vector2 totalForce;
    }

    [System.Serializable]
    public class CameraShake
    {
        public int startFrame;
        public CameraFeedback.Screenshake screenshake;
        public float strongRadius;
        public float fadeRadius;
    }

    [System.Serializable]
    public class ProjectileAttack
    {
        public int startFrame;
        public int quantity;
        public float distance;
        public float offset;
        public float randomOffset;
        public Projectile.ProjectileData projectileData;
    }

    public string displayName;
    public Sprite sprite;
    public enum DurabilityType { None, Ammo, DestroyOnBreak, ChangeWeapon };
    public enum AttackType { Normal, Charged, Dash, EmptyNormal, EmptyCharged, EmptyDash };
    public DurabilityType durabilityType;
    public int maxDurability;
    public WeaponData brokenWeapon;
    public Vector2 naturalOffset;
    public float naturalRotation;

    public Attack normalAttack;
    public Attack chargedAttack;
    public Attack dashAttack;

    public Attack emptyNormalAttack;    //versions of the attacks used when out of ammo (only used if DurabilityType is set to "Ammo")
    public Attack emptyChargedAttack;   
    public Attack emptyDashAttack;      

}
