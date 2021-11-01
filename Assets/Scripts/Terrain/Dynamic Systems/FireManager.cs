using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static FireManager current;


    public MapGen mapGen;
    public Tile[,] tiles;

    void Start()
    {
        mapGen = MapGen.current;
        tiles = mapGen.tiles;
        current = this;
    }

    public void CalculateBurn(Tile tile)
    {
        Tile targetTile = tile.adjacentTiles[Random.Range(0, tile.adjacentTiles.Length)];
        if (targetTile.tileData.isFlammable && targetTile.tileData.flammability > Random.Range(1f, 100f))
        {
            targetTile.igniting = true;
        }
    }

    public void Burn(Tile tile)
    {
        if (tile.igniting)
        {
            tile.igniting = false;
            tile.isBurning = true;
        }

        tile.burnTimeLeft -= Time.fixedDeltaTime;
        if(tile.burnTimeLeft <= 0)
        {
            tile.isBurning = false;
            tile.tileData.flammability = 0;
        }
    }

    private void FixedUpdate()
    {
        foreach (Tile tile in tiles)
        {
            if (tile.isBurning)
            {
                CalculateBurn(tile);
            }
        }

        foreach (Tile tile in tiles)
        {
            if (tile.isBurning || tile.igniting)
            {
                Burn(tile);
            }
        }
    }
}
