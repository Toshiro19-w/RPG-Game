using UnityEngine;
using TMPro;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float lifetime = 1.5f;
    
    public void Initialize(int damage, Color color)
    {
        if (damageText == null)
            damageText = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();
            
        if (damageText != null)
        {
            damageText.text = damage.ToString();
            damageText.color = color;
        }
        
        StartCoroutine(AnimateDamage());
    }
    
    private IEnumerator AnimateDamage()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * 2f;
        float elapsed = 0f;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            // Move up
            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            
            // Fade out
            if (damageText != null)
            {
                Color color = damageText.color;
                color.a = 1f - progress;
                damageText.color = color;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}