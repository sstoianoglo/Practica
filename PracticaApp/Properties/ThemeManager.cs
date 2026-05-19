using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PracticaApp.Properties
{
    internal static class ThemeManager
    {
        public static event EventHandler? ThemeChanged;

        public static bool IsDark { get; private set; } = true;

        public static Color Accent => Color.DarkOrange;
        public static Color Background => IsDark ? Color.Black : Color.FromArgb(246, 248, 252);
        public static Color Surface => IsDark ? Color.FromArgb(32, 32, 37) : Color.White;
        public static Color SurfaceHover => IsDark ? Color.FromArgb(42, 34, 24) : Color.FromArgb(255, 242, 224);
        public static Color SurfaceDown => IsDark ? Color.FromArgb(54, 38, 18) : Color.FromArgb(255, 229, 190);
        public static Color Text => IsDark ? Color.White : Color.FromArgb(24, 28, 36);
        public static Color MutedText => IsDark ? Color.FromArgb(92, 92, 100) : Color.FromArgb(92, 98, 112);
        public static Color InputBack => IsDark ? Color.FromArgb(44, 44, 52) : Color.FromArgb(250, 252, 255);
        public static Color Border => IsDark ? Color.FromArgb(85, 85, 95) : Color.FromArgb(180, 186, 198);
        public static Color LikedCard => IsDark ? Color.FromArgb(58, 18, 28) : Color.FromArgb(255, 235, 241);
        public static Color LikedHeart => Color.FromArgb(220, 45, 65);

        public static void Toggle()
        {
            IsDark = !IsDark;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static Button CreateThemeButton(EventHandler clickHandler)
        {
            Button button = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Symbol", 16F, FontStyle.Bold),
                Location = new Point(0, 24),
                Size = new Size(48, 42),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 1;
            button.Click += clickHandler;
            button.Paint += ThemeButton_Paint;
            StyleThemeButton(button);

            return button;
        }

        public static void StyleThemeButton(Button button)
        {
            button.Text = "";
            button.AccessibleName = IsDark ? "Switch to light mode" : "Switch to dark mode";
            button.BackColor = Surface;
            button.ForeColor = Accent;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.MouseOverBackColor = SurfaceHover;
            button.FlatAppearance.MouseDownBackColor = SurfaceDown;
            button.Invalidate();
        }

        private static void ThemeButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen borderPen = new Pen(Accent, 1);
            e.Graphics.DrawRectangle(borderPen, 0, 0, button.Width - 1, button.Height - 1);

            if (IsDark)
                DrawSunIcon(e.Graphics, button.ClientRectangle);
            else
                DrawMoonIcon(e.Graphics, button.ClientRectangle);
        }

        private static void DrawSunIcon(Graphics graphics, Rectangle bounds)
        {
            Point center = new Point(bounds.Width / 2, bounds.Height / 2);

            using Pen pen = new Pen(Accent, 2.2F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            using SolidBrush brush = new SolidBrush(Accent);

            graphics.FillEllipse(brush, center.X - 5, center.Y - 5, 10, 10);

            for (int angle = 0; angle < 360; angle += 45)
            {
                double radians = Math.PI * angle / 180.0;
                int innerX = center.X + (int)Math.Round(Math.Cos(radians) * 10);
                int innerY = center.Y + (int)Math.Round(Math.Sin(radians) * 10);
                int outerX = center.X + (int)Math.Round(Math.Cos(radians) * 14);
                int outerY = center.Y + (int)Math.Round(Math.Sin(radians) * 14);
                graphics.DrawLine(pen, innerX, innerY, outerX, outerY);
            }
        }

        private static void DrawMoonIcon(Graphics graphics, Rectangle bounds)
        {
            Point center = new Point(bounds.Width / 2, bounds.Height / 2);

            using SolidBrush moonBrush = new SolidBrush(Accent);
            using SolidBrush cutoutBrush = new SolidBrush(Surface);

            graphics.FillEllipse(moonBrush, center.X - 9, center.Y - 10, 18, 20);
            graphics.FillEllipse(cutoutBrush, center.X - 2, center.Y - 12, 18, 22);
        }

        public static void StyleBackButton(Button button)
        {
            button.BackColor = Surface;
            button.ForeColor = Accent;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.MouseOverBackColor = SurfaceHover;
            button.FlatAppearance.MouseDownBackColor = SurfaceDown;
        }
    }
}
