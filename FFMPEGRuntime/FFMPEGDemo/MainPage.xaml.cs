using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace FFMPEGDemo
{
    public partial class MainPage
    {
        private const string    LogFileName         = "ffmpeg_log.txt";
        private const string    OverlayImagePath    = @"Assets\overlay.png";

        private readonly string _logFilePath        = string.Format(@"{0}\{1}", Windows.Storage.ApplicationData.Current.LocalFolder.Path, LogFileName);
        private readonly string _outputFilePath     = string.Format(@"{0}\{1}", Windows.Storage.ApplicationData.Current.LocalFolder.Path, "output.mp4");
        private readonly string _inputFilePath      = string.Format(@"{0}\{1}", Windows.Storage.ApplicationData.Current.LocalFolder.Path, "input.mp4");

        private bool            _shouldCopyAudio;

        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convert the recorded images to an MP4 file and then play it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSubmitClick(object sender, RoutedEventArgs e)
        {
            btnSubmit.IsEnabled = false;

            // Stop the player
            outputPlayer.Stop();
            outputPlayer.Source = null;

            _shouldCopyAudio = Convert.ToBoolean(AudioCheckbox.IsChecked);

            // Execute convert operation on a background thread
            var thread = new Thread(RunConvert);
            thread.Start();
        }

        /// <summary>
        /// Open video  recorder page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRecordClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Recorder.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Handle play output.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartPlayback(object sender, EventArgs e)
        {
            outputPlayer.Play();
        }

        /// <summary>
        /// Handle Pause playing output.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPausePlayback(object sender, EventArgs e)
        {
            outputPlayer.Pause();
        }

        /// <summary>
        /// Handle Stop playing output.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopPlayback(object sender, EventArgs e)
        {
            outputPlayer.Stop();
        }

        /// <summary>
        /// Perform the image conversion using FFmpeg.
        /// 
        /// Assumes that a series of images are stored on local storage with file name format: Input%d.jpeg
        /// The output MP4 file is also written to local storage on success.
        /// </summary>
        private void RunConvert()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Running FFmpeg...");

                var ffmpeg = new FFMPEGRuntime.FFMpeg(_logFilePath);

                // Create argument list and pass to ffmpeg
                var commands = GetCommands();
                var status = ffmpeg.Run(commands);

                Dispatcher.BeginInvoke(
                    () =>
                        {
                            var statusStr = status == 0 ? "Success" : ("Failure (" + status + ")");
                            txtBlockAnswer.Text = "FFMPEG conversion status: " + statusStr;

                            if (status == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Successfully converted input using FFmpeg!");

                                PlayOutputVideo(_outputFilePath);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Failed to converted input using FFmpeg.");
                            }
                        });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                Dispatcher.BeginInvoke(() => btnSubmit.IsEnabled = true);
            }
        }

        /// <summary>
        /// Play the output file.
        /// </summary>
        /// <param name="outputFilePath"></param>
        private void PlayOutputVideo(string outputFilePath)
        {
            outputPlayer.Source = new Uri(outputFilePath);
            outputPlayer.Play();
        }

        /// <summary>
        /// Constructs the ffmpeg command argument list.
        /// An example of a command string with all options enabled is:
        /// 
        /// -i input_file -i overlay_file -filter-complex copy [in]; [in]rotate=PI/2:ow=ih:oh=iw[rotated]; 
        /// [rotated] vflip[flippedy]; [flippedy] copy [flippedx]; [1:v]scale=50:50 [logo_scaled]; 
        /// [flippedx][logo_scaled]overlay=0:0, crop=480:404:0:0
        /// -c:a copy -y output_file
        /// 
        /// See http://www.ffmpeg.org/ffmpeg.html for usage documentation.
        /// </summary>
        /// <returns>array of arguments to pass to ffmpeg</returns>
        private string[] GetCommands()
        {
            var commandList = new List<string>
                {
                    "-i",
                    _inputFilePath,
                    "-i",
                    OverlayImagePath,
                    "-filter_complex",
                    "copy [in]; [in]rotate=PI/2:ow=ih:oh=iw[rotated]; [rotated] vflip[flippedy]; [flippedy] copy [flippedx]; [1:v]scale=50:50 [logo_scaled]; [flippedx][logo_scaled]overlay=0:0, crop=480:404:0:0"
                };

            // Copy audio
            if (_shouldCopyAudio)
            {
                commandList.Add("-c:a");
                commandList.Add("copy");
            }
            else
            {
                // Disable audio
                commandList.Add("-an"); 
            }

            // Output (always overwrite)
            commandList.Add("-y");
            commandList.Add(_outputFilePath);

            return commandList.ToArray();
        }
    }
}
