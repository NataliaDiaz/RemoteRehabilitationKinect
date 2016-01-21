using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Media; //for ProcessFrame method
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Win32;
using Kinect.Toolbox.Voice;
using Kinect.Toolbox.Gestures;
using KPICore;
using System.Media;
//using SitStandGestureDetector;
//using BindableNUICamera;


namespace GesturesViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : iKPIC_subscribeHandler
    {
        KinectSensor kinectSensor;

        SwipeGestureDetector swipeGestureRecognizer;
        //SitStandGestureDetector sitGestureDetector, standGestureDetector;        
        //TemplatedGestureDetector leftHipAbductionGestureRecognizer, rightHipAbductionGestureRecognizer, leftHipExtensionGestureRecognizer, rightHipExtensionGestureRecognizer, standGestureRecognizer, sitGestureRecognizer, leftKneeExtensionGestureRecognizer, rightKneeExtensionGestureRecognizer;
        TemplatedGestureDetector circleGestureRecognizer;
        TemplatedPostureDetector templatePostureDetector;
        AngularTemplatedGestureDetector leftHipAbductionGestureRecognizer, rightHipAbductionGestureRecognizer, leftHipExtensionGestureRecognizer, rightHipExtensionGestureRecognizer, standGestureRecognizer, sitGestureRecognizer, leftKneeExtensionGestureRecognizer, rightKneeExtensionGestureRecognizer;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthManager = new DepthStreamManager();
        AudioStreamManager audioManager;
        SkeletonDisplayManager skeletonDisplayManager;
        readonly ContextTracker contextTracker = new ContextTracker();
        readonly AlgorithmicPostureDetector algorithmicPostureRecognizer = new AlgorithmicPostureDetector();
        private bool recordNextFrameForPosture;
        bool displayDepth;
        string circleKBPath, leftHipAbductionKBPath, rightHipAbductionKBPath, leftHipExtensionKBPath, rightHipExtensionKBPath, standKBPath, sitKBPath, leftKneeExtensionKBPath, rightKneeExtensionKBPath;
        string leftHipAbductionAngleKBPath, rightHipAbductionAngleKBPath, leftHipExtensionAngleKBPath, rightHipExtensionAngleKBPath, standAngleKBPath, sitAngleKBPath, leftKneeExtensionAngleKBPath, rightKneeExtensionAngleKBPath;
        string letterT_KBPath;
        string recordingTemplate = "LHipAbd"; //Default
        string currentPosture = "GoingToStand"; // Initial calibration state for measuring user height.
        KinectRecorder recorder;
        KinectReplay replay;
        BindableNUICamera nuiCamera;
        private Skeleton[] skeletons;
        VoiceCommander voiceCommander;
        float currentAngle;
        float previousAngle = 177f;
        DateTime standStartTime, sitStartTime, sitStandSessionStartDateTime;
        float avgSecondsToSit, avgSecondsToStand, sitStandSessionDuration, totalSittingDuration, totalStandingDuration, standingDuration, sittingDuration;
        int nSits, nStands, avgNSits, userHeightInCm, userFaceID, avgNSitsPerSession;
        string username = "Natalia"; // User currently performing the exercise session
        List <SitStandExerciseSession> sessions = new List<SitStandExerciseSession>();
        
        // Head-hand touch for start/end exercise session
        private float oldHandHeadDistance = 100.0f;
        private float handHeadDistance;
        bool sessionStarted = false;
        SoundPlayer dingPlayer;

        //M3 CONNECTION
        //SSAP_XMLTools M3 = new SSAP_XMLTools("SitStand Node", "SmartSpace");
        string host2 = "130.232.85.58"; // Stefan "dodge.abo.fi"
        static string hostAtom = "192.168.11.47";  // Atom board. Web-SIB-Explorer is also installed in the atom box, and should be available through http://192.168.11.47:5000/
        static string hostLaptop = "192.168.11.38"; // in Stefan Laptop
        //   Insert in SPARQL query: PREFIX aha: <http://www.semanticweb.org/ontologies/2013/7/17/AHA.owl#>  //<http://users.abo.fi/ndiaz/public/AHA.owl#>   <= IRI (Protege refactoring to new IRI does not update all indexes).
        //  http://www.semanticweb.org/ontologies/2013/7/17/
        static string pr =  "http://www.semanticweb.org/ontologies/2013/7/17/AHA.owl#"; // Equivalent to: aha:_ //ontology prefix
        // To access the WebSIBExplorer, run ESLab-Web-SIB-Explorer>explorer.py and go to: http://127.0.0.1:5000/

        KPICore.KPICore M3 = new KPICore.KPICore(hostLaptop, 10010, "X");//, "SitStand");
        bool connectedToSmartSpace = false;

        //string query1 = "SELECT ?calendar1 ?user0  WHERE { ?user0 a " + pr + "User .  ?user0 " + pr + "hasName \"Natalia\"^^xsd:string. ?user0 " + pr + "hasCalendar ?calendar1 .}";
        //string query2= "SELECT * WHERE {?s ?p ?o.}";
        //string subQuery3 = "SELECT *  WHERE { ?session a " + pr + "SitStandSession.  ?session ?hasProperty ?property .}"; //"2005-02-28T00:00:00Z"^^xsd:dateTime ;
        
     
        
        
        public MainWindow()
        {

            connectedToSmartSpace = M3.join();
            System.Console.WriteLine("--- Joining Smart Space X " + connectedToSmartSpace);// + M3.isJoinConfirmed(a));
            if (connectedToSmartSpace)
                System.Console.WriteLine("--- Inserting OWL ontology " + M3.insertOWL(Path.Combine(Environment.CurrentDirectory, @"data\AHA.owl"))); // Only works from WebSIBExplorer
            
            InitializeComponent();
            System.Console.WriteLine("INITIALIZED. Detecting exercises...");
        }

        // Explicit  M3 interface member implementation: 
        void iKPIC_subscribeHandler.kpic_SIBEventHandlerSPARQL(SPARQLResults newResults, SPARQLResults obsoleteResults, string subID) {
            System.Console.WriteLine("--- SPARQL Subscription Event Handler " + newResults + " obsoleteResults "+ obsoleteResults);
        }

        // Explicit  M3 interface member implementation: 
        void iKPIC_subscribeHandler.kpic_SIBEventHandler(ArrayList newResults, ArrayList obsoleteResults, string subID)
        {
            System.Console.WriteLine("--- Subscription Event Handler " + newResults + " obsoleteResults " + obsoleteResults);
        }

        //Within the load event, you have to detect the presence of a Kinect sensor and then launch the initialization code or wait for a Kinect sensor to be available:
        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect was disconnected");
                    }
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.NotPowered:
                    if (kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("Kinect is no longer powered");
                    }
                    break;
                default:
                    MessageBox.Show("Unhandled Status: " + e.Status);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            circleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\circleKB.save");
            letterT_KBPath = Path.Combine(Environment.CurrentDirectory, @"data\t_KB.save");
            leftHipAbductionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateLeftHipAbdutionKB.save");
            rightHipAbductionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateRightHipAbdutionKB.save");
            leftHipExtensionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateLeftHipExtensionKB.save");
            rightHipExtensionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateRightHipExtensionKB.save");
            standKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateStandKB.save");
            sitKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateSitKB.save");
            leftKneeExtensionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateLeftKneeExtensionKB.save");
            rightKneeExtensionKBPath = Path.Combine(Environment.CurrentDirectory, @"data\templateRightKneeExtensionKB.save");
            
            leftHipAbductionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateLeftHipAbdutionKB.save");
            rightHipAbductionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateRightHipAbdutionKB.save");
            leftHipExtensionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateLeftHipExtensionKB.save");
            rightHipExtensionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateRightHipExtensionKB.save");
            standAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateStandKB.save");
            sitAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateSitKB.save");
            leftKneeExtensionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateLeftKneeExtensionKB.save");
            rightKneeExtensionAngleKBPath = Path.Combine(Environment.CurrentDirectory, @"data\angles\templateRightKneeExtensionKB.save");

            //streamreader filereader;
            //if (file.exists(circlekbpath))
            //{
            //    //filereader = new streamreader(circlekbpath);
            //    system.console.writeline("file exists *****************");
            //    //fs.flush();
            //    //fs.close();
            //}
            //else
            //    system.console.writeline("file does not exists *****************");
            try
            {
                //listen to any status change for Kinects
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;
                //loop through all the Kinects attached to this PC, and start the first that is connected without an error.
                foreach (KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if (kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }
                }
                if (KinectSensor.KinectSensors.Count == 0)
                    MessageBox.Show("No Kinect found");
                else
                    Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Initialize method is called when a Kinect sensor is connected
        private void Initialize()
        {
            if (kinectSensor == null)
                return;
            //The beam angle is simply displayed by the AudioStreamManager:
            audioManager = new AudioStreamManager(kinectSensor.AudioSource);
            audioBeamAngle.DataContext = audioManager;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;
            swipeGestureRecognizer = new SwipeGestureDetector();
            swipeGestureRecognizer.OnGestureDetected += OnGestureDetected;
            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, kinectCanvas);
            kinectSensor.Start();
            LoadGestureDetector();
            LoadLetterTPostureDetector();
            nuiCamera = new BindableNUICamera(kinectSensor);
            elevationSlider.DataContext = nuiCamera;
            voiceCommander = new VoiceCommander("record", "stop");
            voiceCommander.OrderDetected += voiceCommander_OrderDetected;
            StartVoiceCommander();
            kinectDisplay.DataContext = colorManager;

            dingPlayer = new SoundPlayer("ding.wav");  // Load sound
        }
        //As you can see, the Initialize method is called when a Kinect sensor is connected. This method calls many methods described later in this chapter.
        //The following code is provided to initialize the TemplatedGestureDetector:
        void LoadGestureDetector()
        {
            using (Stream recordStream = File.Open(circleKBPath, FileMode.OpenOrCreate))
            {
                circleGestureRecognizer = new TemplatedGestureDetector("Circle", recordStream);
                circleGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(leftHipAbductionKBPath, FileMode.OpenOrCreate))
            using(Stream angleRecordStream = File.Open(leftHipAbductionAngleKBPath, FileMode.OpenOrCreate))
            {
                leftHipAbductionGestureRecognizer = new AngularTemplatedGestureDetector("LeftHipAbduction", recordStream, angleRecordStream);
                leftHipAbductionGestureRecognizer.DisplayCanvas = gesturesCanvas;
                leftHipAbductionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }               

            using (Stream recordStream = File.Open(rightHipAbductionKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(rightHipAbductionAngleKBPath, FileMode.OpenOrCreate))
            {
                rightHipAbductionGestureRecognizer = new AngularTemplatedGestureDetector("RightHipAbduction", recordStream, angleRecordStream);
                rightHipAbductionGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                rightHipAbductionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(leftHipExtensionKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(leftHipExtensionAngleKBPath, FileMode.OpenOrCreate))
            {
                leftHipExtensionGestureRecognizer = new AngularTemplatedGestureDetector("LeftHipExtension", recordStream, angleRecordStream);
                leftHipExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                leftHipExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(rightHipExtensionKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(rightHipExtensionAngleKBPath, FileMode.OpenOrCreate))
            {
                rightHipExtensionGestureRecognizer = new AngularTemplatedGestureDetector("RightHipExtension", recordStream, angleRecordStream);
                rightHipExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                rightHipExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(leftKneeExtensionKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(leftKneeExtensionAngleKBPath, FileMode.OpenOrCreate))
            {
                leftKneeExtensionGestureRecognizer = new AngularTemplatedGestureDetector("LeftKneeExtension", recordStream, angleRecordStream);
                leftKneeExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                leftKneeExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(rightKneeExtensionKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(rightKneeExtensionAngleKBPath, FileMode.OpenOrCreate))
            {
                rightKneeExtensionGestureRecognizer = new AngularTemplatedGestureDetector("RightKneeExtension", recordStream, angleRecordStream);
                rightKneeExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                rightKneeExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(standKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(standAngleKBPath, FileMode.OpenOrCreate))
            {
                standGestureRecognizer = new AngularTemplatedGestureDetector("Stand", recordStream, angleRecordStream);
                standGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                standGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            using (Stream recordStream = File.Open(sitKBPath, FileMode.OpenOrCreate))
            using (Stream angleRecordStream = File.Open(sitAngleKBPath, FileMode.OpenOrCreate))
            {
                sitGestureRecognizer = new AngularTemplatedGestureDetector("Sit", recordStream, angleRecordStream);
                sitGestureRecognizer.DisplayCanvas = gesturesCanvas; 
                sitGestureRecognizer.OnGestureDetected += OnGestureDetected;
            }

            //using (Stream recordStream = File.Open(leftHipAbductionKBPath, FileMode.OpenOrCreate))
            //{
            //    leftHipAbductionGestureRecognizer = new TemplatedGestureDetector("LeftHipAbduction", recordStream);
            //    leftHipAbductionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    leftHipAbductionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(rightHipAbductionKBPath, FileMode.OpenOrCreate))
            //{
            //    rightHipAbductionGestureRecognizer = new TemplatedGestureDetector("RightHipAbduction", recordStream);
            //    rightHipAbductionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    rightHipAbductionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(leftHipExtensionKBPath, FileMode.OpenOrCreate))
            //{
            //    leftHipExtensionGestureRecognizer = new TemplatedGestureDetector("LeftHipExtension", recordStream);
            //    leftHipExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    leftHipExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(rightHipExtensionKBPath, FileMode.OpenOrCreate))
            //{
            //    rightHipExtensionGestureRecognizer = new TemplatedGestureDetector("RightHipExtension", recordStream);
            //    rightHipExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    rightHipExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(standKBPath, FileMode.OpenOrCreate))
            //{
            //    standGestureRecognizer = new TemplatedGestureDetector("Stand", recordStream);
            //    standGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    standGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(sitKBPath, FileMode.OpenOrCreate))
            //{
            //    sitGestureRecognizer = new TemplatedGestureDetector("Sit", recordStream);
            //    sitGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    sitGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(leftKneeExtensionKBPath, FileMode.OpenOrCreate))
            //{
            //    leftKneeExtensionGestureRecognizer = new TemplatedGestureDetector("LeftKneeExtension", recordStream);
            //    leftKneeExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    leftKneeExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

            //using (Stream recordStream = File.Open(rightKneeExtensionKBPath, FileMode.OpenOrCreate))
            //{
            //    rightKneeExtensionGestureRecognizer = new TemplatedGestureDetector("RightKneeExtension", recordStream);
            //    rightKneeExtensionGestureRecognizer.DisplayCanvas = gesturesCanvas;
            //    rightKneeExtensionGestureRecognizer.OnGestureDetected += OnGestureDetected;
            //}

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gesture"></param>
        ///When the detector detects a gesture, it raises the following event:        
        void OnGestureDetected(string gesture)
        {
           int pos = detectedGestures.Items.Add(string.Format("{0} : {1}", gesture, DateTime.Now));
           detectedGestures.SelectedIndex = pos;
        }
        private void Button_Clear_History(object sender, RoutedEventArgs e)
        {
            clearHistory(); 
        }

        private void clearHistory()
        {
            detectedGestures.Items.Clear();
        }

        //The TemplatedPostureDetector is initialized the same way:        
        void LoadLetterTPostureDetector()
        {
            using (Stream recordStream = File.Open(letterT_KBPath, FileMode.OpenOrCreate))
            {
                templatePostureDetector = new TemplatedPostureDetector("T", recordStream);
                templatePostureDetector.PostureDetected += templatePostureDetector_PostureDetected;
            }
        }

        //And when a posture is detected, the following code is called:
        void templatePostureDetector_PostureDetected(string posture)
        {
            //MessageBox.Show("Give me a.......posture: " + posture);
            System.Console.WriteLine("**********************Template posture detected -> " + posture);
        }

        // Depth data
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (replay != null && !replay.IsFinished)
                return;
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                if (recorder != null && ((recorder.Options & KinectRecordOptions.Depth) != 0))
                {
                    recorder.Record(frame);
                }
                if (!displayDepth)
                    return;
                depthManager.Update(frame);
            }
        }
        // Color data
        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (replay != null && !replay.IsFinished)
                return;
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                    return;
                if (recorder != null && ((recorder.Options & KinectRecordOptions.Color) != 0))
                {
                    recorder.Record(frame);
                }
                if (displayDepth)
                    return;
                colorManager.Update(frame);
            }
        }
        // Skeleton data
        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            double height;
            if (replay != null && !replay.IsFinished)
                return;
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;
                if (recorder != null && ((recorder.Options & KinectRecordOptions.Skeletons) != 0))
                    recorder.Record(frame);
                Tools.GetSkeletons(frame, ref skeletons);
                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;
                ProcessFrame(frame);
                //if (kinectSensor.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)
                //    System.Console.WriteLine("**********************SEATED MODE*******"); //Does not detect lower limbs
                //else System.Console.WriteLine("**********************DEFAULT MODE*******");

                var skeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                if (skeleton != null)
                {
                    height = SkeletonHeight(skeleton);
                    //System.Console.WriteLine( "-------->USER height: "+height);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            displayDepth = !displayDepth;
            if (displayDepth)
            {
                viewButton.Content = "View Color";
                kinectDisplay.DataContext = depthManager;
            }
            else
            {
                viewButton.Content = "View Depth";
                kinectDisplay.DataContext = colorManager;
            }
        }

        // you can add a new path to the internal learning machine by first calling StartRecordTemplate() method. By doing that, any following calls to Add
        // will save the joint position but will also add the position to a new recorded path. The path will be closed, packed, and integrated into the learning 
        // machine with a call to EndRecordTemplate() method.

        //ProcessFrame method is called when a skeleton frame is ready. It is then responsible for feeding the gesture and posture detector classes 
        //after checking the stability with the context tracker
        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            Dictionary<int, string> stabilities = new Dictionary<int, string>();
            standStartTime = DateTime.Now;
            foreach (var skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;
                // Stability?
                contextTracker.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);
                stabilities.Add(skeleton.TrackingId, contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId) ? "Stable" : "Non stable");
                if (!contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                    continue;

                Joint leftKneePosition = skeleton.Joints.Where(j => j.JointType == JointType.KneeLeft).First();//.Position.ToVector3();
                Joint rightKneePosition = skeleton.Joints.Where(j => j.JointType == JointType.KneeRight).First();//.Position.ToVector3();
                Joint centerHipPosition = skeleton.Joints.Where(j => j.JointType == JointType.HipCenter).First();//.Position.ToVector3();
                Joint rightHipPosition = skeleton.Joints.Where(j => j.JointType == JointType.HipRight).First();//.Position.ToVector3();
                Joint leftHipPosition = skeleton.Joints.Where(j => j.JointType == JointType.HipLeft).First();//.Position.ToVector3();
                Joint rightFootPosition = skeleton.Joints.Where(j => j.JointType == JointType.FootRight).First();//.Position.ToVector3();
                Joint leftFootPosition = skeleton.Joints.Where(j => j.JointType == JointType.FootLeft).First();//.Position.ToVector3();
                Joint spinePosition = skeleton.Joints.Where(j => j.JointType == JointType.Spine).First();
                
                // ALGORITHMIC AUTOMATON BASED RECOGNITION  (based on supposition that user is initially "GoingToStand" when appears in camera at first time.
                currentAngle = AngleBetweenTwoVectors(leftHipPosition, leftKneePosition, leftKneePosition, leftFootPosition);
                if (contextTracker.IsBasicallySitting(skeleton) && contextTracker.IsSitting(skeleton)) {
                    if (currentPosture != "Sitting" && currentPosture == "GoingToSit")
                    {
                        currentPosture = "Sitting"; 
                        sittingDuration = (float)(sitStartTime.Subtract(DateTime.Now)).Duration().TotalSeconds; 
                        System.Console.WriteLine("*********** SITTING *********************left knee degree: " + currentAngle + " and duration: " + sittingDuration);
                        OnGestureDetected("Sit");
                        totalSittingDuration += sittingDuration;
                        nSits++;
                    }                       
                }
                else{
                    if (contextTracker.IsStanding(skeleton))
                    {
                        if (currentPosture != "Standing" && currentPosture == "GoingToStand")
                        {
                            currentPosture = "Standing";
                            standingDuration = (float)(standStartTime.Subtract(DateTime.Now)).Duration().TotalSeconds; 
                            System.Console.WriteLine("*********** STANDING *********************left knee degree: " + currentAngle + " and duration: " + standingDuration);
                            OnGestureDetected("Stand");
                            totalStandingDuration += standingDuration;
                            nStands++;
                        }                            
                    }       
                    else
                        if (contextTracker.IsGoingToSit(skeleton, previousAngle, currentAngle))
                        {
                            if (currentPosture != "GoingToSit" && currentPosture == "Standing")
                            {
                                //System.Console.WriteLine("*********** GOING TO SIT *********************left knee degree: " + currentAngle);
                                currentPosture = "GoingToSit";
                                sitStartTime = DateTime.Now;
                            }                                
                        }
                        else {
                            if (contextTracker.IsGoingToStand(skeleton, previousAngle, currentAngle))
                            {
                                if (currentPosture != "GoingToStand" && currentPosture == "Sitting")
                                {
                                    //System.Console.WriteLine("*********** GOING TO STAND *********************left knee degree: " + currentAngle);
                                    currentPosture = "GoingToStand";
                                    standStartTime = DateTime.Now;
                                }                                    
                            }   
                        }
                }
                previousAngle = currentAngle;

                //System.Console.WriteLine("--- HEAD POSITION " + skeleton.Joints[JointType.Head].Position.X);
                // right limit of X axis vision: --- HEAD POSITION -0,8555053, -0,987416. Left: 0.7

                // Start/Stop Logging ExerciseSession with TinHead sound:

                // Is right hand touching head? (furthermore, the person should be standing straight and with the head in the center of the field vision.
                handHeadDistance = jointDistance(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.HandRight]);
                if (handHeadDistance < 0.4f && oldHandHeadDistance > 0.4f && currentPosture != "Standing" && skeleton.Joints[JointType.Head].Position.X < 0.65 && skeleton.Joints[JointType.Head].Position.X > -0.65)  // If touching head
                {             
                    if (!sessionStarted)
                    { // Start session
                        dingPlayer.Play(); // Play tin-head sound
                        sessionStarted = true;
                        sitStandSessionStartDateTime = DateTime.Now;
                        standStartTime = DateTime.Now;
                        sitStartTime = DateTime.Now;
                        username = "Natalia";  //TODO: recognize user
                        clearHistory();
                    }
                    else
                    { // Stop session and save session in M3 Smart Space
                        dingPlayer.Play(); // Play tin-head sound
                        dingPlayer.Play(); // Play tin-head sound
                        sessionStarted = false;
                        sitStandSessionDuration = (float)(DateTime.Now - sitStandSessionStartDateTime).Duration().TotalSeconds;
                        avgNSits = 5; // TODO: calculate history avgNSits by parsing SPARQLResults
                        avgNSitsPerSession = nSits / avgNSits; // TODO: add property to Person and GHI 
                        avgSecondsToSit = totalSittingDuration / nSits;
                        avgSecondsToStand = totalStandingDuration / nStands;

                        //Graph as Arraylist of string[4]={s,p,o,o_type}   
                        //Each RDF triple is represented using a string[4] datatype, where:
                        //string[0] = subject
                        //string[1] = predicate
                        //string[2] = object
                        //string[3] = object type ["uri"|"literal"]
                        // Insert example
                        //string[] cuatriple = new string[4] { pr + "SitStandSession" + DateTime.Now.ToString(), "a", pr + "SitStandSession", "uri" };
                        //string[] cuatriple2 = new string[4] { pr + "Natalia", pr + "executesSitStandSession", (pr + "SitStandSession" + DateTime.Now.ToString()), "literal" };

                        if (nSits > 0)
                        {
                            SitStandExerciseSession session = new SitStandExerciseSession(("SitStandSession" + username + DateTime.Now.ToString()), username, sitStandSessionStartDateTime, sitStandSessionDuration, nSits, avgSecondsToSit, avgSecondsToStand);
                            sessions.Add(session);

                            foreach (SitStandExerciseSession sitSession in sessions)
                            {
                                string[] cuatriple = new string[4] { pr + session.SessionName, "a", pr + "SitStandSession", "uri" };
                                string[] cuatriple2 = new string[4] { pr + session.Username, pr + "executesSitStandSession", pr + session.SessionName, "uri" };
                                string[] cuatriple3 = new string[4] { pr + session.SessionName, pr + "hasStartDateTime", DateTimeToOWLDateTimeStr(session.StartDateTime), "literal" };
                                string[] cuatriple4 = new string[4] { pr + session.SessionName, pr + "hasDuration", FloatToOWLFloatStr(session.Duration), "literal" };
                                string[] cuatriple5 = new string[4] { pr + session.SessionName, pr + "consistsOfNSits", session.NSits.ToString(), "literal" };
                                string[] cuatriple6 = new string[4] { pr + session.SessionName, pr + "tookAvgSecondsToSit", FloatToOWLFloatStr(session.AvgSecondsToSit), "literal" };
                                string[] cuatriple7 = new string[4] { pr + session.SessionName, pr + "tookAvgSecondsToStand", FloatToOWLFloatStr(session.AvgSecondsToStand), "literal" };

                                ArrayList triple = new ArrayList();
                                triple.Add(cuatriple);
                                ArrayList triple2 = new ArrayList();
                                triple2.Add(cuatriple2);
                                ArrayList triple3 = new ArrayList();
                                triple3.Add(cuatriple3);
                                ArrayList triple4 = new ArrayList();
                                triple4.Add(cuatriple4);
                                ArrayList triple5 = new ArrayList();
                                triple5.Add(cuatriple5);
                                ArrayList triple6 = new ArrayList();
                                triple6.Add(cuatriple6);
                                ArrayList triple7 = new ArrayList();
                                triple7.Add(cuatriple7);
                                if (connectedToSmartSpace)  // SAVE IN M3 SIB 
                                {
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple2));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple3));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple4));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple5));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple6));
                                    System.Console.WriteLine("--- Inserting " + M3.insert(triple7));

                                    }
                                else
                                {
                                    // Writes RDF triples to file to pull to a SIB from scratch. TO-DO: method to load triples from file
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple[0] + ", " + cuatriple[1] + ", " + cuatriple[2] + ">\n"); // <-- use \\ not \ in path
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple2[0] + ", " + cuatriple2[1] + ", " + cuatriple2[2] + ">\n");
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple3[0] + ", " + cuatriple3[1] + ", " + cuatriple3[2] + ">\n");
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple4[0] + ", " + cuatriple4[1] + ", " + cuatriple4[2] + ">\n");
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple5[0] + ", " + cuatriple5[1] + ", " + cuatriple5[2] + ">\n");
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple6[0] + ", " + cuatriple6[1] + ", " + cuatriple6[2] + ">\n");
                                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, @"data\KinectSessionsDB.txt"), "<" + cuatriple7[0] + ", " + cuatriple7[1] + ", " + cuatriple7[2] + ">\n");
                                
                                }
                            }
                            totalSittingDuration = 0;
                            totalStandingDuration = 0;
                            nSits = 0;
                            nStands = 0;
                        }
                    }
                }
                oldHandHeadDistance = handHeadDistance;


                //var leftShoulderPosition = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderLeft).First().Position.ToVector3();
                //var rightShoulderPosition = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderRight).First().Position.ToVector3();                        
                //if (contextTracker.AreHipsTowardsSensor(skeleton)) //FRONT EXERCISES
                //    System.Console.WriteLine("Shoulders Z distance: "+ Math.Abs(leftShoulderPosition.Z - rightShoulderPosition.Z));
                //else if(contextTracker.AreHipsInProfileTowardsSensor(skeleton)) //PROFILE EXERCISES
                //    System.Console.WriteLine("Shoulders Z distance: "+ Math.Abs(leftShoulderPosition.Z - rightShoulderPosition.Z));

                //ANGLE-BASED PATTERN RECOGNITION  
                //TODO: complete with more angles for gesture key angles
                //System.Console.WriteLine("************ANGLE GROIN *******: "+ AngleBetweenTwoVectors(leftKneePosition, centerHipPosition, centerHipPosition, rightKneePosition));
                //System.Console.WriteLine("************ANGLE LEFT hip position: *******: " + leftHipPosition.Position.X + " " + leftHipPosition.Position.Y + " " + leftHipPosition.Position.Z);
                //System.Console.WriteLine("************ANGLE LEFT knee position: *******: " + leftKneePosition.Position.X + " " + leftKneePosition.Position.Y + " " + leftKneePosition.Position.Z);
                //System.Console.WriteLine("************ANGLE LEFT foot position: *******: " + leftFootPosition.Position.X + " " + leftFootPosition.Position.Y + " " + leftFootPosition.Position.Z);
                //System.Console.WriteLine("************ANGLE LEFT WAIST *******: " + AngleBetweenTwoVectors(spinePosition, centerHipPosition, centerHipPosition, leftKneePosition));
                //System.Console.WriteLine("************ANGLE KNEE *********************: " + AngleBetweenTwoVectors(leftHipPosition, leftKneePosition, leftKneePosition, leftFootPosition)); 

                //leftHipAbductionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftKneePosition, centerHipPosition, centerHipPosition, rightKneePosition), kinectSensor);
                //rightHipAbductionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftKneePosition, centerHipPosition, centerHipPosition, rightKneePosition), kinectSensor);

                //leftHipExtensionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftKneePosition, centerHipPosition, centerHipPosition, rightKneePosition), kinectSensor);
                //rightHipExtensionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftKneePosition, centerHipPosition, centerHipPosition, rightKneePosition), kinectSensor);

                //leftKneeExtensionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftHipPosition, leftKneePosition, leftKneePosition, leftFootPosition), kinectSensor);
                //rightKneeExtensionGestureRecognizer.AddAngle(AngleBetweenTwoVectors(rightHipPosition, rightKneePosition, rightKneePosition, rightFootPosition), kinectSensor);

                sitGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftHipPosition, leftKneePosition, leftKneePosition, leftFootPosition), kinectSensor);
                standGestureRecognizer.AddAngle(AngleBetweenTwoVectors(leftHipPosition, leftKneePosition, leftKneePosition, leftFootPosition), kinectSensor);
                
                foreach (Joint joint in skeleton.Joints)
                {
                    if (joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                                        
                    //PATH-BASED PATTERN RECOGNITION
                    if (joint.JointType == JointType.HandRight)
                    {
                        swipeGestureRecognizer.Add(joint.Position, kinectSensor);
                        circleGestureRecognizer.Add(joint.Position, kinectSensor);
                    }

                    //var leftShoulderPosition = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderLeft).First().Position.ToVector3();
                    //var rightShoulderPosition = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderRight).First().Position.ToVector3();                        
                    //if (contextTracker.AreHipsTowardsSensor(skeleton)) //FRONT EXERCISES
                    //    System.Console.WriteLine("Shoulders Z distance: "+ Math.Abs(leftShoulderPosition.Z - rightShoulderPosition.Z));
                    //else if(contextTracker.AreHipsInProfileTowardsSensor(skeleton)) //PROFILE EXERCISES
                    //    System.Console.WriteLine("Shoulders Z distance: "+ Math.Abs(leftShoulderPosition.Z - rightShoulderPosition.Z));
                            
                    //CONDITION FILTERS TO BE USED FOR TRAINING/RECOGNITION: sitting/basicallyStanding, shoulders/hips front/profile  TODO: to test
                    
                    if (joint.JointType == JointType.FootLeft)  // LEFT FOOT EXERCISES
                    {
                        //if (contextTracker.IsShouldersTowardsSensor(skeleton) && contextTracker.IsStanding(skeleton)) // FRONT EXERCISES  -using 2 filters
                        //{
                            //System.Console.WriteLine("**********************SKELETON FRONT *******");
                            leftHipAbductionGestureRecognizer.Add(joint.Position, kinectSensor);
                        //}
                        //else if (contextTracker.AreShouldersInProfileTowardsSensor(skeleton) && contextTracker.IsStanding(skeleton))
                        //{ // PROFILE EXERCISES
                            //System.Console.WriteLine("**********************SKELETON PROFILE*******");
                            leftHipExtensionGestureRecognizer.Add(joint.Position, kinectSensor);                            
                        //}
                        //else if (contextTracker.IsBasicallySitting(skeleton) && contextTracker.IsSitting(skeleton)) // SITTING EXERCISES
                            leftKneeExtensionGestureRecognizer.Add(joint.Position, kinectSensor);
                    }
                    if (joint.JointType == JointType.FootRight)  // RIGHT FOOT EXERCISES
                    {
                        //if (contextTracker.IsShouldersTowardsSensor(skeleton) && contextTracker.IsStanding(skeleton)) // FRONT EXERCISES
                        //{
                            rightHipAbductionGestureRecognizer.Add(joint.Position, kinectSensor);
                        //}
                        //else if (contextTracker.AreShouldersInProfileTowardsSensor(skeleton) && contextTracker.IsStanding(skeleton))
                        //{ // PROFILE EXERCISES
                            rightHipExtensionGestureRecognizer.Add(joint.Position, kinectSensor);
                        //}
                        //else if (contextTracker.IsBasicallySitting(skeleton) && contextTracker.IsSitting(skeleton))  // SITTING EXERCISES
                            rightKneeExtensionGestureRecognizer.Add(joint.Position, kinectSensor);
                    }
                    // SIT-STAND EXERCISES  -> 2 points to detect 1 gesture does not work, use similar to SitStandGestureDetector for hybrid (algorithmic and template-based) detector

                    //if (contextTracker.isSitting(skeleton)) //FRONT EXERCISES
                    //    System.Console.WriteLine("********************** SITTING! *******");
                    //else if (contextTracker.isStanding(skeleton))
                    //    System.Console.WriteLine("*********************STANDING******");
                    //else if (contextTracker.isBasicallySitting(skeleton))
                    //    System.Console.WriteLine("*********************BASICALLY SITTING******"); 
                    
                    if (joint.JointType == JointType.Head)  // SIT-STAND EXERCISES
                    {
                        //if (contextTracker.AreShouldersInProfileTowardsSensor(skeleton)) //PROFILE EXERCISES
                        //{
                            sitGestureRecognizer.Add(joint.Position, kinectSensor);
                            standGestureRecognizer.Add(joint.Position, kinectSensor);
                        //}                        
                    }              
                }
                algorithmicPostureRecognizer.TrackPostures(skeleton);
                templatePostureDetector.TrackPostures(skeleton);
                //The recording of a posture is based on the recordNextFrameForPosture boolean. When it is true, 
                //the ProcessFrame method record the current posture in the TemplatedPostureDectector object:
                if (recordNextFrameForPosture)
                {
                    templatePostureDetector.AddTemplate(skeleton);
                    recordNextFrameForPosture = false;
                }
                Skeleton[] list = new Skeleton[1] { skeleton };
                skeletonDisplayManager.Draw(list, false);
            }
            //skeletonDisplayManager.Draw(frame, false); //, false);
            stabilitiesList.ItemsSource = stabilities;
        }

        //the main job is getting a file with a SaveFileDialog so that it can retrieve a stream for the KinectRecorder class.
        private void recordOption_Click(object sender, RoutedEventArgs e)
        {
            if (recorder != null)
            {
                StopRecord();
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };
                if (saveFileDialog.ShowDialog() == true)
                {
                    DirectRecord(saveFileDialog.FileName);
                }
            }
        }


        void DirectRecord(string targetFileName)
        {
            Stream recordStream = File.Create(targetFileName);
            recorder = new KinectRecorder(KinectRecordOptions.Skeletons | KinectRecordOptions.Color | KinectRecordOptions.Depth, recordStream);
            recordOption.Content = "Stop Recording";
        }

        void StopRecord()
        {
            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
                recordOption.Content = "Record";
                return;
            }
        }

        //The replay part of the application does the same thing—it retrieves a 
        //stream for the KinectReplay class and uses three intermediate events to reproduce the behavior of the original Kinect events:
        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select filename", Filter = "Replay files|*.replay" };
            if (openFileDialog.ShowDialog() == true)
            {
                if (replay != null)
                {
                    replay.SkeletonFrameReady -= replay_SkeletonFrameReady;
                    replay.ColorImageFrameReady -= replay_ColorImageFrameReady;
                    replay.Stop();
                }
                Stream recordStream = File.OpenRead(openFileDialog.FileName);
                replay = new KinectReplay(recordStream);
                replay.SkeletonFrameReady += replay_SkeletonFrameReady;
                replay.ColorImageFrameReady += replay_ColorImageFrameReady;
                replay.DepthImageFrameReady += replay_DepthImageFrameReady;
                replay.Start();
            }
        }

        void replay_DepthImageFrameReady(object sender, ReplayDepthImageFrameReadyEventArgs e)
        {
            if (!displayDepth)
                return;
            depthManager.Update(e.DepthImageFrame);
        }

        void replay_ColorImageFrameReady(object sender, ReplayColorImageFrameReadyEventArgs e)
        {
            if (displayDepth)
                return;
            colorManager.Update(e.ColorImageFrame);
        }

        void replay_SkeletonFrameReady(object sender, ReplaySkeletonFrameReadyEventArgs e)
        {
            ProcessFrame(e.SkeletonFrame);
        }

        //Recording new gestures and postures. 
        //The ProcessFrame method is also in charge of recording gestures and postures with the following methods. This code records gestures:
        //The recordGesture button calls the StartRecordingTemplate method of the TemplatedGestureRecognizer 
        //on first click and then calls the EndRecordTemplate method to finalize the recorded template.
        private void recordGesture_Click(object sender, RoutedEventArgs e)
        {
            //if (circleGestureRecognizer.IsRecordingPath)
            //{
            //    circleGestureRecognizer.EndRecordTemplate();
            //    recordGesture.Content = "Record Gesture";
            //    return;
            //}
            //circleGestureRecognizer.StartRecordTemplate();
            //recordGesture.Content = "Stop Recording";

            switch (recordingTemplate)
            {
                case ("LHipAbd"):
                    if (leftHipAbductionGestureRecognizer.IsRecordingPath)  // OPTIMIZE
                    {
                        leftHipAbductionGestureRecognizer.EndRecordTemplate();
                        leftHipAbductionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    leftHipAbductionGestureRecognizer.StartRecordTemplate();
                    leftHipAbductionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("RHipAbd"):
                    if (rightHipAbductionGestureRecognizer.IsRecordingPath)
                    {
                        rightHipAbductionGestureRecognizer.EndRecordTemplate();
                        rightHipAbductionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    rightHipAbductionGestureRecognizer.StartRecordTemplate();
                    rightHipAbductionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("LHipExt"):
                    if (leftHipExtensionGestureRecognizer.IsRecordingPath)
                    {
                        leftHipExtensionGestureRecognizer.EndRecordTemplate();
                        leftHipExtensionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    leftHipExtensionGestureRecognizer.StartRecordTemplate();
                    leftHipExtensionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("RHipExt"):
                    if (rightHipExtensionGestureRecognizer.IsRecordingPath)
                    {
                        rightHipExtensionGestureRecognizer.EndRecordTemplate();
                        rightHipExtensionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    rightHipExtensionGestureRecognizer.StartRecordTemplate();
                    rightHipExtensionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("Stand"):
                    if (standGestureRecognizer.IsRecordingPath)
                    {
                        standGestureRecognizer.EndRecordTemplate();
                        standGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    standGestureRecognizer.StartRecordTemplate();
                    standGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("Sit"):
                    if (sitGestureRecognizer.IsRecordingPath)
                    {
                        sitGestureRecognizer.EndRecordTemplate();
                        sitGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    sitGestureRecognizer.StartRecordTemplate();
                    sitGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("LKneeExt"):
                    if (leftKneeExtensionGestureRecognizer.IsRecordingPath)
                    {
                        leftKneeExtensionGestureRecognizer.EndRecordTemplate();
                        leftKneeExtensionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    leftKneeExtensionGestureRecognizer.StartRecordTemplate();
                    leftKneeExtensionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                case ("RKneeExt"):
                    if (rightKneeExtensionGestureRecognizer.IsRecordingPath)
                    {
                        rightKneeExtensionGestureRecognizer.EndRecordTemplate();
                        rightKneeExtensionGestureRecognizer.EndRecordAngleTemplate();
                        recordGesture.Content = "Record Gesture";
                        return;
                    }
                    rightKneeExtensionGestureRecognizer.StartRecordTemplate();
                    rightKneeExtensionGestureRecognizer.StartRecordAngleTemplate();
                    recordGesture.Content = "Stop Recording";
                    break;
                default:
                    System.Console.WriteLine("IN ORDER TO RECORD, A GESTURE LEARNING MACHINE NEEDS TO BE SELECTED!");
                    break;
            }
        }

        //This code records postures:
        private void recordT_Click(object sender, RoutedEventArgs e)
        {
            recordNextFrameForPosture = true;
        }


        //The VoiceCommander class is used to track the words “record” and “stop” to control the session recording:
        void StartVoiceCommander()
        {
            voiceCommander.Start(kinectSensor);
        }

        void voiceCommander_OrderDetected(string order)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (audioControl.IsChecked == false)
                    return;
                //Because the user cannot select a file when using the VoiceCommander, the application creates a temporary file called kinectRecord 
                //with a suffix composed of a random globally unique identifier (GUID). The file is saved on the user’s desktop.
                switch (order)
                {
                    case "record":
                        DirectRecord(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "kinectRecord" + Guid.NewGuid() + ".replay"));
                        break;
                    case "stop":
                        StopRecord();
                        break;
                }
            }));
        }

        //Cleaning resources
        //Finally, it is important to dispose of all the resources you used. The following code accomplishes this clean up:
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }

        void CloseGestureDetector()
        {
            if (circleGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(circleKBPath))
                {
                    circleGestureRecognizer.SaveState(recordStream);
                }
                circleGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }


            if (leftHipAbductionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(leftHipAbductionKBPath))
                using (Stream angleRecordStream = File.Create(leftHipAbductionAngleKBPath))
                    {
                        leftHipAbductionGestureRecognizer.SaveState(recordStream);
                        leftHipAbductionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                leftHipAbductionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (rightHipAbductionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(rightHipAbductionKBPath))
                using (Stream angleRecordStream = File.Create(rightHipAbductionAngleKBPath))
                    {
                        rightHipAbductionGestureRecognizer.SaveState(recordStream);
                        rightHipAbductionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                rightHipAbductionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (leftHipExtensionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(leftHipExtensionKBPath))
                using (Stream angleRecordStream = File.Create(leftHipExtensionAngleKBPath))
                    {
                        leftHipExtensionGestureRecognizer.SaveState(recordStream);
                        leftHipExtensionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                leftHipExtensionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (rightHipExtensionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(rightHipExtensionKBPath))
                using (Stream angleRecordStream = File.Create(rightHipExtensionAngleKBPath))
                    {

                        rightHipExtensionGestureRecognizer.SaveState(recordStream);
                        rightHipExtensionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                rightHipExtensionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (standGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(standKBPath))
                using (Stream angleRecordStream = File.Create(standAngleKBPath))
                    {
                        standGestureRecognizer.SaveState(recordStream);
                        standGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                standGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (sitGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(sitKBPath))
                using (Stream angleRecordStream = File.Create(sitAngleKBPath))
                    {
                        sitGestureRecognizer.SaveState(recordStream);
                        sitGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                sitGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (leftKneeExtensionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(leftKneeExtensionKBPath))
                using (Stream angleRecordStream = File.Create(leftKneeExtensionAngleKBPath))
                    {
                        leftKneeExtensionGestureRecognizer.SaveState(recordStream);
                        leftKneeExtensionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                leftKneeExtensionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

            if (rightKneeExtensionGestureRecognizer != null)
            {
                using (Stream recordStream = File.Create(rightKneeExtensionKBPath))
                using (Stream angleRecordStream = File.Create(rightKneeExtensionAngleKBPath))
                    {
                        rightKneeExtensionGestureRecognizer.SaveState(recordStream);
                        rightKneeExtensionGestureRecognizer.SaveAnglesState(angleRecordStream);
                    }
                rightKneeExtensionGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }

        }

        void ClosePostureDetector()
        {
            if (templatePostureDetector == null)
                return;
            using (Stream recordStream = File.Create(letterT_KBPath))
            {
                templatePostureDetector.SaveState(recordStream);
            }
            templatePostureDetector.PostureDetected -= templatePostureDetector_PostureDetected;
        }

        private void Clean()
        {
            // ADD other session summary information to M3.

            // Leave M3 Smart Space
            if (connectedToSmartSpace) 
                System.Console.WriteLine("--- Leaving M3 Smart Space... " + M3.leave());
            
            if (swipeGestureRecognizer != null)
            {
                swipeGestureRecognizer.OnGestureDetected -= OnGestureDetected;
            }
            if (audioManager != null)
            {
                audioManager.Dispose();
                audioManager = null;
            }
            CloseGestureDetector();
            ClosePostureDetector();
            if (voiceCommander != null)
            {
                voiceCommander.OrderDetected -= voiceCommander_OrderDetected;
                voiceCommander.Stop();
                voiceCommander = null;
            }
            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }
            if (kinectSensor != null)
            {
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "LHipAbd";
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "RHipAbd";
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "LHipExt";
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "RHipExt";
        }

        private void RadioButton_Checked_5(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "Stand";
        }

        private void Sit_Checked(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "Sit";
        }

        private void LKneeExt_Checked(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "LKneeExt";
        }

        private void RKneeExt_Checked(object sender, RoutedEventArgs e)
        {
            recordingTemplate = "RKneeExt";
        }

        private void elevationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        //Function that deletes the last gesture detected (for bad recordings or patterns which have other red paths which disturb the training)
        private void deleteGesture_Click(object sender, RoutedEventArgs e)
        {
            switch (recordingTemplate)
            {
                case ("LHipAbd"):
                    if (!leftHipAbductionGestureRecognizer.IsRecordingPath)  // OPTIMIZE
                    {
                        System.Console.WriteLine("--Paths before deleting: " + leftHipAbductionGestureRecognizer.AngularLearningMachine.AngleGestures.Count);
                        leftHipAbductionGestureRecognizer.LearningMachine.RemoveLastPath();
                        leftHipAbductionGestureRecognizer.AngularLearningMachine.RemoveLastGesture();
                        System.Console.WriteLine("----> Deleted last template from LHipAbd learning machine -paths left: " + leftHipAbductionGestureRecognizer.AngularLearningMachine.AngleGestures.Count);//LearningMachine.Angles.Count);
                    }
                    break;
                case ("RHipAbd"):
                    if (!rightHipAbductionGestureRecognizer.IsRecordingPath)
                    {
                        rightHipAbductionGestureRecognizer.LearningMachine.RemoveLastPath();
                        rightHipAbductionGestureRecognizer.AngularLearningMachine.RemoveLastGesture();
                        System.Console.WriteLine("----> Deleted last template from RHipAbd learning machine");
                    }
                    break;
                case ("LHipExt"):
                    if (!leftHipExtensionGestureRecognizer.IsRecordingPath)
                    {
                        leftHipExtensionGestureRecognizer.LearningMachine.RemoveLastPath();
                        leftHipExtensionGestureRecognizer.AngularLearningMachine.RemoveLastGesture();
                        System.Console.WriteLine("----> Deleted last template from LHipExt learning machine");
                    }
                    break;
                case ("RHipExt"):
                    if (!rightHipExtensionGestureRecognizer.IsRecordingPath)
                    {
                        rightHipExtensionGestureRecognizer.LearningMachine.RemoveLastPath();
                        rightHipExtensionGestureRecognizer.AngularLearningMachine.RemoveLastGesture();
                        System.Console.WriteLine("----> Deleted last template from RHipExt learning machine");
                    }
                    break;
                case ("Stand"):
                    if (!standGestureRecognizer.IsRecordingPath)
                    {
                        System.Console.WriteLine("--Paths before deleting: " + standGestureRecognizer.LearningMachine.Paths.Count);
                        standGestureRecognizer.LearningMachine.RemoveLastPath();
                        standGestureRecognizer.AngularLearningMachine.RemoveLastGesture(); 
                        System.Console.WriteLine("----> Deleted last template from Stand learning machine -paths left: " + standGestureRecognizer.LearningMachine.Paths.Count);
                    }
                    break;
                case ("Sit"):
                    if (!sitGestureRecognizer.IsRecordingPath)
                    {
                        sitGestureRecognizer.LearningMachine.RemoveLastPath();
                        sitGestureRecognizer.AngularLearningMachine.RemoveLastGesture(); 
                        System.Console.WriteLine("----> Deleted last template from Sit learning machine");
                    }
                    break;
                case ("LKneeExt"):
                    if (!leftKneeExtensionGestureRecognizer.IsRecordingPath)
                    {
                        leftKneeExtensionGestureRecognizer.LearningMachine.RemoveLastPath();
                        leftKneeExtensionGestureRecognizer.AngularLearningMachine.RemoveLastGesture();
                        System.Console.WriteLine("----> Deleted last template from LKneeExt learning machine");
                    }
                    break;
                case ("RKneeExt"):
                    if (!rightKneeExtensionGestureRecognizer.IsRecordingPath)
                    {
                        rightKneeExtensionGestureRecognizer.LearningMachine.RemoveLastPath();
                        rightKneeExtensionGestureRecognizer.AngularLearningMachine.RemoveLastGesture(); 
                        System.Console.WriteLine("----> Deleted last template from RKneeExt learning machine");
                    }
                    break;
                default:
                    System.Console.WriteLine("IN ORDER TO DELETE A RECORDED GESTURE TEMPLATE, A GESTURE LEARNING MACHINE NEEDS TO BE SELECTED!");
                    break;
            }
        }

        //The orientation of the skeleton is important because a gesture should only be detected when the user is in front of the sensor, focused on the sensor 
        //and facing toward it. To detect orientation, we can determine if the two shoulders of the skeleton are at the same distance (given a threshold) from 
        //the sensor. See IsShouldersTowardsSensor.


        private float jointDistance(Joint first, Joint second)
        {
            float dX = first.Position.X - second.Position.X;
            float dY = first.Position.Y - second.Position.Y;
            float dZ = first.Position.Z - second.Position.Z;
            return (float)Math.Sqrt((dX * dX) + (dY * dY) + (dZ * dZ));
        }

        //public float AngleBetweenTwoVectors(Joint v1Start, Joint v1End, Joint v2Start, Joint v2End)
        //{   // cos theta = dotProduct(a,b)/(|a||b|)
        //    //In many calls v1End and van2Start will be the same point, we asume this is the center point or origin to measure the angle
        //    Vector3 v1 = new Vector3(v1Start.Position.X - v1End.Position.X, v1Start.Position.Y - v1End.Position.Y, v1Start.Position.Z - v1End.Position.Z);
        //    Vector3 v2 = new Vector3(v2End.Position.X - v2Start.Position.X, v2End.Position.Y - v2Start.Position.Y, v2End.Position.Z - v2Start.Position.Z);
        //    Vector3 v1normalized = new Vector3(Vector3.Normalize(v1).X, Vector3.Normalize(v1).Y, Vector3.Normalize(v1).Z);
        //    Vector3 v2normalized = Vector3.Normalize(v2);//Vector3.Normalize(v1)
        //    float v1magnitude = Vector3.Magnitude(v1);
        //    float v2magnitude = Vector3.Magnitude(v2);
            
        //    float dotProduct = float.MaxValue;
        //    dotProduct = Vector3.Dot(v1normalized, v2normalized);
        //    System.Console.WriteLine("---->dotprod and v1Norm and V2Norm:  " + dotProduct + "  " + v2normalized.X + "  " + v2normalized.Y+ " "+v2normalized.Z);
        //    System.Console.WriteLine("---->v2 and magnitude:  " + v2.X + "  " + v2.Y+ " "+v2.Z+ " "+Vector3.Magnitude(v2));
        //    float angle = float.MaxValue;
        //    angle = (float) Math.Acos(dotProduct / (v1magnitude * v2magnitude)); //Returns result in radians
        //    System.Console.WriteLine("----> angle:"+angle);
        //    if (float.IsNaN(angle))
        //    {
        //        System.Console.WriteLine("----> ANGLE IS NAN");
        //        if (Vector3.Normalize(v1).X == Vector3.Normalize(v2).X && Vector3.Normalize(v1).Y == Vector3.Normalize(v2).Y && Vector3.Normalize(v1).Z == Vector3.Normalize(v2).Z)
        //            return 0.0f;
        //        else return RadianToDegree((float)Math.PI);
        //    }               
        //    return RadianToDegree(angle);            
        //}

        public float AngleBetweenTwoVectors(Joint v1Start, Joint v1End, Joint v2Start, Joint v2End)
        {   // cos theta = dotProduct(a,b)/(|a||b|)
            //In many calls v1End and v2Start will be the same point, we asume this is the center point or origin to measure the angle
            Vector3 v1 = new Vector3(v1Start.Position.X - v1End.Position.X, v1Start.Position.Y - v1End.Position.Y, v1Start.Position.Z - v1End.Position.Z);
            Vector3 v2 = new Vector3(v2End.Position.X - v2Start.Position.X, v2End.Position.Y - v2Start.Position.Y, v2End.Position.Z - v2Start.Position.Z);

            Vector3 crossProduct = Vector3.Crossprod(v1,v2);
            float dotProduct = Vector3.Dot(v1, v2);
            float angle = (float) Math.Atan2(Vector3.Magnitude(crossProduct), dotProduct);

            //for negative angles
            //if (end.Y > start.Y)
            //    return MathHelper.PiOver2;
            //return -MathHelper.PiOver2;

            return RadianToDegree(angle);
        }


        private float RadianToDegree(float angle)
        {
           return (float) (angle * (180.0 / Math.PI));
        }

        public static double Length(Joint p1, Joint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2));
        }

        //To find the length of more than 2 joints:
        public double LengthJoints(params Joint[] joints)
        {
            double length = 0;
            for (int index = 0; index < joints.Length - 1; index++)
            {
                length += Length(joints[index], joints[index + 1]);
            }
            return length;
        }


        //Since we suppose that both of the user's legs have the same length, we need to choose the one that is tracked more accurately! This means that 
        //no joint position is hypothesized. Here is how to find out the number of tracked joints within a joint collection:

        public int NumberOfTrackedJoints(params Joint[] joints)
        {
            int trackedJoints = 0;
            foreach (var joint in joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    trackedJoints++;
                }
            }
            return trackedJoints;
        }

        public  double SkeletonHeight( Skeleton skeleton)
        {
            const double HEAD_DIVERGENCE = 0.1;

            var head = skeleton.Joints[JointType.Head];
            var neck = skeleton.Joints[JointType.ShoulderCenter];
            var spine = skeleton.Joints[JointType.Spine];
            var waist = skeleton.Joints[JointType.HipCenter];
            var hipLeft = skeleton.Joints[JointType.HipLeft];
            var hipRight = skeleton.Joints[JointType.HipRight];
            var kneeLeft = skeleton.Joints[JointType.KneeLeft];
            var kneeRight = skeleton.Joints[JointType.KneeRight];
            var ankleLeft = skeleton.Joints[JointType.AnkleLeft];
            var ankleRight = skeleton.Joints[JointType.AnkleRight];
            var footLeft = skeleton.Joints[JointType.FootLeft];
            var footRight = skeleton.Joints[JointType.FootRight];

            // Find which leg is tracked more accurately.
            int legLeftTrackedJoints = NumberOfTrackedJoints(hipLeft, kneeLeft, ankleLeft, footLeft);
            int legRightTrackedJoints = NumberOfTrackedJoints(hipRight, kneeRight, ankleRight, footRight);

            double legLength = legLeftTrackedJoints > legRightTrackedJoints ? LengthJoints(hipLeft, kneeLeft, ankleLeft, footLeft) : LengthJoints(hipRight, kneeRight, ankleRight, footRight);
            return LengthJoints(head, neck, spine, waist) + legLength + HEAD_DIVERGENCE;
        }

        public string DateTimeToOWLDateTimeStr(DateTime date)
        {
            // Example of OWL DateTime literal: 2009-11-22T00:00:00
            string owlDateTime = "";// TODO:  reduce repetitive content
            if (date.Year.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Year;
            else owlDateTime = owlDateTime + date.Year;

            if (date.Month.ToString().Length < 2)
                owlDateTime = owlDateTime + "-0" + date.Month;
            else owlDateTime = owlDateTime + "-" + date.Month;

            if (date.Day.ToString().Length < 2)
                owlDateTime = owlDateTime + "-0" + date.Day + "T";
            else owlDateTime = owlDateTime + "-" + date.Day + "T";  
            
            if (date.Hour.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Hour;
            else owlDateTime = owlDateTime + date.Hour;

            if (date.Minute.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Minute;
            else owlDateTime = owlDateTime + ":" + date.Minute;

            if (date.Second.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Second;
            else owlDateTime = owlDateTime + ":" + date.Second; 
            return owlDateTime;
        }

        public string TimeSpanToOWLDateTimeStr (TimeSpan date)
        {
            
            date = date.Duration();
            string owlDateTime = "\"00-00" + "-" + date.Days + "T";  // TODO: 00-00 Testing
            if (date.Hours.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Hours;
            else owlDateTime = owlDateTime + date.Hours;

            if (date.Minutes.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Minutes;
            else owlDateTime = owlDateTime + ":" + date.Minutes;

            if (date.Seconds.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Seconds;
            else owlDateTime = owlDateTime + ":" + date.Seconds; 
            return owlDateTime;
        }

        public string StringToOWLStringStr (string value)
        {
            string owlString = value ;
            return owlString;
        }

        public string IntToOWLIntStr(int value)
        {
            string owlInt =  value.ToString();
            return owlInt;
        }

        public string ByteToOWLIntStr(int value)
        {
            string owlByte = value.ToString() ;
            return owlByte;
        }

        public string FloatToOWLFloatStr(float value)
        {
            string owlFloat = value.ToString().Replace(",",".") ;
            return owlFloat;
        }

        // METHODS TO BE USED IN SPARQL QUERIES (NOT IN RDF INSERTIONS)

        public string DateTimeToOWLDateTimeDatatypeStr(DateTime date)
        {
            // Example of OWL DateTime literal: 2009-11-22T00:00:00
            string owlDateTime = "";// TODO:  reduce repetitive content
            if (date.Year.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Year;
            else owlDateTime = owlDateTime + date.Year;

            if (date.Month.ToString().Length < 2)
                owlDateTime = owlDateTime + "-0" + date.Month;
            else owlDateTime = owlDateTime + "-" + date.Month;


             

            if (date.Hour.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Hour;
            else owlDateTime = owlDateTime + date.Hour;

            if (date.Minute.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Minute;
            else owlDateTime = owlDateTime + ":" + date.Minute;

            if (date.Second.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Second;
            else owlDateTime = owlDateTime + ":" + date.Second;
            owlDateTime = owlDateTime + "\"ˆˆxsd:dateTime";
            
            return owlDateTime;
        }

        public string TimeSpanToOWLDateTimeDatatypeStr(TimeSpan date)
        {
            // TODO: add 0 to left of H, M, S if length is 1.
            date = date.Duration();
            string owlDateTime = "\"00-00" + "-" + date.Days + "T";    // TODO: 00-00 Testing
            if (date.Hours.ToString().Length < 2)
                owlDateTime = owlDateTime + "0" + date.Hours;
            else owlDateTime = owlDateTime + date.Hours;

            if (date.Minutes.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Minutes;
            else owlDateTime = owlDateTime + ":" + date.Minutes;

            if (date.Seconds.ToString().Length < 2)
                owlDateTime = owlDateTime + ":0" + date.Seconds;
            else owlDateTime = owlDateTime + ":" + date.Seconds;
            owlDateTime = owlDateTime + "\"ˆˆxsd:dateTime";

            return owlDateTime;
        }

        public string StringToOWLStringDatatypeStr(string value)
        {
            string owlString = "\"" + value + "\"ˆˆxsd:string";
            return owlString;
        }

        public string IntToOWLIntDatatypeStr(int value)
        {
            string owlInt = "\"" + value + "\"ˆˆxsd:int";
            return owlInt;
        }

        public string ByteToOWLByteDatatypeStr(byte value)
        {
            string owlByte = "\"" + value + "\"ˆˆxsd:byte";
            return owlByte;
        }

        public string FloatToOWLFloatDatatypeStr(float value)
        {
            string owlFloat = "\"" + value.ToString().Replace(",", ".") + "\"ˆˆxsd:float";
            return owlFloat;
        }

        public string BooleanToOWLBooleanDatatypeStr(bool value)
        {
            //Eg: "true"^^xsd:boolean
            string owlBoolean;
            if (value)
                owlBoolean = "\"true\"ˆˆxsd:boolean";
            else
                owlBoolean = "\"false\"ˆˆxsd:boolean";
            return owlBoolean;
        }

        
    }
}