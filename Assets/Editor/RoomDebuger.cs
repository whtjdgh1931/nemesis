using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor
{
		private RoomType selectedRoomType = RoomType.Normal;
		private NormalRoomType selectedNormalRoomType = NormalRoomType.TechSelect;
		private TechSelectPackType selectedTechPackType = TechSelectPackType.Company1;
		private int autoRoomCount = 3;

		public override void OnInspectorGUI()
		{
				DrawDefaultInspector();
				MapController controller = (MapController)target;

				GUILayout.Space(10);
				GUILayout.Label("🧭 다음 방 생성 옵션", EditorStyles.boldLabel);

				selectedRoomType = (RoomType)EditorGUILayout.EnumPopup("Room Type", selectedRoomType);

				if (selectedRoomType == RoomType.Normal)
				{
						selectedNormalRoomType = (NormalRoomType)EditorGUILayout.EnumPopup("Normal Room Type", selectedNormalRoomType);

						if (selectedNormalRoomType == NormalRoomType.TechSelect)
						{
								selectedTechPackType = (TechSelectPackType)EditorGUILayout.EnumPopup("Tech Pack Type", selectedTechPackType);
						}
				}

				if (GUILayout.Button("➡ 다음 방 생성 (몬스터 제거 포함)"))
				{
						KillMonsters(controller);
						SpawnRoom(controller);
				}

				GUILayout.Space(10);
				GUILayout.Label("🚀 자동 방 생성 시퀀스", EditorStyles.boldLabel);
				autoRoomCount = EditorGUILayout.IntField("생성할 방 개수", autoRoomCount);

				if (GUILayout.Button("자동 방 생성 및 보상 실행"))
				{
						for (int i = 0; i < autoRoomCount; i++)
						{
								KillMonsters(controller);
								SpawnRoom(controller);
								controller.SendMessage("StartReward");
						}
				}

				GUILayout.Space(10);
				GUILayout.Label("🎁 보상 및 방 제거", EditorStyles.boldLabel);

				if (GUILayout.Button("🎉 StartReward() 실행"))
				{
						controller.SendMessage("StartReward");
				}

				if (GUILayout.Button("🧹 DestroyCurrentRoomObjects() 실행"))
				{
						controller.SendMessage("DestroyCurrentRoomObjects");
				}

				GUILayout.Space(10);
				GUILayout.Label("👊 몬스터 수동 제거", EditorStyles.boldLabel);

				if (GUILayout.Button("현재 몬스터 모두 제거"))
				{
						KillMonsters(controller);
				}

				GUILayout.Space(10);
				GUILayout.Label("🔍 현재 방 상태", EditorStyles.boldLabel);

				Room currentRoom = GetPrivateField<Room>(controller, "_currentRoom");
				if (currentRoom != null)
				{
						GUILayout.Label($"방 이름: {currentRoom.name}");
						GUILayout.Label($"방 타입: {currentRoom.RoomInfo?.RoomType}");
						GUILayout.Label($"NormalRoomType: {(currentRoom.RoomInfo?.NormalType.HasValue == true ? currentRoom.RoomInfo.NormalType.ToString() : "없음")}");
						GUILayout.Label($"TechPackType: {(currentRoom.RoomInfo?.TechType.HasValue == true ? currentRoom.RoomInfo.TechType.ToString() : "없음")}");
				}
				else
				{
						GUILayout.Label("현재 방 없음");
				}

				int roomCount = GetPrivateField<int>(controller, "_currentRoomCount");
				GUILayout.Label($"방 번호: {roomCount}");

				bool hasLab = GetPrivateField<bool>(controller, "_hasLabRoomAppeared");
				GUILayout.Label($"Lab 등장 여부: {(hasLab ? "✅ 있음" : "❌ 없음")}");

				GUILayout.Space(10);
				GUILayout.Label("🚪 현재 문 정보", EditorStyles.boldLabel);

				Door[] doors = GetPrivateField<Door[]>(controller, "_currentDoors");
				if (doors != null && doors.Length > 0)
				{
						GUILayout.Label($"문 개수: {doors.Length}");
						for (int i = 0; i < doors.Length; i++)
						{
								GUILayout.Label($"#{i} - {doors[i].name} | RoomType: {doors[i].RoomInfo?.RoomType}");
						}
				}
				else
				{
						GUILayout.Label("현재 문 없음");
				}

				GUILayout.Space(10);
				GUILayout.Label("👾 몬스터 상태", EditorStyles.boldLabel);

				var monsterController = GetPrivateField<object>(controller, "_monsterController");
				var spawner = GetPrivateField<object>(monsterController, "_monsterSpawner");
				var activeList = GetPropertyValue<List<GameObject>>(spawner, "ActiveMonsters");
				GUILayout.Label($"현재 몬스터 수: {activeList?.Count ?? 0}");
		}

		private void SpawnRoom(MapController controller)
		{
				RoomInfo info = new RoomInfo(
						selectedRoomType,
						selectedRoomType == RoomType.Normal ? selectedNormalRoomType : (NormalRoomType?)null,
						(selectedRoomType == RoomType.Normal && selectedNormalRoomType == NormalRoomType.TechSelect)
								? selectedTechPackType
								: (TechSelectPackType?)null
				);

				GameObject dummyObj = new GameObject("DebugDoorInteractor");
				dummyObj.hideFlags = HideFlags.HideAndDontSave;
				DoorInteractor dummyInteractor = dummyObj.AddComponent<DoorInteractor>();
				dummyInteractor.SetRoomInfo(info);

				controller.SendMessage("OnDoorInteracted", dummyInteractor);
				DestroyImmediate(dummyObj);
		}

		private void KillMonsters(MapController controller)
		{
				var monsterController = GetPrivateField<object>(controller, "_monsterController");
				var spawner = GetPrivateField<object>(monsterController, "_monsterSpawner");
				var killMethod = spawner?.GetType().GetMethod("KillAllActiveMonsters", BindingFlags.Public | BindingFlags.Instance);
				killMethod?.Invoke(spawner, null);
		}

		private T GetPrivateField<T>(object obj, string fieldName)
		{
				var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				return field != null ? (T)field.GetValue(obj) : default;
		}

		private T GetPropertyValue<T>(object obj, string propertyName)
		{
				var prop = obj?.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
				return prop != null ? (T)prop.GetValue(obj) : default;
		}
}
