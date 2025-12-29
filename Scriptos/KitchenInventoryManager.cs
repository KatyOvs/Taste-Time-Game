// KitchenInventoryManager.cs (исправленная версия с новым методом)
using UnityEngine;

public class KitchenInventoryManager : MonoBehaviour
{
    [Header("Объекты инвентаря")]
    public GameObject chickenObject; // Основной объект курицы
    
    [Header("Настройки")]
    private const string CHICKEN_KEY = "HasChicken";
    
    void Start()
    {
        Debug.Log("KitchenInventoryManager.Start() вызван");
        
        // Загружаем состояние покупки курицы
        bool hasChicken = PlayerPrefs.GetInt(CHICKEN_KEY, 0) == 1;
        
        // Активируем или деактивируем основной объект курицы
        if (chickenObject != null)
        {
            chickenObject.SetActive(hasChicken);
            Debug.Log($"Курица активна: {hasChicken}");
            
            // ВАЖНО: Если "Курица_порция" является дочерним объектом,
            // он автоматически активируется вместе с родителем
            
            // Просто показываем информацию о дочерних объектах для отладки
            if (hasChicken)
            {
                ShowChildObjectsInfo();
            }
        }
        else
        {
            Debug.LogWarning("ChickenObject не назначен в инспекторе!");
            // Пробуем найти автоматически
            FindChickenAutomatically();
        }
        
        Debug.Log($"Инвентарь загружен. Курица куплена: {hasChicken}");
    }
    
    void FindChickenAutomatically()
    {
        // Автоматический поиск курицы на сцене
        chickenObject = GameObject.Find("курица");
        if (chickenObject == null)
            chickenObject = GameObject.Find("Курица");
        if (chickenObject == null)    
            chickenObject = GameObject.Find("Chicken");
        
        if (chickenObject != null)
        {
            bool hasChicken = PlayerPrefs.GetInt(CHICKEN_KEY, 0) == 1;
            chickenObject.SetActive(hasChicken);
            
            if (hasChicken)
                ShowChildObjectsInfo();
        }
        else
        {
            Debug.LogError("Объект курицы не найден на сцене!");
        }
    }
    
    void ShowChildObjectsInfo()
    {
        if (chickenObject != null && chickenObject.activeSelf)
        {
            Debug.Log($"Информация об объекте '{chickenObject.name}':");
            
            int childCount = chickenObject.transform.childCount;
            if (childCount > 0)
            {
                Debug.Log($"Дочерних объектов: {childCount}");
                foreach (Transform child in chickenObject.transform)
                {
                    Debug.Log($"- {child.name}, активен: {child.gameObject.activeSelf}");
                }
            }
            else
            {
                Debug.Log("Нет дочерних объектов");
            }
        }
    }
    
    [ContextMenu("Проверить инвентарь")]
    public void CheckInventory()
    {
        bool hasChicken = PlayerPrefs.GetInt(CHICKEN_KEY, 0) == 1;
        Debug.Log($"Проверка инвентаря. Курица куплена: {hasChicken}");
        
        if (chickenObject != null)
        {
            Debug.Log($"Объект курицы: {chickenObject.name}, активен: {chickenObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("ChickenObject не назначен!");
        }
    }
    
    [ContextMenu("Сбросить инвентарь")]
    public void ResetInventory()
    {
        PlayerPrefs.DeleteKey(CHICKEN_KEY);
        PlayerPrefs.Save();
        
        if (chickenObject != null)
            chickenObject.SetActive(false);
        
        Debug.Log("Инвентарь сброшен");
    }
    
    [ContextMenu("Найти все объекты курицы на сцене")]
    void FindAllChickenObjects()
    {
        Debug.Log("=== ПОИСК ВСЕХ ОБЪЕКТОВ КУРИЦЫ ===");
        
        // ИСПРАВЛЕННЫЙ МЕТОД - используем новый API
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        int chickenCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if (objName.Contains("кури") || objName.Contains("chick"))
            {
                chickenCount++;
                Debug.Log($"Найден: {obj.name}, активен: {obj.activeSelf}, " +
                         $"родитель: {(obj.transform.parent != null ? obj.transform.parent.name : "нет")}");
            }
        }
        
        Debug.Log($"Всего найдено объектов курицы: {chickenCount}");
    }
}