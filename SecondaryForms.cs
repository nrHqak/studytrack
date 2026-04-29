using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudyTrack
{
    public class AddTaskForm : Form
    {
        public AddTaskForm()
        {
            Text = "Добавить задачу";
            Size = new Size(700, 420);
            MinimumSize = new Size(560, 360);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorTranslator.FromHtml("#1E1E2E");
            ForeColor = Color.FromArgb(245, 224, 220);
            Font = new Font("Segoe UI", 12F);

            Label title = new Label
            {
                Text = "Экран добавления задач",
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold)
            };

            Label hint = new Label
            {
                Text = "Здесь можно реализовать полноценную форму добавления задач.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.FromArgb(186, 194, 222)
            };

            Controls.Add(hint);
            Controls.Add(title);
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
