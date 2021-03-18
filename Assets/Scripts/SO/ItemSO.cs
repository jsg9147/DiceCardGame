using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public int attack;
    public int health;
    public Sprite sprite;
    public float percent; // 카드에서 뽑힐 확률
}

[CreateAssetMenu(fileName ="ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
