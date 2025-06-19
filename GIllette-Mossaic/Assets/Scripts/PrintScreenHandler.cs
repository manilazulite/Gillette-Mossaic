using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using System;

public class PrintScreenHandler : MonoBehaviour
{
    public static PrintScreenHandler instance;
    private GameManager gameManager;

    const int DPI = 300;                    // print quality
    //const int DPI = 182;                    // print quality
    const int A4_W = (int)(210f / 25.4f * DPI);   // ≈ 2480 px
    const int A4_H = (int)(297f / 25.4f * DPI);   // ≈ 3508 px
    //const int squarePx = (int)(297f / 25.4f * DPI);   // ≈ 3508 px
    const int IMAGE_MM = 56;

    public float widthFractionOnA4 = 0.25f;


    public RawImage CamFeed;
    public RectTransform CamFeedParent;

    Vector3[] corners = new Vector3[4];

    private void Start()
    {
        instance = this;
        gameManager = FindFirstObjectByType<GameManager>();
        GetImageCorners();
    }

    private void GetImageCorners()
    {        
        CamFeedParent.GetWorldCorners(corners);

        //for(int i = 0; i < corners.Length; i++)
        //{
        //    Debug.Log(corners[i]);
        //}
    }

    public void InitPrint()
    {
        StartCoroutine(ApplyTheCapturedPixelsToCustomResolution());
    }

    int i = 0;

    public Texture2D screenTex;
    public Texture2D scaled;

    public IEnumerator ApplyTheCapturedPixelsToCustomResolution()
    {        
        Debug.Log("calling the custom printing method : " + i++);

        // ‑‑‑ 1) Grab a frame of the screen ----------------------------------
        yield return new WaitForEndOfFrame();                 // wait for rendering     

        // Step 2: Resize to 55x55mm = ~637x637 pixels at 300 DPI
        int targetSizePx = Mathf.RoundToInt((IMAGE_MM / 25.4f) * DPI); // ≈ 637
        
        scaled = ScaleTextureGPU(screenTex, targetSizePx, targetSizePx);
       
        PortrayTheCapturedImageInA4();
    }

    public void PortrayTheCapturedImageInA4()
    {
        Texture2D a4 = new Texture2D(A4_W, A4_H, TextureFormat.RGB24, false);

        // optional – fill background white
        Color32[] bg = new Color32[A4_W * A4_H];
        for (int i = 0; i < bg.Length; i++) bg[i] = Color.white;
        a4.SetPixels32(bg);

        // top‑left: (0, A4_H‑scaled.height)
        a4.SetPixels32(0, A4_H - scaled.height, scaled.width, scaled.height, scaled.GetPixels32());
        a4.Apply();

        // ‑‑‑ 4) Save to disk -------------------------------------------------
        string imagename = "Gillette_Print" + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss") + ".png";
        byte[] png = a4.EncodeToPNG();
        string path = Path.Combine(gameManager.PrintAssetPath, imagename);
        gameManager.LastSavedPrintImageName = path;
        File.WriteAllBytes(path, png);
        Debug.Log($"Saved A4 page with capture at: {path}");

        // --------------------------------------------------------------------
        
        //Print the Picture 
        StartCoroutine(gameManager.generateQRCodeOnQRBtn());

        //Destroy(screenTex);
        Destroy(a4);
        //gameManager.PrintImageCommand(gameManager.LastSavedPrintImageName);
    }

    /// Scales a texture via GPU blit (fast, keeps quality)
    Texture2D ScaleTextureGPU(Texture2D src, int w, int h)
    {
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D dst = new Texture2D(w, h, TextureFormat.RGB24, false);
        dst.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        dst.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }
}
