using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CMapGenerator : MonoBehaviour {

    [HideInInspector]
    public int m_iWidth;
    [HideInInspector]
    public int m_iHeight;

    [HideInInspector]
    public string m_strSeed;

    [HideInInspector]
    public bool m_bUseRandomSeed;

    [HideInInspector]
    [Range(0, 100)]
    public int m_iRandomFillPercent;

    int[,] m_arrMap;    

    public void GenerateMap()
    {
        m_arrMap = new int[m_iWidth, m_iHeight];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 1;
        int[,] borderedMap = new int[m_iWidth + borderSize * 2, m_iHeight + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < m_iWidth + borderSize && y >= borderSize && y < m_iHeight + borderSize)
                {
                    borderedMap[x, y] = m_arrMap[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        CMeshGenerator meshGen = GetComponent<CMeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

    void ProcessMap()
    {
        List<List<CCoord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<CCoord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (CCoord tile in wallRegion)
                {
                    m_arrMap[tile.m_iTileX, tile.m_iTileY] = 0;
                }
            }
        }

        List<List<CCoord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<CRoom> survivingRooms = new List<CRoom>();

        foreach (List<CCoord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (CCoord tile in roomRegion)
                {
                    m_arrMap[tile.m_iTileX, tile.m_iTileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new CRoom(roomRegion, m_arrMap));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].m_bIsMainRoom = true;
        survivingRooms[0].m_bIsAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<CRoom> a_AllRooms, bool a_ForceAccessibilityFromMainRoom = false)
    {
        List<CRoom> roomListA = new List<CRoom>();
        List<CRoom> roomListB = new List<CRoom>();

        if (a_ForceAccessibilityFromMainRoom)
        {
            foreach (CRoom room in a_AllRooms)
            {
                if (room.m_bIsAccessibleFromMainRoom)
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
            roomListA = a_AllRooms;
            roomListB = a_AllRooms;
        }

        int bestDistance = 0;
        CCoord bestTileA = new CCoord();
        CCoord bestTileB = new CCoord();
        CRoom bestRoomA = new CRoom();
        CRoom bestRoomB = new CRoom();
        bool possibleConnectionFound = false;

        foreach (CRoom roomA in roomListA)
        {
            if (!a_ForceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.m_lstOfConnectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (CRoom roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.m_lstOfEdgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.m_lstOfEdgeTiles.Count; tileIndexB++)
                    {
                        CCoord tileA = roomA.m_lstOfEdgeTiles[tileIndexA];
                        CCoord tileB = roomB.m_lstOfEdgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.m_iTileX - tileB.m_iTileX, 2) + Mathf.Pow(tileA.m_iTileY - tileB.m_iTileY, 2));

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
            if (possibleConnectionFound && !a_ForceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && a_ForceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(a_AllRooms, true);
        }

        if (!a_ForceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(a_AllRooms, true);
        }
    }

    void CreatePassage(CRoom a_RoomA, CRoom a_RoomB, CCoord a_TileA, CCoord a_TileB)
    {
        CRoom.ConnectRooms(a_RoomA, a_RoomB);
        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

        List<CCoord> line = GetLine(a_TileA, a_TileB);
        foreach (CCoord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    void DrawCircle(CCoord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.m_iTileX + x;
                    int drawY = c.m_iTileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        m_arrMap[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<CCoord> GetLine(CCoord a_From, CCoord a_To)
    {
        List<CCoord> line = new List<CCoord>();

        int x = a_From.m_iTileX;
        int y = a_From.m_iTileY;

        int dx = a_To.m_iTileX - a_From.m_iTileX;
        int dy = a_To.m_iTileY - a_From.m_iTileY;

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
            line.Add(new CCoord(x, y));

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

    Vector3 CoordToWorldPoint(CCoord a_Tile)
    {
        return new Vector3(-m_iWidth / 2 + .5f + a_Tile.m_iTileX, 2, -m_iHeight / 2 + .5f + a_Tile.m_iTileY);
    }

    List<List<CCoord>> GetRegions(int tileType)
    {
        List<List<CCoord>> regions = new List<List<CCoord>>();
        int[,] mapFlags = new int[m_iWidth, m_iHeight];

        for (int x = 0; x < m_iWidth; x++)
        {
            for (int y = 0; y < m_iHeight; y++)
            {
                if (mapFlags[x, y] == 0 && m_arrMap[x, y] == tileType)
                {
                    List<CCoord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (CCoord tile in newRegion)
                    {
                        mapFlags[tile.m_iTileX, tile.m_iTileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<CCoord> GetRegionTiles(int a_StartX, int a_StartY)
    {
        List<CCoord> tiles = new List<CCoord>();
        int[,] mapFlags = new int[m_iWidth, m_iHeight];
        int tileType = m_arrMap[a_StartX, a_StartY];

        Queue<CCoord> queue = new Queue<CCoord>();
        queue.Enqueue(new CCoord(a_StartX, a_StartY));
        mapFlags[a_StartX, a_StartY] = 1;

        while (queue.Count > 0)
        {
            CCoord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.m_iTileX - 1; x <= tile.m_iTileX + 1; x++)
            {
                for (int y = tile.m_iTileY - 1; y <= tile.m_iTileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.m_iTileY || x == tile.m_iTileX))
                    {
                        if (mapFlags[x, y] == 0 && m_arrMap[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new CCoord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int a_X, int a_Y)
    {
        return a_X >= 0 && a_X < m_iWidth && a_Y >= 0 && a_Y < m_iHeight;
    }


    void RandomFillMap()
    {
        if (m_bUseRandomSeed)
        {
            m_strSeed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(m_strSeed.GetHashCode());

        for (int x = 0; x < m_iWidth; x++)
        {
            for (int y = 0; y < m_iHeight; y++)
            {
                if (x == 0 || x == m_iWidth - 1 || y == 0 || y == m_iHeight - 1)
                {
                    m_arrMap[x, y] = 1;
                }
                else
                {
                    m_arrMap[x, y] = (pseudoRandom.Next(0, 100) < m_iRandomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < m_iWidth; x++)
        {
            for (int y = 0; y < m_iHeight; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    m_arrMap[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    m_arrMap[x, y] = 0;

            }
        }
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
                        wallCount += m_arrMap[neighbourX, neighbourY];
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
}
