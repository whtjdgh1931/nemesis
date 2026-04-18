using System.Collections.Generic;
using UnityEngine;


public class electricMan : MonoBehaviour
{
    [SerializeField] private int damage = 60;       // 충돌 시 데미지
    [SerializeField] private float damageInterval = 1f; // 같은 몬스터당 딜레이

    private SkillEffect _eletricBeingPrefab;
    private Player _player;

    // 각 몬스터별 마지막 데미지 시간 저장
    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();

    private void Start()
    {
        _player = GameManager.Instance.skillManager.playScene.player;

        if (_eletricBeingPrefab == null)
            _eletricBeingPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/ElectricBeing");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Monster")
        {
            GameObject monster = other.gameObject;
            float lastTime;
            // 딕셔너리에서 마지막 타이밍 가져오기 (없으면 기본값 0)
            lastDamageTime.TryGetValue(monster, out lastTime);
            if (Time.time - lastTime >= damageInterval)
            {
                // 실제 데미지 처리
                MonsterBase target = monster.GetComponent<MonsterBase>();
                if (target != null)
                {
                    target.TakeDamage(GameManager.Instance.PlayerStatManager.knockBackDamage * GameManager.Instance.PlayerStatManager.knockBackDamageMulti, null);

                    Vector3 spawnPos = _player.transform.position + Vector3.up * 1f;
                    GameManager.Instance.PoolManager.GetFromPool(_eletricBeingPrefab,spawnPos,_player.transform.rotation,_player.transform);
                    //효과음
                    GameManager.Instance.SoundManager.PlaySfxAt("ElectricBeing", spawnPos);
                }
                // 타이머 갱신(데미지 적용 시간)
                lastDamageTime[monster] = Time.time;
            }
        }
    }

    public void ClearDictionary()
    {
        lastDamageTime.Clear();
    }

    private void OnDisable()
    {
        GameManager.Instance.skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnAllWavesCompleted -= ClearDictionary;
    }
}
