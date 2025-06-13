using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor.Overlays;
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
    public RenderTexture camRenderTexture;
    [HideInInspector]
    public WebCamTexture webcamTexture;
    [SerializeField] private string cameraName = "";

    public Texture2D texture;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //startCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(CaptureScreenshot());
        }
    }

    public void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log(devices[0].name);

        for (var i = 0; i < devices.Length; i++)
        {
            if (devices[i].name == cameraName)
            {
                webcamTexture = new WebCamTexture(cameraName, 3840, 2160, 60);

                webcamTexture.Play();
                camFeed.texture = webcamTexture;
            }
        }
    }

    public void InitiateCapture()
    {
        StartCoroutine(CaptureScreenshot());
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

    public void SaveImage()
    {
        string imagename = "Gillette_" + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss") + ".png";

                
        // Optionally save it to a file
        byte[] bytes = texture.EncodeToPNG();

        //string path = Path.Combine(Application.dataPath, imagename);
        gameManager.LastSavedImageName = imagename;
        //File.WriteAllBytes(path, bytes);

        FileIOUtility
                .SaveImage(texture, gameManager.AssetPath,
                imagename,
                FileIOUtility.FileExtension.PNG);

        camFeed.gameObject.SetActive(false);
        gameManager.ScreenShotImage.gameObject.SetActive(true);

        LoadTheLastSavedImage();
    }

    //string lastSnappedPicturePath = string.Empty;
    private void LoadTheLastSavedImage()
    {
        gameManager.lastSnappedPicturePath = Path.Combine(gameManager.AssetPath, gameManager.LastSavedImageName);
        byte[] fileData = File.ReadAllBytes(gameManager.lastSnappedPicturePath);
        Texture2D tex = new Texture2D(2, 2); // Temp size
        tex.LoadImage(fileData);

        gameManager.CollageWall_1.Add(tex);
    }

    

    private void DisplayTheScreenShot()
    {

    }
}
