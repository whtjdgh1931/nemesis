using Firebase.Auth;
using System.Collections.Generic;
using UnityEngine;

public class RecordPanel : MonoBehaviour
{
		[SerializeField] private RecordPrefab _recordPrefab;
		[SerializeField] private GameObject parentPanel;
		[SerializeField] private RecordPrefab _currentUserRecord;

		public void OnEnable()
		{
				EventBus.SetCanGetInput(false);

				LoadRankingAsync(); // 비동기 메서드 호출
		}

		private async void LoadRankingAsync()
		{
				var list = await GameManager.Instance.serverManager.downloadManager.GetClearTimeRanking();

				for (int i = 0; i < Mathf.Min(list.Count, 10); i++)
				{
						var (email, bestTime, savedAt) = list[i];
						RecordPrefab obj = Instantiate(_recordPrefab, parentPanel.transform).GetComponent<RecordPrefab>();
						obj.SetData(i+1,email, bestTime, savedAt);
				}

				string currentEmail = FirebaseAuth.DefaultInstance.CurrentUser?.Email;
				int index = list.FindIndex(item => item.email == currentEmail);
				if (index != -1)
				{
						var (email, bestTime, savedAt) = list[index];
						_currentUserRecord.SetData(index + 1, email, bestTime, savedAt);
				}
				else
				{
						_currentUserRecord.SetData(0,"No Record",0f,0f);
				}
		}

		public void OnDisable()
		{
        EventBus.SetCanGetInput(true);
				// parentPanel 아래의 모든 자식 오브젝트 제거
				foreach (Transform child in parentPanel.transform)
				{
						Destroy(child.gameObject);
				}
		}


}
