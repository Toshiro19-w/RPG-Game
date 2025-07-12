using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;
    public Slider MaterVolume, SPXVolume;
    public AudioMixer audioMixer;

    public void ChangeGraphicsQuanlity()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMasterVolume()
    {
        audioMixer.SetFloat("Master Sound", MaterVolume.value);
    }

    public void ChangeSPXVolume()
    {
        audioMixer.SetFloat("SPX", MaterVolume.value);
    }
    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
