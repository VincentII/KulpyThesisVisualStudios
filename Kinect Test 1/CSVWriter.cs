﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_Test_1
{
    public class CSVWriter
    {
        int _current = 0;

        bool _hasEnumeratedPoints = false;

        public bool IsRecording { get; protected set; }

        public string Folder { get; protected set; }

        public string Result { get; protected set; }

        public void Start()
        {
            IsRecording = true;
            Folder = DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss");

            Directory.CreateDirectory(Folder);
        }

        public void UpdatePoints(CameraSpacePoint [] vertices, List<String> keyPointsNames,String annot)
        {
            if (!IsRecording) return;
            if (vertices == null) return;
            if (annot == null)
                annot = "";

            string path = Path.Combine(Folder, _current.ToString() + ".line");

            using (StreamWriter writer = new StreamWriter(path))
            {
                StringBuilder line = new StringBuilder();

                if (!_hasEnumeratedPoints)
                {
                    line.Append("Time,Annotation,");
                    for (int i=0; i<vertices.Length;i++)
                    {
                        line.Append(string.Format("{0},,,", keyPointsNames.ElementAt(i)));
                    }
                    line.AppendLine();

                    line.Append("T,A,");
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        line.Append("X,Y,Z,");
                    }
                    line.AppendLine();

                    _hasEnumeratedPoints = true;
                }

                line.Append(string.Format("{0},",DateTime.Now.ToString("HH:mm:ss")));
                line.Append(string.Format("{0},", annot));
                for (int i = 0; i < vertices.Length; i++)
                {
                    line.Append(string.Format("{0},{1},{2},", vertices[i].X, vertices[i].Y, vertices[i].Z));
                }

                writer.Write(line);

                _current++;
            }
        }

        public void Stop()
        {
            IsRecording = false;
            _hasEnumeratedPoints = false;

            Result = DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss") + ".csv";

            using (StreamWriter writer = new StreamWriter(Result))
            {
                for (int index = 0; index < _current; index++)
                {
                    string path = Path.Combine(Folder, index.ToString() + ".line");

                    if (File.Exists(path))
                    {
                        string line = string.Empty;

                        using (StreamReader reader = new StreamReader(path))
                        {
                            line = reader.ReadToEnd();
                        }

                        writer.WriteLine(line);
                    }
                }
            }

            Directory.Delete(Folder, true);
        }
    }
}