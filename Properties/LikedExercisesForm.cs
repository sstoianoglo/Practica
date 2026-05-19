using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PracticaApp.Properties
{
    internal sealed class LikedExercisesForm : Form
    {
        private readonly ExerciseRepository exerciseRepository = new ExerciseRepository();
        private readonly string userLogin;
        private readonly string[] trainingDays =
        {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
        };

        private readonly Label titleLabel = new Label();
        private readonly Label dayLabel = new Label();
        private readonly Label favoritesLabel = new Label();
        private readonly Label workoutLabel = new Label();
        private readonly ComboBox dayComboBox = new ComboBox();
        private readonly ListBox favoritesListBox = new ListBox();
        private readonly ListBox workoutListBox = new ListBox();
        private readonly Button addToDayButton = new Button();
        private readonly Button removeButton = new Button();
        private readonly Button exportButton = new Button();
        private readonly Button refreshButton = new Button();
        private readonly Button closeButton = new Button();

        public LikedExercisesForm(string userLogin)
        {
            this.userLogin = userLogin.Trim();

            Text = "Liked exercises";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(940, 620);
            MinimumSize = new Size(780, 500);
            AutoScaleMode = AutoScaleMode.Font;

            InitializeControls();
            ApplyTheme();

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            FormClosed += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            Load += (s, e) => LoadData();
            Resize += (s, e) => LayoutControls();
        }

        private void InitializeControls()
        {
            titleLabel.Text = "Liked exercises";
            titleLabel.AutoSize = false;
            titleLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);

            dayLabel.Text = "Training day";
            dayLabel.AutoSize = false;
            dayLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            favoritesLabel.Text = "Liked";
            favoritesLabel.AutoSize = false;
            favoritesLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            workoutLabel.Text = "Workout";
            workoutLabel.AutoSize = false;
            workoutLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            dayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dayComboBox.Items.AddRange(trainingDays);
            dayComboBox.SelectedIndex = 0;
            dayComboBox.SelectedIndexChanged += (s, e) => LoadWorkoutForSelectedDay();

            favoritesListBox.DisplayMember = nameof(Exercise.Name);
            favoritesListBox.IntegralHeight = false;
            favoritesListBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            workoutListBox.DisplayMember = nameof(WorkoutPlanEntry.DisplayText);
            workoutListBox.IntegralHeight = false;
            workoutListBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            addToDayButton.Text = "Add to day";
            addToDayButton.Click += AddToDayButton_Click;

            removeButton.Text = "Remove";
            removeButton.Click += RemoveButton_Click;

            exportButton.Text = "Export TXT";
            exportButton.Click += ExportButton_Click;

            refreshButton.Text = "Refresh";
            refreshButton.Click += (s, e) => LoadData();

            closeButton.Text = "Close";
            closeButton.Click += (s, e) => Close();

            Controls.Add(titleLabel);
            Controls.Add(dayLabel);
            Controls.Add(dayComboBox);
            Controls.Add(favoritesLabel);
            Controls.Add(workoutLabel);
            Controls.Add(favoritesListBox);
            Controls.Add(workoutListBox);
            Controls.Add(addToDayButton);
            Controls.Add(removeButton);
            Controls.Add(exportButton);
            Controls.Add(refreshButton);
            Controls.Add(closeButton);

            LayoutControls();
        }

        private void LayoutControls()
        {
            int margin = 28;
            int top = 24;
            int buttonWidth = 128;
            int buttonHeight = 38;
            int middleWidth = 148;
            int gap = 22;
            int bottomButtonsTop = ClientSize.Height - buttonHeight - margin;
            int listTop = 150;
            int listHeight = Math.Max(120, bottomButtonsTop - listTop - 20);
            int availableWidth = ClientSize.Width - (margin * 2) - middleWidth - (gap * 2);
            int listWidth = Math.Max(220, availableWidth / 2);

            titleLabel.Location = new Point(margin, top);
            titleLabel.Size = new Size(ClientSize.Width - (margin * 2), 48);

            dayLabel.Location = new Point(margin, titleLabel.Bottom + 20);
            dayLabel.Size = new Size(120, 30);

            dayComboBox.Location = new Point(dayLabel.Right + 12, titleLabel.Bottom + 18);
            dayComboBox.Size = new Size(190, 30);

            favoritesLabel.Location = new Point(margin, listTop - 34);
            favoritesLabel.Size = new Size(listWidth, 26);

            favoritesListBox.Location = new Point(margin, listTop);
            favoritesListBox.Size = new Size(listWidth, listHeight);

            int middleLeft = favoritesListBox.Right + gap;
            addToDayButton.Location = new Point(middleLeft + 10, listTop + 56);
            addToDayButton.Size = new Size(buttonWidth, buttonHeight);

            removeButton.Location = new Point(middleLeft + 10, addToDayButton.Bottom + 14);
            removeButton.Size = new Size(buttonWidth, buttonHeight);

            int workoutLeft = middleLeft + middleWidth + gap;
            workoutLabel.Location = new Point(workoutLeft, listTop - 34);
            workoutLabel.Size = new Size(listWidth, 26);

            workoutListBox.Location = new Point(workoutLeft, listTop);
            workoutListBox.Size = new Size(listWidth, listHeight);

            int bottomButtonsWidth = (buttonWidth * 3) + 24;
            int bottomButtonsLeft = Math.Max(margin, ClientSize.Width - margin - bottomButtonsWidth);

            exportButton.Location = new Point(bottomButtonsLeft, bottomButtonsTop);
            exportButton.Size = new Size(buttonWidth, buttonHeight);

            refreshButton.Location = new Point(exportButton.Right + 12, bottomButtonsTop);
            refreshButton.Size = new Size(buttonWidth, buttonHeight);

            closeButton.Location = new Point(refreshButton.Right + 12, bottomButtonsTop);
            closeButton.Size = new Size(buttonWidth, buttonHeight);
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;

            titleLabel.ForeColor = ThemeManager.Text;
            dayLabel.ForeColor = ThemeManager.MutedText;
            favoritesLabel.ForeColor = ThemeManager.Text;
            workoutLabel.ForeColor = ThemeManager.Text;

            StyleListBox(favoritesListBox);
            StyleListBox(workoutListBox);

            dayComboBox.BackColor = ThemeManager.InputBack;
            dayComboBox.ForeColor = ThemeManager.Text;

            StyleButton(addToDayButton, true);
            StyleButton(removeButton, false);
            StyleButton(exportButton, true);
            StyleButton(refreshButton, false);
            StyleButton(closeButton, false);
        }

        private void StyleListBox(ListBox listBox)
        {
            listBox.BackColor = ThemeManager.InputBack;
            listBox.ForeColor = ThemeManager.Text;
            listBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.UseVisualStyleBackColor = false;
            button.BackColor = primary ? ThemeManager.Accent : ThemeManager.Surface;
            button.ForeColor = primary ? Color.White : ThemeManager.Accent;
            button.FlatAppearance.BorderColor = primary ? ThemeManager.Accent : ThemeManager.Border;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(255, 150, 24) : ThemeManager.SurfaceHover;
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(210, 104, 0) : ThemeManager.SurfaceDown;
        }

        private void LoadData()
        {
            try
            {
                exerciseRepository.EnsureTable();
                LoadFavorites();
                LoadWorkoutForSelectedDay();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not load liked exercises.", ex);
            }
        }

        private void LoadFavorites()
        {
            List<Exercise> exercises = exerciseRepository.GetFavoriteExercises(userLogin);

            favoritesListBox.BeginUpdate();
            favoritesListBox.Items.Clear();

            foreach (Exercise exercise in exercises)
            {
                favoritesListBox.Items.Add(exercise);
            }

            favoritesListBox.EndUpdate();
            favoritesLabel.Text = $"Liked ({exercises.Count})";
        }

        private void LoadWorkoutForSelectedDay()
        {
            if (!IsHandleCreated)
                return;

            try
            {
                string trainingDay = GetSelectedDay();
                List<WorkoutPlanEntry> workoutPlan = exerciseRepository.GetWorkoutPlan(userLogin, trainingDay);

                workoutListBox.BeginUpdate();
                workoutListBox.Items.Clear();

                foreach (WorkoutPlanEntry entry in workoutPlan)
                {
                    workoutListBox.Items.Add(entry);
                }

                workoutListBox.EndUpdate();
                workoutLabel.Text = $"{trainingDay} workout ({workoutPlan.Count})";
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not load workout plan.", ex);
            }
        }

        private string GetSelectedDay()
        {
            return dayComboBox.SelectedItem?.ToString() ?? trainingDays[0];
        }

        private void AddToDayButton_Click(object? sender, EventArgs e)
        {
            if (favoritesListBox.SelectedItem is not Exercise exercise)
            {
                MessageBox.Show("Select liked exercise first.", "Liked exercises", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                bool added = exerciseRepository.AddWorkoutPlanExercise(userLogin, GetSelectedDay(), exercise.Id);

                if (!added)
                {
                    MessageBox.Show("This exercise is already in the selected day.", "Workout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoadWorkoutForSelectedDay();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not add exercise to the workout.", ex);
            }
        }

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            if (workoutListBox.SelectedItem is not WorkoutPlanEntry entry)
            {
                MessageBox.Show("Select workout exercise first.", "Workout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                exerciseRepository.RemoveWorkoutPlanExercise(userLogin, entry.Id);
                LoadWorkoutForSelectedDay();
            }
            catch (Exception ex)
            {
                ShowDatabaseError("Could not remove exercise from the workout.", ex);
            }
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export workout plan",
                Filter = "Text file (*.txt)|*.txt",
                FileName = $"FitPro_Workout_{SanitizeFileName(userLogin)}_{DateTime.Now:yyyyMMdd}.txt",
                OverwritePrompt = true
            };

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string exportText = BuildWorkoutPlanExportText();
                File.WriteAllText(saveFileDialog.FileName, exportText, new UTF8Encoding(true));

                MessageBox.Show("Workout plan exported successfully.", "Export TXT", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not export workout plan." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Export TXT",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string BuildWorkoutPlanExportText()
        {
            exerciseRepository.EnsureTable();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("FitPro workout plan");
            builder.AppendLine($"User: {userLogin}");
            builder.AppendLine($"Export date: {DateTime.Now:dd.MM.yyyy HH:mm}");
            builder.AppendLine();

            foreach (string trainingDay in trainingDays)
            {
                List<WorkoutPlanEntry> workoutPlan = exerciseRepository.GetWorkoutPlan(userLogin, trainingDay);
                AppendTrainingDay(builder, trainingDay, workoutPlan);
            }

            return builder.ToString();
        }

        private void AppendTrainingDay(StringBuilder builder, string trainingDay, List<WorkoutPlanEntry> workoutPlan)
        {
            builder.AppendLine(trainingDay);

            if (workoutPlan.Count == 0)
            {
                builder.AppendLine("No exercises");
                builder.AppendLine();
                return;
            }

            for (int index = 0; index < workoutPlan.Count; index++)
            {
                WorkoutPlanEntry entry = workoutPlan[index];
                Exercise exercise = entry.Exercise;
                int setsCount = entry.SetsCount > 0 ? entry.SetsCount : 3;
                int repsCount = entry.RepsCount > 0 ? entry.RepsCount : 10;
                int restSeconds = entry.RestSeconds > 0 ? entry.RestSeconds : 60;

                builder.AppendLine($"{index + 1}. {exercise.Name}");
                builder.AppendLine($"   Muscle group: {EmptyToDash(exercise.MuscleGroupName)}");
                builder.AppendLine($"   Difficulty: {EmptyToDash(exercise.DifficultyLevel)}");
                builder.AppendLine($"   Equipment: {EmptyToDash(exercise.Equipment)}");
                builder.AppendLine($"   Sets: {setsCount}");
                builder.AppendLine($"   Reps: {repsCount}");
                builder.AppendLine($"   Rest: {restSeconds} sec");
            }

            builder.AppendLine();
        }

        private string EmptyToDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private string SanitizeFileName(string value)
        {
            string cleanValue = string.IsNullOrWhiteSpace(value) ? "User" : value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                cleanValue = cleanValue.Replace(invalidChar, '_');
            }

            return cleanValue;
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
    }
}
