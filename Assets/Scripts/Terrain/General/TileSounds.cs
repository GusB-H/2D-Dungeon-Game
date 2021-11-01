using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewTileSounds", menuName = "ScriptableObjects/TileSounds", order = 2)]

public class TileSounds : ScriptableObject
{
    [System.Serializable]
    public class TileType
    {
        public AudioClip[] sounds;

        public AudioClip GetSound()
        {
            return sounds[Random.Range(0, sounds.Length)];
        }
    }

    public TileType[] tileTypes; 
    //Each element corresponds to a Tile.SoundMaterial
    //0 - dirt
    //1 - stone
}
