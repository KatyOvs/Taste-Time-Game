//OrderDataManager.cs
using UnityEngine;

public static class OrderData
{
    private static string currentOrderName;
    private static string currentRecipe;
    private static int currentPrice;
    
    public static string CurrentOrderName 
    { 
        get { return currentOrderName; }
        set { currentOrderName = value; }
    }
    
    public static string CurrentRecipe 
    { 
        get { return currentRecipe; }
        set { currentRecipe = value; }
    }
    
    public static int CurrentPrice 
    { 
        get { return currentPrice; }
        set { currentPrice = value; }
    }
}