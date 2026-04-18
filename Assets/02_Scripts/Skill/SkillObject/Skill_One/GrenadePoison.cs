using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrenadePoisonData
{
		public float time;
		public float extent;

		public GrenadePoisonData(float time, float extent)
		{
				this.time = time;
				this.extent = extent;
		}
}

/// <summary>
/// 비브르 모션 특수 공격 강화 시 유탄 공격 지점에 생성할 프리팹 클래스
/// </summary>
public class GrenadePoison : AreaDotBase,IInitializePoolable
{
		/// <summary>
		/// 실행 중인 코루틴 목록
		/// </summary>
		private Dictionary<int, Coroutine> poisonStackCoroutine = new Dictionary<int, Coroutine>();

		private float _time;

	Coroutine _poisonRoutine;
		

		public override void ActiveSkill(Transform target)
		{
				int id = target.GetInstanceID();
				if (!poisonStackCoroutine.ContainsKey(id))
				{
						Coroutine startPoison = StartCoroutine(StartPoisonStack(target));
						poisonStackCoroutine.Add(id, startPoison);
				}
		}

		public override void EndSkill(Transform target)
		{
				int id = target.GetInstanceID();
				if (poisonStackCoroutine.TryGetValue(id, out Coroutine poisonStack))
				{
						StopCoroutine(poisonStack);
						poisonStackCoroutine.Remove(id);
				}
		}

		public GameObject GetGameObject()
		{
				return gameObject;
		}

		public void Initialize()
		{
				CheckTarget();
				StartCoroutine(ReleasePoisonArea());
		}

		public void ReleaseObject()
		{
				// 진행 중인 코루틴 종료
				foreach(Coroutine poisonCoroutine in poisonStackCoroutine.Values)
				{
						StopCoroutine(poisonCoroutine);
				}

				// dictionary 초기화
				poisonStackCoroutine.Clear();
		}


		/// <summary>
		/// 1초마다 독 쌓기
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public IEnumerator StartPoisonStack(Transform target)
		{
				DebuffHandler targetHandler = target.GetComponent<DebuffHandler>();
				while (target.gameObject.activeSelf)
				{
						targetHandler.ApplyDebuff(DebuffHandler.DebuffData.CreatePoison());
						yield return new WaitForSeconds(Constants.SKILL_ONE_SPATTACK_STACKTIME);
				}
		}

		public IEnumerator ReleasePoisonArea()
		{
				yield return new WaitForSeconds(_time);
		ReleaseObject();
				GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
		}

		public void Initialize(object data)
		{
				if(data is GrenadePoisonData skillData)
				{
						_time = skillData.time;
						SetAreaExtent(skillData.extent);
				}
		}
}
