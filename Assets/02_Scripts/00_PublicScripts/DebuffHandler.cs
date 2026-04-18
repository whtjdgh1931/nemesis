using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DebuffHandler : MonoBehaviour
{
    public static event Action<DebuffHandler> OnDebuff;

    [SerializeField]
    private Dictionary<string, ActiveDebuff> activeDebuffs = new Dictionary<string, ActiveDebuff>();
    private CharacterModelBase character;
    private NavMeshAgent agent;
    private float originalSpeed;
    private float originalDamage;
    private MonsterBase monster;
    private PlayerModel player;
    /// <summary>
    /// 점진되는 고통
    /// </summary>
    private Coroutine _increasePain;


    public void InitializeMonster(NavMeshAgent agent)
    {
        character = GetComponent<CharacterModelBase>();

        this.agent = agent;
        originalSpeed = agent.speed;
        monster = character.GetComponent<MonsterBase>();
        originalDamage = monster.GetAttackDamage();

    }
    public void InitializePlayer()
    {
        character = GetComponent<CharacterModelBase>();
        originalSpeed = character.moveSpeed;
        player = GetComponent<PlayerModel>();
    }

    public class DebuffData
    {
        public string debuffName;      // 디버프 이름
        public float debuffDuration;   // 지속시간
        public float debuffValue;      // 초당 대미지나 배수값
        public int maxStack = 1;           // 최대 스택

        public DebuffData(string name, float duration, float value, int Maxstack = 1)
        {
            debuffName = name;
            debuffDuration = duration;
            debuffValue = value;
            maxStack = Maxstack;
        }

        public static DebuffData CreateBurn(float duration = 5f)
        {
            return new DebuffData(Constants.DEBUFF_SLOW, duration, 0);
        }

        /// <summary>
        /// 슬로우 제작 함수
        /// </summary>
        /// <param name="duration"> 슬로우 지속시간</param>
        /// <param name="slowValue"> 슬로우 수치 %단위로 입력할것 (예 : 30% 슬로우 = 30 입력)</param>
        /// <returns></returns>
        public static DebuffData CreateSlow(float duration = 3f, float slowValue = 30f)
        {
            return new DebuffData(Constants.DEBUFF_SLOW, duration, 1 - slowValue / 100);
        }

        /// <summary>
        /// 약화 제작 함수
        /// </summary>
        /// <param name="duration">약화 지속시간</param>
        /// <param name="weakValue">약화 수치%</param>
        /// <returns></returns>
        public static DebuffData CreateWeaken(float duration = 5f, float weakValue = 30f)
        {
            return new DebuffData(Constants.DEBUFF_WEAKEN, duration, 1 - weakValue / 100);
        }

        /// <summary>
        /// 독 제작 함수
        /// </summary>
        /// <param name="duration"> 독 지속시간</param>
        /// <param name="damagePerSecond"> 독 초당 대미지</param>
        /// <returns></returns>
        public static DebuffData CreatePoison(float duration = 5f, float damagePerSecond = 5f)
        {
            return new DebuffData(Constants.DEBUFF_POISON, duration, damagePerSecond, 5);
        }

        /// <summary>
        /// 과부하 제작 함수
        /// </summary>
        /// <param name="duration"> 과부하 지속 시간</param>
        /// <param name="damagePerSecond"> 과부하 초당 대미지</param>
        /// <returns></returns>
        public static DebuffData CreateOverload(float duration = 4f, float damagePerSecond = 15f)
        {
            return new DebuffData(Constants.DEBUFF_OVERLOAD, duration, damagePerSecond, 5);
        }

        /// <summary>
        /// 스턴 제작 함수
        /// </summary>
        /// <param name="duration"> 스턴 지속시간</param>
        /// <returns></returns>
        public static DebuffData CreateStun(float duration = 2f)
        {
            return new DebuffData(Constants.DEBUFF_STUN, duration, 0f);
        }

        /// <summary>
        /// 혼란 제작 함수.
        /// </summary>
        /// <param name="duration"> 혼란 지속시간 </param>
        public static DebuffData CreateConfusion(float duration = 3f)
        {
            return new DebuffData(Constants.DEBUFF_CONFUSION, duration, 0f);
        }

        /// <summary>
        /// 속박 제작 함수.
        /// </summary>
        /// <param name="duration"> 속박 지속 시간</param>
        /// <returns></returns>
        public static DebuffData CreateBinding(float duration = 3f)
        {
            return new DebuffData(Constants.DEBUFF_BINDING, duration, 0f);
        }
    }

    [SerializeField]
    private class ActiveDebuff
    {
        public DebuffData data;
        public float remainingTime;
        public float totalValue;
        public int stackCount;
        public Coroutine routine;
        public Coroutine effectRoutine; // 스턴, 혼란, 속박 특별 관리용 코루틴 저장 변수

        public ActiveDebuff(DebuffData data)
        {
            this.data = data;
            remainingTime = data.debuffDuration;
            totalValue = data.debuffValue;
            stackCount = 1;
            this.routine = null;
            this.effectRoutine = null;
        }
    }


    /// <summary>
    /// 디버프 적용 함수
    /// </summary>
    public void ApplyDebuff(DebuffData newDebuff)
    {
        if (character == null || character.isDead)
            return;

        if(player!=null)
        {
            if(player.bIsInvincibility)
            {
                return;
            }
        }
       

        if (activeDebuffs.ContainsKey(newDebuff.debuffName))
        {
            ActiveDebuff existing = activeDebuffs[newDebuff.debuffName];

            // 스택형 디버프들 독 / 과부하
            if (newDebuff.debuffName == Constants.DEBUFF_POISON || newDebuff.debuffName == Constants.DEBUFF_OVERLOAD)
            {
                if (character.tag == Constants.TAG_PLAYER && newDebuff.debuffName == Constants.DEBUFF_POISON)
                {
                    newDebuff.maxStack = 2;
                }

                if (existing.stackCount < newDebuff.maxStack)
                {
                    existing.stackCount++;
                    existing.totalValue += newDebuff.debuffValue;
                }
                existing.remainingTime = newDebuff.debuffDuration;
            }

            // 그 외 디버프들 시간 갱신. 스턴, 혼란일 경우 기존 코루틴 중단 후 새 코루틴으로 다시 시작
            else
            {
                if (existing.remainingTime > newDebuff.debuffDuration)
                {
                    return;
                }
                existing.remainingTime = newDebuff.debuffDuration;
                existing.totalValue = newDebuff.debuffValue;

                // 스턴
                if (newDebuff.debuffName == Constants.DEBUFF_STUN)
                {
                    if (existing.effectRoutine != null)
                    {
                        StopCoroutine(existing.effectRoutine);
                    }
                    existing.effectRoutine = StartCoroutine(StunCoroutine(newDebuff.debuffDuration));
                }

                // 혼란
                else if (newDebuff.debuffName == Constants.DEBUFF_CONFUSION)
                {
                    if (existing.effectRoutine != null)
                    {
                        StopCoroutine(existing.effectRoutine);
                    }
                    // 기존 혼란 상태를 먼저 정리
                    if (monster != null)
                    {
                        monster.targetTag = Constants.TAG_PLAYER;
                        monster.SetTarget(null);
                    }
                    // 새 혼란 코루틴 시작
                    existing.effectRoutine = StartCoroutine(ConfuseCoroutine(newDebuff.debuffDuration));
                }

                // 속박
                else if (newDebuff.debuffName == Constants.DEBUFF_BINDING)
                {
                    if (existing.effectRoutine != null)
                    {
                        StopCoroutine(existing.effectRoutine);
                    }
                    existing.effectRoutine = StartCoroutine(BindCoroutine(newDebuff.debuffDuration));
                }


            }
            OnDebuff?.Invoke(this);
            return;
        }
        activeDebuffs.Add(newDebuff.debuffName, new ActiveDebuff(newDebuff));
        activeDebuffs[newDebuff.debuffName].routine = StartCoroutine(HandleDebuff(newDebuff));

        OnDebuff?.Invoke(this);

    }

    private IEnumerator HandleDebuff(DebuffData debuff)
    {
        // 혹시모를 타이밍 오류 방지용 건드리지 말것
        yield return null;

        ActiveDebuff active = activeDebuffs[debuff.debuffName];

        // 시작 시 1회만 받는 효과들
        switch (debuff.debuffName)
        {
            // 슬로우
            case Constants.DEBUFF_SLOW:
                if (agent != null)
                {
                    agent.speed = originalSpeed * active.totalValue;
                }
                break;

            // 약화
            case Constants.DEBUFF_WEAKEN:
                if (monster != null)
                {
                    monster.SetAttackDamage(originalDamage * active.totalValue);
                    monster.isWeaken = true;
                }
                break;

            // 속박
            case Constants.DEBUFF_BINDING:
                active.effectRoutine = StartCoroutine(BindCoroutine(debuff.debuffDuration));
                break;

            // 스턴
            case Constants.DEBUFF_STUN:
                active.effectRoutine = StartCoroutine(StunCoroutine(debuff.debuffDuration));
                break;

            // 혼란
            case Constants.DEBUFF_CONFUSION:
                active.effectRoutine = StartCoroutine(ConfuseCoroutine(debuff.debuffDuration));
                break;
            default:
                break;
        }

        while (active.remainingTime > 0f && !character.isDead)
        {
            switch (debuff.debuffName)
            {
                case Constants.DEBUFF_POISON:
                case Constants.DEBUFF_OVERLOAD:
                    if (character.tag == Constants.TAG_PLAYER)
                    {
                        float finalDamage = active.totalValue;
                        finalDamage /= 5f;
                        character.TakeDamage(finalDamage);
                    }
                    else
                    {
                        character.TakeDamage(active.totalValue);
                    }
                    break;
                default:
                    break;
            }

            active.remainingTime -= 1f;
            yield return new WaitForSeconds(1f);
        }

        // 해제 시 복원 - 현재 슬로우 이외의 다른 복원 요소 없음
        switch (debuff.debuffName)
        {
            // 슬로우
            case Constants.DEBUFF_SLOW:
                if (agent != null)
                {
                    agent.speed = originalSpeed;
                }
                break;
            // 약화
            case Constants.DEBUFF_WEAKEN:
                if (monster != null)
                {
                    monster.SetAttackDamage(originalDamage);
                    monster.isWeaken = false;
                }
                break;
            default:
                break;
        }

        activeDebuffs.Remove(debuff.debuffName);
    }

    #region 디버프 코루틴들

    /// <summary>
    /// 스턴 코루틴
    /// </summary>
    /// <param name="duration"> 지속시간 </param>
    private IEnumerator StunCoroutine(float duration)
    {
        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            yield break;
        }
        character.isStunned = true;

        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;

            }
        }

        yield return new WaitForSeconds(duration);


        if (agent != null && !character.isDead && !character.isBindned)
        {
            agent.isStopped = false;
        }

        character.isStunned = false;
    }

    /// <summary>
    /// 속박 코루틴
    /// </summary>
    /// <param name="duration"> 지속시간 </param>
    private IEnumerator BindCoroutine(float duration)
    {
        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            yield break;
        }
        character.isBindned = true;

        if (agent != null)
        {
            agent.isStopped = true;
        }

        yield return new WaitForSeconds(duration);

        if (agent != null && !character.isDead)
        {
            agent.isStopped = false;
        }

        character.isBindned = false;
    }

    /// <summary>
    /// 혼란 코루틴
    /// </summary>
    /// <param name="duration"> 지속시간 </param>
    private IEnumerator ConfuseCoroutine(float duration)
    {
        if (monster == null) yield break;
        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            yield break;
        }

        // 혼란 상태에 들어가기 전에 원래 상태 저장
        // 이미 혼란 상태면 건너뛰기
        bool wasAlreadyConfused = monster.targetTag == Constants.TAG_MONSTER;
        string originalTag = wasAlreadyConfused ? Constants.TAG_PLAYER : monster.targetTag;
        Transform originalTarget = wasAlreadyConfused ? null : monster.GetTarget();

        float elapsedTime = 0f;

        // 혼란 상태 시작
        monster.targetTag = Constants.TAG_MONSTER;
        monster.SetTarget(null);

        while (elapsedTime < duration)
        {
            // 남은 시간 확인
            if (activeDebuffs.ContainsKey(Constants.DEBUFF_CONFUSION))
            {
                ActiveDebuff activeDebuff = activeDebuffs[Constants.DEBUFF_CONFUSION];
                if (activeDebuff.remainingTime <= 0)
                {
                    break;
                }
            }
            else
            {
                break;
            }

            // 현재 타겟이 죽었거나 없으면 새로 찾기
            Transform currentTarget = monster.GetTarget();
            if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            {
                GameObject[] monsters = GameObject.FindGameObjectsWithTag(Constants.TAG_MONSTER);
                Transform closestMonster = null;
                float closestDistance = Mathf.Infinity;

                foreach (GameObject obj in monsters)
                {
                    if (obj.transform == monster.transform || !obj.activeSelf)
                        continue;

                    float distance = Vector3.Distance(monster.transform.position, obj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestMonster = obj.transform;
                    }
                }

                if (closestMonster != null)
                {
                    monster.SetTarget(closestMonster);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 혼란 상태 복구
        if (!character.isDead && monster != null)
        {
            monster.targetTag = originalTag;

            // originalTarget이 null이면 플레이어를 자동으로 찾기
            if (originalTarget == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag(Constants.TAG_PLAYER);
                if (playerObj != null)
                {
                    monster.SetTarget(playerObj.transform);
                }
                else
                {
                    monster.SetTarget(null);
                }
            }
            else
            {
                monster.SetTarget(originalTarget);
            }
        }
    }
    #endregion

    /// <summary>
    /// 가지고있는 디버프를 확인하는 함수 (디버프 데이터로 확인)
    /// </summary>
    /// <param name="data"> 디버프 정보 </param>
    public bool CheckDebuff(DebuffData data)
    {
        return activeDebuffs.ContainsKey(data.debuffName);
    }

    /// <summary>
    /// 가지고있는 디버프를 확인하는 함수 (이름으로 확인)
    /// </summary>
    /// <param name="debuffName"> 디버프 이름 </param>
    public bool HasDebuff(string debuffName)
    {
        return activeDebuffs.ContainsKey(debuffName);
    }

    /// <summary>
    /// 가지고있는 디버프의 갯수를 반환하는 함수
    /// </summary>
    public int GetActiveDebuffCount()
    {
        int count = 0;

        foreach (KeyValuePair<string, ActiveDebuff> pair in activeDebuffs)
        {
            ActiveDebuff debuff = pair.Value;
            if (debuff != null && debuff.remainingTime > 0f)
            {
                count++;
            }
        }
        return count;
    }


    /// <summary>
    /// 가지고있는 디버프의 현재 스택을 반환하는 함수
    /// </summary>
    public int GetStackCount(string debuffName)
    {
        if (activeDebuffs.ContainsKey(debuffName))
        {
            return activeDebuffs[debuffName].stackCount;
        }
        return 0;
    }

    /// <summary>
    /// 현재 걸린 디버프를 제거하는 함수.디버프 이름을 받아 activeDebuffs에서 활성화중인 코루틴을 제거.
    /// </summary>
    public void RemoveDebuff(string debuffName)
    {
        if (activeDebuffs.ContainsKey(debuffName))
        {
            ActiveDebuff debuff = activeDebuffs[debuffName];

            if (debuff.routine != null)
            {
                StopCoroutine(debuff.routine);
            }

            if (debuff.effectRoutine != null)
            {
                StopCoroutine(debuff.effectRoutine);
            }

            // 수동 복원
            switch (debuffName)
            {
                case Constants.DEBUFF_SLOW:
                    if (agent != null)
                    {
                        agent.speed = originalSpeed;
                    }
                    break;

                case Constants.DEBUFF_WEAKEN:
                    if (monster != null)
                    {
                        monster.SetAttackDamage(originalDamage);
                        monster.isWeaken = false;
                    }
                    break;

                case Constants.DEBUFF_STUN:

                    if (!character.isDead)
                    {
                        if (agent != null)
                        {
                            agent.isStopped = false;
                        }
                        character.isStunned = false;
                    }
                    break;

                case Constants.DEBUFF_CONFUSION:
                    monster.targetTag = Constants.TAG_PLAYER;
                    break;
                case Constants.DEBUFF_BINDING:
                    if (agent != null)
                    {
                        agent.isStopped = false;
                    }
                    character.isBindned = false;
                    break;
                default:
                    break;
            }
            activeDebuffs.Remove(debuffName);
        }
    }

    /// <summary>
    /// 모든 디버프 초기화
    /// </summary>
    public void ClearAllDebuffs()
    {
        // 모든 활성 디버프를 리스트로 복사 (순회 중 삭제를 위해)
        List<string> debuffNames = new List<string>(activeDebuffs.Keys);

        // 각 디버프를 개별적으로 제거
        foreach (string debuffName in debuffNames)
        {
            RemoveDebuff(debuffName);
        }

        // 딕셔너리 완전 초기화 (혹시 모를 누락 방지)
        activeDebuffs.Clear();

        // 점진되는 고통 코루틴 중지
        if (_increasePain != null)
        {
            StopCoroutine(_increasePain);
            _increasePain = null;
        }

        // NavMeshAgent 속도 복원
        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = originalSpeed;
            agent.isStopped = false;
        }

        // 몬스터 상태 초기화
        if (monster != null)
        {
            monster.isStunned = false;
            monster.isBindned = false;
            monster.isWeaken = false;
            monster.SetAttackDamage(originalDamage);
            monster.targetTag = Constants.TAG_PLAYER;
        }

        // 플레이어 상태 초기화
        if (player != null)
        {
            player.isStunned = false;
            player.isBindned = false;
        }

        // UI 갱신 이벤트 발생
        OnDebuff?.Invoke(this);
    }


    /// <summary>
    /// 스킬 점진되는 고통 적용 함수
    /// </summary>
    public void IncreasePain(DebuffHandler debuffHandler)
    {
        if (debuffHandler.character.tag == Constants.TAG_PLAYER)
        {
            return;
        }

        // 점진되는 고통이 적용 중이 아니라면
        if (debuffHandler._increasePain == null)
        {
            debuffHandler._increasePain = StartCoroutine(IncreasePainCoroutine(debuffHandler));
        }
    }

    public IEnumerator IncreasePainCoroutine(DebuffHandler debuffHandler)
    {
        while (debuffHandler.GetActiveDebuffCount() > 0)
        {
            debuffHandler.character.TakeDamage(debuffHandler.GetActiveDebuffCount() * 5f, null);
            yield return new WaitForSeconds(Constants.DEBUFF_TIME);
        }
        _increasePain = null;

    }

    public void ConnectIncreasePain()
    {
        OnDebuff += IncreasePain;
    }
}
