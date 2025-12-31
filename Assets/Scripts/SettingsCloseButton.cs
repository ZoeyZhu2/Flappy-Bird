using UnityEngine;
using UnityEngine.UI;


public class SettingsCloseButton : MonoBehaviour
{
    public Button closeButton;
    [SerializeField] private AudioClip pressSound;
    public float volume = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            SoundFXManager.Instance.PlaySoundFX(pressSound, transform, volume);
            SettingsManager.Instance.CloseSettings();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
