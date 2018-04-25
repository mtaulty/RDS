#if ENABLE_WINMD_SUPPORT
namespace MediaFrameProcessing.Wrappers
{
  using MediaFrameProcessing.Processors;
  using MediaFrameProcessing.VideoDeviceFinders;
  using System;
  using System.Net;
  using Windows.Media.MediaProperties;

  public static class OcrRegexScanner
  {
    public static async void ScanFirstCameraForRegex(
      string regularExpression,
      Action<string> resultCallback,
      TimeSpan timeout)
    {
      string result = null;

      // Note - I keep this frame processor around which means keeping the
      // underlying MediaCapture around because when I didn't keep it
      // around I ended up with a crash in Windows.Media.dll related
      // to disposing of the MediaCapture.
      // So...this isn't what I wanted but it seems to work better :-(
      if (frameProcessor == null)
      {
        var mediaFrameSourceFinder = new MediaFrameSourceFinder();

        // We want a source of media frame groups which contains a color video
        // preview (and we'll take the first one).
        var populated = await mediaFrameSourceFinder.PopulateAsync(
          MediaFrameSourceFinder.ColorVideoPreviewFilter,
          MediaFrameSourceFinder.FirstOrDefault);

        if (populated)
        {
          // We'll take the first video capture device.
          var videoCaptureDevice =
            await VideoCaptureDeviceFinder.FindFirstOrDefaultAsync();

          if (videoCaptureDevice != null)
          {
            // Make a processor which will pull frames from the camera and run
            // ZXing over them to look for QR codes.
            frameProcessor = new OcrRegularExpressionFrameProcessor(
                mediaFrameSourceFinder,
                videoCaptureDevice,
                MediaEncodingSubtypes.Bgra8,
                regularExpression);

            // Remember to ask for auto-focus on the video capture device.
            frameProcessor.SetVideoDeviceControllerInitialiser(
              vd => vd.Focus.TrySetAuto(true));
          }
        }
      }
      if (frameProcessor != null)
      {
        // Process frames for up to 30 seconds to see if we get any QR codes...
        await frameProcessor.ProcessFramesAsync(timeout);

        // See what result we got.
        result = frameProcessor.Result;
      }
      // Call back with whatever result we got.
      resultCallback(result);
    }
    static OcrRegularExpressionFrameProcessor frameProcessor;
  }
}
#endif // #if ENABLE_WINMD_SUPPORT