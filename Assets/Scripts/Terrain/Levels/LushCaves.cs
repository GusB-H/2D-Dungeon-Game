using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LushCaves : MapGen
{
    public Sprite startFlowers;
    public Sprite staircaseDown;
    public AnimationCurve hallWidth;
    public AnimationCurve lightingCurve;

    public GameObject GlowSporePrefab;
    public GameObject MushroomPrefab;
    public GameObject GardenBanditPrefab;
    public GameObject BarrelPrefab;
    public GameObject CratePrefab;
    public GameObject BucketPrefab;
    public GameObject firePrefab;

    public override void PostCreationEffects()
    {
        Vector2Int perlinSeed1 = new Vector2Int(Random.Range(0, 10000), Random.Range(0, 10000));
        Vector2Int perlinSeed2 = new Vector2Int(Random.Range(0, 10000), Random.Range(0, 10000));
        Vector2Int perlinSeed3 = new Vector2Int(Random.Range(0, 10000), Random.Range(0, 10000));

        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                
                Vector2 perlinCoords = new Vector2(x / (Mathf.PI * 2), y / (Mathf.PI * 2)); //divide by an irrational number to minimize artifacts from perlin noise
                float brightness = lightingCurve.Evaluate(Mathf.PerlinNoise(perlinSeed1.x + perlinCoords.x * 1, perlinSeed1.y + perlinCoords.y * 1) * 0.5f +
                                                          Mathf.PerlinNoise(perlinSeed2.x + perlinCoords.x * 2, perlinSeed2.y + perlinCoords.y * 2) * 0.25f +
                                                          Mathf.PerlinNoise(perlinSeed3.x + perlinCoords.x * 4, perlinSeed3.y + perlinCoords.y * 4) * 0.125f); //octave noise
                tiles[x, y].ambientIllumination = (int)(500 * brightness);
                
                tiles[x, y].tileData.isFlammable = true;
                tiles[x, y].tileData.flammability = 2f;
                tiles[x, y].burnTimeLeft = Random.Range(1f, 10f);
            }
        }
    }
    public override void CreateRoom(Room room, List<Room> dungeonRooms, List<Vector2Int> edgeTiles, List<Tile> wallTiles)
    {
        for (int y = -1; y <= room.size.y; y++)
        {
            for (int x = -1; x <= room.size.x; x++)
            {
                Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(room.position.x) + x, Mathf.FloorToInt(room.position.y) + y);

                if (Mathf.Pow((x - room.size.x * 0.5f) / (room.size.x * 0.5f), 2f) +
                    Mathf.Pow((y - room.size.y * 0.5f) / (room.size.y * 0.5f), 2f) < 1)
                {
                    tiles[tilePos.x, tilePos.y].type = Tile.TileType.floor;

                    if ((x - 1) / room.size.x <  0.5 && (y - 1) / room.size.y <  0.5 &&
                        (x + 0) / room.size.x >= 0.5 && (y + 0) / room.size.y >= 0.5)
                    {
                        if (room.type == Room.Type.Entrance)
                        {
                            tiles[tilePos.x, tilePos.y].GetComponentInChildren<SpriteRenderer>().sprite = startFlowers;
                        }
                        else if(room.type == Room.Type.Exit)
                        {
                            tiles[tilePos.x, tilePos.y].GetComponentInChildren<SpriteRenderer>().sprite = staircaseDown;
                            tiles[tilePos.x, tilePos.y].type = Tile.TileType.exit;
                        }
                        else
                        {
                            tiles[tilePos.x, tilePos.y].GetComponentInChildren<SpriteRenderer>().sprite = floorSprite;

                            int roomType = Random.Range(0, 10);

                            switch (roomType)
                            {
                                case int j when 0 <= j && j < 5: //1-4 buckets
                                    for(int i = Random.Range(0, 4); i < 4; i++)
                                    {
                                        Instantiate(BucketPrefab, new Vector3(tilePos.x + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                            tilePos.y + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                            0), Quaternion.identity);
                                    }
                                    break;

                                case int j when 5 <= j && j < 8: //1-3 barrels/crates
                                    for (int i = Random.Range(0, 3); i < 4; i++)
                                    {
                                        if (Random.Range(0, 2) == 0)
                                        {
                                            Instantiate(CratePrefab, new Vector3(tilePos.x + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                                tilePos.y + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                                0), Quaternion.identity);
                                        }
                                        else
                                        {
                                            Instantiate(BarrelPrefab, new Vector3(tilePos.x + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                                tilePos.y + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f),
                                                                                0), Quaternion.identity);
                                        }
                                    }
                                    break;

                                case int j when 8 <= j:
                                    Instantiate(firePrefab, new Vector3(tilePos.x, tilePos.y, 0), Quaternion.identity);
                                    break;
                            }

                            if (Random.Range(1, 5) < 3 * 0)
                            {
                                for (int i = 0; i < Random.Range(2, 4); i++)
                                {
                                    Instantiate(MushroomPrefab, new Vector3(x + room.position.x + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f), y + room.position.y + Random.Range(room.size.y * -0.25f, room.size.y * 0.25f), 0), Quaternion.identity);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Random.Range(1, 5); i++)
                                {
                                    Instantiate(GardenBanditPrefab, new Vector3(x + room.position.x + Random.Range(room.size.x * -0.25f, room.size.x * 0.25f), y + room.position.y + Random.Range(room.size.y * -0.25f, room.size.y * 0.25f), 0), Quaternion.identity);
                                }
                            }
                        }
                    }

                    else
                    {
                        tiles[tilePos.x, tilePos.y].GetComponentInChildren<SpriteRenderer>().sprite = floorSprite;
                    }
                    tiles[tilePos.x, tilePos.y].roomIndex = dungeonRooms.IndexOf(room);
                }
                else
                {
                    if (!edgeTiles.Contains(tilePos))
                    {
                        edgeTiles.Add(tilePos);
                    }
                    if (!wallTiles.Contains(tiles[tilePos.x, tilePos.y]))
                    {
                        wallTiles.Add(tiles[tilePos.x, tilePos.y]);
                    }
                }
            }
        }
    }

    public override void GenerateMap()
    {
        boundaryPadding = new Vector2Int(6, 6);
        float angle = Random.Range(0, 2 * Mathf.PI);
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perpendicular = new Vector2(direction.y, direction.x);
        if(Random.Range(0, 2) == 0)
        {
            perpendicular *= new Vector2(-1, 1);
        }
        else
        {
            perpendicular *= new Vector2(1, -1);
        }

        int mapType = Mathf.FloorToInt(Random.Range(0f, 2f)); //is map short or long? 
        mapType += 2 * Mathf.FloorToInt(Random.Range(0f, 2f)); //is map curved or bent? 

        
        rooms.Add(new Room(new Vector2(0, 0), Vector2.one, new List<Hall>(), Room.Type.Entrance));
        rooms.Add(new Room(new Vector2(0, 1), Vector2.one, new List<Hall>()));
        halls.Add(new Hall(rooms[0], rooms[1]));
        rooms.Add(new Room(new Vector2(1, 1), Vector2.one, new List<Hall>()));
        halls.Add(new Hall(rooms[1], rooms[2]));

        switch (mapType)
        {
            case 0: //short curve
                rooms.Add(new Room(new Vector2(1, 0), Vector2.one, new List<Hall>(), Room.Type.Exit));
                halls.Add(new Hall(rooms[2], rooms[3]));
                break;

            case 1: //long curve
                rooms.Add(new Room(new Vector2(2, 1), Vector2.one, new List<Hall>()));
                halls.Add(new Hall(rooms[2], rooms[3]));
                rooms.Add(new Room(new Vector2(2, 0), Vector2.one, new List<Hall>(), Room.Type.Exit));
                halls.Add(new Hall(rooms[3], rooms[4]));
                break;

            case 2: //short bend
                rooms.Add(new Room(new Vector2(1, 2), Vector2.one, new List<Hall>(), Room.Type.Exit));
                halls.Add(new Hall(rooms[2], rooms[3]));
                break;

            case 3: //long bend
                rooms.Add(new Room(new Vector2(2, 1), Vector2.one, new List<Hall>()));
                halls.Add(new Hall(rooms[2], rooms[3]));
                rooms.Add(new Room(new Vector2(2, 2), Vector2.one, new List<Hall>(), Room.Type.Exit));
                halls.Add(new Hall(rooms[3], rooms[4]));
                break;
        }

        int sideRoomsLeft = Random.Range(2, 5);
        Vector2Int[] roomOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
        int mainPathLength = rooms.Count;

        for(int emergencyExit = 0; emergencyExit < 1000; emergencyExit++)
        {
            int roomId = Random.Range(0, mainPathLength);
            Vector2Int offset = roomOffsets[Random.Range(0, 4)];

            bool isValidRoom = true;
            foreach(Room room in rooms)
            {
                if(room.position == rooms[roomId].position + offset)
                {
                    isValidRoom = false;
                    break;
                }
            }
            if (!isValidRoom)
            {
                continue;
            }

            rooms.Add(new Room(rooms[roomId].position + offset, Vector2.one, new List<Hall>()));
            halls.Add(new Hall(rooms[roomId], rooms[rooms.Count - 1]));
            rooms[roomId].halls.Add(halls[halls.Count - 1]);
            rooms[rooms.Count - 1].halls.Add(halls[halls.Count - 1]);
            sideRoomsLeft--;
            if(sideRoomsLeft <= 0)
            {
                break;
            }
        }

        foreach(Room room in rooms)
        {
            room.position = room.position.y * direction + room.position.x * perpendicular;
            room.position *= 24;
            room.position += new Vector2(Random.Range(-6f, 6f), Random.Range(-6f, 6f));
            room.size = new Vector2(Random.Range(10, 14), Random.Range(10, 14));
        }


    }

    public override void CreateHall(Hall hall, List<Vector2Int> edgeTiles)
    {
        Vector2Int room1Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[0].position.x + hall.rooms[0].size.x / 2), Mathf.FloorToInt(hall.rooms[0].position.y + hall.rooms[0].size.y / 2));
        Vector2Int room2Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[1].position.x + hall.rooms[1].size.x / 2), Mathf.FloorToInt(hall.rooms[1].position.y + hall.rooms[1].size.y / 2));

        Vector2 direction = ((Vector2)room2Pos - room1Pos).normalized;

        float roomDist = Vector2.Distance(room1Pos, room2Pos);
        for (int i = 0; i <= roomDist; i++)
        {
            float evaluateConstant = (float)i / roomDist;
            float d = hallWidth.Evaluate(evaluateConstant);
            float w = Mathf.Max(d * 0.5f * Mathf.Lerp(hall.rooms[0].size.x, hall.rooms[1].size.x, evaluateConstant), 3);
            float h = Mathf.Max(d * 0.5f * Mathf.Lerp(hall.rooms[0].size.y, hall.rooms[1].size.y, evaluateConstant), 3);

            for (int x = -Mathf.CeilToInt(w); x < w; x++)
            {
                for (int y = -Mathf.CeilToInt(h); y < h; y++)
                {

                    int evaluateX = room1Pos.x + Mathf.FloorToInt(direction.x * i + x);
                    int evaluateY = room1Pos.y + Mathf.FloorToInt(direction.y * i + y);
                    if (Mathf.Min(evaluateX, evaluateY) >= 0 &&
                        Mathf.Pow((x - w) / (w), 2f) +
                        Mathf.Pow((y - h) / (h), 2f) < 1
                        && tiles[evaluateX, evaluateY].type != Tile.TileType.floor
                        ) 
                    {
                        tiles[evaluateX, evaluateY].type = Tile.TileType.floor;
                        tiles[evaluateX, evaluateY].GetComponentInChildren<SpriteRenderer>().sprite = floorSprite;
                        edgeTiles.Add(new Vector2Int(evaluateX, evaluateY));
                    }
                }
            }
        }
        
    }
}
