// 14.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class CloseButtonHandler : MonoBehaviour
{
    public GameObject settingsPanel; // Ссылка на панель настроек

    public void OnCloseButtonClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Выключаем панель настроек
        }
    }
}
