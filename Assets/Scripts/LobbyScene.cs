using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyScene : BaseScene
{    
    [SerializeField]
    private TMP_Text gameNameText;
    private void Awake()
    {
        
    }
    
    private void Start()
    {
        GameObject firebase = GameObject.Find("FirebaseManager");
        gameNameText.text = firebase.GetComponent<FirebaseManager>().username;
    }
}
