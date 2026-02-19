using UnityEngine;
using UnityEngine.SceneManagement;



/// <summary>
/// 씬을 관리하는 매니저 클래스
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
		private LOADSTATE _currentState = LOADSTATE.Start;
		public LOADSTATE currentState { get { return _currentState; } }
		public void SetCurrentState(LOADSTATE state)
		{
				_currentState = state;
		}

		public async void LoadIntroScene()
		{
				await GameManager.Instance.PlayerStatManager.Initialize();
				await GameManager.Instance.PlayerStatManager.WaitForInitAsync();
				GameManager.Instance.PlayerStatManager.UploadToFirebase();
				GameManager.Instance.skillManager.Reset();
				EventBus.ResetEvent();

				SceneManager.LoadScene(Constants.SCENE_NAME_INTRO);
				GameManager.Instance.PoolManager.ClearAllPools();

				//맵 미리 생성
				GameObject bossRoom = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Boss", Vector3.zero, Quaternion.identity);
				GameObject NormalRoom = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Normal", Vector3.zero, Quaternion.identity);
				GameObject ShopRoom = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Shop", Vector3.zero, Quaternion.identity);
				GameObject StartRoom = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Start", Vector3.zero, Quaternion.identity);
				GameObject Colosseum = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Colosseum", Vector3.zero, Quaternion.identity);
				GameObject Laboratory = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Map/Rooms/Lab", Vector3.zero, Quaternion.identity);

				//맵 릴리즈
				GameManager.Instance.PoolManager.ReleaseToPool(bossRoom);
				GameManager.Instance.PoolManager.ReleaseToPool(NormalRoom);
				GameManager.Instance.PoolManager.ReleaseToPool(ShopRoom);
				GameManager.Instance.PoolManager.ReleaseToPool(StartRoom);
				GameManager.Instance.PoolManager.ReleaseToPool(Colosseum);
				GameManager.Instance.PoolManager.ReleaseToPool(Laboratory);
		}

		public async void LoadPlayScene()
		{
				await SceneManager.LoadSceneAsync(Constants.SCENE_NAME_LOADING, LoadSceneMode.Additive);
				Scene currentScene = SceneManager.GetActiveScene();

				// 씬 이름 비교
				if (currentScene.name == Constants.SCENE_NAME_PLAY)
				{
						await SceneManager.UnloadSceneAsync(currentScene.name);
				}
				await GameManager.Instance.PlayerStatManager.Initialize();
				await GameManager.Instance.PlayerStatManager.WaitForInitAsync();
				GameManager.Instance.PlayerStatManager.UploadToFirebase();

				GameManager.Instance.skillManager.Reset();
				EventBus.ResetEvent();

				GameManager.Instance.CurrencyManager.SetCreditFromServer();
				_currentState = LOADSTATE.Server;

				Scene targetScene = SceneManager.GetSceneByName(Constants.SCENE_NAME_INTRO);
				if (targetScene.IsValid() && targetScene.isLoaded)
				{
						await SceneManager.UnloadSceneAsync(Constants.SCENE_NAME_INTRO);
				}

				// Play 씬 로드
				var asyncOp = SceneManager.LoadSceneAsync(Constants.SCENE_NAME_PLAY, LoadSceneMode.Additive);
				
				asyncOp.completed += (op) =>
				{
						// 로드된 씬 객체 가져오기
						Scene playScene = SceneManager.GetSceneByName(Constants.SCENE_NAME_PLAY);

						// 활성 씬으로 설정
						SceneManager.SetActiveScene(playScene);

						// 초기화 마무리
						GameManager.Instance.PoolManager.ClearAllPools();
						GameManager.Instance.UIManager.ResetUIManager();
						GameManager.Instance.CurrencyManager.Initialize();
						EventBus.OnMonsterHit += GameManager.Instance.PlayerStatManager.TakeDamage;
						_currentState = LOADSTATE.PlayScene;

						//await SceneManager.LoadSceneAsync(Constants.SCENE_NAME_PLAY,LoadSceneMode.Single);

						//  // 맵에 있는 모든 풀링 오브젝트들을 릴리즈
						//  GameManager.Instance.PoolManager.ClearAllPools();

						//  //UI초기화
						//  GameManager.Instance.UIManager.ResetUIManager();

						//  // 재화 초기화
						//  GameManager.Instance.CurrencyManager.Initialize();

						//  // TakeDamage 이벤트 연결
						//  EventBus.OnMonsterHit += GameManager.Instance.PlayerStatManager.TakeDamage;
				};
		}

		public void LoadLoginScene()
		{
				SceneManager.LoadScene(Constants.SCENE_NAME_LOGIN);
		}

		public async void UnloadScene()
		{
				Scene targetScene = SceneManager.GetSceneByName(Constants.SCENE_NAME_LOADING);

				if (targetScene.IsValid() && targetScene.isLoaded)
				{
						await SceneManager.UnloadSceneAsync(Constants.SCENE_NAME_LOADING);
				}

		}
}
