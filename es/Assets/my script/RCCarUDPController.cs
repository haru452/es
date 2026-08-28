using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class RCCarUDPController : MonoBehaviour
{
    private Thread receiveThread;
    private UdpClient client;
    
    [Header("通信設定")]
    public int port = 5005;

    [Header("ラジコン設定")]
    public float moveSpeed = 5.0f;
    public float turnSpeed = 100.0f;

    [Header("カメラ設定")]
    public Transform cameraTransform; // ここにUnityのエディタからカメラをアタッチします
    public float cameraSensitivity = 0.5f;

    private string lastMoveCommand = "STOP";

    // カメラの現在の回転角度
    private float cameraPitch = 0.0f;
    private float cameraYaw = 0.0f;
// ▼既存の変数の下に追加
    private float initialPitch = 0.0f;
    private float initialYaw = 0.0f;

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("UDPレシーバー起動: ポート " + port);

        if (cameraTransform != null)
        {
            // 初期角度を取得
            Vector3 angles = cameraTransform.localEulerAngles;
            cameraPitch = angles.x;
            cameraYaw = angles.y;

            // ▼追加: 起動時のカメラの角度を「初期位置」として記憶する
            initialPitch = cameraPitch;
            initialYaw = cameraYaw;
        }
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                
                // ▼追加: リセットコマンドが来たら初期角度に戻す
                if (text == "CAMERA_RESET")
                {
                    cameraPitch = initialPitch;
                    cameraYaw = initialYaw;
                }
                // 既存のカメラ操作処理
                else if (text.StartsWith("CAMERA:"))
                {
                    string[] parts = text.Split(':');
                    if (parts.Length == 3)
                    {
                        float dx = float.Parse(parts[1]);
                        float dy = float.Parse(parts[2]);
                        
                        cameraYaw += dx * cameraSensitivity;
                        cameraPitch += dy * cameraSensitivity; 
                        cameraPitch = Mathf.Clamp(cameraPitch, -45f, 45f); 
                    }
                }
                else
                {
                    lastMoveCommand = text;
                }
            }
            catch (System.Exception)
            {
                break; 
            }
        }
    }

    void Update()
    {
        // 1. ラジコンの移動処理
        switch (lastMoveCommand)
        {
            case "FORWARD":
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
                break;
            case "BACKWARD":
                transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
                break;
            case "LEFT":
                transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
                break;
            case "RIGHT":
                transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
                break;
        }

        // 2. カメラの回転処理
        if (cameraTransform != null)
        {
            // カメラ単体を回転させる
            cameraTransform.localEulerAngles = new Vector3(cameraPitch, cameraYaw, 0f);
        }
    }

    void OnApplicationQuit()
    {
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join(500);
    }
}