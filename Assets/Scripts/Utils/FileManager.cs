using System.IO;
using System;
using UnityEngine;
public class FileManager
{

    public static void MoveFile(string source, string dest)
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

    public static string GetParentFolderName(string filePath)
    {
        // 获取文件的目录路径
        # nullable enable
        string? parentDirectory = Directory.GetParent(filePath)?.FullName;
        
        if (parentDirectory == null)
        {
            return string.Empty; // 如果路径无效，返回空字符串
        }

        // 获取目录的名称（上层文件夹名称）
        string parentFolderName = new DirectoryInfo(parentDirectory).Name;
        return parentFolderName;
    }

    public static object? LoadAssetAtPath(Type type, string path)
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
