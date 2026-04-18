using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
		[SerializeField]
		private Slider _loadingSlider;

		private SceneLoadManager _sceneLoadManager;

		private float goalState;   // 목표 값 
		private float lerpSpeed = 1f; // 슬라이더가 따라가는 속도

		private void Update()
		{
				if (_sceneLoadManager == null)
				{
						_sceneLoadManager = GameManager.Instance.SceneLoadManager;
				}

				// 목표 값 갱신
				if (goalState != (float)_sceneLoadManager.currentState)
				{
						goalState = (float)_sceneLoadManager.currentState/(float)LOADSTATE.Finish;
				}

				// 슬라이더 값을 Lerp로 보간
				_loadingSlider.value = Mathf.Lerp(_loadingSlider.value, goalState, Time.deltaTime * lerpSpeed);
		}

}
