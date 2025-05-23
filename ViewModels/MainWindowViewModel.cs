using BaseTemplateForWPF;
using LiveCharts;
using OpenCvSharp;
using PCAImageProcessing.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PCAImageProcessing.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly string _modelPath = System.AppDomain.CurrentDomain.BaseDirectory + "Resources\\Model";
        private readonly string _imagesPath = System.AppDomain.CurrentDomain.BaseDirectory + "Resources\\Images";
        private Custom1DPCA _1dPCA;
        private Custom2DPCA _2dPCA;
        public MainWindowViewModel()
        {
            LoadedCommand = new LambdaCommand(OnLoadedCommandExecute, CanLoadedCommandExecuted);
            PCA1SelectImageCommand = new LambdaCommand(OnPCA1SelectImageCommandExecute, CanPCA1SelectImageCommandExecuted);
            PCA2SelectImageCommand = new LambdaCommand(OnPCA2SelectImageCommandExecute, CanPCA2SelectImageCommandExecuted);
        }

        #region Title 

        private string _Title = "PCAImageProcessing";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        #endregion

        #region LoadVisisbility 

        private Visibility _LoadVisisbility = Visibility.Hidden;
        public Visibility LoadVisisbility
        {
            get => _LoadVisisbility;
            set => Set(ref _LoadVisisbility, value);
        }

        #endregion

        #region ContentEnabled 

        private bool _ContentEnabled = true;
        public bool ContentEnabled
        {
            get => _ContentEnabled;
            set => Set(ref _ContentEnabled, value);
        }

        #endregion

        #region PCA1DComponentCount 

        private int _PCA1DComponentCount = 5;
        public int PCA1DComponentCount
        {
            get => _PCA1DComponentCount;
            set => Set(ref _PCA1DComponentCount, value);
        }

        #endregion

        #region PCA1DMSE 

        private double _PCA1DMSE;
        public double PCA1DMSE
        {
            get => _PCA1DMSE;
            set => Set(ref _PCA1DMSE, value);
        }

        #endregion

        #region MatImages 

        private Mat[] _MatImages = new Mat[0];
        public Mat[] MatImages
        {
            get => _MatImages;
            set => Set(ref _MatImages, value);
        }

        #endregion

        #region BitmapImages 

        private List<BitmapImage> _BitmapImages = new List<BitmapImage>();
        public List<BitmapImage> BitmapImages
        {
            get => _BitmapImages;
            set
            {
                Set(ref _BitmapImages, value);
            }
        }

        #endregion

        #region PCA1DSelectBitmapImageIndex 

        private int _PCA1DSelectBitmapImageIndex = 0;
        public int PCA1DSelectBitmapImageIndex
        {
            get => _PCA1DSelectBitmapImageIndex;
            set
            {
                Set(ref _PCA1DSelectBitmapImageIndex, value);
                PCA1DProcessRecinstruct();
                PCA1SelectedBitmapImage = BitmapImages[value];
            }
        }

        #endregion

        #region PCA1SelectedBitmapImage 

        private BitmapImage _PCA1SelectedBitmapImage;
        public BitmapImage PCA1SelectedBitmapImage
        {
            get => _PCA1SelectedBitmapImage;
            set => Set(ref _PCA1SelectedBitmapImage, value);
        }

        #endregion

        #region PCA1DReconstructedImage 

        private BitmapImage _PCA1DReconstructedImage;
        public BitmapImage PCA1DReconstructedImage
        {
            get => _PCA1DReconstructedImage;
            set => Set(ref _PCA1DReconstructedImage, value);
        }

        #endregion

        #region PCA1DFirstEigenFaces 

        private List<BitmapImage> _PCA1DFirstEigenFaces = new List<BitmapImage>(); 
        public List<BitmapImage> PCA1DFirstEigenFaces
        {
            get => _PCA1DFirstEigenFaces;
            set => Set(ref _PCA1DFirstEigenFaces, value);
        }

        #endregion

        #region PCA1DEigenValues 

        private SeriesCollection _PCA1DEigenValues = new SeriesCollection();
        public SeriesCollection PCA1DEigenValues
        {
            get => _PCA1DEigenValues;
            set => Set(ref _PCA1DEigenValues, value);
        }

        #endregion

        #region PCA2DComponentCount 

        private int _PCA2DComponentCount = 5;
        public int PCA2DComponentCount
        {
            get => _PCA2DComponentCount;
            set => Set(ref _PCA2DComponentCount, value);
        }

        #endregion

        #region PCA2DMSE 

        private double _PCA2DMSE;
        public double PCA2DMSE
        {
            get => _PCA2DMSE;
            set => Set(ref _PCA2DMSE, value);
        }

        #endregion

        #region PCA2DSelectBitmapImageIndex 

        private int _PCA2DSelectBitmapImageIndex = 0;
        public int PCA2DSelectBitmapImageIndex
        {
            get => _PCA2DSelectBitmapImageIndex;
            set
            {
                Set(ref _PCA2DSelectBitmapImageIndex, value);
                PCA2DProcessRecinstruct();
                PCA2SelectedBitmapImage = BitmapImages[value];
            }
        }

        #endregion

        #region PCA2SelectedBitmapImage 

        private BitmapImage _PCA2SelectedBitmapImage;
        public BitmapImage PCA2SelectedBitmapImage
        {
            get => _PCA2SelectedBitmapImage;
            set => Set(ref _PCA2SelectedBitmapImage, value);
        }

        #endregion

        #region PCA2SubSpaceImage 

        private BitmapImage _PCA2SubSpaceImage;
        public BitmapImage PCA2SubSpaceImage
        {
            get => _PCA2SubSpaceImage;
            set => Set(ref _PCA2SubSpaceImage, value);
        }

        #endregion

        #region PCA2DReconstructedImage 

        private BitmapImage _PCA2DReconstructedImage;
        public BitmapImage PCA2DReconstructedImage
        {
            get => _PCA2DReconstructedImage;
            set => Set(ref _PCA2DReconstructedImage, value);
        }

        #endregion

        #region Commands

        #region PCA1SelectImageCommand
        public ICommand PCA1SelectImageCommand { get; }
        private void OnPCA1SelectImageCommandExecute(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpg)|*.jpg";
            if (dlg.ShowDialog() == true)
            {                
                var img = Cv2.ImRead(dlg.FileName, ImreadModes.Grayscale);
                img = img.Resize(new OpenCvSharp.Size(_1dPCA.Width, _1dPCA.Height));
                img.ConvertTo(img, MatType.CV_64F);

                _1dPCA.SubSpace(PCA1DComponentCount);
                var pr = _1dPCA.Project(img);
                var r = _1dPCA.ReconstructImage(pr);
                PCA1DReconstructedImage = ImageHelper.MatToBitmapImage(r);
                PCA1SelectedBitmapImage = ImageHelper.MatToBitmapImage(img);
                PCA1DMSE = ImageHelper.CalculateMSE(img.Clone(), r.Clone());
            }
        }
        private bool CanPCA1SelectImageCommandExecuted(object p) => true;
        #endregion

        #region PCA2SelectImageCommand
        public ICommand PCA2SelectImageCommand { get; }
        private void OnPCA2SelectImageCommandExecute(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Files (*.jpg)|*.jpg";
            if (dlg.ShowDialog() == true)
            {
                var img = Cv2.ImRead(dlg.FileName, ImreadModes.Grayscale);
                img = img.Resize(new OpenCvSharp.Size(_2dPCA.Width, _2dPCA.Height));
                img.ConvertTo(img, MatType.CV_64F);

                _2dPCA.SubSpace(PCA2DComponentCount);
                var pr = _2dPCA.Project(img);
                var r = _2dPCA.ReconstructImage(pr);
                PCA2DReconstructedImage = ImageHelper.MatToBitmapImage(r);
                PCA2SelectedBitmapImage = ImageHelper.MatToBitmapImage(img);
                PCA2SubSpaceImage = ImageHelper.MatToBitmapImage(pr);
                PCA2DMSE = ImageHelper.CalculateMSE(img.Clone(), r.Clone());
            }
        }
        private bool CanPCA2SelectImageCommandExecuted(object p) => true;
        #endregion

        #region LoadedCommand
        public ICommand LoadedCommand { get; }
        private async void OnLoadedCommandExecute(object p)
        {
            await Task.Run(() => 
            {
                TurnToLoadMode();

                MatImages = ImageHelper.LoadImages(_imagesPath);
                var bitmapImages = new List<BitmapImage>();
                for (int i = 0; i < MatImages.Length; i++)
                {                    
                    Application.Current.Dispatcher.Invoke(() => bitmapImages.Add(ImageHelper.MatToBitmapImage(MatImages[i])));
                    MatImages[i].ConvertTo(MatImages[i], MatType.CV_64F);
                }
                Application.Current.Dispatcher.Invoke(() => BitmapImages = bitmapImages);

                _1dPCA = new Custom1DPCA(MatImages, PCA1DComponentCount, _imagesPath, _modelPath);
                _1dPCA.LoadModel();

                var pca1dFirstEigenFaces = new List<BitmapImage>();
                for (int i = 0; i < 10; i++)
                {
                    var eigenFace = _1dPCA.ShowEigenFaces(i, _1dPCA.Width, _1dPCA.Height);
                    Application.Current.Dispatcher.Invoke(() => pca1dFirstEigenFaces.Add(ImageHelper.MatToBitmapImage(eigenFace)));
                }
                Application.Current.Dispatcher.Invoke(() => PCA1DFirstEigenFaces = pca1dFirstEigenFaces);

                _2dPCA = new Custom2DPCA(MatImages, PCA2DComponentCount);
                _2dPCA.CalculateModel();

                //Application.Current.Dispatcher.Invoke(() => PCA1DEigenValues = new SeriesCollection
                //{
                //    new ColumnSeries
                //    {
                //        Values = new ChartValues<long>(_1dPCA.GetEigenValues())
                //    }
                //});

                TurnToDefaultMode();
            });
        }
        private bool CanLoadedCommandExecuted(object p) => true;
        #endregion

        #endregion

        #region Methods

        private void PCA2DProcessRecinstruct()
        {
            _2dPCA.SubSpace(PCA2DComponentCount);
            var pr = _2dPCA.Project(MatImages[PCA2DSelectBitmapImageIndex]);
            Mat prI = pr.Clone();
            
            var r = _2dPCA.ReconstructImage(pr);
            PCA2DMSE = ImageHelper.CalculateMSE(MatImages[PCA2DSelectBitmapImageIndex].Clone(), r.Clone());
            Mat rI = r.Clone();            

            prI = prI.Resize(new OpenCvSharp.Size(300, 300), interpolation: InterpolationFlags.Nearest);
            prI.ConvertTo(prI, MatType.CV_8UC1);

            PCA2SubSpaceImage = ImageHelper.MatToBitmapImage(prI);            
            PCA2DReconstructedImage = ImageHelper.MatToBitmapImage(rI);
        }

        private void PCA1DProcessRecinstruct()
        {
            _1dPCA.SubSpace(PCA1DComponentCount);
            var pr = _1dPCA.Project(MatImages[PCA1DSelectBitmapImageIndex]);
            var r = _1dPCA.ReconstructImage(pr);
            PCA1DReconstructedImage = ImageHelper.MatToBitmapImage(r);
            PCA1DMSE = ImageHelper.CalculateMSE(MatImages[PCA1DSelectBitmapImageIndex].Clone(), r.Clone());
        }

        private void TurnToLoadMode()
        {
            LoadVisisbility = Visibility.Visible;
            ContentEnabled = false;
        }

        private void TurnToDefaultMode()
        {
            LoadVisisbility = Visibility.Hidden;
            ContentEnabled = true;
        }

        #endregion
    }
}
