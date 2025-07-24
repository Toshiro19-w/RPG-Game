using UnityEngine;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private Text promptText;
    [SerializeField] private KeyCode currentKey = KeyCode.B;
    
    void Start()
    {
        if (promptText == null)
            promptText = GetComponentInChildren<Text>();
            
        UpdatePromptText();
    }
    
    public void SetInteractKey(KeyCode key)
    {
        currentKey = key;
        UpdatePromptText();
    }
    
    private void UpdatePromptText()
    {
        if (promptText != null)
            promptText.text = $"Nhấn {currentKey} để chuyển map";
    }
}