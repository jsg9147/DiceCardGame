using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Photon.Pun;
using ExitGames.Client.Photon;

using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    private void Awake()
    {
        Inst = this;
    }

    [Header("Develop")]
    [SerializeField] [Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField] [Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [SerializeField] [Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; //게임 끝나면 isLoading 을 ture로 하면 카드와 엔티티 클릭 방지
    public bool myTurn;

    enum ETurnMode { Random, My, other }
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);

    public static Action<bool> OnAddCard;
    public static event Action<bool> OnTurnStarted;

    int getActorNum;
    Vector2 gridPos;
    bool otherPlayerOutpost;
    bool myOustpostComplete;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworikingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworikingClient_EventReceived;
    }

    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        switch(eTurnMode)
        {
            case ETurnMode.Random:
                myTurn = Random.Range(0, 2) == 0;
                break;
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.other:
                myTurn = false;
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        //GameSetup();
        delay05 = new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1f);
        TurnOrder();

        isLoading = true;
        for(int i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke(false); //?. <- 이게 들어가면 Ation이 Null 일때 뒤에 부분을 실행하지 않는다.
            yield return delay05;
            OnAddCard?.Invoke(true);
        }
        StartCoroutine(StartTurnCo());
    }
    
    IEnumerator StartTurnCo()
    {
        isLoading = true;
        TurnOrder();

        if (myTurn)
            GameManager.Inst.Notification("나의 턴");

        yield return delay07;
        OnAddCard?.Invoke(myTurn);
        yield return delay07;
        isLoading = false;
        OnTurnStarted?.Invoke(myTurn);
    }

    public void EndTurn()
    {
        //myTurn = !myTurn;
        NetworkManager.Inst.TurnCount();       
    }

    public void StartTurn()
    {
        StartCoroutine(StartTurnCo());
    }

    void TurnOrder()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        int turnCount = NetworkManager.Inst.turnCount;
        int myActorNum = NetworkManager.Inst.FindActorNumber();

        if (turnCount % playerCount == (int)PhotonNetwork.PlayerList[myActorNum].CustomProperties["Order"])
        {
            myTurn = true;
        }
        else if (turnCount % playerCount == 0 && (int)PhotonNetwork.PlayerList[myActorNum].CustomProperties["Order"] == playerCount)
        {
            myTurn = true;
        }
        else
            myTurn = false;
    }


    // 나혼자 완료하면 시작을 못함, 상대가 완료한 상태에선 시작 가능
    
    public void SelectOutpost()
    {
        myOustpostComplete = true;

        if (myOustpostComplete && otherPlayerOutpost)
            GameManager.Inst.StartGame();
    }

    // 포톤에서 Null 오류 뜨는데 이유를 못찾고 있음 기능상 문제는 없으나 해결법 찾게되면 해결해야함
    private void NetworikingClient_EventReceived(EventData obj)
    {
        if (obj.Code == (byte)PhotonEventData.Code.OutPost)
        {
            object[] datas = (object[])obj.CustomData;
            getActorNum = (int)datas[0];
            otherPlayerOutpost = (bool)datas[1];
            gridPos = (Vector3)datas[2];

            if (getActorNum != PhotonNetwork.LocalPlayer.ActorNumber)
            {                
                MapManager.Inst.SetFlag(gridPos, false);                
            }

            // 내가 먼저 완료 했을시 상대가 완료신호를 보내면 시작
            if (myOustpostComplete && otherPlayerOutpost)
                GameManager.Inst.StartGame();
        }

    }
}
