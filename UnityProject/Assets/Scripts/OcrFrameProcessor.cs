#if ENABLE_WINMD_SUPPORT
namespace MediaFrameProcessing.Processors
{
    using MediaFrameProcessing.VideoDeviceFinders;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Devices.Enumeration;
    using Windows.Media.Capture;
    using Windows.Media.Capture.Frames;
    using Windows.Media.Ocr;
    using System;
    using System.Text.RegularExpressions;
    using System.Net;

    public class OcrRegularExpressionFrameProcessor : MediaCaptureFrameProcessor
    {
        public string Result { get; private set; }

        public OcrRegularExpressionFrameProcessor(
          MediaFrameSourceFinder mediaFrameSourceFinder,
          DeviceInformation videoDeviceInformation,
          string mediaEncodingSubtype,
          string regularExpression,
          MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu)

          : base(
              mediaFrameSourceFinder,
              videoDeviceInformation,
              mediaEncodingSubtype,
              memoryPreference)
        {
            this.regex = new Regex(regularExpression);
        }
        protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
        {
            bool done = false;

            // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
            // says to dispose this softwarebitmap if you access it.
            using (var bitmap = frameReference.VideoMediaFrame.SoftwareBitmap)
            {
                try
                {
                    if (this.ocrEngine == null)
                    {
                        this.ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                    }
                    var results = await this.ocrEngine.RecognizeAsync(bitmap);

                    if (results != null)
                    {
                        var matchingResults = this.regex.Matches(results.Text);

                        if (matchingResults.Count > 0)
                        {
                            done = true;
                            this.Result = matchingResults[0].Value;
                        }
                    }
                }
                catch
                {
                }
            }
            return (done);
        }
        string regularExpression;
        Regex regex;
        OcrEngine ocrEngine;
    }
}
#endif // #if ENABLE_WINMD_SUPPORT
