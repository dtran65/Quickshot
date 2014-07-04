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

        public void capture()
        {
            Thread uploadThread = new Thread(new ThreadStart(upload));
            uploadThread.Start();
        }

        private void Uploader_Load(object sender, EventArgs e)
        {
            api = new ImgurAPI(Properties.Settings.Default.API_KEY);
        }

        private void upload()
        {
            updateUrl(api.UploadImage(TakeScreenshot()));
        }

        private Image TakeScreenshot()
        {
            ScreenCapture sc = new ScreenCapture();
            Image image = sc.CaptureScreen();
            ImageDisplay display = new ImageDisplay(this, image);
            display.TopMost = true;
            display.ShowDialog();

            return crop(image);
        }

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
#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
                return null;
            }
        }

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
            UnregisterHotKey(this.Handle, 0);
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