using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PracticaApp.Properties
{
    internal sealed class AddExerciseForm : Form
    {
        private const int MaxExerciseNameLength = 100;
        private const int MaxDifficultyLength = 30;
        private const int MaxEquipmentLength = 50;

        private readonly TextBox nameTextBox;
        private readonly TextBox equipmentTextBox;
        private readonly ComboBox muscleGroupComboBox;
        private readonly ComboBox difficultyComboBox;
        private readonly ErrorProvider errorProvider;
        private readonly List<MuscleGroup> muscleGroups;

        public AddExerciseForm(List<MuscleGroup> muscleGroups)
            : this(muscleGroups, null)
        {
        }

        public AddExerciseForm(List<MuscleGroup> muscleGroups, Exercise? exercise)
        {
            this.muscleGroups = muscleGroups;
            bool isEditMode = exercise != null;

            Text = isEditMode ? "Edit exercise" : "Add exercise";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 420);
            BackColor = ThemeManager.Surface;

            errorProvider = new ErrorProvider
            {
                ContainerControl = this,
                BlinkStyle = ErrorBlinkStyle.NeverBlink
            };

            Label titleLabel = new Label
            {
                Text = isEditMode ? "Edit Exercise" : "Add Exercise",
                ForeColor = ThemeManager.Text,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                Size = new Size(300, 36)
            };

            Label nameLabel = CreateLabel("Name", 74);
            nameTextBox = CreateTextBox(104);
            nameTextBox.MaxLength = MaxExerciseNameLength;
            nameTextBox.TextChanged += (s, e) => errorProvider.SetError(nameTextBox, "");

            Label groupLabel = CreateLabel("Muscle group", 142);
            muscleGroupComboBox = CreateComboBox(172);
            muscleGroupComboBox.SelectedIndexChanged += (s, e) => errorProvider.SetError(muscleGroupComboBox, "");

            foreach (MuscleGroup group in muscleGroups)
            {
                muscleGroupComboBox.Items.Add(group);
            }

            SelectMuscleGroup(exercise?.MuscleGroupId ?? 0);

            Label difficultyLabel = CreateLabel("Difficulty", 210);
            difficultyComboBox = CreateComboBox(240);
            difficultyComboBox.Items.AddRange(new object[] { "Easy", "Medium", "Hard" });
            difficultyComboBox.SelectedIndexChanged += (s, e) => errorProvider.SetError(difficultyComboBox, "");
            difficultyComboBox.SelectedItem = string.IsNullOrWhiteSpace(exercise?.DifficultyLevel)
                ? "Medium"
                : exercise.DifficultyLevel;

            if (difficultyComboBox.SelectedIndex < 0)
                difficultyComboBox.SelectedIndex = 1;

            Label equipmentLabel = CreateLabel("Equipment", 278);
            equipmentTextBox = CreateTextBox(308);
            equipmentTextBox.MaxLength = MaxEquipmentLength;
            equipmentTextBox.TextChanged += (s, e) => errorProvider.SetError(equipmentTextBox, "");

            Button saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                BackColor = Color.DarkOrange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(250, 366),
                Size = new Size(90, 34)
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;

            Button cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(346, 366),
                Size = new Size(90, 34)
            };
            cancelButton.FlatAppearance.BorderSize = 0;

            Controls.Add(titleLabel);
            Controls.Add(nameLabel);
            Controls.Add(nameTextBox);
            Controls.Add(groupLabel);
            Controls.Add(muscleGroupComboBox);
            Controls.Add(difficultyLabel);
            Controls.Add(difficultyComboBox);
            Controls.Add(equipmentLabel);
            Controls.Add(equipmentTextBox);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            if (exercise != null)
            {
                nameTextBox.Text = exercise.Name;
                equipmentTextBox.Text = exercise.Equipment;
                nameTextBox.SelectAll();
            }

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        public string ExerciseName => nameTextBox.Text.Trim();
        public string Equipment => equipmentTextBox.Text.Trim();
        public string DifficultyLevel => difficultyComboBox.SelectedItem?.ToString() ?? "Medium";

        public int MuscleGroupId
        {
            get
            {
                return muscleGroupComboBox.SelectedItem is MuscleGroup group
                    ? group.Id
                    : 1;
            }
        }

        private Label CreateLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                ForeColor = ThemeManager.MutedText,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Location = new Point(30, top),
                Size = new Size(160, 24)
            };
        }

        private TextBox CreateTextBox(int top)
        {
            return new TextBox
            {
                Location = new Point(30, top),
                Size = new Size(406, 28),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private ComboBox CreateComboBox(int top)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(30, top),
                Size = new Size(406, 28),
                BackColor = ThemeManager.InputBack,
                ForeColor = ThemeManager.Text
            };
        }

        private void SelectMuscleGroup(int muscleGroupId)
        {
            if (muscleGroupComboBox.Items.Count == 0)
                return;

            for (int index = 0; index < muscleGroupComboBox.Items.Count; index++)
            {
                if (muscleGroupComboBox.Items[index] is MuscleGroup group && group.Id == muscleGroupId)
                {
                    muscleGroupComboBox.SelectedIndex = index;
                    return;
                }
            }

            muscleGroupComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateExerciseForm())
                DialogResult = DialogResult.None;
        }

        private bool ValidateExerciseForm()
        {
            errorProvider.Clear();

            Control? firstInvalidControl = null;
            List<string> messages = new List<string>();

            ValidateTextBox(
                nameTextBox,
                ExerciseName,
                "Write exercise name.",
                MaxExerciseNameLength,
                "Exercise name",
                messages,
                ref firstInvalidControl
            );

            if (muscleGroups.Count == 0)
            {
                AddValidationError(
                    muscleGroupComboBox,
                    "No muscle groups found in the database.",
                    messages,
                    ref firstInvalidControl
                );
            }
            else if (muscleGroupComboBox.SelectedItem is not MuscleGroup)
            {
                AddValidationError(
                    muscleGroupComboBox,
                    "Choose muscle group.",
                    messages,
                    ref firstInvalidControl
                );
            }

            ValidateTextBox(
                difficultyComboBox,
                DifficultyLevel,
                "Choose difficulty level.",
                MaxDifficultyLength,
                "Difficulty level",
                messages,
                ref firstInvalidControl
            );

            ValidateTextBox(
                equipmentTextBox,
                Equipment,
                "Write equipment.",
                MaxEquipmentLength,
                "Equipment",
                messages,
                ref firstInvalidControl
            );

            if (messages.Count == 0)
                return true;

            firstInvalidControl?.Focus();
            MessageBox.Show(
                string.Join(Environment.NewLine, messages),
                "Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return false;
        }

        private void ValidateTextBox(
            Control control,
            string value,
            string requiredMessage,
            int maxLength,
            string fieldName,
            List<string> messages,
            ref Control? firstInvalidControl
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddValidationError(control, requiredMessage, messages, ref firstInvalidControl);
                return;
            }

            if (value.Length > maxLength)
            {
                AddValidationError(
                    control,
                    $"{fieldName} must be at most {maxLength} characters.",
                    messages,
                    ref firstInvalidControl
                );
            }
        }

        private void AddValidationError(
            Control control,
            string message,
            List<string> messages,
            ref Control? firstInvalidControl
        )
        {
            errorProvider.SetError(control, message);
            messages.Add(message);
            firstInvalidControl ??= control;
        }
    }
}
