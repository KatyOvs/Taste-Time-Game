using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KitchenRecipeDisplay : MonoBehaviour
{
 
    public GameObject hintPanel; // Панель с рецептом
    public TextMeshProUGUI recipeText; // Текст рецепта
    public TextMeshProUGUI orderNameText; // Название заказа
    public Button closeButton; // Кнопка закрытия

    public Button hintImageButton; // Изображение подсказки 

    void Start()
    {
        // Инициализируем кнопки
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseHintPanel);
        }

        if (hintImageButton != null)
        {
            hintImageButton.onClick.AddListener(OpenHintPanel);
        }

        // Сначала скрываем панель подсказки
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        // Изображение подсказки всегда активно - не скрываем его
        if (hintImageButton != null)
        {
            hintImageButton.gameObject.SetActive(true);
        }
    }

    void OpenHintPanel()
    {
        if (hintPanel != null)
        {
            // Проверяем, есть ли активный заказ
            if (string.IsNullOrEmpty(OrderData.CurrentOrderName))
            {
                // Нет заказа - показываем сообщение
                if (orderNameText != null)
                {
                    orderNameText.text = "Заказ отсутствует";
                }
                
                if (recipeText != null)
                {
                    recipeText.text = "Нет активных заказов. Вернитесь к клиентам!";
                }
            }
            else
            {
                // Есть заказ - показываем информацию
                if (orderNameText != null)
                {
                    orderNameText.text = "Заказ: " + OrderData.CurrentOrderName;
                }

                if (recipeText != null)
                {
                    recipeText.text = OrderData.CurrentRecipe;
                }
            }

            // Показываем панель
            hintPanel.SetActive(true);
        }
    }

    void CloseHintPanel()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    // Метод для принудительного обновления панели (например, после принятия нового заказа)
    public void UpdateHintPanel()
    {
        // Если панель уже открыта - обновляем её содержимое
        if (hintPanel != null && hintPanel.activeSelf)
        {
            if (string.IsNullOrEmpty(OrderData.CurrentOrderName))
            {
                orderNameText.text = "Заказ отсутствует";
                recipeText.text = "Нет активных заказов. Вернитесь к клиентам!";
            }
            else
            {
                orderNameText.text = "Заказ: " + OrderData.CurrentOrderName;
                recipeText.text = OrderData.CurrentRecipe;
            }
        }
    }

    // Метод для очистки данных заказа (после выполнения)
    public void ClearOrderData()
    {
        OrderData.CurrentOrderName = "";
        OrderData.CurrentRecipe = "";
        OrderData.CurrentPrice = 0;
        
        // Обновляем панель, если она открыта
        UpdateHintPanel();
    }
}