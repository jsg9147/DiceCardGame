using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Inst { get; private set; }
    void Awake() => Inst = this;

    public PRS originPRS;
    [SerializeField] Tile selectTile;

    public GameObject TilePrefab;
    public GameObject ParentTile;
    public int mapSize; // 지금 7로 에디터에 입력해놨음
    public int tilePrefabSize = 3;

    public Tile[,] mapData;

    List<List<Tile>> map = new List<List<Tile>>();

    int outPostCount;
    bool selectOutpostComplete;


    // mapSize -1 을 해야하는데 그냥했음 추후 오류 가능성 있으니 무조건 수정해야함
    
    private void Start()
    {
        Setup();
        generateMap();
        ChangeColorOutOfTile(Color.red);
    }

    private void Setup()
    {
        mapData = new Tile[mapSize, mapSize];
        selectOutpostComplete = false;
        outPostCount = 0;
    }

    void generateMap()
    {
        map = new List<List<Tile>>();
        for (int i = 0; i < mapSize; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < mapSize; j++)
            {
                Tile tile = ((GameObject)Instantiate(TilePrefab, new Vector3((i - Mathf.Floor(mapSize / 2)) * tilePrefabSize, (j
                    - Mathf.Floor(mapSize / 2)) * tilePrefabSize, 1), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
                tile.transform.parent = ParentTile.transform;
                tile.tileData.gridPos = new Vector2(i, j);
                mapData[i, j] = tile;
                row.Add(tile);
                SetupMapData(i, j);
            }
            map.Add(row);
        }
    }

    void SetupMapData(int row , int col)
    {
        mapData[row, col].tileData.onMonster = false;
        mapData[row, col].tileData.canSpawn = false;
        mapData[row, col].tileData.canSelect = true;
        mapData[row, col].tileData.gridPos = new Vector3(row, col, 0);
    }

    void ChangeColorOutOfTile(Color color)
    {
        for(int i = 0; i < mapSize; i++)
        {
            for(int j = mapSize; j > (mapSize / 2); j--)
            {
                mapData[i, j - 1].ChangeTileColor(color);

                if(color == Color.red)
                    mapData[i, j - 1].tileData.canSelect = false;

                else
                    mapData[i, j - 1].tileData.canSelect = true;

            }
        }
    }

    public void SetupOutpost(Tile tile)
    {
        if (selectOutpostComplete)
            return;

        if (tile.tileData.gridPos.y >= mapSize / 2)
            return;

        selectTile = tile;
        selectTile.tileData.outpost = true;
        outPostCount++;

        SetupSpawnPost(selectTile.tileData.gridPos);
        SetFlag(selectTile.tileData.gridPos, true);

        if (outPostCount <= 2)
        {
            selectOutpostComplete = false;            
            NetworkManager.Inst.SelectOutpost(selectOutpostComplete, selectTile.tileData.gridPos);
        }
        else
        {
            selectOutpostComplete = true;
            ChangeColorOutOfTile(Color.white);
            NetworkManager.Inst.SelectOutpost(selectOutpostComplete, selectTile.tileData.gridPos);
            TurnManager.Inst.SelectOutpost();
        }
    }


    // 지금은 깃발 이미지가 같지만 상대편 깃발 이미지는 다르게 할 예정
    public void SetFlag(Vector3 gridPos, bool isMine)
    {
        int mapMaxPos = mapSize - 1;
        if(isMine)
            mapData[(int)gridPos.x, (int)gridPos.y].tileData.flag.SetActive(true);
        else
            mapData[(int)gridPos.x, mapMaxPos - (int)gridPos.y].tileData.flag.SetActive(true);
    }

    void SetupSpawnPost(Vector3 gridPos)
    {
        int minX = (int)gridPos.x - 1;
        int minY = (int)gridPos.y - 1;
        int maxX = (int)gridPos.x + 1;
        int maxY = (int)gridPos.y + 1;

        if (minX <= 0)
            minX = 0;

        if (minY <= 0)
            minY = 0;

        if (maxX >= mapSize)
            maxX = mapSize - 1;

        if (maxY >= mapSize)
            maxY = mapSize - 1;

        for(int i = minX; i <= maxX; i++)
        {
            for(int j = minY; j <= maxY; j++)
            {
                mapData[i, j].tileData.canSpawn = true;
            }
        }
    }

    public void ChangeOnMonster(Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int y = (int)gridPos.y;
        mapData[x, y].tileData.onMonster = !mapData[x, y].tileData.onMonster;
    }

    public void MonsterIsMineChange(Vector2 gridPos,bool isMine)
    {
        int x = (int)gridPos.x;
        int y = (int)gridPos.y;
        mapData[x, y].tileData.isMyMonster = isMine;
    }
    #region Tile

    public void TileMouseEnter(Tile tile)
    {   
        selectTile = tile;
        EntityManager.Inst.InsertSelectTile(selectTile);
    }

    public void TileMouseExit()
    {
        EntityManager.Inst.RemoveSelectTile();
    }

    public void DestroyOnTile(Tile tile)
    {
        tile.tileData.onMonster = false;
    }

    #endregion
}
