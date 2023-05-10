using UnityEngine;
using System.Linq;
using UI = UnityEngine.UI;
using Klak.TestTools;
using AprilTag;
using System;

sealed class DetectionTest : MonoBehaviour
{
    [SerializeField] GameObject[] go;
    private GameObject newgo;
    private int active;
    [SerializeField] ImageSource _source = null;
    [SerializeField] int _decimation = 4;
    [SerializeField] float _tagSize = 0.05f;
    [SerializeField] Material _tagMaterial = null;
    [SerializeField] UI.RawImage _webcamPreview = null;
    [SerializeField] UI.Text _debugText = null;
    private WebCamTexture webcam;
    private Texture texture;
    private RenderTexture renderTexture;
    private float fps;
    private float time = 0f;
    private int frames = 0;
    private float refresh = 1f;

    AprilTag.TagDetector _detector;

    void Awake()
    {
        active = 0;
        newgo = Instantiate(go[active], Vector3.zero, Quaternion.identity);
        newgo.SetActive(false);
    }

    void Start()
    {
        var dims = _source.OutputResolution;
        //webcam = new WebCamTexture();
        //_webcamPreview.texture = webcam;
        //webcam.requestedFPS = 60;
        //webcam.requestedHeight = 1080;
        //webcam.requestedWidth = 1920;
        //webcam.Play();
        _detector = new AprilTag.TagDetector(dims.x, dims.y, _decimation);
        _webcamPreview.texture = _source.Texture;
    }

    void OnDestroy()
    {
        _detector.Dispose();
    }

    void LateUpdate()
    {
        //texture = webcam;
        //var image = texture.AsSpan();
        // Source image acquisition
        var image = _source.Texture.AsSpan();
        if (image.IsEmpty) return;

        // AprilTag detection
        var fov = Camera.main.fieldOfView * Mathf.Deg2Rad;
        _detector.ProcessImage(image, fov, _tagSize);

        // Detected tag visualization
        foreach (var tag in _detector._detectedTags)
        {
            if(tag.ID == 0)
            {
                newgo.SetActive(true);
                newgo.transform.position = tag.Position;
                newgo.transform.rotation = tag.Rotation;
            }
            else
            {
                newgo.SetActive(false);
            }
        }
        if (_detector._detectedTags.Count == 0)
            newgo.SetActive(false);
        
        /*if (Time.frameCount % 30 == 0)
        {
            fps = 30f / time;
            _debugText.text = fps.ToString();
            time = 0f;
        }
        else
        {
            time = time + Time.deltaTime;
        }*/
        if(time < refresh)
        {
            frames++;
            time += Time.deltaTime;
        }
        else
        {
            fps = (float)frames / time;
            frames = 0;
            time = 0f;
            _debugText.text = fps.ToString();
        }
    }

    

    public void OnClick()
    {
        active = (active + 1) % 2;
        newgo.SetActive(false);
        newgo = Instantiate(go[active], Vector3.zero, Quaternion.identity);
        newgo.SetActive(false);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
