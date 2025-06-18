using NatML;
using NatML.Vision;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

//BRIO 4K Stream Edition
public class RobustVideoMattingSampleAPI : MonoBehaviour
{
    [Header(@"NatML")]
    public string accessKey;

    [Header(@"UI")]
    public RawImage maskImage;

    public RawImage originalImage;

    public AspectRatioFitter aspectFitter;

    //WebCamTexture webCamTexture;

    RenderTexture segmentationImage;

    MLModel model;

    RobustVideoMattingPredictor predictor;

    public Material BGmat;

    public Texture2D BGImage;
    public ScreenShotHandler camFeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    public async void Init()
    {
        Debug.Log("Fetching model data from NatML...");

        // Fetch model data from NatML
        var modelData =
            await MLModelData
                .FromHub("@natsuite/robust-video-matting", accessKey);

        // Deserialize the model
        model = modelData.Deserialize();

        // Create the predictor

        predictor = new RobustVideoMattingPredictor(model);

        // Create and display the destination segmentation image
        while (camFeed.webcamTexture.width == 16 || camFeed.webcamTexture.height == 16)
            await Task.Yield();
        segmentationImage =
            new RenderTexture(camFeed.webcamTexture.width, camFeed.webcamTexture.height, 0);
        maskImage.texture = segmentationImage;
        originalImage.texture = camFeed.webcamTexture;
        /*if (aspectFitter)
        {
            aspectFitter.aspectRatio =
                (float)1 / 1;
        }*/

        SetBGMaterial();
    }

    private void SetBGMaterial()
    {
        //BGmat.SetTexture("_Background", BGImage);
        BGmat.SetTexture("_CameraFeed", camFeed.webcamTexture);
        BGmat.SetTexture("_Mask", segmentationImage);
        BGmat.SetFloat("_Threshold", 0.1f);
    }

    void Update()
    {
        if (camFeed.webcamTexture != null)
        {
            if (camFeed.webcamTexture.isPlaying)
            {
                // Check that the segmentation image has been created
                if (!segmentationImage) return;

                // Check that the camera frame updated
                if (!camFeed.webcamTexture.didUpdateThisFrame) return;

                // Predict
                var matte = predictor.Predict(camFeed.webcamTexture);
                matte.Render(segmentationImage);
            }

        }

    }

    void OnDisable()
    {
        // Dispose the predictor
        predictor?.Dispose();

        // Dispose the model
        model?.Dispose();
    }
}
