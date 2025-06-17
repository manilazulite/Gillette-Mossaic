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

        for(int i = 0; i < corners.Length; i++)
        {
            Debug.Log(corners[i]);
        }
    }

    public void InitPrint()
    {
        StartCoroutine(ApplyTheCapturedPixelsToCustomResolution());
    }

    int i = 0;

    public IEnumerator ApplyTheCapturedPixelsToCustomResolution()
    {
        
        Debug.Log("calling the custom printing method : " + i++);
        // ‑‑‑ 1) Grab a frame of the screen ----------------------------------
        yield return new WaitForEndOfFrame();                 // wait for rendering

        //Texture2D screenTex = new Texture2D(Screen.width, Screen.height,
        //                                    TextureFormat.RGB24, false);

        Rect capturedrect = CamFeedParent.GetComponent<RectTransform>().rect;

        Texture2D screenTex = new Texture2D((int)capturedrect.width, (int)capturedrect.height,
                                            TextureFormat.RGB24, false);

        //screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //screenTex.ReadPixels(capturedrect, 0, 0);

        var temprect = new Rect(corners[0].x, corners[0].y, capturedrect.width, capturedrect.height);
        screenTex.ReadPixels(temprect, 0,0);
        screenTex.Apply();                                    // make data readable


        // ── 2. Scale to a fraction of A4 width, preserving aspect ratio ──────────
        //int targetWidth = Mathf.RoundToInt(A4_W * widthFractionOnA4);
        //int targetHeight = Mathf.RoundToInt((float)capturedrect.height / capturedrect.width * targetWidth);

        // Step 2: Resize to 55x55mm = ~637x637 pixels at 300 DPI
        int targetSizePx = Mathf.RoundToInt((IMAGE_MM / 25.4f) * DPI); // ≈ 637

        Texture2D scaled = ScaleTextureGPU(screenTex, targetSizePx, targetSizePx);
        //gameManager.ScreenShotImage.texture = scaled;

        // --------------------------------------------------------------------

        // ‑‑‑ 2) Create a blank “page” at A4 print resolution -----------------
        Texture2D a4 = new Texture2D(A4_W, A4_H, TextureFormat.RGB24, false);

        // optional – fill background white
        Color32[] bg = new Color32[A4_W * A4_H];
        for (int i = 0; i < bg.Length; i++) bg[i] = Color.white;
        a4.SetPixels32(bg);

        // top‑left: (0, A4_H‑scaled.height)
        a4.SetPixels32(0, A4_H - scaled.height, scaled.width, scaled.height, scaled.GetPixels32());
        a4.Apply();

        // ── 4. Save or use the A4 texture ───────────────────────────────────────
        //File.WriteAllBytes($"{Application.dataPath}/A4_Output.png", a4.EncodeToPNG());
        //Debug.Log("Saved A4_Output.png");
        // --------------------------------------------------------------------

        // ‑‑‑ 4) Save to disk -------------------------------------------------
        string imagename = "Gillette_Print" + DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss") + ".png";
        byte[] png = a4.EncodeToPNG();
        string path = Path.Combine(gameManager.PrintAssetPath, imagename);
        gameManager.LastSavedPrintImageName = path;
        File.WriteAllBytes(path, png);
        Debug.Log($"Saved A4 page with capture at: {path}");
        // --------------------------------------------------------------------

        // housekeeping
        Destroy(screenTex);
        Destroy(a4);

        //Print the Picture 

        gameManager.PrintImageCommand(gameManager.LastSavedPrintImageName);
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
