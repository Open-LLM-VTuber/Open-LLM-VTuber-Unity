using System.Collections.Generic;
using UnityEngine;

public class BaseOption : MonoBehaviour
{
    public string settingKey = ""; // 对应的设置键（例如 "Audio.Mute"）

    public static List<BaseOption> baseOptions = new ();

    public static void SaveSettings()
    {
        foreach (var option in baseOptions)
        {
            var strValue = PlayerPrefs.GetString(option.settingKey, "<null>");
            if (strValue == "<null>")
            {
                var intValue = PlayerPrefs.GetInt(option.settingKey, 0);
                strValue = intValue != 0 ? "True" : "False";
            }

            SettingsManager.Instance.UpdateSetting(option.settingKey, strValue);
            Debug.Log("Updated: " + option.settingKey + ": " + strValue);
        }
        SettingsManager.Instance.SaveSettings();
    }

}