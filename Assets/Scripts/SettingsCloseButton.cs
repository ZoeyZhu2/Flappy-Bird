using UnityEngine;
using UnityEngine.UI;


public class SettingsCloseButton : MonoBehaviour
{
    [SerializeField] private Button closeButton; //used to be public
    [SerializeField] private AudioClip pressSound;
    [SerializeField] private float volume = 1f; //used to be public

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundFX(pressSound, transform, volume);
            SettingsManager.Instance.CloseSettings();
        });
    }
}
