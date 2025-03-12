using UnityEngine;
using UnityEngine.UI;

public class FloatOption : BaseOption
{
    private Scrollbar scrollbar;

    void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
        string settingValue = SettingsManager.Instance.GetSetting(settingKey);
        float curVal;
        if (float.TryParse(settingValue, out curVal)) {
            scrollbar.value = curVal;
        }
        OnScrollValueChanged();
        baseOptions.Add(this);
    }

    void OnDestroy()
    {
        baseOptions.Remove(this);
    }

    public void OnScrollValueChanged()
    {
        PlayerPrefs.SetString(settingKey, scrollbar.value.ToString());
        PlayerPrefs.Save();
    }

}
