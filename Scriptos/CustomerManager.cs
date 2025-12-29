using System;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CustomerManager : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject customerSprite; // Спрайт клиента
    public GameObject orderCloud; // Облако с текстом
    public TextMeshProUGUI orderText; // Текст заказа в облаке
    public Button acceptButton; // Кнопка "Принять"
    public Button declineButton; // Кнопка "Отказать"
    public TextMeshProUGUI moneyText; // Текст для отображения денег

    [Header("Система доставки")]
    public GameObject dishDropZone; // Область для перетаскивания блюда
    public GameObject deliveredDishPrefab; // Префаб блюда
    public Transform dishSpawnPoint; // Место появления блюда

    [Header("Эмоции клиента")]
    public Sprite happyCustomerSprite;
    public Sprite angryCustomerSprite;
    public Sprite normalCustomerSprite;

    public int money = 100; // Начальное количество денег у игрока

    // Список заказов и текстов из XML
    private XmlDocument dishesXml;
    private XmlNodeList dishNodes;

    private string currentOrderName; // Текущий заказ
    private string currentRecipe;
    private int currentPrice;

    private Animator customerAnimator; // Ссылка на компонент Animator
    private Image customerImage; // Image компонент клиента
    private Canvas canvas;

    // Флаги состояния
    private bool waitingForDelivery = false;
    private bool isProcessingDelivery = false;
private static CustomerManager instance;
private GameDataManager dataManager;

    void Start()
    { 
        dataManager = GameDataManager.Instance;
    
    // Загружаем деньги из сохраненных данных
    money = dataManager.GameData.playerMoney;
        // Находим Canvas 
        canvas = FindFirstObjectByType<Canvas>();
        
        // Скрываем элементы интерфейса при старте
        customerSprite.SetActive(false);
        orderCloud.SetActive(false);
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        if (dishDropZone != null) dishDropZone.SetActive(false);

        // Назначаем действия для кнопок
        acceptButton.onClick.AddListener(AcceptOrder);
        declineButton.onClick.AddListener(DeclineOrder);

        // Инициализируем отображение денег
        UpdateMoneyDisplay();

        // Получаем ссылку на компонент Animator и Image
        customerAnimator = customerSprite.GetComponent<Animator>();
        customerImage = customerSprite.GetComponent<Image>();
        if (customerImage == null)
            customerImage = customerSprite.GetComponentInChildren<Image>();

        // Загружаем XML с блюдами
        LoadDishesFromXML();
    // Проверяем, есть ли доставленное блюдо
    if (!string.IsNullOrEmpty(DishDeliveryData.DeliveredDishName))
    {
        // Есть блюдо для доставки - показываем клиента в режиме ожидания
        StartDeliveryMode();
    }
    else
    {
        // Нет блюда - начинаем новый заказ
        // ОСТАВЛЯЕМ как было - CustomerManager больше не проверяет IsNewGame
        Invoke("SpawnCustomer", 2f); // Задержка перед появлением клиента
    }
    
    dataManager.OnDataUpdated += OnGameDataUpdated;
}
// Метод для обновления UI при изменении данных
void OnGameDataUpdated(GameData data)
{
    money = data.playerMoney;
    UpdateMoneyDisplay();
}
    void LoadDishesFromXML()
    {
        // Создаем XML документ и загружаем данные
        dishesXml = new XmlDocument();
        
        // Загружаем XML файл из Resources
        TextAsset xmlFile = Resources.Load<TextAsset>("dishes");
        if (xmlFile != null)
        {
            dishesXml.LoadXml(xmlFile.text);
            dishNodes = dishesXml.SelectNodes("/dishes/dish");
        }
        else
        {
            Debug.LogError("XML файл dishes.xml не найден в папке Resources!");
        }
    }

 public   void SpawnCustomer()
{
    if (dishNodes == null || dishNodes.Count == 0)
    {
        Debug.LogError("Нет данных о блюдах!");
        return;
    }

    // Сбрасываем состояние доставки
    waitingForDelivery = false;
    isProcessingDelivery = false;

    // Проверяем, куплена ли курица
    bool hasChicken = PlayerPrefs.GetInt("HasChicken", 0) == 1;
    
    XmlNode selectedDish = null;
    int attempts = 0;
    int maxAttempts = 10; // Максимальное количество попыток найти подходящее блюдо
    
    // Пытаемся найти подходящее блюдо
    while (selectedDish == null && attempts < maxAttempts)
    {
        int randomIndex = UnityEngine.Random.Range(0, dishNodes.Count);
        XmlNode candidateDish = dishNodes[randomIndex];
        string dishName = candidateDish.SelectSingleNode("name").InnerText.ToLower();
        
        // Если блюдо содержит курицу, но курица не куплена - пропускаем
        if ((dishName.Contains("куриц") || dishName.Contains("chicken")) && !hasChicken)
        {
            attempts++;
            Debug.Log($"Пропускаем блюдо с курицей: {dishName}");
            continue;
        }
        
        selectedDish = candidateDish;
    }
    
    // Если не нашли подходящее блюдо, берем первое без курицы
    if (selectedDish == null)
    {
        Debug.LogWarning("Не найдено подходящего блюда, ищем любое без курицы");
        foreach (XmlNode dishNode in dishNodes)
        {
            string dishName = dishNode.SelectSingleNode("name").InnerText.ToLower();
            if (!dishName.Contains("куриц") && !dishName.Contains("chicken"))
            {
                selectedDish = dishNode;
                break;
            }
        }
        
        // Если все блюда с курицей, берем первое
        if (selectedDish == null && dishNodes.Count > 0)
        {
            selectedDish = dishNodes[0];
            Debug.LogWarning("Все блюда содержат курицу, показываем первое");
        }
    }

    if (selectedDish == null)
    {
        Debug.LogError("Не удалось выбрать блюдо!");
        return;
    }

    // Сохраняем данные о заказе
    currentOrderName = selectedDish.SelectSingleNode("name").InnerText;
    string orderTextStr = selectedDish.SelectSingleNode("orderText").InnerText;
    currentRecipe = selectedDish.SelectSingleNode("recipe").InnerText;
    currentPrice = int.Parse(selectedDish.SelectSingleNode("price").InnerText);

    // Показываем клиента с обычным спрайтом
    customerSprite.SetActive(true);
    if (customerImage != null && normalCustomerSprite != null)
        customerImage.sprite = normalCustomerSprite;

    // Задержка перед появлением облака с текстом и кнопками
    Invoke("ShowOrderUI", 1f);

    // Устанавливаем текст заказа
    orderText.text = orderTextStr;
    
    Debug.Log($"Выбран заказ: {currentOrderName} (Цена: {currentPrice})");
}

    void StartDeliveryMode()
{
    Debug.Log("Режим доставки активирован. Блюдо: " + DishDeliveryData.DeliveredDishName);
Debug.Log("=== StartDeliveryMode ===");
    Debug.Log($"Доставка блюда: {DishDeliveryData.DeliveredDishName}");
    
    // Всегда берем из OrderData
    currentOrderName = OrderData.CurrentOrderName;
    currentRecipe = OrderData.CurrentRecipe;
    currentPrice = OrderData.CurrentPrice;
    
    Debug.Log($"Восстановлено из OrderData:");
    Debug.Log($"- Заказ: {currentOrderName}");
    Debug.Log($"- Цена: {currentPrice}");
    Debug.Log($"OrderData.CurrentPrice = {OrderData.CurrentPrice}");
    // ВАЖНО: Нужно получить текущий заказ из сохраненных данных
    if (string.IsNullOrEmpty(currentOrderName))
    {
        // Если нет текущего заказа, возьмите его из OrderData
        if (!string.IsNullOrEmpty(OrderData.CurrentOrderName))
        {
            currentOrderName = OrderData.CurrentOrderName;
            currentRecipe = OrderData.CurrentRecipe;
            currentPrice = OrderData.CurrentPrice;
            Debug.Log($"Восстановлен заказ: {currentOrderName}");
        }
        else
        {
            // Или создайте временный заказ для теста
            Debug.LogWarning("Нет сохраненного заказа! Использую тестовый.");
            currentOrderName = "Картошка вареная"; // Тестовый
            currentPrice = 12;
        }
    }

    // Показываем клиента БЕЗ анимации входа
    customerSprite.SetActive(true);
    
    // СНАЧАЛА устанавливаем спрайт нормального клиента
    if (customerImage != null && normalCustomerSprite != null)
        customerImage.sprite = normalCustomerSprite;

    // НИКАКИХ анимаций триггеров - клиент просто стоит

    // Скрываем кнопки заказа
    orderCloud.SetActive(false);
    acceptButton.gameObject.SetActive(false);
    declineButton.gameObject.SetActive(false);

    // Показываем дроп-зону
    if (dishDropZone != null)
        dishDropZone.SetActive(true);

    // Создаем блюдо для перетаскивания
    CreateDeliveredDish();

    // Устанавливаем ожидание доставки
    waitingForDelivery = true;
    isProcessingDelivery = false;
    
    Debug.Log($"Режим доставки установлен. Ожидание блюда: {DishDeliveryData.DeliveredDishName} для заказа: {currentOrderName}");
}
   void CreateDeliveredDish()
{
    if (deliveredDishPrefab == null)
    {
        Debug.LogError("DeliveredDish prefab is not assigned!");
        return;
    }

    // Создаем экземпляр блюда
    GameObject dish = Instantiate(deliveredDishPrefab, dishSpawnPoint.position, Quaternion.identity);
    dish.transform.SetParent(canvas.transform, false);
    dish.name = "DeliveredDish";
    
    // Получаем RectTransform блюда
    RectTransform dishRT = dish.GetComponent<RectTransform>();
    
    // ЖЕСТКО задаем координаты 221, -401
    dishRT.anchoredPosition = new Vector2(221f, -401f);
    dishRT.localScale = Vector3.one;

    // Загружаем спрайт блюда
    string dishName = DishDeliveryData.DeliveredDishName;
    Sprite dishSprite = LoadDishSprite(dishName);
    
    if (dishSprite != null)
    {
        Image dishImage = dish.GetComponent<Image>();
        if (dishImage != null)
        {
            // Устанавливаем спрайт
            dishImage.sprite = dishSprite;
            dishImage.preserveAspect = true;
            
            // Устанавливаем разумный размер
            dishImage.rectTransform.sizeDelta = new Vector2(300, 300);
            
            Debug.Log($"Спрайт установлен: {dishSprite.name}");
        }
        else
        {
            Debug.LogError("No Image component on dish prefab!");
        }
    }
    else
    {
        Debug.LogError($"Не удалось загрузить спрайт для блюда: {dishName}");
        
        // Создаем текстовую метку для отладки
        GameObject textObj = new GameObject("DishLabel");
        textObj.transform.SetParent(dish.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = dishName;
        text.fontSize = 16;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;
    }

    // Добавляем скрипт для перетаскивания
    DragDeliveredDish dragScript = dish.AddComponent<DragDeliveredDish>();
    dragScript.customerManager = this;
    dragScript.dropZone = dishDropZone;

    Debug.Log("Блюдо создано: " + dishName);
}
    void ShowOrderUI()
    {
        // Показываем облако с текстом и кнопки
        orderCloud.SetActive(true);
        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);
    }

    void AcceptOrder()
{
     OrderData.CurrentOrderName = currentOrderName;
    OrderData.CurrentRecipe = currentRecipe;
    OrderData.CurrentPrice = currentPrice;

    // Сохраняем через GameDataManager
    dataManager.AddMoneyEarned(currentPrice);
    
    Debug.Log($"Заказ принят: +{currentPrice} руб.");
    
    // Обновляем локальную переменную
    money = dataManager.GameData.playerMoney;
    UpdateMoneyDisplay();
    
    SceneManager.LoadScene("KitchenScene");
}

    void DeclineOrder()
    {
        // Запускаем анимацию ухода
        customerAnimator.SetTrigger("Exit");

        // Скрываем элементы интерфейса после завершения анимации
        Invoke("HideCustomer", 1f);
        Invoke("SpawnCustomer", 2f);
    }

    // Метод вызываемый при перетаскивании блюда на клиента
    public void OnDishDelivered()
    {
        if (isProcessingDelivery || !waitingForDelivery) return;

        isProcessingDelivery = true;
        waitingForDelivery = false;

        // Скрываем дроп-зону
        if (dishDropZone != null)
            dishDropZone.SetActive(false);

        // Проверяем блюдо
        CheckDish();
    }
void CheckDish()
{
    if (string.IsNullOrEmpty(DishDeliveryData.DeliveredDishName))
    {
        Debug.LogError("DishDeliveryData.DeliveredDishName пустой!");
        return;
    }
    
    if (string.IsNullOrEmpty(currentOrderName))
    {
        Debug.LogError("currentOrderName пустой!");
        return;
    }

    string deliveredDish = DishDeliveryData.DeliveredDishName.ToLower();
    string orderedDish = currentOrderName.ToLower();

    Debug.Log($"Проверка доставки: '{deliveredDish}' vs заказ: '{orderedDish}'");

    // Нормализуем названия для сравнения
    deliveredDish = NormalizeDishName(deliveredDish);
    orderedDish = NormalizeDishName(orderedDish);
    
    Debug.Log($"После нормализации: '{deliveredDish}' vs '{orderedDish}'");

    // Более гибкое сравнение
    bool isCorrect = CheckDishMatch(deliveredDish, orderedDish);

    Debug.Log($"Результат проверки: {isCorrect}");

    // Обрабатываем результат
 if (isCorrect)
{
    // Блюдо понравилось - добавляем в дневную и общую статистику
    dataManager.AddDishLiked(DishDeliveryData.DeliveredDishName);
    orderText.text = "Великолепно! Именно то, что нужно. Спасибо!";
    Debug.Log($"Правильное блюдо! Клиент доволен.");
     if (happyCustomerSprite != null)
                customerImage.sprite = happyCustomerSprite;

    // Добавляем ЗАРАБОТАННЫЕ деньги
    dataManager.AddMoneyEarned(currentPrice);
}
else
{
    // Блюдо не понравилось - добавляем в дневную и общую статистику
    dataManager.AddDishCooked(DishDeliveryData.DeliveredDishName);
    dataManager.AddMoneySpent(currentPrice); // ТРАТЫ (возврат денег)
    orderText.text = "Это не мой заказ... Возвращаю деньги!\n-" + currentPrice + " руб.";
    Debug.Log($"Неправильное блюдо! Возврат {-currentPrice} руб.");
     if (angryCustomerSprite != null)
                customerImage.sprite =angryCustomerSprite;
 
}
    
    // Всегда добавляем в статистику приготовленных блюд
    dataManager.AddDishCooked(DishDeliveryData.DeliveredDishName);
    
    // Обновляем локальные деньги
    money = dataManager.GameData.playerMoney;
    UpdateMoneyDisplay();
    // Показываем отзыв (используем то же облако)
    orderCloud.SetActive(true);

    // Очищаем данные доставки
    DishDeliveryData.ClearData();

    // Автоматически продолжаем через время
    Invoke("ContinueAfterFeedback", 3f);
}

// Новый метод для нормализации названий
// CustomerManager.cs - метод NormalizeDishName
string NormalizeDishName(string dishName)
{
    if (string.IsNullOrEmpty(dishName))
        return dishName;
    
    string normalized = dishName.ToLower();
    
    // Убираем лишние символы
    normalized = normalized.Replace("_", " ").Replace("-", " ").Replace(".", " ");
    
    // Исправляем опечатки/вариации
    normalized = normalized.Replace("варена", "варен");
    normalized = normalized.Replace("жарена", "жарен");
    normalized = normalized.Replace("картошка", "картошк");
    normalized = normalized.Replace("картофель", "картошк");
    normalized = normalized.Replace("драник", "драни");
    normalized = normalized.Replace("с курицей", "куриц");
    normalized = normalized.Replace("курица", "куриц");
    
    // Убираем множественные пробелы
    while (normalized.Contains("  "))
        normalized = normalized.Replace("  ", " ");
    
    return normalized.Trim();
}
// Новый метод для гибкого сравнения
// CustomerManager.cs - новый метод CheckDishMatch
bool CheckDishMatch(string delivered, string ordered)
{
    if (delivered == ordered)
        return true;
    
    // Улучшенное сравнение для картошки с курицей
    bool deliveredHasChicken = delivered.Contains("куриц") || delivered.Contains("chicken");
    bool orderedHasChicken = ordered.Contains("куриц") || ordered.Contains("chicken");
    
    // Если в заказе есть курица, а в доставленном блюде нет - это ошибка
    if (orderedHasChicken && !deliveredHasChicken)
    {
        Debug.Log("ОШИБКА: В заказе есть курица, а в доставленном блюде нет!");
        return false;
    }
    
    // Если в доставленном блюде есть курица, а в заказе нет - это тоже ошибка
    if (!orderedHasChicken && deliveredHasChicken)
    {
        Debug.Log("ОШИБКА: В доставленном блюде есть курица, а в заказе нет!");
        return false;
    }
    
    // Проверяем частичное совпадение (только если с курицей все совпало)
    if (delivered.Contains(ordered) || ordered.Contains(delivered))
        return true;
    
    // Проверяем ключевые слова
    string[] deliveredWords = delivered.Split(' ');
    string[] orderedWords = ordered.Split(' ');
    
    int matchCount = 0;
    foreach (string dw in deliveredWords)
    {
        foreach (string ow in orderedWords)
        {
            if (dw.Contains(ow) || ow.Contains(dw))
            {
                matchCount++;
                break;
            }
        }
    }
    
    // Если совпало больше половины слов
    int minWords = Mathf.Min(deliveredWords.Length, orderedWords.Length);
    return matchCount >= minWords * 0.7f;
}

    void ContinueAfterFeedback()
    {
        HideCustomer();
        
        // Новый клиент через задержку
        Invoke("SpawnCustomer", 1.5f);
    }

    void HideCustomer()
    {
        customerSprite.SetActive(false);
        orderCloud.SetActive(false);
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        if (dishDropZone != null) dishDropZone.SetActive(false);
    }

    void UpdateMoneyDisplay()
{
    // ВАЖНО: Всегда берем актуальные деньги из dataManager
    if (dataManager != null && dataManager.GameData != null)
    {
        money = dataManager.GameData.playerMoney;
    }
    
    // Обновляем текст
    moneyText.text = "Деньги: " + money.ToString() + " руб.";
    
    // Отладочная печать
    Debug.Log($"UpdateMoneyDisplay: {money} руб.");
}
  // CustomerManager.cs - метод LoadDishSprite
Sprite LoadDishSprite(string dishName)
{
    Debug.Log($"=== Пытаемся загрузить спрайт для блюда: {dishName} ===");
    
    string spriteName = "";
    
    if (dishName.ToLower().Contains("картошка") && dishName.ToLower().Contains("куриц"))
        spriteName = "картошка_курица_блюдо";
    else if (dishName.ToLower().Contains("картошка"))
        spriteName = "картошка_блюдо";
    else if (dishName.ToLower().Contains("драник"))
        spriteName = "драники_блюдо";
    else
        spriteName = dishName + "_блюдо";
    
    Debug.Log($"Ищем спрайт: {spriteName}");
    
    // Пробуем загрузить напрямую из Resources
    Sprite sprite = Resources.Load<Sprite>(spriteName);
    
    if (sprite == null)
    {
        // Пробуем с .png
        sprite = Resources.Load<Sprite>(spriteName + ".png");
    }
    
    if (sprite != null)
    {
        Debug.Log($"Успешно загружен: {sprite.name}");
        return sprite;
    }
    
    // Отладочная информация
    Debug.LogError($"Не найден спрайт: {spriteName}");
    
    // Проверяем, что вообще есть в Resources
    Sprite[] allSprites = Resources.LoadAll<Sprite>("");
    Debug.Log($"Всего в Resources: {allSprites.Length} спрайтов");
    
    foreach (Sprite s in allSprites)
        Debug.Log($"- {s.name}");
    
    // Создаем тестовый
    return CreateTestSprite(spriteName);
}
// Вспомогательный метод для создания тестового спрайта
Sprite CreateTestSprite(string spriteName)
{
    // Создаем текстуру 64x64
    Texture2D texture = new Texture2D(64, 64);
    
    // Заполняем цветом
    Color color = spriteName.Contains("драник") ? Color.yellow : Color.green;
    Color[] pixels = new Color[64 * 64];
    for (int i = 0; i < pixels.Length; i++)
    {
        pixels[i] = color;
    }
    texture.SetPixels(pixels);
    texture.Apply();
    
    // Создаем спрайт
    Sprite testSprite = Sprite.Create(
        texture, 
        new Rect(0, 0, 64, 64), 
        new Vector2(0.5f, 0.5f)
    );
    testSprite.name = spriteName + "_test";
    
    Debug.Log($"Создан тестовый спрайт: {testSprite.name}");
    return testSprite;
}

}