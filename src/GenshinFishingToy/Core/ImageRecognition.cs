using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = OpenCvSharp.Point;

namespace GenshinFishingToy.Core;

internal class ImageRecognition
{
    public static List<Rect> GetRect(Bitmap img, bool enableImShow = false)
    {
        if (img == null)
        {
            return null!;
        }

        using Mat mask = new();
        using Mat rgbMat = new();
        using Mat src = img.ToMat();

        Cv2.CvtColor(src, rgbMat, ColorConversionCodes.BGR2RGB);
        Scalar lowPurple = new(255, 255, 192);
        Scalar highPurple = new(255, 255, 192);
        Cv2.InRange(rgbMat, lowPurple, highPurple, mask);
        Cv2.Threshold(mask, mask, 0, 255, ThresholdTypes.Binary); // 二值化

        Cv2.FindContours(mask, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, null);
        if (contours.Length > 0)
        {
            Mat imgTar = src.Clone();
            IEnumerable<Rect> boxes = contours.Select(Cv2.BoundingRect).Where(w => w.Height >= 10);
            List<Rect> rects = boxes.ToList();
            foreach (Rect rect in rects)
            {
                Cv2.Rectangle(imgTar, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), Scalar.Red, 2);
            }
            if (enableImShow)
            {
                Cv2.ImShow(Mui("RecognitionJiggingDebug"), imgTar);
            }
            return rects;
        }
        else
        {
            return null!;
        }
    }

    public static Rect MatchWords(Bitmap img, bool enableImShow = false)
    {
        if (img == null)
        {
            return default;
        }
        using Mat src = img.ToMat();
        using Mat result = new();

        Cv2.CvtColor(src, src, ColorConversionCodes.BGR2RGB);
        var lowPurple = new Scalar(253, 253, 253);
        var highPurple = new Scalar(255, 255, 255);
        Cv2.InRange(src, lowPurple, highPurple, src);
        Cv2.Threshold(src, src, 0, 255, ThresholdTypes.Binary);
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(20, 20), new Point(-1, -1));
        Cv2.Dilate(src, src, kernel); // 膨胀

        Scalar color = new(0, 0, 255);
        Cv2.FindContours(src, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, null);
        if (contours.Length > 0)
        {
            var imgTar = img.ToMat();
            var boxes = contours.Select(Cv2.BoundingRect);
            List<Rect> rects = boxes.ToList();
            if (rects.Count > 1)
            {
                rects.Sort((a, b) => b.Height.CompareTo(a.Height));
            }
            if (rects[0].Height < src.Height
                && rects[0].Width * 1d / rects[0].Height >= 3 // 长宽比判断
                && ImageCapture.W > rects[0].Width * 3  // 文字范围3倍小于钓鱼条范围的
                && ImageCapture.W * 1d / 2 > rects[0].X // 中轴线判断左
                && ImageCapture.W * 1d / 2 < rects[0].X + rects[0].Width) // 中轴线判断右
            {
                foreach (Rect rect in rects)
                {
                    Cv2.Rectangle(imgTar, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), Scalar.Red, 2);
                }
                if (enableImShow)
                {
                    Cv2.ImShow(Mui("RecognitionLiftingDebug"), imgTar);
                }
                return rects[0];
            }
        }
        return Rect.Empty;
    }
}
