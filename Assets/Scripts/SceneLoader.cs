using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void LoadGameScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("로드할 씬 이름이 유효하지 않습니다.");
            return;
        }

        // 씬을 로드
        SceneManager.LoadScene(sceneName);
    }
}
