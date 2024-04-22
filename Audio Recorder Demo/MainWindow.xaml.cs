using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace Audio_Recorder_Demo
{   
    public partial class MainWindow : Window
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int mciSendString(string lpstrCommand, string lpstrReturn, int uReturnLength, int hwndCallback);

        internal string RecordingURI = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AudioRecorderButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderButton.Visibility = Visibility.Collapsed;
            AudioRecorderOffButton.Visibility = Visibility.Visible;

            // Bits per sample = 16
            // Samples per second = 22050
            // Channels = 2
            // Alignment = 4
            // Bytes per second = (16 * 2 * 22050)/8 = 88200

            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            string sCommand = "set recsound bitspersample 16 channels 2 alignment 4 samplespersec 22050 bytespersec 88200 format tag pcm wait";
            mciSendString(sCommand, "", 0, 0);
            mciSendString("record recsound", "", 0, 0);

            URITextBlock.Text = "Recording!!";
        }

        private void AudioRecorderOffButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderOffButton.Visibility = Visibility.Collapsed;
            AudioRecorderButton.Visibility = Visibility.Visible;

            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|wav (.wav)|*.wav|All files (*.*)|*.*";
            dialogue.FilterIndex = 1; //this just tells the computer which dialogue
            // to select first

            string tempName = System.IO.Path.GetTempFileName();

            mciSendString("save recsound " + tempName, "", 0, 0);
            mciSendString("close recsound", "", 0, 0);

            if (dialogue.ShowDialog() == true)
            {
                RecordingURI = dialogue.FileName.Replace("/", "\\");
                try 
                {
                    System.IO.File.Move(tempName, RecordingURI);
                }
                catch (System.IO.IOException) 
                {
                    System.IO.File.Delete(RecordingURI);
                    System.IO.File.Move(tempName, RecordingURI);
                }
                
                //mciSendString("save recsound " + RecordingURI, "", 0, 0);
            }

            URITextBlock.Text = "Recording saved to " + RecordingURI;
        }
    }
}