﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Microsoft.Win32;
using System.Timers;
using System.Threading;
using System.IO;

namespace Kinect_Test_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Provides a Kinect sensor reference.
        private KinectSensor _sensor = null;

        // Acquires body frame data.
        private BodyFrameSource _bodySource = null;

        // Reads body frame data.
        private BodyFrameReader _bodyReader = null;

        // Acquires HD face data.
        private HighDefinitionFaceFrameSource _faceSource = null;

        // Reads HD face data.
        private HighDefinitionFaceFrameReader _faceReader = null;

        // Required to access the face vertices.
        private FaceAlignment _faceAlignment = null;

        // Required to access the face Rotation (Vincent Ortega)
        private Vector4 _faceRotationQuaternion;

        // Required to access the face pivot point (Vincent Ortega)
        private CameraSpacePoint _headPivotPoint;

        // Required to access the face model points.
        private FaceModel _faceModel = null;

        // The color frame reader is used to display the RGB stream
        private ColorFrameReader _colorReader = null;

        // Used to display 1,000 points on screen.
        private List<Ellipse> _points = new List<Ellipse>();

        // Used to display 1,000 points on screen.
        private List<TextBlock> _pointTextBlocks = new List<TextBlock>();


        //This is the list of values for the points needed
        private List<int> _keyPoints = new List<int>();

        //This is the list of values for the points needed
        private List<String> _keyPointsNames = new List<String>();

        //Newly Made CSVWriter
        private CSVWriter _csvWriter = new CSVWriter();

        //BitmapSaver
        private BitmapSaver _bitmapSaver = new BitmapSaver();

        //Statuses
        private Boolean _isFaceTracked = false;

        //Annotion Buttons
        private List<Button> _annotButtons = new List<Button>();

        //Annotation String 
        private String _annotation = null;

        //Output for CSV
        private List<String> _outputList = new List<String>();

        //For Image Saving
        WriteableBitmap _wbmp = null;

        //For timed image saving
        System.Timers.Timer _recordTimer = new System.Timers.Timer();

        FrameData _currentFrameData;

        public MainWindow()
        {
            InitializeComponent();
            Initialize_Sensor();
            Initialize_Key_Points();
            Initialize_Annot_Buttons();

            _recordTimer.Elapsed += (sender, e) => OnTimedEvent(sender, e, _wbmp);
            _recordTimer.Interval = 31;
            _recordTimer.Enabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_faceReader != null)
            {
                _faceReader.Dispose();
                _faceReader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }

            _csvWriter.Stop(null);
        }


        private void Initialize_Sensor()
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                // Listen for body data.
                _bodySource = _sensor.BodyFrameSource;
                _bodyReader = _bodySource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                // Listen for HD face data.
                _faceSource = new HighDefinitionFaceFrameSource(_sensor);
                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;

                _faceModel = new FaceModel();           
                _faceAlignment = new FaceAlignment();
                _faceRotationQuaternion = new Vector4();


                _colorReader = _sensor.ColorFrameSource.OpenReader();
                _colorReader.FrameArrived += ColorReader_FrameArrived;

                // Start tracking!        
                _sensor.Open();
            }
        }

        private void Initialize_Key_Points()
        {
            var a = Enum.GetValues(typeof(HighDetailFacePoints));


            foreach (HighDetailFacePoints m in a)
            {
                _keyPoints.Add((int)m);
                _keyPointsNames.Add(m.ToString());

            }
        }

        private void Initialize_Annot_Buttons()
        {
            _annotButtons.Add(annot_start);
            _annotButtons.Add(annot_end);
            _annotButtons.Add(annot_rest);
            _annotButtons.Add(annot_comment);
        }



        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Body[] bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    Body body = bodies.Where(b => b.IsTracked).FirstOrDefault();


                    
                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {

                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }

                    if (body != null)
                    {
                        //_faceRotationQuaternion = body.JointOrientations[JointType.Neck].Orientation;
                        //System.Diagnostics.Debug.WriteLine(body.Joints[JointType.Head].Position.X +" "+ body.Joints[JointType.Head].Position.Y + " " + body.Joints[JointType.Head].Position.Z);

                    }
                }
            }
        }

        private void FaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                
                if (frame != null && frame.IsFaceTracked)
                {
                   

                    frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);
                    //TODO ISOLATE HERE
                    UpdateFacePoints();
                    Update_Statuses();
                }
            }
        }

        void ColorReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _wbmp = frame.ToBitmap();
                    camera.Source = _wbmp;
                }
            }
        }





        private void UpdateFacePoints()
        {
            if (_faceModel == null)
            {

                return;
            }


            var verts = _faceModel.CalculateVerticesForAlignment(_faceAlignment);


            _faceRotationQuaternion = _faceAlignment.FaceOrientation;

            _headPivotPoint = _faceAlignment.HeadPivotPoint;



            CameraSpacePoint[] vertices = new CameraSpacePoint[_keyPoints.Count];

          

            for (int i = 0; i < vertices.Count(); i++)
            {
                vertices[i] = verts.ElementAt(_keyPoints.ElementAt(i));
            }

            //vertices = verts.ToArray(); unlock to use all points


            if (vertices.Count() > 0 && verts.Count>0)
            {
                _isFaceTracked = true;


                //Updating Points
                _currentFrameData = new FrameData(vertices, _keyPointsNames, _annotation, _faceRotationQuaternion, _headPivotPoint);

                if (_points.Count == 0)
                {
                    for (int index = 0; index < vertices.Count(); index++)
                    {
                        Ellipse ellipse = new Ellipse
                        {
                            Width = 2.0,
                            Height = 2.0,
                            Fill = new SolidColorBrush(Colors.Yellow)
                        };

                        _points.Add(ellipse);

                    }

                    foreach (Ellipse ellipse in _points)
                    {
                        canvas.Children.Add(ellipse);
                    }
                }

                if (_pointTextBlocks.Count == 0){
                    for (int index = 0; index < vertices.Count(); index++)
                    {
                        TextBlock textBlock = new TextBlock
                        {
                            Text = _keyPoints.ElementAt(index).ToString(),

                            Foreground = new SolidColorBrush(Colors.White),
                            TextAlignment = TextAlignment.Left,
                            FontSize = 5
                        };


                        _pointTextBlocks.Add(textBlock);
                    }

                    foreach (TextBlock textBlock in _pointTextBlocks)
                    {
                        canvas.Children.Add(textBlock);
                    }
                }

                for (int index = 0; index < vertices.Count(); index++)
                {
                    CameraSpacePoint vertice = vertices[index];
                    DepthSpacePoint point = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(vertice);

                    if (float.IsInfinity(point.X) || float.IsInfinity(point.Y)) return;

                    Ellipse ellipse = _points[index];


                    TextBlock textBlock = _pointTextBlocks[index];

                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);

                    Canvas.SetLeft(textBlock, point.X + 5);
                    Canvas.SetTop(textBlock, point.Y);
                }


            }

        }



        private void Record_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_csvWriter.IsRecording)
            {
                _csvWriter.Stop(_outputList);

                _bitmapSaver.Stop();


                button.Content = "Record";
                button.Background = Brushes.Green;
                button.Foreground = Brushes.Black;


                MessageBox.Show("Recording Finished");
                /*
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = "Excel files|*.csv"
                };

                dialog.ShowDialog();
                
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    System.IO.File.Copy(_csvWriter.Result, dialog.FileName);
                }
                */

                Enable_Buttons(false);


                _outputList = new List<string>();
                _recordTimer.Enabled = false;
            }
            else
            {
                if(title_box.Text.Equals("")||title_box.Text==null)
                    MessageBox.Show("Please input a title");
                else if (!Directory.Exists(title_box.Text))
                {
                    _csvWriter.Start(title_box.Text,gloss_box.Text,accurate_box.Text);

                    if (check_record_video.IsChecked == true)
                        _bitmapSaver.Start(title_box.Text);

                    Enable_Buttons(true);
                    button.Content = "Stop";
                    button.Background = Brushes.Red;
                    button.Foreground = Brushes.White;

                    _recordTimer.Enabled = true;
                }
                else
                    MessageBox.Show("Title Already Exists");
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e, WriteableBitmap bmp)
        {
            //Console.WriteLine("THIS THING: " + source);
            if (_bitmapSaver.IsRecording)
            {
                Dispatcher.BeginInvoke(
                    new ThreadStart(() => _bitmapSaver.SaveBitmap(bmp)));
            }

            if (_csvWriter.IsRecording)
            {
                Dispatcher.BeginInvoke(
                    new ThreadStart(() => _outputList.Add(_csvWriter.UpdatePoints(_currentFrameData.Vertices, _currentFrameData.KeyPointsNames, _currentFrameData.Annotation, _currentFrameData.FaceRotationQuaternion, _currentFrameData.HeadPivotPoint))));

                if (_annotation != null)
                    _annotation = null;
            }
        }

        private void Update_Statuses()
        {


            if (_isFaceTracked)
            {
                face_track_status.Fill = Brushes.LimeGreen;
            }
            else 
            {
                face_track_status.Fill = Brushes.Black;
            }

        }


        private void Annot_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            _annotation = button.ContentStringFormat;
        }

        private void Comment_Click(object sender, RoutedEventArgs e)
        {

            _annotation = annot_comment_box.Text;
        }

        private void Enable_Buttons (Boolean b)
        {
            foreach(Button butt in _annotButtons)
            {
                butt.IsEnabled = b;
            }
        }

        private void camera_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void camera_Initialized(object sender, EventArgs e)
        {

        }
    }



}
