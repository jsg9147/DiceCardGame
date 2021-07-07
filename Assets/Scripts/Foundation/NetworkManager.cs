using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using ExitGames.Client.Photon;
using Photon.Realtime;

#pragma warning disable 649

public class NetworkManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    public static NetworkManager Inst { get; private set; }

    public EntityManager entityManager;
    public GameManager gameManager;

    public int turnCount;
    public int myActorNumber;

    PunTurnManager turnManager;

    PhotonManager pm;

    private void Awake()
    {
#if UNITY_EDITOR
        PhotonNetwork.AutomaticallySyncScene = true;
#endif
        Inst = this;

        Screen.SetResolution(960, 540, false);

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
#if UNITY_EDITOR
        Connect();
#endif

        this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.TurnDuration = 60f;

        GameManager.Inst.UISetup();
    }

    private void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    // 임시 테스트용 코드
    #region Unity Test

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke(true);

        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.OnAddCard?.Invoke(false);

        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();

        if (Input.GetKeyDown(KeyCode.Keypad4))
            //CardManager.Inst.TryPutCard(false);

        if (Input.GetKeyDown(KeyCode.Keypad5))
            EntityManager.Inst.DamageBoss(true, 19);

        if (Input.GetKeyDown(KeyCode.Keypad6))
            EntityManager.Inst.DamageBoss(false, 19);

        if (Input.GetKeyDown(KeyCode.Keypad7))
            GameManager.Inst.StartGame();

    }

    public void Connect()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogError($"Connected to server. Looking for random room");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError($"Joining random room failed because of {message}. Creating a new one.");
        PhotonNetwork.CreateRoom(null);
    }

    public override void OnJoinedRoom()
    {
        Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room");
        TurnSetup();
        // GameManager.Inst.StartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError($"Player {newPlayer.ActorNumber} entered the room");
    }
    #endregion

    #region TurnManager Callbacks
    public void OnTurnBegins(int turn)
    {
        // Debug.Log("OnTurnBegins() turn: " + turn);
        turnCount = turn;
        TurnManager.Inst.StartTurn();
    }
    public void OnTurnCompleted(int obj)
    {
        Debug.Log("OnTurnCompleted: " + obj);
        //TurnManager.Inst.EndTurn();
    }
    public void OnPlayerMove(Player photonPlayer, int turn, object move)
    {
        Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
        turnCount = turn;
        throw new System.NotImplementedException();
    }

    public void OnPlayerFinished(Player photonPlayer, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);
        turnCount = turn;
    }

    public void OnTurnTimeEnds(int obj)
    {
        Debug.Log("시간 끝났다! 근데 구현 안했다!");
        throw new System.NotImplementedException();
    }

    // 라이프 갱신할 함수
    private void UpdateLife()
    {

    }
    #endregion

    #region Gameplay Methods

    // 테스트를 위해 놔둠, 포톤 매니저에서 실행함
    public void TurnSetup()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int playerCount = PhotonNetwork.PlayerList.Length;
            int[] order = new int[playerCount];
            for(int i = 0; i < playerCount; i++)
            {
                order[i] = i+1;                
            }

            Shuffle(order);
            for(int i = 0; i < playerCount; i++)
            {
                PhotonNetwork.PlayerList[i].SetCustomProperties(new Hashtable { { "Order", order[i] } });
            }
        }
        FindActorNumber();
    }
    // 이거도 혹시 모르지만 포톤매니저에서 사용중
    void Shuffle (int[] deck)
    {
        for (int i = 0; i < deck.Length; i++)
        {
            int temp = deck[i];
            int randomIndex = Random.Range(0, deck.Length);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public int FindActorNumber()
    {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                myActorNumber = i;
        }

        return myActorNumber;
    }

    public void TurnCount()
    {
        this.turnManager.BeginTurn();
    }

    // 카드 소환시 포톤을 사용해 상대에게도 적용 시키는 함수
    public void SpwanCard(int cardId, Vector3 spawnPos, Vector2 gridData)
    {
        object[] datas = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, cardId, spawnPos , gridData};

        PhotonNetwork.RaiseEvent((byte)PhotonEventData.Code.CardSpawn, datas,RaiseEventOptions.Default, SendOptions.SendUnreliable);
    }

    public void SelectOutpost(bool selectOutpost, Vector3 gridPos)
    {
        object[] datas = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, selectOutpost , gridPos};

        PhotonNetwork.RaiseEvent((byte)PhotonEventData.Code.OutPost, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
    }

    public void MoveCard(int cardIdx, Vector2 tileGridPos)
    {
        object[] datas = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, cardIdx, tileGridPos };

        PhotonNetwork.RaiseEvent((byte)PhotonEventData.Code.MoveCard, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
    }

    public void Attack(int attackerIdx, int defenderIdx)
    {
        object[] datas = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, attackerIdx, defenderIdx };

        PhotonNetwork.RaiseEvent((byte)PhotonEventData.Code.Attack, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
    }

    #endregion
}
