using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

using System.IO;
using System.Text;
using Newtonsoft.Json;

using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
    [SerializeField] List<Card> otherCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform otherCardSpawnPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] Transform otherCardLeft;
    [SerializeField] Transform otherCardRight;
    [SerializeField] EcardState ecardState;

    List<Item> itemBuffer;
    Card selectCard;
    bool isMyCardDrag;
    bool onMyCardArea;
    int myPutCount;
    enum EcardState { Nothing, CanMouseOver, CanMouseDrag }

    int[] deckCard;

    public Item PopItem()
    {
        // 0개가 되면 다시 100개를 채워 넣는 코드, 추후 수정 jsg
        if (itemBuffer.Count == 0)
            SetupItemBuffer();
        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }
    void SetupItemBuffer()
    {

#if false
        itemBuffer = new List<Item>(deckCard.Length);
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
                itemBuffer.Add(item);
        }

        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
#endif
        int[] deckData = DataManager.Inst.deckData;
        itemBuffer = new List<Item>(deckData.Length);

        List<CardData> cardDatas = DataManager.Inst.cardData;

        for (int i = 0; i < deckData.Length; i++)
        {
            Item item = new Item(cardDatas[deckData[i]]);
            itemBuffer.Add(item);
        }

        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }

    void AddCard(bool isMine)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), isMine);
        (isMine ? myCards : otherCards).Add(card);
        SetOriginOrder(isMine);
        CardAlignment(isMine);
    }

    //카드 뒷장일때의 OrderLayer를 + 해서 앞장만 보이게 하는 함수
    void SetOriginOrder(bool isMine)
    {
        int count = isMine ? myCards.Count : otherCards.Count;
        for(int i = 0; i < count; i++)
        {
            var targetCard = isMine ? myCards[i] : otherCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i); //타겟 카드가 존재를 한다면
        }
    }

    void CardAlignment(bool isMine)
    {
        List<PRS> originCardPRSs = new List<PRS>();
        if (isMine)
            originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);
        else
            originCardPRSs = RoundAlignment(otherCardLeft, otherCardRight, otherCards.Count, -0.5f, Vector3.one * 1.9f);

        var targetCards = isMine ? myCards : otherCards;
        for(int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> result = new List<PRS>(objCount);

        switch(objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Quaternion.identity;
            if(objCount >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            result.Add(new PRS(targetPos, targetRot, scale));
        }

        return result;
    }

    private void CardDrag()
    {
        if (!onMyCardArea)
        {
            selectCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectCard.originPRS.scale), false);
            EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    // 소환
    public bool TryPutCard(bool isMine, int cardId)
    {
        if (isMine && myPutCount >= 10) // 카드를 이미 냈으면 카드를 못내게 하는것 지금은 1장 추후 삭제예정
            return false;

        if (!isMine && otherCards.Count <= 0) // 다른사람의 카드 카운트
            return false;

        //여기 정의 다시 생각해보자
        Card card = isMine ? selectCard : otherCards[Random.Range(0, otherCards.Count)];

        var spawnPos = Utils.MousePos;
        var targetCards = myCards;

        if (isMine)
        {
            cardId = selectCard.item.cardNum;
            spawnPos = Utils.MousePos;
            targetCards = myCards;
        }
        else
        {
            spawnPos = EntityManager.Inst.AliSpawnPos;
            targetCards = otherCards;
        }

        if(EntityManager.Inst.SpawnEntity(isMine, spawnPos, cardId)) // 그냥 카드를 밖으로 꺼내면 
        {
            targetCards.Remove(card);
            card.transform.DOKill();
            DestroyImmediate(card.gameObject);
            if(isMine)
            {
                selectCard = null;
                myPutCount++;
            }
            CardAlignment(isMine); 
            return true;
        }

        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            CardAlignment(isMine);
            return false;
        }
    }

    void OnTurnStarted(bool myTurn)
    {
        if (myTurn)
            myPutCount = 0;
    }

    private void Start()
    {
        SetupItemBuffer();
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    private void Update()
    {
        if (isMyCardDrag)
            CardDrag();

        DetectCardArea();
        SetEcardState();
    }



#region MyCard

    public void CardMouseOver(Card card)
    {
        if (ecardState == EcardState.Nothing)
            return;

        selectCard = card;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        EnlargeCard(false, card);
    }

    public void CardMouseDown()
    {
        if (ecardState != EcardState.CanMouseDrag)
            return;

        isMyCardDrag = true;
    }

    public void CardMouseUp()
    {
        isMyCardDrag = false;

        if (ecardState != EcardState.CanMouseDrag)
            return;

        if (onMyCardArea)
            EntityManager.Inst.RemoveMyEmptyEntity();
        else
            TryPutCard(true, selectCard.cardData.cardId);
    }

    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    private void SetEcardState()
    {
        if (TurnManager.Inst.isLoading)
            ecardState = EcardState.Nothing;

#if false
        else if (!TurnManager.Inst.myTurn || myPutCount == 1 || EntityManager.Inst.IsFullMyEntities) // 카드가 최대치 나가있거나 한턴 사용 카드 다 썼거나
            ecardState = EcardState.CanMouseOver;
#endif

        else if (!TurnManager.Inst.myTurn || EntityManager.Inst.IsFullMyEntities) // 기존 코드에서 내턴 카드 제한을 풀었음
            ecardState = EcardState.CanMouseOver;

        else if (TurnManager.Inst.myTurn && myPutCount == 0)
            ecardState = EcardState.CanMouseDrag;
    }

#endregion
}
