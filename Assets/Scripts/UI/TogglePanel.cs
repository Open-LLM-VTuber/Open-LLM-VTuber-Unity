using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public Animator animator; // 引用 Animator 组件
    public bool isVisible = true; // 当前状态

    public void Start()
    {
        OnAnimationEnd();
    }
    public void Toggle()
    {
        OnAnimationStart();
        isVisible = !isVisible; // 切换状态
        if (isVisible)
        {
            animator.SetTrigger("doShow");
        }
        else
        {
            animator.SetTrigger("doHide");
        }
        Debug.Log("isVisible: " + isVisible);
    }

    public void OnAnimationStart()
    {
        if (animator != null)
        {
            // 获得参数的控制权
            animator.applyRootMotion = true;
            animator.enabled = true; // 启用 Animator，自动控制参数
            //Debug.Log("OnAnimationStart");
        }

    }

    // 动画结束时调用
    public void OnAnimationEnd()
    {
        if (animator != null)
        {
            // 释放参数的控制权
            animator.applyRootMotion = false;
            animator.enabled = false; // 禁用 Animator，手动控制参数
            //Debug.Log("OnAnimationEnd");
        }
    }

}