using UnityEngine;

public class BoolOption : MonoBehaviour
{
    public string settingKey; // 对应的设置键（例如 "Audio.Mute"）

    private bool _currentState; // 当前状态
    private DrawerAnimation _drawerAnimator;
    private ColorAnimation _colorAnimator;

    void Start()
    {
        _drawerAnimator = GetComponent<DrawerAnimation>();
        _colorAnimator = GetComponent<ColorAnimation>();
        // 从 SettingsManager 获取当前设置值
        //Debug.Log($"BoolOption: {settingKey}");
        string settingValue = SettingsManager.Instance.GetSetting(settingKey);
        if (bool.TryParse(settingValue, out _currentState))
        {
            OnBoolChanged();
            // 如果设置值为 false，执行 Toggle 操作
            if (!_currentState)
            {
                Toggle();
            }
        }
        else
        {
            Debug.LogError($"Failed to parse setting '{settingKey}' as boolean.");
        }
    }

    public void Toggle()
    {
        _drawerAnimator.ToggleDrawer();
        _colorAnimator.ToggleColor();
        OnBoolChanged();
    }

    void OnBoolChanged()
    {
        PlayerPrefs.SetInt(settingKey, _currentState ? 1 : 0);
        PlayerPrefs.Save();
    }


}
