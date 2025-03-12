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
using System.Linq;


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
        #region Serialized Fields
        [Header("Display")]
        [SerializeField] private string character;
        [SerializeField] private GameObject parentObject;
        [SerializeField] private Vector3 position = Vector3.zero;
        [SerializeField] private Vector3 scale = Vector3.one;

        [Header("Model Scroll View")]
        [SerializeField] private ScrollRect modelScrollRect;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private Sprite addNewModelSprite;
        [SerializeField] private Sprite removeModelSprite;

        [Header("Motion Scroll View")]
        public ScrollRect MotionScrollRect;
        public GameObject MotionCardPrefab;

        [Header("Model Info")]
        [SerializeField] private Scrollbar modelSizeScrollbar;
        #endregion

        #region Private Fields
        private List<ModelInfo> modelInfos = new List<ModelInfo>();
        private HashSet<string> loadedModels = new HashSet<string>();
        private string baseUrl;
        private float maxModelSize = 2400f;
        private float scaleFactor = 0.5f;
        private CubismModel currentModel;
        #endregion

        #region Properties and Events
        public event Action OnModelsInfoLoaded;
        public IReadOnlyList<ModelInfo> ModelInfos => modelInfos.AsReadOnly();
        public string Character
        {
            get => character;
            set => character = value;
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            character = SettingsManager.Instance.GetSetting("Live2D.CurrentModel");
            baseUrl = SettingsManager.Instance.GetSetting("General.BaseUrl").Trim('/');
            float.TryParse(SettingsManager.Instance.GetSetting("Live2D.MaxModelSize"), out maxModelSize);
            float.TryParse(SettingsManager.Instance.GetSetting("Live2D.ScaleFactor"), out scaleFactor);
        }

        private void Start()
        {
            LoadModel();
        }
        #endregion

        #region Model Management
        [ContextMenu("ShowModels")]
        public void ShowModels()
        {
            RemoveModels();
            AddSystemCards();
            StartCoroutine(LoadModelInfo());
        }

        [ContextMenu("LoadModel")]
        public void LoadModel()
        {
            if (string.IsNullOrEmpty(character)) return;
            // remember
            SettingsManager.Instance.UpdateSetting("Live2D.CurrentModel", character);
            string modelPath = $"live2d-models/{character}/{character}.model3.json";
            string absolutePath = GetLocalPath(modelPath);
            
            if (File.Exists(absolutePath))
            {
                StartCoroutine(LoadModelFromLocal(absolutePath));
            }
            else
            {
                string modelUrl = $"{baseUrl}/{modelPath}";
                StartCoroutine(LoadModelFromWeb(modelUrl, absolutePath));
            }
        }

        public void UnloadModel(string modelName)
        {
            if (string.IsNullOrEmpty(modelName) || !loadedModels.Contains(modelName)) return;

            Transform modelTransform = parentObject.transform.Find(modelName);
            if (modelTransform == null) return;

            GameObject live2dObject = modelTransform.gameObject;
            foreach (var renderer in live2dObject.GetComponentsInChildren<Renderer>())
            {
                if (renderer.material?.mainTexture is Texture2D texture)
                {
                    Resources.UnloadAsset(texture);
                }
                Destroy(renderer.material);
            }

            Destroy(live2dObject);
            loadedModels.Remove(modelName);
        }

        [ContextMenu("UnloadModels")]
        public void UnloadModels()
        {
            foreach (string model in new List<string>(loadedModels))
            {
                UnloadModel(model);
            }
        }
        #endregion

        #region UI Management
        public void RemoveModels()
        {
            if (modelScrollRect == null || !modelScrollRect.IsActive()) return;
            foreach (Transform child in modelScrollRect.content)
            {
                if (child.TryGetComponent<LongPressButton>(out var button))
                {
                    button.onShortPress.RemoveAllListeners();
                }
                Destroy(child.gameObject);
            }
        }

        private void AddSystemCards()
        {
            AddCharacterCard("添加新模型", addNewModelSprite, () =>
                DebugWrapper.Instance.Log("请到后端live2d-models文件夹下手动放入模型文件夹"));
            AddCharacterCard("移除当前模型", removeModelSprite, UnloadModels);
        }
        #endregion

        #region Loading Coroutines
        private IEnumerator LoadModelInfo()
        {
            string localModelsPath = GetLocalPath("live2d-models");
            string infoPath = "live2d-models/info";
            string absInfoPath = GetLocalPath($"{infoPath}.json");
            string infoUrl = $"{baseUrl}/{infoPath}";

            // 用于存储所有模型信息的集合
            modelInfos.Clear();
            HashSet<ModelInfo> localModels = new ();

            // 1. 扫描本地模型文件夹（本地优先）
            Live2DModelsInfo localInfo = ScanLocalModels(localModelsPath);
            if (localInfo != null && localInfo.characters != null)
            {
                foreach (var model in localInfo.characters)
                {
                    localModels.Add(model);
                    modelInfos.Add(model);
                    AddCharacterCard(model);
                }
                Debug.LogWarning($"Found {localInfo.count} local models");
            }

            if (File.Exists(absInfoPath)) {
                File.Delete(absInfoPath);
            }
            // 2. 从网络获取模型信息并合并（仅添加不存在的模型）
            yield return DownloadFile(infoUrl, absInfoPath);
            if (TryParseJson<Live2DModelsInfo>(absInfoPath, out var webInfo))
            {
                foreach (var model in webInfo.characters)
                {
                    // 只有当本地不存在同名模型时才添加网络模型
                    if (!localModels.Any(m => m.name == model.name))
                    {
                        modelInfos.Add(model);
                        AddCharacterCard(model);
                    }
                }
                Debug.LogWarning($"Found {webInfo.count} web models, added only non-duplicates");
            }

            OnModelsInfoLoaded?.Invoke();
        }

        // 扫描本地模型文件夹
        private Live2DModelsInfo ScanLocalModels(string localModelsPath)
        {
            if (!Directory.Exists(localModelsPath))
            {
                return new Live2DModelsInfo { type = "local", count = 0, characters = Array.Empty<ModelInfo>() };
            }

            var modelFolders = Directory.GetDirectories(localModelsPath);
            List<ModelInfo> localModels = new List<ModelInfo>();

            foreach (string folder in modelFolders)
            {
                string modelName = Path.GetFileName(folder);
                string modelFile = Path.Combine(folder, $"{modelName}.model3.json");
                string avatarFile = Path.Combine(folder, $"{modelName}.png");

                if (File.Exists(modelFile))
                {
                    localModels.Add(new ModelInfo
                    {
                        name = modelName,
                        avatar = $"live2d-models/{modelName}/{modelName}.png" , // 如果存在则设置为相对路径，否则为null
                        model_path = $"live2d-models/{modelName}/{modelName}.model3.json"
                    });
                }
            }

            return new Live2DModelsInfo
            {
                type = "local",
                count = localModels.Count,
                characters = localModels.ToArray()
            };
        }


        private IEnumerator LoadModelFromLocal(string absolutePath)
        {
            yield return LoadModelCore(absolutePath);
        }

        private IEnumerator LoadModelFromWeb(string modelUrl, string absolutePath)
        {
            yield return DownloadFile(modelUrl, absolutePath);
            yield return LoadModelCore(absolutePath);
        }

        private IEnumerator LoadModelCore(string absolutePath)
        {
            string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(absolutePath));
            
            var modelJson = CubismModel3Json.LoadAtPath(absolutePath, FileManager.LoadAssetAtPath);
            if (modelJson == null)
            {
                LogError("Failed to parse model JSON");
                yield break;
            }

            yield return DownloadReferencedFiles(modelJson);
            var model = modelJson.ToModel();

            if (model != null)
            {
                PostInitModel(model, absolutePath);
                loadedModels.Add(name);
                Debug.Log("Live2D Model Loaded Successfully!");
            }
            else
            {
                LogError("Failed to instantiate Live2D Model");
            }
            OnModelsInfoLoaded?.Invoke();
        }
        #endregion

        #region Helper Methods
        private void AddCharacterCard(ModelInfo modelInfo)
        {
            if (!modelScrollRect.gameObject.activeInHierarchy) return;

            var card = Instantiate(characterCardPrefab, modelScrollRect.content);
            card.GetComponentInChildren<TMP_Text>().text = modelInfo.name;

            if (!string.IsNullOrEmpty(modelInfo.avatar))
            {
                SetupAvatar(card, modelInfo.avatar);
            }

            var button = card.GetComponent<LongPressButton>();
            button.onShortPress.AddListener(() =>
            {
                UnloadModel(character);
                character = modelInfo.name;
                LoadModel();
            });
        }

        private void AddCharacterCard(string name, Sprite sprite, Action onClick)
        {
            var card = Instantiate(characterCardPrefab, modelScrollRect.content);
            card.GetComponentInChildren<TMP_Text>().text = name;
            card.GetComponent<AvatarManager>().avatarImage.sprite = sprite;
            
            var button = card.GetComponent<LongPressButton>();
            button.onShortPress.AddListener(() => onClick?.Invoke());
        }

        private void SetupAvatar(GameObject card, string avatarPath)
        {
            string avatarUrl = $"{baseUrl}/{avatarPath}";
            string localPath = GetLocalPath(avatarPath);
            card.GetComponent<AvatarManager>().SetAvatar(avatarUrl, localPath);
        }

        private IEnumerator DownloadFile(string url, string absolutePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            yield return Download(url, absolutePath, r =>
            {
                if (!r.Success)
                {
                    LogError($"Download failed: {Path.GetFileName(absolutePath)}: {r.ErrorMessage}");
                }
            });
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

            string absolutePath = GetLocalPath($"live2d-models/{character}/{relativePath}");
            if (File.Exists(absolutePath)) yield break;

            string url = $"{baseUrl}/live2d-models/{character}/{relativePath}".Replace("\\", "/");
            yield return DownloadFile(url, absolutePath);
        }

        private IEnumerator Download(string url, string absolutePath, Action<DownloadResult> onComplete)
        {
            bool done = false;
            DownloadResult result = null;
            HttpDownloader.Instance.Download(url, absolutePath, r =>
            {
                if (r.Success && r.FilePath != absolutePath)
                {
                    FileManager.MoveFile(r.FilePath, absolutePath);
                    r.FilePath = absolutePath;
                }
                result = r;
                done = true;
            });

            yield return new WaitUntil(() => done);
            onComplete?.Invoke(result);
        }

        private string GetLocalPath(string relativePath) =>
            Path.Combine(Application.temporaryCachePath, relativePath).Replace("/", Path.DirectorySeparatorChar.ToString());

        private bool TryParseJson<T>(string path, out T result)
        {
            try
            {
                string json = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<T>(json);
                return result != null;
            }
            catch (Exception e)
            {
                LogError($"Failed to parse JSON at {path}: {e.Message}");
                result = default;
                return false;
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
            DebugWrapper.Instance.Log(message, Color.red);
        }

        public void FitModelSize()
        {
            if (currentModel == null) return;
            scaleFactor = modelSizeScrollbar.value;
            currentModel.transform.localScale = scaleFactor * new Vector3(maxModelSize, maxModelSize, 0) + Vector3.forward;
        }
        #endregion

        #region Model Initialization
        private void PostInitModel(CubismModel model, string model3JsonPath)
        {
            SetupTransform(model);
            SetupLive2DComponents(model, model3JsonPath);
            currentModel = model;
            
        }

        private void SetupTransform(CubismModel model)
        {
            Transform t = model.transform;
            t.SetParent(parentObject.transform, false);
            t.localPosition = position;
            scale = scaleFactor * new Vector3(maxModelSize, maxModelSize, 0) + Vector3.forward;
            t.localScale = scale;
        }

        private void SetupLive2DComponents(CubismModel model, string model3JsonPath)
        {
            PostInitModelLookAt(model);
            PostInitModelMouth(model);
            PostInitModelExpMotion(model, model3JsonPath);
            PostInitModelRaycast(model);
        }

        private void PostInitModelMouth(CubismModel model) 
        {
            var mouthController = model.GetComponent<CubismMouthController>();
            if (mouthController == null) {
                model.AddComponent<CubismMouthController>();
            }
            if (mouthController != null) {
                mouthController.BlendMode = CubismParameterBlendMode.Override;
                mouthController.MouthOpening = 0f;
            }
            
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
            float maxFactor = 30f;
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
        #endregion

    }
}