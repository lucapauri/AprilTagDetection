using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Klak.TestTools;
using AprilTag;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System;

public class ARDetection : MonoBehaviour
{
    [SerializeField] GameObject go;
    private GameObject newgo;
    [SerializeField] int _decimation = 4;
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] ARSession session;
    [SerializeField] ARSessionOrigin sessionOrigin;
    [SerializeField] ARCameraManager cameraManager;
    [SerializeField] Camera camer;
    private Texture2D cameraImageTexture;


    AprilTag.TagDetector _detector = null;

    void Awake()
    {
        newgo = Instantiate(go, Vector3.zero, Quaternion.identity);
        newgo.SetActive(false);
    }

    private void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnDestroy()
    {
        _detector.Dispose();
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        //cameraImageTexture.LoadRawTextureData(buffer);
        //cameraImageTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        // Detect and decode the barcode inside the bitmap
        if (_detector == null)
        {
            var dims = conversionParams.outputDimensions;
            _detector = new AprilTag.TagDetector(dims.x, dims.y, _decimation);
        }

        // AprilTag detection
        var fov = camer.fieldOfView * Mathf.Deg2Rad;
        ReadOnlySpan<Color32> imag = new ReadOnlySpan<Color32>(cameraImageTexture.GetPixels32());
        _detector.ProcessImage(imag, fov, _tagSize);

        // Detected tag visualization
        foreach (var tag in _detector._detectedTags)
        {
            if (tag.ID == 0)
            {
                newgo.SetActive(true);
                newgo.transform.position = tag.Position;
                newgo.transform.rotation = tag.Rotation;
            }
            else
            {
                newgo.SetActive(false);
            }
            //_drawer.Draw(tag.ID, tag.Position, tag.Rotation, _tagSize);
        }
        if (_detector._detectedTags.Count == 0)
            newgo.SetActive(false);
    }
}
