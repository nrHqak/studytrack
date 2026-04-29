using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace StudyTrack
{
    /// <summary>
    /// Главный экран приложения StudyTrack.
    /// </summary>
    public class Form1 : Form
    {
        private readonly Color _bgMain = ColorTranslator.FromHtml("#1E1E2E");
        private readonly Color _bgCard = Color.FromArgb(42, 46, 68);
        private readonly Color _accent = ColorTranslator.FromHtml("#CBA6F7");
        private readonly Color _textPrimary = Color.FromArgb(245, 224, 220);
        private readonly Color _textSecondary = Color.FromArgb(186, 194, 222);

        private readonly DateTime _studyStartDate = DateTime.Parse("2025-09-01");
        private readonly Exam _exam = new Exam("МЭСК", DateTime.Parse("2026-05-25"), "Комплексный экзамен");

        private readonly Timer _timer = new Timer();
        private readonly ProgressCalculator _progressCalculator = new ProgressCalculator();
        private readonly FileManager _fileManager;

        private List<StudyTask> _tasks;

        private Label _countdownValueLabel;
        private Label _timePassedLabel;
        private CustomProgressBar _timelineProgress;
        private Label _progressLabel;
        private CustomProgressBar _topicProgress;
        private DataGridView _tasksGrid;
        private Label _titleLabel;
        private Panel _headerPanel;
        private Button _closeButton;

        private bool _dragging;
        private Point _dragStart;

        /// <summary>
        /// Инициализирует форму, данные и UI.
        /// </summary>
        public Form1()
        {
            _fileManager = new FileManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.txt"));

            InitializeForm();
            InitializeData();
            BuildUI();
            UpdateTimerBlock();
            UpdateProgressBlock();
            ConfigureTimer();
        }

        /// <summary>
        /// Настраивает базовые параметры формы.
        /// </summary>
        private void InitializeForm()
        {
            Text = "StudyTrack";
            BackColor = _bgMain;
            ForeColor = _textPrimary;
            Size = new Size(1120, 760);
            MinimumSize = new Size(980, 680);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            DoubleBuffered = true;
            Padding = new Padding(16);
        }

        /// <summary>
        /// Загружает задачи из файла или создаёт список по умолчанию.
        /// </summary>
        private void InitializeData()
        {
            _tasks = _fileManager.LoadTasks();
            if (_tasks.Count == 0)
            {
                _tasks = new List<StudyTask>
                {
                    new StudyTask("XX век", false, "История Казахстана"),
                    new StudyTask("Прогрессия", false, "Математика"),
                    new StudyTask("Законы Кирхгофа", false, "Физика"),
                    new StudyTask("Сравнение текстов", false, "Казахский язык"),
                    new StudyTask("Эссе", false, "Русский язык")
                };

                _fileManager.SaveTasks(_tasks);
            }
        }

        /// <summary>
        /// Строит интерфейс главного экрана.
        /// </summary>
        private void BuildUI()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(29, 33, 56)
            };
            Controls.Add(_headerPanel);

            _titleLabel = new Label
            {
                Text = "StudyTrack",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(20, 14)
            };
            _headerPanel.Controls.Add(_titleLabel);

            _closeButton = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                ForeColor = _bgMain,
                BackColor = _accent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(48, 40),
                Location = new Point(Width - 100, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => Close();
            _headerPanel.Controls.Add(_closeButton);

            _headerPanel.MouseDown += StartDrag;
            _headerPanel.MouseMove += DragWindow;
            _headerPanel.MouseUp += EndDrag;
            _titleLabel.MouseDown += StartDrag;
            _titleLabel.MouseMove += DragWindow;
            _titleLabel.MouseUp += EndDrag;

            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 16, 8, 8)
            };
            Controls.Add(content);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 78F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
            content.Controls.Add(layout);

            Panel leftCardHost = CreateCardHost();
            Panel rightCardHost = CreateCardHost();
            Panel navCardHost = CreateCardHost();
            layout.Controls.Add(leftCardHost, 0, 0);
            layout.Controls.Add(rightCardHost, 1, 0);
            layout.Controls.Add(navCardHost, 0, 1);
            layout.SetColumnSpan(navCardHost, 2);

            BuildTimerCard(leftCardHost);
            BuildProgressCard(rightCardHost);
            BuildNavigationCard(navCardHost);
        }

        /// <summary>
        /// Создаёт контейнер с псевдо-тенью и скруглённой карточкой.
        /// </summary>
        private Panel CreateCardHost()
        {
            Panel host = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 8, 0)
            };

            RoundedPanel shadow = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 18, 32),
                Radius = 18
            };
            host.Controls.Add(shadow);

            RoundedPanel card = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _bgCard,
                Radius = 18,
                Padding = new Padding(18)
            };
            shadow.Controls.Add(card);

            host.Tag = card;
            return host;
        }

        /// <summary>
        /// Строит блок таймера обратного отсчёта.
        /// </summary>
        private void BuildTimerCard(Panel host)
        {
            RoundedPanel card = host.Tag as RoundedPanel;

            Label blockTitle = new Label
            {
                Text = "БЛОК ТАЙМЕРА",
                Font = new Font("Segoe UI", 16F),
                ForeColor = _textSecondary,
                AutoSize = true
            };
            card.Controls.Add(blockTitle);

            Label countdownLabel = new Label
            {
                Text = "До МЭСК осталось:",
                Font = new Font("Segoe UI", 14F),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(0, 55)
            };
            card.Controls.Add(countdownLabel);

            _countdownValueLabel = new Label
            {
                Font = new Font("Segoe UI", 36F, FontStyle.Bold),
                ForeColor = _accent,
                AutoSize = true,
                Location = new Point(0, 90)
            };
            card.Controls.Add(_countdownValueLabel);

            Label examDateLabel = new Label
            {
                Text = "Дата экзамена: 25 мая 2026 года",
                Font = new Font("Segoe UI", 12F),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(0, 190)
            };
            card.Controls.Add(examDateLabel);

            _timelineProgress = new CustomProgressBar
            {
                Location = new Point(0, 250),
                Size = new Size(450, 24),
                Maximum = 100,
                Value = 0,
                ProgressColor = _accent,
                TrackColor = Color.FromArgb(49, 50, 68),
                CornerRadius = 12
            };
            card.Controls.Add(_timelineProgress);

            _timePassedLabel = new Label
            {
                Text = "ПРОЙДЕНО ВРЕМЕНИ (с 1 сент 2025)",
                Font = new Font("Segoe UI", 11F),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(0, 284)
            };
            card.Controls.Add(_timePassedLabel);
        }

        /// <summary>
        /// Строит блок прогресса по темам.
        /// </summary>
        private void BuildProgressCard(Panel host)
        {
            RoundedPanel card = host.Tag as RoundedPanel;

            Label blockTitle = new Label
            {
                Text = "БЛОК \"Прогресс по темам\"",
                Font = new Font("Segoe UI", 16F),
                ForeColor = _textSecondary,
                AutoSize = true
            };
            card.Controls.Add(blockTitle);

            _progressLabel = new Label
            {
                Text = "Прогресс: 0%",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = _accent,
                AutoSize = true,
                Location = new Point(0, 56)
            };
            card.Controls.Add(_progressLabel);

            _topicProgress = new CustomProgressBar
            {
                Location = new Point(0, 96),
                Size = new Size(450, 24),
                Maximum = 100,
                Value = 0,
                ProgressColor = _accent,
                TrackColor = Color.FromArgb(49, 50, 68),
                CornerRadius = 12
            };
            card.Controls.Add(_topicProgress);

            _tasksGrid = new DataGridView
            {
                Location = new Point(0, 138),
                Size = new Size(450, 240),
                BackgroundColor = Color.FromArgb(36, 38, 58),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(68, 71, 90)
            };

            _tasksGrid.EnableHeadersVisualStyles = false;
            _tasksGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(49, 50, 68);
            _tasksGrid.ColumnHeadersDefaultCellStyle.ForeColor = _textPrimary;
            _tasksGrid.DefaultCellStyle.BackColor = Color.FromArgb(36, 38, 58);
            _tasksGrid.DefaultCellStyle.ForeColor = _textPrimary;
            _tasksGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(64, 69, 98);
            _tasksGrid.DefaultCellStyle.SelectionForeColor = _textPrimary;
            _tasksGrid.ColumnHeadersHeight = 34;
            _tasksGrid.RowTemplate.Height = 34;

            DataGridViewCheckBoxColumn checkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Выполнено",
                HeaderText = "✓",
                FillWeight = 20
            };
            _tasksGrid.Columns.Add(checkCol);

            DataGridViewTextBoxColumn subjectCol = new DataGridViewTextBoxColumn
            {
                Name = "Предмет",
                HeaderText = "Предмет",
                FillWeight = 38,
                ReadOnly = true
            };
            _tasksGrid.Columns.Add(subjectCol);

            DataGridViewTextBoxColumn topicCol = new DataGridViewTextBoxColumn
            {
                Name = "Тема",
                HeaderText = "Тема",
                FillWeight = 42,
                ReadOnly = true
            };
            _tasksGrid.Columns.Add(topicCol);

            _tasksGrid.CurrentCellDirtyStateChanged += TasksGrid_CurrentCellDirtyStateChanged;
            _tasksGrid.CellValueChanged += TasksGrid_CellValueChanged;

            card.Controls.Add(_tasksGrid);

            RenderTasksToGrid();
        }

        /// <summary>
        /// Строит нижнюю навигацию с тремя кнопками.
        /// </summary>
        private void BuildNavigationCard(Panel host)
        {
            RoundedPanel card = host.Tag as RoundedPanel;

            Label navTitle = new Label
            {
                Text = "НИЖНЯЯ НАВИГАЦИЯ",
                Font = new Font("Segoe UI", 16F),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(0, 4)
            };
            card.Controls.Add(navTitle);

            FlowLayoutPanel navButtons = new FlowLayoutPanel
            {
                Location = new Point(0, 40),
                Width = card.Width - 10,
                Height = 90,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0)
            };
            card.Controls.Add(navButtons);

            navButtons.Controls.Add(CreateNavButton("🏠 Главная", "home"));
            navButtons.Controls.Add(CreateNavButton("➕ Добавить задачу", "add"));
            navButtons.Controls.Add(CreateNavButton("👤 Профиль", "profile"));
        }

        /// <summary>
        /// Создаёт кнопку нижней навигации и вешает обработчик switch.
        /// </summary>
        private Button CreateNavButton(string text, string tag)
        {
            Button btn = new Button
            {
                Text = text,
                Tag = tag,
                Width = 220,
                Height = 64,
                Margin = new Padding(0, 0, 16, 0),
                Font = new Font("Segoe UI", 14F),
                FlatStyle = FlatStyle.Flat,
                ForeColor = _accent,
                BackColor = Color.FromArgb(36, 38, 58)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += NavigationButton_Click;
            return btn;
        }

        /// <summary>
        /// Обрабатывает навигацию через switch/case.
        /// </summary>
        private void NavigationButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string route = button != null ? button.Tag.ToString() : string.Empty;

            switch (route)
            {
                case "home":
                    MessageBox.Show("Вы уже на главном экране.", "Навигация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "add":
                    OpenChildForm(new AddTaskForm(_fileManager, ReloadTasksAndProgress));
                    break;
                case "profile":
                    OpenChildForm(new ProfileForm());
                    break;
                default:
                    MessageBox.Show("Неизвестный раздел.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }


        private void ReloadTasksAndProgress()
        {
            _tasks = _fileManager.LoadTasks();
            UpdateProgressBlock();
        }

        /// <summary>
        /// Открывает отдельную форму и скрывает текущую, пока дочерняя не закрыта.
        /// </summary>
        private void OpenChildForm(Form child)
        {
            child.FormClosed += (s, e) => Show();
            child.Show();
            Hide();
        }

        /// <summary>
        /// Простое окно добавления задачи с валидацией (&& и ||).
        /// </summary>
        private void AddTaskDialog()
        {
            string subject = Microsoft.VisualBasic.Interaction.InputBox("Введите предмет:", "Добавить задачу", "Биология");
            string title = Microsoft.VisualBasic.Interaction.InputBox("Введите тему:", "Добавить задачу", "Генетика");

            if ((string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(title)) ||
                (subject != null && title != null && (subject.Trim().Length == 0 || title.Trim().Length == 0)))
            {
                MessageBox.Show("Предмет и тема не должны быть пустыми.", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _tasks.Add(new StudyTask(title.Trim(), false, subject.Trim()));
            _fileManager.SaveTasks(_tasks);
            RenderTasksToGrid();
            UpdateProgressBlock();
        }

        /// <summary>
        /// Включает таймер, который обновляет обратный отсчёт каждую секунду.
        /// </summary>
        private void ConfigureTimer()
        {
            _timer.Interval = Convert.ToInt32(1000);
            _timer.Tick += (s, e) => UpdateTimerBlock();
            _timer.Start();
        }

        /// <summary>
        /// Обновляет таймер обратного отсчёта и прогресс времени до экзамена.
        /// </summary>
        private void UpdateTimerBlock()
        {
            TimeSpan remaining = _exam.Date - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                _countdownValueLabel.Text = "Экзамен начался";
            }
            else
            {
                _countdownValueLabel.Text = string.Format(
                    "{0} д {1} ч {2} м {3} с",
                    remaining.Days,
                    remaining.Hours,
                    remaining.Minutes,
                    remaining.Seconds);
            }

            double totalPeriodSeconds = (_exam.Date - _studyStartDate).TotalSeconds;
            double elapsedSeconds = (DateTime.Now - _studyStartDate).TotalSeconds;
            double ratio = 0;

            if (totalPeriodSeconds > 0)
            {
                ratio = elapsedSeconds / totalPeriodSeconds;
            }

            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;

            int percent = Convert.ToInt32(ratio * 100);
            _timelineProgress.Value = percent;
            _timePassedLabel.Text = "ПРОЙДЕНО ВРЕМЕНИ: " + percent.ToString() + "% (с 1 сентября 2025)";
        }

        /// <summary>
        /// Пересчитывает и отображает общий прогресс по темам.
        /// </summary>
        private void UpdateProgressBlock()
        {
            double progress = _progressCalculator.Calculate(_tasks);
            int rounded = Convert.ToInt32(progress);
            _progressLabel.Text = "Прогресс: " + progress.ToString("0.0") + "%";
            _topicProgress.Value = rounded;
        }

        /// <summary>
        /// Отрисовывает список задач в DataGridView.
        /// </summary>
        private void RenderTasksToGrid()
        {
            _tasksGrid.Rows.Clear();
            foreach (StudyTask task in _tasks)
            {
                _tasksGrid.Rows.Add(task.IsCompleted, task.Subject, task.Title);
            }
        }

        /// <summary>
        /// Фиксирует изменение значения чекбокса сразу после клика.
        /// </summary>
        private void TasksGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_tasksGrid.IsCurrentCellDirty)
            {
                _tasksGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// Обновляет модель данных после изменения чекбокса и сохраняет в файл.
        /// </summary>
        private void TasksGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0)
            {
                return;
            }

            object cellValue = _tasksGrid.Rows[e.RowIndex].Cells[0].Value;
            bool completed = cellValue != null && Convert.ToBoolean(cellValue);

            if (e.RowIndex < _tasks.Count)
            {
                _tasks[e.RowIndex].IsCompleted = completed;
                _fileManager.SaveTasks(_tasks);
                UpdateProgressBlock();
            }
        }

        /// <summary>
        /// Начинает перетаскивание формы.
        /// </summary>
        private void StartDrag(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _dragStart = new Point(e.X, e.Y);
        }

        /// <summary>
        /// Перемещает форму во время перетаскивания.
        /// </summary>
        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point screenPos = PointToScreen(e.Location);
                Location = new Point(screenPos.X - _dragStart.X, screenPos.Y - _dragStart.Y);
            }
        }

        /// <summary>
        /// Завершает перетаскивание формы.
        /// </summary>
        private void EndDrag(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }
    }

    /// <summary>
    /// Панель со скруглёнными углами (для карточек и секций).
    /// </summary>
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 16;

        /// <summary>
        /// Переопределяет отрисовку панели и рисует скруглённый контур.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreatePath(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
                Region = new Region(path);
            }
        }

        /// <summary>
        /// Создаёт GraphicsPath для прямоугольника со скруглёнными углами.
        /// </summary>
        private GraphicsPath CreatePath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Кастомный прогресс-бар, который отрисовывается через Graphics.
    /// </summary>
    public class CustomProgressBar : Control
    {
        private int _value;
        private int _maximum = 100;

        public Color TrackColor { get; set; } = Color.DarkGray;
        public Color ProgressColor { get; set; } = Color.MediumPurple;
        public int CornerRadius { get; set; } = 10;

        /// <summary>
        /// Максимальное значение шкалы.
        /// </summary>
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value <= 0 ? 100 : value;
                if (_value > _maximum)
                {
                    _value = _maximum;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Текущее значение заполнения прогресс-бара.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_value < 0) _value = 0;
                if (_value > _maximum) _value = _maximum;
                Invalidate();
            }
        }

        /// <summary>
        /// Создаёт кастомный прогресс-бар.
        /// </summary>
        public CustomProgressBar()
        {
            DoubleBuffered = true;
            Height = 22;
        }

        /// <summary>
        /// Рисует трек и заполненную часть прогресс-бара.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath trackPath = GetRoundedPath(rect, CornerRadius))
            using (SolidBrush trackBrush = new SolidBrush(TrackColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            float ratio = _maximum > 0 ? (float)_value / _maximum : 0f;
            int fillWidth = (int)((Width - 1) * ratio);
            if (fillWidth <= 0)
            {
                return;
            }

            Rectangle fillRect = new Rectangle(0, 0, fillWidth, Height - 1);
            using (GraphicsPath fillPath = GetRoundedPath(fillRect, CornerRadius))
            using (SolidBrush fillBrush = new SolidBrush(ProgressColor))
            {
                e.Graphics.FillPath(fillBrush, fillPath);
            }
        }

        /// <summary>
        /// Формирует скруглённый путь для отрисовки элемента.
        /// </summary>
        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            int r = radius;
            int d = r * 2;
            GraphicsPath path = new GraphicsPath();

            if (rect.Width < d) d = rect.Width;
            if (rect.Height < d) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
