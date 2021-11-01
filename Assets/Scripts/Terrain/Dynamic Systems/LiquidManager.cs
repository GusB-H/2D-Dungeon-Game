using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidManager : MonoBehaviour
{
    public static LiquidManager current;

    public Texture waterTexture;
    public Texture cobwebTexture;
    [System.Serializable]
    public class LiquidStats
    {
        public Color color;
        public float viscosity;
        public float friction;
        public float traction;
    }
    public enum Liquid { None, Water, Cobweb };

    public LiquidStats waterStats, cobwebStats;
    public MapGen mapGen;
    public Tile[,] tiles;


    public LiquidStats GetLiquidStats(Liquid liquid)
    {
        switch (liquid)
        {
            case Liquid.Water:  return waterStats;
            case Liquid.Cobweb: return cobwebStats;
        }

        return null;
    }

    void Start()
    {
        mapGen = MapGen.current;
        tiles = mapGen.tiles;
        current = this;
    }

    public virtual void CalculateFlow(Tile tile)
    {
        if(tile.liquidLevel <= tile.tileData.liquidCapacity)
        {
            return;
        }
        tile.liquidBuffer -= (tile.liquidLevel - tile.tileData.liquidCapacity) * 0.2f;
        float liquidSpread = ((tile.liquidLevel - tile.tileData.liquidCapacity) / tile.adjacentTiles.Length) * 0.2f;


        foreach (Tile targetTile in tile.adjacentTiles)
        {
            if(targetTile.currentLiquid == tile.currentLiquid || targetTile.currentLiquid == Liquid.None)
            {
                targetTile.currentLiquid = tile.currentLiquid;
                targetTile.liquidBuffer += liquidSpread;

            }
        }


        
    }

    public void Flow(Tile tile)
    {
        tile.liquidLevel += tile.liquidBuffer;
        tile.liquidBuffer = 0;
    }

    public void Interact(Tile tile, Tile targetTile, float quantity)
    {

    }

    public void CheckInteraction(Liquid liquid, Liquid target)
    {

    }

    void FixedUpdate()
    {
        foreach (Tile tile in tiles)
        {
            if(tile.currentLiquid != Liquid.None)
            {
                CalculateFlow(tile);
            }
        }


        foreach (Tile tile in tiles)
        {
            if (tile.currentLiquid != Liquid.None)
            {
                Flow(tile);
            }
        }
    }
}
