using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int xy;
    public enum TileType { wall, floor, edgeWall, nothing, outOfBounds, exit };
    public TileType type;
    public int roomIndex;
    public Tile[] adjacentTiles;
    public bool isExit;
    public int exitTo;
    
    [System.Serializable]
    public class TileData
    {
        public bool isSolid;
        public float opacity;

        //movement stuff
        public float friction;
        public float traction;

        //fire stuff
        public bool isFlammable;
        public float flammability;

        //liquid stuff
        public float liquidCapacity = 1;
    }

    public TileData tileData;

    //lighting stuff
    public bool lightingUpdateFlag = false;
    public bool isKnown;
    public bool isVisible;
    public int visibility; //how visible the tile is, from 0 to 100
    public int illumination; //how  well lit tile is
    public float drawBrightness; //how bright the tile is currently being drawn. Used by entities to calculate how bright they should be drawn based on their position
    public int illuminationBuffer; //how will lit the tile will be next frame
    public int ambientIllumination; //how well lit the tile is by default
    public int knownBrightness; //how bright the the tile should appear when known
    public int curBrightness; //how bright the tile currently is being displayed

    //rendering stuff
    public SpriteRenderer spriteRenderer;
    public Texture liquidTexture;
    Material material;

    //fire stuff
    public bool isBurning;
    public bool igniting;
    public float burnTimeLeft;

    //liquid stuff
    public float liquidLevel;
    public float liquidBuffer;
    public LiquidManager.Liquid currentLiquid;

    public enum SoundMaterial { Dirt, Stone };
    public SoundMaterial soundMaterial;

    public Tile()
    {
        tileData = new TileData();
    }

    public AudioClip GetFootstepSound()
    {
        if ((int)soundMaterial >= 0 && (int)soundMaterial < MapGen.current.tileSounds.tileTypes.Length)
        {
            return MapGen.current.tileSounds.tileTypes[(int)soundMaterial].GetSound();
        }
        else return null;
    }

    private void Awake()
    {
        if (tileData == null) tileData = new TileData();
        illumination = 0;
        knownBrightness = 20;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        material = spriteRenderer.material;
        material.SetFloat("_LiquidStartHeight", 1.5f);
    }

    private void Start()
    {
        xy = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        adjacentTiles = MapGen.current.AdjacentTiles(this, true);
        if (gameObject.layer == 9)
        {
            spriteRenderer.sortingLayerID = SortingLayer.NameToID("Ground");
        }
        spriteRenderer.sortingOrder = -Mathf.FloorToInt(transform.position.y);



        //UPDATE THIS LATER BY HAVING THE MAPGEN POPULATE TILEDATA
        tileData.traction = 1;
    }

    public float GetTraction()
    {
        if(currentLiquid == LiquidManager.Liquid.None)
        {
            return tileData.traction;
        }

        return Mathf.Lerp(tileData.traction, LiquidManager.current.GetLiquidStats(currentLiquid).traction, liquidLevel);
    }

    private void FixedUpdate()
    {

    }

    public void SetLightLevel(int lightLevel)
    {
        illuminationBuffer = Mathf.Max(illuminationBuffer, lightLevel);
    }

    public void UpdateColor()
    {
        if(type == TileType.outOfBounds)
        {
            return;
        }

        if (currentLiquid == LiquidManager.Liquid.Water)
        {
            liquidTexture = LiquidManager.current.waterTexture;
        }
        if (currentLiquid == LiquidManager.Liquid.Cobweb)
        {
            liquidTexture = LiquidManager.current.cobwebTexture;
        }

        material.SetTexture("_OverlayTex", liquidTexture);
        material.SetFloat("_LiquidLevel", Mathf.Clamp01(liquidLevel));
    }
}
