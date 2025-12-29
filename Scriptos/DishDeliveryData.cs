// DishDeliveryData.cs
using UnityEngine;

public static class DishDeliveryData
{
    // Приготовленное блюдо для доставки
    public static string DeliveredDishName { get; set; }
    
    // Флаг проверки (правильное/неправильное блюдо)
    public static bool IsCorrectDish { get; set; }
    
    // Сообщение клиента
    public static string CustomerMessage { get; set; }
    
    // Сумма денег (положительная или отрицательная)
    public static int MoneyChange { get; set; }
    
    // Очистка данных
    public static void ClearData()
    {
        DeliveredDishName = "";
        IsCorrectDish = false;
        CustomerMessage = "";
        MoneyChange = 0;
    }
}