// ShopManager.cs 
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI moneyText;
    public Button nextDayButton;
    public Button buyChickenButton;
    public TextMeshProUGUI chickenPriceText;
    
    [Header("Настройки магазина")]
    public int chickenPrice = 70;
    public string nextSceneName = "GameScene";
    
    [Header("Система инвентаря")]
    public bool hasChicken = false;
    private const string CHICKEN_KEY = "HasChicken";
    
    private GameDataManager dataManager;
    
    void Start()
    {
        Debug.Log("ShopManager.Start() вызван");
        
        // Получаем GameDataManager
        dataManager = GameDataManager.Instance;
        
        // Проверяем, инициализирован ли dataManager
        if (dataManager == null)
        {
            Debug.LogError("GameDataManager не найден!");
            return;
        }
        
        // Загружаем состояние покупки курицы
        LoadChickenPurchase();
        
        // Подписываемся на обновление данных
        dataManager.OnDataUpdated += UpdateMoneyDisplay;
        
        // Настраиваем кнопки
        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(StartNextDay);
        }
        
        if (buyChickenButton != null)
        {
            buyChickenButton.onClick.AddListener(BuyChicken);
            UpdateChickenButtonState();
        }
        
        if (chickenPriceText != null)
        {
            chickenPriceText.text = chickenPrice + " руб.";
        }
        
        // Первоначальное обновление денег
        UpdateMoneyDisplay(dataManager.GameData);
    }
    
    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (dataManager != null)
        {
            dataManager.OnDataUpdated -= UpdateMoneyDisplay;
        }
    }
    
    void UpdateMoneyDisplay(GameData data)
    {
        if (moneyText != null)
        {
            moneyText.text = "Деньги: " + data.playerMoney + " руб.";
            Debug.Log($"Shop: Обновлены деньги - {data.playerMoney} руб.");
        }
        else
        {
            Debug.LogWarning("moneyText не назначен!");
        }
        
        // Обновляем состояние кнопки покупки
        UpdateChickenButtonState();
    }
    
    void UpdateChickenButtonState()
    {
        if (buyChickenButton != null && dataManager != null)
        {
            // Проверяем, куплена ли уже курица
            if (hasChicken)
            {
                buyChickenButton.interactable = false;
                var buttonText = buyChickenButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Куплено";
                }
                return;
            }
            
            // Проверяем, хватает ли денег
            bool canAfford = dataManager.GameData.playerMoney >= chickenPrice;
            buyChickenButton.interactable = canAfford;
            
            var buttonText2 = buyChickenButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText2 != null)
            {
                buttonText2.text = canAfford ? "Купить" : "Недостаточно денег";
            }
        }
    }
    
    public void BuyChicken()
    {
        if (dataManager == null) return;
        
        // Проверяем, не куплена ли уже курица
        if (hasChicken)
        {
            Debug.Log("Курица уже куплена!");
            return;
        }
        
        // Проверяем, хватает ли денег
        if (dataManager.GameData.playerMoney < chickenPrice)
        {
            Debug.Log($"Недостаточно денег! Нужно: {chickenPrice}, есть: {dataManager.GameData.playerMoney}");
            return;
        }
        
        // Списываем деньги
        dataManager.AddMoneySpent(chickenPrice);
        
        // Устанавливаем флаг покупки
        hasChicken = true;
        
        // Сохраняем покупку
        SaveChickenPurchase();
        
        Debug.Log($"Курица куплена за {chickenPrice} руб. Осталось денег: {dataManager.GameData.playerMoney}");
        
        // Обновляем кнопку
        UpdateChickenButtonState();
    }
    
   public void StartNextDay()
{
    Debug.Log("=== Начинаем новый день из магазина ===");
    
    if (dataManager != null)
    {
        // Проверяем текущий день перед увеличением
        int currentDayBefore = dataManager.GameData.dayData.currentDay;
        Debug.Log($"Текущий день перед увеличением: {currentDayBefore}");
        
        // Начинаем новый день
        dataManager.StartNewDay();
        
        int currentDayAfter = dataManager.GameData.dayData.currentDay;
        Debug.Log($"День после увеличения: {currentDayAfter}");
        
        Debug.Log($"Разница: {currentDayAfter - currentDayBefore}");
    }
    
    // Загружаем сцену с игрой
    SceneManager.LoadScene(nextSceneName);
}
    
    void SaveChickenPurchase()
    {
      
    dataManager.SaveInventoryPurchase("HasChicken", true);
      
        Debug.Log("Покупка курицы сохранена: " + hasChicken);
    }
    
    void LoadChickenPurchase()
    {
        // Загружаем из PlayerPrefs
        hasChicken = PlayerPrefs.GetInt(CHICKEN_KEY, 0) == 1;
        Debug.Log("Покупка курицы загружена: " + hasChicken);
    }
    
    [ContextMenu("Сбросить покупку курицы")]
    public void ResetChickenPurchase()
    {
        hasChicken = false;
        PlayerPrefs.DeleteKey(CHICKEN_KEY);
        PlayerPrefs.Save();
        UpdateChickenButtonState();
        Debug.Log("Покупка курицы сброшена");
    }
    
    // Для отладки
    [ContextMenu("Обновить отображение денег")]
    void UpdateMoneyManually()
    {
        if (dataManager != null && moneyText != null)
        {
            moneyText.text = "Деньги: " + dataManager.GameData.playerMoney + " руб.";
        }
    }
}