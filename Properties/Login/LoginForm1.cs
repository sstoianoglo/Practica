using Org.BouncyCastle.Crypto.Digests;
using PracticaApp.Properties;
using PracticaApp.Properties.Autorizare;
using PracticaApp.Properties.Login;

namespace PracticaApp
{
    public partial class LoginForm1 : Form
    {
        private readonly Image? originalBackgroundImage;
        private Button? themeButton;

        public LoginForm1()
        {
            InitializeComponent();
            originalBackgroundImage = BackgroundImage;
            this.WindowState = FormWindowState.Maximized;
            SetupLoginMethodButtons();
            SetupThemeButton();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
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

            label1.ForeColor = ThemeManager.Text;
            label2.ForeColor = ThemeManager.Text;
            label3.ForeColor = ThemeManager.MutedText;
            Lozung.ForeColor = ThemeManager.Accent;
            CreateAccount.ForeColor = Color.RoyalBlue;
            FitPro.ForeColor = ThemeManager.IsDark ? Color.Transparent : Color.Black;

            label4.ForeColor = ThemeManager.Accent;
            label5.ForeColor = ThemeManager.Accent;
            label6.ForeColor = ThemeManager.Accent;
            label7.ForeColor = ThemeManager.MutedText;
            label8.ForeColor = ThemeManager.MutedText;
            label11.ForeColor = ThemeManager.MutedText;

            button1.BackColor = ThemeManager.Accent;
            button1.ForeColor = Color.White;

            if (themeButton != null)
                ThemeManager.StyleThemeButton(themeButton);
        }

        private void SetupLoginMethodButtons()
        {
            button4.Text = "Email";
            button4.BackColor = Color.RoyalBlue;
            pictureBox3.BackColor = Color.RoyalBlue;
            pictureBox3.Image = Resources.gmail;

            button2.Click += PhoneLogin_Click;
            button3.Click += GoogleLogin_Click;
            button4.Click += EmailLogin_Click;
            pictureBox3.Click += EmailLogin_Click;
            pictureBox5.Click += GoogleLogin_Click;
            pictureBox6.Click += PhoneLogin_Click;

            button2.Cursor = Cursors.Hand;
            button3.Cursor = Cursors.Hand;
            button4.Cursor = Cursors.Hand;
            pictureBox3.Cursor = Cursors.Hand;
            pictureBox5.Cursor = Cursors.Hand;
            pictureBox6.Cursor = Cursors.Hand;
        }

        private void TemporarilyUnavailable_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "This feature is temporarily unavailable.",
                "Temporarily unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void PhoneLogin_Click(object? sender, EventArgs e)
        {
            string? phone = ShowPhoneLoginDialog();

            if (phone == null)
                return;

            if (Authorization.NormalizePhone(phone) == "")
            {
                MessageBox.Show("Enter phone number.", "Phone login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Authorization.AuthorizationByUserPhone(phone))
            {
                MessageBox.Show(
                    "This phone number is not allowed for user login.",
                    "Access denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Welcome user!");
            Hide();

            UserForm userForm = new UserForm();
            userForm.Show();
        }

        private async void GoogleLogin_Click(object? sender, EventArgs e)
        {
            SetGoogleLoginEnabled(false);

            try
            {
                GoogleUserProfile profile = await GoogleAuthService.SignInAsync();

                if (!Authorization.AuthorizationByGoogleEmail(profile.Email))
                {
                    MessageBox.Show(
                        $"No account found for {profile.Email}. Create an account with this email first.",
                        "Google login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (TryOpenAuthorizedForm("Google login"))
                    MessageBox.Show($"Welcome {profile.Name}!");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Google login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Google login error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetGoogleLoginEnabled(true);
            }
        }

        private void EmailLogin_Click(object? sender, EventArgs e)
        {
            EmailLoginData? loginData = ShowEmailLoginDialog();

            if (loginData == null)
                return;

            if (loginData.Email == "" || loginData.Password == "")
            {
                MessageBox.Show("Enter email and password.", "Email login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Authorization.AuthorizationByEmailAndPassword(loginData.Email, loginData.Password))
            {
                MessageBox.Show(
                    "Invalid email or password.",
                    "Email login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (TryOpenAuthorizedForm("Email login"))
                MessageBox.Show("Welcome!");
        }

        private void SetGoogleLoginEnabled(bool enabled)
        {
            button3.Enabled = enabled;
            pictureBox5.Enabled = enabled;
            Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }

        private bool TryOpenAuthorizedForm(string errorTitle)
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
                    errorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private string? ShowPhoneLoginDialog()
        {
            using Form phoneForm = new Form
            {
                Text = "Phone login",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(390, 185),
                BackColor = ThemeManager.Surface
            };

            Label titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Text,
                Location = new Point(24, 22),
                Text = "Enter user phone number"
            };

            TextBox phoneTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                Location = new Point(24, 62),
                Size = new Size(342, 34)
            };

            Button loginButton = new Button
            {
                Text = "LOGIN",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Accent,
                ForeColor = Color.White,
                Location = new Point(176, 120),
                Size = new Size(90, 36),
                UseVisualStyleBackColor = false
            };

            Button cancelButton = new Button
            {
                Text = "CANCEL",
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                Location = new Point(276, 120),
                Size = new Size(90, 36),
                UseVisualStyleBackColor = false
            };

            loginButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            cancelButton.FlatAppearance.BorderColor = ThemeManager.Border;

            phoneForm.Controls.Add(titleLabel);
            phoneForm.Controls.Add(phoneTextBox);
            phoneForm.Controls.Add(loginButton);
            phoneForm.Controls.Add(cancelButton);
            phoneForm.AcceptButton = loginButton;
            phoneForm.CancelButton = cancelButton;

            return phoneForm.ShowDialog(this) == DialogResult.OK
                ? phoneTextBox.Text.Trim()
                : null;
        }

        private EmailLoginData? ShowEmailLoginDialog()
        {
            using Form emailForm = new Form
            {
                Text = "Email login",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(410, 245),
                BackColor = ThemeManager.Surface
            };

            Label titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.Text,
                Location = new Point(24, 22),
                Text = "Enter email and password"
            };

            Label emailLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.MutedText,
                Location = new Point(24, 60),
                Size = new Size(342, 22),
                Text = "Email"
            };

            TextBox emailTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                Location = new Point(24, 82),
                Size = new Size(362, 32)
            };

            Label passwordLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ThemeManager.MutedText,
                Location = new Point(24, 122),
                Size = new Size(342, 22),
                Text = "Password"
            };

            TextBox passwordTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                Location = new Point(24, 144),
                Size = new Size(362, 32),
                UseSystemPasswordChar = true
            };

            Button loginButton = new Button
            {
                Text = "LOGIN",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Accent,
                ForeColor = Color.White,
                Location = new Point(196, 194),
                Size = new Size(90, 36),
                UseVisualStyleBackColor = false
            };

            Button cancelButton = new Button
            {
                Text = "CANCEL",
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                Location = new Point(296, 194),
                Size = new Size(90, 36),
                UseVisualStyleBackColor = false
            };

            loginButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            cancelButton.FlatAppearance.BorderColor = ThemeManager.Border;

            emailForm.Controls.Add(titleLabel);
            emailForm.Controls.Add(emailLabel);
            emailForm.Controls.Add(emailTextBox);
            emailForm.Controls.Add(passwordLabel);
            emailForm.Controls.Add(passwordTextBox);
            emailForm.Controls.Add(loginButton);
            emailForm.Controls.Add(cancelButton);
            emailForm.AcceptButton = loginButton;
            emailForm.CancelButton = cancelButton;

            return emailForm.ShowDialog(this) == DialogResult.OK
                ? new EmailLoginData(emailTextBox.Text.Trim(), passwordTextBox.Text.Trim())
                : null;
        }

        private void CenterPanel()
        {
            MainPanel.Left = (this.ClientSize.Width - MainPanel.Width) / 2;
            MainPanel.Top = (this.ClientSize.Height - MainPanel.Height) / 2;

            panel1.Left = MainPanel.Right - 10;
            panel1.Top = MainPanel.Top + 30;
            
            panel2.Left = MainPanel.Left - panel2.Width - 10;
            panel2.Top = MainPanel.Top + 20;

            label11.Left += 15;
            label4.Left += 15;
            label5.Left += 15;
            label6.Left += 15;
            label7.Left += 15;
            label8.Left += 15;
            FitPro.Left += 20;

            label11.Top += 40;
            label4.Top += 40;
            label5.Top += 40;
            label6.Top += 40;
            label7.Top += 40;
            label8.Top += 40;
            FitPro.Top += 20;

            this.DoubleBuffered = true;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.WindowState = FormWindowState.Maximized;

           MainPanel.BackColor = Color.Transparent;

        }

        private void LoginForm1_Load(object sender, EventArgs e)
        {
            CenterPanel();
        }

        private void LoginForm1_Resize(object sender, EventArgs e)
        {
            CenterPanel();
            AlignThemeButton();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm2 loginForm2 = new LoginForm2();
            loginForm2.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            EmailLogin_Click(sender, e);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }


        private void CreateAccount_Click(object sender, EventArgs e)
        {
            this.Hide();
            AutorizareForm2 autorizareForm2 = new AutorizareForm2();
            autorizareForm2.Show();
        }


        private void CreateAccount_MouseEnter(object sender, EventArgs e)
        {
            CreateAccount.ForeColor = Color.Blue;

        }

        private void CreateAccount_MouseLeave(object sender, EventArgs e)
        {
            CreateAccount.ForeColor = Color.RoyalBlue;
        }

        private sealed record EmailLoginData(string Email, string Password);
    }
}
