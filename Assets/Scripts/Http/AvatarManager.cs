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
    private object lockObj = new ();

    public void SetAvatar(string url, string absolutePath)
    {   
        HttpDownloader.Instance.Download(url, absolutePath, r => {
            StartCoroutine(LoadAndProcessImage(absolutePath));
        });
    }

    /// <summary>
    /// 加载并处理本地图片
    /// </summary>
    private IEnumerator LoadAndProcessImage(string filePath)
    {
        lock (lockObj) {
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