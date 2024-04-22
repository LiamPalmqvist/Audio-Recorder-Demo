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

namespace Audio_Recorder_Demo
{   
    public partial class MainWindow : Window
    {
        internal Uri RecordingURI = new Uri(@"C:\file.txt");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AudioRecorderButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderButton.Visibility = Visibility.Collapsed;
            AudioRecorderOffButton.Visibility = Visibility.Visible;

            URITextBlock.Text = "Recording!!";
        }

        private void AudioRecorderOffButton_Click(object sender, RoutedEventArgs e)
        {
            AudioRecorderOffButton.Visibility = Visibility.Collapsed;
            AudioRecorderButton.Visibility = Visibility.Visible;

            var dialogue = new SaveFileDialog();
            dialogue.Filter = "mp3 (*.mp3)|*.mp3|All files (*.*)|*.*";
            // dialogue.FilterIndex = 1; this just tells the computer which dialogue
            // to select first

            if (dialogue.ShowDialog() == true)
            {
                RecordingURI = new Uri(dialogue.FileName);
                
            }

            URITextBlock.Text = "Recording saved to " + RecordingURI;
        }
    }
}