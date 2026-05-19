namespace PracticaApp.Properties.Login
{
    partial class LoginForm2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PanelLogin = new Panel();
            MainPassword = new Label();
            Welcome = new Label();
            MainLogin = new Label();
            LogButton = new Button();
            Password = new TextBox();
            Login = new TextBox();
            AboutUs = new Label();
            OurMission = new Label();
            PanelLogin.SuspendLayout();
            SuspendLayout();
            // 
            // PanelLogin
            // 
            PanelLogin.Anchor = AnchorStyles.None;
            PanelLogin.BackColor = Color.Transparent;
            PanelLogin.Controls.Add(MainPassword);
            PanelLogin.Controls.Add(Welcome);
            PanelLogin.Controls.Add(MainLogin);
            PanelLogin.Controls.Add(LogButton);
            PanelLogin.Controls.Add(Password);
            PanelLogin.Controls.Add(Login);
            PanelLogin.Location = new Point(455, 54);
            PanelLogin.Name = "PanelLogin";
            PanelLogin.Size = new Size(686, 744);
            PanelLogin.TabIndex = 6;
            PanelLogin.Resize += LoginForm2_Resize;
            // 
            // MainPassword
            // 
            MainPassword.AutoSize = true;
            MainPassword.Font = new Font("Toledo", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MainPassword.ForeColor = SystemColors.Window;
            MainPassword.Location = new Point(146, 434);
            MainPassword.Name = "MainPassword";
            MainPassword.Size = new Size(151, 36);
            MainPassword.TabIndex = 11;
            MainPassword.Text = "Password";
            // 
            // Welcome
            // 
            Welcome.AutoSize = true;
            Welcome.Font = new Font("Toledo", 72F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Welcome.ForeColor = SystemColors.Window;
            Welcome.Location = new Point(0, 0);
            Welcome.Name = "Welcome";
            Welcome.Size = new Size(684, 156);
            Welcome.TabIndex = 10;
            Welcome.Text = "WELCOME";
            // 
            // MainLogin
            // 
            MainLogin.AutoSize = true;
            MainLogin.Font = new Font("Toledo", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MainLogin.ForeColor = SystemColors.Window;
            MainLogin.Location = new Point(146, 263);
            MainLogin.Name = "MainLogin";
            MainLogin.Size = new Size(90, 36);
            MainLogin.TabIndex = 9;
            MainLogin.Text = "Login";
            // 
            // LogButton
            // 
            LogButton.BackColor = Color.DarkOrange;
            LogButton.FlatStyle = FlatStyle.Flat;
            LogButton.Font = new Font("Toledo", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LogButton.ForeColor = Color.Snow;
            LogButton.Location = new Point(249, 678);
            LogButton.Name = "LogButton";
            LogButton.Size = new Size(185, 63);
            LogButton.TabIndex = 8;
            LogButton.Text = "Log In";
            LogButton.UseVisualStyleBackColor = false;
            LogButton.Click += LogButton_Click;
            LogButton.MouseEnter += LogButton_MouseEnter;
            LogButton.MouseLeave += LogButton_MouseLeave;
            // 
            // Password
            // 
            Password.Font = new Font("Toledo", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Password.Location = new Point(146, 473);
            Password.Multiline = true;
            Password.Name = "Password";
            Password.Size = new Size(403, 55);
            Password.TabIndex = 7;
            Password.UseSystemPasswordChar = true;
            Password.TextChanged += Password_TextChanged;
            // 
            // Login
            // 
            Login.Font = new Font("Toledo", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Login.Location = new Point(146, 302);
            Login.Multiline = true;
            Login.Name = "Login";
            Login.Size = new Size(403, 55);
            Login.TabIndex = 6;
            // 
            // AboutUs
            // 
            AboutUs.BackColor = Color.Transparent;
            AboutUs.Font = new Font("Toledo", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AboutUs.ForeColor = Color.White;
            AboutUs.Location = new Point(46, 356);
            AboutUs.Name = "AboutUs";
            AboutUs.Size = new Size(403, 248);
            AboutUs.TabIndex = 7;
            AboutUs.Text = "About us\r\n\r\nFitPro is a fitness planner created to help people train with purpose, stay organized, and see real progress.";
            // 
            // OurMission
            // 
            OurMission.BackColor = Color.Transparent;
            OurMission.Font = new Font("Toledo", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            OurMission.ForeColor = Color.White;
            OurMission.Location = new Point(1167, 356);
            OurMission.Name = "OurMission";
            OurMission.Size = new Size(403, 248);
            OurMission.TabIndex = 8;
            OurMission.Text = "Our mission\r\n\r\nWe make workouts easier to plan, track, and improve, whether you are a beginner or already experienced.";
            // 
            // LoginForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            BackgroundImage = Resources.background;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1582, 900);
            Controls.Add(OurMission);
            Controls.Add(AboutUs);
            Controls.Add(PanelLogin);
            Name = "LoginForm2";
            Text = "LoginForm2";
            PanelLogin.ResumeLayout(false);
            PanelLogin.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel PanelLogin;
        private Label MainPassword;
        private Label Welcome;
        private Label MainLogin;
        private Button LogButton;
        private TextBox Password;
        private TextBox Login;
        private Label AboutUs;
        private Label OurMission;
    }
}