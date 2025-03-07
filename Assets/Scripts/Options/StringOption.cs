using TMPro;
using UnityEngine;

public class StringOption : BaseOption
{
    private TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        string settingValue = SettingsManager.Instance.GetSetting(settingKey);
        inputField.text = settingValue;
        OnInputFieldChanged();
        baseOptions.Add(this);
    }

    void OnDestroy()
    {
        baseOptions.Remove(this);
    }

    public void OnInputFieldChanged()
    {
        PlayerPrefs.SetString(settingKey, inputField.text);
        PlayerPrefs.Save();
    }

}
