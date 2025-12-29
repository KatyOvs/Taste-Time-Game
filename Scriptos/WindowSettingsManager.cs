// 14.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WindowSettingsManager : MonoBehaviour
{
    public Button fullScreenButton; // Кнопка для перехода в полноэкранный режим
    public Button scaleButton; // Кнопка для масштабирования окна

    private bool isFullScreen = false; // Состояние полноэкранного режима

    private void Start()
    {
        // Добавляем слушатели для кнопок
        fullScreenButton.onClick.AddListener(ToggleFullScreen);
        scaleButton.onClick.AddListener(ScaleWindow);
    }

    private void ToggleFullScreen()
    {
        isFullScreen = !isFullScreen;
        Screen.fullScreen = isFullScreen; // Переключение полноэкранного режима
    }

    private void ScaleWindow()
    {
        // Масштабируем окно до заданного размера
        int width = 1280; // Ширина окна
        int height = 720; // Высота окна
        bool fullscreen = false; // Оставляем окно не полноэкранным

        Screen.SetResolution(width, height, fullscreen);
    }
}
