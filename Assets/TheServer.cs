using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using ZXing;
using ZXing.QrCode.Internal;

public class TheServer : MonoBehaviour
{
    public int Port = 30402;

    private Label _LastMessage;

    private string _LastReceived;

    private Thread? _Receiver;

    void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();

        _LastMessage = (Label)doc.rootVisualElement.Q("LastMessage");

        _LastReceived = string.Format("{0}:{1}", GetIP(), Port);

        var qr = doc.rootVisualElement.Q("QRCode");

        qr.style.backgroundImage = new Background { texture = CreateQRCode(400, 400) };

        _Receiver = new(ReceiveData) { IsBackground = true };
        _Receiver.Start();
    }

    private Texture2D CreateQRCode(int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = {
                Height = height,
                Hints = { { EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H } },
                Margin = 0,
                PureBarcode = false,
                Width = width,
            }
        };

        var pixels = writer.Write(_LastReceived);
        var tex = new Texture2D(width, height);

        tex.SetPixels32(pixels);
        tex.Apply();

        return tex;
    }

    private static string GetIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();

        return string.Empty;
    }

    void Update()
    {
        _LastMessage.text = _LastReceived;
    }

    private void ReceiveData()
    {
        for (var client = new UdpClient(Port); ;)
            try
            {
                var anyIP = new IPEndPoint(IPAddress.Any, 0);

                _LastReceived = Encoding.UTF8.GetString(client.Receive(ref anyIP));
            }
            catch (ThreadAbortException)
            {
                break;
            }
            catch (Exception err)
            {
                Debug.LogError(err.ToString());
            }
    }

    void OnDestroy()
    {
        _Receiver?.Abort();
    }
}
