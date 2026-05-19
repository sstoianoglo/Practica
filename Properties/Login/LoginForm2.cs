using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using PracticaApp.Properties;
using PracticaApp.Properties.Autorizare;

namespace PracticaApp.Properties.Login
{
    public partial class LoginForm2 : Form
    {
        private readonly Image? originalBackgroundImage;
        private Button? backButton;
        private Button? themeButton;

        public LoginForm2()
        {
            InitializeComponent();
            originalBackgroundImage = BackgroundImage;
            this.WindowState = FormWindowState.Maximized;
            SetupBackButton();
            SetupThemeButton();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            
            AboutUs.Left = + 120;
            OurMission.Left = + 1420;




        }

        private void SetupBackButton()
        {
            backButton = new Button
            {
                Text = string.Empty,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ThemeManager.Surface,
                FlatStyle = FlatStyle.Flat,
                ForeColor = ThemeManager.Accent,
                Location = new Point(24, 24),
                Size = new Size(54, 44),
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            backButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            backButton.FlatAppearance.BorderSize = 1;
            backButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
            backButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
            backButton.Click += BackButton_Click;
            backButton.Paint += BackButton_Paint;

            Controls.Add(backButton);
            backButton.BringToFront();
        }

        private void SetupThemeButton()
        {
            themeButton = ThemeManager.CreateThemeButton(ThemeButton_Click);
            Controls.Add(themeButton);
            AlignThemeButton();
            themeButton.BringToFront();
        }

        private void AlignThemeButton()
        {
            if (themeButton == null)
                return;

            themeButton.Left = ClientSize.Width - themeButton.Width - 24;
            themeButton.Top = 24;
            themeButton.BringToFront();
        }

        private void ThemeButton_Click(object? sender, EventArgs e)
        {
            ThemeManager.Toggle();
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackgroundImage = ThemeManager.IsDark ? originalBackgroundImage : null;
            BackColor = ThemeManager.Background;

            Welcome.ForeColor = ThemeManager.Text;
            MainLogin.ForeColor = ThemeManager.Text;
            MainPassword.ForeColor = ThemeManager.Text;
            AboutUs.ForeColor = ThemeManager.Text;
            OurMission.ForeColor = ThemeManager.Text;

            Login.BackColor = ThemeManager.InputBack;
            Login.ForeColor = ThemeManager.Text;
            Password.BackColor = ThemeManager.InputBack;
            Password.ForeColor = ThemeManager.Text;

            LogButton.BackColor = ThemeManager.Accent;
            LogButton.ForeColor = Color.White;

            if (backButton != null)
            {
                ThemeManager.StyleBackButton(backButton);
                backButton.Invalidate();
            }

            if (themeButton != null)
                ThemeManager.StyleThemeButton(themeButton);
        }

        private void BackButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int centerX = button.ClientSize.Width / 2;
            int centerY = button.ClientSize.Height / 2;

            using Pen pen = new Pen(ThemeManager.Accent, 3)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            e.Graphics.DrawLine(pen, centerX + 6, centerY - 10, centerX - 5, centerY);
            e.Graphics.DrawLine(pen, centerX - 5, centerY, centerX + 6, centerY + 10);
        }

        private void BackButton_Click(object? sender, EventArgs e)
        {
            Hide();

            LoginForm1 loginForm = new LoginForm1();
            loginForm.Show();
        }

        private void LoginForm2_Load(object sender, EventArgs e)
        {
            CenterPanel();
        }

        private void CenterPanel()
        {
            PanelLogin.Left = (this.ClientSize.Width - PanelLogin.Width) / 2;
            PanelLogin.Top = (this.ClientSize.Height - PanelLogin.Height) / 2;

        }

        private void LoginForm2_Resize(object sender, EventArgs e)
        {
            CenterPanel();
            AlignThemeButton();
        }

        private void LogButton_Click(object sender, EventArgs e)
        {
            if (Login.Text == "" || Password.Text == "")
            {
                MessageBox.Show("Fill all fields!");
                return;
            }

            Authorization.Authorization1(Login.Text, Password.Text);

            if (Authorization.Role == null)
            {
                MessageBox.Show("Incorrect login or password");
                return;
            }

            if (string.Equals(Authorization.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Welcome admin!");
                this.Hide();

                AdminForm admin = new AdminForm();
                admin.Show();
            }
            else if (string.Equals(Authorization.Role, "user", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Welcome user!");
                this.Hide();

                UserForm exercisesForm = new UserForm();
                exercisesForm.Show();
            }
        }

        private void LogButton_MouseEnter(object sender, EventArgs e)
        {
            LogButton.BackColor = Color.Orange;
            LogButton.ForeColor = Color.White;
        }

        private void LogButton_MouseLeave(object sender, EventArgs e)
        {
            LogButton.BackColor = ThemeManager.Accent;
            LogButton.ForeColor = Color.White;
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            Password.Multiline = false;
            Password.UseSystemPasswordChar = true;
        }
    }
}
