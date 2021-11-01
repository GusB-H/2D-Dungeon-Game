using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager current;

    public Texture2D lightTexture;
    public Texture2D lightDisplayTexture;
    public Texture2D displacementMap;
    public MapGen mapGen;
    public Tile[,] tiles;
    Vector2Int[] offsets;
    int[] offsetReductions = { 141, 100, 141, 100, 1, 100, 141, 100, 141 };
    public AnimationCurve brightnessCurve;
    public Canvas lightingCanvas;

    void Start()
    {
        mapGen = MapGen.current;
        tiles = mapGen.tiles;
        current = this;
        offsets = new Vector2Int[]{
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1, -1),
        new Vector2Int( 1,  0),
        new Vector2Int( 1,  1)
        };

        lightTexture = new Texture2D(tiles.GetLength(0), tiles.GetLength(1), TextureFormat.RG16, false, true);
        //Create a texture to store light values. Each pixel corrosponds to a tile. Red channel is brightness, Green channel is tile opacity.
        lightTexture.filterMode = FilterMode.Point; //Wrap mode defaults to clamp, no need to set it twice
        //GetComponent<SpriteRenderer>().sprite = Sprite.Create(lightTexture, new Rect(Vector2.zero, new Vector2(tiles.GetLength(0), tiles.GetLength(1))), Vector2.zero); //set own sprite renderer for debug purposes
        lightDisplayTexture = new Texture2D(tiles.GetLength(0) * 2, tiles.GetLength(1) * 2, TextureFormat.RG16, false, true);
        lightDisplayTexture.filterMode = FilterMode.Point;

        lightingCanvas.GetComponent<SpriteRenderer>().sprite = Sprite.Create(lightDisplayTexture, new Rect(Vector2.zero, new Vector2(tiles.GetLength(0) * 2, tiles.GetLength(1) * 2)), Vector2.zero, 2);
        lightingCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(tiles.GetLength(0), tiles.GetLength(1));
        lightingCanvas.GetComponent<SpriteRenderer>().material.SetTexture("_LightMap", lightDisplayTexture);


        displacementMap = new Texture2D(2 * tiles.GetLength(0), 2 * tiles.GetLength(1), TextureFormat.RG16, false, true);
        displacementMap.filterMode = FilterMode.Point;
        UpdateDisplacementMap();
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(displacementMap, new Rect(Vector2.zero, new Vector2(tiles.GetLength(0) * 2, tiles.GetLength(1) * 2)), Vector2.zero);
    }

    public void UpdateDisplacementMap()
    {
        Color halfDown = new Color(0, 0.5f / tiles.GetLength(1), 0);
        halfDown = Color.green;
        Color[] newDisplacementMap = new Color[tiles.GetLength(0) * tiles.GetLength(1) * 4]; // x4 because each axis has two pixels per tile
        for (int y = tiles.GetLength(1) - 2; y >= 0; y--) //iterate downwards so that lower walls are in front of higher ones
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {

                if(tiles[x, y].tileData.isSolid)
                {
                    newDisplacementMap[(2 * x) + ((2 * y + 2) * 2 * tiles.GetLength(0))] = halfDown;
                    newDisplacementMap[(2 * x + 1) + ((2 * y + 2) * 2 * tiles.GetLength(0))] = halfDown;
                }
                else
                {

                }

            }
        }
        displacementMap.SetPixels(newDisplacementMap);
        displacementMap.Apply();    
    }

    void CalculateLighting()
    {


        List<Tile> queue = new List<Tile>();

        foreach (Tile tile in tiles)
        {
            if (tile.lightingUpdateFlag)
            {
                queue.Add(tile);
                tile.lightingUpdateFlag = false;
            }
        }

        int i = 0;
        int count = 0;
        while (queue.Count > 0)
        {
            if (queue[0].tileData.isSolid)
            {
                queue.RemoveAt(0);
                continue;
            }
            i = 0;
            foreach (Tile tile in queue[0].adjacentTiles)
            {
                if (tile.illuminationBuffer < queue[0].illuminationBuffer - offsetReductions[i])
                {
                    tile.illuminationBuffer = queue[0].illuminationBuffer - offsetReductions[i];
                    queue.Add(tile);
                }
                i++;
            }
            queue.RemoveAt(0);
            count++;
        }

        Color[] lightPixels = new Color[tiles.GetLength(0) * tiles.GetLength(1)];
        foreach (Tile tile in tiles)
        {
            lightPixels[tile.xy.x + tile.xy.y * tiles.GetLength(0)] = new Color(brightnessCurve.Evaluate(Mathf.Max(tile.illuminationBuffer, tile.ambientIllumination)), tile.tileData.isSolid ? 1 : 0, 0);
            //tile.illumination = tile.illuminationBuffer;
            tile.illuminationBuffer = Mathf.Max(tile.illuminationBuffer / 2, 0);
        }
        lightTexture.SetPixels(lightPixels);
        lightTexture.Apply();

        Color[] lightDisplayPixels = new Color[tiles.GetLength(0) * tiles.GetLength(1) * 4];
        for(int y = 0; y < tiles.GetLength(1) * 2; y++)
        {
            for(int x = 0; x < tiles.GetLength(0) * 2; x++)
            {
                int halfx = (int)(x * 0.5f);
                int halfy = (int)(y * 0.5f);
                if (y > 0 && y % 2 == 0 && tiles[halfx, halfy - 1].tileData.isSolid)
                {
                    lightDisplayPixels[x + y * tiles.GetLength(0) * 2] = lightPixels[halfx + (halfy - 1) * tiles.GetLength(0)];
                }
                else
                {
                    lightDisplayPixels[x + y * tiles.GetLength(0) * 2] = lightPixels[halfx + (halfy) * tiles.GetLength(0)];
                }
                
            }
        }           //loop through the higher resolution array to upscale the low res texture;
        lightDisplayTexture.SetPixels(lightDisplayPixels);
        lightDisplayTexture.Apply();



    }

    private void FixedUpdate()
    {
        CalculateLighting();

        foreach (Tile tile in tiles)
        {
            tile.UpdateColor();
        }
    }
}
