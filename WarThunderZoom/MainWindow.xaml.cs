using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WarThunderZoom
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private bool isVisible = true;
        private ScreenCapture screenCapture;
        private GlobalKeyboardHook hook;
        private bool isTogglingVisibility = false;

        public MainWindow()
        {
            InitializeComponent();


            screenCapture = new ScreenCapture();

            // Initialize and start the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20); // Capture interval
            timer.Tick += Timer_Tick;
            timer.Start();

            

            // Set up global key listener
            hook = new GlobalKeyboardHook();
            hook.KeyboardPressed += OnKeyPressed;
            hook.StartListening();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Set the position of the window after it is rendered (offset to the right and up)
            this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2) + 350; // Offset right by 200
            this.Top = (SystemParameters.WorkArea.Height / 2) - (this.Height / 2) - 200; // Offset up by 100
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            var left = (SystemParameters.WorkArea.Width / 2);
            var top = (SystemParameters.WorkArea.Height / 2);


            //get the center of the screen
            left = left - 56;
            top = top - 50;


            // Define the area to capture (X, Y, Width, Height)
            Rectangle captureRectangle = new Rectangle((int)left, (int)top, 112, 200);

            // Capture the screen
            Bitmap bitmap = screenCapture.CaptureScreen(captureRectangle);

            // Display the bitmap in the Image control
            previewImage.Source = BitmapToImageSource(bitmap);

            // Dispose of the bitmap object
            bitmap.Dispose();
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardData.VirtualCode == (int)VirtualKeyCode.F9 && !isTogglingVisibility) // F9 to toggle visibility
            {
                isTogglingVisibility = true;
                ToggleVisibility();
                e.Handled = true;

                // Debounce logic
                DispatcherTimer debounceTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(300) // Adjust the interval as needed
                };
                debounceTimer.Tick += (s, args) =>
                {
                    isTogglingVisibility = false;
                    debounceTimer.Stop();
                };
                debounceTimer.Start();
            }
        }

        private void ToggleVisibility()
        {
            if (isVisible)
            {
                this.Hide();
                isVisible = false;
            }
            else
            {
                this.Show();
                isVisible = true;
            }
        }

        private BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}