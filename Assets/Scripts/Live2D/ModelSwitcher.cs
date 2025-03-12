using Live2D.Cubism.Core;
using UnityEngine;

namespace Live2D {
    public class ModelSwitcher : MonoBehaviour
    {
        public LoadModelWeb loadModelWeb; // 引用 LoadModelWeb 脚本
        public GameObject leftButton;     // 向左切换按钮
        public GameObject rightButton;   // 向右切换按钮

        private LongPressButton left, right;

        public int currentModelIndex = 0;

        void Start()
        {
            left = leftButton.GetComponent<LongPressButton>();
            right = rightButton.GetComponent<LongPressButton>();
            left.onShortPress.AddListener(OnLeftButtonClick);
            right.onShortPress.AddListener(OnRightButtonClick);
            loadModelWeb.OnModelsInfoLoaded += InitCurrentModelIndex;
            
        }

        void OnEnable()
        {
            loadModelWeb.ShowModels();
        }

        public void Destroy()
        {
            loadModelWeb.RemoveModels();
            loadModelWeb.OnModelsInfoLoaded -= InitCurrentModelIndex;
            left.onShortPress.RemoveAllListeners();
            right.onShortPress.RemoveAllListeners();
        }

        public void InitCurrentModelIndex()
        {
            if (loadModelWeb.ModelInfos == null || loadModelWeb.ModelInfos.Count == 0)
            {
                Debug.LogWarning("ModelInfos is empty or not initialized.");
                return;
            }

            // 查找当前加载的模型在 ModelInfos 中的索引
            for (int i = 0; i < loadModelWeb.ModelInfos.Count; i++)
            {
                if (loadModelWeb.ModelInfos[i].name == loadModelWeb.Character)
                {
                    currentModelIndex = i;
                    break;
                }
            }

        }

        private void OnLeftButtonClick()
        {
            if (loadModelWeb.ModelInfos == null || loadModelWeb.ModelInfos.Count == 0)
                return;

            currentModelIndex--;
            if (currentModelIndex < 0)
                currentModelIndex = loadModelWeb.ModelInfos.Count - 1;

            SwitchModel(currentModelIndex);
        }

        private void OnRightButtonClick()
        {
            if (loadModelWeb.ModelInfos == null || loadModelWeb.ModelInfos.Count == 0)
                return;

            currentModelIndex++;
            if (currentModelIndex >= loadModelWeb.ModelInfos.Count)
                currentModelIndex = 0;

            SwitchModel(currentModelIndex);
        }

        private void SwitchModel(int index)
        {
            if (index < 0 || index >= loadModelWeb.ModelInfos.Count)
                return;

            var modelInfo = loadModelWeb.ModelInfos[index];
            loadModelWeb.UnloadModel(loadModelWeb.Character);
            loadModelWeb.Character = modelInfo.name;
            loadModelWeb.LoadModel();
        }
    }
}