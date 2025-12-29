using UnityEngine;
using TMPro;

public class DayTimerController : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public ResultsOfDayPanel resultsPanel;
    
    private float realTimer = 0f;
    private bool isDayEnded = false;
    
    void Start()
    {
       // Подписываемся на событие обновления данных
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager != null)
        {
            dataManager.OnDataUpdated += OnGameDataUpdated;
        }
        
        Invoke(nameof(InitializeTimer), 0.0f);
    }
    
    void InitializeTimer()
    {
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager == null) return;
        
        UpdateTimerDisplay();
        
        // Если время уже 17:00 или больше - показываем панель
        if (dataManager.GameData.dayData.currentHour >= 17)
        {
            EndDay(dataManager);
        }
    }
    
    void Update()
    {
        if (isDayEnded || resultsPanel == null) return;
        
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager == null || !dataManager.IsDayActive()) return;
        
        if (resultsPanel.gameObject.activeSelf)
        {
            isDayEnded = true;
            return;
        }
        
        realTimer += Time.deltaTime;
        
        if (realTimer >= 5f)
        {
            AddGameTime(dataManager, 0, 30);
            realTimer = 0f;
        }
    }
    
    void AddGameTime(GameDataManager dataManager, int addHours, int addMinutes)
    {
        int hours = dataManager.GameData.dayData.currentHour;
        int minutes = dataManager.GameData.dayData.currentMinute;
        
        minutes += addMinutes;
        hours += addHours;
        
        if (minutes >= 60)
        {
            hours += minutes / 60;
            minutes = minutes % 60;
        }
        
        // Сохраняем новое время
        dataManager.GameData.dayData.currentHour = hours;
        dataManager.GameData.dayData.currentMinute = minutes;
        
        // Если 17:00 или больше - завершаем день
        if (hours >= 17)
        {
            EndDay(dataManager);
            return;
        }
        
        // Иначе просто сохраняем и обновляем
        dataManager.SaveGameData();
        UpdateTimerDisplay();
    }
    
    void EndDay(GameDataManager dataManager)
    {
        if (isDayEnded) return;
        
        Debug.Log($"День завершен в {dataManager.GameData.dayData.currentHour:D2}:{dataManager.GameData.dayData.currentMinute:D2}");
        
        isDayEnded = true;
        
        // Завершаем день (устанавливаем isDayActive = false)
        dataManager.EndDay();
        dataManager.SaveGameData();
        
        // Показываем панель результатов
        if (resultsPanel != null)
        {
            resultsPanel.ShowResults();
        }
        
        UpdateTimerDisplay();
    }
    
    public void ResetForNewDay()
    {
        Debug.Log("Сбрасываем таймер для нового дня");
        
        isDayEnded = false;
        realTimer = 0f;
        UpdateTimerDisplay();
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager == null || dataManager.GameData == null || dataManager.GameData.dayData == null)
        {
            timerText.text = "Загрузка...";
            return;
        }
        
        timerText.text = $"День {dataManager.GameData.dayData.currentDay}, " +
                        $"{dataManager.GameData.dayData.currentHour:D2}:{dataManager.GameData.dayData.currentMinute:D2}";
    }
    
    private GameDataManager FindGameDataManager()
    {
        GameDataManager manager = GameDataManager.Instance;
        return manager ?? FindFirstObjectByType<GameDataManager>();
    }
    void OnGameDataUpdated(GameData gameData)
    {
        Debug.Log("DayTimerController: данные обновлены, обновляем таймер");
        UpdateTimerDisplay();
        
        // Если день стал активным после обновления, сбрасываем флаг
        if (gameData.dayData != null && gameData.dayData.isDayActive)
        {
            isDayEnded = false;
        }
    }
    void OnDestroy()
    {
        // Отписываемся при уничтожении объекта
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager != null)
        {
            dataManager.OnDataUpdated -= OnGameDataUpdated;
        }
    }
}