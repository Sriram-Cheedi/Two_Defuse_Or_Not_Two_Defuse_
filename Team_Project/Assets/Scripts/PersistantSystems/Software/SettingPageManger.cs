using UnityEngine;

namespace PersistantSystems.Software
{
    public class SettingPageManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        public static SettingPageManager Instance {get; private set;}
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            mainMenuPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            mainMenuPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }
    }
}
