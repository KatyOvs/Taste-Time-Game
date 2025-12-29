using UnityEngine;

public class WelcomePanelController : MonoBehaviour
{
    public GameObject welcomePanel;

    void Start()
    {
        Debug.Log("=== WelcomePanelController Start() ===");
        Debug.Log($"StaticData.IsNewGame: {StaticData.IsNewGame}");
        
        if (StaticData.IsNewGame)
        {
            welcomePanel.SetActive(true);
        }
        else
        {
            welcomePanel.SetActive(false);
        }
    }
    
    public void OnCloseButtonClicked()
    {
        Debug.Log("=== OnCloseButtonClicked ===");
        
        // Если панель null, ищем её
        if (welcomePanel == null)
        {
            Debug.LogWarning("Панель null, ищу заново...");
            welcomePanel = FindWelcomePanel();
        }
        
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
            StaticData.IsNewGame = false;
            Debug.Log("Панель закрыта");
        }
        else
        {
            Debug.LogError("Не могу найти WelcomePanel даже после поиска!");
        }
    }
    
    private GameObject FindWelcomePanel()
    {
        // Ищем по имени
        GameObject panel = GameObject.Find("WelcomePanel");
        
        if (panel == null)
        {
            // Ищем любой объект с "Welcome" в имени
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Welcome") && obj.transform.childCount > 0)
                {
                    panel = obj;
                    break;
                }
            }
        }
        
        if (panel != null)
        {
            Debug.Log($"Найдена панель: {panel.name}");
        }
        
        return panel;
    }
}