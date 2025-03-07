using UnityEngine;

public class TestDebugWrapper : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugWrapper.Instance.Log("普通消息");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DebugWrapper.Instance.Log("[b]粗体红色消息[/b]", Color.red);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            DebugWrapper.Instance.Log("[i][size]大斜体绿色消息[/size][/i]", Color.green);
        }
    }
}