using UnityEngine.Events;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private TransitionConfig transitionConfig;

    [Header("Events")]
    public UnityEvent OnPreSceneLoad;
    public UnityEvent OnPostSceneLoad;

    public void TriggerSceneSwitch()
    {
        PlayerPrefs.SetString("CurrentScene", sceneName);
        PlayerPrefs.Save();
        SceneTransitionManager.Instance.SwitchScene(
            sceneName,
            transitionConfig,
            OnPreSceneLoad.Invoke,
            OnPostSceneLoad.Invoke
        );
    }
}