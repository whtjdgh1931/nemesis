using UnityEngine;

public class ServerScene : MonoBehaviour
{


    public void Start()
    {
        GameManager.Instance.serverManager.ServerStart();
  
    }

}
