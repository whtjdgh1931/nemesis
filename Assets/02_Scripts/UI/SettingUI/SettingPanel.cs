using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] GameObject introPanel;
    [SerializeField] GameObject playPanel;


    public void OnOpenSetting()
    {
        EventBus.SetCanGetInput(false);
        if(SceneManager.GetActiveScene().buildIndex ==0|| SceneManager.GetActiveScene().buildIndex == 1)
        {
            introPanel.SetActive(true);
        }
        else
        {
            playPanel.SetActive(true);
            Time.timeScale = 0.0f;
            EventBus.SetCanTimeRun(false);
            if (EventBus.IsColosseumRoom)
            {
                Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = true; // 커서 보이게/숨기기
            }
        }
    }

    public void ResetTimeScale()
    {
        EventBus.SetCanTimeRun(true);
        Time.timeScale = 1f;
    }

    public void OnBtnClick_GameExit()
    {
        Application.Quit();
    }

    public void OnBtnClick_Re()
    {
       GameManager.Instance.SceneLoadManager.LoadPlayScene();
    }

}
