using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garden : MapGen
{
    public Hall exitHall;
    public DoorGroup exitHallDoors;

    public override void PreCreationEffects()
    {
        exitHallDoors = new DoorGroup();
    }

    public override void Populate(Room room)
    {
        Vector2 roomPos = room.position + (room.size - Vector2.one) / 2;
        switch (room.type)
        {
            case Room.Type.Generic:
                //Instantiate(Resources.Load("Bees (WIP)") as GameObject, roomPos, Quaternion.identity);
                break;

            case Room.Type.MainPath:
                switch (room.detail)
                {
                    case int n when n < 10: //0-1
                        Instantiate(Resources.Load("Campfire") as GameObject, roomPos, Quaternion.identity);
                        for (int i = Random.Range(2, 5); i > 0; i--)
                        {
                            Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(1.25f, 1.5f);
                            if(Random.Range(0, 2) == 0) Instantiate(Resources.Load("Bramble Bandit") as GameObject, roomPos + randomOffset, Quaternion.identity);
                            else Instantiate(Resources.Load("Garden Bandit") as GameObject, roomPos + randomOffset, Quaternion.identity);
                        }
                        for (int i = Random.Range(1, 4); i > 0; i--)
                        {
                            Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(3f, 5f);
                            Instantiate(Resources.Load("Crate") as GameObject, roomPos + randomOffset, Quaternion.identity);
                        }
                        break;
                }

                //Instantiate(banditPrefab, room.position + (room.size - Vector2.one) / 2, Quaternion.identity);
                break;

            case Room.Type.Entrance:
                break;

            case Room.Type.Exit:
                tiles[Mathf.FloorToInt(room.position.x + room.size.x / 2), Mathf.FloorToInt(room.position.y + room.size.y / 2)].GetComponentInChildren<SpriteRenderer>().sprite = exitSprite;
                tiles[Mathf.FloorToInt(room.position.x + room.size.x / 2), Mathf.FloorToInt(room.position.y + room.size.y / 2)].type = Tile.TileType.exit;
                break;

            case Room.Type.Setpiece:
                break;

            case Room.Type.Challenge:
                break;

            case Room.Type.Treasure:
                break;

            case Room.Type.Puzzle:
                break;

            case Room.Type.Secret:
                break;

            case Room.Type.Special:
                Switch.CreateSwitch(roomPos, exitHallDoors);
                break;
        }
    }

    public override void PostCreationEffects()
    {

    }

    public void AddKeyRoom(IDoorInterface doors, Room accessibleFrom, int maxDistance)
    {

    }

    public void CreateBranch(Room startingRoom, int minLength, int maxLength, RoomAndHall[] mainTypes, RoomAndHall[] endTypes, bool forceEndRoom = true)
    {
        Vector2Int[] legalDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };
        Room currentRoom = startingRoom;
        int branchLength = Random.Range(minLength, maxLength + 1);

        for(int curBranchSpot = 0; curBranchSpot < branchLength; curBranchSpot++)
        {
            Utilities.Shuffle(legalDirections);
            for(int i = 0; i < 4; i++)
            {
                if(CheckPlacement(Vector2Int.RoundToInt(currentRoom.position) + legalDirections[i]))
                {
                    RoomAndHall roomAndHall;
                    if (curBranchSpot == branchLength)
                    {
                        roomAndHall = endTypes[Random.Range(0, endTypes.Length)];
                    }
                    else
                    {
                        roomAndHall = mainTypes[Random.Range(0, mainTypes.Length)];
                    }

                    rooms.Add(new Room(currentRoom.position + legalDirections[i], new Vector2(5, 5), new List<Hall>(), roomAndHall.roomType));
                    halls.Add(new Hall(rooms[rooms.Count - 1], currentRoom, roomAndHall.hallType));
                    rooms[rooms.Count - 1].halls.Add(halls[halls.Count - 1]);
                    currentRoom.halls.Add(halls[halls.Count - 1]);
                    currentRoom = rooms[rooms.Count - 1];

                    break;
                }

                if(i == 3) //All four offsets are already occupied, what do we do now?
                {
                    if (forceEndRoom && curBranchSpot > 0) //If we have created >=1 room and need the end room, turn the last room into the end room
                    {
                        RoomAndHall roomAndHall = endTypes[Random.Range(0, endTypes.Length)];
                        Room lastRoom = rooms[rooms.Count - 1];
                        lastRoom.type = roomAndHall.roomType;
                        lastRoom.halls[lastRoom.halls.Count - 1].type = roomAndHall.hallType;
                    }
                    return; //end the function early
                }
            }

        }

    }

    public override void GenerateMap()
    {
        rooms.Add(new Room(Vector2.zero, new Vector2(5, 5), new List<Hall>(), Room.Type.Entrance));
        Vector2Int testPos = new Vector2Int(0, 0);
        for (int i = 0; i < Run.current.GetSubStage() + 1; i++)
        {
            Vector2Int testOffset = new Vector2Int(0, Random.Range(-1, 2)); // the test offset will be either +/- 1 unit up...
            if (testOffset.y == 0) testOffset = Vector2Int.right;           // ...or one unit to the right

            bool isValidPlacement = CheckPlacement(testPos + testOffset);

            if (!isValidPlacement) testOffset = Vector2Int.right;   //  The way this generator works is that each new room will be 1 unit above, below, or to the right of the last.
                                                                    //  If the last room generated below its parent, then attempting to place the next room above it would be a problem.
            testPos += testOffset;                                  //  Instead, we place it one unit to the right. This way, the main path never loops back on itself, and can generate smoothly.
            
            rooms.Add(new Room(testPos, new Vector2(5, 5), new List<Hall>(), Room.Type.MainPath)); 
            halls.Add(new Hall(rooms[rooms.Count - 1], rooms[rooms.Count - 2], Hall.Type.Connector));
            rooms[rooms.Count - 1].halls.Add(halls[halls.Count - 1]);
            rooms[rooms.Count - 2].halls.Add(halls[halls.Count - 1]);
        }



        rooms.Add(new Room(testPos + Vector2.right, new Vector2(5, 5), new List<Hall>(), Room.Type.Exit));
        halls.Add(new Hall(rooms[rooms.Count- 1], rooms[rooms.Count - 2], Hall.Type.Locked, exitHallDoors));
        exitHall = halls[halls.Count - 1];
        rooms[rooms.Count - 1].halls.Add(halls[halls.Count - 1]);
        rooms[rooms.Count - 2].halls.Add(halls[halls.Count - 1]);

        List<Vector2Int> randomOffsets = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.right };
        Room keyRoomBase = rooms[Random.Range(1, rooms.Count - 1)];

        for(int i = 0; i < 2; i++)
        {
            Vector2Int testOffset = randomOffsets[Random.Range(0, randomOffsets.Count - 1)]; 
            bool isValidPlacement = true;
            foreach (Room room in rooms)                     //  If there is another room where we're trying to put our current one, we put it one unit to the right of the last placed room instead.
            {
                if (room.position == keyRoomBase.position + testOffset)
                {
                    isValidPlacement = false;
                    break;
                }
            }

            if (isValidPlacement)
            {
                rooms.Add(new Room(keyRoomBase.position + testOffset, new Vector2(5, 5), new List<Hall>(), Room.Type.Special));
                halls.Add(new Hall(keyRoomBase, rooms[rooms.Count - 1]));
                rooms[rooms.Count - 1].halls.Add(halls[halls.Count - 1]);
                keyRoomBase.halls.Add(halls[halls.Count - 1]);
                break;
            }
            randomOffsets.Remove(testOffset);
        }

        CreateBranch(rooms[1], 1, 2, new RoomAndHall[] { new RoomAndHall(Room.Type.Generic, Hall.Type.Generic) }, new RoomAndHall[] { new RoomAndHall(Room.Type.Generic, Hall.Type.Generic) });
        if (Run.current.GetSubStage() >= 3) 
        {
            CreateBranch(rooms[Random.Range(1, Run.current.GetSubStage())], 1, 2, new RoomAndHall[] { new RoomAndHall(Room.Type.Generic, Hall.Type.Generic) }, new RoomAndHall[] { new RoomAndHall(Room.Type.Generic, Hall.Type.Generic) });
        }
        

        PlaceRooms();
    }

    void PlaceRooms()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perpendicular = new Vector2(direction.y, direction.x);
        direction = Vector2.up;
        perpendicular = Vector2.right;

        foreach (Room room in rooms)
        {
            room.size = new Vector2Int(11, 11);
            //room.size = new Vector2(Random.Range(4, 8), Random.Range(4, 8));
            room.position = room.position.y * direction + room.position.x * perpendicular;
            room.position *= 13;
            //room.position += new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));   
        }
    }

    public override void CreateRoom(Room room, List<Room> dungeonRooms, List<Vector2Int> edgeTiles, List<Tile> wallTiles)
    {

        //Default room generator, should be overriden if you want anything interesting

        Vector2 roomCenter = (room.size - Vector2.one) / 2;
        for (int y = -1; y <= room.size.y; y++)
        {
            for (int x = -1; x <= room.size.x; x++)
            {
                if ((y == -1 || y == room.size.y || x == -1 || x == room.size.x)
                    || Mathf.Pow((y - roomCenter.y) * 2 / room.size.y, 2) + 
                       Mathf.Pow((x - roomCenter.x) * 2 / room.size.x, 2) > 1)
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

    

    public override void CreateHall(Hall hall, List<Vector2Int> edgeTiles, DoorGroup doorGroup)
    {
        Vector2Int room1Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[0].position.x + hall.rooms[0].size.x / 2), Mathf.FloorToInt(hall.rooms[0].position.y + hall.rooms[0].size.y / 2));
        Vector2Int room2Pos = new Vector2Int(Mathf.FloorToInt(hall.rooms[1].position.x + hall.rooms[1].size.x / 2), Mathf.FloorToInt(hall.rooms[1].position.y + hall.rooms[1].size.y / 2));

        switch (hall.type)
        {
            default:
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
                break;

            case Hall.Type.Locked:
                if(doorGroup == null) goto default;

                if (Mathf.Abs(room1Pos.x - room2Pos.x) < (hall.rooms[0].size.x + hall.rooms[1].size.x) * 0.5f)
                {
                    for (int y = 0; y <= Mathf.Abs(room2Pos.y - room1Pos.y); y++)
                    {
                        int x = Mathf.RoundToInt((room1Pos.x + room2Pos.x) * 0.5f);
                        SetHallTile(x, room2Pos.y - y * (int)Mathf.Sign(room2Pos.y - room1Pos.y), edgeTiles, doorGroup);
                    }
                }
                else if (Mathf.Abs(room1Pos.y - room2Pos.y) < (hall.rooms[0].size.y + hall.rooms[1].size.y) * 0.5f)
                {
                    for (int x = 0; x <= Mathf.Abs(room2Pos.x - room1Pos.x); x++)
                    {
                        int y = Mathf.RoundToInt((room1Pos.y + room2Pos.y) * 0.5f);
                        SetHallTile(room2Pos.x - x * (int)Mathf.Sign(room2Pos.x - room1Pos.x), y, edgeTiles, doorGroup);
                    }
                }
                else
                {
                    for (int x = 0; x <= Mathf.Abs(room2Pos.x - room1Pos.x); x++)
                    {
                        SetHallTile(room1Pos.x + x * (int)Mathf.Sign(room2Pos.x - room1Pos.x), room1Pos.y, edgeTiles, doorGroup);
                    }

                    for (int y = 0; y <= Mathf.Abs(room2Pos.y - room1Pos.y); y++)
                    {
                        SetHallTile(room2Pos.x, room2Pos.y - y * (int)Mathf.Sign(room2Pos.y - room1Pos.y), edgeTiles, doorGroup);
                    }
                }

                return;

            case Hall.Type.Connector:
                Vector2Int lowCorner = Vector2Int.FloorToInt(new Vector2(
                    Mathf.Min(hall.rooms[0].position.x, hall.rooms[1].position.x), 
                    Mathf.Min(hall.rooms[0].position.y, hall.rooms[1].position.y)));
                Vector2Int highCorner = Vector2Int.CeilToInt(new Vector2(
                    Mathf.Max(hall.rooms[0].position.x + hall.rooms[0].size.x, hall.rooms[1].position.x + hall.rooms[1].size.x),
                    Mathf.Max(hall.rooms[0].position.y + hall.rooms[0].size.y, hall.rooms[1].position.y + hall.rooms[1].size.y)));

                for (int y = lowCorner.y; y <= highCorner.y; y++)
                {
                    for (int x = lowCorner.x; x <= highCorner.x; x++)
                    {
                        if (Utilities.SqrDistanceFromLineSegment(startPos + new Vector2Int(x, y), room1Pos, room2Pos) <= 25)
                        {
                            SetHallTile(x, y, edgeTiles);
                        }
                    }
                }

                return;
        }


        
    }
}
