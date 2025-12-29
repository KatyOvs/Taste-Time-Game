// GameData.cs
using System.Collections.Generic;

[System.Serializable]
public class DayData
{
    public int currentDay = 1;
    public int currentHour = 10;
    public int currentMinute = 0;
    public bool isDayActive = true;
}

[System.Serializable]
public class GameData
{
    // Основные данные
    public int playerMoney = 100;
    
    // ОБЩАЯ статистика (за все время)
    public int totalDishesCooked = 0;
    public int totalDishesLiked = 0;
    public int totalMoneyEarned = 0;
    public int totalMoneySpent = 0;
    public List<string> cookedDishes = new List<string>();
    public List<string> likedDishes = new List<string>();
    
    // ДНЕВНАЯ статистика (только за текущий день)
    public int dailyDishesCooked = 0;
    public int dailyDishesLiked = 0;
    public int dailyMoneyEarned = 0;
    public int dailyMoneySpent = 0;
    
    public DayData dayData = new DayData();
    
    // Методы для обновления ОБЩЕЙ статистики
    public void AddDishCooked(string dishName)
    {
        totalDishesCooked++;
        dailyDishesCooked++;
        cookedDishes.Add(dishName);
    }
    
    public void AddDishLiked(string dishName)
    {
        totalDishesLiked++;
        dailyDishesLiked++;
        likedDishes.Add(dishName);
    }
    
    public void AddMoneyEarned(int amount)
    {
        if (amount > 0)
        {
            totalMoneyEarned += amount;
            dailyMoneyEarned += amount;
            playerMoney += amount;
        }
    }
    
    public void AddMoneySpent(int amount)
    {
        if (amount > 0)
        {
            totalMoneySpent += amount;
            dailyMoneySpent += amount;
            playerMoney -= amount;
        }
    }
    
    // Сброс дневной статистики
    public void ResetDailyStats()
    {
        dailyDishesCooked = 0;
        dailyDishesLiked = 0;
        dailyMoneyEarned = 0;
        dailyMoneySpent = 0;
    }
    
    // Метод для начала нового дня
    public void NextDay()
    {
        // Сначала сбрасываем дневную статистику
        ResetDailyStats();
        
        // Затем увеличиваем день (если нужно)
        if (dayData != null)
        {
            dayData.currentDay++;
            dayData.currentHour = 10;
            dayData.currentMinute = 0;
            dayData.isDayActive = true;
        }
    }
    
    // Геттеры для статистики
    public int GetDailyProfit()
    {
        return dailyMoneyEarned - dailyMoneySpent;
    }
    
    public float GetDailySatisfaction()
    {
        if (dailyDishesCooked == 0) return 0;
        return (float)dailyDishesLiked / dailyDishesCooked * 100f;
    }
    
    public int GetTotalProfit()
    {
        return totalMoneyEarned - totalMoneySpent;
    }
    
    public float GetTotalSatisfaction()
    {
        if (totalDishesCooked == 0) return 0;
        return (float)totalDishesLiked / totalDishesCooked * 100f;
    }
}

public static class StaticData
{
    public static bool IsNewGame { get; set; } = false;
}