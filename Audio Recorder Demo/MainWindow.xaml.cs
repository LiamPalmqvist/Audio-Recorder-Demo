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
//using NAudio;
//using NAudio.Wave;
using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;



namespace Audio_Recorder_Demo
{   
    public partial class MainWindow : Window
    {

        internal string RecordingURI = "";
        string tempName = System.IO.Path.GetTempFileName();

        // Set up global audio capture sources
        internal WaveIn? soundKeeper = null;
        internal WasapiLoopbackCapture? desktopSoundKeeper = null;
        internal IWriteable ?audioWriter = null;
        internal IWaveSource ?finalWaveSource = null;
        internal IWaveSource ?desktopFinalWaveSource = null;

        // Setting up Threading
        Thread micThread;
        Thread desktopThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AudioRecorderButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderButton.Visibility = Visibility.Collapsed;
            AudioRecorderOffButton.Visibility = Visibility.Visible;

            soundKeeper = new WaveIn();
            //soundKeeper.Device = selected_device;
            soundKeeper.Initialize();
            var soundInSource = new SoundInSource(soundKeeper);

            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ToSampleSource());
            finalWaveSource = singleBlockNotificationStream.ToWaveSource();

            audioWriter = new WaveWriter(tempName, finalWaveSource.WaveFormat);
            byte[] buffer = new byte[finalWaveSource.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = finalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                    audioWriter.Write(buffer, 0, read);
            };

            soundKeeper.Start();

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

            // These won't be null because the button cannot be pressed if
            // the software isn't recording
            soundKeeper.Stop();
            soundKeeper.Dispose();
            soundKeeper = null;
            finalWaveSource.Dispose();

            if (audioWriter is IDisposable)
            {
                ((IDisposable)audioWriter).Dispose();
            }

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

            desktopSoundKeeper = new WasapiLoopbackCapture();
            //soundKeeper.Device = selected_device;
            desktopSoundKeeper.Initialize();
            var soundInSource = new SoundInSource(desktopSoundKeeper);

            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ToSampleSource());
            finalWaveSource = singleBlockNotificationStream.ToWaveSource();

            audioWriter = new WaveWriter(tempName, finalWaveSource.WaveFormat);
            byte[] buffer = new byte[finalWaveSource.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = finalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                    audioWriter.Write(buffer, 0, read);
            };

            desktopSoundKeeper.Start();

            URITextBlock.Text = "Recording!!";
        }

        private void DesktopAudioRecorderOffButton_Click(object sender, RoutedEventArgs e)
        {
            DesktopAudioRecorderOffButton.Visibility = Visibility.Collapsed;
            DesktopAudioRecorderButton.Visibility = Visibility.Visible;

            // These won't be null because the button cannot be pressed if
            // the software isn't recording
            desktopSoundKeeper.Stop();
            desktopSoundKeeper.Dispose();
            desktopSoundKeeper = null;
            finalWaveSource.Dispose();

            if (audioWriter is IDisposable)
            {
                ((IDisposable)audioWriter).Dispose();
            }

            Trace.WriteLine("Success");

            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|wav (.wav)|*.wav|All files (*.*)|*.*";
            dialogue.FilterIndex = 2; //this just tells the computer which dialogue

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

            StartDesktopRecording();
            StartMicRecording();

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
        
        async Task StartDesktopRecording()
        {
            await Task.Run(() =>
            { 
                desktopSoundKeeper = new WasapiLoopbackCapture();
                desktopSoundKeeper.Initialize();

                var desktopSoundInSource = new SoundInSource(desktopSoundKeeper);

                var desktopSingleBlockNotificationStream = new SingleBlockNotificationStream(desktopSoundInSource.ToSampleSource());
                desktopFinalWaveSource = desktopSingleBlockNotificationStream.ToWaveSource();

                audioWriter = new WaveWriter(System.IO.Path.GetTempFileName(), desktopFinalWaveSource.WaveFormat);
                byte[] buffer = new byte[finalWaveSource.WaveFormat.BytesPerSecond / 2];

                desktopSoundInSource.DataAvailable += (s, e) =>
                {
                    int read;
                    while ((read = desktopFinalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                        audioWriter.Write(buffer, 0, (int)read);
                };

                desktopSoundKeeper.Start();
            });
        }

        async Task StartMicRecording()
        {
            await Task.Run(() =>
            {
                soundKeeper = new WaveIn();

                soundKeeper.Initialize();

                var micSoundInSource = new SoundInSource(soundKeeper);

                var micSingleBlockNotificationStream = new SingleBlockNotificationStream(micSoundInSource.ToSampleSource());
                finalWaveSource = micSingleBlockNotificationStream.ToWaveSource();

                audioWriter = new WaveWriter(System.IO.Path.GetTempFileName(), finalWaveSource.WaveFormat);
                byte[] buffer = new byte[finalWaveSource.WaveFormat.BytesPerSecond / 2];
                micSoundInSource.DataAvailable += (s, e) =>
                {
                    int read;
                    while ((read = finalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                        audioWriter.Write(buffer, 0, read);
                };

                soundKeeper.Start();
            });
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