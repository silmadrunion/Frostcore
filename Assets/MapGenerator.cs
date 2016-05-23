﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    [HideInInspector]
    public int[,] map;

    public IEnumerator GenerateMap()
    {
        GameMaster.gm.Generating = true;

        map = new int[width, height];

        StartCoroutine(RandomFillMap());
        for (; ; )
            if (inRandomFillMap)
                yield return null;
            else break;

        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(SmoothMap());
            for (; ; )
                if (inSmoothMap)
                    yield return null;
                else break;
        }

        StartCoroutine(ProcessMap());
        for (; ; )
            if (inProcessmap)
                yield return null;
            else break;

        StartCoroutine(FillWithDefault());
        for (; ; )
            if (inFillWIthDefault)
                yield return null;
            else break;

        mapFlags = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] != 0 && mapFlags[x, y] == 0)
                    FindPropperItemFor(x, y);
            }

            if (x % (int)(10000 / height) == 0)
                yield return null;
        }

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                AddHalfblock(x, y);
            }

            if (x % (int)(10000 / height) == 0)
                yield return null;
        }

        GameMaster.gm.numberOfGens++;
        GameMaster.gm.Generating = false;

        yield break;
    }

    List<List<Coord>> wallRegions;

    bool inProcessmap;
    IEnumerator ProcessMap()
    {
        inProcessmap = true;
        wallRegions = GetRegions(1);

        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        yield return null;

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        yield return null;

        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
        inProcessmap = false;
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    System.Random pseudoRandom;

    bool inRandomFillMap;
    IEnumerator RandomFillMap()
    {
        inRandomFillMap = true;
        if (useRandomSeed)
        {
            seed = Time.realtimeSinceStartup.ToString();
        }

        pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }

            if (x % (int)(10000 / height) == 0)
                yield return null;
        }

        inRandomFillMap = false;
    }

    bool inSmoothMap;
    public IEnumerator SmoothMap()
    {
        inSmoothMap = true;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }

            if (x % (int)(10000 / height) == 0)
                yield return null;
        }
        inSmoothMap = false;
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }


    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    public int MaxChance;
    public int MinBlocksTogheder;
    public int MaxBlocksTogheder;

    [System.Serializable]
    public struct Element
    {
        public int SourceId;
        public int MaxDepth;
        public int MinDepth;
        public int Chance;
    };

    public Element[] Blocks;
    public Element[] Default;

    bool inFillWIthDefault;
    IEnumerator FillWithDefault()
    {
        inFillWIthDefault = true;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                    continue;

                Element possibleElementFound = new Element();
                foreach (Element d in Default)
                {
                    if (y > d.MinDepth && y < d.MaxDepth)
                    {
                        possibleElementFound = d;
                    }
                }

                if (possibleElementFound.SourceId != 0)
                    map[x, y] = possibleElementFound.SourceId;
            }

            if (x % (int)(10000 / height) == 0)
                yield return null;
        }
        inFillWIthDefault = false;
    }

    int[,] mapFlags;

    void FindPropperItemFor(int x, int y)
    {
        int rand = pseudoRandom.Next(0, MaxChance);

        foreach (Element e in Blocks)
        {
            if (y > e.MinDepth && y < e.MaxDepth)
                if (rand < e.Chance)
                {
                    rand = pseudoRandom.Next(MinBlocksTogheder, MaxBlocksTogheder);

                    mapFlags[x, y] = 1;

                    Coord temp = new Coord(x, y);

                    int tryhard = 0;
                    while (rand > 0)
                    {
                        int randDirX = pseudoRandom.Next(-1, 1);
                        int randDirY = pseudoRandom.Next(-1, 1);

                        if (!IsInMapRange(temp.tileX + randDirX, temp.tileY + randDirY))
                            break;
                        if (mapFlags[temp.tileX + randDirX, temp.tileY + randDirY] == 0 && map[temp.tileX + randDirX, temp.tileY + randDirY] != 0)
                        {
                            temp.tileX += randDirX;
                            temp.tileY += randDirY;
                            mapFlags[temp.tileX, temp.tileY] = 1;
                            map[temp.tileX, temp.tileY] = e.SourceId;
                            rand--;
                        }
                        else
                            tryhard++;

                        if (tryhard > MaxBlocksTogheder * 2)
                            break;
                    }

                    return;
                }
        }
    }

    void AddHalfblock(int x, int y)
    {
        if (map[x, y] != 0)
            return;

        if (map[x, y - 1] != 0 && map[x + 1, y] != 0 && map[x, y - 1] < 10 && map[x + 1, y] < 10)
        {
            if (map[x + 1, y] == 1 || map[x + 1, y] == 2 || map[x + 1, y] == 3)
                map[x, y] = map[x + 1, y] * 10 + 1;
            else if (map[x, y - 1] == 1 || map[x, y - 1] == 2 || map[x, y - 1] == 3)
                map[x, y] = map[x, y - 1] * 10 + 1;
        }
        else if (map[x + 1, y] != 0 && map[x, y + 1] != 0 && map[x + 1, y] < 10 && map[x, y + 1] < 10)
        {
            if (map[x, y + 1] == 1 || map[x, y + 1] == 2 || map[x, y + 1] == 3)
                map[x, y] = map[x, y + 1] * 10 + 2;
            else if (map[x + 1, y] == 1 || map[x + 1, y] == 2 || map[x + 1, y] == 3)
                map[x, y] = map[x + 1, y] * 10 + 2;
        }
        else if (map[x, y + 1] != 0 && map[x - 1, y] != 0 && map[x, y + 1] < 10 && map[x - 1, y] < 10)
        {
            if (map[x - 1, y] == 1 || map[x - 1, y] == 2 || map[x - 1, y] == 3)
                map[x, y] = map[x - 1, y] * 10 + 3;
            else if (map[x, y + 1] == 1 || map[x, y + 1] == 2 || map[x, y + 1] == 3)
                map[x, y] = map[x, y + 1] * 10 + 3;
        }
        else if (map[x - 1, y] != 0 && map[x, y - 1] != 0 && map[x - 1, y] < 10 && map[x, y - 1] < 10)
        {
            if (map[x, y - 1] == 1 || map[x, y - 1] == 2 || map[x, y - 1] == 3)
                map[x, y] = map[x, y - 1] * 10 + 4;
            else if (map[x - 1, y] == 1 || map[x - 1, y] == 2 || map[x - 1, y] == 3)
                map[x, y] = map[x - 1, y] * 10 + 4;
        }
    }
}
