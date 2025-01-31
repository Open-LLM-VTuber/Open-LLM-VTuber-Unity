using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    public RawImage rawImage; // 用于显示摄像头画面
    public Image backgroundImage; // 原有背景

    public Image cameraButton; // 摄像头按钮
    public Color activeColor = new Color(0f, 0.5f, 1f); // 淡蓝色
    public Color inactiveColor = new Color(0f, 0f, 0f); // 黑色

    public bool mirror = true; // 是否启用镜像
    private bool isCameraOn = false; // 摄像头状态

    void Start()
    {
        // 默认隐藏RawImage，显示背景
        rawImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(true);

        // 初始化按钮颜色为非激活状态
        if (cameraButton != null)
        {
            cameraButton.color = inactiveColor;
        }
        else
        {
            Debug.LogError("Camera button not assigned!");
        }
    }

    // 切换按钮颜色的方法
    public void ToggleButtonColor()
    {
        if (isCameraOn)
        {
            cameraButton.color = activeColor; // 摄像头打开时为淡蓝色
        }
        else
        {
            cameraButton.color = inactiveColor; // 摄像头关闭时为黑色
        }
    }

    // 切换摄像头状态的方法
    public void ToggleCamera()
    {
        if (isCameraOn)
        {
            // 关闭摄像头
            CloseCamera();
        }
        else
        {
            // 打开摄像头
            OpenCamera();
        }

        // 切换状态
        isCameraOn = !isCameraOn;

        // 切换按钮颜色
        ToggleButtonColor();
    }

    void OpenCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            // 初始化摄像头
            webCamTexture = new WebCamTexture(devices[0].name); // 使用默认分辨率
            rawImage.texture = webCamTexture;
            webCamTexture.Play();

            // 显示RawImage，隐藏背景
            rawImage.gameObject.SetActive(true);
            //backgroundImage.gameObject.SetActive(false);

            // 等待摄像头启动完成后调整画面
            StartCoroutine(WaitForCameraStart());
        }
        else
        {
            Debug.LogError("No camera found on the device.");
        }
    }

    void CloseCamera()
    {
        if (webCamTexture != null)
        {
            // 停止摄像头
            webCamTexture.Stop();

            // 隐藏RawImage，显示背景
            rawImage.gameObject.SetActive(false);
            //backgroundImage.gameObject.SetActive(true);
        }
    }

    IEnumerator WaitForCameraStart()
    {
        // 等待摄像头启动完成
        while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            yield return null; // 等待一帧
        }

        // 摄像头启动完成后调整画面
        AdjustCameraView();
    }

    void AdjustCameraView()
    {
        if (webCamTexture.width > 0 && webCamTexture.height > 0)
        {
            // 获取RawImage的实际尺寸
            float rawImageWidth = rawImage.rectTransform.rect.width;
            float rawImageHeight = rawImage.rectTransform.rect.height;

            // 获取摄像头的宽高
            float cameraWidth = webCamTexture.width;
            float cameraHeight = webCamTexture.height;

            // 计算RawImage和摄像头的宽高比
            float rawImageAspect = rawImageWidth / rawImageHeight;
            float cameraAspect = cameraWidth / cameraHeight;

            // 计算裁剪区域
            Rect uvRect;
            if (cameraAspect > rawImageAspect)
            {
                // 摄像头的宽高比大于RawImage，裁剪宽度
                float scale = rawImageHeight / cameraHeight;
                float scaledCameraWidth = cameraWidth * scale;
                float cropWidth = (scaledCameraWidth - rawImageWidth) / 2;

                uvRect = new Rect(
                    cropWidth / scaledCameraWidth, // UV起点X
                    0, // UV起点Y
                    rawImageWidth / scaledCameraWidth, // UV宽度
                    1 // UV高度
                );
            }
            else
            {
                // 摄像头的宽高比小于RawImage，裁剪高度
                float scale = rawImageWidth / cameraWidth;
                float scaledCameraHeight = cameraHeight * scale;
                float cropHeight = (scaledCameraHeight - rawImageHeight) / 2;

                uvRect = new Rect(
                    0, // UV起点X
                    cropHeight / scaledCameraHeight, // UV起点Y
                    1, // UV宽度
                    rawImageHeight / scaledCameraHeight // UV高度
                );
            }

            // 设置RawImage的UV坐标
            rawImage.uvRect = uvRect;

            // 镜像处理
            if (mirror)
            {
                rawImage.rectTransform.localScale = new Vector3(-1, 1, 1); // 水平翻转
            }
            else
            {
                rawImage.rectTransform.localScale = new Vector3(1, 1, 1); // 恢复正常
            }
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }
    }
}