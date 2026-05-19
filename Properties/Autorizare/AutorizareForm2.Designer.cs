namespace PracticaApp
{
    partial class AutorizareForm2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutorizareForm2));
            CrearePanel = new Panel();
            CreateAccount = new Label();
            RegisterName = new TextBox();
            checkBox1 = new CheckBox();
            LogInButton = new Label();
            label5 = new Label();
            button1 = new Button();
            label4 = new Label();
            label3 = new Label();
            pictureBox1 = new PictureBox();
            label2 = new Label();
            label1 = new Label();
            RegisterNumber = new TextBox();
            RegisterEmail = new TextBox();
            RegisterPassword = new TextBox();
            CrearePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // CrearePanel
            // 
            CrearePanel.Anchor = AnchorStyles.None;
            CrearePanel.BackColor = Color.Transparent;
            CrearePanel.Controls.Add(CreateAccount);
            CrearePanel.Controls.Add(RegisterName);
            CrearePanel.Controls.Add(checkBox1);
            CrearePanel.Controls.Add(LogInButton);
            CrearePanel.Controls.Add(label5);
            CrearePanel.Controls.Add(button1);
            CrearePanel.Controls.Add(label4);
            CrearePanel.Controls.Add(label3);
            CrearePanel.Controls.Add(pictureBox1);
            CrearePanel.Controls.Add(label2);
            CrearePanel.Controls.Add(label1);
            CrearePanel.Controls.Add(RegisterNumber);
            CrearePanel.Controls.Add(RegisterEmail);
            CrearePanel.Controls.Add(RegisterPassword);
            CrearePanel.Location = new Point(429, 35);
            CrearePanel.Name = "CrearePanel";
            CrearePanel.Size = new Size(628, 821);
            CrearePanel.TabIndex = 16;
            // 
            // CreateAccount
            // 
            CreateAccount.AutoSize = true;
            CreateAccount.Font = new Font("Toledo", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            CreateAccount.ForeColor = SystemColors.Window;
            CreateAccount.Location = new Point(3, 54);
            CreateAccount.Name = "CreateAccount";
            CreateAccount.Size = new Size(615, 78);
            CreateAccount.TabIndex = 32;
            CreateAccount.Text = "Create your account";
            // 
            // RegisterName
            // 
            RegisterName.BackColor = SystemColors.ActiveCaptionText;
            RegisterName.Font = new Font("Toledo", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegisterName.ForeColor = SystemColors.Window;
            RegisterName.Location = new Point(19, 200);
            RegisterName.Multiline = true;
            RegisterName.Name = "RegisterName";
            RegisterName.Size = new Size(580, 52);
            RegisterName.TabIndex = 30;
            RegisterName.UseSystemPasswordChar = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.BackColor = Color.Transparent;
            checkBox1.Font = new Font("Toledo", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox1.ForeColor = SystemColors.ButtonFace;
            checkBox1.Location = new Point(19, 630);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(266, 24);
            checkBox1.TabIndex = 29;
            checkBox1.Text = "Accept our Terms and Conditions";
            checkBox1.UseVisualStyleBackColor = false;
            // 
            // LogInButton
            // 
            LogInButton.AllowDrop = true;
            LogInButton.Font = new Font("Toledo", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LogInButton.ForeColor = Color.RoyalBlue;
            LogInButton.Location = new Point(373, 798);
            LogInButton.Name = "LogInButton";
            LogInButton.Size = new Size(61, 25);
            LogInButton.TabIndex = 28;
            LogInButton.Text = "Log In";
            LogInButton.Click += LogInButton_Click;
            LogInButton.MouseEnter += LogInButton_MouseEnter;
            LogInButton.MouseLeave += LogInButton_MouseLeave;
            // 
            // label5
            // 
            label5.Font = new Font("Toledo", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = SystemColors.ButtonShadow;
            label5.Location = new Point(149, 798);
            label5.Name = "label5";
            label5.Size = new Size(227, 25);
            label5.TabIndex = 27;
            label5.Text = "Already have an account?\r\n";
            // 
            // button1
            // 
            button1.BackColor = SystemColors.ButtonHighlight;
            button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(173, 173, 173);
            button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(173, 173, 173);
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Toledo", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.Location = new Point(144, 704);
            button1.Name = "button1";
            button1.Size = new Size(308, 57);
            button1.TabIndex = 26;
            button1.Text = "Create Free Account";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label4
            // 
            label4.Font = new Font("Toledo", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = SystemColors.Control;
            label4.Location = new Point(19, 499);
            label4.Name = "label4";
            label4.Size = new Size(113, 22);
            label4.TabIndex = 25;
            label4.Text = "Phone number";
            // 
            // label3
            // 
            label3.Font = new Font("Toledo", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.Control;
            label3.Location = new Point(19, 390);
            label3.Name = "label3";
            label3.Size = new Size(101, 22);
            label3.TabIndex = 24;
            label3.Text = "Email Adress";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(19, 282);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(24, 24);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 23;
            pictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.Font = new Font("Toledo", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.Control;
            label2.Location = new Point(49, 284);
            label2.Name = "label2";
            label2.Size = new Size(77, 22);
            label2.TabIndex = 22;
            label2.Text = "Password";
            // 
            // label1
            // 
            label1.Font = new Font("Toledo", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.Control;
            label1.Location = new Point(19, 175);
            label1.Name = "label1";
            label1.Size = new Size(52, 22);
            label1.TabIndex = 21;
            label1.Text = "Login";
            // 
            // RegisterNumber
            // 
            RegisterNumber.BackColor = SystemColors.ActiveCaptionText;
            RegisterNumber.Font = new Font("Toledo", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegisterNumber.ForeColor = SystemColors.Window;
            RegisterNumber.Location = new Point(19, 524);
            RegisterNumber.Multiline = true;
            RegisterNumber.Name = "RegisterNumber";
            RegisterNumber.Size = new Size(580, 52);
            RegisterNumber.TabIndex = 20;
            // 
            // RegisterEmail
            // 
            RegisterEmail.BackColor = SystemColors.ActiveCaptionText;
            RegisterEmail.Font = new Font("Toledo", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegisterEmail.ForeColor = SystemColors.Window;
            RegisterEmail.Location = new Point(19, 415);
            RegisterEmail.Multiline = true;
            RegisterEmail.Name = "RegisterEmail";
            RegisterEmail.Size = new Size(580, 52);
            RegisterEmail.TabIndex = 19;
            // 
            // RegisterPassword
            // 
            RegisterPassword.BackColor = SystemColors.ActiveCaptionText;
            RegisterPassword.Font = new Font("Toledo", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RegisterPassword.ForeColor = SystemColors.Window;
            RegisterPassword.Location = new Point(19, 309);
            RegisterPassword.Name = "RegisterPassword";
            RegisterPassword.Size = new Size(580, 53);
            RegisterPassword.TabIndex = 18;
            RegisterPassword.UseSystemPasswordChar = true;
            // 
            // AutorizareForm2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1387, 960);
            Controls.Add(CrearePanel);
            Cursor = Cursors.Hand;
            Name = "AutorizareForm2";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AutorizareForm2";
            Load += AutorizareForm2_Load;
            MouseDown += AutorizareForm2_MouseDown;
            MouseMove += AutorizareForm2_MouseMove;
            CrearePanel.ResumeLayout(false);
            CrearePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Panel CrearePanel;
        private Label CreateAccount;
        private TextBox RegisterName;
        private CheckBox checkBox1;
        private Label LogInButton;
        private Label label5;
        private Button button1;
        private Label label4;
        private Label label3;
        private PictureBox pictureBox1;
        private Label label2;
        private Label label1;
        private TextBox RegisterNumber;
        private TextBox RegisterEmail;
        private TextBox RegisterPassword;
    }
}