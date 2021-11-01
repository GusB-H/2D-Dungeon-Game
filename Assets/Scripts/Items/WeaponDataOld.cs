using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewWeaponOld", menuName = "ScriptableObjects/Weapon", order = 4)]
public class WeaponDataOld : ScriptableObject
{
    [System.Serializable]
    public class Hitbox
    {
        public int startFrame;
        public int duration;
        public Vector2 offset;
        public Vector3 dimensions;
        public Entity.Damage damage;
        public float knockback;
        public int hitstun;
        public int hitstop;

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

    public string displayName;
    public Sprite sprite;
    public enum DurabilityType { None, Ammo, DestroyOnBreak, ChangeWeapon };
    public DurabilityType durabilityType;
    public int maxDurability;
    public int attackDuration;
    public SoundEffect[] soundEffects;
    public Hitbox[] hitboxes;
    public SwingParticle[] swingParticles;
    public SwingForce[] swingForces;
    public CameraShake[] cameraShakes;
    public AnimationCurve swingCurve;
    public AnimationCurve movementLockCurve;
}
