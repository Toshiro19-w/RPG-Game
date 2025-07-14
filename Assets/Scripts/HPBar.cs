using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image hpBarImage;

    public void HPInGame(int currentHealth, int maxHealth)
    {
        if (hpBarImage == null)
        {
            hpBarImage = GetComponent<Image>();
        }

        if (hpBarImage != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            hpBarImage.fillAmount = healthPercentage;
        }
    }
}
