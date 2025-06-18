using UnityEngine;
using System.Collections;
using System.IO;
//using UnityEditor.Overlays;
using UnityEngine.UI;
using System;

public class ScreenShotHandler : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private Camera TargetCamera;

    public RectTransform TargetRect; // Assign the UI element which you wanna capture

    [SerializeField] private int width, height;

    private Rect captureArea;

    public RawImage camFeed;
    public RawImage NewCamFeed;

    public GameObject CameraFeedParent;

    public RenderTexture camRenderTexture;
    [HideInInspector]
    public WebCamTexture webcamTexture;
    [SerializeField] private string cameraName = "";

    public Texture2D texture;
    Texture2D screenTex;

    Vector3[] corners = new Vector3[4];

    public RobustVideoMattingSampleAPI sampleAPI;

    public Camera myMainCamera;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //startCamera();
        GetImageCorners();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartCoroutine(CaptureScreenshot());
        //}
    }

    public void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log(devices[0].name);

        if (devices.Length > 0)
        {
            webcamTexture = new WebCamTexture(devices[0].name, 3840, 2160, 60);
            //webcamTexture = new WebCamTexture(devices[0].name, 1080, 1080, 60);

            //camFeed.texture = webcamTexture;
            NewCamFeed.texture = webcamTexture;
            webcamTexture.Play();

            //StartCoroutine(ResizeAfterInit());
        }
        sampleAPI.Init();
    }

    private void GetImageCorners()
    {
        gameManager.CamFeedParent.GetWorldCorners(corners);

        for (int i = 0; i < corners.Length; i++)
        {
            Debug.Log(corners[i]);
        }
    }

    public void InitiateCapture()
    {
        //StartCoroutine(CaptureScreenshot());
        StartCoroutine(StartCaptureScreen());
    }

    private IEnumerator CaptureScreenshot()
    {
        //webcamTexture.Pause();
        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        TargetRect.GetWorldCorners(corners);

        
        if(TargetRect.transform.eulerAngles.z == 90)
        {
            captureArea = new Rect(corners[1].x, corners[1].y, TargetRect.sizeDelta.y, TargetRect.sizeDelta.x);
        }
        else
        {
            captureArea = new Rect(corners[1].x, corners[1].y, TargetRect.sizeDelta.y, TargetRect.sizeDelta.x);
        }

        //texture = new Texture2D((int)captureArea.width, (int)captureArea.height, TextureFormat.RGB24, false);
        //texture.ReadPixels(captureArea, 0, 0);        
        //texture.Apply();

        texture = new Texture2D((int)webcamTexture.width, (int)webcamTexture.height, TextureFormat.RGB24, false);
        texture.SetPixels(webcamTexture.GetPixels());
        texture.Apply();

        //string imagename = "Gillette_" + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss") + ".png";



        //// Optionally save it to a file
        byte[] bytes = texture.EncodeToPNG();
        gameManager.ScreenShotImage.texture = texture;

        camFeed.gameObject.SetActive(false);
        gameManager.ScreenShotImage.gameObject.SetActive(true);

        gameManager.ScreenShotImage.transform.rotation = Quaternion.Euler(0, 0, 90);
       
        gameManager.ScreenContoller(2);
    }

    private IEnumerator StartCaptureScreen()
    {
        yield return new WaitForEndOfFrame();

        Rect capturedrect = gameManager.CamFeedParent.GetComponent<RectTransform>().rect;
        screenTex = CaptureRegionFromCamera();
        /*screenTex = new Texture2D((int)capturedrect.width, (int)capturedrect.height,TextureFormat.RGB24, false);
        Debug.Log("Width = " + capturedrect.width + "Height = " + capturedrect.height + "Start Y = " + (3840-900-1400));
        var temprect = new Rect(360, 3840-900-1440, 1440, 1440);
        screenTex.ReadPixels(temprect, 0, 0);
        screenTex.Apply();*/

        byte[] bytes = screenTex.EncodeToPNG();
        //texture = new Texture2D(screenTex.width, screenTex.height, TextureFormat.RGB24, false);
        gameManager.ScreenShotImage.texture = screenTex;

        CameraFeedParent.SetActive(false);
        camFeed.gameObject.SetActive(false);
        gameManager.ScreenShotImage.gameObject.SetActive(true);

        //gameManager.ScreenShotImage.transform.rotation = Quaternion.Euler(0, 0, 90);

        gameManager.ScreenContoller(2);


    }

    public Texture2D CaptureRegionFromCamera()
    {
        // Setup temporary RenderTexture with screen size or desired resolution
        RenderTexture tempRT = new RenderTexture(Screen.width, Screen.height, 24);

        // Backup original
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture prevCamRT = myMainCamera.targetTexture;

        myMainCamera.targetTexture = tempRT;
        myMainCamera.Render();

        RenderTexture.active = tempRT;

        // Create texture with region size
        Texture2D result = new Texture2D(1440, 1440, TextureFormat.RGB24, false);
        var temprect = new Rect(360, 3840 - 900 - 1440, 1440, 1440);
        result.ReadPixels(temprect, 0, 0);
        result.Apply();

        // Cleanup
        myMainCamera.targetTexture = prevCamRT;
        RenderTexture.active = prevRT;
        tempRT.Release();
        UnityEngine.Object.Destroy(tempRT);

        return result;
    }

    public void SaveImage(bool isDisplay)
    {
        string imagename = "Gillette_" + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss") + ".png";

        //byte[] bytes = texture.EncodeToPNG();
        byte[] bytes = screenTex.EncodeToPNG();

        gameManager.LastSavedImageName = imagename;

        //FileIOUtility
        //        .SaveImage(texture, gameManager.AssetPath,
        //        imagename,
        //        FileIOUtility.FileExtension.PNG);

        if (isDisplay)
        {
            FileIOUtility
                .SaveImage(screenTex, gameManager.AssetPath,
                imagename,
                FileIOUtility.FileExtension.PNG);
        } else
        {
            FileIOUtility
                .SaveImage(screenTex, gameManager.NonDisplayPath,
                imagename,
                FileIOUtility.FileExtension.PNG);
        }


            CameraFeedParent.SetActive(false);
        camFeed.gameObject.SetActive(false);
        gameManager.ScreenShotImage.gameObject.SetActive(true);
        LoadTheLastSavedImage(isDisplay);
    }
    
    private void LoadTheLastSavedImage(bool isDisplay)
    {
        if (isDisplay)
        {
            gameManager.lastSnappedPicturePath = Path.Combine(gameManager.AssetPath, gameManager.LastSavedImageName);
        } else
        {
            gameManager.lastSnappedPicturePath = Path.Combine(gameManager.NonDisplayPath, gameManager.LastSavedImageName);
        }
        byte[] fileData = File.ReadAllBytes(gameManager.lastSnappedPicturePath);
        Texture2D tex = new Texture2D(2, 2); // Temp size
        tex.LoadImage(fileData);

        if (isDisplay)
        {
            gameManager.CollageWall_1.Add(tex);
        }
        gameManager.uploadImageToServerAsync(isDisplay);
    }

    private void CaptureScreenInSquare()
    {

    }

}
