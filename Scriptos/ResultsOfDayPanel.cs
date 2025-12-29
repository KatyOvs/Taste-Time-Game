using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsOfDayPanel : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI moneyEarnedText;
    public TextMeshProUGUI moneySpentText;
    public TextMeshProUGUI totalText;
    public TextMeshProUGUI dishesCookedText;
    public TextMeshProUGUI correctDishesText;
    
    [Header("Кнопки")]
    public Button shopButton;
    public Button nextDayButton;
    
    [Header("Ссылки")]
    public DayTimerController dayTimerController; // Добавьте эту ссылку!
    
    [Header("Сцены")]
    public string shopSceneName = "Shop";
    
    void Awake()
    {
        Debug.Log($"ResultsOfDayPanel.Awake() вызван. GameObject активен: {gameObject.activeSelf}");
    }
    
    void Start()
    {
        Debug.Log($"ResultsOfDayPanel.Start() вызван. GameObject активен: {gameObject.activeSelf}");
        
        // Если dayTimerController не назначен, пытаемся найти
        if (dayTimerController == null)
        {
            dayTimerController = FindFirstObjectByType<DayTimerController>();
            if (dayTimerController != null)
                Debug.Log("DayTimerController найден автоматически");
        }
        
        // Настраиваем кнопки
        if (shopButton != null)
            shopButton.onClick.AddListener(GoToShop);
        
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(StartNextDay);
    }
    
    // ResultsOfDayPanel.cs - обновленный метод ShowResults()
    public void ShowResults()
    {
        // Получаем GameDataManager
        GameDataManager dataManager = FindGameDataManager();
        if (dataManager == null) return;
        
        GameData data = dataManager.GameData;
        
        // Заполняем ДНЕВНУЮ статистику (обратите внимание - используем daily*)
        if (dayText != null)
            dayText.text = $"{data.dayData.currentDay}";
        
        if (moneyEarnedText != null)
            moneyEarnedText.text = $"+{data.dailyMoneyEarned}";
        
        if (moneySpentText != null)
            moneySpentText.text = $"-{data.dailyMoneySpent}";
        
        if (totalText != null)
            totalText.text = $"{data.GetDailyProfit()}";
        
        if (dishesCookedText != null)
            dishesCookedText.text = $"{data.dailyDishesCooked}";
        
        if (correctDishesText != null)
            correctDishesText.text = $"{data.dailyDishesLiked}";
        
        // Показываем панель
        gameObject.SetActive(true);
        
        // Ставим игру на паузу
        Time.timeScale = 0f;
    }
    
    private GameDataManager FindGameDataManager()
    {
        // Ищем GameDataManager разными способами
        GameDataManager manager = GameDataManager.Instance;
        
        if (manager == null)
        {
            manager = FindFirstObjectByType<GameDataManager>();
        }
        
        if (manager == null)
        {
            Debug.LogError("GameDataManager не найден!");
            return null;
        }
        
        if (manager.GameData == null)
        {
            Debug.LogError("GameData не инициализирован!");
            return null;
        }
        
        return manager;
    }
    
    private void GoToShop()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(shopSceneName);
    }
    
   private void StartNextDay()
{
    Debug.Log("=== Начинаем новый день из панели результатов ===");
    
    Time.timeScale = 1f;
    
    GameDataManager dataManager = FindGameDataManager();
    if (dataManager != null)
    {
        int currentDayBefore = dataManager.GameData.dayData.currentDay;
        Debug.Log($"Текущий день перед увеличением: {currentDayBefore}");
        
        // Начинаем новый день
        dataManager.StartNewDay();
        
        int currentDayAfter = dataManager.GameData.dayData.currentDay;
        Debug.Log($"День после увеличения: {currentDayAfter}");
        
        Debug.Log($"Разница: {currentDayAfter - currentDayBefore}");
        
        // Форсируем сохранение
        dataManager.SaveGameData();
    }
    
    // Скрываем панель
    gameObject.SetActive(false);
    
    // ЗАГРУЖАЕМ СЦЕНУ ИГРЫ, как делает магазин
    SceneManager.LoadScene("GameScene"); // укажите имя вашей игровой сцены
}
}