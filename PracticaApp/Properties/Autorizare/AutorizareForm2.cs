using MySql.Data.MySqlClient;
using PracticaApp.Properties;
using PracticaApp.Properties.Autorizare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PracticaApp
{
    public partial class AutorizareForm2 : Form
    {
        static public string loginActive;
        static public string whois;
        private readonly Image? originalBackgroundImage;
        private Button? backButton;
        private Button? themeButton;
        private Button? googleRegisterButton;

        public AutorizareForm2()
        {
            InitializeComponent();
            originalBackgroundImage = BackgroundImage;

            this.RegisterName.AutoSize = false;
            this.RegisterName.Size = new Size(this.RegisterName.Size.Width, 52);
            this.RegisterName.UseSystemPasswordChar = false;
            this.WindowState = FormWindowState.Maximized;
            SetupBackButton();
            SetupThemeButton();
            SetupGoogleRegisterButton();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
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

            CreateAccount.ForeColor = ThemeManager.Text;
            label1.ForeColor = ThemeManager.Text;
            label2.ForeColor = ThemeManager.Text;
            label3.ForeColor = ThemeManager.Text;
            label4.ForeColor = ThemeManager.Text;
            label5.ForeColor = ThemeManager.MutedText;
            checkBox1.ForeColor = ThemeManager.Text;
            LogInButton.ForeColor = Color.RoyalBlue;

            RegisterName.BackColor = ThemeManager.InputBack;
            RegisterName.ForeColor = ThemeManager.Text;
            RegisterPassword.BackColor = ThemeManager.InputBack;
            RegisterPassword.ForeColor = ThemeManager.Text;
            RegisterEmail.BackColor = ThemeManager.InputBack;
            RegisterEmail.ForeColor = ThemeManager.Text;
            RegisterNumber.BackColor = ThemeManager.InputBack;
            RegisterNumber.ForeColor = ThemeManager.Text;

            button1.BackColor = ThemeManager.IsDark ? Color.White : ThemeManager.Accent;
            button1.ForeColor = ThemeManager.IsDark ? Color.Black : Color.White;

            if (googleRegisterButton != null)
            {
                googleRegisterButton.BackColor = ThemeManager.InputBack;
                googleRegisterButton.ForeColor = ThemeManager.Accent;
                googleRegisterButton.FlatAppearance.BorderColor = ThemeManager.Accent;
                googleRegisterButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
                googleRegisterButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
            }

            if (backButton != null)
            {
                ThemeManager.StyleBackButton(backButton);
                backButton.Invalidate();
            }

            if (themeButton != null)
                ThemeManager.StyleThemeButton(themeButton);
        }

        private void SetupGoogleRegisterButton()
        {
            googleRegisterButton = new Button
            {
                Text = "Continue with Google",
                Location = new Point(144, 660),
                Size = new Size(308, 36),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Toledo", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            googleRegisterButton.FlatAppearance.BorderSize = 1;
            googleRegisterButton.Click += GoogleRegisterButton_Click;
            CrearePanel.Controls.Add(googleRegisterButton);
            googleRegisterButton.BringToFront();
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

        private void CenterPanel()
        {
            CrearePanel.Left = (this.ClientSize.Width - CrearePanel.Width) / 2;
            CrearePanel.Top = (this.ClientSize.Height - CrearePanel.Height) / 2;
        }

        private void AutorizareForm2_Load(object sender, EventArgs e)
        {
            DBConnection.ConnectionDB();
            CenterPanel();

            RegisterPassword.UseSystemPasswordChar = true;
        }

        private void AutorizareForm2_Resize(object sender, EventArgs e)
        {
            CenterPanel();
            AlignThemeButton();
        }
        Point lastPoint;

        private void AutorizareForm2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void AutorizareForm2_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void LogInButton_Click(object sender, EventArgs e)
        {
            this.Hide();

            LoginForm1 loginForm1 = new LoginForm1();
            loginForm1.Show();
        }

        private void LogInButton_MouseEnter(object sender, EventArgs e)
        {
            LogInButton.ForeColor = Color.Blue;
        }

        private void LogInButton_MouseLeave(object sender, EventArgs e)
        {
            LogInButton.ForeColor = Color.RoyalBlue;
        }

        private async void GoogleRegisterButton_Click(object? sender, EventArgs e)
        {
            SetGoogleRegisterEnabled(false);

            try
            {
                GoogleUserProfile profile = await GoogleAuthService.SignInAsync();

                if (Authorization.AccountExistsByEmail(profile.Email))
                {
                    MessageBox.Show(
                        "Account already exists. Please log in.",
                        "Google registration",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                if (Authorization.CreateUserFromGoogleProfile(profile, out string errorMessage))
                {
                    if (TryOpenAuthorizedForm())
                        MessageBox.Show("Account created from Google successfully!");

                    return;
                }

                RegisterName.Text = BuildGoogleLogin(profile);
                RegisterEmail.Text = profile.Email;
                RegisterPassword.Focus();

                MessageBox.Show(
                    "Google filled the name and email, but the account was not created automatically.\n\n"
                    + errorMessage
                    + "\n\nAdd password and phone number, then create the account.",
                    "Google registration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Google registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Google registration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetGoogleRegisterEnabled(true);
            }
        }

        private void SetGoogleRegisterEnabled(bool enabled)
        {
            if (googleRegisterButton != null)
                googleRegisterButton.Enabled = enabled;

            Cursor = enabled ? Cursors.Hand : Cursors.WaitCursor;
        }

        private string BuildGoogleLogin(GoogleUserProfile profile)
        {
            string login = profile.Name.Trim();

            if (login == "")
                login = profile.Email.Split('@')[0];

            return login;
        }

        private bool TryOpenAuthorizedForm()
        {
            try
            {
                Form authorizedForm = string.Equals(Authorization.Role, "admin", StringComparison.OrdinalIgnoreCase)
                    ? new AdminForm()
                    : new UserForm();

                authorizedForm.Show();
                Hide();
                return true;
            }
            catch (Exception ex)
            {
                Show();
                WindowState = FormWindowState.Maximized;
                BringToFront();
                MessageBox.Show(
                    "Could not open the application window.\n\n" + ex.Message,
                    "Google registration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = RegisterName.Text.Trim();
            string password = RegisterPassword.Text.Trim();
            string email = RegisterEmail.Text.Trim();
            string phone = RegisterNumber.Text.Trim();

            if (login == "" || password == "" || email == "" || phone == "")
            {
                MessageBox.Show("Fill in all fields!");
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(DBConnection.ConnectionString))
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM accounts WHERE Login = @login OR Email = @email";

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@login", login);
                        checkCmd.Parameters.AddWithValue("@email", email);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show("Login or email already exists!");
                            return;
                        }
                    }

                    string normalizedPhone = Authorization.NormalizePhone(phone);
                    string phoneCheckQuery = "SELECT Phone FROM accounts";

                    using (MySqlCommand phoneCheckCmd = new MySqlCommand(phoneCheckQuery, con))
                    using (MySqlDataReader phoneReader = phoneCheckCmd.ExecuteReader())
                    {
                        while (phoneReader.Read())
                        {
                            string accountPhone = phoneReader["Phone"]?.ToString() ?? "";

                            if (Authorization.NormalizePhone(accountPhone) == normalizedPhone)
                            {
                                MessageBox.Show("Phone already exists!");
                                return;
                            }
                        }
                    }

                    string insertQuery = @"
                        INSERT INTO accounts
                        (Login, Password, Email, Phone, id_role)
                        VALUES
                        (@login, @password, @email, @phone, 2)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phone);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Account created successfully!");

                    this.Hide();

                    LoginForm1 loginForm = new LoginForm1();
                    loginForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error");
            }
        }
    }
}
