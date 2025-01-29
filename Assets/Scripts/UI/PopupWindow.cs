using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupWindow : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text intro;
    public string titleString = string.Empty;
    public string introString = string.Empty;
    public void Setup()
    {
        if (title != null && !string.IsNullOrWhiteSpace(titleString))
        {
            title.text = titleString;
        }
        if (intro != null && !string.IsNullOrWhiteSpace(introString))
        {
            intro.text = introString;
        }
    }

}
