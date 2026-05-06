using UnityEngine;

public class AudienceMainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuStaticDisplay2;
    [SerializeField] private GameObject cutSceneDisplay2;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuStaticDisplay2.SetActive(true);
        cutSceneDisplay2.SetActive(false);
    }

    public void ShowCutscene()
    {
        mainMenuStaticDisplay2.SetActive(false);
        cutSceneDisplay2.SetActive(true);
    }
}