using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class SceneSwitcher : MonoBehaviour
{
    public string sceneName; // 要加载的场景名称

    public void LoadScene()
    {
        // 加载指定的场景
        SceneManager.LoadScene(sceneName);
    }
}