using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class LoadingManager : MonoBehaviour
{

    public string targetScene;

    [Header("UI References")]
    public Slider progressBar; // 进度条
    public TMP_Text progressText;  // 百分比文本
    public float minLoadTime = 1.0f; // 最小加载时间

    private AsyncOperation loadingOperation;
    private float currentProgress;

    void Start()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            StartCoroutine(LoadSceneAsync(targetScene));
        }
        else
        {
            Debug.LogError("Target scene not set!");
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        
        float startTime = Time.time;

        loadingOperation = SceneManager.LoadSceneAsync(sceneName);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone)
        {
            float elapsedTime = Time.time - startTime;
            currentProgress = Mathf.Clamp01(loadingOperation.progress / 0.9f);

            // 如果加载完成但未达到最小加载时间，继续等待
            if (loadingOperation.progress >= 0.9f && elapsedTime < minLoadTime)
            {
                currentProgress = Mathf.Clamp01(elapsedTime / minLoadTime);
            }

            UpdateUI(currentProgress);

            if (loadingOperation.progress >= 0.9f && elapsedTime >= minLoadTime)
            {
                loadingOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    void UpdateUI(float progress)
    {
        // 更新进度条
        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        // 更新百分比文本
        if (progressText != null)
        {
            progressText.text = $"{(int)(progress * 100)}%";
        }
    }
}