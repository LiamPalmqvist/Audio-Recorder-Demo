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
using NAudio;
using NAudio.Wave;

namespace Audio_Recorder_Demo
{   
    public partial class MainWindow : Window
    {

        internal string RecordingURI = "";
        string tempName = System.IO.Path.GetTempFileName();
  
        internal WaveInEvent waveIn = new WaveInEvent();
        internal WasapiLoopbackCapture desktopWaveIn = new WasapiLoopbackCapture(); 
        internal WaveFileWriter writer = null;
        internal WaveFileWriter desktopWriter = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AudioRecorderButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderButton.Visibility = Visibility.Collapsed;
            AudioRecorderOffButton.Visibility = Visibility.Visible;

            writer = new WaveFileWriter(tempName, waveIn.WaveFormat);
            waveIn.StartRecording();
            waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
            };

            URITextBlock.Text = "Recording!!";
        }

        private void AudioRecorderOffButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderOffButton.Visibility = Visibility.Collapsed;
            AudioRecorderButton.Visibility = Visibility.Visible;
            
            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|wav (.wav)|*.wav|All files (*.*)|*.*";
            dialogue.FilterIndex = 2; //this just tells the computer which dialogue
            // to select the second dialogue option

            waveIn.StopRecording();

            waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
                waveIn.Dispose();
            };

            // This saves the file after the recording is done and put in a temp file
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
            }

            URITextBlock.Text = "Recording saved to " + RecordingURI;
        }

        private void DesktopAudioRecorderButton_Click(object sender, RoutedEventArgs e)
        {
            DesktopAudioRecorderButton.Visibility = Visibility.Collapsed;
            DesktopAudioRecorderOffButton.Visibility = Visibility.Visible;

            desktopWriter = new WaveFileWriter(tempName, desktopWaveIn.WaveFormat);
            desktopWaveIn.StartRecording();
            desktopWaveIn.DataAvailable += (s, a) =>
            {
                desktopWriter.Write(a.Buffer, 0, a.BytesRecorded);
            };

            URITextBlock.Text = "Recording!!";
        }

        private void DesktopAudioRecorderOffButton_Click(object sender, RoutedEventArgs e)
        {
            DesktopAudioRecorderOffButton.Visibility = Visibility.Collapsed;
            DesktopAudioRecorderButton.Visibility = Visibility.Visible;

            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|wav (.wav)|*.wav|All files (*.*)|*.*";
            dialogue.FilterIndex = 2; //this just tells the computer which dialogue
            // to select the second dialogue option

            desktopWaveIn.StopRecording();

            desktopWaveIn.RecordingStopped += (s, a) =>
            {
                desktopWriter?.Dispose();
                desktopWriter = null;
                desktopWaveIn.Dispose();
            };

            // This saves the file after the recording is done and put in a temp file
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
            }

            URITextBlock.Text = "Recording saved to " + RecordingURI;
        }
    }
}