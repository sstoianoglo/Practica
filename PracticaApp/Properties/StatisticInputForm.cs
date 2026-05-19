using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace PracticaApp.Properties
{
    internal sealed class StatisticInputForm : Form
    {
        private readonly string statisticName;
        private readonly string unit;
        private readonly bool allowDecimal;

        private readonly Label titleLabel = new Label();
        private readonly Label currentLabel = new Label();
        private readonly Label targetLabel = new Label();
        private readonly TextBox currentTextBox = new TextBox();
        private readonly TextBox targetTextBox = new TextBox();
        private readonly Button saveButton = new Button();
        private readonly Button cancelButton = new Button();

        public StatisticInputForm(string statisticName, string unit, decimal currentValue, decimal targetValue, bool allowDecimal)
        {
            this.statisticName = statisticName;
            this.unit = unit;
            this.allowDecimal = allowDecimal;

            CurrentValue = currentValue;
            TargetValue = targetValue;

            Text = statisticName;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(360, 230);
            AutoScaleMode = AutoScaleMode.Font;

            InitializeControls();
            currentTextBox.Text = FormatInputValue(currentValue);
            targetTextBox.Text = FormatInputValue(targetValue);
            ApplyTheme();
        }

        public decimal CurrentValue { get; private set; }
        public decimal TargetValue { get; private set; }

        private void InitializeControls()
        {
            titleLabel.Text = statisticName;
            titleLabel.AutoSize = false;
            titleLabel.Location = new Point(24, 20);
            titleLabel.Size = new Size(312, 36);
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);

            currentLabel.Text = unit == "" ? "Current value" : $"Current value ({unit})";
            currentLabel.AutoSize = false;
            currentLabel.Location = new Point(24, 70);
            currentLabel.Size = new Size(140, 24);
            currentLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            currentTextBox.Location = new Point(176, 68);
            currentTextBox.Size = new Size(160, 28);
            currentTextBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            targetLabel.Text = unit == "" ? "Goal" : $"Goal ({unit})";
            targetLabel.AutoSize = false;
            targetLabel.Location = new Point(24, 112);
            targetLabel.Size = new Size(140, 24);
            targetLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            targetTextBox.Location = new Point(176, 110);
            targetTextBox.Size = new Size(160, 28);
            targetTextBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            saveButton.Text = "Save";
            saveButton.Location = new Point(88, 172);
            saveButton.Size = new Size(110, 38);
            saveButton.Click += SaveButton_Click;

            cancelButton.Text = "Cancel";
            cancelButton.Location = new Point(214, 172);
            cancelButton.Size = new Size(110, 38);
            cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(titleLabel);
            Controls.Add(currentLabel);
            Controls.Add(currentTextBox);
            Controls.Add(targetLabel);
            Controls.Add(targetTextBox);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void ApplyTheme()
        {
            BackColor = ThemeManager.Background;

            titleLabel.ForeColor = ThemeManager.Text;
            currentLabel.ForeColor = ThemeManager.MutedText;
            targetLabel.ForeColor = ThemeManager.MutedText;

            currentTextBox.BackColor = ThemeManager.InputBack;
            currentTextBox.ForeColor = ThemeManager.Text;
            targetTextBox.BackColor = ThemeManager.InputBack;
            targetTextBox.ForeColor = ThemeManager.Text;

            StyleButton(saveButton, true);
            StyleButton(cancelButton, false);
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

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!TryReadValue(currentTextBox.Text, out decimal current))
            {
                ShowValidationMessage("Current value is not valid.");
                currentTextBox.Focus();
                return;
            }

            if (!TryReadValue(targetTextBox.Text, out decimal target))
            {
                ShowValidationMessage("Goal value is not valid.");
                targetTextBox.Focus();
                return;
            }

            if (current < 0)
            {
                ShowValidationMessage("Current value cannot be negative.");
                currentTextBox.Focus();
                return;
            }

            if (target <= 0)
            {
                ShowValidationMessage("Goal must be greater than zero.");
                targetTextBox.Focus();
                return;
            }

            CurrentValue = current;
            TargetValue = target;
            DialogResult = DialogResult.OK;
        }

        private bool TryReadValue(string text, out decimal value)
        {
            string normalizedText = text.Trim().Replace(" ", "");

            bool parsed = decimal.TryParse(normalizedText, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
                || decimal.TryParse(normalizedText, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

            if (!parsed)
                return false;

            return allowDecimal || value == decimal.Truncate(value);
        }

        private string FormatInputValue(decimal value)
        {
            if (allowDecimal)
                return value.ToString("0.#", CultureInfo.InvariantCulture);

            return decimal.ToInt32(Math.Round(value, 0)).ToString(CultureInfo.InvariantCulture);
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(message, statisticName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
