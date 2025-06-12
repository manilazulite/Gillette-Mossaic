using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Collections.Generic;

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

    public RawImage ScreenShotImage;

    [Space(5)]
    public TextMeshProUGUI CountDownText, HeaderText;

    private int countDown = 3;

    public string LastSavedImageName = string.Empty;

    public List<Texture2D> CollageWall_1 = new List<Texture2D>();
    public List<RawImage> CollageWall_1_ImageList = new List<RawImage>();

    public string AssetPath; 

    private void Awake()
    {
        scrnShotHandler = FindAnyObjectByType<ScreenShotHandler>();

        StartCapture.GetComponent<Button>().onClick.AddListener(()=> ScreenContoller(1));
        CaptureButton.GetComponent<Button>().onClick.AddListener(StartTimer);
        RetakeButton.GetComponent<Button>().onClick.AddListener(InitiateRetakPicture);
        ProceedButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(3));
        //DisplayAndPrintButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(4));
        DisplayAndPrintButton.GetComponent<Button>().onClick.AddListener(InitiateDisplayOrPrint);
        PrintButton.GetComponent<Button>().onClick.AddListener(()=>ScreenContoller(4));

        HomeButton.GetComponent<Button>().onClick.AddListener(Home);
        BackButton.GetComponent<Button>().onClick.AddListener(Home);   
        
        AssetPath = Application.streamingAssetsPath + "/CapturedImages";

        LoadTheImagesFromStreamingAssets();
    }

    private void StartTimer()
    {
        StartCoroutine(StartCountDown());
    }

    public IEnumerator StartCountDown()
    {
        OverlayImage.SetActive(true);
        CountDownText.text = countDown.ToString();

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

    public void ScreenContoller(int index)
    {
        switch(index)
        {
            case 1://Capture screen
                state = ScreenState.capture;
                scrnShotHandler.camFeed.gameObject.SetActive(true);
                Panel_1.SetActive(false);
                Panel_2.SetActive(true);
                RetakeButton.SetActive(false);
                ProceedButton.SetActive(false);

                HomeButton.SetActive(true);
                BackButton.SetActive(true);
                
                scrnShotHandler.StartCamera();
                break;
            case 2://Captured Screen
                state = ScreenState.showCaptured;
                RetakeButton.SetActive(true);
                ProceedButton.SetActive(true);
                CaptureButton.SetActive(false);

                BackButton.SetActive(false);
                break;
            case 3://Display or Print
                state = ScreenState.displyAndPrint;
                scrnShotHandler.webcamTexture.Stop();
                scrnShotHandler.SaveImage();
                //CollageWall_1.Add(scrnShotHandler.texture);
                
                RetakeButton.SetActive(false);
                ProceedButton.SetActive(false);
                CaptureButton.SetActive(true);

                Panel_2.SetActive(false);
                Panel_3.SetActive(true);
                break;
            case 4://Thank you
                state = ScreenState.thankYou;
                
                ScreenShotImage.gameObject.SetActive(false);
                Panel_3.SetActive(false);
                Panel_4.SetActive(true);
                break;
            default:

                break;
        }
    }

    public void InitiateRetakPicture()
    {
        StartCoroutine(RetakePicture());
    }

    private IEnumerator RetakePicture()
    {
        countDown = 3;
        Debug.Log("Inside the retake");
        yield return new WaitForSeconds(0.25f);
        //yield return new WaitForEndOfFrame();
        //scrnShotHandler.webcamTexture.Play();
        scrnShotHandler.camFeed.gameObject.SetActive(true);
        //ScreenShotImage.sprite = null;
        ScreenShotImage.gameObject.SetActive(false);

        RetakeButton.SetActive(false);
        ProceedButton.SetActive(false);
        CaptureButton.SetActive(true);

        BackButton.SetActive(true);

        StartTimer();
        Debug.Log("retake done");
    }

    private void InitiateDisplayOrPrint()
    {
        StartCoroutine(DisplayAndPrint_Wall_1());
    }

    private IEnumerator DisplayAndPrint_Wall_1()
    {
        yield return new WaitForSeconds(0.5f);

        int tempval = 0;

        if (CollageWall_1.Count > 10)
        {
            for (int i = CollageWall_1.Count; i > CollageWall_1.Count - 10; i--)
            {
                CollageWall_1_ImageList[tempval].GetComponent<RawImage>().texture = CollageWall_1[i - 1];
                tempval++;
            }
        }
        
        ScreenContoller(4);
    }

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

                HomeButton.SetActive(false);
                BackButton.SetActive(false);
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
                    Debug.LogWarning("Could not load image: " + filePath);
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

    private void DisplayImages()
    {

    }

    private void ResetGame()
    {
        countDown = 3;
    }
}
