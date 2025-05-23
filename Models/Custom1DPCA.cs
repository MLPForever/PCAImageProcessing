using OpenCvSharp;
using System.IO;

namespace PCAImageProcessing.Models
{
    class Custom1DPCA
    {
        public Mat EigenVectors { get; private set; }
        public Mat EigenValues { get; private set; }
        public Mat ReduceEigenVectors { get; private set; }
        public Mat ReduceSubSpace { get; private set; }
        public Mat Mean { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Mat[] Images { get; private set; } = new Mat[0];
        public Mat DMat { get; private set; }
        public int NumComponents { get; set; } = 5;
        private readonly string _modelPath;
        private readonly string _imagesPath;

        public Custom1DPCA(Mat[] images, int numComponents, string imagesPath, string modelPath)
        {
            Images = images;
            Width = images[0].Width;
            Height = images[0].Height;
            NumComponents = numComponents;
            _modelPath = modelPath;
            _imagesPath = imagesPath;
        }

        public void LoadModel()
        {
            byte[] loadedBytesEigenVectors = File.ReadAllBytes(_modelPath + "\\EigenVectors.bin");
            EigenVectors = new Mat(Width * Height, Width * Height, MatType.CV_64F);
            System.Runtime.InteropServices.Marshal.Copy(loadedBytesEigenVectors, 0, EigenVectors.Data, loadedBytesEigenVectors.Length);

            byte[] loadedBytesEigenValues = File.ReadAllBytes(_modelPath + "\\EigenValues.bin");
            EigenValues = new Mat(Width * Height, 1, MatType.CV_64F);
            System.Runtime.InteropServices.Marshal.Copy(loadedBytesEigenValues, 0, EigenValues.Data, loadedBytesEigenValues.Length);

            byte[] loadedBytesMean = File.ReadAllBytes(_modelPath + "\\Mean.bin");
            Mean = new Mat(new Size(Width, Height), MatType.CV_64F);
            System.Runtime.InteropServices.Marshal.Copy(loadedBytesMean, 0, Mean.Data, loadedBytesMean.Length);

            ReshaperToDim();
            SubSpace();
        }

        public void SaveModel()
        {
            byte[] rawBytesEigenVectors = new byte[EigenVectors.Total() * EigenVectors.ElemSize()];
            System.Runtime.InteropServices.Marshal.Copy(EigenVectors.Data, rawBytesEigenVectors, 0, rawBytesEigenVectors.Length);
            File.WriteAllBytes(_modelPath + "\\EigenVectors.bin", rawBytesEigenVectors);

            byte[] rawBytesEigenValues = new byte[EigenValues.Total() * EigenValues.ElemSize()];
            System.Runtime.InteropServices.Marshal.Copy(EigenValues.Data, rawBytesEigenValues, 0, rawBytesEigenValues.Length);
            File.WriteAllBytes(_modelPath + "\\EigenValues.bin", rawBytesEigenValues);

            byte[] rawBytesMean = new byte[Mean.Total() * Mean.ElemSize()];
            System.Runtime.InteropServices.Marshal.Copy(Mean.Data, rawBytesMean, 0, rawBytesMean.Length);
            File.WriteAllBytes(_modelPath + "\\Mean.bin", rawBytesMean);
        }

        public void CalculateModel()
        {

            Mean = new Mat(new Size(Width, Height), MatType.CV_64F);
            foreach (var image in Images)
            {
                Cv2.Add(Mean, image, Mean);
            }
            Cv2.Divide(Mean, Images.Length, Mean);

            ReshaperToDim();

            Mat covar = new Mat();
            Mat meanCovar = new Mat();
            Cv2.CalcCovarMatrix(DMat, covar, meanCovar, CovarFlags.Scale | CovarFlags.Rows);

            EigenValues = new Mat();
            EigenVectors = new Mat();
            Cv2.Eigen(covar, EigenValues, EigenVectors);

            SaveModel();
            
        }

        public long[] GetEigenValues()
        {
            var eigenValues = new long[200];
            for (int i = 0; i < 200; i++)
            {
                var a = EigenValues.Row(i).Data.ToInt64();
                eigenValues[i] = EigenValues.Row(i).Data.ToInt64();
            }
            return eigenValues;
        }

        public void ReshaperToDim()
        {
            List<Mat> centered = new List<Mat>();

            foreach (var image in Images)
            {
                centered.Add(image - Mean);
            }

            Mat dMat = new Mat(new Size(0, Width * Height), MatType.CV_64F);
            foreach (var mat in centered)
            {
                var temp = mat.Reshape(0, Width * Height);
                Cv2.HConcat(dMat, temp, dMat);
                temp.Dispose();
            }

            DMat = dMat;
        }

        private void SubSpace()
        {
            //ReduceEigenVectors = EigenVectors.RowRange(0, NumComponents).Clone();
            //ReduceSubSpace = ReduceEigenVectors * DMat;

            var rIndices = GetTopPIndices(EigenValues, NumComponents);
            ReduceEigenVectors = CreateReductionMatrixRow(EigenVectors, rIndices);

            ReduceSubSpace = ReduceEigenVectors * DMat;
        }

        public void SubSpace(int n)
        {
            //ReduceEigenVectors = EigenVectors.RowRange(0, n).Clone();
            //ReduceSubSpace = ReduceEigenVectors * DMat;

            var rIndices = GetTopPIndices(EigenValues, n);
            ReduceEigenVectors = CreateReductionMatrixRow(EigenVectors, rIndices);

            ReduceSubSpace = ReduceEigenVectors * DMat;
        }     

        public Mat ReconstructImage(Mat projection)
        {
            Mat r = (projection * ReduceEigenVectors).ToMat().Reshape(1, Height) + Mean;
            return r;
        }

        public Mat Project(Mat image)
        {
            Mat pr = (image - Mean).ToMat().Reshape(0, Width * Height).T() * ReduceEigenVectors.T();
            return pr;
        }

        public Mat ShowEigenFaces(int i,int imgWidth, int imgHeight)
        {
            Mat eigenFace = EigenVectors.Row(i).Reshape(1, imgHeight);

            Mat normalized = new Mat();
            Cv2.Normalize(eigenFace, normalized, 0, 255, NormTypes.MinMax);
            normalized.ConvertTo(normalized, MatType.CV_8U);
            normalized = normalized.Resize(new Size(100, 100));

            return normalized;
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
                .OrderByDescending(x => x.Value)
                .Take(p)
                .Select(x => x.Index)
                .ToArray();

            return sortedIndices;
        }

        private Mat CreateReductionMatrixRow(Mat eigenVectors, int[] selectedIndices)
        {
            Mat reductionMatrix = new Mat(selectedIndices.Length, eigenVectors.Cols, eigenVectors.Type());

            for (int i = 0; i < selectedIndices.Length; i++)
            {
                Mat row = eigenVectors.Row(selectedIndices[i]);
                row.CopyTo(reductionMatrix.Row(i));
            }

            return reductionMatrix;
        }
    }
}
