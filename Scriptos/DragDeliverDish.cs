using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragDeliveredDish : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CustomerManager customerManager;
    public GameObject dropZone;
    
    private GameObject dragObject;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isBeingDragged = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = FindFirstObjectByType<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        // Сохраняем оригинальную позицию
        originalPosition = rectTransform.anchoredPosition;
    }
    
    void OnEnable()
    {
        // Сбрасываем позицию при каждом включении объекта
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
        isBeingDragged = false;
        
        // Гарантируем, что оригинальный объект видим
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        isBeingDragged = true;
        
        // Создаем объект для перетаскивания
        dragObject = new GameObject("DragDish");
        dragObject.transform.SetParent(canvas.transform, false);
        dragObject.transform.SetAsLastSibling(); // Помещаем поверх всего
        
        // Копируем изображение
        Image dragImage = dragObject.AddComponent<Image>();
        Image originalImage = GetComponent<Image>();
        
        if (originalImage != null)
        {
            dragImage.sprite = originalImage.sprite;
            dragImage.color = originalImage.color;
            dragImage.preserveAspect = true;
        }
        
        dragImage.raycastTarget = false;
        
        // Устанавливаем размер и позицию
        RectTransform dragRT = dragObject.GetComponent<RectTransform>();
        dragRT.sizeDelta = rectTransform.sizeDelta;
        
        // Позиционируем точно под курсором
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint
        );
        dragRT.anchoredPosition = localPoint;
        
        // тут надо прозрачность намного меньше сделать но я не знаю как
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.3f;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject == null || !isBeingDragged) return;
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint
        );
        
        dragObject.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;
        
        // Восстанавливаем оригинальный объект
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Уничтожаем объект перетаскивания
        if (dragObject != null)
            Destroy(dragObject);
        
        // Проверяем попадание в дроп-зону
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == dropZone)
            {
                // Блюдо доставлено клиенту!
                if (customerManager != null)
                {
                    customerManager.OnDishDelivered();
                }
                // Уничтожаем оригинальный объект блюда
                Destroy(gameObject);
                break;
            }
        }
        
        isBeingDragged = false;
    }
    
    void OnDestroy()
    {
        // Очищаем объект перетаскивания при уничтожении
        if (dragObject != null)
            Destroy(dragObject);
    }
}