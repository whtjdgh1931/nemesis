using UnityEngine;

/// <summary>
/// 인트로 씬을 관리하는 클래스
/// </summary>
public class IntroScene : MonoBehaviour
{
    [SerializeField] IntroSceneView _introSceneView;

    private void Start()
    {
        GameManager.Instance.SoundManager.PlayBGM("Intro");
    }

    private void OnDisable()
    {
        GameManager.Instance.SoundManager.StopBGM();
    }
}
