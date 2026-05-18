using System;
using System.Drawing;
using System.Windows.Forms;

namespace InvoicesManager
{
    internal static class UiStyle
    {
        internal static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
        internal static readonly Color Surface = Color.White;
        internal static readonly Color SurfaceMuted = Color.FromArgb(226, 232, 240);
        internal static readonly Color Header = Color.FromArgb(15, 23, 42);
        internal static readonly Color HeaderText = Color.FromArgb(226, 232, 240);
        internal static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
        internal static readonly Color TextMuted = Color.FromArgb(71, 85, 105);

        internal static readonly Color Accent = Color.FromArgb(14, 116, 144);
        internal static readonly Color AccentAlt = Color.FromArgb(37, 99, 235);
        internal static readonly Color Success = Color.FromArgb(22, 163, 74);
        internal static readonly Color Danger = Color.FromArgb(220, 38, 38);

        internal static readonly Font TitleFont = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
        internal static readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        internal static readonly Font ButtonFont = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);

        internal static void StyleActionButton(Button button, Color backgroundColor)
        {
            button.BackColor = backgroundColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(12, 0, 0, 0);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        internal static void AddHoverEffect(Button button)
        {
            if (button == null)
            {
                return;
            }

            Color baseColor = button.BackColor;
            button.MouseEnter += (sender, args) => button.BackColor = ShiftColor(baseColor, 16);
            button.MouseLeave += (sender, args) => button.BackColor = baseColor;
        }

        internal static void NormalizeButtonIcon(Button button, int size = 24)
        {
            if (button?.Image == null)
            {
                return;
            }

            button.Image = ResizeImage(button.Image, size, size);
        }

        internal static void NormalizePictureIcon(PictureBox pictureBox, int size = 34)
        {
            if (pictureBox?.Image == null)
            {
                return;
            }

            pictureBox.Image = ResizeImage(pictureBox.Image, size, size);
            pictureBox.Size = new Size(size + 2, size + 2);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private static Image ResizeImage(Image source, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(source, 0, 0, width, height);
            }

            return resized;
        }

        private static Color ShiftColor(Color color, int delta)
        {
            int r = Math.Max(0, Math.Min(255, color.R + delta));
            int g = Math.Max(0, Math.Min(255, color.G + delta));
            int b = Math.Max(0, Math.Min(255, color.B + delta));
            return Color.FromArgb(r, g, b);
        }
    }
}
