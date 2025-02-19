using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum SceneTransitionType
{
    None,
    Fade,
    SlideUp,
    SlideDown,
    Custom // 可扩展其他类型
}

[System.Serializable]
public class TransitionConfig
{
    public SceneTransitionType type;
    public float duration = 0.5f;
    public Ease easeType = Ease.Linear;
    public Vector3 customTarget; // 用于自定义动画类型
}

public class SceneTransitionManager : InitOnceSingleton<SceneTransitionManager>
{
    [SerializeField] private string canvasPrefabPath = "Prefabs/Switch Canvas";
    private GameObject canvasInstance;
    private Image fadePanel;
    private RectTransform slidePanel;

    public void SwitchScene(string sceneName,
                           TransitionConfig config,
                           UnityAction preLoadAction = null,
                           UnityAction postLoadAction = null)
    {
        StartCoroutine(TransitionCoroutine(sceneName, config, preLoadAction, postLoadAction));
    }

    private System.Collections.IEnumerator TransitionCoroutine(
        string sceneName,
        TransitionConfig config,
        UnityAction preLoadAction,
        UnityAction postLoadAction)
    {
        // 执行切换前委托
        preLoadAction?.Invoke();

        // 加载并实例化 Canvas Prefab
        LoadAndInstantiateCanvas();

        // 播放退出动画
        switch (config.type)
        {
            case SceneTransitionType.None:
                break;
            case SceneTransitionType.Fade:
                fadePanel.DOFade(1, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.SlideUp:
                slidePanel.DOAnchorPosY(Screen.height, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.SlideDown:
                slidePanel.DOAnchorPosY(-Screen.height, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.Custom:
                slidePanel.DOLocalMove(config.customTarget, config.duration).SetEase(config.easeType);
                break;
        }

        yield return new WaitForSeconds(config.duration);

        // 销毁退出动画的 Canvas 实例
        // DestroyCanvasInstance(); 加载新场景时自动销毁

        // 加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // 再次加载并实例化 Canvas Prefab
        LoadAndInstantiateCanvas();

        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            color.a = 1f;
            fadePanel.color = color;
        }

        // 播放进入动画
        switch (config.type)
        {
            case SceneTransitionType.None:
                break;
            case SceneTransitionType.Fade:
                fadePanel.DOFade(0, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.SlideUp:
                slidePanel.DOAnchorPosY(-Screen.height, 0);
                slidePanel.DOAnchorPosY(0, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.SlideDown:
                slidePanel.DOAnchorPosY(Screen.height, 0);
                slidePanel.DOAnchorPosY(0, config.duration).SetEase(config.easeType);
                break;
            case SceneTransitionType.Custom:
                slidePanel.DOLocalMove(Vector3.zero, config.duration).SetEase(config.easeType);
                break;
        }

        yield return new WaitForSeconds(config.duration);

        // 销毁进入动画的 Canvas 实例
        DestroyCanvasInstance();

        // 执行加载后委托
        postLoadAction?.Invoke();
    }

    private void LoadAndInstantiateCanvas()
    {
        // 动态加载 Canvas Prefab
        GameObject canvasPrefab = Resources.Load<GameObject>(canvasPrefabPath);
        if (canvasPrefab == null)
        {
            Debug.LogError("Canvas Prefab not found at path: " + canvasPrefabPath);
            return;
        }

        // 实例化 Canvas
        canvasInstance = Instantiate(canvasPrefab);

        // 设置 Canvas 的 Render Camera 为当前场景的主摄像机
        Canvas canvas = canvasInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Debug.LogError("Canvas component not found on the prefab.");
        }

        // 获取 fadePanel 和 slidePanel
        fadePanel = canvasInstance.transform.Find("FadePanel")?.GetComponent<Image>();
        slidePanel = canvasInstance.transform.Find("SlidePanel")?.GetComponent<RectTransform>();

        if (fadePanel == null || slidePanel == null)
        {
            Debug.LogError("FadePanel or SlidePanel not found in the Canvas Prefab.");
        }
    }

    private void DestroyCanvasInstance()
    {
        if (canvasInstance != null)
        {
            Destroy(canvasInstance);
            canvasInstance = null;
            fadePanel = null;
            slidePanel = null;
        }
    }
}