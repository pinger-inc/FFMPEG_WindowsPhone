using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;

namespace FFMPEGDemo
{
    public partial class Recorder
    {
        private enum ButtonState { Initialized, Ready, Recording, Playback, Paused, NoChange, CameraNotSupported };
        private ButtonState                 _currentAppState;

        private VideoBrush                  _videoRecorderBrush;
        private VideoCaptureDevice          _videoCaptureDevice;
        private CaptureSource               _captureSource;
        private FileSink                    _fileSink;
        private IsolatedStorageFileStream   _isoVideoFile;
        private const string                IsoVideoFileName = "input.mp4";

        public Recorder()
        {
            InitializeComponent();

            // Prepare ApplicationBar and buttons.
            PhoneAppBar = (ApplicationBar)ApplicationBar;
            PhoneAppBar.IsVisible = true;
            StartRecording = ((ApplicationBarIconButton)ApplicationBar.Buttons[0]);
            StopPlaybackRecording = ((ApplicationBarIconButton)ApplicationBar.Buttons[1]);
            StartPlayback = ((ApplicationBarIconButton)ApplicationBar.Buttons[2]);
            PausePlayback = ((ApplicationBarIconButton) ApplicationBar.Buttons[3]);
        }

        #region UI Events
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitializeVideoRecorder();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Dispose of camera and media objects.
            DisposeVideoPlayer();
            DisposeVideoRecorder();

            base.OnNavigatedFrom(e);
        }

        // Update the buttons and text on the UI thread based on app state.
        private void UpdateUI(ButtonState currentButtonState, string statusMessage)
        {
            // Run code on the UI thread.
            Dispatcher.BeginInvoke(delegate
            {

                switch (currentButtonState)
                {
                    // When the camera is not supported by the phone.
                    case ButtonState.CameraNotSupported:
                        StartRecording.IsEnabled = false;
                        StopPlaybackRecording.IsEnabled = false;
                        StartPlayback.IsEnabled = false;
                        PausePlayback.IsEnabled = false;
                        break;

                    // First launch of the application, so no video is available.
                    case ButtonState.Initialized:
                        StartRecording.IsEnabled = true;
                        StopPlaybackRecording.IsEnabled = false;
                        StartPlayback.IsEnabled = false;
                        PausePlayback.IsEnabled = false;
                        break;

                    // Ready to record, so video is available for viewing.
                    case ButtonState.Ready:
                        StartRecording.IsEnabled = true;
                        StopPlaybackRecording.IsEnabled = false;
                        StartPlayback.IsEnabled = true;
                        PausePlayback.IsEnabled = false;
                        break;

                    // Video recording is in progress.
                    case ButtonState.Recording:
                        StartRecording.IsEnabled = false;
                        StopPlaybackRecording.IsEnabled = true;
                        StartPlayback.IsEnabled = false;
                        PausePlayback.IsEnabled = false;
                        break;

                    // Video playback is in progress.
                    case ButtonState.Playback:
                        StartRecording.IsEnabled = false;
                        StopPlaybackRecording.IsEnabled = true;
                        StartPlayback.IsEnabled = false;
                        PausePlayback.IsEnabled = true;
                        break;

                    // Video playback has been paused.
                    case ButtonState.Paused:
                        StartRecording.IsEnabled = false;
                        StopPlaybackRecording.IsEnabled = true;
                        StartPlayback.IsEnabled = true;
                        PausePlayback.IsEnabled = false;
                        break;
                }

                // Display a message.
                txtDebug.Text = statusMessage;

                // Note the current application state.
                _currentAppState = currentButtonState;
            });
        }

        // Start the video recording.
        private void StartRecording_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            StartRecording.IsEnabled = false;

            StartVideoRecording();
        }

        // Handle stop requests.
        private void StopPlaybackRecording_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            StopPlaybackRecording.IsEnabled = false;

            // Stop during video recording.
            if (_currentAppState == ButtonState.Recording)
            {
                StopVideoRecording();

                // Set the button state and the message.
                UpdateUI(ButtonState.NoChange, "Recording stopped.");
            }

            // Stop during video playback.
            else
            {
                // Remove playback objects.
                DisposeVideoPlayer();

                StartVideoPreview();

                // Set the button state and the message.
                UpdateUI(ButtonState.NoChange, "Playback stopped.");
            }
        }

        // Start video playback.
        private void StartPlayback_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            StartPlayback.IsEnabled = false;

            // Start video playback when the file stream exists.
            if (_isoVideoFile != null)
            {
                VideoPlayer.Play();
            }
            // Start the video for the first time.
            else
            {
                // Stop the capture source.
                _captureSource.Stop();

                // Remove VideoBrush from the tree.
                viewfinderRectangle.Fill = null;

                // Create the file stream and attach it to the MediaElement.
                _isoVideoFile = new IsolatedStorageFileStream(IsoVideoFileName,
                                        FileMode.Open, FileAccess.Read,
                                        IsolatedStorageFile.GetUserStoreForApplication());

                VideoPlayer.SetSource(_isoVideoFile);

                // Add an event handler for the end of playback.
                VideoPlayer.MediaEnded += VideoPlayerMediaEnded;

                // Start video playback.
                VideoPlayer.Play();
            }

            // Set the button state and the message.
            UpdateUI(ButtonState.Playback, "Playback started.");
        }

        // Pause video playback.
        private void PausePlayback_Click(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            PausePlayback.IsEnabled = false;

            // If mediaElement exists, pause playback.
            if (VideoPlayer != null)
            {
                VideoPlayer.Pause();
            }

            // Set the button state and the message.
            UpdateUI(ButtonState.Paused, "Playback paused.");
        }

        // Handle stop requests.
        private void StopPlaybackRecordingClick(object sender, EventArgs e)
        {
            // Avoid duplicate taps.
            StopPlaybackRecording.IsEnabled = false;

            // Stop during video recording.
            if (_currentAppState == ButtonState.Recording)
            {
                StopVideoRecording();

                // Set the button state and the message.
                UpdateUI(ButtonState.NoChange, "Recording stopped.");
            }
        }
        #endregion

        public void InitializeVideoRecorder()
        {
            if (_captureSource == null)
            {
                // Create the VideoRecorder objects.
                _captureSource = new CaptureSource();
                _fileSink = new FileSink();

                _videoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();

                // Add eventhandlers for captureSource.
                _captureSource.CaptureFailed += OnCaptureFailed;

                // Initialize the camera if it exists on the phone.
                if (_videoCaptureDevice != null)
                {
                    // Create the VideoBrush for the viewfinder.
                    _videoRecorderBrush = new VideoBrush();
                    _videoRecorderBrush.SetSource(_captureSource);

                    // Display the viewfinder image on the rectangle.
                    viewfinderRectangle.Fill = _videoRecorderBrush;

                    // Start video capture and display it on the viewfinder.
                    _captureSource.Start();

                    // Set the button state and the message.
                    UpdateUI(ButtonState.Initialized, "Tap record to start recording...");
                }
                else
                {
                    // Disable buttons when the camera is not supported by the phone.
                    UpdateUI(ButtonState.NoChange, "Camera Not Supported.");
                }
            }
        }

        // Set recording state: start recording.
        private void StartVideoRecording()
        {
            try
            {
                // Connect _fileSink to _captureSource.
                if (_captureSource.VideoCaptureDevice != null
                    && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();

                    // Connect the input and output of _fileSink.
                    _fileSink.CaptureSource = _captureSource;
                    _fileSink.IsolatedStorageFileName = IsoVideoFileName;
                }

                // Begin recording.
                if (_captureSource.VideoCaptureDevice != null
                    && _captureSource.State == CaptureState.Stopped)
                {
                    _captureSource.Start();
                }

                // Set the button states and the message.
                UpdateUI(ButtonState.Recording, "Recording...");
            }

            // If recording fails, display an error.
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(delegate {
                    txtDebug.Text = "ERROR: " + e.Message;
                });
            }
        }

        // Set the recording state: stop recording.
        private void StopVideoRecording()
        {
            try
            {
                // Stop recording.
                if (_captureSource.VideoCaptureDevice != null
                && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();

                    // Disconnect _fileSink.
                    _fileSink.CaptureSource = null;
                    _fileSink.IsolatedStorageFileName = null;

                    UpdateUI(ButtonState.NoChange, "Preparing viewfinder...");
                    
                    StartVideoPreview();
                }
            }
            // If stop fails, display an error.
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(delegate {
                    txtDebug.Text = "ERROR: " + e.Message.ToString(CultureInfo.InvariantCulture);
                });
            }
        }


        // Set the recording state: display the video on the viewfinder.
        private void StartVideoPreview()
        {
            try
            {
                // Display the video on the viewfinder.
                if (_captureSource.VideoCaptureDevice != null && _captureSource.State == CaptureState.Stopped)
                {
                    // Add captureSource to videoBrush.
                    _videoRecorderBrush.SetSource(_captureSource);

                    // Add videoBrush to the visual tree.
                    viewfinderRectangle.Fill = _videoRecorderBrush;

                    _captureSource.Start();

                    // Set the button states and the message.
                    UpdateUI(ButtonState.Ready, "Ready to record.");
                }
            }
            // If preview fails, display an error.
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(delegate {
                    txtDebug.Text = "ERROR: " + e.Message;
                });
            }
        }

        // If recording fails, display an error message.
        private void OnCaptureFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(delegate {
                txtDebug.Text = "ERROR: " + e.ErrorException.Message;
            });
        }

        // Display the viewfinder when playback ends.
        public void VideoPlayerMediaEnded(object sender, RoutedEventArgs e)
        {
            // Remove the playback objects.
            DisposeVideoPlayer();

            StartVideoPreview();
        }

        private void DisposeVideoPlayer()
        {
            if (VideoPlayer != null)
            {
                // Stop the VideoPlayer MediaElement.
                VideoPlayer.Stop();

                // Remove playback objects.
                VideoPlayer.Source = null;
                _isoVideoFile = null;

                // Remove the event handler.
                VideoPlayer.MediaEnded -= VideoPlayerMediaEnded;
            }
        }

        private void DisposeVideoRecorder()
        {
            if (_captureSource != null)
            {
                // Stop captureSource if it is running.
                if (_captureSource.VideoCaptureDevice != null
                    && _captureSource.State == CaptureState.Started)
                {
                    _captureSource.Stop();
                }

                // Remove the event handler for captureSource.
                _captureSource.CaptureFailed -= OnCaptureFailed;

                // Remove the video recording objects.
                _captureSource = null;
                _videoCaptureDevice = null;
                _fileSink = null;
                _videoRecorderBrush = null;
            }
        }
    }
}
