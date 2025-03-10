using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Live2D.Cubism.Framework.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using Unity.VisualScripting;
using Live2D.Cubism.Framework.LookAt;
using Live2D.Cubism.Samples.OriginalWorkflow.Demo;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Framework.MouthMovement;


namespace Live2D 
{
    [Serializable]
    public class Live2DModelsInfo
    {
        public string type;
        public int count;
        public ModelInfo[] characters;
        
    }

    [Serializable]
    public class ModelInfo
    {
        public string name;
        public string avatar;
        public string model_path;
    }

    public class LoadModelWeb : MonoBehaviour
    {
        # region Display
        [Header("Display")]
        [SerializeField] private string character;  // Character name for the model
        [SerializeField] private GameObject parentObject;  // Parent object for the model
        [SerializeField] private Vector3 position;         // Local position of the model
        [SerializeField] private Vector3 scale;            // Local scale of the model
        #endregion

        #region  Model Scroll View
        [Header("Model Scroll View")]
        [SerializeField] private ScrollRect modelScrollRect;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private Sprite addNewModelSprite; 
        [SerializeField] private Sprite removeModelSprite; 
        #endregion

        #region  Motion Scroll View
        [Header("Motion Scroll View")]
        public ScrollRect MotionScrollRect;
        public GameObject MotionCardPrefab;
        #endregion

        private List<string> loadedModels;
        private List<ModelInfo> modelInfos;
        private string baseUrl;                            // Base URL from settings
        private string localRoot;                          // Local storage root

        public event Action onModelsInfoLoaded; 

        public List<ModelInfo> ModelInfos {
            get { return modelInfos; }
        }
        public string Character {
            get { return character; }
            set { character = value; }
        }

        void Start()
        {
            baseUrl = SettingsManager.Instance.GetSetting("General.BaseUrl").Trim('/');
            localRoot = Application.temporaryCachePath;
            loadedModels = new List<string>();
            // 默认load一个模型, 设置为character
            LoadModel();
        }

        public void RemoveModels()
        {
            foreach (Transform child in modelScrollRect.content.transform)
            {   
                var button = child.GetComponent<LongPressButton>();
                if (button != null)
                {
                    button.onShortPress.RemoveAllListeners(); // 移除所有监听器
                }
                Destroy(child.gameObject);
            }
        }

        [ContextMenu("ShowModels")]
        public void ShowModels() 
        {
            // 移除先前所有的card
            RemoveModels();
            // system card
            AddCharacterCard("添加新模型", addNewModelSprite);
            AddCharacterCard("移除当前模型", removeModelSprite);

            // user card
            string infoUrl = $"{baseUrl}/live2d-models/info";
            StartCoroutine(LoadModelsFromWeb(infoUrl, "live2d-model-info.json"));
        }

        [ContextMenu("LoadModel")]
        public void LoadModel()
        {
            if (!string.IsNullOrEmpty(character))
            {
                string modelUrl = $"{baseUrl}/live2d-models/{character}/{character}.model3.json";
                StartCoroutine(LoadModelFromWeb(modelUrl));
            }
        }

        public void UnloadModel(string c)
        {
            if (!string.IsNullOrEmpty(c) && loadedModels.Contains(c)) 
            {
                GameObject live2dObject = parentObject.transform.Find(c).gameObject;
                
                // 释放纹理资源
                var renderers = live2dObject.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (renderer.material != null)
                    {
                        Texture2D texture = renderer.material.mainTexture as Texture2D;
                        if (texture != null)
                        {
                            Resources.UnloadAsset(texture);
                        }
                        Destroy(renderer.material);
                    }
                }

                Destroy(live2dObject);
                loadedModels.Remove(c);
            }
        }

        [ContextMenu("UnloadModels")]
        public void UnloadModels()
        {
            var tmps = new List<string>(loadedModels);
            foreach (string character in tmps)
            {
                UnloadModel(character);
            }
            loadedModels.Clear();
        }

        private IEnumerator LoadModelsFromWeb(string infoUrl, string fileName) 
        {
            string jsonPath = null;
            yield return Download(infoUrl, fileName, r => jsonPath = r.Success ? r.FilePath : null);
            if (jsonPath == null) {
                yield break;
            }
            var info = JsonConvert.DeserializeObject<Live2DModelsInfo>(File.ReadAllText(jsonPath));
            if (info == null) {
                Debug.LogError("Failed to parse live2d-models JSON");
            }
            Debug.Log($"Found: {info.count} models");
            modelInfos = new (info.characters);
            
            foreach (var modelInfo in modelInfos)
            {
                AddCharacterCard(modelInfo);
            }
            onModelsInfoLoaded?.Invoke();
        }

        private void AddCharacterCard(ModelInfo modelInfo)
        {
            var card = Instantiate(characterCardPrefab, modelScrollRect.content.transform);
                
            var textObj = card.GetComponentInChildren<TMP_Text>();
            textObj.text = modelInfo.name; // 设置名称

            if (modelInfo.avatar != null) {
                var avatarUrl = $"{baseUrl}/{modelInfo.avatar}";
                var avatarManager = card.GetComponent<AvatarManager>();
                var folderPath = Path.Combine(localRoot, "live2d-models", modelInfo.name);
                avatarManager.SetAvatar(avatarUrl, folderPath);
            }

            var button = card.GetComponent<LongPressButton>();
            button.onShortPress.AddListener(() => {
                UnloadModel(character);
                character = modelInfo.name;
                LoadModel();
            });
        }

        private void AddCharacterCard(string c_name, Sprite sprite)
        {
            var card = Instantiate(characterCardPrefab, modelScrollRect.content.transform);
            var textObj = card.GetComponentInChildren<TMP_Text>();
            textObj.text = c_name;
            var avatarManager = card.GetComponent<AvatarManager>();
            avatarManager.avatarImage.sprite = sprite;
            var button = card.GetComponent<LongPressButton>();
            if (c_name.Contains("添加")) {
                button.onShortPress.AddListener(() => {
                    DebugWrapper.Instance.Log("请到后端live2d-models文件夹下手动放入模型文件夹");
                });
            }
            else if (c_name.Contains("移除")) {
                button.onShortPress.AddListener(() => {
                    UnloadModels();
                });
            }
        }
        

        private IEnumerator LoadModelFromWeb(string modelUrl)
        {
            string jsonPath = null;
            yield return Download(modelUrl, r => jsonPath = r.Success ? r.FilePath : null);
            string lastPart = Path.GetFileName(modelUrl);
            string name = lastPart.Split('.')[0];

            if (jsonPath == null)
            {
                var err = $"Failed to download {lastPart}";
                Debug.LogError(err);
                DebugWrapper.Instance.Log(err, Color.red);
                yield break;
            }

            Debug.LogWarning($"jsonPath: {jsonPath}");

            var modelJson = CubismModel3Json.LoadAtPath(jsonPath, FileManager.LoadAssetAtPath);
            if (modelJson == null)
            {
                var err = "Failed to parse model JSON";
                Debug.LogError(err);
                DebugWrapper.Instance.Log(err, Color.red);
                yield break;
            }
            
            yield return DownloadReferencedFiles(modelJson);
            var model = modelJson.ToModel();

            if (model != null)
            {
                PostInitModel(model, jsonPath);
                loadedModels.Add(name);
                var msg = "Live2D Model Loaded Successfully!";
                Debug.Log(msg);
                DebugWrapper.Instance.Log(msg);
            }
            else
            {
                var err = "Failed to instantiate Live2D Model";
                Debug.LogError(err);
                DebugWrapper.Instance.Log(err, Color.red);
            }
            onModelsInfoLoaded?.Invoke();
        }

        private void PostInitModel(CubismModel model, string model3JsonPath)
        {
            #region Unity Part
            Transform t = model.transform;
            t.parent = parentObject.transform;
            t.localPosition = position;
            t.localScale = scale;
            #endregion

            #region Live2D Part

            PostInitModelLookAt(model);

            PostInitModelMouth(model);

            PostInitModelExpMotion(model, model3JsonPath);
  
            PostInitModelRaycast(model);

            #endregion
        }

        private void PostInitModelMouth(CubismModel model) 
        {
            var mouthController = model.GetComponent<CubismMouthController>();
            if (mouthController == null) {
                model.AddComponent<CubismMouthController>();
            }
            mouthController.BlendMode = CubismParameterBlendMode.Override;
            mouthController.MouthOpening = 0f;
            
            var ParamMouthUp = model.Parameters.FindById("ParamMouthUp")?.AddComponent<CubismMouthParameter>();
            if (ParamMouthUp == null) {
                ParamMouthUp = model.Parameters.FindById("ParamMouthOpenY")?.AddComponent<CubismMouthParameter>();
            }
            if (ParamMouthUp == null) {
                return;
            }
            var audioMouthInput = model.AddComponent<MouthInputController>();
            audioMouthInput.SamplingQuality = CubismAudioSamplingQuality.VeryHigh;
            audioMouthInput.Gain = 5;
            audioMouthInput.Smoothing = 0.2f;
        }

        private void PostInitModelLookAt(CubismModel model) 
        {
            Transform t = model.transform;
            string[] paramIds = { "ParamAngleX", "ParamAngleY", "ParamEyeBallX", "ParamEyeBallY" };
            float maxFactor = model.Parameters[0].MaximumValue;
            foreach (var paramId in paramIds)
            {
                var param = model.Parameters.FindById(paramId)?.AddComponent<CubismLookParameter>();
                if (param == null) {
                    continue;
                }
                if (param.name.EndsWith("Y"))
                {
                    param.Axis = CubismLookAxis.Y;
                }
                else if (param.name.EndsWith("Z"))
                {
                    param.Axis = CubismLookAxis.Z;
                }
                else
                {
                    param.Axis = CubismLookAxis.X;
                }
                param.Factor = maxFactor;
                
            }

            var cubismLookTarget = model.AddComponent<CubismLookTarget>();
            cubismLookTarget.Center = t;
            var cubismLookController = model.AddComponent<CubismLookController>();
            cubismLookController.Center = t;
            cubismLookController.Target = cubismLookTarget;
        }

        private void PostInitModelRaycast(CubismModel model)
        {
            var cubismRaycastable = model.AddComponent<CubismRaycastable>();  
            cubismRaycastable.Precision = CubismRaycastablePrecision.BoundingBox; // Triangles
            model.AddComponent<CubismRaycaster>();  
            model.AddComponent<CanvasRenderer>();
            model.AddComponent<HitRaycaster>();
            model.AddComponent<DragController>();
            
            var motionController = model.AddComponent<MotionController>();
            motionController.Initialize(model);
        }

        private void PostInitModelExpMotion(CubismModel model, string model3JsonPath)
        {
            model.AddComponent<CubismUpdateController>();
            
            model.AddComponent<CubismPoseController>();
            model.AddComponent<CubismExpressionController>();
#if false   // For UnityEditor Only
            var animatorSetup = model.AddComponent<DynamicAnimatorSetup>();
            animatorSetup.Initialize(model3JsonPath);
#endif
            var paramStore = model.AddComponent<CubismParameterStore>();
            var controller = model.AddComponent<CubismFadeController>();

            var fadeMotionSetup = model.AddComponent<DynamicFadeMotionSetup>();
            fadeMotionSetup.Initialize(model3JsonPath);

            var expressionSetup = model.AddComponent<DynamicExpressionSetup>();
            expressionSetup.Initialize(model3JsonPath);  
        }

        private IEnumerator DownloadReferencedFiles(CubismModel3Json modelJson)
        {
            var refs = modelJson.FileReferences;
            yield return DownloadIfNeeded(refs.Moc);
            if (refs.Textures != null)
                foreach (string texture in refs.Textures)
                    yield return DownloadIfNeeded(texture);
            yield return DownloadIfNeeded(refs.Physics);
            yield return DownloadIfNeeded(refs.Pose);
            yield return DownloadIfNeeded(refs.DisplayInfo);
            yield return DownloadIfNeeded(refs.UserData);
            if (refs.Expressions != null)
                foreach (var exp in refs.Expressions)
                    yield return DownloadIfNeeded(exp.File);
            if (refs.Motions.Motions != null && refs.Motions.GroupNames != null)
                for (int i = 0; i < Math.Min(refs.Motions.Motions.Length, refs.Motions.GroupNames.Length); i++)
                    if (refs.Motions.Motions[i] != null)
                        foreach (var m in refs.Motions.Motions[i])
                        {
                            yield return DownloadIfNeeded(m.File);
                            yield return DownloadIfNeeded(m.Sound);
                        }
        }

        private IEnumerator DownloadIfNeeded(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) yield break;

            string url = $"{baseUrl}/live2d-models/{character}/{relativePath}".Replace("\\", "/");
            string localPath = LocalPath(relativePath);
            
            if (File.Exists(localPath)) yield break;

            yield return Download(url, r =>
            {
                if (!r.Success)
                    Debug.LogError($"Download failed: {relativePath}: {r.ErrorMessage}");
            });
        }

        private IEnumerator Download(string url, string fileName, Action<DownloadResult> onComplete)
        {
            string localPath = LocalPath(fileName);
            yield return DownloadInternal(url, localPath, onComplete);
        }

        private IEnumerator Download(string url, Action<DownloadResult> onComplete)
        {
            string localPath = LocalPath(url.Replace(baseUrl, "").Trim('/'));
            yield return DownloadInternal(url, localPath, onComplete);
        }

        private IEnumerator DownloadInternal(string url, string localPath, Action<DownloadResult> onComplete)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath) ?? "");

            bool done = false;
            DownloadResult result = null;
            HttpDownloader.Instance.Download(url, r =>
            {
                if (r.Success && r.FilePath != localPath)
                {
                    FileManager.MoveFile(r.FilePath, localPath);
                    r.FilePath = localPath;
                }
                result = r;
                done = true;
            });

            yield return new WaitUntil(() => done);
            onComplete?.Invoke(result);
        }

        private string LocalPath(string relativePath) =>
            Path.Combine(localRoot, relativePath).Replace("/", Path.DirectorySeparatorChar.ToString());
       
    }
}