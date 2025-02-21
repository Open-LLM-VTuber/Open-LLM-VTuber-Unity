using TMPro;
using UnityEngine;

public class StringOption : MonoBehaviour
{
    public string settingKey; // 对应的设置键（例如 "Audio.Mute"）

    private TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        string settingValue = SettingsManager.Instance.GetSetting(settingKey);
        inputField.text = settingValue;
        OnInputFieldChanged();
    }

    public void OnInputFieldChanged()
    {
        PlayerPrefs.SetString(settingKey, inputField.text);
        PlayerPrefs.Save();
    }

}
