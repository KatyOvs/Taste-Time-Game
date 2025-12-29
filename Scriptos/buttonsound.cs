// ButtonSoundHandler.cs
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundHandler : MonoBehaviour
{
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    void OnButtonClick()
    {
        // Используем синглтон GameSettingsHandler для воспроизведения звука
        if (GameSettingsHandler.Instance != null)
        {
            GameSettingsHandler.Instance.PlayClickSound();
        }
    }
}