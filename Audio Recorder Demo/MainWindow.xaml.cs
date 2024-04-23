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

        private void RecordAllButton_Click(object sender, RoutedEventArgs e)
        {
            RecordAllButton.Visibility = Visibility.Collapsed;
            StopRecordAllButton.Visibility = Visibility.Visible;
        }

        private void StopRecordAllButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecordAllButton.Visibility = Visibility.Collapsed;
            RecordAllButton.Visibility = Visibility.Visible;
        }

        private void RecordAllSwitch_Click(object sender, RoutedEventArgs e)
        {
            DesktopAudioRecorderButton.Visibility = Visibility.Collapsed;
            DesktopAudioRecorderOffButton.Visibility = Visibility.Collapsed;
            AudioRecorderButton.Visibility = Visibility.Collapsed;
            AudioRecorderOffButton.Visibility = Visibility.Collapsed;
        }
        
        private void RecordAllSwitch_Unclick(object sender, RoutedEventArgs e)
        {
            DesktopAudioRecorderButton.Visibility = Visibility.Visible;
            DesktopAudioRecorderOffButton.Visibility = Visibility.Visible;
            AudioRecorderButton.Visibility = Visibility.Visible;
            AudioRecorderOffButton.Visibility = Visibility.Visible;
        }
    }
}

/*
 * Audio graph generation via audio capture
 * 
 * STEP 1. Grab the Audio peak volume
 * STEP 2. Transform it to be a value between 0 and 100
 * STEP 2.5. Volume/MaxVolume * 100
 * STEP 3. Generate waveform BAR with height equal to the calculated volume output height
 * STEP 3.5 Possibly a <Rectangle> with custom class with Height set to calculated volume output height and Center Vertical Alignment
 * STEP 4. Move the bar left using DoubleAnimation and Storyboard in C#
 */