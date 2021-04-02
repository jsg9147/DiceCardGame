using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }
    private void Awake()
    {
        Inst = this;
    }

    [SerializeField] GameObject entityPrefab;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;
    [SerializeField] GameObject TargetPicker;
    [SerializeField] Entity myEmptyEntity;
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;

    const int MAX_ENTITY_COUNT = 6;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistTargetPickEntity => targetPickEntity != null;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
    int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
    bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading;

    Entity selectEntity;
    Entity targetPickEntity;
    WaitForSeconds delay1 = new WaitForSeconds(1);


    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;        
    }

    void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);

        if (!myTurn)
            StartCoroutine(AICo());
    }

    private void Update()
    {
        ShowTargetPicker(ExistTargetPickEntity);
    }

    IEnumerator AICo()
    {
        CardManager.Inst.TryPutCard(false);
        yield return delay1;

        //공격로직
        TurnManager.Inst.EndTurn();
    }

    void EntityAlignment(bool isMine)
    {
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
    }


    // 마우스를 필드에 드래그 했을때 빈 게임 오브젝트를 없으면 생성해서 x좌표에 따라 리스트 순서를 바꿔주는 함수
    public void InsertMyEmptyEntity(float xPos)
    {
        if (IsFullMyEntities)
            return;

        if (!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntity);

        Vector3 emptyEntityPos = myEmptyEntity.transform.position;
        emptyEntityPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntityPos;

        int _emptyEntityIndex = MyEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));
        if (MyEmptyEntityIndex != _emptyEntityIndex)
            EntityAlignment(true);
    }

    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;

        myEntities.RemoveAt(MyEmptyEntityIndex);
        EntityAlignment(true);
    }

    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
    {
        if(isMine)
        {
            if (IsFullMyEntities || !ExistMyEmptyEntity)
                return false;
        }
        else
        {
            if (IsFullOtherEntities)
                return false;
        }

        var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>();

        if (isMine)
            myEntities[MyEmptyEntityIndex] = entity;
        else
            otherEntities.Insert(Random.Range(0, otherEntities.Count), entity);

        entity.isMine = isMine;
        entity.Setup(item);
        EntityAlignment(isMine);

        return true;
    }

    public void EntityMouseDown(Entity entity)
    {
        if (!CanMouseInput)
            return;

        selectEntity = entity;
    }

    public void EntityMouseUp()
    {
        if (!CanMouseInput)
            return;

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
            if(entity != null && !entity.isMine && selectEntity.attackable)
            {
                targetPickEntity = entity;
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPickEntity = null;
    }

    public void EntityMouseUP()
    {
        if (!CanMouseInput)
            return;

        // selectEntity, targetPickEntity 둘다 존재하면 공격한다. 바로 null, null 로 만든다.
        if (selectEntity && targetPickEntity && selectEntity.attackable)
            Attack(selectEntity, targetPickEntity);

        selectEntity = null;
        targetPickEntity = null;
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
            .OnComplete(() => { }); //죽음 처리
    }

    public void AttackableReset(bool isMine)
    {
        var targetEntities = isMine ? myEntities : otherEntities;
        targetEntities.ForEach(x => x.attackable = true);
    }

    void SpawnDamage(int damage, Transform tr)
    {
        var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }

}
