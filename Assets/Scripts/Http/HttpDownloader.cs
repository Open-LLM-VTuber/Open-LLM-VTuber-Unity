using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

public class HttpDownloader : InitOnceSingleton<HttpDownloader>
{
   
    // 下载方法，接受 URL 和回调函数
    public void Download(string url, string absolutePath, Action<DownloadResult> callback = null)
    {
        StartCoroutine(DownloadCoroutine(url, absolutePath, callback));
    }

    // 下载协程，处理异步下载逻辑
    private IEnumerator DownloadCoroutine(string url, string absolutePath, Action<DownloadResult> callback = null)
    {
        string directory = Path.GetDirectoryName(absolutePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 创建下载结果对象
        DownloadResult result = new DownloadResult();

        // 如果文件存在，跳过, unity读取图片时会占用，删不掉
        if (File.Exists(absolutePath) && new FileInfo(absolutePath).Length > 0)
        {
            result.Success = true;
            result.FilePath = absolutePath;
            result.FileSize = new FileInfo(absolutePath).Length; // 获取文件大小
            result.CompletionTime = DateTime.Now;           // 记录完成时间
            callback?.Invoke(result);
            yield break;
        }

        // 使用 UnityWebRequest 发起请求
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // 设置下载处理器，直接保存到文件
            www.downloadHandler = new DownloadHandlerFile(absolutePath);
            yield return www.SendWebRequest();

            // 检查下载是否成功
            if (www.result == UnityWebRequest.Result.Success)
            {
                result.Success = true;
                result.FilePath = absolutePath;
                result.FileSize = new FileInfo(absolutePath).Length; // 获取文件大小
                result.CompletionTime = DateTime.Now;           // 记录完成时间
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = www.error; // 记录错误信息
                if (File.Exists(absolutePath)) {
                    File.Delete(absolutePath);
                }
            }

            // 调用回调函数，返回结果
            callback?.Invoke(result);
        }
    }
}

public class DownloadResult
{
    public bool Success { get; set; } // 是否下载成功
    public string FilePath { get; set; } // 文件保存路径
    public long FileSize { get; set; } // 文件大小（字节）
    public DateTime CompletionTime { get; set; } // 完成时间
    public string ErrorMessage { get; set; } // 错误信息（如果失败）

    public override string ToString()
    {
        return $"DownloadResult: Success={Success}, Path={FilePath}, " +
            $"Size={FileSize} bytes, Time={CompletionTime}, Error={ErrorMessage}";
    }
}