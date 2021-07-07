using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public enum Type
    {
        pawn,
        rook,
        bishop,
        knight,
        queen,
        king,
        magic,
        trap,
        error,
        test
    }

    public int cardNum; // 이거를 이용해서 카드 위치를 찾는게 좋을듯
    public string name;
    public Type type;
    public int atk;
    public int def;
    public int cost;
    public string explanation;
    public Sprite sprite;

    public Vector2 gridPos;
    int CurrentX, CurrentY;    

    public Item(CardData itemData)
    {
        this.cardNum = itemData.cardId;
        this.name = itemData.name;
        this.atk = itemData.atk;
        this.def = itemData.def;
        this.cost = itemData.cost;
        SetType(itemData.cardType);
        SetSprite(itemData.spriteName);
        this.explanation = itemData.explanation;
    }

    private void SetType(string cardType)
    {
        switch(cardType)
        {
            case "pawn":
                this.type = Type.pawn;
                break;
            case "rook":
                this.type = Type.rook;
                break;
            case "bishop":
                this.type = Type.bishop;
                break;
            case "knight":
                this.type = Type.knight;
                break;
            case "queen":
                this.type = Type.queen;
                break;
            case "king":
                this.type = Type.king;
                break;
            case "trap":
                this.type = Type.trap;
                break;
            case "magic":
                this.type = Type.magic;
                break;
            case "test":
                this.type = Type.test;
                break;

            default:
                this.type = Type.error;
                break;
        }
    }

    private void SetSprite(string spriteName)
    {
        sprite = Resources.Load<Sprite>("Images/" + spriteName) as Sprite;
    }

    public void SetPostion(int x, int y)
    {
        gridPos = new Vector2(x, y);
    }

    public bool[,] PawnMove()
    {
        int mapSize = MapManager.Inst.mapSize;
        Tile[,] mapData = MapManager.Inst.mapData;
        bool[,] r = new bool[mapSize, mapSize];

        return r;
    }
    
}
