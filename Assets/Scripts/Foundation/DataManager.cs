using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public List<CardData> cardData = new List<CardData>();

    public int[] deckData;

    public static DataManager Inst { get; set; }
    void Awake() => Inst = this;

    

    private void Start()
    {
        LoadCardDataFromJson();
        LoadDeckDataFromFirebase();
    }

    void LoadCardDataFromJson()
    {
        string path = Path.Combine(Application.streamingAssetsPath + "/card_definitions.json");
        string jsonData = File.ReadAllText(path);
        cardData = JsonConvert.DeserializeObject<List<CardData>>(jsonData);
    }

    void LoadDeckDataFromFirebase()
    {
        deckData = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0 };
    }

    public int FindCardData(int cardId)
    {
        int cardIdx = 0;
        for(int i = 0; i < cardData.Count; i++)
        {
            if (cardId == cardData[i].cardId)
                cardIdx = i;
        }

        return cardIdx;
    }
}
