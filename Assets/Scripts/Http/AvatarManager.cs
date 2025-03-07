using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class AvatarManager : MonoBehaviour
{
    public Image avatarImage; // 在 Inspector 中绑定的 Image 组件
    public int width = 128;
    public int height = 128;
    // 静态字典，存储人名和对应的 URL
    private static Dictionary<string, string> avatarUrls = new Dictionary<string, string>();

    /// <summary>
    /// 添加或更新人名和 URL
    /// </summary>
    public static void AddOrUpdateAvatarUrl(string name, string url)
    {
        if (avatarUrls.ContainsKey(name))
        {
            avatarUrls[name] = url; // 更新已有条目
        }
        else
        {
            avatarUrls.Add(name, url); // 添加新条目
        }
    }

    /// <summary>
    /// 删除人名和 URL
    /// </summary>
    public static void RemoveAvatarUrl(string name)
    {
        if (avatarUrls.ContainsKey(name))
        {
            avatarUrls.Remove(name);
        }
    }

    /// <summary>
    /// 获取人名对应的 URL
    /// </summary>
    public static string GetAvatarUrl(string name)
    {
        if (avatarUrls.TryGetValue(name, out string url))
        {
            return url;
        }
        return null; // 未找到则返回 null
    }

    /// <summary>
    /// 根据人名下载并设置头像图片
    /// </summary>
    public void SetAvatarByName(string name)
    {
        string url = GetAvatarUrl(name);
        if (url != null)
        {
            SetAvatar(url); // 使用 URL 下载并设置头像
        }
        else
        {
            Debug.LogError("未找到人名对应的 URL: " + name);
        }
    }

    /// <summary>
    /// 下载并设置头像图片
    /// </summary>
    public void SetAvatar(string url, string folderPath = null)
    {
        
        HttpDownloader.Instance.Download(url, result =>
        {
            if (result.Success)
            {
                var targetPath = result.FilePath;
                if (folderPath != null) {
                    Directory.CreateDirectory(folderPath ?? "");
                    targetPath = Path.Combine(folderPath, Path.GetFileName(result.FilePath));
                    MoveFile(result.FilePath, targetPath);
                }
                StartCoroutine(LoadAndProcessImage(targetPath));
            }
            else
            {
                var err = "下载avatar失败: " + result.ErrorMessage;
                DebugWrapper.Instance.Log(err, Color.red);
                Debug.LogError(err);
            }
        });
    }

    /// <summary>
    /// 加载并处理本地图片
    /// </summary>
    private IEnumerator LoadAndProcessImage(string filePath)
    {
        Texture2D texture = LoadTextureFromFile(filePath);
        if (texture == null)
        {
            Debug.LogError("无法加载图片: " + filePath);
            yield break;
        }

        Texture2D resizedTexture = ResizeTexture(texture, width, height);
        Sprite sprite = TextureToSprite(resizedTexture);
        avatarImage.sprite = sprite;
    }

    /// <summary>
    /// 从本地文件加载 Texture2D
    /// </summary>
    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }

    /// <summary>
    /// 调整 Texture2D 至指定尺寸
    /// </summary>
    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D newTexture = new Texture2D(newWidth, newHeight);
        newTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return newTexture;
    }

    /// <summary>
    /// 将 Texture2D 转换为 Sprite
    /// </summary>
    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

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
}