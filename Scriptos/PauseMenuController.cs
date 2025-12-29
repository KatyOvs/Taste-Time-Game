// 16.12.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel; // Ссылка на панель паузы
    public GameObject settingsPanel; // Ссылка на панель настроек
    public Button resumeButton; // Кнопка "Вернуться"
    public Button settingsButton; // Кнопка "Настройки"
    public Button mainMenuButton; // Кнопка "В главное меню"
    public Button closeSettingsButton; // Кнопка "Закрыть настройки"

    private bool isPaused = false;

    void Start()
    {
        // Устанавливаем начальное состояние панелей
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Назначаем действия для кнопок с добавлением звуков
        resumeButton.onClick.AddListener(() => {
            ResumeGame();
            PlayClickSound();
        });
        
        settingsButton.onClick.AddListener(() => {
            OpenSettings();
            PlayClickSound();
        });
        
        mainMenuButton.onClick.AddListener(() => {
            PlayClickSound();
            GoToMainMenu();
        });
        
        closeSettingsButton.onClick.AddListener(() => {
            CloseSettings();
            PlayClickSound();
        });
        
        // Добавляем звуки клика ко всем кнопкам в настройках
        AddClickSoundsToSettingsPanel();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        
        // Останавливаем или возобновляем время
        Time.timeScale = isPaused ? 0 : 1;
        
        PlayClickSound();
    }

    private void ResumeGame()
    {
        // Закрываем панель паузы и возобновляем игру
        pausePanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1; // Возобновляем время
    }

    private void OpenSettings()
    {
        // Открываем панель настроек и скрываем панель паузы
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        // Закрываем панель настроек и возвращаемся к панели паузы
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    private void GoToMainMenu()
    {
        // Возврат в главное меню
        Time.timeScale = 1; // Возобновляем время перед переходом
        SceneManager.LoadScene("Menu"); // Укажите название вашей сцены главного меню
    }
    
    private void PlayClickSound()
    {
        if (GameSettingsHandler.Instance != null)
        {
            GameSettingsHandler.Instance.PlayClickSound();
        }
    }
    
    private void AddClickSoundsToSettingsPanel()
    {
        if (settingsPanel == null) return;
        
        // Находим все кнопки в панели настроек
        Button[] settingsButtons = settingsPanel.GetComponentsInChildren<Button>(true);
        
        foreach (Button button in settingsButtons)
        {
            // Пропускаем кнопки, у которых уже есть обработчики
            if (button == closeSettingsButton) continue;
            
            // Добавляем звук клика
            button.onClick.AddListener(PlayClickSound);
        }
        
        // Также добавляем звуки к слайдерам в настройках
        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            slider.onValueChanged.AddListener((value) => PlayClickSound());
        }
        
        // Добавляем звуки к переключателям
        Toggle[] toggles = settingsPanel.GetComponentsInChildren<Toggle>(true);
        foreach (Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener((value) => PlayClickSound());
        }
    }
}