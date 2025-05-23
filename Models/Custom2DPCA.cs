using OpenCvSharp;
using System.Windows.Media.Media3D;

namespace PCAImageProcessing.Models
{
    public class Custom2DPCA
    {
        public Mat ReigenValues { get; private set; }
        public Mat ReigenVectors { get; private set; }

        public Mat CeigenValues { get; private set; }
        public Mat CeigenVectors { get; private set; }

        public Mat ReduceReigenVectors { get; private set; }
        public Mat ReduceCeigenVectors { get; private set; }

        public Mat Mean { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Mat[] Images { get; private set; } = new Mat[0];
        public int NumComponents { get; set; } = 5;

        public Custom2DPCA(Mat[] images, int numComponents)
        {
            Images = images;
            Width = images[0].Width;
            Height = images[0].Height;
            NumComponents = numComponents;
        }

        public void CalculateModel()
        {
            Mean = new Mat(new Size(Width, Height), MatType.CV_64FC1);
            foreach (var image in Images)
            {
                Cv2.Add(Mean, image, Mean);
            }
            Cv2.Divide(Mean, Images.Length, Mean);

            List<Mat> centered = new List<Mat>();

            foreach (var image in Images)
            {
                centered.Add(image - Mean);
            }

            Mat R = new Mat(new Size(Height, Height), MatType.CV_64FC1);
            Mat C = new Mat(new Size(Width, Width), MatType.CV_64FC1);

            for (int i = 0; i < centered.Count; i++)
            {
                Cv2.Add(R, centered[i] * centered[i].T(), R);
                Cv2.Add(C, centered[i].T() * centered[i], C);
            }

            ReigenValues = new Mat();
            ReigenVectors = new Mat();
            Cv2.Eigen(R, ReigenValues, ReigenVectors);

            CeigenValues = new Mat();
            CeigenVectors = new Mat();
            Cv2.Eigen(C, CeigenValues, CeigenVectors);

            var eigenValues = new long[ReigenValues.Height];
            for (int i = 0; i < ReigenValues.Height; i++)
            {
                var a = ReigenValues.Row(i).Data.ToInt64();
                eigenValues[i] = ReigenValues.Row(i).Data.ToInt64();
            }

            int NumComponents = 5;

            var rIndices = GetTopPIndices(ReigenValues, NumComponents);
            var cIndices = GetTopPIndices(CeigenValues, NumComponents);

            Mat ReduceReigenVectors = CreateReductionMatrixRow(ReigenVectors, rIndices);
            Mat ReduceCeigenVectors = CreateReductionMatrixCol(CeigenVectors, cIndices);
        }

        public void SubSpace(int n)
        {
            //var eigenValues = new long[ReigenValues.Rows];
            //for (int i = 0; i < ReigenValues.Rows; i++)
            //{
            //    var a = ReigenValues.Row(i).Data.ToInt64();
            //    eigenValues[i] = ReigenValues.Row(i).Data.ToInt64();
            //}            

            var rIndices = GetTopPIndices(ReigenValues, n);
            var cIndices = GetTopPIndices(CeigenValues, n);

            ReduceReigenVectors = CreateReductionMatrixRow(ReigenVectors, rIndices);
            ReduceCeigenVectors = CreateReductionMatrixCol(CeigenVectors, cIndices);
        }

        public Mat Project(Mat image)
        {
            image.ConvertTo(image, MatType.CV_64FC1);
            image = image.Resize(new Size(Width,  Height));

            Mat pr = ReduceReigenVectors * image * ReduceCeigenVectors;            
            return pr;
        }

        public Mat ReconstructImage(Mat projection)
        {            
            Mat r = (ReduceReigenVectors.T() * projection * ReduceCeigenVectors.T()) + Mean;

            return r;
        }

        private int[] GetTopPIndices(Mat eigenValues, int p)
        {
            var values = new double[eigenValues.Rows];
            for (int i = 0; i < eigenValues.Rows; i++)
            {
                values[i] = eigenValues.At<double>(i);
            }

            var sortedIndices = values
                .Select((value, index) => new { Value = value, Index = index })
                .OrderBy(x => x.Value)
                .Take(p)
                .Select(x => x.Index)
                .ToArray();

            return sortedIndices;
        }

        private  Mat CreateReductionMatrixRow(Mat eigenVectors, int[] selectedIndices)
        {
            Mat reductionMatrix = new Mat(selectedIndices.Length, eigenVectors.Cols, eigenVectors.Type());

            for (int i = 0; i < selectedIndices.Length; i++)
            {
                Mat row = eigenVectors.Row(selectedIndices[i]);
                row.CopyTo(reductionMatrix.Row(i));
            }

            return reductionMatrix;
        }       

        private Mat CreateReductionMatrixCol(Mat eigenVectors, int[] selectedIndices)
        {
            Mat reductionMatrix = new Mat(eigenVectors.Rows, selectedIndices.Length, eigenVectors.Type());

            for (int i = 0; i < selectedIndices.Length; i++)
            {
                Mat row = eigenVectors.Col(selectedIndices[i]);
                row.CopyTo(reductionMatrix.Col(i));
            }

            return reductionMatrix;
        }
    }
}
