using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CielaSpike.Unity.Barcode;

public class QRCodeGenerator : MonoBehaviour
{
    public static QRCodeGenerator INSQRCodeGenerator;
    public RawImage rwImgQRCode;

    void Awake()
    {
        INSQRCodeGenerator = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GenerateQRCode(string _QRCodeURL)
    {
        var encoder = Barcode.GetEncoder(BarcodeType.QrCode, new QrCodeEncodeOptions
        {
            Margin = 5,
            Width = 600,
            Height = 600,
            ECLevel = QrCodeErrorCorrectionLevel.M
        });

        var result = encoder.Encode(_QRCodeURL);
        if (result.Success)
        {
            var qrCodeTexture = result.GetTexture();

            if (qrCodeTexture != null)
            {
                rwImgQRCode.texture = qrCodeTexture;
                rwImgQRCode.SetNativeSize();
                // Adjust the size of the RawImage to fit the QR code
            }
            else
            {
                Debug.LogError("Failed to convert result to texture.");
            }
        }
        else
        {
            Debug.LogError("Encoding failed: " + result.ErrorMessage);
        }
    }
}
