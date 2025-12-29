using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        // Load the GameScene
        SceneManager.LoadScene("GameScene");
    }

    public void StartNewGame()
    {
        Debug.Log("=== НАЧАЛО НОВОЙ ИГРЫ ===");
        
        // 1. Устанавливаем флаг в статическом классе
        StaticData.IsNewGame = true;
        
        // 2. Удаляем файл сохранения
        string savePath = Path.Combine(Application.persistentDataPath, "game_save.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Удален файл сохранения: " + savePath);
        }
        
        // 3. Очищаем PlayerPrefs (опционально)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // 4. Загружаем сцену
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGame()
    {
        Debug.Log("=== ЗАГРУЗКА ИГРЫ ===");
        
        // Устанавливаем флаг загруженной игры
        StaticData.IsNewGame = false;
        
        // Проверяем сохранение
        string savePath = Path.Combine(Application.persistentDataPath, "game_save.json");
        
        if (File.Exists(savePath))
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning("Файл сохранения не найден!");
            ShowNoSaveMessage();
        }
    }

    // Сообщение об отсутствии сохранения
    void ShowNoSaveMessage()
    {
        Debug.Log("Нет сохраненной игры. Начните новую игру!");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.DisplayDialog("Сохранение не найдено", 
            "Нет сохраненной игры. Начните новую игру!", "OK");
        #endif
        
    }
}