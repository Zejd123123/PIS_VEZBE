using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace biblioteka_vezba
{
    public static class Zaobljeno
    {
        public static GraphicsPath Putanja(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class RoundedPanel : Panel
    {
        public Color FillColor { get; set; } = Color.White;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int Radius { get; set; } = 14;

        public RoundedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = Zaobljeno.Putanja(r, Radius))
            using (var fill = new SolidBrush(FillColor))
            {
                e.Graphics.FillPath(fill, path);
                if (BorderColor != Color.Transparent)
                    using (var pen = new Pen(BorderColor))
                        e.Graphics.DrawPath(pen, path);
            }
        }
    }

    public class RoundedButton : Button
    {
        public Color FillColor { get; set; } = Color.Navy;
        public Color HoverColor { get; set; } = Color.RoyalBlue;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderWidth { get; set; } = 0;
        public int Radius { get; set; } = 14;
        private bool _hover;

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : SystemColors.Control);

            var r = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var path = Zaobljeno.Putanja(r, Radius))
            using (var fill = new SolidBrush(_hover ? HoverColor : FillColor))
            {
                g.FillPath(fill, path);
                if (BorderWidth > 0)
                    using (var pen = new Pen(BorderColor, BorderWidth))
                        g.DrawPath(pen, path);
            }

            const int ik = 28, razmak = 12;
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            Size t = TextRenderer.MeasureText(g, Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            int ukupno = t.Width + (Image != null ? ik + razmak : 0);
            int x = (Width - ukupno) / 2;

            if (Image != null)
            {
                g.DrawImage(Image, new Rectangle(x, (Height - ik) / 2, ik, ik));
                x += ik + razmak;
            }

            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, t.Width + 4, Height), ForeColor, flags);
        }
    }

    public static class TextBoxExtensions
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        public static void SetPlaceholder(this TextBox tb, string tekst)
        {
            SendMessage(tb.Handle, EM_SETCUEBANNER, 1, tekst);
        }
    }
}
