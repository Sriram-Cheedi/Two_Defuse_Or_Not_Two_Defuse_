using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace PersistantSystems.Software
{
    public class FOVSettings : MonoBehaviour
    {
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TMP_Text fovValue;
        private const string fovKEY = "FOV";
        private const float fovDefault = 90f;
        private const float fovMin = 70f;
        private const float fovMax = 100f;
        
        private void Start()
        {
            if (fovSlider == null || fovValue == null) //change
            {
                Debug.LogWarning("FOV settings not set");
                return;
            }
            fovSlider.minValue = fovMin;
            fovSlider.maxValue = fovMax;
            fovSlider.wholeNumbers = true;
            float savedFov = PlayerPrefs.GetFloat(fovKEY, fovDefault);
            fovSlider.SetValueWithoutNotify(savedFov);
            // fovValue.text = Mathf.RoundToInt(savedFov).ToString(); //include this
            UpdateFovValue(savedFov);
            fovSlider.onValueChanged.AddListener(OnFovChanged);
        }

        private void OnFovChanged(float value)
        {
            PlayerPrefs.SetFloat(fovKEY, value);
            PlayerPrefs.Save();
            UpdateFovValue(value);
        }

        private void UpdateFovValue(float value)
        {
            fovValue.text = Mathf.RoundToInt(value).ToString();
        }
    }
}
