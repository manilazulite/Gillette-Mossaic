using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Net.Http;
using CielaSpike.Unity.Barcode;

public class GameManager : MonoBehaviour
{
    public enum ScreenState
    {
        home = 0,
        capture,
        showCaptured,
        displyAndPrint,
        thankYou
    };

    ScreenState state = ScreenState.home;

    [SerializeField] private ScreenShotHandler scrnShotHandler;
    [SerializeField] private PrintScreenHandler printScreenHandler;

    [Header("Gameobjects")]
    [Space(5)]
    public GameObject Panel_1;
    public GameObject Panel_2;
    public GameObject Panel_3;
    public GameObject Panel_4;
    public GameObject OverlayImage;

    [Header("ButtonObjects")]
    [Space(5)]
    public GameObject StartCapture;
    public GameObject CaptureButton;
    public GameObject RetakeButton;
    public GameObject ProceedButton;
    public GameObject DisplayAndPrintButton;
    public GameObject PrintButton;
    public GameObject HomeButton;
    public GameObject BackButton;
    public GameObject MainMenu;
    public GameObject FrameBar;
    public GameObject Frame;

    [SerializeField] private TMP_InputField printerNameField;

    public RawImage ScreenShotImage;
    public RectTransform CamFeedParent;

    [Space(5)]
    public TextMeshProUGUI CountDownText, HeaderText;

    private int countDown = 3;

    public string LastSavedImageName = string.Empty;
    public string LastSavedPrintImageName = string.Empty;
    public string lastSnappedPicturePath = string.Empty;

    public List<Texture2D> CollageWall_1 = new List<Texture2D>();
    public List<RawImage> CollageWall_1_ImageList = new List<RawImage>();
    public List<RawImage> CollageWall_2_ImageList = new List<RawImage>();

    public string AssetPath;
    public string NonDisplayPath;
    public string PrintAssetPath;


    public event Action TriggerQrCodeGeneration;

    private void Awake()
    {
        scrnShotHandler = FindAnyObjectByType<ScreenShotHandler>();

        StartCapture.GetComponent<Button>().onClick.AddListener(()=> ScreenContoller(1));
        CaptureButton.GetComponent<Button>().onClick.AddListener(StartTimer);
        RetakeButton.GetComponent<Button>().onClick.AddListener(InitiateRetakPicture);
        ProceedButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(3));
        DisplayAndPrintButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            ScreenContoller(6);
            DisplayTheImagesInTheWall();
        });
        //DisplayAndPrintButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(4));
        //PrintButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(4));
        PrintButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(7));

        HomeButton.GetComponent<Button>().onClick.AddListener(Home);
        BackButton.GetComponent<Button>().onClick.AddListener(Home);   
        
        AssetPath = Application.streamingAssetsPath + "/CapturedImages";
        NonDisplayPath = Application.streamingAssetsPath + "/NonDisplay";
        PrintAssetPath = Application.streamingAssetsPath + "/printimages";

        if (!Directory.Exists(AssetPath))
        {
            Directory.CreateDirectory(AssetPath);
        }

        if (!Directory.Exists(NonDisplayPath))
        {
            Directory.CreateDirectory(NonDisplayPath);
        }

        if (!Directory.Exists(PrintAssetPath))
        {
            Directory.CreateDirectory(PrintAssetPath);
        }

        LoadTheImagesFromStreamingAssets();

        DisplayTheImagesInTheWall();

        //for (int i = 1; i < Display.displays.Length; i++)
        //{
        //    // Option A – let Unity use the monitor's native resolution
        //    Display.displays[i].Activate();

        //    // Option B – pick a resolution/refresh if you need to
        //    // Display.displays[i].Activate(1920, 1080, 60);
        //}

        Display[] displays = Display.displays;
        foreach (Display display in displays)
        {
            display.Activate();
        }

    }


    private void StartTimer()
    {
        //StartCoroutine(PrintScreenHandler.instance.ApplyTheCapturedPixelsToCustomResolution());
        
        StartCoroutine(StartCountDown());
    }

    public IEnumerator StartCountDown()
    {
        OverlayImage.SetActive(true);
        CountDownText.text = countDown.ToString();

        HomeButton.GetComponent<Button>().interactable = false;
        BackButton.GetComponent<Button>().interactable = false;

        if (countDown > 0)
        {
            countDown--;
            yield return new WaitForSeconds(1f);
            CountDownText.text = countDown.ToString();
            StartCoroutine(StartCountDown());
        }
        else if (countDown == 0)
        {
            yield return new WaitForSeconds(1f);
            StopCoroutine(StartCountDown());
            scrnShotHandler.InitiateCapture();
            OverlayImage.SetActive(false);
        }
    }

    //1545/2000

    public void ScreenContoller(int index)
    {
        switch(index)
        {
            case 1://Capture screen
                state = ScreenState.capture;
                scrnShotHandler.CameraFeedParent.SetActive(true);
                scrnShotHandler.camFeed.gameObject.SetActive(true);
                Panel_1.SetActive(false);
                Panel_2.SetActive(true);
                RetakeButton.SetActive(false);
                ProceedButton.SetActive(false);

                HomeButton.SetActive(true);
                BackButton.SetActive(true);

                Frame.SetActive(true);
                FrameBar.SetActive(true);
                
                scrnShotHandler.StartCamera();
                break;
            case 2://Captured Screen
                state = ScreenState.showCaptured;
                RetakeButton.SetActive(true);
                ProceedButton.SetActive(true);
                CaptureButton.SetActive(false);

                HomeButton.GetComponent<Button>().interactable = true;
                HomeButton.GetComponent<Button>().interactable = true;

                BackButton.SetActive(false);
                break;
            case 3://Display
                state = ScreenState.displyAndPrint;
                scrnShotHandler.webcamTexture.Stop();
                
                //CollageWall_1.Add(scrnShotHandler.texture);
                
                RetakeButton.SetActive(false);
                ProceedButton.SetActive(false);
                CaptureButton.SetActive(true);

                Panel_2.SetActive(false);
                Panel_3.SetActive(true);
                break;
            case 4://Thank you
                state = ScreenState.thankYou;
                HomeButton.GetComponent<Button>().interactable = false;
                StartCoroutine(ThankYou());

                break;
            case 6://Display
                scrnShotHandler.SaveImage(true);
                break;
            case 7://Print
                scrnShotHandler.SaveImage(false);
                state = ScreenState.thankYou;
                HomeButton.GetComponent<Button>().interactable = false;

                printScreenHandler.InitPrint();
                break;
            default:

                break;
        }
    }

    private IEnumerator ThankYou()
    {
        UnityEngine.Debug.Log("calling thank u");
        yield return new WaitForSeconds(3f);
        ScreenShotImage.gameObject.SetActive(false);
        Panel_3.SetActive(false);
        Frame.SetActive(false);
        FrameBar.SetActive(false);
        Panel_4.SetActive(true);
        HomeButton.GetComponent<Button>().interactable = true;
    }

    public void InitiateRetakPicture()
    {
        StartCoroutine(RetakePicture());
    }

    private IEnumerator RetakePicture()
    {
        countDown = 3;
        UnityEngine.Debug.Log("Inside the retake");
        yield return new WaitForSeconds(0.25f);
        //yield return new WaitForEndOfFrame();
        //scrnShotHandler.webcamTexture.Play();
        scrnShotHandler.CameraFeedParent.SetActive(true);
        scrnShotHandler.camFeed.gameObject.SetActive(true);
        //ScreenShotImage.sprite = null;
        ScreenShotImage.gameObject.SetActive(false);

        RetakeButton.SetActive(false);
        ProceedButton.SetActive(false);
        CaptureButton.SetActive(true);

        BackButton.SetActive(true);

        StartTimer();
        UnityEngine.Debug.Log("retake done");
    }    

    private void DisplayTheImagesInTheWall()
    {
        int tempval = 0;

        for (int i = CollageWall_1.Count; i > 0; i--)
        {
            CollageWall_1_ImageList[tempval].GetComponent<RawImage>().texture = CollageWall_1[i - 1];
            tempval++;
        }
    }

    //private IEnumerator DisplayAndPrint_Wall_1()
    //{
    //    yield return new WaitForSeconds(0.5f);

    //    int tempval = 0;
    //    int tempvounterlessthanten = 0;

    //    if (CollageWall_1.Count > 10)
    //    {
    //        for (int i = CollageWall_1.Count; i > CollageWall_1.Count - 10; i--)
    //        {
    //            CollageWall_1_ImageList[tempval].GetComponent<RawImage>().texture = CollageWall_1[i - 1];
    //            tempval++;
    //        }
    //    }

    //    if(CollageWall_1.Count < 10)
    //    {
    //        for(int i = CollageWall_1.Count; i > 0; i--)
    //        {
    //            CollageWall_1_ImageList[tempvounterlessthanten].GetComponent<RawImage>().texture = CollageWall_1[i - 1];
    //            tempvounterlessthanten++;
    //        }
    //    }

    //    if (CollageWall_1.Count < CollageWall_2_ImageList.Count)
    //    {
    //        for (int i = 0; i < CollageWall_1.Count; i++)
    //        {
    //            CollageWall_2_ImageList[i].texture = CollageWall_1[i];
    //        }
    //    }
    //    else
    //    {

    //        //Displaying the second wall Images
    //        for (int i = 0; i < CollageWall_2_ImageList.Count; i++)
    //        {
    //            CollageWall_2_ImageList[i].texture = CollageWall_1[i];
    //        }
    //    }

    //    ScreenContoller(4);
    //}

    public void Home()
    {
        scrnShotHandler.webcamTexture.Stop();
        ScreenShotImage.gameObject.SetActive(false);
        ResetGame();

        switch (state)
        {
            case ScreenState.capture:

                Panel_2.SetActive(false);
                Panel_1.SetActive(true);

                //HomeButton.SetActive(false);
                //BackButton.SetActive(false);
                break;

            case ScreenState.showCaptured:

                Panel_2.SetActive(false);
                Panel_1.SetActive(true);

                HomeButton.SetActive(false);
                BackButton.SetActive(false);
                RetakeButton.SetActive(false);
                ProceedButton.SetActive(false);

                CaptureButton.SetActive(true);
                break;

            case ScreenState.displyAndPrint:

                Panel_3.SetActive(false);
                Panel_1.SetActive(true);
                break;

            case ScreenState.thankYou:

                Destroy(scrnShotHandler.texture);
                Panel_4.SetActive(false);
                Panel_1.SetActive(true);
                HomeButton.SetActive(false);
                break;

            default:
                Panel_1.SetActive(true);
                break;
        }
    }    

    private void LoadTheImagesFromStreamingAssets()
    {
        string[] files = Directory.GetFiles(AssetPath, "*.png");

        if (files.Length == 0)
            return;

        if (CollageWall_1.Count == 0)
        {
            foreach (string filePath in files)
            {
                byte[] pngData = File.ReadAllBytes(filePath);

                Texture2D tex = new Texture2D(2, 2); // placeholder size
                if (tex.LoadImage(pngData)) // Loads and resizes texture
                {
                    CollageWall_1.Add(tex);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Could not load image: " + filePath);
                }
            }
        }
    }

    //private void LoadImagesFromAssetFolder()
    //{
    //    if (CollageWall_1.Count == 0)
    //    {
    //        string[] files = Directory.GetFiles(AssetPath, "*.png");

    //        foreach (string filePath in files)
    //        {
    //            byte[] pngData = File.ReadAllBytes(filePath);

    //            Texture2D tex = new Texture2D(2, 2); // placeholder size
    //            if (tex.LoadImage(pngData)) // Loads and resizes texture
    //            {
    //                //// Convert to Sprite
    //                //Sprite sprite = Sprite.Create(
    //                //    tex,
    //                //    new Rect(0, 0, tex.width, tex.height),
    //                //    new Vector2(0.5f, 0.5f)
    //                //);

    //                CollageWall_1.Add(tex);
    //            }
    //            else
    //            {
    //                Debug.LogWarning("Could not load image: " + filePath);
    //            }
    //        }

    //        for (int i = CollageWall_1.Count; i < CollageWall_1.Count - 10; i--)
    //        {
    //            CollageWall_1_ImageList[i].GetComponent<RawImage>().texture = CollageWall_1[i];
    //        }

    //    }
    //    else
    //    {
    //        for(int i = CollageWall_1.Count; i < CollageWall_1.Count - 10; i--)
    //        {
    //            CollageWall_1_ImageList[i].GetComponent<RawImage>().texture = CollageWall_1[i];
    //        }
    //    }
    //}

    #region Print the Captured Image
    public void PrintImageCommand(string fileName)
    {
        UnityEngine.Debug.Log("Called Print Image");
        // // NOTE : Printe command
        //string printerName = "HiTi P525";  //Use player pref and UI to enter the printer name
        string printerName = PlayerPrefs.GetString("printer");  //Use player pref and UI to enter the printer name

        //string _filePath = "C:\\ImagesFolder" + "\\1.jpg";
        string fullCommand =
            "rundll32 C:\\WINDOWS\\system32\\shimgvw.dll,ImageView_PrintTo " +
            "\"" +
            Application.streamingAssetsPath.Replace(@"/", @"\")
            +
            "\\printimages\\" +
            fileName +
            "\"" +
            " " +
            "\"" +
            printerName +
            "\"";
        ExecuteCommand(fullCommand);
        StartCoroutine(generateQRCodeOnQRBtn());
        
    }

    public static void ExecuteCommand(string _cmd)
    {
        UnityEngine.Debug.Log("Command Executed");
        try
        {
            Process myProcess = new Process();
            //myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.Arguments = "/c " + _cmd;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit();           
            
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }

        

        //add your logic if you want something to happen after print


    }

    #endregion
    
    public void OpenCommandPanel()
    {
        if (MainMenu.activeSelf)
        {
            if (printerNameField.text != null)
            {
                PlayerPrefs.SetString("printer", printerNameField.text);
                //printerNameForDisplay = PlayerPrefs.GetString("printer");
                MainMenu.SetActive(false);
            }
            else
            {
                UnityEngine.Debug.Log("Field is empty");
            }
        }
        else
        {            
            MainMenu.SetActive(true);
        }
    }

    #region QR code Generation

    private void PrintPicture()
    {
        //uploadImageToServerAsync();
    }

    private string qrCodeFileName = "";
    private string _capturedImagePath = "";
    
    public async void uploadImageToServerAsync(bool isDisplay)
    {
        DisplayAndPrintButton.GetComponent<Button>().interactable = false;
        PrintButton.GetComponent<Button>().interactable = false;
        HomeButton.GetComponent<Button>().interactable = false;

        var client = new HttpClient();
        string url = "";
        if (isDisplay)
        {
            url = "https://lazulite.online/routes/Sweet_Water/upload-image";
        } else
        {
            url = "https://lazulite.online/routes/Sweet_Water/upload-image";
        }
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead(lastSnappedPicturePath)), "image", lastSnappedPicturePath);
        request.Content = content;
        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var contents = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        qrCodeFileName = contents;

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            //PrintScreenHandler.instance.InitPrint();
            StartCoroutine(generateQRCodeOnQRBtn());            
        });
    }
   
    public IEnumerator generateQRCodeOnQRBtn()
    {
        yield return new WaitForSeconds(2f);
        triggerQRCodeGenerator();
    }

    private void triggerQRCodeGenerator()
    {
        if (qrCodeFileName.Length > 0)
        {
            StopCoroutine(GenQRCode());
            StartCoroutine(GenQRCode());
        }
    }

    public RawImage rwImgQRCode;
    IEnumerator GenQRCode()
    {
        yield return new WaitForEndOfFrame();
        this.generateQRCode("https://lazulite.online/routes/Sweet_Water/DownloadImage?filename=" + qrCodeFileName);
        //this.generateQRCode("http://157.175.150.146:3000/routes/Sweet_Water/DownloadImage?filename=" + qrCodeFileName);
    }

    private void generateQRCode(string _qrCodeURL)
    {
        var encoder = Barcode.GetEncoder(BarcodeType.QrCode, new QrCodeEncodeOptions
        {
            Margin = 5,
            Width = 900,
            Height = 900,
            ECLevel = QrCodeErrorCorrectionLevel.M
        });

        var result = encoder.Encode(_qrCodeURL);

        if (result.Success)
        {
            var qrCodeTexture = result.GetTexture();

            if (qrCodeTexture != null)
            {
                rwImgQRCode.texture = qrCodeTexture;
                rwImgQRCode.SetNativeSize();
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to convert result to texture.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Encoding failed: " + result.ErrorMessage);
        }

        ScreenContoller(4);
    }

    #endregion

    private void ResetGame()
    {
        countDown = 3;
        HomeButton.SetActive(false);
        BackButton.SetActive(false);

        DisplayAndPrintButton.GetComponent<Button>().interactable = true;
        PrintButton.GetComponent<Button>().interactable = true;
        HomeButton.GetComponent<Button>().interactable = true;
        BackButton.GetComponent<Button>().interactable = true;
    }
}
