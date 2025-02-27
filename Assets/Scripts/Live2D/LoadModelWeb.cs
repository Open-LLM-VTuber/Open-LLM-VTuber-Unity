using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Live2D.Cubism.Framework.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

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

        #region  Horizontal Scroll View
        [Header("Horizontal Scroll View")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject characterCardPrefab;             
        #endregion

        private List<string> characters;
        private string baseUrl;                            // Base URL from settings
        private string localRoot;                          // Local storage root

        void Start()
        {
            baseUrl = SettingsManager.Instance.GetSetting("General.BaseUrl").Trim('/');
            localRoot = Application.persistentDataPath;
            characters = new List<string>();
        }

        [ContextMenu("ShowModels")]
        public void ShowModels() 
        {
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
            if (!string.IsNullOrEmpty(c))
            {
                GameObject live2dObject = parentObject.transform.Find(c).gameObject;
                Destroy(live2dObject);
                characters.Remove(c);
            }
        }

        [ContextMenu("UnloadModels")]
        public void UnloadModels()
        {
            var tmps = new List<string>(characters);
            foreach (string character in tmps)
            {
                UnloadModel(character);
            }
            characters.Clear();
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

            foreach (Transform child in scrollRect.content.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var character in info.characters)
            {
                var card = Instantiate(characterCardPrefab, scrollRect.content.transform);
                
                var textObj = card.GetComponentInChildren<TMP_Text>();
                textObj.text = character.name; // 设置名称

                if (character.avatar != null) {
                    var avatarUrl = $"{baseUrl}/{character.avatar}";
                    var avatarManager = card.GetComponent<AvatarManager>();
                    var folderPath = Path.Combine(localRoot, "live2d-models", character.name);
                    avatarManager.SetAvatar(avatarUrl, folderPath);
                }

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
                Debug.LogError($"Failed to download {lastPart}");
                yield break;
            }

            Debug.LogWarning($"jsonPath: {jsonPath}");

            var modelJson = CubismModel3Json.LoadAtPath(jsonPath, LoadAssetAtPath);
            if (modelJson == null)
            {
                Debug.LogError("Failed to parse model JSON");
                yield break;
            }
            
            yield return DownloadReferencedFiles(modelJson);
            var model = modelJson.ToModel();

            if (model != null)
            {
                Transform t = model.transform;
                t.parent = parentObject.transform;
                t.localPosition = position;
                t.localScale = scale;
                characters.Add(name);
                Debug.Log("Live2D Model Loaded Successfully!");
            }
            else
            {
                Debug.LogError("Failed to instantiate Live2D Model");
            }
            
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
                    MoveFile(r.FilePath, localPath);
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

        private static void MoveFile(string source, string dest)
        {
            try
            {
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                if (File.Exists(source))
                {
                    File.Move(source, dest);
                }
            }
            catch (IOException ex)
            {
                Debug.LogWarning($"Move failed: {source} to {dest}: {ex.Message}");
            }
        }

        private static object LoadAssetAtPath(Type type, string path)
        {
            if (type == typeof(byte[])) return File.ReadAllBytes(path);
            else if (type == typeof(string)) return File.ReadAllText(path);
            else if (type == typeof(Texture2D))
            {
                var texture = new Texture2D(1, 1);
                byte[] bytes = File.ReadAllBytes(path);
                if (!texture.LoadImage(bytes))
                {
                    Debug.LogError($"Failed to load texture at {path}");
                    return null;
                }
                return texture;
            }
            throw new NotSupportedException();
        }
    }
}