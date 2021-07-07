using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPossibleMove
{
    int CurrentX, CurrentY;
    int mapSize;
    bool[,] r;
    public bool[,] RookMove(bool isMine, Vector2 gridPos)
    {
        mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        r = new bool[mapSize, mapSize];

        CurrentX = (int)gridPos.x;
        CurrentY = (int)gridPos.y;

        int i = CurrentX + 1;

        // Right        

        while (true)
        {
            if (i >= mapSize)
                break;

            if (!mapData[i, CurrentY].tileData.onMonster)
            {
                r[i, CurrentY] = true;
            }
            else
            {
                if (isMine != mapData[i, CurrentY].tileData.isMyMonster)
                    r[i, CurrentY] = true;

                break;
            }
            i++;
        }

        // Left        
        i = CurrentX - 1;
        while (true)
        {
            if (i < 0)
                break;

            if (!mapData[i, CurrentY].tileData.onMonster)
            {
                r[i, CurrentY] = true;
            }
            else
            {
                if (isMine != mapData[i, CurrentY].tileData.isMyMonster)
                    r[i, CurrentY] = true;

                break;
            }
            i--;
        }

        // Up        
        i = CurrentY + 1;
        while (true)
        {
            if (i >= mapSize)
                break;

            if (!mapData[CurrentX, i].tileData.onMonster)
            {
                r[CurrentX, i] = true;
            }
            else
            {
                if (isMine != mapData[CurrentX, i].tileData.isMyMonster)
                    r[CurrentX, i] = true;

                break;
            }
            i++;
        }

        // Down
        i = CurrentY - 1;
        while (true)
        {
            if (i < 0)
                break;

            if (!mapData[CurrentX, i].tileData.onMonster)
            {
                r[CurrentX, i] = true;
            }
            else
            {
                if (isMine != mapData[CurrentX, i].tileData.isMyMonster)
                    r[CurrentX, i] = true;

                break;
            }
            i--;
        }
        return r;
    }

    public bool [,] BishopMove(bool isMine, Vector2 gridPos)
    {
        mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        r = new bool[mapSize, mapSize];

        CurrentX = (int)gridPos.x;
        CurrentY = (int)gridPos.y;

        int i = CurrentX + 1;
        int j = CurrentY + 1;

        // Right Up
        while (true)
        {
            if (i >= mapSize || j >= mapSize)
                break;

            if (!mapData[i, j].tileData.onMonster)
            {
                r[i, j] = true;
            }
            else
            {
                if (isMine != mapData[i, j].tileData.isMyMonster)
                    r[i, j] = true;

                break;
            }
            i++;
            j++;
        }

        // Left Up

        i = CurrentX - 1;
        j = CurrentY + 1;
        
        while (true)
        {
            if (i < 0 || j >= mapSize)
                break;

            if (!mapData[i, j].tileData.onMonster)
            {
                r[i, j] = true;
            }
            else
            {
                if (isMine != mapData[i, j].tileData.isMyMonster)
                    r[i, j] = true;

                break;
            }
            i--;
            j++;
        }

        // Right Down

        i = CurrentX + 1;
        j = CurrentY - 1;

        while (true)
        {
            if (i >= mapSize || j < 0)
                break;

            if (!mapData[i, j].tileData.onMonster)
            {
                r[i, j] = true;
            }
            else
            {
                if (isMine != mapData[i, j].tileData.isMyMonster)
                    r[i, j] = true;

                break;
            }
            i++;
            j--;
        }

        // Left Down

        i = CurrentX - 1;
        j = CurrentY - 1;

        while (true)
        {
            if (i < 0 || j < 0)
                break;

            if (!mapData[i, j].tileData.onMonster)
            {
                r[i, j] = true;
            }
            else
            {
                if (isMine != mapData[i, j].tileData.isMyMonster)
                    r[i, j] = true;

                break;
            }
            i--;
            j--;
        }

        return r;
    }

    public bool [,] KnightMove(bool isMine, Vector2 gridPos)
    {
        mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        r = new bool[mapSize, mapSize];

        CurrentX = (int)gridPos.x;
        CurrentY = (int)gridPos.y;

        // Up 2 Right1
        KnightMoveSupport(CurrentX + 1, CurrentY +2, isMine, ref r);

        // Up 1 Right 2
        KnightMoveSupport(CurrentX + 2, CurrentY + 1, isMine, ref r);

        // Up 2 Left 1
        KnightMoveSupport(CurrentX - 1, CurrentY + 2, isMine, ref r);

        // Up 1 Left 2
        KnightMoveSupport(CurrentX - 2, CurrentY + 1, isMine, ref r);

        // Down 2 Right 1
        KnightMoveSupport(CurrentX + 1, CurrentY - 2, isMine, ref r);

        // Down 1 Right 2
        KnightMoveSupport(CurrentX + 2, CurrentY - 1, isMine, ref r);

        // Down 2 Left 1
        KnightMoveSupport(CurrentX - 1, CurrentY - 2, isMine, ref r);

        // Down 1 Left 2
        KnightMoveSupport(CurrentX - 2, CurrentY - 1, isMine, ref r);

        return r;
    }

    public bool [,] KingMove(bool isMine, Vector2 gridPos)
    {
        mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        r = new bool[mapSize, mapSize];

        CurrentX = (int)gridPos.x;
        CurrentY = (int)gridPos.y;

        int i, j;

        // Top Side
        i = CurrentX - 1;
        j = CurrentY + 1;
        if (CurrentY != mapSize - 1)
        {
            for (int k = 0; k < 3; k++)
            {
                if (i >= 0 || i < mapSize)
                {
                    if (!mapData[i, j].tileData.onMonster)
                        r[i, j] = true;
                    else if (isMine != mapData[i, j].tileData.onMonster)
                        r[i, j] = true;
                }
                i++;
            }
        }

        // Down Side
        i = CurrentX - 1;
        j = CurrentY - 1;
        if (CurrentY != 0)
        {
            for (int k = 0; k < 3; k++)
            {
                if (i >= 0 || i < mapSize)
                {
                    if (!mapData[i, j].tileData.onMonster)
                        r[i, j] = true;
                    else if (isMine != mapData[i, j].tileData.onMonster)
                        r[i, j] = true;
                }
                i++;
            }
        }

        // Middle Left
        if (CurrentX != 0)
        {
            if (!mapData[CurrentX - 1, CurrentY].tileData.onMonster)
                r[CurrentX - 1, CurrentY] = true;
            else if (isMine != mapData[CurrentX - 1, CurrentY].tileData.onMonster)
                r[CurrentX - 1, CurrentY] = true;
        }

        // Middle Right
        if (CurrentX != mapSize - 1)
        {
            if (!mapData[CurrentX + 1, CurrentY].tileData.onMonster)
                r[CurrentX + 1, CurrentY] = true;
            else if (isMine != mapData[CurrentX - 1, CurrentY].tileData.onMonster)
                r[CurrentX + 1, CurrentY] = true;
        }

        return r;
    }

    public void KnightMoveSupport(int x, int y,bool isMine ,ref bool [,] r)
    {
        TileData tileData;
        if (x >= 0 && x < mapSize && y >= 0 && y < mapSize)
        {
            tileData = MapManager.Inst.mapData[x, y].tileData;
            if (!tileData.onMonster)
                r[x, y] = true;
            else if (tileData.isMyMonster != isMine)
                r[x, y] = true;
        }
    }

    public bool[,] PawnMove(bool isMine, Vector2 gridPos, int moveCount)
    {
        mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        r = new bool[mapSize, mapSize];

        Tile t1, t2;

        CurrentX = (int)gridPos.x;
        CurrentY = (int)gridPos.y;

        if(isMine)
        {
            // Diagonal Left
            if(CurrentX != 0 && CurrentY < mapSize)
            {
                t1 = mapData[CurrentX - 1, CurrentY + 1];
                if (t1.tileData.onMonster && !t1.tileData.isMyMonster)
                    r[CurrentX - 1, CurrentY + 1] = true;
            }

            // Diagonal Right
            if (CurrentX < mapSize && CurrentY < mapSize)
            {
                t1 = mapData[CurrentX + 1, CurrentY + 1];
                if (t1.tileData.onMonster && !t1.tileData.isMyMonster)
                    r[CurrentX + 1, CurrentY + 1] = true;
            }

            // Middle
            if (CurrentY < mapSize)
            {
                t1 = mapData[CurrentX, CurrentY + 1];
                if (!t1.tileData.onMonster)
                    r[CurrentX, CurrentY + 1] = true;
            }

            // Middle on first move
            if(moveCount < 1)
            {
                t1 = mapData[CurrentX, CurrentY + 1];
                t2 = mapData[CurrentX, CurrentY + 2];
                if (!t1.tileData.onMonster && !t2.tileData.onMonster)
                    r[CurrentX, CurrentY + 2] = true;
            }
        }
        else
        {
            // Diagonal Left
            if (CurrentX > 0 && CurrentY > 0)
            {
                t1 = mapData[CurrentX - 1, CurrentY - 1];
                if (t1.tileData.onMonster && t1.tileData.isMyMonster)
                    r[CurrentX - 1, CurrentY - 1] = true;
            }

            // Diagonal Right
            if (CurrentX < mapSize && CurrentY > 0)
            {
                t1 = mapData[CurrentX + 1, CurrentY - 1];
                if (t1.tileData.onMonster && t1.tileData.isMyMonster)
                    r[CurrentX + 1, CurrentY - 1] = true;
            }

            // Middle
            if (CurrentY > 0)
            {
                t1 = mapData[CurrentX, CurrentY - 1];
                if (!t1.tileData.onMonster)
                    r[CurrentX, CurrentY - 1] = true;
            }

            // Middle on first move
            if (moveCount < 1)
            {
                t1 = mapData[CurrentX, CurrentY - 1];
                t2 = mapData[CurrentX, CurrentY - 2];
                if (!t1.tileData.onMonster && !t2.tileData.onMonster)
                    r[CurrentX, CurrentY - 2] = true;
            }
        }

        return r;
    }
}
