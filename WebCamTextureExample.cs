using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine.UI;
using System.Threading;
using System.Linq;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using DlibFaceLandmarkDetector;

namespace DlibFaceLandmarkDetectorExample
{
    /// <summary>
    /// WebCamTexture Example
    /// An example of detecting face landmarks in WebCamTexture images.
    /// </summary>
    public class WebCamTextureExample : MonoBehaviour
    {
        /// <summary>
        /// Set the name of the device to use.
        /// </summary>
        [SerializeField, TooltipAttribute ("Set the name of the device to use.")]
        public string requestedDeviceName = null;

        /// <summary>
        /// Set the width of WebCamTexture.
        /// </summary>
        [SerializeField, TooltipAttribute ("Set the width of WebCamTexture.")]
        public int requestedWidth = 320;

        /// <summary>
        /// Set the height of WebCamTexture.
        /// </summary>
        [SerializeField, TooltipAttribute ("Set the height of WebCamTexture.")]
        public int requestedHeight = 240;

        /// <summary>
        /// Set FPS of WebCamTexture.
        /// </summary>
        [SerializeField, TooltipAttribute ("Set FPS of WebCamTexture.")]
        public int requestedFPS = 30;

        /// <summary>
        /// Set whether to use the front facing camera.
        /// </summary>
        [SerializeField, TooltipAttribute ("Set whether to use the front facing camera.")]
        public bool requestedIsFrontFacing = true;

        /// <summary>
        /// The adjust pixels direction toggle.
        /// </summary>
        public Toggle adjustPixelsDirectionToggle;

        /// <summary>
        /// Determines if adjust pixels direction.
        /// </summary>
        [SerializeField, TooltipAttribute ("Determines if adjust pixels direction.")]
        public bool adjustPixelsDirection = true;

        /// <summary>
        /// The webcam texture.
        /// </summary>
        WebCamTexture webCamTexture;

        /// <summary>
        /// The webcam device.
        /// </summary>
        WebCamDevice webCamDevice;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;
        /// <summary>
        /// The rotated colors.
        /// </summary>
        Color32[] rotatedColors;

        /// <summary>
        /// Determines if rotates 90 degree.
        /// </summary>
        bool rotate90Degree = false;

        /// <summary>
        /// Indicates whether this instance is waiting for initialization to complete.
        /// </summary>
        bool isInitWaiting = false;

        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        bool hasInitDone = false;

        /// <summary>
        /// The screenOrientation.
        /// </summary>
        ScreenOrientation screenOrientation;

        /// <summary>
        /// The width of the screen.
        /// </summary>
        int screenWidth;

        /// <summary>
        /// The height of the screen.
        /// </summary>
        int screenHeight;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// The dlib shape predictor file name.
        /// </summary>
        string dlibShapePredictorFileName = "sp_human_face_68_for_mobile.dat";

        /// <summary>
        /// The dlib shape predictor file path.
        /// </summary>
        string dlibShapePredictorFilePath;

        #if UNITY_WEBGL && !UNITY_EDITOR
        Stack<IEnumerator> coroutines = new Stack<IEnumerator> ();
        #endif

                /// <summary>
        /// In Debug Mode
        /// </summary>
        bool debugMode;

        /// <summary>
        /// Face points
        /// </summary>
        List<Vector2> points;

        /// <summary>
        /// Current Face Direction
        /// </summary>
        int curVerDir;
        int curHorDir;

        /// <summary>
        /// Just the face
        /// </summary>
        Texture face=null;

        /// <summary>
        /// Left eye Texture
        /// </summary>
        Texture leftEyeImg=null;

        /// <summary>
        /// Right eye Texture
        /// </summary>
        Texture rightEyeImg=null;

        /// <summary>
        /// Left eye Texture to match to
        /// </summary>
        Texture prevLeftEyeImg=null;

        /// <summary>
        /// Right eye Texture to match to
        /// </summary>
        Texture prevRightEyeImg=null;

        /// <summary>
        /// Some integer
        /// </summary>
        int i=0;

        bool headShakeMonitor=false;
        int headShakeMonitorFrames=0;
        List<int> headShakeDirs = new List<int>();

        bool headNodMonitor=false;
        int headNodMonitorFrames=0;
        List<int> headNodDirs = new List<int>();

        List<int> headVerDirs = new List<int>();
        List<int> headHorDirs = new List<int>();

        float faceHeight = 0;

        public UnityEngine.UI.Button faceDirHorButton;
        public UnityEngine.UI.Text faceDirHorText;

        public UnityEngine.UI.Button headShakeButton;
        public UnityEngine.UI.Text headShakeText;

        public UnityEngine.UI.Button headNodButton;
        public UnityEngine.UI.Text headNodText;

        public UnityEngine.UI.Text leftPointsMatchedText;

        public UnityEngine.UI.Text rightPointsMatchedText;

        public UnityEngine.UI.Button faceDirVerButton;
        public UnityEngine.UI.Text faceDirVerText;

        public UnityEngine.UI.Button mouthButton;
        public UnityEngine.UI.Text mouthText;

        public UnityEngine.UI.Button smileButton;
        public UnityEngine.UI.Text smileText;

        public UnityEngine.UI.Text eyesText;

        public UnityEngine.UI.Text dist1;
        public UnityEngine.UI.Text dist2;
        public UnityEngine.UI.Text dist3;
        public UnityEngine.UI.Text dist4;
        public UnityEngine.UI.Text dist5;

        public UnityEngine.UI.Text emotion1;
        public UnityEngine.UI.Text emotion2;
        public UnityEngine.UI.Text emotion3;
        public UnityEngine.UI.Text emotion4;

        public UnityEngine.UI.Button recab;
        public UnityEngine.UI.Text recabText;

        public UnityEngine.UI.Button colorOfObject;
        public UnityEngine.UI.Text colorText;
        float initNoseDist;

        bool firstFrame = true;

        int number_of_prev_points=0;

        List<Vector2> normal_points;

        Thread pointsthread;

        public bool pointsthread_running = false;

        public int period_between_cv = 20;

        static readonly object copyablepointslocker = new object();

        List<Vector2> copyablepoints;

        float copyableHeight;

        float copyableWidth;

        Rect copyableRect;

        public float rectHeight;

        public float rectWidth;

        public Rect rect;

        bool updatedpoints = false;

        public List<float> rgbColors;

        int color;


        float scale_area=1f;
        float scale_height=1f;
        float scale_width=1f;
        float trained_height=266.0f;
        float trained_width=265.94f;


        // Microsoft.Scripting.Hosting.ScriptEngine Engine = null;

        // Microsoft.Scripting.Hosting.ScriptScope ScriptScope = null;

        // Microsoft.Scripting.Hosting.ScriptSource ScriptSource = null;

        // [DllImport("model")]
        // public static extern int predict (double[] n);

        #if UNITY_EDITOR
            [DllImport("model_desktop")]
        #else
            [DllImport("model")]
        #endif
        private static extern int predict (double[] n);


        int n_e = 0;
        bool doEmotion = false;
        List<int> emotions = new List<int>();

        // Use this for initialization
        public void Start ()
        {

            faceDirHorButton.onClick.AddListener(faceDirHor);
            faceDirVerButton.onClick.AddListener(faceDirVer);
            headShakeButton.onClick.AddListener(setHeadShakeMonitor);
            headNodButton.onClick.AddListener(setHeadNodMonitor);
            mouthButton.onClick.AddListener(mouthDetect);
            // smileButton.onClick.AddListener(smileDetect);
            recab.onClick.AddListener(recab_points);
            colorOfObject.onClick.AddListener(color_of_object);

            smileButton.onClick.AddListener(emotionDetect);

            fpsMonitor = GetComponent<FpsMonitor> ();

            adjustPixelsDirectionToggle.isOn = true;

            dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName;
            #if UNITY_WEBGL && !UNITY_EDITOR
            var getFilePath_Coroutine = Utils.getFilePathAsync (dlibShapePredictorFileName, (result) => {
                coroutines.Clear ();

                dlibShapePredictorFilePath = result;
                Run ();
            });
            coroutines.Push (getFilePath_Coroutine);
            StartCoroutine (getFilePath_Coroutine);
            #else
            dlibShapePredictorFilePath = Utils.getFilePath (dlibShapePredictorFileName);
            Run ();
            #endif
        }

        private void Run ()
        {
            faceLandmarkDetector = new FaceLandmarkDetector (dlibShapePredictorFilePath);

            Initialize ();
        }

        /// <summary>
        /// Initializes webcam texture.
        /// </summary>
        private void Initialize ()
        {
            if (isInitWaiting)
                return;
            pointsthread = new Thread(PointsThread);
            pointsthread.Start();
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, pixel 2)
            // https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            if (requestedIsFrontFacing) {
                int rearCameraFPS = requestedFPS;
                requestedFPS = 15;
                StartCoroutine (_Initialize ());
                requestedFPS = rearCameraFPS;
            } else {
                StartCoroutine (_Initialize ());
            }
            #else
            StartCoroutine (_Initialize ());
            #endif
        }

        /// <summary>
        /// Initializes webcam texture by coroutine.
        /// </summary>
        private IEnumerator _Initialize ()
        {
            if (hasInitDone)
                Dispose ();

            isInitWaiting = true;

            // Creates the camera
            if (!String.IsNullOrEmpty (requestedDeviceName)) {
                int requestedDeviceIndex = 1;
                if (Int32.TryParse (requestedDeviceName, out requestedDeviceIndex)) {
                    if (requestedDeviceIndex >= 0 && requestedDeviceIndex < WebCamTexture.devices.Length) {
                        webCamDevice = WebCamTexture.devices [requestedDeviceIndex];
                        webCamTexture = new WebCamTexture (webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                    }
                }
                else {
                    for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
                        if (WebCamTexture.devices [cameraIndex].name == requestedDeviceName) {
                            webCamDevice = WebCamTexture.devices [cameraIndex];
                            webCamTexture = new WebCamTexture (webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                            break;
                        }
                    }
                }
                if (webCamTexture == null)
                    UnityEngine.Debug.Log ("Cannot find camera device " + requestedDeviceName + ".");
            }

            if (webCamTexture == null) {
                // Checks how many and which cameras are available on the device
                #if UNITY_ANDROID && !UNITY_EDITOR
                for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
                    if (WebCamTexture.devices [cameraIndex].isFrontFacing == true) {
                        webCamDevice = WebCamTexture.devices [cameraIndex];
                        webCamTexture = new WebCamTexture (webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                        break;
                    }
                }
                #else
                for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
                    if (WebCamTexture.devices [cameraIndex].isFrontFacing == true) {
                        webCamDevice = WebCamTexture.devices [cameraIndex];
                        webCamTexture = new WebCamTexture (webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                        break;
                    }
                }
                #endif
            }

            if (webCamTexture == null) {
                if (WebCamTexture.devices.Length > 0) {
                    webCamDevice = WebCamTexture.devices [1];
                    webCamTexture = new WebCamTexture (webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                } else {
                    UnityEngine.Debug.LogError ("Camera device does not exist.");
                    isInitWaiting = false;
                    yield break;
                }
            }


            // Starts the camera
            webCamTexture.Play ();

            while (true) {
                //If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
                #if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
                if (webCamTexture.width > 16 && webCamTexture.height > 16) {
                #else
                if (webCamTexture.didUpdateThisFrame) {
                    #if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2
                    while (webCamTexture.width <= 16) {
                        webCamTexture.GetPixels32 ();
                        yield return new WaitForEndOfFrame ();
                    }
                    #endif
                #endif

                    UnityEngine.Debug.Log ("name:" + webCamTexture.deviceName + " width:" + webCamTexture.width + " height:" + webCamTexture.height + " fps:" + webCamTexture.requestedFPS);
                    UnityEngine.Debug.Log ("videoRotationAngle:" + webCamTexture.videoRotationAngle + " videoVerticallyMirrored:" + webCamTexture.videoVerticallyMirrored + " isFrongFacing:" + webCamDevice.isFrontFacing);

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;
                    isInitWaiting = false;
                    hasInitDone = true;

                    OnInited ();

                    break;
                } else {
                    yield return 0;
                }
            }
        }

        /// <summary>
        /// Releases all resource.
        /// </summary>
        private void Dispose ()
        {
            rotate90Degree = false;
            isInitWaiting = false;
            hasInitDone = false;
            if (pointsthread_running)
            {
                // This forces the while loop in the ThreadedWork function to abort.
                pointsthread_running = false;

                // This waits until the thread exits,
                // ensuring any cleanup we do after this is safe.
                pointsthread.Join();
            }
            if (webCamTexture != null) {
                webCamTexture.Stop ();
                WebCamTexture.Destroy (webCamTexture);
                webCamTexture = null;
            }
            if (texture != null) {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        /// <summary>
        /// Raises the webcam texture initialized event.
        /// </summary>
        private void OnInited ()
        {
            if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height) {
                colors = new Color32[webCamTexture.width * webCamTexture.height];
                rotatedColors = new Color32[webCamTexture.width * webCamTexture.height];
            }

            if (true) {
                #if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
                    rotate90Degree = true;
                }else{
                    rotate90Degree = false;
                }
                #endif
            }
            if (rotate90Degree) {
                texture = new Texture2D (webCamTexture.height, webCamTexture.width, TextureFormat.RGB24, false);
            } else {
                texture = new Texture2D (webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            }

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            if (texture==null) {
                UnityEngine.Debug.Log("NULL");
            }


            gameObject.transform.localScale = new Vector3 (texture.width, texture.height, 1);
            UnityEngine.Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null){
                fpsMonitor.Add ("dlib shape predictor", dlibShapePredictorFileName);
                fpsMonitor.Add ("width", texture.width.ToString());
                fpsMonitor.Add ("height", texture.height.ToString());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString());
            }


            float width = texture.width;
            float height = texture.height;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
        }

        double[] getLandmarks(List<Vector2> p, Rect r)
        {
            scale_height = trained_height/r.height;
            scale_width = trained_width/r.width;
            // dist3.text = scale.ToString();
            double[] landmarks = new double[272];
            double[] xlist = new double[68];
            double[] ylist = new double[68];
            double[] xcentral = new double[68];
            double[] ycentral = new double[68];
            int i = 0;
            foreach(Vector2 v in p) {
                xlist[i] = (double)v.x*scale_width;
                ylist[i] = (double)v.y*scale_height;
                i++;
            }
            double xmean = xlist.Average();
            double ymean = ylist.Average();

            xcentral = xlist.Select(x => x-xmean).ToArray();
            ycentral = ylist.Select(y => y-ymean).ToArray();

            i = 0;
            for (int j=0; j<68; j++) {
                landmarks[i] = xcentral[j];
                i++;
                landmarks[i] = ycentral[j];
                i++;
                Vector2 mean = new Vector2((float)ymean,(float)xmean);
                Vector2 coor = new Vector2((float)ylist[j],(float)xlist[j]);
                landmarks[i] = Vector2.Distance(coor,mean);
                i++;
                landmarks[i] = ((Math.Atan2(ycentral[j], xcentral[j])*360)/(2*Math.PI));
                i++;
            }
            return landmarks;
        }

        double[] getLandmarks_desktop(List<Vector2> p, Rect r)
        {
            double[] landmarks = new double[273];
            double[] xlist = new double[68];
            double[] ylist = new double[68];
            double[] xcentral = new double[68];
            double[] ycentral = new double[68];
            int i = 0;
            foreach(Vector2 v in p) {
                xlist[i] = (double)v.x;
                ylist[i] = (double)v.y;
                i++;
            }
            double xmean = xlist.Average();
            double ymean = ylist.Average();

            xcentral = xlist.Select(x => x-xmean).ToArray();
            ycentral = ylist.Select(y => y-ymean).ToArray();

            i = 0;
            for (int j=0; j<68; j++) {
                landmarks[i] = xlist[j];
                i++;
                landmarks[i] = ylist[j];
                i++;
                Vector2 mean = new Vector2((float)ymean,(float)xmean);
                Vector2 coor = new Vector2((float)ylist[j],(float)xlist[j]);
                landmarks[i] = Vector2.Distance(coor,mean);
                i++;
                landmarks[i] = ((Math.Atan2(ycentral[j], xcentral[j])*360)/(2*Math.PI));
                i++;
            }
            landmarks[i] = (Math.Abs(ylist[21] - ylist[7]));
            return landmarks;
        }

        void Update ()
        {
            if (true) {
                // Catch the orientation change of the screen.
                if (screenOrientation != Screen.orientation && (screenWidth != Screen.width || screenHeight != Screen.height)) {
                    Initialize ();
                } else {
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;
                }
            }

            if (hasInitDone && webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame) {

                float red = 0;
                float green = 0;
                float blue = 0;
                Color32[] colors = GetColors ();

                dist1.color = Color.red;
                dist2.color = Color.green;
                dist3.color = Color.blue;

                color = -1;

                if (colors != null) {
                    faceLandmarkDetector.SetImage<Color32> (colors, texture.width, texture.height, 4, true);

                    faceLandmarkDetector.DrawDetectResult<Color32> (colors, texture.width, texture.height, 4, true, 255, 0, 0, 255, 2);
                    texture.SetPixels32 (colors);
                    texture.Apply ();

                    if (updatedpoints && copyablepoints.Count>0)
                    {
                        lock (copyablepointslocker)
                        {
                            updatedpoints = false;
                            points = copyablepoints;
                            rectHeight = copyableHeight;
                            rectWidth = copyableWidth;
                            rect = copyableRect;
                            if (firstFrame) {
                                normal_points=points;
                                firstFrame=false;
                                number_of_prev_points=1;
                                rgbColors.Add(red);
                                rgbColors.Add(green);
                                rgbColors.Add(blue);
                                rgbColors.Add(0);
                                rgbColors.Add(0);
                                rgbColors.Add(0);
                                recabText.text = "calibrating...";
                            }
                            else if (number_of_prev_points<10) {
                                setNormal();
                                recabText.text = "calibrating...";
                            }
                            else {
                                recabText.text = "calibrated!";

                                faceDirHor();
                                faceDirVer();

                                if (headHorDirs.Count>15) {
                                    headHorDirs.RemoveAt(0);
                                    headHorDirs.Add(curHorDir);
                                }
                                else {
                                    headHorDirs.Add(curHorDir);
                                }
                                headShake2(headHorDirs);

                                if (headVerDirs.Count>15) {
                                    headVerDirs.RemoveAt(0);
                                    headVerDirs.Add(curVerDir);
                                }
                                else {
                                    headVerDirs.Add(curVerDir);
                                }
                                headNod2(headVerDirs);



                                mouthDetect();

                                float prev_eyesVal_left = quadArea(normal_points[37], normal_points[38], normal_points[40], normal_points[41]);
                                float cur_eyesVal_left = quadArea(points[37], points[38], points[40], points[41]);

                                float prev_eyesVal_right = quadArea(normal_points[43], normal_points[44], normal_points[46], normal_points[47]);
                                float cur_eyesVal_right = quadArea(points[43], points[44], points[46], points[47]);

                                float percDiff_left = (((prev_eyesVal_left-cur_eyesVal_left)/(prev_eyesVal_left))*100);
                                float percDiff_right = (((prev_eyesVal_right-cur_eyesVal_right)/(prev_eyesVal_right))*100);


                                if (percDiff_left > 40 && percDiff_right > 40) {
                                    eyesText.color = Color.green;
                                    eyesText.text = "Eyes Closed";
                                }
                                else if (percDiff_left > 40) {
                                    eyesText.color = Color.green;
                                    eyesText.text = "Left Wink";
                                }
                                else if (percDiff_right > 40) {
                                    eyesText.color = Color.green;
                                    eyesText.text = "Right Wink";
                                }
                                else {
                                    eyesText.color = Color.red;
                                    eyesText.text = "Eyes Open";
                                }

                                #if UNITY_EDITOR
                                    double[] f = getLandmarks_desktop(points, rect);
                                #else
                                    double[] f = getLandmarks(points, rect);
                                #endif

                                int emotion = predict(f);

                                if (emotion==0) {
                                    emotion1.color = Color.green;
                                    emotion2.color = Color.red;
                                    emotion3.color = Color.red;
                                    emotion4.color = Color.red;
                                }
                                else if (emotion==1) {
                                    emotion1.color = Color.red;;
                                    emotion2.color = Color.green;
                                    emotion3.color = Color.red;
                                    emotion4.color = Color.red;
                                }
                                else if (emotion==2) {
                                    emotion1.color = Color.red;
                                    emotion2.color = Color.red;
                                    emotion3.color = Color.green;
                                    emotion4.color = Color.red;
                                }
                                else if (emotion==3) {
                                    emotion1.color = Color.red;
                                    emotion2.color = Color.red;
                                    emotion3.color = Color.red;
                                    emotion4.color = Color.green;
                                }


                            }
                        }
                    }
                }
            }
            // faceLandmarkDetector.DrawDetectLandmarkResult<Color32> (colors, texture.width, texture.height, 4, true, 0, 255, 0, 255);
        }

        void PointsThread()
        {
            pointsthread_running = true;

            // This pattern lets us interrupt the work at a safe point if neeeded.
            while (pointsthread_running)
            {
                if (texture != null)
                {
                    //detect face rects
                    List<Rect> detectResult = faceLandmarkDetector.Detect ();
                    List<Vector2> localpoints;
                    float localHeight;
                    float localWidth;
                    Rect localRect;
                    if (detectResult.Count > 0)
                    {

                        Rect imgRect = detectResult[0];

                        localpoints = faceLandmarkDetector.DetectLandmark(imgRect);
                        localHeight = detectResult[0].height;
                        localWidth = detectResult[0].width;
                        localRect = detectResult[0];
                        lock (copyablepointslocker)
                        {
                            updatedpoints = true;
                            copyablepoints = localpoints;
                            copyableHeight = localHeight;
                            copyableWidth = localWidth;
                            copyableRect = localRect;
                        }

                    }
                    else {
                        lock (copyablepointslocker)
                        {
                            updatedpoints = true;
                            copyablepoints = new List<Vector2>();
                            copyableHeight = 0;
                            copyableWidth = 0;
                            copyableRect = new Rect();
                        }
                    }

                }
                // Prevent thread spinning
                Thread.Sleep(period_between_cv);
            }

            pointsthread_running = false;
        }

        void color_of_object() {
            if (color==-1) {
                colorText.text = "no new object";
                colorText.color = Color.white;
            }

            else if (color==0) {
                colorText.text = "Red";
                colorText.color = Color.red;
            }

            else if (color==1) {
                colorText.text = "Green";
                colorText.color = Color.green;
            }

            else if (color==2) {
                colorText.text = "Blue";
                colorText.color = Color.blue;
            }
        }

        void emotionDetect() {
            doEmotion = true;
        }

        void startEmotion() {
            #if UNITY_EDITOR
                double[] f = getLandmarks_desktop(points, rect);
            #else
                double[] f = getLandmarks(points, rect);
            #endif
            int emotion = predict(f);
            emotions.Add(emotion);
            if (n_e==20) {
                string commonEmotion = "";
                int mostOccuringEmotion = emotions.GroupBy(n=> n).OrderByDescending(g=> g.Count()).Select(g => g.Key).FirstOrDefault();
                if (mostOccuringEmotion==0) {
                    commonEmotion = "NEUTRAL";
                }
                else if (mostOccuringEmotion==1) {
                    commonEmotion = "HAPPY";
                }
                else if (mostOccuringEmotion==2) {
                    commonEmotion = "ANGRY";
                }
                else if (mostOccuringEmotion==3) {
                    commonEmotion = "SAD";
                }
                smileText.text =  commonEmotion;
                emotions.Clear();
                n_e=0;
                doEmotion = false;
            }
            n_e++;
        }


        //0=looking straight
        //1=looking left
        //2=looking right
        //3=looking up
        //4=looking down

        void recab_points() {
            firstFrame=true;
            normal_points.Clear();
            rgbColors.Clear();
            rectHeight=0;
            number_of_prev_points=0;
        }

        void setNormal() {
            for (int i = 0; i<68; i++) {
                normal_points[i] = new Vector2((normal_points[i].x*number_of_prev_points+points[i].x)/(number_of_prev_points+1),
                                                (normal_points[i].y*number_of_prev_points+points[i].y)/(number_of_prev_points+1));
            }
            number_of_prev_points++;
        }

        float quadArea(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            float x1 = p1.x;
            float y1 = p1.y;
            float x2 = p2.x;
            float y2 = p2.y;
            float x3 = p3.x;
            float y3 = p3.y;
            float x4 = p4.x;
            float y4 = p4.y;
            return ((Math.Abs(x1*y2+x2*y3+x3*y4+x4*y1-x2*y1-x3*y2-x4*y3-x1*y4))/2);
        }
        void rightLeftDetect() {

            float leftPointsDiff=Math.Abs(points[48].x - points[3].x);
            float rightPointsDiff=Math.Abs(points[13].x - points[54].x);
            float prev_left = Math.Abs(normal_points[48].x - normal_points[3].x);
            float prev_right = Math.Abs(normal_points[13].x - normal_points[54].x);
            float normal_ratio = prev_left/prev_right;
            float ratio = leftPointsDiff/rightPointsDiff;

            if (ratio>=(normal_ratio-(30*normal_ratio/100)) && ratio<=(normal_ratio+(30*normal_ratio/100))) {
                curHorDir = 0;
            }

            else {
                if (rightPointsDiff>leftPointsDiff) {
                    curHorDir = 1;
                }
                else if (rightPointsDiff<leftPointsDiff) {
                    curHorDir = 2;
                }
            }
        }

        void faceDirHor() {
            rightLeftDetect();
            faceDirHorText.color = Color.red;
            if (curHorDir==0) {
                faceDirHorText.text="Looking Straight";
            }
            else if (curHorDir==1){
                faceDirHorText.text="Looking Left";
            }
            else if (curHorDir==2){
                faceDirHorText.text="Looking Right";
            }
        }

        void upDownDetect() {
            // bool chin_up = (points[8].y <= (normal_points[8].y-8));
            // bool chin_down = (points[8].y >= (8+normal_points[8].y));

            float normal_upAngle=Vector2.Angle(normal_points[31]-normal_points[30], normal_points[30]-normal_points[35]);
            float normal_downAngle=Vector2.Angle(normal_points[31]-normal_points[33], normal_points[33]-normal_points[35]);
            float normal_noseTriangle = (normal_upAngle/normal_downAngle)*10;

            float upAngle=Vector2.Angle(points[31]-points[30], points[30]-points[35]);
            float downAngle=Vector2.Angle(points[31]-points[33], points[33]-points[35]);
            float noseTriangle = (upAngle/downAngle)*10;

            float normal_jaw = (Vector2.Distance(normal_points[0], normal_points[8])/rectHeight)*100;
            float curr_jaw = (Vector2.Distance(points[0], points[8])/rectHeight)*100;

            float lowBound = normal_jaw - (11*(normal_jaw)/100);
            float upBound = normal_jaw + (10*(normal_jaw)/100);

            bool is_eye_level = (normal_noseTriangle<=30);


            if (is_eye_level) {
                if (noseTriangle>30) {
                    curVerDir = 3;
                }
                else if (noseTriangle<10) {
                    curVerDir = 4;
                }
                else {
                    curVerDir = 0;
                }
            }
            else {
                if (curr_jaw>=lowBound && curr_jaw<=upBound) {
                    curVerDir = 0;
                }
                else if (curr_jaw<lowBound || noseTriangle>80) {
                    curVerDir = 3;
                }
                else {
                    curVerDir = 4;
                }
            }
        }

        void faceDirVer() {
            upDownDetect();
            faceDirVerText.color = Color.red;
            if (curVerDir==3) {
                faceDirVerText.text="Looking Up";
            }
            else if (curVerDir==4){
                faceDirVerText.text="Looking Down";
            }
            else if (curVerDir==0){
                faceDirVerText.text="Looking Straight";
            }
        }

        void headShake2(List<int> l) {
            int straight=0;
            int left=0;
            int right=0;
            foreach (int j in l) {
                if (j==0) {
                    straight++;
                }
                else if (j==1) {
                    left++;
                }
                else {
                    right++;
                }
            }
            if (left>=1 && right>=1) {
                headShakeText.text="Shaking";
            }
            else {
                headShakeText.text="Not shaking";
            }
        }


        void headNod2(List<int> l) {
            int straight=0;
            int up=0;
            int down=0;
            foreach (int j in l) {
                if (j==0) {
                    straight++;
                }
                else if (j==4) {
                    down++;
                }
                else {
                    up++;
                }
            }
            if (up>=1 && down>=1) {
                headNodText.text="Nodding";
            }
            else {
                headNodText.text="Not Nodding";
            }
        }

        void mouthDetect() {
            // float mouthDist=Math.Abs(points[62].y-points[66].y);
            mouthText.color = Color.red;

            float mouth_area = quadArea(points[51], points[53], points[55], points[57]);
            mouth_area = (mouth_area*100)/(rect.height*rect.width);

            if (mouth_area>1.5) {
                mouthText.text = "Mouth Open";
            }
            else {
                mouthText.text = "Mouth Closed";
            }
        }

        void smileDetect() {
            float leftSmile=points[50].y-points[48].y;
            float rightSmile=points[54].y-points[52].y;

            float leftLipSlope = Math.Abs((points[66].y-points[48].y) / (points[66].x-points[48].x));
            float rightLipSlope = Math.Abs((points[66].y-points[54].y) / (points[66].x-points[54].x));

            if (leftLipSlope>0.18 && rightLipSlope>0.18) {
                smileText.text = "Full Smile";
            }
            else if (leftLipSlope>0.18) {
                smileText.text = "Left Smile";
            }
            else if (rightLipSlope>0.18) {
                smileText.text = "Right Smile";
            }
            else {
                smileText.text = "No Smile";
            }
        }

        /// <summary>
        /// Gets the current WebCameraTexture frame that converted to the correct direction.
        /// </summary>
        private Color32[] GetColors ()
        {
            webCamTexture.GetPixels32 (colors);

            if (true) {
                //Adjust an array of color pixels according to screen orientation and WebCamDevice parameter.
                if (rotate90Degree) {
                    Rotate90CW (colors, rotatedColors, webCamTexture.width, webCamTexture.height);
                    FlipColors (rotatedColors, webCamTexture.width, webCamTexture.height);
                    return rotatedColors;
                } else {
                    FlipColors (colors, webCamTexture.width, webCamTexture.height);
                    return colors;
                }
            }
            // return colors;
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();

            #if UNITY_WEBGL && !UNITY_EDITOR
            foreach (var coroutine in coroutines) {
                StopCoroutine (coroutine);
                ((IDisposable)coroutine).Dispose ();
            }
            #endif
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("DlibFaceLandmarkDetectorExample");
            #else
            Application.LoadLevel ("DlibFaceLandmarkDetectorExample");
            #endif
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            if (hasInitDone) {
                requestedDeviceName = null;
                requestedIsFrontFacing = !requestedIsFrontFacing;
                Initialize ();
            }
        }

        /// <summary>
        /// Raises the adjust pixels direction toggle value changed event.
        /// </summary>
        public void OnAdjustPixelsDirectionToggleValueChanged ()
        {
            if (adjustPixelsDirectionToggle.isOn != true) {
                adjustPixelsDirection = true;
                Initialize ();
            }
        }

        /// <summary>
        /// Flips the colors.
        /// </summary>
        /// <param name="colors">Colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void FlipColors (Color32[] colors, int width, int height)
        {
            int flipCode = int.MinValue;

            if (webCamDevice.isFrontFacing) {
                if (webCamTexture.videoRotationAngle == 0) {
                    flipCode = 1;
                } else if (webCamTexture.videoRotationAngle == 90) {
                    flipCode = 1;
                }
                if (webCamTexture.videoRotationAngle == 180) {
                    flipCode = 0;
                } else if (webCamTexture.videoRotationAngle == 270) {
                    flipCode = 0;
                }
            } else {
                if (webCamTexture.videoRotationAngle == 180) {
                    flipCode = -1;
                } else if (webCamTexture.videoRotationAngle == 270) {
                    flipCode = -1;
                }
            }

            if (flipCode > int.MinValue) {
                if (rotate90Degree) {
                    if (flipCode == 0) {
                        FlipVertical (colors, colors, height, width);
                    } else if (flipCode == 1) {
                        FlipHorizontal (colors, colors, height, width);
                    } else if (flipCode < 0) {
                        Rotate180 (colors, colors, height, width);
                    }
                } else {
                    if (flipCode == 0) {
                        FlipVertical (colors, colors, width, height);
                    } else if (flipCode == 1) {
                        FlipHorizontal (colors, colors, width, height);
                    } else if (flipCode < 0) {
                        Rotate180 (colors, colors, height, width);
                    }
                }
            }
        }

        /// <summary>
        /// Flips vertical.
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void FlipVertical (Color32[] src, Color32[] dst, int width, int height)
        {
            for(var i = 0; i < height / 2; i++) {
                var y = i * width;
                var x = (height - i - 1) * width;
                for(var j = 0; j < width; j++) {
                    int s = y + j;
                    int t = x + j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }

        /// <summary>
        /// Flips horizontal.
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void FlipHorizontal (Color32[] src, Color32[] dst, int width, int height)
        {
            for (int i = 0; i < height; i++) {
                int y = i * width;
                int x = y + width - 1;
                for(var j = 0; j < width / 2; j++) {
                    int s = y + j;
                    int t = x - j;
                    Color32 c = src[s];
                    dst[s] = src[t];
                    dst[t] = c;
                }
            }
        }

        /// <summary>
        /// Rotates 180 degrees.
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void Rotate180 (Color32[] src, Color32[] dst, int height, int width)
        {
            int i = src.Length;
            for (int x = 0; x < i/2; x++) {
                Color32 t = src[x];
                dst[x] = src[i-x-1];
                dst[i-x-1] = t;
            }
        }

        /// <summary>
        /// Rotates 90 degrees (CLOCKWISE).
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        void Rotate90CW (Color32[] src, Color32[] dst, int height, int width)
        {
            int i = 0;
            for (int x = height - 1; x >= 0; x--) {
                for (int y = 0; y < width; y++) {
                    dst [i] = src [x + y * height];
                    i++;
                }
            }
        }

        /// <summary>
        /// Rotates 90 degrees (COUNTERCLOCKWISE).
        /// </summary>
        /// <param name="src">Src colors.</param>
        /// <param name="dst">Dst colors.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        void Rotate90CCW (Color32[] src, Color32[] dst, int width, int height)
        {
            int i = 0;
            for (int x = 0; x < width; x++) {
                for (int y = height - 1; y >= 0; y--) {
                    dst [i] = src [x + y * width];
                    i++;
                }
            }
        }
    }
}
