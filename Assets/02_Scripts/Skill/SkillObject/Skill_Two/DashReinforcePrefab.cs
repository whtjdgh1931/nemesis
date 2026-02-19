using System.Collections;
using UnityEngine;

public class DashReinforceData
{
	public float skillExtent;

	public DashReinforceData(float extent)
	{
		skillExtent = extent;
	}
}

public class DashReinforcePrefab : AreaDamageBase, IInitializePoolable
{
	public override void ActiveSkill(Transform target)
	{
		// 화상 적용
		DebuffHandler targetDebuffHandler = target.GetComponent<DebuffHandler>();
		targetDebuffHandler.ApplyDebuff(DebuffHandler.DebuffData.CreateBurn());
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}


	public void Initialize()
	{
		CheckTarget();
		StartCoroutine(ReleaseObjectCoroutine());
	}

	public void Initialize(object data)
	{
		if (data is DashReinforceData skillData)
		{
			SetAreaExtent(skillData.skillExtent);
		}
	}

	IEnumerator ReleaseObjectCoroutine()
	{
		yield return new WaitForSeconds(Constants.SKILL_REMAIN);
		GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
	}


	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _areaExtent); // 반지름 5짜리 원형 Gizmo

	}
}
