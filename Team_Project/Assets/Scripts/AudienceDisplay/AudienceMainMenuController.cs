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
        if (mainMenuStaticDisplay2 != null) mainMenuStaticDisplay2.SetActive(true);
        if (cutSceneDisplay2 != null) cutSceneDisplay2.SetActive(false);
    }

    public void ShowCutscene()
    {
        if (mainMenuStaticDisplay2 != null) mainMenuStaticDisplay2.SetActive(false);
        if (cutSceneDisplay2 != null) cutSceneDisplay2.SetActive(true);
    }
}