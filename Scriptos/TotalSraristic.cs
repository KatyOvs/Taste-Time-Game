// TotalStatisticsPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TotalStatisticsPanel : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI totalDaysText;
    public TextMeshProUGUI totalMoneyEarnedText;
    public TextMeshProUGUI totalMoneySpentText;
    public TextMeshProUGUI totalProfitText;
    public TextMeshProUGUI totalDishesCookedText;
    public TextMeshProUGUI totalCorrectDishesText;
    public TextMeshProUGUI satisfactionRateText;
    
    [Header("Кнопки")]
    public Button openButton;
    public Button closeButton;
    
    [Header("Панель")]
    public GameObject panel;
    
    private GameDataManager dataManager;
    
    void Start()
    {
        dataManager = GameDataManager.Instance;
        
        // Настраиваем кнопки
        if (openButton != null)
            openButton.onClick.AddListener(ShowPanel);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);
            
        // Скрываем панель по умолчанию
        if (panel != null)
            panel.SetActive(false);
    }
    
    public void ShowPanel()
    {
        if (dataManager == null || dataManager.GameData == null) return;
        
        // Заполняем ОБЩУЮ статистику
        GameData data = dataManager.GameData;
        
        if (totalDaysText != null)
            totalDaysText.text = $"{data.dayData.currentDay}";
        
        if (totalMoneyEarnedText != null)
            totalMoneyEarnedText.text = $"{data.totalMoneyEarned}";
        
        if (totalMoneySpentText != null)
            totalMoneySpentText.text = $"{data.totalMoneySpent}";
        
        if (totalProfitText != null)
            totalProfitText.text = $"{data.GetTotalProfit()}";
        
        if (totalDishesCookedText != null)
            totalDishesCookedText.text = $"{data.totalDishesCooked}";
        
        if (totalCorrectDishesText != null)
            totalCorrectDishesText.text = $"{data.totalDishesLiked}";
        
        if (satisfactionRateText != null)
            satisfactionRateText.text = $"{data.GetTotalSatisfaction():F1}%";
        
        // Показываем панель
        if (panel != null)
        {
            panel.SetActive(true);
            Time.timeScale = 0f; // Пауза при открытии
        }
    }
    
    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            Time.timeScale = 1f; // Возобновляем игру
        }
    }
}