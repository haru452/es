using UnityEngine;
using System.Net.Sockets;
using System.Collections;

public class CameraUDPStreamer : MonoBehaviour
{
    private Camera targetCamera;
    
    [Header("通信設定")]
    public string ipAddress = "127.0.0.1";
    public int port = 5006; // 映像配信用に別のポートを使用

    [Header("映像設定")]
    public int resolutionWidth = 320;
    public int resolutionHeight = 240;
    [Range(10, 100)] public int jpgQuality = 40;

    private UdpClient udpClient;
    private RenderTexture renderTexture;
    private Texture2D texture2D;

    void Start()
    {
        targetCamera = GetComponent<Camera>();
        udpClient = new UdpClient();
        
        // 映像キャプチャ用のテクスチャを準備
        renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        
        // カメラの描画先を画面ではなくテクスチャに変更
        targetCamera.targetTexture = renderTexture;
        
        StartCoroutine(SendVideoFrames());
    }

    IEnumerator SendVideoFrames()
    {
        while (true)
        {
            // レンダリングが完全に終わるフレームの最後まで待つ
            yield return new WaitForEndOfFrame();

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
            texture2D.Apply();

            // JPGに圧縮（UDPのサイズ制限に収めるため）
            byte[] imageBytes = texture2D.EncodeToJPG(jpgQuality);

            // UDPの制限サイズ（約64KB = 65507バイト）を超えない場合のみ送信
            if (imageBytes.Length < 65000)
            {
                udpClient.Send(imageBytes, imageBytes.Length, ipAddress, port);
            }
            else
            {
                Debug.LogWarning("フレームサイズがUDP制限を超過: " + imageBytes.Length + " bytes");
            }
        }
    }

    void OnApplicationQuit()
    {
        if (udpClient != null) udpClient.Close();
    }
}