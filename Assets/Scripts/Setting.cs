using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Thêm dòng này

public class Setting : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;
    public Slider masterVolumeSlider; // Đổi tên để rõ ràng hơn
    public Slider sfxVolumeSlider;

    // Bỏ biến audioMixer này đi nếu AudioManager đã có rồi
    // public AudioMixer audioMixer;
    
    // Thêm vào 2 dòng này
    private const string MASTER_VOL = "MusicVolume";
    private const string SFX_VOL = "SFXVolume";

    private void Start()
    {
        // Load các giá trị đã lưu
        masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOL, 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SFX_VOL, 1f);
        
        // Cập nhật giá trị lên slider ngay khi bắt đầu
        ChangeMasterVolume();
        ChangeSPXVolume();
    }

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ChangeMasterVolume()
    {
        // Âm lượng trong Mixer dùng thang đo Logarit, cần chuyển đổi
        float volume = masterVolumeSlider.value;
        AudioManager.Instance.audioMixer.SetFloat(MASTER_VOL, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(MASTER_VOL, volume);
    }

    public void ChangeSPXVolume()
    {
        float volume = sfxVolumeSlider.value;
        AudioManager.Instance.audioMixer.SetFloat(SFX_VOL, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SFX_VOL, volume);
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene(0); // Dùng SceneManager
    }
}