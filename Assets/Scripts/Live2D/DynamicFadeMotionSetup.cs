#if true
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework.Motion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


namespace Live2D
{
    public class DynamicFadeMotionSetup : MonoBehaviour
    {
        [SerializeField] private string modelJsonPath; // 在 Inspector 中赋值，例如 "Assets/Models/model3.json"

        private CubismMotionController _motionController;
        public CubismMotionController MotionController => _motionController;
        private CubismFadeController _fadeController;
        private AnimationClip _loopMotion;
        public Dictionary<string, List<AnimationClip>> motionClipsByGroup;
        
        // UI 相关
        private ScrollRect motionScrollRect;
        private GameObject motionCardPrefab;
        private Dictionary<string, List<Button>> buttonsByGroup;

        public void Initialize(string jsonPath)
        {
            modelJsonPath = jsonPath;
            StartSetup(); // 调用初始化逻辑
        }

        private void StartSetup()
        {
            _fadeController = GetComponent<CubismFadeController>();
            // 加载并设置 FadeMotions
            LoadFadeMotions();
            
            _motionController = gameObject.AddComponent<CubismMotionController>();
            // 开始循环放Idle动画
            PlayIdleAnimation();

            
            SetupUIComponents();
            GenerateMotionButtons();

        }

        private void OnDestroy()
        {
            ClearButtons();
            motionClipsByGroup.Clear();
        }

        private void PlayIdleAnimation()
        {
            _motionController.StopAnimation(0);
            _motionController.PlayLegacyAnimation(_loopMotion, priority: CubismMotionPriority.PriorityIdle, isLoop: true);
        }
    
        /// <summary>
        /// 播放指定组名和索引的动画。
        /// </summary>
        public void PlayMotion(string groupName, int index, int layerIndex = 0, 
            int priority = CubismMotionPriority.PriorityNormal, bool isLoop = true)
        {
            if (motionClipsByGroup == null || !motionClipsByGroup.ContainsKey(groupName))
            {
                Debug.LogWarning($"Motion group '{groupName}' not found.");
                return;
            }

            var clips = motionClipsByGroup[groupName];
            if (index < 0 || index >= clips.Count)
            {
                Debug.LogWarning($"Invalid motion index {index} for group '{groupName}'.");
                return;
            }

            var clip = clips[index];
            _motionController.PlayLegacyAnimation(clip, layerIndex, priority, isLoop, speed: 1, onComplete: PlayIdleAnimation);
        }

        /// <summary>
        /// 加载 Fade 动画
        /// </summary>
        private void LoadFadeMotions()
        {
            // 加载 model3.json 并解析 Motions
            var modelJson = CubismModel3Json.LoadAtPath(modelJsonPath, FileManager.LoadAssetAtPath);
            var motions = modelJson.FileReferences.Motions;

            if (motions.GroupNames != null && motions.Motions != null && motions.GroupNames.Length == motions.Motions.Length)
            {
                // 创建 CubismFadeMotionList
                var fadeMotionList = ScriptableObject.CreateInstance<CubismFadeMotionList>();
                fadeMotionList.MotionInstanceIds = new int[0];
                fadeMotionList.CubismFadeMotionObjects = new CubismFadeMotionData[0];

                string modelJsonDir = Path.GetDirectoryName(modelJsonPath);
                motionClipsByGroup = new Dictionary<string, List<AnimationClip>>();

                // 加载每个 Motion 并缓存到字典
                for (int i = 0; i < motions.GroupNames.Length; i++)
                {
                    string groupName = motions.GroupNames[i];
                    var motionArray = motions.Motions[i];

                    // 初始化组的动画列表
                    if (!motionClipsByGroup.ContainsKey(groupName))
                    {
                        motionClipsByGroup[groupName] = new List<AnimationClip>();
                    }

                    for (int j = 0; j < motionArray.Length; j++)
                    {
                        string motionFilePath = Path.Combine(modelJsonDir, motionArray[j].File);
                        if (!File.Exists(motionFilePath)) {
                            continue;
                        }
                        var motion3JsonString = File.ReadAllText(motionFilePath);
                        CubismMotion3Json motion3Json = CubismMotion3Json.LoadFrom(motion3JsonString);
                        var clip = motion3Json.ToAnimationClip(poseJson: null);
                        clip.name = Path.GetFileNameWithoutExtension(motionFilePath);

                        string motionName = Path.GetFileName(motionFilePath);

                        // 创建并添加 FadeMotion
                        CubismFadeMotionRuntimeCreator.CreateFadeMotionForAnimationClip(
                            clip, motion3Json, motionName, fadeMotionList);

                         // 缓存 AnimationClip 到字典
                        motionClipsByGroup[groupName].Add(clip);
                    }

                    // 存储Idle动画，默认选第一个循环播放
                    if (motionArray.Length > 0 && groupName.Contains("Idle")) {
                        var clip = motionClipsByGroup[groupName].First();
                        _loopMotion = clip;
                        _loopMotion.name = clip.name;
                    }
                }

                // 绑定到 CubismFadeController
                _fadeController.CubismFadeMotionList = fadeMotionList;
             
            }
            else
            {
                Debug.LogWarning("Motions data in model3.json is invalid or missing.");
            }
        }

        #region UI Management

        private void SetupUIComponents()
        {
            var loadModelWeb = transform.parent.GetComponentInParent<LoadModelWeb>();
            if (loadModelWeb == null)
            {
                Debug.LogError("LoadModelWeb component not found in parent hierarchy.");
                return;
            }

            motionScrollRect = loadModelWeb.MotionScrollRect;
            motionCardPrefab = loadModelWeb.MotionCardPrefab;
            buttonsByGroup = new Dictionary<string, List<Button>>();

            if (motionScrollRect == null || motionCardPrefab == null)
            {
                Debug.LogError("Motion ScrollRect, Card Prefab, or Content is not assigned in LoadModelWeb.");
            }
        }

        private void GenerateMotionButtons()
        {
            // Early return with null check
            if (motionScrollRect == null || motionCardPrefab == null) return;

            ClearButtons(); // Clear existing buttons

            // Define colors
            Color lightGreen = new Color(0.8f, 1f, 0.8f);  // RGB for light green
            Color lightYellow = new Color(1f, 1f, 0.8f);  // RGB for light yellow

            foreach (var group in motionClipsByGroup)
            {
                string groupName = group.Key;
                List<AnimationClip> clips = group.Value;
                var groupButtons = new List<Button>(clips.Count * 2); // Pre-allocate capacity

                for (int index = 0; index < clips.Count; index++)
                {
                    // One-time button
                    Button oneTimeButton = CreateMotionButton(
                        groupName, 
                        index, 
                        false, 
                        $"(Once):{groupName} [{index}]", 
                        lightGreen
                    );
                    groupButtons.Add(oneTimeButton);

                    // Loop button
                    Button loopButton = CreateMotionButton(
                        groupName, 
                        index, 
                        true, 
                        $"(Loop):{groupName} [{index}]", 
                        lightYellow
                    );
                    groupButtons.Add(loopButton);
                }

                buttonsByGroup[groupName] = groupButtons;
            }
        }

        // Helper method to create buttons
        private Button CreateMotionButton(string groupName, int index, bool isLoop, string text, Color color)
        {
            GameObject buttonObj = Instantiate(motionCardPrefab, motionScrollRect.content);
            Button button = buttonObj.GetComponent<Button>();
            
            // Set text
            var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = text;

            // Set color
            if (button.TryGetComponent<Image>(out var buttonImage))
            {
                buttonImage.color = color;
            }

            // Add click listener
            button.onClick.AddListener(() => PlayMotion(groupName, index, isLoop: isLoop));
            
            return button;
        }


        private void ClearButtons()
        {
            buttonsByGroup?.Clear();

            if (motionScrollRect == null) return;

            foreach (Transform child in motionScrollRect.content.transform)
            {   
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners(); // 移除所有监听器
                }
                Destroy(child.gameObject);
            }

        }
        #endregion
    }


    public class CubismFadeMotionRuntimeCreator
    {
        /// <summary>
        /// 在运行时为给定的 AnimationClip 和对应的 Motion3Json 创建 FadeMotion 并添加到 CubismFadeMotionList。
        /// </summary>
        /// <param name="clip">输入的 AnimationClip</param>
        /// /// <param name="motionFilePath">对应的 .motion3.json 文件路径</param>
        /// <param name="motionName">动画名称</param>
        /// <param name="fadeMotionList">目标 CubismFadeMotionList</param>
        public static void CreateFadeMotionForAnimationClip(
            AnimationClip clip,
            CubismMotion3Json motion3Json,
            string motionName,
            CubismFadeMotionList fadeMotionList
        )
        {
            if (clip == null || fadeMotionList == null)
            {
                Debug.LogError("AnimationClip, CubismFadeMotionList, or motionFilePath is invalid.");
                return;
            }

            if (motion3Json == null)
            {
                Debug.LogError($"Failed to load Motion3Json");
                return;
            }

            var instanceId = 0;
            var isExistInstanceId = false;
            var events = clip.events;
            for (var k = 0; k < events.Length; ++k)
            {
                if (events[k].functionName != "InstanceId")
                {
                    continue;
                }

                instanceId = events[k].intParameter;
                isExistInstanceId = true;
                break;
            }

            // 获取或生成 InstanceId
            if (!isExistInstanceId)
            {
                instanceId = clip.GetInstanceID();
            }

            var motionIndex = -1;

            for (var i = 0; i < fadeMotionList.CubismFadeMotionObjects.Length; i++)
            {
                if (Path.GetFileName(fadeMotionList.CubismFadeMotionObjects[i].MotionName) != motionName)
                {
                    continue;
                }

                motionIndex = i;
                break;
            }

            CubismFadeMotionData fadeMotion;

            if (motionIndex != -1)
            {
                var oldFadeMotion = fadeMotionList.CubismFadeMotionObjects[motionIndex];

                fadeMotion = CubismFadeMotionData.CreateInstance(
                    oldFadeMotion,
                    motion3Json,
                    motionName,
                    clip.length
                );

                // 替换掉 EditorUtility.CopySerialized，直接赋值覆盖
                fadeMotionList.CubismFadeMotionObjects[motionIndex] = fadeMotion;
                fadeMotionList.MotionInstanceIds[motionIndex] = instanceId;
            }
            else
            {
                // Create fade motion instance.
                fadeMotion = CubismFadeMotionData.CreateInstance(
                    motion3Json,
                    motionName,
                    clip.length
                );

                motionIndex = fadeMotionList.MotionInstanceIds.Length;

                Array.Resize(ref fadeMotionList.MotionInstanceIds, motionIndex + 1);
                fadeMotionList.MotionInstanceIds[motionIndex] = instanceId;

                Array.Resize(ref fadeMotionList.CubismFadeMotionObjects, motionIndex + 1);
                fadeMotionList.CubismFadeMotionObjects[motionIndex] = fadeMotion;
            }

            Debug.Log($"FadeMotion created for {motionName} with InstanceId: {instanceId}");

            // Add animation event
            {
                var sourceAnimationEvents = clip.events;
                var index = -1;

                for(var i = 0; i < sourceAnimationEvents.Length; ++i)
                {
                    if(sourceAnimationEvents[i].functionName != "InstanceId")
                    {
                        continue;
                    }

                    index = i;
                    break;
                }

                if(index == -1)
                {
                    index = sourceAnimationEvents.Length;
                    Array.Resize(ref sourceAnimationEvents, sourceAnimationEvents.Length + 1);
                    sourceAnimationEvents[sourceAnimationEvents.Length - 1] = new AnimationEvent();
                }

                sourceAnimationEvents[index].time = 0;
                sourceAnimationEvents[index].functionName = "InstanceId";
                sourceAnimationEvents[index].intParameter = instanceId;
                sourceAnimationEvents[index].messageOptions = SendMessageOptions.DontRequireReceiver;
                // 运行时覆盖
                clip.events = sourceAnimationEvents;
            }

        }
    }
}
#endif