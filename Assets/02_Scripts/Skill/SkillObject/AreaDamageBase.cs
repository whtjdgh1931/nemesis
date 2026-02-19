using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영역내에 한번에 효과를 주는 스킬
/// CheckTarget 구현됨, CheckTarget이 Target에게 ActiveSkill 실행
/// ActiveSkill 구현 필요
/// </summary>
public class AreaDamageBase : PoolableObject
{
    /// <summary>
    /// 데미지 범위 넓이
    /// </summary>
    [SerializeField]
    protected float _areaExtent;
    public float areaExtent { get { return _areaExtent; } }
    public void SetAreaExtent(float areaExtent)
    {
        _areaExtent = areaExtent * GameManager.Instance.PlayerStatManager.playerAreaExtent;
        Vector3 desiredWorldScale = Vector3.one * _areaExtent * 2f;
        Vector3 parentWorldScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            desiredWorldScale.x / parentWorldScale.x,
            desiredWorldScale.y / parentWorldScale.y,
            desiredWorldScale.z / parentWorldScale.z
        );
    }

    [SerializeField]
    protected string _checkTargetTag = Constants.TAG_MONSTER;
    public string checkTargetTag { get { return _checkTargetTag; } }

    /// <summary>
    /// 탐색 콜라이더 리스트
    /// </summary>
    [SerializeField]
    private Collider[] _results = new Collider[Constants.DRONE_SEARCHNUM];




    /// <summary>
    /// 영역 안의 적 탐색
    /// </summary>
    public void CheckTarget()
    {
        
        // 콜라이더 탐색 (필요하다면 레이어마스크 설정)
        int hitColliders = Physics.OverlapSphereNonAlloc(transform.position, _areaExtent, _results);


        for (int i = 0; i < hitColliders; i++)
        {
            if (_results[i].CompareTag(_checkTargetTag))
            {
                //효과 발동
                ActiveSkill(_results[i].transform);
            }

        }
    }

   

    /// <summary>
    /// 스킬에 맞는 효과 발동
    /// </summary>
    /// <param name="target"></param>
    public virtual void ActiveSkill(Transform target)
    {

    }



}


