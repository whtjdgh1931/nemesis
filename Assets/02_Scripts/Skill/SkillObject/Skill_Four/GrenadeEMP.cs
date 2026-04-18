using System.Collections;
using UnityEngine;

public class GrenadeEMPData
{
		public float time;
		public float extent;

		public GrenadeEMPData(float time, float extent)
		{
				this.time = time;
				this.extent = extent;
		}
}


public class GrenadeEMP : AreaDamageBase, IInitializePoolable
{
		private float _time;

		public void Initialize(object data)
		{
				if (data is GrenadeEMPData skillData)
				{
						_time = skillData.time;
						SetAreaExtent(skillData.extent);
				}
		}

		public void Initialize()
		{
				CheckTarget();
				StartCoroutine(DestroyPoisonSpreadCoroutine());
		}

		/// <summary>
		/// 스킬에 맞는 효과 발동
		/// </summary>
		/// <param name="target"></param>
		public override void ActiveSkill(Transform target)
		{
				// 속박 적용
				target.GetComponent<DebuffHandler>().ApplyDebuff(DebuffHandler.DebuffData.CreateBinding(_time));

		}

		public IEnumerator DestroyPoisonSpreadCoroutine()
		{
				yield return new WaitForSeconds(Constants.SKILL_REMAIN);
				GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
		}

}
