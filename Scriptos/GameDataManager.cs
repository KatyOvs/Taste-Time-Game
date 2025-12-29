using UnityEngine;

using System.IO;

using System;



public class GameDataManager : MonoBehaviour

{

    private static GameDataManager instance;

    private GameData gameData;

    

    // События для обновления UI

    public event Action<GameData> OnDataUpdated;

    

    public static GameDataManager Instance

    {

        get

        {

            if (instance == null)

            {

                GameObject obj = new GameObject("GameDataManager");

                instance = obj.AddComponent<GameDataManager>();

                DontDestroyOnLoad(obj);

                instance.Initialize();

            }

            return instance;

        }

    }

    

    public GameData GameData

    {

        get { return gameData; }

    }

    

    void Initialize()

    {

        LoadGameData();

    }

    

    void Awake()

    {

        if (instance == null)

        {

            instance = this;

            DontDestroyOnLoad(gameObject);

            LoadGameData();

        }

        else if (instance != this)

        {

            Destroy(gameObject);

        }

    }

    

    // Сохранение данных

    public void SaveGameData()

    {

        string filePath = GetSaveFilePath();

        string jsonData = JsonUtility.ToJson(gameData, true);

        

        try

        {

            File.WriteAllText(filePath, jsonData);

            Debug.Log($"Игра сохранена: {filePath}");

        }

        catch (Exception e)

        {

            Debug.LogError($"Ошибка сохранения: {e.Message}");

        }

    }

    

    // Загрузка данных

    public void LoadGameData()

    {

        string filePath = GetSaveFilePath();

        

        if (File.Exists(filePath))

        {

            try

            {

                string jsonData = File.ReadAllText(filePath);

                gameData = JsonUtility.FromJson<GameData>(jsonData);

                Debug.Log($"Игра загружена: {filePath}");

            }

            catch (Exception e)

            {

                Debug.LogError($"Ошибка загрузки: {e.Message}");

                CreateNewGameData();

            }

        }

        else

        {

            CreateNewGameData();

        }

        

        // ВАЖНО: убеждаемся, что dayData инициализирован

        if (gameData.dayData == null)

        {

            Debug.Log("DayData равен null, инициализируем...");

            gameData.dayData = new DayData();

        }

        

        Debug.Log($"Загружено время: День {gameData.dayData.currentDay}, {gameData.dayData.currentHour:D2}:{gameData.dayData.currentMinute:D2}");

    }

    

    // Создание новых данных

    void CreateNewGameData()

    {

        gameData = new GameData();

        // ВАЖНО: нужно явно создать DayData

        gameData.dayData = new DayData()

        {

            currentDay = 1,

            currentHour = 10,

            currentMinute = 0,

            isDayActive = true

        };

        Debug.Log("Созданы новые данные игры");

        Debug.Log($"Начальное время: День {gameData.dayData.currentDay}, {gameData.dayData.currentHour:D2}:{gameData.dayData.currentMinute:D2}");

    }

    

    string GetSaveFilePath()

    {

        return Path.Combine(Application.persistentDataPath, "game_save.json");

    }

    

    // Методы для обновления статистики

    public void AddDishCooked(string dishName)

    {

        gameData.AddDishCooked(dishName);

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    
public void SaveInventoryPurchase(string itemKey, bool isPurchased)
{
    PlayerPrefs.SetInt(itemKey, isPurchased ? 1 : 0);
    PlayerPrefs.Save();
    Debug.Log($"Покупка сохранена: {itemKey} = {isPurchased}");
}

public bool LoadInventoryPurchase(string itemKey)
{
    bool isPurchased = PlayerPrefs.GetInt(itemKey, 0) == 1;
    Debug.Log($"Покупка загружена: {itemKey} = {isPurchased}");
    return isPurchased;
}

    public void AddDishLiked(string dishName)

    {

        gameData.AddDishLiked(dishName);

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    

    public void AddMoneyEarned(int amount)

    {

        gameData.AddMoneyEarned(amount);

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    

    public void AddMoneySpent(int amount)

    {

        gameData.AddMoneySpent(amount);

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    

    public void SetPlayerMoney(int money)

    {

        gameData.playerMoney = money;

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    

    public void NextDay()
{
    // Вместо ResetDailyStats() вызывайте у gameData
    if (gameData != null)
    {
        gameData.ResetDailyStats();
    }
    
    SaveGameData();
    OnDataUpdated?.Invoke(gameData);
}
    

    // Сохранить время дня

    public void SaveDayTime(int hour, int minute, int day)

    {

        if (gameData.dayData == null)

        {

            Debug.Log("DayData равен null, создаем новый");

            gameData.dayData = new DayData();

        }

            

        gameData.dayData.currentHour = hour;

        gameData.dayData.currentMinute = minute;

        gameData.dayData.currentDay = day;

        gameData.dayData.isDayActive = true;

        

        Debug.Log($"SaveDayTime: сохраняем День {day}, {hour:D2}:{minute:D2}");

        

        SaveGameData();

        OnDataUpdated?.Invoke(gameData);

    }

    

    // Проверить, активен ли день

    public bool IsDayActive()

    {

        return gameData.dayData != null && gameData.dayData.isDayActive;

    }

    

    // Завершить день

    public void EndDay()

    {

        if (gameData.dayData != null)

        {

            gameData.dayData.isDayActive = false;

            SaveGameData();

            Debug.Log($"День завершен: День {gameData.dayData.currentDay}");

        }

    }

    

   // В GameDataManager.cs

public void StartNewDay()
{
    if (gameData.dayData == null)
    {
        gameData.dayData = new DayData();
    }
    
    // ЗАЩИТА: проверяем, не начат ли уже день
    // Если день активен (isDayActive == true), значит новый день уже начат
    if (gameData.dayData.isDayActive)
    {
        Debug.LogWarning($"День {gameData.dayData.currentDay} уже активен! Не увеличиваем день.");
        return; // Выходим, чтобы не увеличивать день повторно
    }
    
    // Увеличиваем день только если предыдущий день завершен
    gameData.dayData.currentDay++;
    Debug.Log($"День увеличен до: {gameData.dayData.currentDay}");
    
    // Сбрасываем дневную статистику
    if (gameData != null)
    {
        gameData.ResetDailyStats();
    }
    
    // Сбрасываем время на начало дня
    gameData.dayData.currentHour = 10;
    gameData.dayData.currentMinute = 0;
    gameData.dayData.isDayActive = true;
    
    SaveGameData();
    Debug.Log($"Начат новый день: День {gameData.dayData.currentDay}, 10:00");
    
    // Вызываем событие обновления
    OnDataUpdated?.Invoke(gameData);
}

    // Удалить сохранения

    [ContextMenu("Удалить сохранения")]

    public void DeleteSaveData()

    {

        string filePath = GetSaveFilePath();

        if (File.Exists(filePath))

        {

            File.Delete(filePath);

            Debug.Log("Сохранения удалены");

            CreateNewGameData();

        }

    }

    

    // Для отладки

    [ContextMenu("Показать текущее время")]

    public void ShowCurrentTime()

    {

        if (gameData.dayData != null)

        {

            Debug.Log($"Текущее время: День {gameData.dayData.currentDay}, {gameData.dayData.currentHour:D2}:{gameData.dayData.currentMinute:D2}");

        }

        else

        {

            Debug.Log("DayData равен null!");

        }

    }

}