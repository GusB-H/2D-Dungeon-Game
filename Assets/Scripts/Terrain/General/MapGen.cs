using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGen : MonoBehaviour
{
    public static MapGen current;
    public TileSounds tileSounds;

    public Tile[,] tiles;
    public GameObject tilePrefab;
    public Sprite edgeWallSprite;
    public Sprite wallSprite;
    public Sprite wallTorchSprite;
    public Sprite floorSprite;
    public Sprite hallFloorSprite;
    public Sprite exitSprite;
    public Sprite openDoorSprite;
    public Sprite closedDoorSprite;
    public Vector2Int boundaryPadding;
    public Vector2Int mapSize;
    public Vector2 startPos;
    public Transform playerTransform;


    [System.Serializable]
    public class LockAndKey
    {
        public Switch key;
        public List<IDoorInterface> doors;
    }
    [System.Serializable]
    public class Room
    {
        public enum Type { Generic, MainPath, Entrance, Exit, Setpiece, Challenge, Treasure, Puzzle, Secret, Special };
        public Type type;
        public int detail;
        public Vector2 position;
        public Vector2 size;
        public List<Hall> halls;
        public List<Vector2> doorCandidates;

        public Room(Vector2 position, Vector2 size, List<Hall> halls, Type type = Type.Generic, int detail = 0)
        {
            this.position = position;
            this.halls = halls;
            this.size = size;
            doorCandidates = new List<Vector2>();
            this.type = type;
            this.detail = detail;
        }

        public Room(Vector2 position, Vector2 size, List<Vector2> doorCandidates, Type type = Type.Generic, int detail = 0)
        {
            this.position = position;
            halls = new List<Hall>();
            this.size = size;
            this.doorCandidates = doorCandidates;
            this.type = type;
            this.detail = detail;
        }
    }
    public class Hall
    {
        public enum Type { Generic, Challenge, Secret, Locked, Connector };
        public Room[] rooms = new Room[2];
        public Type type;
        public int detail;
        public DoorGroup doorGroup;

        public Hall(Room room1, Room room2 = null, Type type = Type.Generic, int detail = 0)
        {
            rooms[0] = room1;
            rooms[1] = room2;
            this.type = type;
            this.detail = detail;
        }
        public Hall(Room room1, Room room2, Type type, DoorGroup doorGroup, int detail = 0)
        {
            rooms[0] = room1;
            rooms[1] = room2;
            this.type = type;
            this.doorGroup = doorGroup;
            this.detail = detail;
        }
    }
    public struct RoomAndHall
    {
        public Room.Type roomType;
        public Hall.Type hallType;
        public bool overrideExitHalls;

        public RoomAndHall(Room.Type roomType, Hall.Type hallType, bool overrideExitHalls = false)
        {
            this.roomType = roomType;
            this.hallType = hallType;
            this.overrideExitHalls = overrideExitHalls;
        }
    }

    public List<Room> rooms;
    public List<Hall> halls;

    protected void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        boundaryPadding = new Vector2Int(4, 4);
        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").transform;
        }
        rooms = new List<Room>();
        halls = new List<Hall>();

        PreCreationEffects();
        GenerateMap();
        CreateDungeon(rooms);
        PostCreationEffects();

        startPos = rooms[0].position + rooms[0].size * 0.5f;
        playerTransform.position = (Vector3)startPos;
    }

    public virtual void Populate(Room room)
    {
        switch (room.type)
        {
            case Room.Type.Exit:
                tiles[Mathf.FloorToInt(room.position.x + room.size.x / 2), Mathf.FloorToInt(room.position.y + room.size.y / 2)].GetComponentInChildren<SpriteRenderer>().sprite = exitSprite;
                tiles[Mathf.FloorToInt(room.position.x + room.size.x / 2), Mathf.FloorToInt(room.position.y + room.size.y / 2)].type = Tile.TileType.exit;
                break;
        }
    }

    public virtual void PreCreationEffects()
    {

    }

    public virtual void PostCreationEffects()
    {

    }

    public Door SwapToDoor(Vector2Int position, bool isOpen = false)
    {
        GameObject doorObject = tiles[position.x, position.y].gameObject;
        Tile tile = tiles[position.x, position.y];
        Door door = doorObject.AddComponent<Door>();
        tiles[position.x, position.y] = door;

        door.roomIndex = tile.roomIndex;
        door.isExit = tile.isExit;
        door.exitTo = tile.exitTo;
        door.ambientIllumination = tile.ambientIllumination;
        door.openSprite = openDoorSprite;
        door.closedSprite = closedDoorSprite;
        if (isOpen)
        {
            door.isOpen = true;
            door.type = Tile.TileType.floor;
            door.spriteRenderer.sprite = door.openSprite;
        }
        else
        {
            door.isOpen = false;
            door.type = Tile.TileType.wall;
            door.spriteRenderer.sprite = door.closedSprite;
        }

        Destroy(tile);
        return (door);
    }

    public void SwapToDoor(Tile tile, bool isOpen = false)
    {
        SwapToDoor(tile.xy, isOpen);
    }

    public virtual void GenerateMap()
    {
        //Default map generator, makes two rooms connected by a single hallway. Customize the fuck out of this

        rooms.Add(new Room(Vector2.zero, new Vector2(5, 5), new List<Hall>(), Room.Type.Entrance));
        rooms.Add(new Room(Vector2.up * 20, new Vector2(5, 5), new List<Hall>(), Room.Type.Exit));
        halls.Add(new Hall(rooms[0], rooms[1]));
        rooms[0].halls.Add(halls[0]);
        rooms[1].halls.Add(halls[0]);
    }

    public virtual void CreateRoom(Room room, List<Room> dungeonRooms, List<Vector2Int> edgeTiles, List<Tile> wallTiles)
    {
        //Default room generator, should be overriden if you want anything interesting

        for (int y = -1; y <= room.size.y; y++)
        {
            for (int x = -1; x <= room.size.x; x++)
            {
                if (y == -1 || y == room.size.y || x == -1 || x == room.size.x)
                {
                    Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(room.position.x) + x, Mathf.FloorToInt(room.position.y) + y);
                    tiles[tilePos.x, tilePos.y].roomIndex = dungeonRooms.IndexOf(room);
                    if (!edgeTiles.Contains(tilePos))
                    {
                        edgeTiles.Add(tilePos);
                    }
                    if (!wallTiles.Contains(tiles[tilePos.x, tilePos.y]))
                    {
                        wallTiles.Add(tiles[tilePos.x, tilePos.y]);
                    }
                }
                else
                {
                    tiles[Mathf.FloorToInt(room.position.x) + x, Mathf.FloorToInt(room.position.y) + y].type = Tile.TileType.floor;
                    tiles[Mathf.FloorToInt(room.position.x) + x, Mathf.FloorToInt(room.position.y) + y].GetComponentInChildren<SpriteRenderer>().sprite = floorSprite;
                    tiles[Mathf.FloorToInt(room.position.x) + x, Mathf.FloorToInt(room.position.y) + y].roomIndex = dungeonRooms.IndexOf(room);
                }
            }
        }
    }

    public bool CheckPlacement(Room room, Vector2Int offset, bool exactPosition = false)
    {
        return CheckPlacement(Vector2Int.RoundToInt(room.position) + offset, exactPosition);
    }

    public bool CheckPlacement(Vector2Int position, bool exactPosition = false)
    {
        bool isValidPlacement = true;
        foreach (Room room in rooms)                     //  If there is another room where we're trying to put our current one, we put it one unit to the right of the last placed room instead.
        {
            if (room.position == position)
            {
                isValidPlacement = false;
                break;
            }
        }

        return isValidPlacement;
    }

    public virtual void SetHallTile(int x, int y, List<Vector2Int> edgeTiles) 
    {
        if(tiles[x, y].type == Tile.TileType.floor)
        {
            return;
        }
        tiles[x, y].type = Tile.TileType.floor;
        tiles[x, y].GetComponentInChildren<SpriteRenderer>().sprite = hallFloorSprite;
        edgeTiles.Add(new Vector2Int(x, y));
    }

    public virtual void SetHallTile(int x, int y, List<Vector2Int> edgeTiles, DoorGroup doorGroup, bool isOpen = false)
    {
        if (tiles[x, y].type == Tile.TileType.floor)
        {
            return;
        }
        doorGroup.doors.Add(SwapToDoor(new Vector2Int(x, y), isOpen));
        edgeTiles.Add(new Vector2Int(x, y));
    }

    public virtual void CreateHall(Hall hall, List<Vector2Int> edgeTiles)
    {
        CreateHall(hall, edgeTiles, null);
    }

    public virtual void CreateHall(Hall hall, List<Vector2Int> edgeTiles, DoorGroup doorGroup)
    {
        //Default hall generator. Do not use this

        Vector2Int room1Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[0].position.x + hall.rooms[0].size.x / 2), Mathf.FloorToInt(hall.rooms[0].position.y + hall.rooms[0].size.y / 2));
        Vector2Int room2Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[1].position.x + hall.rooms[1].size.x / 2), Mathf.FloorToInt(hall.rooms[1].position.y + hall.rooms[1].size.y / 2));


        if (Mathf.Abs(room1Pos.x - room2Pos.x) < (hall.rooms[0].size.x + hall.rooms[1].size.x) * 0.5f)
        {
            for (int y = 0; y <= Mathf.Abs(room2Pos.y - room1Pos.y); y++)
            {
                int x = Mathf.RoundToInt((room1Pos.x + room2Pos.x) * 0.5f);
                SetHallTile(x, room2Pos.y - y * (int)Mathf.Sign(room2Pos.y - room1Pos.y), edgeTiles);
            }
        }

        else if (Mathf.Abs(room1Pos.y - room2Pos.y) < (hall.rooms[0].size.y + hall.rooms[1].size.y) * 0.5f)
        {
            for (int x = 0; x <= Mathf.Abs(room2Pos.x - room1Pos.x); x++)
            {
                int y = Mathf.RoundToInt((room1Pos.y + room2Pos.y) * 0.5f);
                SetHallTile(room2Pos.x - x * (int)Mathf.Sign(room2Pos.x - room1Pos.x), y, edgeTiles);
            }
        }

        else
        {
            for (int x = 0; x <= Mathf.Abs(room2Pos.x - room1Pos.x); x++)
            {
                SetHallTile(room1Pos.x + x * (int)Mathf.Sign(room2Pos.x - room1Pos.x), room1Pos.y, edgeTiles);
            }

            for (int y = 0; y <= Mathf.Abs(room2Pos.y - room1Pos.y); y++)
            {
                SetHallTile(room2Pos.x, room2Pos.y - y * (int)Mathf.Sign(room2Pos.y - room1Pos.y), edgeTiles);
            }
        }
    }

    public void CreateDungeon(List<Room> dungeonRooms)
    {
        Vector2Int bounds = CalculateBoundaries(dungeonRooms, boundaryPadding);
        tiles = new Tile[bounds.x, bounds.y];
        List<Vector2Int> edgeTiles = new List<Vector2Int>();
        List<Tile> wallTiles = new List<Tile>();

        for(int y = 0; y < tiles.GetLength(1); y++)
        {
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                tiles[x, y] = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity, transform).AddComponent<Tile>();
                tiles[x, y].xy = new Vector2Int(x, y);

                if (x == 0 || y == 0 || x == tiles.GetLength(0) - 1 || y == tiles.GetLength(1) - 1)
                {
                    tiles[x, y].type = Tile.TileType.edgeWall;
                }
                else
                {
                    tiles[x, y].type = Tile.TileType.nothing;
                }

                tiles[x, y].roomIndex = -1;
                
            }
        }

        foreach (Room room in dungeonRooms)
        {
            CreateRoom(room, dungeonRooms, edgeTiles, wallTiles);
        }

        foreach (Hall hall in halls)
        {
            CreateHall(hall, edgeTiles, hall.doorGroup);
        }

        foreach(Room room in rooms)
        {
            Populate(room);
        }


        Vector2Int[] offsets = {
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1, -1),
        new Vector2Int( 1,  0),
        new Vector2Int( 1,  1)
        };



        foreach(Vector2Int tilePos in edgeTiles)
        {
            foreach (Vector2Int offset in offsets)
            {
                Vector2Int curTilePos = tilePos + offset;


                if (curTilePos.x >= 0 && curTilePos.y >= 0 && curTilePos.x < tiles.GetLength(0) && curTilePos.y < tiles.GetLength(1))
                {
                    if(tiles[curTilePos.x, curTilePos.y].type == Tile.TileType.nothing && !wallTiles.Contains(tiles[curTilePos.x, curTilePos.y]))
                    {
                        wallTiles.Add(tiles[curTilePos.x, curTilePos.y]);
                    }
                }

            }
        }

        foreach(Tile wallTile in wallTiles)
        {
            if (wallTile.type == Tile.TileType.nothing || wallTile.type == Tile.TileType.wall)
            {
                wallTile.GetComponentInChildren<SpriteRenderer>().sprite = wallSprite;
                wallTile.type = Tile.TileType.wall;
                wallTile.gameObject.layer = 10;
            }
        }



        foreach(Tile tile in tiles)
        {
            if(tile.type == Tile.TileType.nothing || tile.type == Tile.TileType.edgeWall)
            {
                bool adjacentToMap = false;

                foreach (Vector2Int offset in offsets)
                {
                    Vector2Int curTilePos = tile.xy + offset;


                    if (curTilePos.x >= 0 && curTilePos.y >= 0 && curTilePos.x < tiles.GetLength(0) && curTilePos.y < tiles.GetLength(1))
                    {
                        if (tiles[curTilePos.x, curTilePos.y].type == Tile.TileType.floor ||
                            tiles[curTilePos.x, curTilePos.y].type == Tile.TileType.wall)
                        {
                            adjacentToMap = true;
                        }
                    }
                }



                tile.gameObject.layer = 10;
                tile.tileData.isSolid = true;
                tile.GetComponentInChildren<SpriteRenderer>().sprite = edgeWallSprite;
                if (adjacentToMap)
                {
                    tile.type = Tile.TileType.edgeWall;
                }
                else
                {
                    tile.type = Tile.TileType.outOfBounds;
                }
            }
        }

        foreach(Tile tile in tiles)
        {
            if(tile.type == Tile.TileType.floor)
            {
                tile.tileData.isSolid = false;
            }
            else
            {
                tile.tileData.isSolid = true;
            }
        }

        Entity.tiles = tiles;
    }

    public Tile[] AdjacentTiles(Tile tile, bool returnSelf = false)
    {
        List<Tile> returnList = new List<Tile>();
        for (int y = -1; y <= 1; y++) 
        {
            for (int x = -1; x <= 1; x++)
            {
                if(!returnSelf && y == 0 && x == 0)
                {
                    continue;
                }

                if(tile.xy.x + x >= 0 && tile.xy.y + y >= 0 && tile.xy.x + x < tiles.GetLength(0) && tile.xy.y + y < tiles.GetLength(1))
                {
                    //if(tiles[tile.xy.x + x, tile.xy.y + y].gameObject.layer != 10)
                    {
                        returnList.Add(tiles[tile.xy.x + x, tile.xy.y + y]);
                    }
                }
            }
        }

        return returnList.ToArray();
    }

    public Tile GetTile(float x, float y)
    {
        return tiles[Mathf.RoundToInt(Mathf.Clamp(x, 0, tiles.GetLength(0))), Mathf.RoundToInt(Mathf.Clamp(y, 0, tiles.GetLength(1)))];
    }
    public Tile GetTile(Vector3 position)
    {
        return GetTile(position.x, position.y);
    }
    public Tile GetTile(Vector2 position)
    {
        return GetTile(position.x, position.y);
    }

    public virtual Vector2Int CalculateBoundaries(List<Room> rooms, Vector2Int padding)
    {
        Vector2Int maxCorner = Vector2Int.zero;
        Vector2Int minCorner = Vector2Int.zero;

        foreach(Room room in rooms)
        {
            maxCorner = new Vector2Int(Mathf.FloorToInt(Mathf.Max(room.position.x + room.size.x, maxCorner.x)),
                                       Mathf.FloorToInt(Mathf.Max(room.position.y + room.size.y, maxCorner.y)));
            minCorner = new Vector2Int(Mathf.FloorToInt(Mathf.Min(room.position.x - 4, minCorner.x)),
                                       Mathf.FloorToInt(Mathf.Min(room.position.y - 4, minCorner.y)));
        }

        foreach(Room room in rooms)
        {
            room.position -= minCorner;
        }

        return maxCorner - minCorner + padding;   
    }   
}
