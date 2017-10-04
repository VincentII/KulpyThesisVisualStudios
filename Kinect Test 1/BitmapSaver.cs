using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kinect_Test_1
{
    class BitmapSaver
    {
        public int index;

        public bool IsRecording { get; protected set; }

        public string Folder { get; protected set; }

        public void Start()
        {
            IsRecording = true;
            index = 0;
            Folder ="IMG-"+ DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss");

            System.Diagnostics.Debug.Write(Folder);
           Directory.CreateDirectory(Folder);
        }

        public void SaveBitmap(WriteableBitmap bmpSource)
        {
            // JpegBitmapEncoder to save BitmapSource to file
            // imageSerial is the serial of the sequential image
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpSource));
            using (var fs = new FileStream("./"+Folder+"/IMG-" + (index++) + ".jpeg", FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }

        public void Stop()
        {
            IsRecording = false;
            index = 0;
        }
    }
}
