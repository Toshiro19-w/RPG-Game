using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextMap : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Kiểm tra Tag của Player
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // Lấy tên của scene bằng Build Index của nó
                string nextSceneName = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
                
                nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextSceneName);


                PlayerPrefs.SetString("NextSceneToLoad", nextSceneName);
                SceneManager.LoadScene("Loading");
            }
            else
            {
                // Đây là trường hợp đã đến Map cuối cùng (Map 3 trong ví dụ của bạn)
                // Bạn có thể chuyển về Lobby, Menu, hoặc Game Win Scene
                Debug.Log("Reached the last map. Transitioning to Lobby (or Game End Scene).");
                PlayerPrefs.SetString("NextSceneToLoad", "Lobby");
                SceneManager.LoadScene("Loading");
            }
        }
    }
}
