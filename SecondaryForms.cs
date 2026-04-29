using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudyTrack
{
    public class AddTaskForm : Form
    {
        private readonly FileManager _fileManager;
        private readonly Action _onTaskAdded;

        private TextBox _subjectTextBox;
        private TextBox _titleTextBox;
        private CheckBox _completedCheckBox;

        public AddTaskForm(FileManager fileManager, Action onTaskAdded)
        {
            _fileManager = fileManager;
            _onTaskAdded = onTaskAdded;

            Text = "Добавить задачу";
            Size = new Size(720, 460);
            MinimumSize = new Size(620, 430);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorTranslator.FromHtml("#1E1E2E");
            ForeColor = Color.FromArgb(245, 224, 220);
            Font = new Font("Segoe UI", 11F);

            BuildUi();
        }

        private void BuildUi()
        {
            Label title = new Label
            {
                Text = "Добавление новой задачи",
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            TableLayoutPanel formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 8, 24, 16),
                ColumnCount = 2,
                RowCount = 4
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _subjectTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Например: Математика" };
            _titleTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Например: Пределы" };
            _completedCheckBox = new CheckBox { Text = "Уже выполнено", AutoSize = true, ForeColor = ForeColor };

            formGrid.Controls.Add(CreateFieldLabel("Предмет:"), 0, 0);
            formGrid.Controls.Add(_subjectTextBox, 1, 0);
            formGrid.Controls.Add(CreateFieldLabel("Тема задачи:"), 0, 1);
            formGrid.Controls.Add(_titleTextBox, 1, 1);
            formGrid.Controls.Add(new Label(), 0, 2);
            formGrid.Controls.Add(_completedCheckBox, 1, 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 12, 0, 0)
            };

            Button saveButton = CreateButton("Сохранить", ColorTranslator.FromHtml("#CBA6F7"), ColorTranslator.FromHtml("#1E1E2E"));
            Button cancelButton = CreateButton("Отмена", Color.FromArgb(69, 71, 90), Color.FromArgb(205, 214, 244));

            saveButton.Click += SaveButton_Click;
            cancelButton.Click += (s, e) => Close();

            buttons.Controls.Add(saveButton);
            buttons.Controls.Add(cancelButton);
            formGrid.Controls.Add(buttons, 1, 3);

            Controls.Add(formGrid);
            Controls.Add(title);
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
        }

        private Button CreateButton(string text, Color back, Color fore)
        {
            Button button = new Button
            {
                Text = text,
                Width = 130,
                Height = 38,
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0, 0, 10, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string subject = _subjectTextBox.Text.Trim();
            string title = _titleTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Заполните предмет и тему задачи.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tasks = _fileManager.LoadTasks();
            bool duplicateExists = tasks.Any(t =>
                t.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase)
                && t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                MessageBox.Show("Такая задача уже существует.", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            tasks.Add(new StudyTask(title, _completedCheckBox.Checked, subject));
            _fileManager.SaveTasks(tasks);
            _onTaskAdded?.Invoke();

            MessageBox.Show("Задача успешно добавлена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }

    public class ProfileForm : Form
    {
        public ProfileForm()
        {
            Text = "Профиль";
            Size = new Size(700, 420);
            MinimumSize = new Size(560, 360);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorTranslator.FromHtml("#1E1E2E");
            ForeColor = Color.FromArgb(245, 224, 220);
            Font = new Font("Segoe UI", 12F);

            Label title = new Label
            {
                Text = "Экран профиля",
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            Label hint = new Label
            {
                Text = "Отдельная форма профиля открывается через нижнюю навигацию.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.FromArgb(186, 194, 222)
            };

            Controls.Add(hint);
            Controls.Add(title);
        }
    }
}
