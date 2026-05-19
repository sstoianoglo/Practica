using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using PracticaApp.Properties.Autorizare;

namespace PracticaApp.Properties
{
    public partial class AdminForm : Form
    {
        private const string AllMuscleGroupsText = "All muscle groups";
        private const string AllDifficultiesText = "All difficulties";
        private const string AllEquipmentText = "All equipment";
        private const int BackgroundCropPadding = 28;

        private readonly ExerciseRepository exerciseRepository = new ExerciseRepository();
        private readonly List<CardLikeState> cardLikeStates = new List<CardLikeState>();
        private readonly List<Panel> designTimeCardPanels = new List<Panel>();
        private readonly List<Panel> dynamicExerciseCards = new List<Panel>();
        private readonly List<StatisticCardState> statisticCards = new List<StatisticCardState>();
        private readonly Dictionary<string, Image> iconImages = new Dictionary<string, Image>();

        private Image? originalBackgroundImage;
        private Bitmap? cachedBackgroundImage;
        private Button? addExerciseButton;
        private Button? likedExercisesButton;
        private Button? accountButton;
        private Button? themeButton;
        private Button? filterButton;
        private Panel? statisticsPanel;
        private Label? statisticsTitleLabel;
        private Panel? exerciseInfoPanel;
        private Label? exerciseInfoTitleLabel;
        private Label? exerciseInfoBodyLabel;
        private Panel? filterPanel;
        private ComboBox? muscleGroupFilterComboBox;
        private ComboBox? difficultyFilterComboBox;
        private ComboBox? equipmentFilterComboBox;
        private Button? clearFiltersButton;

        private Color normalCardColor = ThemeManager.Surface;
        private Color likedCardColor = ThemeManager.LikedCard;

        private Color normalHeartColor = ThemeManager.MutedText;
        private Color likedHeartColor = ThemeManager.LikedHeart;
        private Color normalHeartBorderColor = ThemeManager.Border;

        private int exerciseCardsGap = 96;
        private int latestExerciseCardsBottom;
        private Point exerciseCardsStartLocation = Point.Empty;

        private bool IsAdmin => string.Equals(Authorization.Role, "admin", StringComparison.OrdinalIgnoreCase);

        public AdminForm()
        {
            InitializeComponent();

            WindowState = FormWindowState.Maximized;
            AutoScroll = true;
            SetupFormPerformance();

            SetupRoleMode();
            SetupBackButton();
            SetupThemeButton();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;

            MainText.Top = 80;
            label1.Top = 80;

            CaptureDesignTimeCards();
            HideDesignTimeCards();
            SetupSearchBox();
            SetupFilterControls();
            SetupUserControls();
            SetupAdminControls();
            SetupExerciseInfoPopup();

            LoadExercisesFromDatabase();

            Shown += (s, e) => ApplyRoundedCorners();
            Resize += (s, e) =>
            {
                AlignThemeButton();
                AlignSearchPanel();
                FilterExerciseCards(textBox1.Text);
            };
            Scroll += (s, e) =>
            {
                AlignThemeButton();
                HideExerciseInfoPopup();
            };

            panel1.Resize += (s, e) => RoundControl(panel1, 20);
            panel1.Paint += SearchPanel_Paint;
            textBox1.Resize += (s, e) => RoundControl(textBox1, 20);
            filterPanel?.Resize += (s, e) => RoundControl(filterPanel, 18);
        }

        private void SetupRoleMode()
        {
            Text = IsAdmin ? "Admin" : "User";
        }

        private void SetupBackButton()
        {
            AdminButton.Text = string.Empty;
            AdminButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            AdminButton.BackColor = ThemeManager.Surface;
            AdminButton.FlatStyle = FlatStyle.Flat;
            AdminButton.ForeColor = ThemeManager.Accent;
            AdminButton.Location = new Point(24, 24);
            AdminButton.Size = new Size(54, 44);
            AdminButton.Padding = Padding.Empty;
            AdminButton.UseVisualStyleBackColor = false;
            AdminButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            AdminButton.FlatAppearance.BorderSize = 1;
            AdminButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
            AdminButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
            AdminButton.Paint -= BackButton_Paint;
            AdminButton.Paint += BackButton_Paint;
            AdminButton.BringToFront();
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

            themeButton.Left = -AutoScrollPosition.X + ClientSize.Width - themeButton.Width - 24;
            themeButton.Top = -AutoScrollPosition.Y + 24;
            themeButton.BringToFront();
            AlignAccountButton();
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
            normalCardColor = ThemeManager.Surface;
            likedCardColor = ThemeManager.LikedCard;
            normalHeartColor = ThemeManager.MutedText;
            likedHeartColor = ThemeManager.LikedHeart;
            normalHeartBorderColor = ThemeManager.Border;

            BackColor = ThemeManager.Background;
            BackgroundImage = null;
            if (ThemeManager.IsDark)
                CacheBackgroundImage();
            else
                ClearCachedBackgroundImage();

            MainText.ForeColor = ThemeManager.Text;
            label1.ForeColor = ThemeManager.Accent;
            ThemeManager.StyleBackButton(AdminButton);

            if (themeButton != null)
                ThemeManager.StyleThemeButton(themeButton);

            StyleAccountButton();

            panel1.BackColor = ThemeManager.Surface;
            textBox1.BackColor = ThemeManager.Surface;
            textBox1.ForeColor = ThemeManager.Text;
            panel1.Invalidate();
            StyleFilterControls();
            StyleStatisticsControls();
            StyleExerciseInfoPopup();

            if (addExerciseButton != null)
            {
                addExerciseButton.BackColor = ThemeManager.Accent;
                addExerciseButton.ForeColor = Color.White;
            }

            StyleLikedExercisesButton();

            foreach (CardLikeState state in cardLikeStates)
            {
                ApplyCardTheme(state);
            }

            Invalidate(true);
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CacheBackgroundImage();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (!ThemeManager.IsDark || cachedBackgroundImage == null)
                return;

            e.Graphics.DrawImageUnscaled(cachedBackgroundImage, Point.Empty);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        [DllImport("Gdi32.dll", EntryPoint = "DeleteObject")]
        private static extern bool DeleteObject(IntPtr hObject);

        private void ApplyRoundedCorners()
        {
            RoundControl(panel1, 20);
            RoundControl(textBox1, 20);

            if (filterPanel != null)
                RoundControl(filterPanel, 18);

            if (exerciseInfoPanel != null)
                RoundControl(exerciseInfoPanel, 18);

            foreach (StatisticCardState state in statisticCards)
            {
                RoundControl(state.CardPanel, 22);
                state.CardPanel.Invalidate();
            }

            foreach (CardLikeState state in cardLikeStates)
            {
                RoundControl(state.CardPanel, 24);
                FitHeartButton(state);
                state.HeartButton.Invalidate();
            }
        }

        private void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;

            IntPtr regionHandle = CreateRoundRectRgn(
                0,
                0,
                control.Width + 1,
                control.Height + 1,
                radius * 2,
                radius * 2
            );

            control.Region = Region.FromHrgn(regionHandle);
            DeleteObject(regionHandle);
        }

        private void SetupFormPerformance()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );
            UpdateStyles();

            originalBackgroundImage = BackgroundImage;
            BackgroundImage = null;
            BackgroundImageLayout = ImageLayout.None;
            CacheBackgroundImage();

            FormClosed += (s, e) => cachedBackgroundImage?.Dispose();
        }

        private void SetupExerciseInfoPopup()
        {
            exerciseInfoPanel = new Panel
            {
                Size = new Size(360, 240),
                BackColor = ThemeManager.Surface,
                Visible = false,
                Padding = new Padding(16)
            };

            exerciseInfoPanel.Paint += ExerciseInfoPanel_Paint;
            exerciseInfoPanel.MouseEnter += (s, e) => exerciseInfoPanel.Visible = true;
            exerciseInfoPanel.MouseLeave += (s, e) => HideExerciseInfoPopup();

            exerciseInfoTitleLabel = new Label
            {
                AutoSize = false,
                Location = new Point(16, 14),
                Size = new Size(328, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            exerciseInfoBodyLabel = new Label
            {
                AutoSize = false,
                Location = new Point(16, 50),
                Size = new Size(328, 174),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            exerciseInfoPanel.Controls.Add(exerciseInfoTitleLabel);
            exerciseInfoPanel.Controls.Add(exerciseInfoBodyLabel);
            Controls.Add(exerciseInfoPanel);

            StyleExerciseInfoPopup();
            RoundControl(exerciseInfoPanel, 18);
        }

        private void StyleExerciseInfoPopup()
        {
            if (exerciseInfoPanel == null)
                return;

            exerciseInfoPanel.BackColor = ThemeManager.Surface;
            exerciseInfoPanel.Invalidate();

            if (exerciseInfoTitleLabel != null)
            {
                exerciseInfoTitleLabel.ForeColor = ThemeManager.Accent;
                exerciseInfoTitleLabel.BackColor = ThemeManager.Surface;
            }

            if (exerciseInfoBodyLabel != null)
            {
                exerciseInfoBodyLabel.ForeColor = ThemeManager.Text;
                exerciseInfoBodyLabel.BackColor = ThemeManager.Surface;
            }
        }

        private void ShowExerciseInfoPopup(Control sourceControl, Exercise exercise)
        {
            if (exerciseInfoPanel == null || exerciseInfoTitleLabel == null || exerciseInfoBodyLabel == null)
                return;

            exerciseInfoTitleLabel.Text = exercise.Name;
            exerciseInfoBodyLabel.Text = BuildExerciseInfoText(exercise);
            StyleExerciseInfoPopup();

            Point preferredPoint = PointToClient(sourceControl.PointToScreen(new Point(0, sourceControl.Height + 8)));
            int visibleX = preferredPoint.X;
            int visibleY = preferredPoint.Y;

            if (visibleX + exerciseInfoPanel.Width > ClientSize.Width - 18)
                visibleX = ClientSize.Width - exerciseInfoPanel.Width - 18;

            if (visibleY + exerciseInfoPanel.Height > ClientSize.Height - 18)
                visibleY = PointToClient(sourceControl.PointToScreen(new Point(0, -exerciseInfoPanel.Height - 8))).Y;

            visibleX = Math.Max(18, visibleX);
            visibleY = Math.Max(18, visibleY);

            exerciseInfoPanel.Location = new Point(
                visibleX - AutoScrollPosition.X,
                visibleY - AutoScrollPosition.Y
            );
            exerciseInfoPanel.Visible = true;
            exerciseInfoPanel.BringToFront();
        }

        private void HideExerciseInfoPopup()
        {
            if (exerciseInfoPanel != null)
                exerciseInfoPanel.Visible = false;
        }

        private void ExerciseInfoPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is Panel panel)
                DrawRoundedSurfaceBorder(e.Graphics, panel.ClientRectangle, 18, GetSurfaceOutlineColor());
        }

        private void CacheBackgroundImage()
        {
            if (!ThemeManager.IsDark)
            {
                ClearCachedBackgroundImage();
                return;
            }

            if (originalBackgroundImage == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            Bitmap newBackground = new Bitmap(ClientSize.Width, ClientSize.Height);

            using (Graphics graphics = Graphics.FromImage(newBackground))
            {
                graphics.InterpolationMode = InterpolationMode.Low;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.DrawImage(
                    originalBackgroundImage,
                    new Rectangle(Point.Empty, ClientSize),
                    GetBackgroundSourceRectangle(originalBackgroundImage),
                    GraphicsUnit.Pixel
                );
            }

            Bitmap? oldBackground = cachedBackgroundImage;
            cachedBackgroundImage = newBackground;
            oldBackground?.Dispose();
            Invalidate();
        }

        private Rectangle GetBackgroundSourceRectangle(Image image)
        {
            int cropX = Math.Min(BackgroundCropPadding, image.Width / 4);
            int cropY = Math.Min(BackgroundCropPadding, image.Height / 4);

            return new Rectangle(
                cropX,
                cropY,
                image.Width - cropX * 2,
                image.Height - cropY * 2
            );
        }

        private void ClearCachedBackgroundImage()
        {
            Bitmap? oldBackground = cachedBackgroundImage;
            cachedBackgroundImage = null;
            oldBackground?.Dispose();
            Invalidate();
        }

        private void CaptureDesignTimeCards()
        {
            designTimeCardPanels.Clear();

            foreach (Control control in Controls)
            {
                if (control is Panel panel && TryGetHeartControls(panel, out _, out _))
                    designTimeCardPanels.Add(panel);
            }

            designTimeCardPanels.Sort((first, second) =>
            {
                int topCompare = first.Top.CompareTo(second.Top);
                return topCompare != 0 ? topCompare : first.Left.CompareTo(second.Left);
            });

            if (designTimeCardPanels.Count > 0)
                exerciseCardsStartLocation = designTimeCardPanels[0].Location;

            if (designTimeCardPanels.Count > 1)
            {
                exerciseCardsGap = designTimeCardPanels[1].Left - designTimeCardPanels[0].Right;

                if (exerciseCardsGap < 40)
                    exerciseCardsGap = 96;
            }

            for (int index = 0; index < designTimeCardPanels.Count; index++)
            {
                Image icon = GetCardIcon(designTimeCardPanels[index]);
                iconImages["icon" + (index + 1)] = icon;
            }

            if (!iconImages.ContainsKey("icon1"))
                iconImages["icon1"] = CreateFallbackIcon();
        }

        private void HideDesignTimeCards()
        {
            foreach (Panel panel in designTimeCardPanels)
            {
                panel.Visible = false;
            }
        }

        private bool TryGetHeartControls(Panel cardPanel, out Panel? heartPanel, out Button? heartButton)
        {
            heartPanel = null;
            heartButton = null;

            foreach (Control child in cardPanel.Controls)
            {
                if (child is not Panel candidateHeartPanel)
                    continue;

                foreach (Control nestedChild in candidateHeartPanel.Controls)
                {
                    if (nestedChild is Button candidateHeartButton)
                    {
                        heartPanel = candidateHeartPanel;
                        heartButton = candidateHeartButton;
                        return true;
                    }
                }
            }

            return false;
        }

        private Image GetCardIcon(Panel cardPanel)
        {
            foreach (Control child in cardPanel.Controls)
            {
                if (child is PictureBox pictureBox && pictureBox.Image != null)
                    return new Bitmap(pictureBox.Image);
            }

            return CreateFallbackIcon();
        }

        private Image CreateFallbackIcon()
        {
            Bitmap bitmap = new Bitmap(70, 70);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.FromArgb(77, 77, 77), 5))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                graphics.DrawLine(pen, 12, 35, 58, 35);
                graphics.DrawRectangle(pen, 12, 22, 10, 26);
                graphics.DrawRectangle(pen, 48, 22, 10, 26);
            }

            return bitmap;
        }

        private void SetupSearchBox()
        {
            panel1.BackColor = normalCardColor;

            textBox1.Multiline = false;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.BackColor = normalCardColor;
            textBox1.ForeColor = ThemeManager.Text;
            textBox1.PlaceholderText = "Search";
            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

            textBox1.TextChanged -= textBox1_TextChanged;
            textBox1.TextChanged += textBox1_TextChanged;

            AlignSearchPanel();
        }

        private void SetupFilterControls()
        {
            filterButton = new Button
            {
                Text = "FILTERS",
                Size = new Size(116, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };
            filterButton.FlatAppearance.BorderSize = 1;
            filterButton.Click += FilterButton_Click;

            filterPanel = new Panel
            {
                Size = new Size(panel1.Width, 170),
                Visible = false,
                Padding = new Padding(18),
                BackColor = ThemeManager.Surface
            };
            filterPanel.Paint += FilterPanel_Paint;

            muscleGroupFilterComboBox = CreateFilterComboBox(10);
            difficultyFilterComboBox = CreateFilterComboBox(50);
            equipmentFilterComboBox = CreateFilterComboBox(90);

            filterPanel.Controls.Add(CreateFilterLabel("Muscle group", 12));
            filterPanel.Controls.Add(muscleGroupFilterComboBox);
            filterPanel.Controls.Add(CreateFilterLabel("Difficulty", 52));
            filterPanel.Controls.Add(difficultyFilterComboBox);
            filterPanel.Controls.Add(CreateFilterLabel("Equipment", 92));
            filterPanel.Controls.Add(equipmentFilterComboBox);

            clearFiltersButton = new Button
            {
                Text = "CLEAR",
                Size = new Size(108, 30),
                Location = new Point(filterPanel.Width - 126, 130),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            clearFiltersButton.FlatAppearance.BorderSize = 1;
            clearFiltersButton.Click += ClearFiltersButton_Click;
            filterPanel.Controls.Add(clearFiltersButton);

            Controls.Add(filterButton);
            Controls.Add(filterPanel);
            StyleFilterControls();
            ResetFilterOptions();
            AlignSearchPanel();
        }

        private Label CreateFilterLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Location = new Point(18, top),
                Size = new Size(122, 24),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = ThemeManager.MutedText,
                BackColor = ThemeManager.Surface
            };
        }

        private ComboBox CreateFilterComboBox(int top)
        {
            ComboBox comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(148, top),
                Size = new Size(208, 28),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text
            };

            comboBox.SelectedIndexChanged += (s, e) => FilterExerciseCards(textBox1.Text);
            return comboBox;
        }

        private void FilterButton_Click(object? sender, EventArgs e)
        {
            if (filterPanel == null)
                return;

            filterPanel.Visible = !filterPanel.Visible;
            AlignSearchPanel();
            FilterExerciseCards(textBox1.Text);
        }

        private void ClearFiltersButton_Click(object? sender, EventArgs e)
        {
            ResetComboBoxSelection(muscleGroupFilterComboBox);
            ResetComboBoxSelection(difficultyFilterComboBox);
            ResetComboBoxSelection(equipmentFilterComboBox);
            FilterExerciseCards(textBox1.Text);
        }

        private void ResetComboBoxSelection(ComboBox? comboBox)
        {
            if (comboBox != null && comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private void StyleFilterControls()
        {
            if (filterButton != null)
            {
                filterButton.BackColor = ThemeManager.Surface;
                filterButton.ForeColor = ThemeManager.Accent;
                filterButton.FlatAppearance.BorderColor = ThemeManager.Accent;
                filterButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
                filterButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
            }

            if (filterPanel == null)
                return;

            filterPanel.BackColor = ThemeManager.Surface;
            filterPanel.Invalidate();

            foreach (Control control in filterPanel.Controls)
            {
                control.BackColor = control is ComboBox ? ThemeManager.InputBack : ThemeManager.Surface;
                control.ForeColor = control is Label ? ThemeManager.MutedText : ThemeManager.Text;

                if (control is Button button)
                {
                    button.BackColor = ThemeManager.InputBack;
                    button.ForeColor = ThemeManager.Accent;
                    button.FlatAppearance.BorderColor = ThemeManager.Border;
                    button.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
                    button.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
                }
            }
        }

        private void ResetFilterOptions()
        {
            ResetFilterComboBox(muscleGroupFilterComboBox, AllMuscleGroupsText, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
            ResetFilterComboBox(difficultyFilterComboBox, AllDifficultiesText, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
            ResetFilterComboBox(equipmentFilterComboBox, AllEquipmentText, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private void UpdateFilterOptions()
        {
            SortedSet<string> muscleGroups = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            SortedSet<string> difficulties = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            SortedSet<string> equipment = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (CardLikeState state in cardLikeStates)
            {
                AddFilterOption(muscleGroups, state.Exercise.MuscleGroupName);
                AddFilterOption(difficulties, state.Exercise.DifficultyLevel);
                AddFilterOption(equipment, state.Exercise.Equipment);
            }

            ResetFilterComboBox(muscleGroupFilterComboBox, AllMuscleGroupsText, muscleGroups);
            ResetFilterComboBox(difficultyFilterComboBox, AllDifficultiesText, difficulties);
            ResetFilterComboBox(equipmentFilterComboBox, AllEquipmentText, equipment);
        }

        private void AddFilterOption(SortedSet<string> options, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                options.Add(value.Trim());
        }

        private void ResetFilterComboBox(ComboBox? comboBox, string defaultText, SortedSet<string> options)
        {
            if (comboBox == null)
                return;

            string selectedValue = GetSelectedFilterValue(comboBox);

            comboBox.Items.Clear();
            comboBox.Items.Add(defaultText);

            foreach (string option in options)
            {
                comboBox.Items.Add(option);
            }

            SelectFilterValue(comboBox, selectedValue);
        }

        private void SelectFilterValue(ComboBox comboBox, string selectedValue)
        {
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                for (int index = 1; index < comboBox.Items.Count; index++)
                {
                    if (string.Equals(comboBox.Items[index]?.ToString(), selectedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox.SelectedIndex = index;
                        return;
                    }
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private void SetupUserControls()
        {
            if (IsAdmin)
                return;

            SetupAccountButton();
            SetupStatisticsControls();

            likedExercisesButton = new Button
            {
                Text = "LIKED",
                Size = new Size(128, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };

            likedExercisesButton.FlatAppearance.BorderSize = 1;
            likedExercisesButton.Click += LikedExercisesButton_Click;

            Controls.Add(likedExercisesButton);
            likedExercisesButton.BringToFront();

            StyleLikedExercisesButton();
            AlignSearchPanel();
        }

        private void SetupAccountButton()
        {
            accountButton = new Button
            {
                Size = new Size(48, 42),
                FlatStyle = FlatStyle.Flat,
                Text = string.Empty,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            accountButton.FlatAppearance.BorderSize = 1;
            accountButton.Paint += AccountButton_Paint;
            accountButton.Click += AccountButton_Click;

            Controls.Add(accountButton);
            AlignAccountButton();
            StyleAccountButton();
            accountButton.BringToFront();
        }

        private void AlignAccountButton()
        {
            if (accountButton == null || themeButton == null)
                return;

            accountButton.Left = themeButton.Left - accountButton.Width - 12;
            accountButton.Top = themeButton.Top;
            accountButton.BringToFront();
        }

        private void StyleAccountButton()
        {
            if (accountButton == null)
                return;

            accountButton.BackColor = ThemeManager.Surface;
            accountButton.ForeColor = ThemeManager.Accent;
            accountButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            accountButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
            accountButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
            accountButton.AccessibleName = "Account";
            accountButton.Invalidate();
        }

        private void AccountButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen borderPen = new Pen(ThemeManager.Accent, 1);
            e.Graphics.DrawRectangle(borderPen, 0, 0, button.Width - 1, button.Height - 1);

            int centerX = button.ClientSize.Width / 2;
            int centerY = button.ClientSize.Height / 2;

            using Pen iconPen = new Pen(ThemeManager.Accent, 2.2F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            e.Graphics.DrawEllipse(iconPen, centerX - 6, centerY - 12, 12, 12);
            e.Graphics.DrawArc(iconPen, centerX - 13, centerY - 1, 26, 22, 205, 130);
        }

        private void AccountButton_Click(object? sender, EventArgs e)
        {
            using Form accountForm = new Form
            {
                Text = "Account",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(360, 220),
                BackColor = ThemeManager.Background
            };

            Label titleLabel = new Label
            {
                Text = "Account",
                AutoSize = false,
                Location = new Point(24, 20),
                Size = new Size(310, 34),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeManager.Text
            };

            Label userLabel = new Label
            {
                Text = $"User: {GetCurrentUserLogin()}",
                AutoSize = false,
                Location = new Point(24, 72),
                Size = new Size(310, 26),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = ThemeManager.Text
            };

            Label roleLabel = new Label
            {
                Text = $"Role: {Authorization.Role ?? "user"}",
                AutoSize = false,
                Location = new Point(24, 104),
                Size = new Size(310, 26),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = ThemeManager.MutedText
            };

            Button logoutButton = new Button
            {
                Text = "LOG OUT",
                Location = new Point(126, 158),
                Size = new Size(98, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Accent,
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };

            Button closeButton = new Button
            {
                Text = "CLOSE",
                Location = new Point(236, 158),
                Size = new Size(98, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                UseVisualStyleBackColor = false
            };

            logoutButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            closeButton.FlatAppearance.BorderColor = ThemeManager.Border;
            logoutButton.Click += (s, e) =>
            {
                accountForm.DialogResult = DialogResult.OK;
                accountForm.Close();
            };
            closeButton.Click += (s, e) => accountForm.Close();

            accountForm.Controls.Add(titleLabel);
            accountForm.Controls.Add(userLabel);
            accountForm.Controls.Add(roleLabel);
            accountForm.Controls.Add(logoutButton);
            accountForm.Controls.Add(closeButton);

            if (accountForm.ShowDialog(this) != DialogResult.OK)
                return;

            Authorization.Role = null;
            Authorization.User = null;

            Hide();
            global::PracticaApp.LoginForm1 loginForm = new global::PracticaApp.LoginForm1();
            loginForm.Show();
        }

        private void SetupStatisticsControls()
        {
            statisticsPanel = new Panel
            {
                Size = new Size(720, 88),
                BackColor = Color.Transparent
            };

            statisticsTitleLabel = new Label
            {
                Text = "YOUR STATISTICS",
                AutoSize = false,
                Location = new Point(0, 0),
                Size = new Size(220, 20),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            statisticsPanel.Controls.Add(statisticsTitleLabel);

            AddStatisticCard("Calories", "", 0, 500, false, Color.FromArgb(48, 207, 116), "flame");
            AddStatisticCard("Steps", "", 0, 10000, false, Color.FromArgb(255, 164, 28), "steps");
            AddStatisticCard("Water", "L", 0, 2, true, Color.FromArgb(86, 132, 255), "water");
            LoadStatisticProgress();

            Controls.Add(statisticsPanel);
            statisticsPanel.BringToFront();

            StyleStatisticsControls();
            AlignStatisticsPanel();
        }

        private void AddStatisticCard(string title, string unit, decimal current, decimal target, bool allowDecimal, Color accentColor, string iconKey)
        {
            if (statisticsPanel == null)
                return;

            Panel cardPanel = new Panel
            {
                Size = new Size(215, 44),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            StatisticCardState state = new StatisticCardState(cardPanel, title, unit, current, target, allowDecimal, accentColor, iconKey);
            statisticCards.Add(state);

            cardPanel.Tag = state;
            cardPanel.Paint += StatisticCard_Paint;
            cardPanel.Click += StatisticCard_Click;

            statisticsPanel.Controls.Add(cardPanel);
        }

        private void AlignStatisticsPanel(int contentBottom = -1)
        {
            if (statisticsPanel == null)
                return;

            int left = 40;
            int referenceBottom = contentBottom >= 0 ? contentBottom : latestExerciseCardsBottom;

            if (referenceBottom <= 0)
                referenceBottom = exerciseCardsStartLocation == Point.Empty ? 273 : exerciseCardsStartLocation.Y;

            int top = referenceBottom + 46;
            int width = Math.Min(720, Math.Max(360, ClientSize.Width - 80));
            int cardTop = 30;
            int cardGap = 16;
            bool compact = width < 650;
            int cardWidth = compact ? width : (width - (cardGap * 2)) / 3;

            statisticsPanel.Location = new Point(left, top);
            statisticsPanel.Size = new Size(width, compact ? 178 : 88);

            if (statisticsTitleLabel != null)
                statisticsTitleLabel.Size = new Size(width, 20);

            for (int index = 0; index < statisticCards.Count; index++)
            {
                StatisticCardState state = statisticCards[index];

                if (compact)
                {
                    state.CardPanel.Location = new Point(0, cardTop + (index * 48));
                    state.CardPanel.Size = new Size(width, 42);
                }
                else
                {
                    state.CardPanel.Location = new Point(index * (cardWidth + cardGap), cardTop);
                    state.CardPanel.Size = new Size(cardWidth, 44);
                }

                RoundControl(state.CardPanel, 22);
                state.CardPanel.Invalidate();
            }

            statisticsPanel.BringToFront();
        }

        private void StyleStatisticsControls()
        {
            if (statisticsPanel == null)
                return;

            statisticsPanel.BackColor = Color.Transparent;

            if (statisticsTitleLabel != null)
                statisticsTitleLabel.ForeColor = ThemeManager.MutedText;

            foreach (StatisticCardState state in statisticCards)
            {
                state.CardPanel.Invalidate();
            }
        }

        private void StatisticCard_Click(object? sender, EventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not StatisticCardState state)
                return;

            using StatisticInputForm inputForm = new StatisticInputForm(
                state.Title,
                state.Unit,
                state.Current,
                state.Target,
                state.AllowDecimal
            );

            if (inputForm.ShowDialog(this) != DialogResult.OK)
                return;

            state.Current = inputForm.CurrentValue;
            state.Target = inputForm.TargetValue;
            SaveStatisticProgress();
            state.CardPanel.Invalidate();
        }

        private void StatisticCard_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not StatisticCardState state)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = new Rectangle(1, 1, panel.Width - 3, panel.Height - 3);
            Color fillColor = GetStatisticFillColor(state.AccentColor);

            using (GraphicsPath path = GetRoundedPath(bounds, 22))
            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            using (Pen borderPen = new Pen(state.AccentColor, 1.4F))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            Rectangle iconBounds = new Rectangle(18, 12, 20, 20);
            DrawStatisticIcon(e.Graphics, state.IconKey, iconBounds, state.AccentColor);

            Rectangle textBounds = new Rectangle(52, 0, panel.Width - 60, panel.Height);
            using Font textFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            TextRenderer.DrawText(
                e.Graphics,
                state.DisplayText,
                textFont,
                textBounds,
                state.AccentColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis
            );
        }

        private Color GetStatisticFillColor(Color accentColor)
        {
            return ThemeManager.IsDark
                ? Color.FromArgb(
                    Math.Max(16, accentColor.R / 5),
                    Math.Max(16, accentColor.G / 5),
                    Math.Max(18, accentColor.B / 5)
                )
                : Color.FromArgb(
                    Math.Min(255, 238 + accentColor.R / 18),
                    Math.Min(255, 238 + accentColor.G / 18),
                    Math.Min(255, 238 + accentColor.B / 18)
                );
        }

        private void DrawStatisticIcon(Graphics graphics, string iconKey, Rectangle bounds, Color color)
        {
            using Pen pen = new Pen(color, 2F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using SolidBrush brush = new SolidBrush(color);

            if (iconKey == "steps")
            {
                graphics.FillEllipse(brush, bounds.Left + 1, bounds.Top + 8, 7, 10);
                graphics.FillEllipse(brush, bounds.Left + 10, bounds.Top + 3, 7, 10);
                graphics.DrawLine(pen, bounds.Left + 8, bounds.Bottom - 3, bounds.Right - 1, bounds.Bottom - 3);
                return;
            }

            using GraphicsPath path = new GraphicsPath();

            if (iconKey == "water")
            {
                path.AddBezier(
                    bounds.Left + bounds.Width / 2,
                    bounds.Top + 1,
                    bounds.Right - 2,
                    bounds.Top + 8,
                    bounds.Right - 1,
                    bounds.Bottom - 3,
                    bounds.Left + bounds.Width / 2,
                    bounds.Bottom - 1
                );
                path.AddBezier(
                    bounds.Left + bounds.Width / 2,
                    bounds.Bottom - 1,
                    bounds.Left + 1,
                    bounds.Bottom - 3,
                    bounds.Left + 2,
                    bounds.Top + 8,
                    bounds.Left + bounds.Width / 2,
                    bounds.Top + 1
                );
                path.CloseFigure();
                graphics.DrawPath(pen, path);
                return;
            }

            path.AddBezier(
                bounds.Left + bounds.Width / 2,
                bounds.Top + 1,
                bounds.Right - 2,
                bounds.Top + 9,
                bounds.Right - 4,
                bounds.Bottom - 1,
                bounds.Left + bounds.Width / 2,
                bounds.Bottom - 1
            );
            path.AddBezier(
                bounds.Left + bounds.Width / 2,
                bounds.Bottom - 1,
                bounds.Left + 2,
                bounds.Bottom - 2,
                bounds.Left + 2,
                bounds.Top + 10,
                bounds.Left + bounds.Width / 2,
                bounds.Top + 1
            );
            path.CloseFigure();
            graphics.DrawPath(pen, path);
        }

        private void LoadStatisticProgress()
        {
            try
            {
                string filePath = GetStatisticProgressFilePath();

                if (!File.Exists(filePath))
                    return;

                string json = File.ReadAllText(filePath);
                Dictionary<string, StatisticProgressData>? savedStatistics =
                    JsonSerializer.Deserialize<Dictionary<string, StatisticProgressData>>(json);

                if (savedStatistics == null)
                    return;

                foreach (StatisticCardState state in statisticCards)
                {
                    if (!savedStatistics.TryGetValue(state.Title, out StatisticProgressData? savedState))
                        continue;

                    state.Current = Math.Max(0, savedState.Current);

                    if (savedState.Target > 0)
                        state.Target = savedState.Target;
                }
            }
            catch
            {
                // Local progress is optional; if the file is damaged, the app keeps default values.
            }
        }

        private void SaveStatisticProgress()
        {
            try
            {
                string filePath = GetStatisticProgressFilePath();
                string? directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrWhiteSpace(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                Dictionary<string, StatisticProgressData> progressByTitle = new Dictionary<string, StatisticProgressData>();

                foreach (StatisticCardState state in statisticCards)
                {
                    progressByTitle[state.Title] = new StatisticProgressData
                    {
                        Current = state.Current,
                        Target = state.Target
                    };
                }

                string json = JsonSerializer.Serialize(progressByTitle, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save local statistics progress." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Statistics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private string GetStatisticProgressFilePath()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string userKey = SanitizeStatisticFileName(Authorization.User ?? "default");

            return Path.Combine(localAppDataPath, "FitPro", $"statistics_{userKey}.json");
        }

        private string SanitizeStatisticFileName(string value)
        {
            string cleanValue = string.IsNullOrWhiteSpace(value) ? "default" : value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                cleanValue = cleanValue.Replace(invalidChar, '_');
            }

            return cleanValue;
        }

        private void StyleLikedExercisesButton()
        {
            if (likedExercisesButton == null)
                return;

            likedExercisesButton.BackColor = ThemeManager.Surface;
            likedExercisesButton.ForeColor = ThemeManager.Accent;
            likedExercisesButton.FlatAppearance.BorderColor = ThemeManager.Accent;
            likedExercisesButton.FlatAppearance.MouseOverBackColor = ThemeManager.SurfaceHover;
            likedExercisesButton.FlatAppearance.MouseDownBackColor = ThemeManager.SurfaceDown;
        }

        private void SetupAdminControls()
        {
            if (!IsAdmin)
                return;

            addExerciseButton = new Button
            {
                Text = "+ ADD",
                Size = new Size(128, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Visible = true
            };

            addExerciseButton.FlatAppearance.BorderSize = 0;
            addExerciseButton.Click += AddExerciseButton_Click;

            Controls.Add(addExerciseButton);
            addExerciseButton.BringToFront();

            AlignSearchPanel();
        }

        private void AlignSearchPanel()
        {
            panel1.Top = MainText.Top + 3;

            int rightMargin = 90;
            int minimumLeft = label1.Right + 45;
            panel1.Left = Math.Max(minimumLeft, ClientSize.Width - panel1.Width - rightMargin);

            if (accountButton != null && panel1.Top < accountButton.Bottom + 12)
                panel1.Left = Math.Min(panel1.Left, accountButton.Left - panel1.Width - 18);

            pictureBox1.Left = 18;
            pictureBox1.Top = (panel1.Height - pictureBox1.Height) / 2;

            textBox1.Left = 66;
            textBox1.Width = panel1.Width - textBox1.Left - 20;
            textBox1.Top = (panel1.Height - textBox1.Height) / 2;

            if (filterButton != null)
            {
                filterButton.Left = panel1.Left;
                filterButton.Top = panel1.Bottom + 12;
                filterButton.BringToFront();
            }

            if (addExerciseButton != null)
            {
                addExerciseButton.Left = panel1.Left + panel1.Width - addExerciseButton.Width;
                addExerciseButton.Top = panel1.Bottom + 12;
                addExerciseButton.BringToFront();
            }

            if (likedExercisesButton != null)
            {
                likedExercisesButton.Left = panel1.Left + panel1.Width - likedExercisesButton.Width;
                likedExercisesButton.Top = panel1.Bottom + 12;
                likedExercisesButton.BringToFront();
            }

            if (accountButton != null)
                accountButton.BringToFront();

            if (themeButton != null)
                themeButton.BringToFront();

            if (filterPanel != null)
            {
                filterPanel.Left = panel1.Left;
                int controlsBottom = Math.Max(filterButton?.Bottom ?? panel1.Bottom, likedExercisesButton?.Bottom ?? panel1.Bottom);
                filterPanel.Top = controlsBottom + 8;
                filterPanel.Width = panel1.Width;

                if (clearFiltersButton != null)
                    clearFiltersButton.Left = filterPanel.Width - clearFiltersButton.Width - 18;

                filterPanel.BringToFront();
                RoundControl(filterPanel, 18);
            }

            RoundControl(panel1, 20);
            RoundControl(textBox1, 20);
        }

        private void LoadExercisesFromDatabase()
        {
            try
            {
                exerciseRepository.EnsureTable();

                List<Exercise> exercises = exerciseRepository.GetAll();
                RenderExerciseCards(exercises);
                ApplyFavoriteStates();
                UpdateSearchSuggestions();
                UpdateFilterOptions();
                FilterExerciseCards(textBox1.Text);
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not load exercises from the database.", ex);
            }
        }

        private void RenderExerciseCards(List<Exercise> exercises)
        {
            ClearDynamicExerciseCards();

            foreach (Exercise exercise in exercises)
            {
                Panel cardPanel = CreateExerciseCard(exercise);
                dynamicExerciseCards.Add(cardPanel);
                Controls.Add(cardPanel);
                cardPanel.BringToFront();
            }

            if (addExerciseButton != null)
                addExerciseButton.BringToFront();

            if (likedExercisesButton != null)
                likedExercisesButton.BringToFront();

            if (statisticsPanel != null)
                statisticsPanel.BringToFront();

            LayoutVisibleCards(cardLikeStates);
            ApplyRoundedCorners();
        }

        private void ClearDynamicExerciseCards()
        {
            foreach (Panel panel in dynamicExerciseCards)
            {
                Controls.Remove(panel);
                panel.Dispose();
            }

            dynamicExerciseCards.Clear();
            cardLikeStates.Clear();
        }

        private Panel CreateExerciseCard(Exercise exercise)
        {
            Panel template = designTimeCardPanels.Count > 0 ? designTimeCardPanels[0] : cardPanel;

            Panel newCardPanel = new Panel
            {
                Size = template.Size,
                BackColor = normalCardColor,
                Tag = exercise
            };
            newCardPanel.Paint += CardPanel_Paint;

            Label aboutLabel = new Label
            {
                Text = "About",
                AutoSize = false,
                Font = label3.Font,
                ForeColor = ThemeManager.MutedText,
                Location = label3.Location,
                Size = new Size(120, 24),
                BackColor = normalCardColor,
                Cursor = Cursors.Hand
            };
            aboutLabel.MouseEnter += (s, e) => ShowExerciseInfoPopup(aboutLabel, exercise);
            aboutLabel.MouseLeave += (s, e) => HideExerciseInfoPopup();
            aboutLabel.Click += (s, e) => ShowExerciseInfoPopup(aboutLabel, exercise);

            PictureBox iconBox = new PictureBox
            {
                Image = GetExerciseIcon(exercise.IconKey),
                Location = pictureBox2.Location,
                Size = pictureBox2.Size,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = normalCardColor
            };

            Label nameLabel = new Label
            {
                Text = WrapExerciseName(exercise.Name),
                AutoSize = false,
                Font = label2.Font,
                ForeColor = ThemeManager.Text,
                Location = label2.Location,
                Size = new Size(180, 70),
                BackColor = normalCardColor
            };

            Panel heartPanel = new Panel
            {
                Size = panel3.Size,
                Location = panel3.Location,
                BackColor = normalCardColor,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(0)
            };

            Button heartButton = new Button
            {
                Text = string.Empty,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                BackColor = normalCardColor,
                ForeColor = normalHeartColor,
                Font = new Font("Segoe UI Symbol", 24, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            heartButton.FlatAppearance.BorderSize = 0;
            heartButton.FlatAppearance.MouseOverBackColor = normalCardColor;
            heartButton.FlatAppearance.MouseDownBackColor = normalCardColor;

            heartPanel.Controls.Add(heartButton);

            newCardPanel.Controls.Add(aboutLabel);
            newCardPanel.Controls.Add(iconBox);
            newCardPanel.Controls.Add(nameLabel);
            newCardPanel.Controls.Add(heartPanel);

            if (IsAdmin)
            {
                newCardPanel.Controls.Add(CreateEditButton(exercise));
                newCardPanel.Controls.Add(CreateDeleteButton(exercise));
            }

            RegisterExerciseCard(newCardPanel, heartPanel, heartButton, exercise);
            return newCardPanel;
        }

        private Button CreateEditButton(Exercise exercise)
        {
            Button editButton = new Button
            {
                Name = "EditExerciseButton",
                Text = string.Empty,
                Size = new Size(82, 54),
                Location = new Point(22, 234),
                FlatStyle = FlatStyle.Flat,
                BackColor = normalCardColor,
                ForeColor = Color.DarkOrange,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Tag = exercise
            };

            editButton.Region = null;
            editButton.FlatAppearance.BorderSize = 0;
            editButton.FlatAppearance.MouseOverBackColor = normalCardColor;
            editButton.FlatAppearance.MouseDownBackColor = normalCardColor;
            editButton.Paint += EditButton_Paint;
            editButton.Click += EditExerciseButton_Click;

            return editButton;
        }

        private Button CreateDeleteButton(Exercise exercise)
        {
            Button deleteButton = new Button
            {
                Text = "DELETE",
                Size = new Size(62, 30),
                Location = new Point(156, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(86, 18, 32),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0),
                Tag = exercise
            };

            deleteButton.FlatAppearance.BorderSize = 1;
            deleteButton.FlatAppearance.BorderColor = Color.FromArgb(220, 45, 65);
            deleteButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 45, 65);
            deleteButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 22, 36);
            deleteButton.Click += DeleteExerciseButton_Click;

            return deleteButton;
        }

        private void RegisterExerciseCard(Panel cardPanel, Panel heartPanel, Button heartButton, Exercise exercise)
        {
            CardLikeState state = new CardLikeState(cardPanel, heartPanel, heartButton, exercise);
            cardLikeStates.Add(state);

            heartButton.Region = null;
            heartButton.Padding = new Padding(0);
            heartButton.Location = Point.Empty;
            heartButton.Size = heartPanel.ClientSize;
            heartButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            heartButton.Tag = state;

            heartButton.Paint += heartButton_Paint;
            heartButton.Click += heartButton_Click;

            cardPanel.Resize += (s, e) => RoundControl(cardPanel, 24);
            heartPanel.Resize += (s, e) =>
            {
                FitHeartButton(state);
                heartButton.Invalidate();
            };

            FitHeartButton(state);
        }

        private Image GetExerciseIcon(string iconKey)
        {
            if (!iconImages.TryGetValue(iconKey, out Image? icon))
                icon = iconImages["icon1"];

            return new Bitmap(icon);
        }

        private string WrapExerciseName(string name)
        {
            string cleanName = CleanExerciseName(name);

            if (cleanName.Length <= 16 || cleanName.Contains("\n"))
                return cleanName;

            int splitIndex = cleanName.LastIndexOf(' ', Math.Min(cleanName.Length - 1, 16));

            return splitIndex > 0
                ? cleanName.Remove(splitIndex, 1).Insert(splitIndex, "\r\n")
                : cleanName;
        }

        private string BuildExerciseInfoText(Exercise exercise)
        {
            (string sets, string reps, string restSeconds) = GetExerciseTrainingDetails(exercise);

            return
                "Technique: " + GetExerciseTechnique(exercise) + Environment.NewLine +
                Environment.NewLine +
                "Sets: " + sets + Environment.NewLine +
                "Reps: " + reps + Environment.NewLine +
                "Rest: " + restSeconds + " sec" + Environment.NewLine +
                Environment.NewLine +
                "Tip: keep the movement controlled and do not rush the negative phase.";
        }

        private (string Sets, string Reps, string RestSeconds) GetExerciseTrainingDetails(Exercise exercise)
        {
            string name = NormalizeExerciseNameKey(exercise.Name);

            if (name is "pull ups")
                return ("4", "6-10", "90");

            if (name is "barbell row")
                return ("4", "8-10", "90");

            if (name is "skull crushers")
                return ("3", "10-12", "75");

            string difficulty = exercise.DifficultyLevel.Trim().ToLowerInvariant();

            return difficulty switch
            {
                "hard" => ("4", "8", "90"),
                "easy" => ("3", "12", "45"),
                _ => ("3", "10", "60")
            };
        }

        private string GetExerciseTechnique(Exercise exercise)
        {
            string name = NormalizeExerciseNameKey(exercise.Name);

            return name switch
            {
                "cable chest press" =>
                    "Sit or stand with the back stable, handles at chest level. Press forward without locking the elbows, then return slowly.",
                "lat pulldown" =>
                    "Sit straight, fix the thighs under the pads and pull the bar toward the upper chest. Do not lean back too much.",
                "pull ups" =>
                    "Start from a full hang, keep the core tight and pull the chest toward the bar. Lower yourself under control.",
                "barbell row" =>
                    "Stand with knees slightly bent, hinge at the hips and keep the back straight. Pull the bar toward the lower ribs.",
                "dumbbell bicep curl" =>
                    "Stand or sit upright, keep elbows close to the body and curl the dumbbells without swinging the torso.",
                "incline dumbbell press" =>
                    "Lie on an incline bench, keep feet on the floor and press dumbbells upward from chest level with controlled movement.",
                "pec deck fly" =>
                    "Sit with the back against the pad, elbows slightly bent. Bring the handles together and return slowly.",
                "seated cable row" =>
                    "Sit tall with feet fixed, pull the handle toward the waist and squeeze the shoulder blades together.",
                "cable machine" =>
                    "Choose a stable position near the cable machine, keep the core tight and move only through the target joint.",
                "dumbbells" =>
                    "Use a stable stance or bench position. Keep wrists neutral and move both dumbbells with the same speed.",
                "face pull" =>
                    "Stand facing the cable, pull the rope toward the face with elbows high and squeeze the rear shoulders.",
                "hammer curl" =>
                    "Hold dumbbells with a neutral grip, keep elbows fixed near the body and curl without swinging.",
                "skull crushers" =>
                    "Lie on a bench, keep elbows pointed upward and lower the weight toward the forehead under control.",
                _ => GetDefaultExerciseTechnique(exercise)
            };
        }

        private string GetDefaultExerciseTechnique(Exercise exercise)
        {
            string muscleGroup = exercise.MuscleGroupName.Trim().ToLowerInvariant();

            return muscleGroup switch
            {
                "chest" =>
                    "Keep the chest lifted, shoulders stable and press or bring the arms together with controlled movement.",
                "back" =>
                    "Keep the back straight, pull with the elbows and squeeze the shoulder blades at the end of the movement.",
                "legs" =>
                    "Keep the feet stable, knees aligned with toes and control the movement without bouncing.",
                "shoulders" =>
                    "Keep the core tight, avoid shrugging and move the weight smoothly through the shoulder joint.",
                "arms" =>
                    "Keep elbows stable, avoid swinging and focus on controlled contraction of the arm muscles.",
                _ =>
                    "Use a stable body position, keep the core tight and perform the exercise with controlled tempo."
            };
        }

        private string NormalizeExerciseNameKey(string name)
        {
            return CleanExerciseName(name).ToLowerInvariant();
        }

        private void FitHeartButton(CardLikeState state)
        {
            state.HeartButton.Region = null;
            state.HeartButton.Location = Point.Empty;
            state.HeartButton.Size = state.HeartPanel.ClientSize;
        }

        private void UpdateSearchSuggestions()
        {
            AutoCompleteStringCollection suggestions = new AutoCompleteStringCollection();

            foreach (CardLikeState state in cardLikeStates)
            {
                suggestions.Add(state.Exercise.Name);
            }

            textBox1.AutoCompleteCustomSource = suggestions;
        }

        private void textBox1_TextChanged(object? sender, EventArgs e)
        {
            FilterExerciseCards(textBox1.Text);
        }

        private void FilterExerciseCards(string searchText)
        {
            string query = NormalizeSearchText(searchText);
            List<CardLikeState> visibleCards = new List<CardLikeState>();

            foreach (CardLikeState state in cardLikeStates)
            {
                bool isSearchMatch = string.IsNullOrWhiteSpace(query)
                    || NormalizeSearchText(state.Exercise.Name).Contains(query);

                bool isFilterMatch =
                    MatchesSelectedFilter(muscleGroupFilterComboBox, state.Exercise.MuscleGroupName)
                    && MatchesSelectedFilter(difficultyFilterComboBox, state.Exercise.DifficultyLevel)
                    && MatchesSelectedFilter(equipmentFilterComboBox, state.Exercise.Equipment);

                bool isMatch = isSearchMatch && isFilterMatch;

                state.CardPanel.Visible = isMatch;

                if (isMatch)
                    visibleCards.Add(state);
            }

            LayoutVisibleCards(visibleCards);
        }

        private bool MatchesSelectedFilter(ComboBox? comboBox, string value)
        {
            string selectedValue = GetSelectedFilterValue(comboBox);

            return string.IsNullOrWhiteSpace(selectedValue)
                || string.Equals(selectedValue, value, StringComparison.OrdinalIgnoreCase);
        }

        private string GetSelectedFilterValue(ComboBox? comboBox)
        {
            if (comboBox == null || comboBox.SelectedIndex <= 0)
                return "";

            return comboBox.SelectedItem?.ToString() ?? "";
        }

        private void LayoutVisibleCards(List<CardLikeState> visibleCards)
        {
            if (visibleCards.Count == 0)
            {
                int emptyContentBottom = GetExerciseCardsStartY();

                if (filterPanel != null && filterPanel.Visible)
                    emptyContentBottom = Math.Max(emptyContentBottom, filterPanel.Bottom + 24);

                latestExerciseCardsBottom = emptyContentBottom;
                AlignStatisticsPanel(emptyContentBottom);
                UpdateScrollArea(GetContentBottomWithStatistics(emptyContentBottom));
                return;
            }

            int startX = exerciseCardsStartLocation == Point.Empty ? 131 : exerciseCardsStartLocation.X;
            int startY = GetExerciseCardsStartY();

            if (filterPanel != null && filterPanel.Visible)
                startY = Math.Max(startY, filterPanel.Bottom + 24);

            int x = startX;
            int y = startY;
            int rightLimit = ClientSize.Width - 70;
            int rowGap = 42;
            int contentBottom = 0;

            foreach (CardLikeState state in visibleCards)
            {
                if (x > startX && x + state.CardPanel.Width > rightLimit)
                {
                    x = startX;
                    y += state.CardPanel.Height + rowGap;
                }

                state.CardPanel.Location = new Point(x, y);
                RoundControl(state.CardPanel, 24);
                state.HeartButton.Invalidate();
                contentBottom = Math.Max(contentBottom, state.CardPanel.Bottom);

                x += state.CardPanel.Width + exerciseCardsGap;
            }

            latestExerciseCardsBottom = contentBottom;
            AlignStatisticsPanel(contentBottom);
            UpdateScrollArea(GetContentBottomWithStatistics(contentBottom));
        }

        private int GetExerciseCardsStartY()
        {
            return exerciseCardsStartLocation == Point.Empty ? 273 : exerciseCardsStartLocation.Y;
        }

        private int GetContentBottomWithStatistics(int contentBottom)
        {
            if (statisticsPanel != null && statisticsPanel.Visible)
                return Math.Max(contentBottom, statisticsPanel.Bottom);

            return contentBottom;
        }

        private void UpdateScrollArea(List<CardLikeState> visibleCards)
        {
            int contentBottom = 0;

            foreach (CardLikeState state in visibleCards)
            {
                contentBottom = Math.Max(contentBottom, state.CardPanel.Bottom);
            }

            UpdateScrollArea(contentBottom);
        }

        private void UpdateScrollArea(int contentBottom)
        {
            int bottomPadding = 80;
            AutoScrollMinSize = new Size(0, Math.Max(ClientSize.Height, contentBottom + bottomPadding));
        }

        private string NormalizeSearchText(string text)
        {
            StringBuilder builder = new StringBuilder();

            foreach (char symbol in text)
            {
                if (!char.IsWhiteSpace(symbol))
                    builder.Append(char.ToLowerInvariant(symbol));
            }

            return builder.ToString();
        }

        private string CleanExerciseName(string text)
        {
            return text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private void AddExerciseButton_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdminAccess())
                return;

            if (!TryLoadMuscleGroups(out List<MuscleGroup> muscleGroups))
                return;

            using AddExerciseForm addExerciseForm = new AddExerciseForm(muscleGroups);

            if (addExerciseForm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                exerciseRepository.Add(
                    addExerciseForm.ExerciseName,
                    addExerciseForm.MuscleGroupId,
                    addExerciseForm.DifficultyLevel,
                    addExerciseForm.Equipment
                );

                LoadExercisesFromDatabase();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not add exercise to the database.", ex);
            }
        }

        private void EditExerciseButton_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdminAccess())
                return;

            if (sender is not Button editButton || editButton.Tag is not Exercise exercise)
                return;

            if (!TryLoadMuscleGroups(out List<MuscleGroup> muscleGroups))
                return;

            using AddExerciseForm editExerciseForm = new AddExerciseForm(muscleGroups, exercise);

            if (editExerciseForm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                exerciseRepository.Update(
                    exercise.Id,
                    editExerciseForm.ExerciseName,
                    editExerciseForm.MuscleGroupId,
                    editExerciseForm.DifficultyLevel,
                    editExerciseForm.Equipment
                );

                LoadExercisesFromDatabase();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not update exercise in the database.", ex);
            }
        }

        private void DeleteExerciseButton_Click(object? sender, EventArgs e)
        {
            if (!EnsureAdminAccess())
                return;

            if (sender is not Button deleteButton || deleteButton.Tag is not Exercise exercise)
                return;

            DialogResult result = MessageBox.Show(
                $"Delete exercise \"{exercise.Name}\"?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                exerciseRepository.Delete(exercise.Id);
                LoadExercisesFromDatabase();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not delete exercise from the database.", ex);
            }
        }

        private bool TryLoadMuscleGroups(out List<MuscleGroup> muscleGroups)
        {
            try
            {
                muscleGroups = exerciseRepository.GetMuscleGroups();
                return true;
            }
            catch (Exception ex)
            {
                muscleGroups = new List<MuscleGroup>();
                ShowDatabaseError("Could not load muscle groups from the database.", ex);
                return false;
            }
        }

        private void ShowDatabaseError(string message, Exception ex)
        {
            MessageBox.Show(
                message + Environment.NewLine + Environment.NewLine + ex.Message,
                "Database error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private void ApplyFavoriteStates()
        {
            string userLogin = GetCurrentUserLogin();

            if (userLogin == "")
                return;

            List<int> favoriteIds = exerciseRepository.GetFavoriteExerciseIds(userLogin);
            HashSet<int> favoriteIdSet = new HashSet<int>(favoriteIds);

            foreach (CardLikeState state in cardLikeStates)
            {
                state.IsLiked = favoriteIdSet.Contains(state.Exercise.Id);
                ApplyCardTheme(state);
            }
        }

        private string GetCurrentUserLogin()
        {
            return Authorization.User?.Trim() ?? "";
        }

        private bool EnsureAdminAccess()
        {
            if (IsAdmin)
                return true;

            MessageBox.Show("Only admin can change exercises.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void AdminButton_Click(object sender, EventArgs e)
        {
            Hide();

            global::PracticaApp.LoginForm1 loginForm = new global::PracticaApp.LoginForm1();
            loginForm.Show();
        }

        private void Admin_Load(object sender, EventArgs e)
        {

        }

        private void LikedExercisesButton_Click(object? sender, EventArgs e)
        {
            string userLogin = GetCurrentUserLogin();

            if (userLogin == "")
            {
                MessageBox.Show("Please log in before using liked exercises.", "Liked exercises", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using LikedExercisesForm likedExercisesForm = new LikedExercisesForm(userLogin);
            likedExercisesForm.ShowDialog(this);

            try
            {
                ApplyFavoriteStates();
                FilterExerciseCards(textBox1.Text);
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not refresh liked exercises.", ex);
            }
        }

        private void heartButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button heartButton || heartButton.Tag is not CardLikeState state)
                return;

            string userLogin = GetCurrentUserLogin();

            if (userLogin == "")
            {
                MessageBox.Show("Please log in before liking exercises.", "Liked exercises", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool newLikedState = !state.IsLiked;

            try
            {
                exerciseRepository.SetFavorite(userLogin, state.Exercise.Id, newLikedState);
                state.IsLiked = newLikedState;
                ApplyCardTheme(state);
                RoundControl(state.CardPanel, 24);
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not update liked exercises.", ex);
            }
        }

        private void ApplyCardTheme(CardLikeState state)
        {
            Color cardColor = state.IsLiked ? likedCardColor : normalCardColor;
            Color heartColor = state.IsLiked ? likedHeartColor : normalHeartColor;
            Color borderColor = state.IsLiked ? likedHeartColor : normalHeartBorderColor;
            state.BorderColor = borderColor;

            state.CardPanel.BackColor = cardColor;
            state.HeartPanel.BackColor = cardColor;
            SetCardSurfaceColor(state.CardPanel, cardColor);

            state.HeartButton.Region = null;
            state.HeartButton.BackColor = cardColor;
            state.HeartButton.FlatAppearance.MouseOverBackColor = cardColor;
            state.HeartButton.FlatAppearance.MouseDownBackColor = cardColor;
            state.HeartButton.ForeColor = heartColor;
            state.HeartButton.FlatAppearance.BorderColor = borderColor;
            state.HeartButton.Invalidate();
            state.CardPanel.Invalidate();

            foreach (Control child in state.CardPanel.Controls)
            {
                if (child is Button button && button.Name == "EditExerciseButton")
                {
                    button.BackColor = cardColor;
                    button.ForeColor = ThemeManager.Accent;
                    button.FlatAppearance.MouseOverBackColor = cardColor;
                    button.FlatAppearance.MouseDownBackColor = cardColor;
                    button.Invalidate();
                }
            }
        }

        private void SetCardSurfaceColor(Panel cardPanel, Color cardColor)
        {
            foreach (Control child in cardPanel.Controls)
            {
                if (child is Label || child is PictureBox)
                    child.BackColor = cardColor;

                if (child is Label label)
                {
                    label.ForeColor = label.Text == "About"
                        ? ThemeManager.MutedText
                        : ThemeManager.Text;
                }
            }
        }

        private void EditButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(button.BackColor);

            Rectangle borderRect = new Rectangle(3, 3, button.Width - 7, button.Height - 7);

            using (GraphicsPath path = GetRoundedPath(borderRect, 10))
            using (Pen pen = new Pen(ThemeManager.Accent, 2.2F))
            {
                e.Graphics.DrawPath(pen, path);
            }

            TextRenderer.DrawText(
                e.Graphics,
                "EDIT",
                button.Font,
                button.ClientRectangle,
                ThemeManager.Accent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
            );
        }

        private void heartButton_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Button heartButton || heartButton.Tag is not CardLikeState state)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(2, 2, heartButton.Width - 5, heartButton.Height - 5);

            using (GraphicsPath path = GetRoundedPath(rect, 10))
            using (Pen pen = new Pen(state.BorderColor, 3))
            {
                e.Graphics.DrawPath(pen, path);
            }

            string heart = state.IsLiked ? "\u2665" : "\u2661";
            Rectangle textRect = new Rectangle(0, 1, heartButton.Width, heartButton.Height);

            using (SolidBrush brush = new SolidBrush(state.IsLiked ? likedHeartColor : normalHeartColor))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(heart, heartButton.Font, brush, textRect, format);
            }
        }

        private void SearchPanel_Paint(object? sender, PaintEventArgs e)
        {
            DrawRoundedSurfaceBorder(e.Graphics, panel1.ClientRectangle, 20, GetSurfaceOutlineColor());
        }

        private void FilterPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is Control control)
                DrawRoundedSurfaceBorder(e.Graphics, control.ClientRectangle, 18, GetSurfaceOutlineColor());
        }

        private void CardPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            if (ThemeManager.IsDark)
                return;

            Color borderColor = GetCardOutlineColor(panel);
            DrawRoundedSurfaceBorder(e.Graphics, panel.ClientRectangle, 24, borderColor);
        }

        private Color GetCardOutlineColor(Panel panel)
        {
            if (panel.Tag is Exercise && !ThemeManager.IsDark)
                return Color.FromArgb(45, 45, 52);

            return ThemeManager.Border;
        }

        private Color GetSurfaceOutlineColor()
        {
            return ThemeManager.IsDark
                ? ThemeManager.Border
                : Color.FromArgb(35, 35, 42);
        }

        private void DrawRoundedSurfaceBorder(Graphics graphics, Rectangle bounds, int radius, Color color)
        {
            if (bounds.Width <= 2 || bounds.Height <= 2)
                return;

            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(
                bounds.X + 1,
                bounds.Y + 1,
                bounds.Width - 3,
                bounds.Height - 3
            );

            using GraphicsPath path = GetRoundedPath(rect, radius);
            using Pen pen = new Pen(color, ThemeManager.IsDark ? 1.5F : 2F);
            graphics.DrawPath(pen, path);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }

        private sealed class StatisticCardState
        {
            public StatisticCardState(
                Panel cardPanel,
                string title,
                string unit,
                decimal current,
                decimal target,
                bool allowDecimal,
                Color accentColor,
                string iconKey
            )
            {
                CardPanel = cardPanel;
                Title = title;
                Unit = unit;
                Current = current;
                Target = target;
                AllowDecimal = allowDecimal;
                AccentColor = accentColor;
                IconKey = iconKey;
            }

            public Panel CardPanel { get; }
            public string Title { get; }
            public string Unit { get; }
            public decimal Current { get; set; }
            public decimal Target { get; set; }
            public bool AllowDecimal { get; }
            public Color AccentColor { get; }
            public string IconKey { get; }

            public string DisplayText
            {
                get
                {
                    if (Unit == "")
                        return $"{Title} {FormatNumber(Current, AllowDecimal)} / {FormatNumber(Target, AllowDecimal)}";

                    return $"{Title} {FormatNumber(Current, AllowDecimal)} {Unit} / {FormatNumber(Target, AllowDecimal)} {Unit}";
                }
            }

            private static string FormatNumber(decimal value, bool allowDecimal)
            {
                if (allowDecimal)
                    return value.ToString("0.#", CultureInfo.InvariantCulture);

                return decimal.ToInt32(Math.Round(value, 0)).ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
            }
        }

        private sealed class StatisticProgressData
        {
            public decimal Current { get; set; }
            public decimal Target { get; set; }
        }

        private sealed class CardLikeState
        {
            public CardLikeState(Panel cardPanel, Panel heartPanel, Button heartButton, Exercise exercise)
            {
                CardPanel = cardPanel;
                HeartPanel = heartPanel;
                HeartButton = heartButton;
                Exercise = exercise;
            }

            public Panel CardPanel { get; }
            public Panel HeartPanel { get; }
            public Button HeartButton { get; }
            public Exercise Exercise { get; }
            public bool IsLiked { get; set; }
            public Color BorderColor { get; set; } = Color.FromArgb(85, 85, 95);
        }

    }
}
