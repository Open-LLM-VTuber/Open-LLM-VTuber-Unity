using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSpanManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void ToggleConfirmButtonClicked()
    {
        // 确认退出逻辑
        Debug.Log("退出游戏");
        Application.Quit(); // 退出游戏
    }
}
