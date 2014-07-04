using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace io.github.charries96.quickshot
{
    public partial class Uploader : Form
    {
        public Boolean drawn = false;
        public Rectangle size;
        private ImgurAPI api;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private NotifyIcon icon;
        private ContextMenu menu;
        private HotkeyManager hkm;

        /// <summary>
        /// Register the PrintScreen hotkey,
        /// Create the ContextMenu and
        /// setup the NotifyIcon.
        /// </summary>
        public Uploader()
        {
            InitializeComponent();

            hkm = new HotkeyManager(this);

            menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("Capture", doCapture));
            menu.MenuItems.Add(new MenuItem("Exit", OnExit));

            icon = new NotifyIcon();
            icon.Text = "Quickshot";
            icon.Icon = this.Icon;

            icon.ContextMenu = menu;
            icon.Visible = true;
            RegisterHotKey(hkm.Handle, 0x0000, 0x0000, Keys.PrintScreen.GetHashCode());
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;

            base.OnLoad(e);
        }

        public void doCapture(object sender, EventArgs args)
        {
            capture();
        }

        /// <summary>
        /// Create a new thread to upload the image on, 
        /// stops the entire application being locked up
        /// until it's complete.
        /// </summary>
        public void capture()
        {
            Thread uploadThread = new Thread(new ThreadStart(upload));
            uploadThread.Start();
        }

        private void Uploader_Load(object sender, EventArgs e)
        {
            api = new ImgurAPI(Properties.Settings.Default.CLIENT_ID);
        }

        /// <summary>
        /// Upload the image to Imgur using the ImgurAPI class.
        /// </summary>
        private void upload()
        {
            Image image = TakeScreenshot();
            if(drawn)
                updateUrl(api.UploadImage(image));
        }

        /// <summary>
        /// Grab the entire screen locally then display the 
        /// ImageDisplay form to let users select the area
        /// to be cropped.
        /// </summary>
        /// <returns>PrintScreen as Image</returns>
        private Image TakeScreenshot()
        {
            ScreenCapture sc = new ScreenCapture();
            Image image = sc.CaptureScreen();
            ImageDisplay display = new ImageDisplay(this, image);
            display.TopMost = true;
            display.ShowDialog();

            return crop(image);
        }

        /// <summary>
        /// Crop the image based on what the ImageDisplay class was told.
        /// </summary>
        /// <param name="image">Image to crop</param>
        /// <returns>Cropped Image as a Bitmap</returns>
        public Bitmap crop(Image image)
        {
            try
            {
                Bitmap target = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height), size, GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// (Thread-safe) Set the TextBox value.
        /// </summary>
        /// <param name="value">Value to set</param>
        public void updateUrl(String value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<String>(updateUrl), new object[] { value });
                return;
            }
            url.Text = value;
            drawn = false;
            size = new Rectangle(0, 0, 1, 1);
            this.Show();
        }

        private void copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(url.Text);
            this.Hide();
        }

        private void OnExit(object sender, EventArgs e)
        {
            UnregisterHotKey(hkm.Handle, 0);
            hkm.Dispose();
            Application.Exit();
        }
    }

    public sealed class HotkeyManager : NativeWindow, IDisposable
    {
        private Uploader uploader;
        public HotkeyManager(Uploader u)
        {
            CreateHandle(new CreateParams());
            uploader = u;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                uploader.capture();
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}