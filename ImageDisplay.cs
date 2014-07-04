using System.Drawing;
using System.Windows.Forms;

namespace io.github.charries96.quickshot
{
    public partial class ImageDisplay : Form
    {
        private Rectangle rec = new Rectangle(0, 0, 0, 0);
        private Uploader uploader;

        public ImageDisplay(Uploader uploader, Image image)
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.uploader = uploader;
            this.BackgroundImage = image;
        }

        private void ImageDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rec = new Rectangle(e.X, e.Y, 0, 0);
            }
        }

        private void ImageDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                rec.Width = e.X - rec.X;
                rec.Height = e.Y - rec.Y;
                Invalidate();
            }
        }

        private void ImageDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            uploader.drawn = true;
            uploader.size = rec;
            this.Close();
        }

        private void ImageDisplay_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black, 2);
            e.Graphics.DrawRectangle(pen, rec);
        }
    }
}