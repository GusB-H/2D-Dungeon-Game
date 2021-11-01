using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFeedback : MonoBehaviour
{
    [System.Serializable]
    public class Screenshake
    {
        public float randomValue;
        public int windupFrames;
        public int duration;
        public int currentFrame;
        public Vector2 direction;
        public float magnitude;
        public float diminishedMagnitude;
        public bool alternate;

        public Vector2 Shake(bool iterateFrame = true)
        {
            Vector2 randVector = Vector2.zero;
            Vector2 dir = direction * (alternate?currentFrame % 2 * 2 - 1 : 1);

            if (randomValue > 0)
            {
                randVector = new Vector2(Random.value - 0.5f, Random.value - 0.5f).normalized;
            }

            if (iterateFrame)
            {
                currentFrame++;
            }
            
            dir = (dir.normalized * (1 - randomValue) + randVector * randomValue * Mathf.Pow(Random.value, 3)).normalized;

            if(currentFrame < 0)
            {
                dir *= Mathf.Lerp(magnitude, 0, -(float)currentFrame / windupFrames); //wind up. The interpolator starts at 1 here and goes to 0
            }
            else
            {
                dir *= Mathf.Lerp(magnitude, diminishedMagnitude, (float)currentFrame / duration);
            }

            return dir;
        }

        public Screenshake Copy()
        {
            return new Screenshake(this);
        }
        public Screenshake Copy(Vector2 overrideDirection)
        {
            Screenshake newShake = new Screenshake(this);
            newShake.direction = overrideDirection;
            return newShake;
        }
        Screenshake(Screenshake screenshake)
        {
            this.randomValue = screenshake.randomValue;
            this.windupFrames = screenshake.windupFrames;
            this.duration = screenshake.duration;
            this.currentFrame = -screenshake.windupFrames;
            this.direction = screenshake.direction;
            this.magnitude = screenshake.magnitude;
            this.diminishedMagnitude = screenshake.diminishedMagnitude;
            this.alternate = screenshake.alternate;
        }
        public Screenshake(int duration, Vector2 direction, float magnitude, bool fade = false)
        {
            this.randomValue = 0;
            this.duration = duration;
            this.currentFrame = 0;
            this.direction = direction;
            this.magnitude = magnitude;
            this.diminishedMagnitude = fade?0:magnitude;
            this.alternate = true;
            this.windupFrames = 0;
        }
        public Screenshake(float randomValue, int duration, Vector2 direction, float magnitude, float diminishedMagnitude, bool alternate = true, int windupFrames = 0)
        {
            this.randomValue = randomValue;
            this.duration = duration;
            this.currentFrame = -windupFrames;
            this.direction = direction;
            this.magnitude = magnitude;
            this.diminishedMagnitude = diminishedMagnitude;
            this.alternate = alternate;
            this.windupFrames = windupFrames;
        }
    }

    List<Screenshake> activeScreenshakes;

    private void Awake()
    {
        activeScreenshakes = new List<Screenshake>();
    }

    private void FixedUpdate()
    {
        Vector2 shake = Vector2.zero;

        for(int i = 0; i < activeScreenshakes.Count; i++)
        {
            Screenshake screenshake = activeScreenshakes[i];
            shake += screenshake.Shake();

            if(screenshake.currentFrame >= screenshake.duration)
            {
                activeScreenshakes.Remove(screenshake);
                i--;
            }
        }

        transform.localPosition = new Vector3(shake.x, shake.y, -10);
    }


    public void AddShake(Screenshake screenshake)
    {
        activeScreenshakes.Add(screenshake.Copy());
    }
    public void RandomShake(float magnitude, int frames, bool fade = false)
    {
        activeScreenshakes.Add(
            new Screenshake(1, frames, Vector2.zero, magnitude, fade?0:magnitude));
    }

    public void Jolt(float magnitude, Vector2 direction, int frames, int windupFrames = 0)
    {
        activeScreenshakes.Add(
            new Screenshake(0, frames, direction, magnitude, 0, false, windupFrames));
    }
}
