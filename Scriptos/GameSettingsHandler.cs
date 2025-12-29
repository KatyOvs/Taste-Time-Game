using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettingsHandler : MonoBehaviour
{
    // Синглтон инстанс
    public static GameSettingsHandler Instance { get; private set; }

    [Header("Настройки звука")]
    public AudioClip buttonClickSound;        // Звук клика кнопки
    public AudioClip backgroundMusic;         // Фоновая музыка

    [Header("UI Элементы")]
    public Slider musicVolumeSlider;          // Слайдер громкости музыки
    public Slider soundVolumeSlider;          // Слайдер громкости звуков
    public Button musicToggleButton;          // Кнопка вкл/выкл музыки
    public Button soundToggleButton;          // Кнопка вкл/выкл звуков
    public Button fullScreenButton;           // Кнопка полноэкранного режима
    public Button scaleButton;                // Кнопка масштабирования

    [Header("Спрайты")]
    public Sprite soundOnSprite;              // Спрайт включенного звука
    public Sprite soundOffSprite;             // Спрайт выключенного звука

    [Header("UI Элементы - Панель настроек")]
    public GameObject settingsPanel;
    public Button openSettingsButton;      // Кнопка открытия настроек
    public Button closeSettingsButton;     // Кнопка закрытия настроек

    // Аудиоисточники
    private AudioSource musicSource;
    private AudioSource soundSource;

    // Переменные состояния
    private bool isMusicOn = true;
    private bool isSoundOn = true;
    private float musicVolumePercent = 50f;  // 0-100
    private float soundVolumePercent = 50f;  // 0-100
    
    // Настройки масштабирования
    private float[] scaleOptions = { 1.0f, 1.25f, 1.5f, 1.75f, 2.0f }; // Варианты масштабирования
    private int currentScaleIndex = 0; // Текущий индекс масштаба
    private int baseWidth = 1280;
    private int baseHeight = 720;
    private bool isFullScreen = false;
    // Флаг, чтобы отслеживать, была ли уже запущена музыка
    private bool isMusicStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Создаем аудиоисточники
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
            
            // Загружаем сохраненные настройки
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Настройка UI в текущей сцене
        SetupUI();
        
        // Запуск фоновой музыки только если она еще не запущена
        if (!isMusicStarted)
        {
            StartBackgroundMusic();
        }
        
        // Обновляем состояние кнопок
        UpdateMusicButtonState();
        UpdateSoundButtonState();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Находим UI-элементы в новой сцене
        FindUIElementsInNewScene();
        
        // Обновляем UI при смене сцены
        UpdateUIAfterSceneChange();
    }
    
    private void FindUIElementsInNewScene()
{
    // Ищем все Canvas в сцене и проверяем их дочерние объекты
    Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
    
    foreach (Canvas canvas in allCanvases)
    {
        // Рекурсивно ищем нужные элементы в дочерних объектах канваса
        FindElementsInTransform(canvas.transform);
    }
    
    // Также ищем отдельно по имени на всякий случай
    FindElementsByName();
}
    private void FindElementsInTransform(Transform parent)
{
    // Проходим по всем дочерним объектам
    for (int i = 0; i < parent.childCount; i++)
    {
        Transform child = parent.GetChild(i);
        
        // Проверяем имя объекта
        string childName = child.name;
        
        // Панель настроек
        if (childName.Contains("SettingsPanel") && settingsPanel == null)
        {
            settingsPanel = child.gameObject;
        }
        
        // Слайдеры
        if (childName.Contains("Music") && childName.Contains("Slider") && musicVolumeSlider == null)
        {
            Slider slider = child.GetComponent<Slider>();
            if (slider != null) musicVolumeSlider = slider;
        }
        
        if (childName.Contains("Sound") && childName.Contains("Slider") && soundVolumeSlider == null)
        {
            Slider slider = child.GetComponent<Slider>();
            if (slider != null) soundVolumeSlider = slider;
        }
        
        // Кнопки (ищем по частичным совпадениям)
        if (childName.Contains("Music") && childName.Contains("Toggle") && musicToggleButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) musicToggleButton = button;
        }
        
        if (childName.Contains("Sound") && childName.Contains("Toggle") && soundToggleButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) soundToggleButton = button;
        }
        
        if ((childName.Contains("FullScreen") || childName.Contains("Fullscreen")) && fullScreenButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) fullScreenButton = button;
        }
        
        if ((childName.Contains("Scale") || childName.Contains("Масштаб")) && scaleButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) scaleButton = button;
        }
        
        if ((childName.Contains("OpenSettings") || childName.Contains("Open")) && openSettingsButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) openSettingsButton = button;
        }
        
        if ((childName.Contains("CloseSettings") || childName.Contains("Close")) && closeSettingsButton == null)
        {
            Button button = child.GetComponent<Button>();
            if (button != null) closeSettingsButton = button;
        }
        
        // Рекурсивно проверяем дочерние объекты
        if (child.childCount > 0)
        {
            FindElementsInTransform(child);
        }
    }
}

private void FindElementsByName()
{
    // Альтернативный поиск по точным именам (если знаете точные имена)
    
    // Пример поиска слайдеров
    Slider[] allSliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
    foreach (Slider slider in allSliders)
    {
        if (slider.name == "MusicVolumeSlider" && musicVolumeSlider == null)
            musicVolumeSlider = slider;
        else if (slider.name == "SoundVolumeSlider" && soundVolumeSlider == null)
            soundVolumeSlider = slider;
    }
    
    // Пример поиска кнопок
    Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
    foreach (Button button in allButtons)
    {
        string buttonName = button.name;
        
        if (buttonName == "MusicToggleButton" && musicToggleButton == null)
            musicToggleButton = button;
        else if (buttonName == "SoundToggleButton" && soundToggleButton == null)
            soundToggleButton = button;
        else if (buttonName == "FullScreenButton" && fullScreenButton == null)
            fullScreenButton = button;
        else if (buttonName == "ScaleButton" && scaleButton == null)
            scaleButton = button;
        else if (buttonName == "OpenSettingsButton" && openSettingsButton == null)
            openSettingsButton = button;
        else if (buttonName == "CloseSettingsButton" && closeSettingsButton == null)
            closeSettingsButton = button;
    }
    
    // Поиск панели настроек
    GameObject panel = GameObject.Find("SettingsPanel");
    if (panel != null && settingsPanel == null)
        settingsPanel = panel;
}
    private void SetupUI()
    {
        // Настройка слайдеров
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 100f;
            musicVolumeSlider.value = musicVolumePercent;
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.minValue = 0f;
            soundVolumeSlider.maxValue = 100f;
            soundVolumeSlider.value = soundVolumePercent;
            soundVolumeSlider.onValueChanged.RemoveAllListeners();
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        }

        // Настройка кнопок
        SetupButtonListeners();
    }
    
    private void UpdateUIAfterSceneChange()
    {
        // Обновляем значения слайдеров
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolumePercent;
        }
        
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.value = soundVolumePercent;
        }
        
        // Обновляем состояние кнопок
        UpdateMusicButtonState();
        UpdateSoundButtonState();
        
        // Переподписываемся на кнопки
        SetupButtonListeners();
        
        // Применяем текущие настройки масштабирования
        ApplyScale();
    }
    
    public void SetupButtonListeners()
    {
        // Удаляем старые слушатели и добавляем новые
        
        // Кнопка музыки
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.RemoveAllListeners();
            musicToggleButton.onClick.AddListener(() =>
            {
                ToggleMusic();
                PlayClickSound();
            });
        }
        
        // Кнопка звука
        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.RemoveAllListeners();
            soundToggleButton.onClick.AddListener(() =>
            {
                ToggleSound();
                if (isSoundOn) PlayClickSound();
            });
        }
        
        // Кнопка полноэкранного режима
        if (fullScreenButton != null)
        {
            fullScreenButton.onClick.RemoveAllListeners();
            fullScreenButton.onClick.AddListener(() =>
            {
                ToggleFullScreen();
                PlayClickSound();
            });
        }
        
        // Кнопка масштабирования
        if (scaleButton != null)
        {
            scaleButton.onClick.RemoveAllListeners();
            scaleButton.onClick.AddListener(() =>
            {
                CycleScale();
                PlayClickSound();
            });
        }
        
        // Кнопки панели настроек
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveAllListeners();
            openSettingsButton.onClick.AddListener(() =>
            {
                if (settingsPanel != null) settingsPanel.SetActive(true);
                PlayClickSound();
            });
        }
        
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveAllListeners();
            closeSettingsButton.onClick.AddListener(() =>
            {
                if (settingsPanel != null) settingsPanel.SetActive(false);
                PlayClickSound();
            });
        }
    }
    
    // ============ ЛОГИКА МАСШТАБИРОВАНИЯ ============
    
    private void CycleScale()
    {
        // Переключаем на следующий масштаб
        currentScaleIndex = (currentScaleIndex + 1) % scaleOptions.Length;
        
        // Применяем масштаб (изменяем размер окна)
        ApplyScale();
        
        // Сохраняем настройки
        SaveSettings();
    }
    
    private void ApplyScale()
    {
       if (scaleOptions.Length == 0) return;
        
        float scale = scaleOptions[currentScaleIndex];
        
        // Изменяем размер окна приложения (только в оконном режиме)
        if (!isFullScreen)
        {
            // Рассчитываем новые размеры окна
            int newWidth = Mathf.RoundToInt(baseWidth * scale);
            int newHeight = Mathf.RoundToInt(baseHeight * scale);
            
            // Устанавливаем размер окна
            Screen.SetResolution(newWidth, newHeight, FullScreenMode.Windowed);
        }
    }
    
    // ============ ЛОГИКА ПОЛНОГО ЭКРАНА ============
    
    private void ToggleFullScreen()
    {
       isFullScreen = !isFullScreen;
        
        if (isFullScreen)
        {
            // Включаем полноэкранный режим БЕЗ заголовка окна
            Screen.SetResolution(Screen.currentResolution.width, 
                               Screen.currentResolution.height, 
                               FullScreenMode.FullScreenWindow);
        }
        else
        {
            // Возвращаемся в оконный режим с текущим масштабом
            ApplyScale();
        }
    }
    
   
    
    // ============ ЛОГИКА ЗВУКА ============
    
    private void OnMusicVolumeChanged(float value)
    {
        musicVolumePercent = value;
        if (musicSource != null && isMusicOn)
        {
            musicSource.volume = value / 100f;
            if (value > 0.01f && !musicSource.isPlaying)
            {
                StartBackgroundMusic();
            }
            else if (value <= 0.01f && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
        SaveSettings();
    }
    
    private void OnSoundVolumeChanged(float value)
    {
        soundVolumePercent = value;
        if (soundSource != null)
            soundSource.volume = isSoundOn ? (value / 100f) : 0f;
        SaveSettings();
    }
    
    private void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        
        if (musicSource != null)
        {
            if (isMusicOn)
            {
                // Включаем музыку
                musicSource.volume = musicVolumePercent / 100f;
                if (musicVolumePercent > 0.01f && !musicSource.isPlaying)
                {
                    StartBackgroundMusic();
                }
            }
            else
            {
                // Выключаем музыку - полностью останавливаем
                if (musicSource.isPlaying)
                {
                    musicSource.Stop();
                }
            }
        }
        
        UpdateMusicButtonState();
        SaveSettings();
    }
    
    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        if (soundSource != null) soundSource.volume = isSoundOn ? (soundVolumePercent / 100f) : 0f;
        UpdateSoundButtonState();
        SaveSettings();
    }
    
    // ============ СОХРАНЕНИЕ И ЗАГРУЗКА ============
    
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumePercent);
        PlayerPrefs.SetFloat("SoundVolume", soundVolumePercent);
        PlayerPrefs.SetInt("CurrentScaleIndex", currentScaleIndex);
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
     private void LoadSettings()
    {
       isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        musicVolumePercent = PlayerPrefs.GetFloat("MusicVolume", 50f);
        soundVolumePercent = PlayerPrefs.GetFloat("SoundVolume", 50f);
        currentScaleIndex = PlayerPrefs.GetInt("CurrentScaleIndex", 0);
        
        // Ограничиваем индекс масштаба
        if (currentScaleIndex >= scaleOptions.Length)
            currentScaleIndex = 0;
            
        // Восстанавливаем полноэкранный режим
        isFullScreen = PlayerPrefs.GetInt("FullScreen", 0) == 1;
        
        // Применяем настройки экрана
        if (isFullScreen)
        {
            Screen.SetResolution(Screen.currentResolution.width, 
                               Screen.currentResolution.height, 
                               FullScreenMode.FullScreenWindow);
        }
        else
        {
            ApplyScale();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void PlayClickSound()
    {
        if (soundSource != null && buttonClickSound != null && isSoundOn && soundVolumePercent > 0.01f)
        {
            soundSource.PlayOneShot(buttonClickSound, soundVolumePercent / 100f);
        }
    }
    
    private void UpdateMusicButtonState()
    {
        if (musicToggleButton != null)
        {
            Image musicImg = musicToggleButton.GetComponent<Image>();
            if (musicImg != null) 
            {
                musicImg.sprite = isMusicOn ? soundOnSprite : soundOffSprite;
            }
        }
    }
    
    private void UpdateSoundButtonState()
    {
        if (soundToggleButton != null)
        {
            Image soundImg = soundToggleButton.GetComponent<Image>();
            if (soundImg != null) 
            {
                soundImg.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
            }
        }
    }
    
    private void StartBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null && 
            isMusicOn && musicVolumePercent > 0.01f && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolumePercent / 100f;
            musicSource.Play();
            isMusicStarted = true;
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            isMusicStarted = false;
        }
    }
    
    // Публичные методы для доступа к настройкам
    public float GetMusicVolume() => musicVolumePercent;
    public float GetSoundVolume() => soundVolumePercent;
    public bool IsMusicOn() => isMusicOn;
    public bool IsSoundOn() => isSoundOn;
    public float GetCurrentScale() => scaleOptions.Length > 0 ? scaleOptions[currentScaleIndex] : 1.0f;
}