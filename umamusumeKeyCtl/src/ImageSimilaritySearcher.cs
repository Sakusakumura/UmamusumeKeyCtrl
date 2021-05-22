using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace umamusumeKeyCtl
{
    public class ImageSimilaritySearcher
    {

        public ImageSimilaritySearcher()
        {
        }

        public double CalcurateSimilarity(Bitmap srcImage, Bitmap targetImage)
        {
            double result = 0.0;
            
            // 検索対象の画像とテンプレート画像
            using (Mat src = BitmapConverter.ToMat(srcImage))
            using (Mat target = BitmapConverter.ToMat(targetImage))
            using (Mat _result = new Mat())
            {

                // テンプレートマッチ
                Cv2.MatchTemplate(src, target, _result, TemplateMatchModes.CCoeffNormed);

                // 類似度が最大/最小となる画素の位置を調べる
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(_result, out minval, out maxval, out minloc, out maxloc);

                // しきい値で判断
                var threshold = 0.9;
                if (maxval >= threshold)
                {

                    // 最も見つかった場所に赤枠を表示
                    Rect rect = new Rect(maxloc.X, maxloc.Y, target.Width, target.Height);
                    Cv2.Rectangle(src, rect, new OpenCvSharp.Scalar(0, 0, 255), 2);

                    // ウィンドウに画像を表示
                    Cv2.ImShow("template1_show", src);

                }
                else
                {
                    // 見つからない
                }
            }

            return 0.0;
        }
    }
}