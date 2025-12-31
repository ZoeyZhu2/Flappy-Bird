using UnityEngine;
using UnityEngine.UI;

public class SettingsCloseButton : MonoBehaviour
{
    public Button closeButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            SettingsManager.Instance.CloseSettings();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
