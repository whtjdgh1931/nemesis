using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MonsterController))]
public class MonsterDebuggerEditor : Editor
{
		private int selectedIndex = 0;
		private Vector3 spawnPosition = Vector3.zero;
		private int monsterCount = 1;

		private bool useCustomHP = false;
		private int customHP = 100;

		private bool useCustomSpeed = false;
		private float customSpeed = 3f;

		private bool filterBoss = false;
		private bool filterElite = false;

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();

				MonsterController controller = (MonsterController)target;
				MonsterSpawner spawner = controller.MonsterSpawner;

				GUILayout.Space(10);
				GUILayout.Label("🧪 Monster Debugger", EditorStyles.boldLabel);

				List<PoolableObject> prefabs = spawner.GetMonsterPrefabs();
				if (prefabs == null || prefabs.Count == 0)
				{
						GUILayout.Label("monsterPrefabs가 비어 있습니다.");
						return;
				}

				// 필터링
				List<PoolableObject> filtered = new List<PoolableObject>();
				foreach (var prefab in prefabs)
				{
						string name = prefab.name.ToLower();
						if (filterBoss && !name.Contains("boss")) continue;
						if (filterElite && !name.Contains("elite")) continue;
						filtered.Add(prefab);
				}

				if (filtered.Count == 0)
				{
						GUILayout.Label("필터 조건에 맞는 몬스터가 없습니다.");
						return;
				}

				string[] names = filtered.ConvertAll(p => p.name).ToArray();
				selectedIndex = EditorGUILayout.Popup("몬스터 선택", selectedIndex, names);

				spawnPosition = EditorGUILayout.Vector3Field("소환 위치", spawnPosition);
				monsterCount = EditorGUILayout.IntSlider("소환 개수", monsterCount, 1, 10);

				useCustomHP = EditorGUILayout.Toggle("체력 설정 사용", useCustomHP);
				if (useCustomHP)
				{
						customHP = EditorGUILayout.IntField("체력 값 (int)", customHP);
				}

				useCustomSpeed = EditorGUILayout.Toggle("속도 설정 사용", useCustomSpeed);
				if (useCustomSpeed)
				{
						customSpeed = EditorGUILayout.FloatField("속도 값 (float)", customSpeed);
				}

				filterBoss = EditorGUILayout.Toggle("보스만 보기", filterBoss);
				filterElite = EditorGUILayout.Toggle("엘리트만 보기", filterElite);

				if (GUILayout.Button("🧟 몬스터 소환"))
				{
						for (int i = 0; i < monsterCount; i++)
						{
								Vector3 offset = spawnPosition + new Vector3(i * 1.5f, 0, 0);
								GameObject spawned = GameManager.Instance.PoolManager.GetFromPool(filtered[selectedIndex], offset, Quaternion.identity);
								MonsterBase monster = spawned.GetComponent<MonsterBase>();
								if (monster != null)
								{
										int hp = useCustomHP ? customHP : monster.maxHealth;
										float speed = useCustomSpeed ? customSpeed : monster.moveSpeed;
										monster.SetDebugStats(hp, speed);
										monster.Initialize();
										spawner.ActiveMonsters.Add(spawned);
								}
						}
				}

				if (GUILayout.Button("💀 모든 몬스터 제거"))
				{
						spawner.KillAllActiveMonsters();
				}

				GUILayout.Space(5);
				GUILayout.Label($"현재 몬스터 수: {spawner.ActiveMonsters?.Count ?? 0}");
		}
}
