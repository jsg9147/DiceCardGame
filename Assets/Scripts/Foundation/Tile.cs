using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
    public Vector2 gridPos;
    public Vector3 tilePos;
    public bool onMonster;
    public bool outpost; // outpost 전초기지
    public bool canSpawn;

    public bool canSelect;
    public bool isMyMonster;

    public Sprite sprite; // 차후 맵 만들때 쓰일듯

    public GameObject flag;
}

public class Tile : MonoBehaviour
{
    public TileData tileData;

    public Vector3 GridPosition()
    {
        Vector3 currentTile = new Vector3(tileData.gridPos.x, tileData.gridPos.y, 0);

        return currentTile;
    }

    public void ChangeTileColor(Color color)
    {
        transform.GetComponent<Renderer>().material.color = color;
    }

    void Start()
    {
        tileData.tilePos = this.transform.position;
    }

    private void OnMouseUp()
    {
        
    }

    private void OnMouseEnter()
    {
        if (!tileData.canSelect)
        {
            transform.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.blue;
        }
        MapManager.Inst.TileMouseEnter(this);
    }

    private void OnMouseDown()
    {
        MapManager.Inst.SetupOutpost(this);
    }

    private void OnMouseExit()
    {
        if (!tileData.canSelect)
        {
            transform.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            transform.GetComponent<Renderer>().material.color = Color.white;
            MapManager.Inst.TileMouseExit();
        }        
    }
}
