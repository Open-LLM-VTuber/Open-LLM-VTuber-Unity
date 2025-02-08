using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool isSplash = false;

    private void Awake()
    {
        // 保存目标场景名称
        PlayerPrefs.SetString("TargetScene", sceneName);
        PlayerPrefs.Save();
    }
    public void SwitchToScene()
    {
        if (isSplash)
        {
            // 加载 LoadingScene
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}