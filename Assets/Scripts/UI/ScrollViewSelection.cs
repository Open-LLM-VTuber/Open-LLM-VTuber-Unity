using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ScrollViewSelection : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent; // ScrollView 的 Content
    public Button buttonPrefab; // 按钮预制体

    [Header("Options")]
    public List<string> options = new List<string>(); // 选项文本列表
    public Color normalColor = Color.white; // 正常颜色
    public Color selectedColor = Color.yellow; // 选中颜色

    [Header("Selection Mode")]
    public bool isMultiSelect = false; // 是否为多选模式

    private List<Button> buttonPool = new List<Button>(); // 按钮对象池
    private HashSet<int> selectedIndices = new HashSet<int>(); // 多选时的选中索引
    private int singleSelectedIndex = -1; // 单选时的选中索引

    // 初始化按钮
    public void InitializeButtons()
    {
        // 清空现有按钮
        foreach (Button button in buttonPool)
        {
            Destroy(button.gameObject);
        }
        buttonPool.Clear();
        selectedIndices.Clear();
        singleSelectedIndex = -1;

        // 创建新按钮
        for (int i = 0; i < options.Count; i++)
        {
            Button button = Instantiate(buttonPrefab, contentParent);
            button.GetComponentInChildren<TMP_Text>().text = options[i];
            int index = i; // 捕获当前索引
            button.onClick.AddListener(() => OnButtonClicked(index));
            buttonPool.Add(button);

            // 初始化按钮颜色
            button.image.color = normalColor;
        }
    }

    // 按钮点击事件
    private void OnButtonClicked(int index)
    {
        if (isMultiSelect)
        {
            ToggleMultiSelect(index);
        }
        else
        {
            SetSingleSelect(index);
        }
    }

    // 设置单选
    public void SetSingleSelect(int index)
    {

        if (singleSelectedIndex != index)
        {
            if (singleSelectedIndex != -1)
            {
                // 取消之前的选择
                buttonPool[singleSelectedIndex].image.color = normalColor;
            }
            // 选择新项
            singleSelectedIndex = index;
            buttonPool[index].image.color = selectedColor;
        }
        
    }

    // 切换多选
    public void ToggleMultiSelect(int index)
    {
        if (selectedIndices.Contains(index))
        {
            // 取消选择
            selectedIndices.Remove(index);
            buttonPool[index].image.color = normalColor;
        }
        else
        {
            // 选择新项
            selectedIndices.Add(index);
            buttonPool[index].image.color = selectedColor;
        }
    }

    // 获取单选结果
    public int GetSingleSelectedIndex()
    {
        return singleSelectedIndex;
    }

    // 获取单选文本
    public string GetSingleSelectedText()
    {
        if (singleSelectedIndex != -1)
        {
            return options[singleSelectedIndex];
        }
        return null;
    }

    // 获取多选结果
    public List<int> GetMultiSelectedIndices()
    {
        return new List<int>(selectedIndices);
    }

    // 获取多选文本
    public List<string> GetMultiSelectedTexts()
    {
        List<string> selectedTexts = new List<string>();
        foreach (int index in selectedIndices)
        {
            selectedTexts.Add(options[index]);
        }
        return selectedTexts;
    }

    // 清除所有选择
    public void ClearSelection()
    {
        if (isMultiSelect)
        {
            foreach (int index in selectedIndices)
            {
                buttonPool[index].image.color = normalColor;
            }
            selectedIndices.Clear();
        }
        else
        {
            if (singleSelectedIndex != -1)
            {
                buttonPool[singleSelectedIndex].image.color = normalColor;
                singleSelectedIndex = -1;
            }
        }
    }
}