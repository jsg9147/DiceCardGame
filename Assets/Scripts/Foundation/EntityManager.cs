using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using ExitGames.Client.Photon;

public class EntityManager : MonoBehaviourPun
{
    public static EntityManager Inst { get; set; }
    private void Awake()
    {
        Inst = this;
    }

    [SerializeField] GameObject entityPrefab;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;
    [SerializeField] List<Tile> onTile;
    [SerializeField] GameObject TargetPicker;
    [SerializeField] Entity myEmptyEntity;
    [SerializeField] Entity mySelectEntity;
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;

    const int MAX_ENTITY_COUNT = 10;

    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistTargetPickEntity => targetPickEntity != null;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
    bool OnSelectTile => selectTile != null;
    int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
    bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading;

    bool FindCanMovePos => canMovePos.Exists(x => x == moveEntityPos);

    CardPossibleMove cardPossibleMove = new CardPossibleMove();

    Entity selectEntity;
    Entity targetPickEntity;
    Tile selectTile;

    // 포톤으로 카드소환 변수 받아오는 변수
    public int getActorNum;
    public int cardId;
    public Vector3 AliSpawnPos;
    Vector2 getGridData;

    public int cardIdx;
    public Vector2 otherMoveGridPos;

    public int attackerIdx, defenderIdx;

    public Vector2 moveEntityPos;
    // 이동 가능한곳 저장하기 위한 리스트
    List<Vector2> canMovePos = new List<Vector2>();

    WaitForSeconds delay1 = new WaitForSeconds(1);
    WaitForSeconds delay2 = new WaitForSeconds(2);


    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;        
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworikingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworikingClient_EventReceived;        
    }

    void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);
#if false
        if (!myTurn)
            StartCoroutine(AICo());
#endif
    }

    private void Update()
    {
        ShowTargetPicker(ExistTargetPickEntity);
    }

#if false
    IEnumerator AICo()
    {
        yield return delay1;

        CardManager.Inst.TryPutCard(false);
        yield return delay1;

        // attackable이 true인 모든 otherEntities를 가져와 섞음
        var attackers = new List<Entity>(otherEntities.FindAll(x => x.attackable == true));
        for (int i =0; i< attackers.Count; i++)
        {
            int rand = Random.Range(i, attackers.Count);
            Entity temp = attackers[i];
            attackers[i] = attackers[rand];
            attackers[rand] = temp;
        }

        // 보스를 포함한 myEntities를 랜덤하게 시간차 공격
        foreach (var attacker in attackers)
        {
            var defenders = new List<Entity>(myEntities);
            defenders.Add(myBossEntity);
            int rand = Random.Range(0, defenders.Count);
            Attack(attacker, defenders[rand]);

            if (TurnManager.Inst.isLoading)
                yield break;

            yield return new WaitForSeconds(2);
        }

        TurnManager.Inst.EndTurn();

}
#endif


    // 카드 정렬 함수, 현재 타일에서 좌표를 받아와서 카드를 소환 할수 있게 해야함
    void EntityAlignment(bool isMine, Vector3 spawnPos)
    {
#if false
        float targetY = isMine ? -4.35f : 4.15f; // isMine 이라면 필드에서 내 필드에 존재하도록 하는 좌표, 이후 내 방식에 맞춰서 값을 가져가게 해야함 변경 필요
        var targetEntities = isMine ? myEntities : otherEntities;

        for(int i =0; i < targetEntities.Count; i++)
        {
            float targetX = (targetEntities.Count - 1) * -3.4f + i * 6.8f; // 리스트에 따라 가로로 정렬하는 방법, 추후 변경 필요 

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
            targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
        }
#endif
        float targetX = spawnPos.x;
        float targetY = spawnPos.y;

        if (selectTile != null)
        {
            targetX = selectTile.tileData.tilePos.x;
            targetY = selectTile.tileData.tilePos.y;
        }

        var targetEntities = isMine ? myEntities : otherEntities;
        
        // 공격할때도 생각 해야하나?
        var targetEntity = targetEntities[targetEntities.Count - 1];
        targetEntity.originPos = new Vector3(targetX, targetY, 0);
        targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
        targetEntity.GetComponent<Order>()?.SetOriginOrder(10);

        AliSpawnPos = targetEntity.originPos;
    }

    // 카드를 이동 시키는 함수
    void MoveToPoint(bool isMine, int cardIdx, Vector2 tileGridPos)
    {
        var targetEntity = isMine ? myEntities[cardIdx] : otherEntities[cardIdx];

        moveEntityPos = tileGridPos;
        
        if(!isMine)
            EntityMobility(targetEntity, isMine);

        if (!FindCanMovePos)
            return;

        int row = (int)tileGridPos.x;
        int col = (int)tileGridPos.y;
        Tile targetTile = MapManager.Inst.mapData[row, col];
        Vector2 movePoint = targetTile.tileData.tilePos;

        if(targetEntity.attackable && !targetTile.tileData.onMonster)
        {          
            targetEntity.originPos = movePoint;
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
            targetEntity.GetComponent<Order>()?.SetOriginOrder(10);

            targetEntity.attackable = false;

            // 전투시 ChangeOnMonster 동작하면 안댐
            MapManager.Inst.ChangeOnMonster(targetEntity.gridPosOfTile);
            MapManager.Inst.ChangeOnMonster(tileGridPos);
            MapManager.Inst.MonsterIsMineChange(tileGridPos, targetEntity.isMine);

            targetEntity.gridPosOfTile = tileGridPos;
            NetworkManager.Inst.MoveCard(cardIdx, tileGridPos);
        }
        // 폰은 대각선으로 움직이는데 그거 만들어줘야함
    }

    void EntityMobility(Entity entity, bool isMine)
    {
        canMovePos.Clear();
        entity.canAttackPos.Clear();
        int mapSize = MapManager.Inst.mapSize - 1;
        Tile[,] mapData = MapManager.Inst.mapData;
        int xPos = (int)entity.gridPosOfTile.x;
        int yPos = (int)entity.gridPosOfTile.y;
        bool[,] possibleMove;

        switch (entity.item.type)
        {
            case Item.Type.pawn:
                possibleMove = cardPossibleMove.PawnMove(isMine, entity.gridPosOfTile, entity.moveCount);
                AddCanMovePos(possibleMove);

                break;

            case Item.Type.rook:
                possibleMove = cardPossibleMove.RookMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);

                break;

            case Item.Type.bishop:
                possibleMove = cardPossibleMove.BishopMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);

                break;

            case Item.Type.knight:
                possibleMove = cardPossibleMove.KnightMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);

                break;

            case Item.Type.king:
                possibleMove = cardPossibleMove.KingMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);

                break;

            case Item.Type.queen:
                possibleMove = cardPossibleMove.RookMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);
                possibleMove = cardPossibleMove.BishopMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);
                canMovePos = canMovePos.Distinct().ToList();

                break;

            case Item.Type.test:
                possibleMove = cardPossibleMove.RookMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);
                possibleMove = cardPossibleMove.BishopMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);
                possibleMove = cardPossibleMove.KnightMove(isMine, entity.gridPosOfTile);
                AddCanMovePos(possibleMove);
                canMovePos = canMovePos.Distinct().ToList();

                break;

            default:
                Debug.Log("지정되지 않은 형식");
                break;
        }
    }

    void AddCanMovePos(bool [,] possibleMove)
    {
        int mapSize = MapManager.Inst.mapSize;
        for(int i = 0; i < mapSize; i++)
        {
            for(int j = 0; j < mapSize; j++)
            {
                if (possibleMove[i, j])
                    canMovePos.Add(new Vector2(i, j));
            }
        }
    }

    public void InsertSelectTile(Tile tile)
    {
        selectTile = tile;
    }

    // 마우스를 필드에 드래그 했을때 빈 게임 오브젝트를 없으면 생성해서 x좌표에 따라 리스트 순서를 바꿔주는 함수
    public void InsertMyEmptyEntity(float xPos)
    {
        if (IsFullMyEntities)
            return;

        if (!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntity);

        //기존 x좌표에 따라 리스트를 정렬해주던것 삭제 예정
#if false
        Vector3 emptyEntityPos = myEmptyEntity.transform.position;
        emptyEntityPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntityPos;

        int _emptyEntityIndex = MyEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));
        if (MyEmptyEntityIndex != _emptyEntityIndex)
            EntityAlignment(true);
#endif
    }

    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;

        myEntities.RemoveAt(MyEmptyEntityIndex);
        
        // 기존 카드 파괴시 정렬되는것
        // EntityAlignment(true);
    }

    public bool SpawnEntity(bool isMine, Vector3 spawnPos, int cardId)
    {
        if(isMine)
        {
            if (selectTile == null)
                return false;

            if (!selectTile.tileData.canSpawn)
                return false;

            if (selectTile.tileData.onMonster || !OnSelectTile)
                return false;

            if (IsFullMyEntities || !ExistMyEmptyEntity)
                return false;
        }
        else
        {
            if (IsFullOtherEntities)
                return false;

            spawnPos.y = -spawnPos.y;
        }

        var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>();

        if (isMine)
        {
            myEntities[MyEmptyEntityIndex] = entity;
            entity.idx = myEntities.IndexOf(entity);
            entity.gridPosOfTile = selectTile.tileData.gridPos;

            selectTile.tileData.onMonster = true;
        }
        else
        {
            otherEntities.Add(entity);
            entity.idx = otherEntities.IndexOf(entity);

            int x = (int)getGridData.x;
            int y = (int)getGridData.y;
            int mapSize = MapManager.Inst.mapSize - 1; // 행렬 개념으로 -1 해야 크기가 맞음
            Tile targetTile = MapManager.Inst.mapData[x, mapSize - y];
            targetTile.tileData.onMonster = true;

            entity.gridPosOfTile = targetTile.tileData.gridPos;
            targetTile.tileData.isMyMonster = isMine;
        }

        entity.isMine = isMine;

        Item item = new Item(DataManager.Inst.cardData[cardId]);

        entity.Setup(item);

        EntityAlignment(isMine, spawnPos); // 여기서 카드 정렬

        if (isMine)
            NetworkManager.Inst.SpwanCard(cardId, AliSpawnPos, selectTile.tileData.gridPos);

        return true;
    }

    public void EntityMouseDown(Entity entity)
    {
        if (!CanMouseInput)
            return;

        if(entity.isMine)
            EntityMobility(entity, entity.isMine);

        selectEntity = entity;
    }

    public void EntityMouseUp()
    {
        if (!CanMouseInput)
            return;

        selectEntity = null;
        targetPickEntity = null;
    }

    public void EntityMouseUP(Entity entity)
    {
        if (!CanMouseInput)
            return;

        // selectEntity, targetPickEntity 둘다 존재하면 공격한다. 바로 null, null 로 만든다.
        // 지 이동능력 무시하고 공격 가능하니 수정필요

#if false
        if (selectEntity && targetPickEntity && selectEntity.attackable)
            Attack(selectEntity, targetPickEntity);
#endif
        if (selectEntity && selectEntity.attackable)
        {
            if (targetPickEntity)
                Attack(selectEntity, targetPickEntity);
            else
            {
                if(selectTile != null)
                    MoveToPoint(entity.isMine, entity.idx, selectTile.tileData.gridPos);
            }
        }

        selectEntity = null;
        targetPickEntity = null;
    }

    public void EntityMouseDrag()
    {
        if (!CanMouseInput || selectEntity == null)
            return;

        // ohter 타겟 엔티티 찾기
        bool existTarget = false;
        foreach(var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();
            if (entity != null && !entity.isMine && selectEntity.attackable && canMovePos.Exists(x => x == entity.gridPosOfTile))
            {
                targetPickEntity = entity;
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPickEntity = null;
    }

    public void RemoveSelectTile()
    {
        selectTile = null;
    }

    private void ShowTargetPicker(bool isShow)
    {
        TargetPicker.SetActive(isShow);
        
        if (ExistTargetPickEntity)
            TargetPicker.transform.position = targetPickEntity.transform.position;
    }

    void Attack(Entity attacker, Entity defender)
    {
        // _attacker가 _defender의 위치로 이동하다 원래 위치로 돌아온다, 이때 order가 제일 높다
        attacker.attackable = false;
        attacker.GetComponent<Order>().SetMostFrontOrder(true);        

        // 시퀀스 개념 좀더 정의 필요
        Sequence sequence = DOTween.Sequence()
            .Append(attacker.transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine)
            .AppendCallback(() =>
            {
                attacker.Damaged(defender.attack);
                defender.Damaged(attacker.attack);
                attacker.GetComponent<Order>().SetMostFrontOrder(false);
                SpawnDamage(defender.attack, attacker.transform);
                SpawnDamage(attacker.attack, defender.transform);
            })
            .Append(attacker.transform.DOMove(attacker.originPos, 0.4f)).SetEase(Ease.OutSine)
            .OnComplete(() => AttackCallback(attacker, defender)); //죽음 처리

        if(attacker.isMine)
        {
            int myEntityIdx, otherEntityIdx;
            myEntityIdx = myEntities.FindIndex(x => x == attacker);
            otherEntityIdx = otherEntities.FindIndex(x => x == defender);

            NetworkManager.Inst.Attack(myEntityIdx, otherEntityIdx);
        }
    }

    void AttackCallback(params Entity[] entities)
    {
        entities[0].GetComponent<Order>().SetMostFrontOrder(false);

        foreach (var entity in entities)
        {
            if (!entity.isDie || entity.isBossOrEmpty)
                continue;

            if (entity.isMine)
                myEntities.Remove(entity);
            else
                otherEntities.Remove(entity);

            Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(1.3f))
                .Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    // 이부분은 이해가 필요

                    //EntityAlignment(entity.isMine);
                    Destroy(entity.gameObject);
                });
        }
        StartCoroutine(CheckBossDie());
    }

    public void AttackableReset(bool isMine)
    {
        var targetEntities = isMine ? myEntities : otherEntities;
        targetEntities.ForEach(x => x.attackable = true);
    }

    void SpawnDamage(int damage, Transform tr)
    {
        /*
        if (damage <= 0)
            return;
        */
        var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }

    IEnumerator CheckBossDie()
    {
        yield return delay2;

        if (myBossEntity.isDie)
            StartCoroutine(GameManager.Inst.GameOver(false));

        if (otherBossEntity.isDie)
            StartCoroutine(GameManager.Inst.GameOver(true));
    }

    public void DamageBoss(bool isMine, int damage)
    {
        var targetBossEntity = isMine ? myBossEntity : otherBossEntity;
        targetBossEntity.Damaged(damage);
        StartCoroutine(CheckBossDie());
    }

#region Photon
    
    private void NetworikingClient_EventReceived(EventData obj)
    {
        if(obj.Code == (byte)PhotonEventData.Code.CardSpawn)
        {
            object[] datas = (object[])obj.CustomData;
            getActorNum = (int)datas[0];
            cardId = (int)datas[1];
            AliSpawnPos = (Vector3)datas[2];
            getGridData = (Vector2)datas[3];

            if (getActorNum != PhotonNetwork.LocalPlayer.ActorNumber)
                CardManager.Inst.TryPutCard(false, cardId);

        }

        if (obj.Code == (byte)PhotonEventData.Code.MoveCard)
        {
            object[] datas = (object[])obj.CustomData;
            getActorNum = (int)datas[0];
            cardIdx = (int)datas[1];
            otherMoveGridPos = (Vector2)datas[2];

            otherMoveGridPos.y = MapManager.Inst.mapSize - otherMoveGridPos.y - 1;

            if (getActorNum != PhotonNetwork.LocalPlayer.ActorNumber)
                MoveToPoint(false, cardIdx, otherMoveGridPos);
        }

        if (obj.Code == (byte)PhotonEventData.Code.Attack)
        {
            object[] datas = (object[])obj.CustomData;
            getActorNum = (int)datas[0];
            attackerIdx = (int)datas[1];
            defenderIdx = (int)datas[2];

            Entity attacker, defender;
            attacker = otherEntities[defenderIdx];
            defender = myEntities[attackerIdx];

            if (getActorNum != PhotonNetwork.LocalPlayer.ActorNumber)
                Attack(attacker, defender);
        }
    }

#endregion
}
