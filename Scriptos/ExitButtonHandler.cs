using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExitButtonHandler : MonoBehaviour
{
    public GameObject exitConfirmationPanel; // Панель подтверждения выхода
    public Button stayButton; // Кнопка "Остаться"
    public Button saveAndExitButton; // Кнопка "Сохранить и выйти"
    
    private bool isSaving = false; // Флаг чтобы не запускать сохранение дважды

    private void Start()
    {
        // Инициализация панели подтверждения
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }

        // Назначаем действия для кнопок
        if (stayButton != null)
            stayButton.onClick.AddListener(OnStayButtonClicked);
        
        if (saveAndExitButton != null)
            saveAndExitButton.onClick.AddListener(OnSaveAndExitButtonClicked);
    }

    public void OnExitButtonClicked()
    {
        // Показываем панель подтверждения выхода
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
        }
    }

    private void OnStayButtonClicked()
    {
        // Скрываем панель подтверждения
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }
    }

    private void OnSaveAndExitButtonClicked()
    {
        // Не даем нажать кнопку дважды
        if (isSaving) return;
        isSaving = true;
        
        // Делаем кнопку неактивной
        saveAndExitButton.interactable = false;
        
        Debug.Log("=== НАЧАЛО СОХРАНЕНИЯ ===");
        
        // 1. Сохраняем игровые данные через GameDataManager
        bool saveSuccess = SaveGameData();
        
        if (saveSuccess)
        {
            Debug.Log("=== СОХРАНЕНИЕ УСПЕШНО ===");
            
            // Даем время на запись в файл (очень важно!)
            Invoke("QuitGame", 0.5f);
        }
        else
        {
            Debug.LogError("=== СОХРАНЕНИЕ НЕ УДАЛОСЬ ===");
            
            // Возвращаем возможность нажать снова
            isSaving = false;
            saveAndExitButton.interactable = true;
            
            // Можно показать сообщение об ошибке
            Debug.Log("Не удалось сохранить игру! Попробуйте снова.");
        }
    }

    // Основной метод сохранения
    private bool SaveGameData()
    {
        try
        {
            // Получаем GameDataManager
            GameDataManager dataManager = GameDataManager.Instance;
            
            if (dataManager == null)
            {
                Debug.LogError("GameDataManager не найден!");
                return false;
            }
            
            if (dataManager.GameData == null)
            {
                Debug.LogError("GameData не инициализирован!");
                return false;
            }
            
            // Показываем, что сохраняем
            Debug.Log($"Сохраняем данные:");
            Debug.Log($"- День: {dataManager.GameData.dayData.currentDay}");
            Debug.Log($"- Деньги: {dataManager.GameData.playerMoney}");
            Debug.Log($"- Время: {dataManager.GameData.dayData.currentHour}:{dataManager.GameData.dayData.currentMinute}");
            Debug.Log($"- Приготовлено сегодня: {dataManager.GameData.dailyDishesCooked}");
            Debug.Log($"- Заработано сегодня: {dataManager.GameData.dailyMoneyEarned}");
            
            // Сохраняем
            dataManager.SaveGameData();
            
            // Форсируем запись на диск
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(Application.persistentDataPath, "game_save.json"),
                UnityEngine.JsonUtility.ToJson(dataManager.GameData, true)
            );
            
            Debug.Log("Данные записаны на диск");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
            Debug.LogError($"StackTrace: {e.StackTrace}");
            return false;
        }
    }

    private void QuitGame()
    {
        Debug.Log("=== ВЫХОД ИЗ ИГРЫ ===");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}