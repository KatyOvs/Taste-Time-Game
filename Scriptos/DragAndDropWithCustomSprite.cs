using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DragAndDropWithCustomSprite : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite dragSprite; // Sprite to display during dragging
    
    // Product states
    public Sprite cutSprite; // Нарезанный вид
    public Sprite gratedSprite; // Тертый вид
    public bool autoStartCooking = false; // Автоматически начинать готовку
    public bool manualStartRequired = true; // Требуется ручной запуск
    public Button cookButton; // Кнопка для запуска готовки (опционально)
    // Object type identification
    [Header("Тип объекта")]
    public bool isProduct = false; // Является ли объект продуктом
    public bool isKnife = false;
    public bool isGrater = false;
    public bool isPan = false; // Сковорода
    public bool isPot = false; // Кастрюля
    public bool isPlate = false; // Тарелка
    public bool isBoard = false; // Доска
    [Header("Система доставки")]
    public Button deliverButton; // Кнопка "Отнести посетителю"
    private bool dishReady = false; // Флаг готовности блюда
    private string cookedDishName = ""; // Название приготовленного блюда
    // Cooking settings
    public Sprite panCookingSprite; // Спрайт сковороды во время готовки
    public Sprite panNormalSprite; // Обычный спрайт сковороды
    public Sprite potCookingSprite; // Спрайт кастрюли во время готовки
    public Sprite potNormalSprite; // Обычный спрайт кастрюли
    
    // Product combinations for dishes
    [System.Serializable]
    public class DishRecipe
    {
        public string dishName;
        public List<string> requiredProducts; // Список названий продуктов (например, "картошка_нарезана", "соль")
        public string cookingTool; // "pan" или "pot"
        public Sprite dishSprite; // Спрайт готового блюда
    }
    
    [Header("Рецепты блюд")]
    public List<DishRecipe> dishRecipes = new List<DishRecipe>();
    
    private GameObject dragObject; // Object to display the sprite during dragging
    private RectTransform rectTransform; // RectTransform of the original object
    private RectTransform dragObjectRectTransform; // RectTransform for the sprite object
    private Canvas canvas; // Canvas for proper rendering
    private CanvasGroup canvasGroup; // CanvasGroup for interaction control
    
    // Original positions for tools
    private Vector2 originalPosition;
    
    // References
    private GameObject plateObject; // Ссылка на тарелку
    private Image panImage; // Image компонент сковороды
    private Image potImage; // Image компонент кастрюли
    private bool isCooking = false; // Флаг готовки
    private string currentDishName = ""; // Название текущего блюда
    private List<string> currentIngredients = new List<string>(); // Текущие ингредиенты в посуде
    
    private const float COOKING_TIME = 3f; // Фиксированное время готовки - 3 секунды
    
    private void Awake()
    {
        // Get RectTransform of the object
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        
        // Auto-detect object types based on name if not manually set
        if (!isProduct && !isKnife && !isGrater && !isPan && !isPot && !isPlate && !isBoard)
        {
            string objName = gameObject.name.ToLower();
            
            // Check for products (potato, salt, onion, etc.)
            if (objName.Contains("картош") || objName.Contains("potato") || 
                objName.Contains("соль") || objName.Contains("salt") ||
                objName.Contains("лук") || objName.Contains("onion"))
            {
                isProduct = true;
            }
            else if (objName.Contains("нож") || objName.Contains("knife"))
                isKnife = true;
            else if (objName.Contains("терк") || objName.Contains("grater"))
                isGrater = true;
            else if (objName.Contains("сковор") || objName.Contains("pan"))
                isPan = true;
            else if (objName.Contains("кастрюл") || objName.Contains("pot"))
                isPot = true;
            else if (objName.Contains("тарел") || objName.Contains("plate"))
                isPlate = true;
            else if (objName.Contains("доск") || objName.Contains("board"))
                isBoard = true;
        }
        
        // Get pan image if this is a pan
        if (isPan)
        {
            panImage = GetComponent<Image>();
            if (panNormalSprite == null && panImage != null)
            {
                panNormalSprite = panImage.sprite;
            }
        }
        if (isPan || isPot)
        {
            // Настраиваем кнопку готовки
            if (cookButton != null)
            {
                cookButton.onClick.AddListener(StartCookingManually);
                cookButton.gameObject.SetActive(false); // Скрываем кнопку по умолчанию
            }
        }//cut

        // Get pot image if this is a pot
        if (isPot)
        {
            potImage = GetComponent<Image>();
            if (potNormalSprite == null && potImage != null)
            {
                potNormalSprite = potImage.sprite;
            }
        }
        
        // Check if CanvasGroup exists before adding
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Find Canvas in the scene
        canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in the scene. Ensure it exists.");
        }
        
        // Find plate in the scene if not found
        if (plateObject == null && (isPan || isPot))
        {
            plateObject = GameObject.Find("Тарелка");
            if (plateObject == null)
            {
                plateObject = GameObject.Find("Plate");
            }
        }
        
        // Initialize dish recipes with examples
        if (dishRecipes.Count == 0 && (isPan || isPot))
        {
            InitializeDefaultRecipes();
        }
    }
    
    private void InitializeDefaultRecipes()
{
    // Очищаем старые рецепты
    dishRecipes.Clear();
    
    // Жареная картошка = картошка нарезанная + специи, на сковороде
    DishRecipe friedPotatoes = new DishRecipe();
    friedPotatoes.dishName = "жареная_картошка";
    friedPotatoes.requiredProducts = new List<string> { "картошка_нарезана", "специи" };
    friedPotatoes.cookingTool = "pan";
    
    // Драники = картошка тертая + специи + лук, на сковороде
    DishRecipe draniki = new DishRecipe();
    draniki.dishName = "драники";
    draniki.requiredProducts = new List<string> { "картошка_натерта", "специи", "лук" };
    draniki.cookingTool = "pan";
    
    // Картошка вареная = картошка нарезанная + специи, в кастрюле
    DishRecipe boiledPotatoes = new DishRecipe();
    boiledPotatoes.dishName = "картошка_вареная";
    boiledPotatoes.requiredProducts = new List<string> { "картошка_нарезана", "специи" };
    boiledPotatoes.cookingTool = "pot";
     DishRecipe potatoesWithChicken = new DishRecipe();
    potatoesWithChicken.dishName = "картошка_курица";
    potatoesWithChicken.requiredProducts = new List<string> { "картошка_нарезана", "курица", "специи" };
    potatoesWithChicken.cookingTool = "pan";
    dishRecipes.Add(potatoesWithChicken);
    dishRecipes.Add(friedPotatoes);
    dishRecipes.Add(draniki);
    dishRecipes.Add(boiledPotatoes);
}
    
   public void OnBeginDrag(PointerEventData eventData)
{
    // Don't allow dragging if pan/pot is cooking
    if ((isPan || isPot) && isCooking)
    {
        Debug.Log("Посуда готовит, нельзя перемещать!");
        return;
    }
    
    // Don't allow dragging board
    if (isBoard)
    {
        return;
    }
    
    // ВАЖНО: Если нет dragSprite, используем текущий спрайт
    if (dragSprite == null)
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            dragSprite = img.sprite;
        }
    }
    
    // ВАЖНО: Всегда создаем dragObject, даже если нет dragSprite
    dragObject = new GameObject("DragSprite");
    dragObject.transform.SetParent(canvas.transform, false);
    
    // Add Image component to display the sprite
    var image = dragObject.AddComponent<Image>();
    
    // Используем dragSprite если есть, иначе текущий спрайт
    if (dragSprite != null)
    {
        image.sprite = dragSprite;
    }
    else
    {
        Image currentImage = GetComponent<Image>();
        if (currentImage != null)
        {
            image.sprite = currentImage.sprite;
        }
    }
    
    image.raycastTarget = false;
    image.preserveAspect = true;
    
    // Get RectTransform of the object
    dragObjectRectTransform = dragObject.GetComponent<RectTransform>();
   
   Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
    dragObjectRectTransform.anchoredPosition = localPoint;
    if (gameObject.name.Contains("_порция"))
    {
        // Для порций - фиксированный маленький размер
        dragObjectRectTransform.sizeDelta = new Vector2(100, 100); 
      
    }
    else
    {
        // Для всех остальных объектов (не порций) - оригинальный размер
        dragObjectRectTransform.sizeDelta = rectTransform.sizeDelta;
    }
    
    // Disable interaction with the original object
    canvasGroup.blocksRaycasts = false;
    
    Debug.Log($"Начато перетаскивание: {gameObject.name}");
}
    
    public void OnDrag(PointerEventData eventData)
    {
        // Don't allow dragging if pan/pot is cooking
        if ((isPan || isPot) && isCooking)
        {
            return;
        }
        
        // Don't allow dragging board
        if (isBoard)
        {
            return;
        }
        
        if (dragSprite != null && dragObjectRectTransform != null)
        {
            // Update position of the sprite object during dragging
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
            dragObjectRectTransform.anchoredPosition = localPoint;
        }
        else
        {
            // If sprite is not specified, drag the object itself pot
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Don't allow dropping if pan/pot is cooking
        if ((isPan || isPot) && isCooking)
        {
            canvasGroup.blocksRaycasts = true;
            return;
        }
        
        // Don't allow dragging board
        if (isBoard)
        {
            canvasGroup.blocksRaycasts = true;
            return;
        }
        
        if (dragSprite != null)
        {
            if (dragObject == null)
            {
                canvasGroup.blocksRaycasts = true;
                return;
            }
            
            // Get the target object (what we dropped onto)
            GameObject targetObject = GetTargetObject(eventData);
            
            // Handle interaction if target exists
            if (targetObject != null)
            {
                HandleInteraction(targetObject);
            }
            
            // Destroy the drag object
            Destroy(dragObject);
            
            // Enable interaction with the original object
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    private GameObject GetTargetObject(PointerEventData eventData)
    {
        // Use EventSystem to detect the target object for UI elements
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        // Skip the first result (it's usually the drag object itself)
        for (int i = 0; i < results.Count; i++)
        {
            GameObject resultObject = results[i].gameObject;
            
            // Skip the drag object and the original object
            if (resultObject != gameObject && resultObject != dragObject)
            {
                return resultObject;
            }
        }
        
        return null; // No valid target object detected
    }
    
private void HandleInteraction(GameObject targetObject)
{
    Debug.Log($"=== HandleInteraction: {gameObject.name} -> {targetObject.name} ===");
    Debug.Log($"isProduct: {isProduct}, isKnife: {isKnife}, isGrater: {isGrater}");
    
    DragAndDropWithCustomSprite targetScript = targetObject.GetComponent<DragAndDropWithCustomSprite>();
    
    if (targetScript != null)
    {
        Debug.Log($"Target: isPan: {targetScript.isPan}, isPot: {targetScript.isPot}, isBoard: {targetScript.isBoard}");
    }
    
    // CASE 1: Tool (knife or grater) used on product
    if (isKnife || isGrater)
    {
        HandleToolInteraction(targetObject);
    }
    // CASE 2: Product dragged onto pan (включая порции с доски)
   else if (isProduct && targetScript != null && targetScript.isPan)
    {
        Debug.Log($"Добавляем {gameObject.name} на сковороду");
        HandleProductOnPan(targetObject);
    }
    // CASE 3: Product dragged onto pot (включая порции с доски)
    else if (isProduct && targetScript != null && targetScript.isPot)
    {
        Debug.Log($"Обработка продукта {gameObject.name} в кастрюле");
        HandleProductOnPot(targetObject);
    }
    // CASE 4: Product dragged onto board
    else if (isProduct && targetScript != null && targetScript.isBoard)
    {
        HandleProductOnBoard(targetObject);
    }
    // CASE 5: Product dragged onto tool (knife/grater)
    else if (isProduct && targetScript != null && (targetScript.isKnife || targetScript.isGrater))
    {
        // Return product to original position
        rectTransform.anchoredPosition = originalPosition;
    }
    // CASE 6: Other interactions
    else
    {
        Debug.Log($"Неизвестное взаимодействие: {gameObject.name} -> {targetObject.name}");
        HandleOtherInteractions(targetObject);
    }
}
    private void HandleToolInteraction(GameObject targetObject)
    {
        DragAndDropWithCustomSprite targetScript = targetObject.GetComponent<DragAndDropWithCustomSprite>();
        
        // Check if target is a product
        if (targetScript != null && targetScript.isProduct)
        {
            Debug.Log($"{gameObject.name} использован на продукте: {targetObject.name}");
            
            if (isKnife)
            {
                CutProduct(targetObject);
            }
            else if (isGrater)
            {
                GrateProduct(targetObject);
            }
            
            // Return tool to original position
            ReturnToolToOriginalPosition();
        }
        // Check if target is the board
        else if (targetScript != null && targetScript.isBoard)
        {
            Debug.Log($"{gameObject.name} помещен на доску, поиск продуктов...");
            
            // Find and process all products on the board
            ProcessProductsOnBoard(targetObject);
            
            // Return tool to original position
            ReturnToolToOriginalPosition();
        }
        else
        {
            Debug.Log($"{gameObject.name} не может взаимодействовать с {targetObject.name}");
            ReturnToolToOriginalPosition();
        }
    }
    
    private void HandleProductOnPan(GameObject panObject)
{
    Debug.Log($"HandleProductOnPan вызван для: {gameObject.name}");
    
    // Get the pan script
    DragAndDropWithCustomSprite panScript = panObject.GetComponent<DragAndDropWithCustomSprite>();
    if (panScript == null)
    {
        Debug.LogError("Сковорода не имеет скрипта DragAndDropWithCustomSprite");
        return;
    }
    
    Debug.Log($"Добавляем ингредиент: {gameObject.name}");
    
    // Add product to pan's ingredients
    panScript.AddIngredient(gameObject.name);
    
    // ВАЖНО: Проверяем, находится ли этот объект на доске
    // Если родитель этого объекта - доска, значит это порция с доски
    if (transform.parent != null)
    {
        DragAndDropWithCustomSprite parentScript = transform.parent.GetComponent<DragAndDropWithCustomSprite>();
        if (parentScript != null && parentScript.isBoard)
        {
            Debug.Log($"Объект {gameObject.name} находится на доске, удаляем его");
            
            // Уничтожаем объект (порцию на доске)
            Destroy(gameObject);
            return;
        }
    }
    
    // Если не на доске, возвращаем на исходную позицию
    rectTransform.anchoredPosition = originalPosition;
}
    private void HandleProductOnPot(GameObject potObject)
{
    Debug.Log($"Продукт {gameObject.name} помещен в кастрюлю");
    
    // Get the pot script
    DragAndDropWithCustomSprite potScript = potObject.GetComponent<DragAndDropWithCustomSprite>();
    if (potScript == null)
    {
        Debug.LogError("Кастрюля не имеет скрипта DragAndDropWithCustomSprite");
        return;
    }
    
    // Add product to pot's ingredients
    potScript.AddIngredient(gameObject.name);
    
    // ВАЖНО: Удаляем объект с доски, если он оттуда
    bool isFromBoard = transform.parent != null;
    
    if (isFromBoard)
    {
        // Получаем родительский объект (доску)
        DragAndDropWithCustomSprite parentScript = transform.parent.GetComponent<DragAndDropWithCustomSprite>();
        
        if (parentScript != null && parentScript.isBoard)
        {
            Debug.Log($"Объект {gameObject.name} находится на доске, удаляем его");
            
            // Уничтожаем объект (порцию на доске)
            Destroy(gameObject);
            return; // Выходим из метода после уничтожения
        }
    }
    
    // Если не с доски, возвращаем на исходную позицию
    rectTransform.anchoredPosition = originalPosition;
}
    
   public void AddIngredient(string ingredientName)
{
    Debug.Log($"=== AddIngredient вызван с: {ingredientName} ===");
    
    // Extract product state
    string state = "";
    string productName = ingredientName;
    
    Debug.Log($"Оригинальное имя: {ingredientName}");
    
    if (ingredientName.Contains("_нарезана"))
    {
        state = "_нарезана";
        productName = ingredientName.Replace("_нарезана", "");
        Debug.Log($"Обнаружено '_нарезана'. Новое имя: {productName}");
    }
    else if (ingredientName.Contains("_натерта"))
    {
        state = "_натерта";
        productName = ingredientName.Replace("_натерта", "");
        Debug.Log($"Обнаружено '_натерта'. Новое имя: {productName}");
    }
    
    // Удаляем "_порция" если есть
    productName = productName.Replace("_порция", "");
    Debug.Log($"После удаления '_порция': {productName}");
    
    // Формируем нормализованное имя ингредиента
    string fullIngredientName = NormalizeIngredientName(productName + state);
    Debug.Log($"Нормализованное имя: {fullIngredientName}");
    
    // Добавляем в текущие ингредиенты
    if (!currentIngredients.Contains(fullIngredientName))
    {
        currentIngredients.Add(fullIngredientName);
        Debug.Log($"Добавлен ингредиент: {fullIngredientName}. Всего: {currentIngredients.Count}");
        
        // Показываем кнопку готовки
        if (manualStartRequired && cookButton != null)
        {
            cookButton.gameObject.SetActive(true);
            Debug.Log("Кнопка готовки показана");
        }
    }
    else
    {
        Debug.Log($"Ингредиент {fullIngredientName} уже добавлен");
    }
}
    
    public void StartCooking()
{
    if ((!isPan && !isPot) || isCooking)
    {
        Debug.Log("Посуда уже готовит или это не посуда");
        return;
    }
    
    // Проверяем, есть ли ингредиенты
    if (currentIngredients.Count == 0)
    {
        Debug.Log("Нет ингредиентов для готовки");
        return;
    }
    
    // Определяем, какое блюдо можно приготовить
    DishRecipe recipeToCook = CheckForRecipe();
    
    if (recipeToCook != null)
    {
        currentDishName = recipeToCook.dishName;
        
        // Change sprite to cooking sprite
        if (isPan && panImage != null && panCookingSprite != null)
        {
            panImage.sprite = panCookingSprite;
            Debug.Log("Сковорода начала готовку");
        }
        else if (isPot && potImage != null && potCookingSprite != null)
        {
            potImage.sprite = potCookingSprite;
            Debug.Log("Кастрюля началa готовку");
        }
        
        // Start cooking coroutine
        StartCoroutine(CookingCoroutine(recipeToCook));
    }
    else
    {
        Debug.Log("Нет подходящего рецепта для этих ингредиентов");
        
        // Выводим отладочную информацию
        Debug.Log("Текущие ингредиенты:");
        foreach (string ingredient in currentIngredients)
        {
            Debug.Log($"- {ingredient}");
        }
        
        Debug.Log("Доступные рецепты:");
        foreach (DishRecipe recipe in dishRecipes)
        {
            Debug.Log($"- {recipe.dishName}: {string.Join(", ", recipe.requiredProducts)}");
        }
        
        currentIngredients.Clear();
    }
}
    
/*   private DishRecipe CheckForRecipe()
{
    Debug.Log($"=== Проверка рецептов ===");
    Debug.Log($"Текущие ингредиенты: {string.Join(", ", currentIngredients)}");
    
    foreach (DishRecipe recipe in dishRecipes)
    {
        Debug.Log($"\nПроверяем рецепт: {recipe.dishName}");
        Debug.Log($"Требуемые: {string.Join(", ", recipe.requiredProducts)}");
        
        // Проверяем, соответствует ли посуда
        bool toolMatches = (isPan && recipe.cookingTool == "pan") || (isPot && recipe.cookingTool == "pot");
        
        if (!toolMatches)
        {
            Debug.Log($"Не подходит посуда: нужна {recipe.cookingTool}, а это {(isPan ? "pan" : "pot")}");
            continue;
        }
        
        // Проверяем, есть ли все необходимые продукты
        bool hasAllIngredients = true;
        
        // Копируем текущие ингредиенты для проверки
        List<string> tempIngredients = new List<string>(currentIngredients);
        
        foreach (string requiredProduct in recipe.requiredProducts)
        {
            string requiredLower = requiredProduct.ToLower();
            bool found = false;
            
            Debug.Log($"Ищем: {requiredProduct}");
            
            for (int i = 0; i < tempIngredients.Count; i++)
            {
                string currentLower = tempIngredients[i].ToLower();
                Debug.Log($"Сравниваем: '{currentLower}' с '{requiredLower}'");
                
                // Прямое сравнение или частичное
                if (currentLower == requiredLower || 
                    currentLower.Contains(requiredLower) || 
                    requiredLower.Contains(currentLower))
                {
                    Debug.Log($"Найдено совпадение!");
                    tempIngredients.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                hasAllIngredients = false;
                Debug.Log($"Не найден: {requiredProduct}");
                break;
            }
        }
        
        if (hasAllIngredients)
        {
            Debug.Log($"=== Найден подходящий рецепт: {recipe.dishName} ===");
            return recipe;
        }
        else
        {
            Debug.Log($"Рецепт {recipe.dishName} не подходит");
        }
    }
    
    Debug.Log("=== Не найдено подходящих рецептов ===");
    return null;
}*/
    private DishRecipe CheckForRecipe()
{
    Debug.Log($"=== Проверка рецептов ===");
    Debug.Log($"Текущие ингредиенты: {string.Join(", ", currentIngredients)}");
    
    foreach (DishRecipe recipe in dishRecipes)
    {
        Debug.Log($"\nПроверяем рецепт: {recipe.dishName}");
        Debug.Log($"Требуемые: {string.Join(", ", recipe.requiredProducts)}");
        
        // Проверяем, соответствует ли посуда
        bool toolMatches = (isPan && recipe.cookingTool == "pan") || (isPot && recipe.cookingTool == "pot");
        
        if (!toolMatches)
        {
            Debug.Log($"Не подходит посуда: нужна {recipe.cookingTool}, а это {(isPan ? "pan" : "pot")}");
            continue;
        }
        
        // ВАЖНО: Проверяем точное количество ингредиентов
        if (currentIngredients.Count != recipe.requiredProducts.Count)
        {
            Debug.Log($"Разное количество ингредиентов: текущих {currentIngredients.Count}, требуется {recipe.requiredProducts.Count}");
            continue;
        }
        
        // Проверяем, есть ли все необходимые продукты
        bool hasAllIngredients = true;
        
        // Копируем текущие ингредиенты для проверки
        List<string> tempIngredients = new List<string>(currentIngredients);
        
        foreach (string requiredProduct in recipe.requiredProducts)
        {
            string requiredLower = requiredProduct.ToLower();
            bool found = false;
            
            Debug.Log($"Ищем: {requiredProduct}");
            
            for (int i = 0; i < tempIngredients.Count; i++)
            {
                string currentLower = tempIngredients[i].ToLower();
                Debug.Log($"Сравниваем: '{currentLower}' с '{requiredLower}'");
                
                if (currentLower == requiredLower || 
                    currentLower.Contains(requiredLower) || 
                    requiredLower.Contains(currentLower))
                {
                    Debug.Log($"Найдено совпадение!");
                    tempIngredients.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                hasAllIngredients = false;
                Debug.Log($"Не найден: {requiredProduct}");
                break;
            }
        }
        
        if (hasAllIngredients)
        {
            Debug.Log($"=== Найден подходящий рецепт: {recipe.dishName} ===");
            return recipe;
        }
        else
        {
            Debug.Log($"Рецепт {recipe.dishName} не подходит");
        }
    }
    
    Debug.Log("=== Не найдено подходящих рецептов ===");
    return null;
}
    private IEnumerator CookingCoroutine(DishRecipe recipe)
    {
        isCooking = true;
        
        // Show cooking progress
        Debug.Log($"Готовка {recipe.dishName} началась. Время: {COOKING_TIME} секунд");
        
        // Wait for cooking time (фиксированно 3 секунды)
        yield return new WaitForSeconds(COOKING_TIME);
        
        // Cooking finished
        Debug.Log($"Готовка {recipe.dishName} завершена!");
        
        // Return to normal sprite
        if (isPan && panImage != null && panNormalSprite != null)
        {
            panImage.sprite = panNormalSprite;
        }
        else if (isPot && potImage != null && potNormalSprite != null)
        {
            potImage.sprite = potNormalSprite;
        }
        
        // Serve dish to plate
        ServeDishToPlate(recipe);
        
        // Clear ingredients
        currentIngredients.Clear();
        isCooking = false;
        currentDishName = "";
    }
    
    private void ServeDishToPlate(DishRecipe recipe)
    {
        if (plateObject == null)
        {
            Debug.LogWarning("Тарелка не найдена в сцене!");
            return;
        }
        
        // Get plate script
        DragAndDropWithCustomSprite plateScript = plateObject.GetComponent<DragAndDropWithCustomSprite>();
        if (plateScript == null)
        {
            Debug.LogError("Тарелка не имеет скрипта DragAndDropWithCustomSprite");
            return;
        }
        
        // Serve dish to plate
        plateScript.ReceiveDish(recipe.dishName, recipe.dishSprite);
    }
    
    
    // В методе ReceiveDish добавьте:
    public void ReceiveDish(string dishName, Sprite dishSprite = null)
    {
        if (!isPlate)
        {
            Debug.LogError("Этот объект не является тарелкой!");
            return;
        }
        
        Debug.Log($"Тарелка получила блюдо: {dishName}");
        
        // Сохраняем название приготовленного блюда
        cookedDishName = dishName;
        dishReady = true;
        
        // Показываем кнопку доставки
        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(true);
            deliverButton.onClick.RemoveAllListeners();
            deliverButton.onClick.AddListener(DeliverToCustomer);
        }
        
        // Change plate sprite to show the dish
        Image plateImage = GetComponent<Image>();
        if (plateImage != null)
        {
            if (dishSprite != null)
            {
                plateImage.sprite = dishSprite;
            }
            else
            {
                Sprite loadedSprite = Resources.Load<Sprite>(dishName);
                if (loadedSprite != null)
                {
                    plateImage.sprite = loadedSprite;
                }
            }
        }
        
        Debug.Log($"Блюдо '{dishName}' готово к доставке!");
    }
    
    // Метод для доставки блюда клиенту
    public void DeliverToCustomer()
    {
        if (!dishReady || string.IsNullOrEmpty(cookedDishName))
        {
            Debug.Log("Блюдо не готово к доставке!");
            return;
        }
        
        Debug.Log($"Доставляем блюдо: {cookedDishName}");
        
        // Сохраняем приготовленное блюдо для передачи на сцену GameScene
        DishDeliveryData.DeliveredDishName = cookedDishName;
        
        // Переходим на сцену GameScene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    // Метод для сброса состояния тарелки
    public void ResetPlate()
    {
        dishReady = false;
        cookedDishName = "";
        
        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(false);
        }
        
        // Возвращаем исходный спрайт тарелки
        Image plateImage = GetComponent<Image>();
        if (plateImage != null)
        {
            // Загрузите исходный спрайт тарелки из Resources или используйте дефолтный
            Sprite defaultPlateSprite = Resources.Load<Sprite>("Plate_Default");
            if (defaultPlateSprite != null)
            {
                plateImage.sprite = defaultPlateSprite;
            }
        }
    }
    
   private void CutProduct(GameObject productObject)
{
    Image productImage = productObject.GetComponent<Image>();
    if (productImage == null) return;
    
    Sprite newSprite = GetProductStateSprite(productObject, "cut");
    
    if (newSprite != null)
    {
        productImage.sprite = newSprite;
        
        // Update product name
        string newName = productObject.name.Replace("_порция", "") + "_нарезана";
        if (!productObject.name.Contains("_порция"))
        {
            newName += "_порция";
        }
        productObject.name = newName;
        
        // ВАЖНО: Устанавливаем флаг isProduct для нарезанного продукта
        DragAndDropWithCustomSprite productScript = productObject.GetComponent<DragAndDropWithCustomSprite>();
        if (productScript != null)
        {
            productScript.isProduct = true;
        }
        else
        {
            // Если скрипта нет, добавляем его
            productScript = productObject.AddComponent<DragAndDropWithCustomSprite>();
            productScript.isProduct = true;
            productScript.cutSprite = newSprite;
        }
        
        Debug.Log($"Продукт {productObject.name} нарезан! isProduct: {productScript.isProduct}");
    }
}
    
    private void GrateProduct(GameObject productObject)
{
    Image productImage = productObject.GetComponent<Image>();
    if (productImage == null) return;
    
    Sprite newSprite = GetProductStateSprite(productObject, "grated");
    
    if (newSprite != null)
    {
        productImage.sprite = newSprite;
        
        // Update product name
        string newName = productObject.name.Replace("_порция", "") + "_натерта";
        if (!productObject.name.Contains("_порция"))
        {
            newName += "_порция";
        }
        productObject.name = newName;
        
        // ВАЖНО: Устанавливаем флаг isProduct для натертого продукта
        DragAndDropWithCustomSprite productScript = productObject.GetComponent<DragAndDropWithCustomSprite>();
        if (productScript != null)
        {
            productScript.isProduct = true;
        }
        else
        {
            // Если скрипта нет, добавляем его
            productScript = productObject.AddComponent<DragAndDropWithCustomSprite>();
            productScript.isProduct = true;
            productScript.gratedSprite = newSprite;
        }
        
        Debug.Log($"Продукт {productObject.name} натерт! isProduct: {productScript.isProduct}");
    }
}
    private Sprite GetProductStateSprite(GameObject productObject, string state)
    {
        DragAndDropWithCustomSprite productScript = productObject.GetComponent<DragAndDropWithCustomSprite>();
        
        if (productScript != null)
        {
            if (state == "cut" && productScript.cutSprite != null)
                return productScript.cutSprite;
            else if (state == "grated" && productScript.gratedSprite != null)
                return productScript.gratedSprite;
        }
        
        GameObject originalProduct = FindOriginalProduct(productObject.name);
        if (originalProduct != null)
        {
            DragAndDropWithCustomSprite originalScript = originalProduct.GetComponent<DragAndDropWithCustomSprite>();
            if (originalScript != null)
            {
                if (state == "cut" && originalScript.cutSprite != null)
                    return originalScript.cutSprite;
                else if (state == "grated" && originalScript.gratedSprite != null)
                    return originalScript.gratedSprite;
            }
        }
        
        string spriteName = GetSpriteNameFromProduct(productObject.name, state);
        return Resources.Load<Sprite>(spriteName);
    }
    
    private string GetSpriteNameFromProduct(string productName, string state)
    {
        string cleanName = productName.Replace("_порция", "").Replace("_portion", "");
        
        if (state == "cut")
            return cleanName + "_нарезана";
        else if (state == "grated")
            return cleanName + "_натерта";
        else if (state == "dish")
            return cleanName + "_блюдо";
        else
            return cleanName;
    }
    
    private GameObject FindOriginalProduct(string portionName)
    {
        string originalName = portionName.Replace("_порция", "").Replace("_portion", "")
                                         .Replace("_нарезана", "").Replace("_натерта", "");
        GameObject originalProduct = GameObject.Find(originalName);
        
        if (originalProduct == null)
        {
            originalProduct = GameObject.Find(gameObject.name);
        }
        
        return originalProduct;
    }
    
    private void ProcessProductsOnBoard(GameObject boardObject)
    {
        bool foundProducts = false;
        
        foreach (Transform child in boardObject.transform)
        {
            DragAndDropWithCustomSprite childScript = child.GetComponent<DragAndDropWithCustomSprite>();
            if (childScript != null && childScript.isProduct)
            {
                foundProducts = true;
                
                if (isKnife)
                {
                    CutProduct(child.gameObject);
                }
                else if (isGrater)
                {
                    GrateProduct(child.gameObject);
                }
            }
        }
        
        if (!foundProducts)
        {
            Debug.Log($"На доске нет продуктов для обработки {gameObject.name}");
        }
    }
    
    private void ReturnToolToOriginalPosition()
    {
        if (isKnife)
        {
            rectTransform.anchoredPosition = new Vector2(-646f, 103f);
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    private void HandleProductOnBoard(GameObject boardObject)
    {
        Debug.Log($"Продукт {gameObject.name} помещен на доску");
        
        string portionName = $"{gameObject.name}_порция";
        
        Transform existingPortion = boardObject.transform.Find(portionName);
        if (existingPortion != null) return;
        
        GameObject portionObject = new GameObject(portionName);
        portionObject.transform.SetParent(boardObject.transform, false);
        
        Image portionImage = portionObject.AddComponent<Image>();
        portionImage.sprite = dragSprite != null ? dragSprite : GetComponent<Image>().sprite;
        portionImage.raycastTarget = true;
        portionImage.preserveAspect = true;
        
        DragAndDropWithCustomSprite portionScript = portionObject.AddComponent<DragAndDropWithCustomSprite>();
        portionScript.isProduct = true;
        
        DragAndDropWithCustomSprite originalScript = GetComponent<DragAndDropWithCustomSprite>();
        if (originalScript != null)
        {
            portionScript.cutSprite = originalScript.cutSprite;
            portionScript.gratedSprite = originalScript.gratedSprite;
        }
        
        RectTransform portionRT = portionObject.GetComponent<RectTransform>();
        portionRT.sizeDelta = new Vector2(100, 100);
        
        float randomX = UnityEngine.Random.Range(-50f, 50f);
        float randomY = UnityEngine.Random.Range(-50f, 50f);
        portionRT.anchoredPosition = new Vector2(randomX, randomY);
        
        portionRT.localScale = Vector3.one;
        
        Debug.Log($"Создана порция: {portionName}");
    }
    
    private void HandleOtherInteractions(GameObject targetObject)
    {
        Debug.Log($"Объект {gameObject.name} помещен на {targetObject.name}");
        
        // Return product to original position
        if (isProduct)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private string NormalizeIngredientName(string ingredient)
    {
        // Нормализуем названия ингредиентов
        string normalized = ingredient.ToLower();
        
        if (normalized.Contains("картош"))
            return "картошка" + GetStateSuffix(ingredient);
        else if (normalized.Contains("специ"))
            return "специи";
        else if (normalized.Contains("лук"))
            return "лук";
        
        return ingredient;
    }
    
    private string GetStateSuffix(string ingredient)
    {
        if (ingredient.Contains("_нарезана"))
            return "_нарезана";
        else if (ingredient.Contains("_натерта"))
            return "_натерта";
        return "";
    }
    
    public void StartCookingManually()
    {
        if (currentIngredients.Count == 0)
        {
            Debug.Log("Нет ингредиентов для готовки!");
            return;
        }
        
        StartCooking();
        
        // Скрываем кнопку после начала готовки
        if (cookButton != null)
        {
            cookButton.gameObject.SetActive(false);
        }
    }
    // Helper methods
    public bool IsTool()
    {
        return isKnife || isGrater;
    }
    
    public bool IsCookingWare()
    {
        return isPan || isPot;
    }
}
