using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_Test_1 {
    public class FrameData {
        CameraSpacePoint[] vertices;
        List<String> keyPointsNames;
        String annotation;
        Vector4 faceRotationQuaternion;
        CameraSpacePoint headPivotPoint;

        public CameraSpacePoint[] Vertices
        {
            get
            {
                return vertices;
            }

            set
            {
                vertices = value;
            }
        }

        public List<string> KeyPointsNames
        {
            get
            {
                return keyPointsNames;
            }

            set
            {
                keyPointsNames = value;
            }
        }

        public Vector4 FaceRotationQuaternion
        {
            get
            {
                return faceRotationQuaternion;
            }

            set
            {
                faceRotationQuaternion = value;
            }
        }

        public CameraSpacePoint HeadPivotPoint
        {
            get
            {
                return headPivotPoint;
            }

            set
            {
                headPivotPoint = value;
            }
        }

        public string Annotation
        {
            get
            {
                return annotation;
            }

            set
            {
                annotation = value;
            }
        }

        public FrameData(CameraSpacePoint[] vertices, List<String> keyPointsNames, String annotation, Vector4 faceRotationQuaternion, CameraSpacePoint headPivotPoint) {
            this.Vertices = vertices;
            this.KeyPointsNames = keyPointsNames;
            this.Annotation = annotation;
            this.FaceRotationQuaternion = faceRotationQuaternion;
            this.HeadPivotPoint = headPivotPoint;
        }

    }
}
