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
using System.IO;



namespace Audio_Recorder_Demo
{   
    public partial class MainWindow : Window
    {

        internal string RecordingURI = "";
        string tempName = System.IO.Path.GetTempFileName();
        string tempName2 = System.IO.Path.GetTempFileName();

        // Set up global audio capture sources
        internal WaveIn? soundKeeper = null;
        internal WasapiLoopbackCapture? desktopSoundKeeper = null;
        internal IWriteable ?audioWriter = null;
        internal IWriteable ?desktopAudioWriter = null;
        internal IWaveSource ?finalWaveSource = null;
        internal IWaveSource ?desktopFinalWaveSource = null;

        // set up for cancellation of tasks
        CancellationTokenSource tokenSource = new CancellationTokenSource();

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

            // Set up for cancellation of task
            CancellationToken ct = tokenSource.Token;
            
            Task.Run(() =>
            {
                tempName = System.IO.Path.GetTempFileName();

                soundKeeper = new WaveIn();

                soundKeeper.Initialize();

                var micSoundInSource = new SoundInSource(soundKeeper);

                var micSingleBlockNotificationStream = new SingleBlockNotificationStream(micSoundInSource.ToSampleSource());
                finalWaveSource = micSingleBlockNotificationStream.ToWaveSource();

                audioWriter = new WaveWriter(tempName, finalWaveSource.WaveFormat);
                byte[] buffer = new byte[finalWaveSource.WaveFormat.BytesPerSecond / 2];
                micSoundInSource.DataAvailable += (s, e) =>
                {
                    int read;
                    while ((read = finalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        audioWriter.Write(buffer, 0, read);
                    }
                };
                soundKeeper.Start();
            }, tokenSource.Token);

            Task.Run(() =>
            {
                tempName = System.IO.Path.GetTempFileName();

                desktopSoundKeeper = new WasapiLoopbackCapture();
                desktopSoundKeeper.Initialize();

                var desktopSoundInSource = new SoundInSource(desktopSoundKeeper);

                var desktopSingleBlockNotificationStream = new SingleBlockNotificationStream(desktopSoundInSource.ToSampleSource());
                desktopFinalWaveSource = desktopSingleBlockNotificationStream.ToWaveSource();

                desktopAudioWriter = new WaveWriter(tempName2, desktopFinalWaveSource.WaveFormat);
                byte[] buffer = new byte[desktopFinalWaveSource.WaveFormat.BytesPerSecond / 2];

                desktopSoundInSource.DataAvailable += (s, e) =>
                {
                    int read;
                    while ((read = desktopFinalWaveSource.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        desktopAudioWriter.Write(buffer, 0, (int)read);
                    }
                };
                desktopSoundKeeper.Start();
            }, tokenSource.Token);
        }

        private async void StopRecordAllButton_Click(object sender, RoutedEventArgs e)
        {
            StopRecordAllButton.Visibility = Visibility.Collapsed;
            RecordAllButton.Visibility = Visibility.Visible;

            soundKeeper.Stop();
            soundKeeper.Dispose();
            soundKeeper = null;
            finalWaveSource.Dispose();

            ((IDisposable)audioWriter).Dispose();


            desktopSoundKeeper.Stop();
            desktopSoundKeeper.Dispose();
            desktopSoundKeeper = null;
            desktopFinalWaveSource.Dispose();

            ((IDisposable)desktopAudioWriter).Dispose();

            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|wav (.wav)|*.wav|All files (*.*)|*.*";
            dialogue.FilterIndex = 2; //this just tells the computer which dialogue

            // This saves the file after the recording is done and put in a temp file
            if (dialogue.ShowDialog() == true)
            {
                RecordingURI = dialogue.FileName.Replace("/", "\\");
                
                // Simple temp name for audio recording desktop part
                string RecordingURI2 = "";
                string[] temp = RecordingURI.Split(".");
                temp[temp.Length - 2] += "_Desktop.";
                foreach (string URI in temp)
                {
                    RecordingURI2 += URI;
                }
                

                Trace.WriteLine(RecordingURI2);



                FileInfo file = new FileInfo(tempName2);
                try
                {
                    using (FileStream stream = file.OpenRead())
                    {
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    foreach (Process process in FileUtil.WhoIsLocking(tempName2))
                    {
                        Trace.WriteLine("processname: " + process);
                        process.Close();
                    }
                }
                
                /*
                file = new FileInfo(tempName2);
                try
                {
                    using (FileStream stream = file.OpenRead())
                    {
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    foreach (Process process in FileUtil.WhoIsLocking(tempName))
                    {
                        Trace.WriteLine("processname: " + process);
                        process.Close();
                    }
                }
                */

                
                System.IO.File.Move(tempName, RecordingURI, true);
                
                System.IO.File.Move(tempName2, RecordingURI2, true);

            }

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

        public static class FileUtil
        {
            [StructLayout(LayoutKind.Sequential)]
            struct RM_UNIQUE_PROCESS
            {
                public int dwProcessId;
                public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
            }

            const int RmRebootReasonNone = 0;
            const int CCH_RM_MAX_APP_NAME = 255;
            const int CCH_RM_MAX_SVC_NAME = 63;

            enum RM_APP_TYPE
            {
                RmUnknownApp = 0,
                RmMainWindow = 1,
                RmOtherWindow = 2,
                RmService = 3,
                RmExplorer = 4,
                RmConsole = 5,
                RmCritical = 1000
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            struct RM_PROCESS_INFO
            {
                public RM_UNIQUE_PROCESS Process;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
                public string strAppName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
                public string strServiceShortName;

                public RM_APP_TYPE ApplicationType;
                public uint AppStatus;
                public uint TSSessionId;
                [MarshalAs(UnmanagedType.Bool)]
                public bool bRestartable;
            }

            [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
            static extern int RmRegisterResources(uint pSessionHandle,
                                                  UInt32 nFiles,
                                                  string[] rgsFilenames,
                                                  UInt32 nApplications,
                                                  [In] RM_UNIQUE_PROCESS[] rgApplications,
                                                  UInt32 nServices,
                                                  string[] rgsServiceNames);

            [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
            static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

            [DllImport("rstrtmgr.dll")]
            static extern int RmEndSession(uint pSessionHandle);

            [DllImport("rstrtmgr.dll")]
            static extern int RmGetList(uint dwSessionHandle,
                                        out uint pnProcInfoNeeded,
                                        ref uint pnProcInfo,
                                        [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                        ref uint lpdwRebootReasons);

            /// <summary>
            /// Find out what process(es) have a lock on the specified file.
            /// </summary>
            /// <param name="path">Path of the file.</param>
            /// <returns>Processes locking the file</returns>
            /// <remarks>See also:
            /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
            /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
            /// 
            /// </remarks>
            static public List<Process> WhoIsLocking(string path)
            {
                uint handle;
                string key = Guid.NewGuid().ToString();
                List<Process> processes = new List<Process>();

                int res = RmStartSession(out handle, 0, key);
                if (res != 0) throw new Exception("Could not begin restart session.  Unable to determine file locker.");

                try
                {
                    const int ERROR_MORE_DATA = 234;
                    uint pnProcInfoNeeded = 0,
                         pnProcInfo = 0,
                         lpdwRebootReasons = RmRebootReasonNone;

                    string[] resources = new string[] { path }; // Just checking on one resource.

                    res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                    if (res != 0) throw new Exception("Could not register resource.");

                    //Note: there's a race condition here -- the first call to RmGetList() returns
                    //      the total number of process. However, when we call RmGetList() again to get
                    //      the actual processes this number may have increased.
                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                    if (res == ERROR_MORE_DATA)
                    {
                        // Create an array to store the process results
                        RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                        pnProcInfo = pnProcInfoNeeded;

                        // Get the list
                        res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                        if (res == 0)
                        {
                            processes = new List<Process>((int)pnProcInfo);

                            // Enumerate all of the results and add them to the 
                            // list to be returned
                            for (int i = 0; i < pnProcInfo; i++)
                            {
                                try
                                {
                                    processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                                }
                                // catch the error -- in case the process is no longer running
                                catch (ArgumentException) { }
                            }
                        }
                        else throw new Exception("Could not list processes locking resource.");
                    }
                    else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
                }
                finally
                {
                    RmEndSession(handle);
                }

                return processes;
            }
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