using UnityEngine;
using UnityEngine.UI;

public class InteractionGuideBtn : InteractionGuideView
{
    [SerializeField] private Button interactBtn;

    public void Initialize(Player player)
    {
        interactBtn.onClick.AddListener(()=>player.ExecuteInteraction());
		}


		public void SetActive(bool obj)
    {
        gameObject.SetActive(obj);
      
    }
}
