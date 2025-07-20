using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void play()
    {
        PlayerPrefs.SetString("NextSceneToLoad", "Lobby"); // "Lobby" là tên scene bạn muốn tải sau Loading
        SceneManager.LoadScene("Loading");
    }

    public void Opption()
    {
        SceneManager.LoadScene("Setting");
    }
    // Update is called once per frame
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
