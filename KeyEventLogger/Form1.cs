using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KeyEventLogger
{
    public partial class Form1 : Form
    {
        private bool isLogging = false;

        // --- Controls ---
        private RichTextBox rtbLog = null!;
        private Button btnStart = null!;
        private Button btnStop = null!;
        private Button btnClear = null!;
        private Button btnSave = null!;
        private Panel pnlStatusIndicator = null!;
        private Label lblStatusText = null!;
        private GroupBox grpLog = null!;
        private GroupBox grpControls = null!;
        private Label lblInfo = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            WireEvents();
        }

        // ==================== UI BUILDING ====================
        private void BuildUI()
        {
            // --- Form settings ---
            this.Text = "Key Event Logger — Eğitsel Keylogger";
            this.Size = new Size(780, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(680, 480);
            this.KeyPreview = true; // Capture key events at form level
            this.Font = new Font("Segoe UI", 10);

            // --- Status indicator panel (top bar) ---
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(12, 8, 12, 8)
            };

            pnlStatusIndicator = new Panel
            {
                Size = new Size(18, 18),
                Location = new Point(14, 16),
                BackColor = Color.Gray
            };
            pnlStatusIndicator.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(pnlStatusIndicator.BackColor);
                e.Graphics.FillEllipse(brush, 0, 0, 16, 16);
            };

            lblStatusText = new Label
            {
                Text = "Durum: Durduruldu",
                Location = new Point(38, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            pnlTop.Controls.Add(pnlStatusIndicator);
            pnlTop.Controls.Add(lblStatusText);
            this.Controls.Add(pnlTop);

            // --- Controls group ---
            grpControls = new GroupBox
            {
                Text = "Kontroller",
                Location = new Point(12, 58),
                Size = new Size(740, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9)
            };

            btnStart = new Button
            {
                Text = "▶ Başlat",
                Location = new Point(15, 25),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;

            btnStop = new Button
            {
                Text = "■ Durdur",
                Location = new Point(145, 25),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnStop.FlatAppearance.BorderSize = 0;

            btnClear = new Button
            {
                Text = "🗑 Temizle",
                Location = new Point(290, 25),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;

            btnSave = new Button
            {
                Text = "💾 Kaydet (.txt)",
                Location = new Point(435, 25),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            grpControls.Controls.AddRange(new Control[] { btnStart, btnStop, btnClear, btnSave });
            this.Controls.Add(grpControls);

            // --- Log group ---
            grpLog = new GroupBox
            {
                Text = "Tuş Kayıtları",
                Location = new Point(12, 135),
                Size = new Size(740, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9)
            };

            rtbLog = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(720, 305),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };

            grpLog.Controls.Add(rtbLog);
            this.Controls.Add(grpLog);

            // --- Info label at bottom ---
            lblInfo = new Label
            {
                Text = "ℹ Bu uygulama yalnızca kendi penceresi aktifken tuş girişlerini yakalar.",
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            this.Controls.Add(lblInfo);
        }

        // ==================== EVENT WIRING ====================
        private void WireEvents()
        {
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnClear.Click += BtnClear_Click;
            btnSave.Click += BtnSave_Click;

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPress += Form1_KeyPress;
        }

        // ==================== BUTTON HANDLERS ====================
        private void BtnStart_Click(object? sender, EventArgs e)
        {
            isLogging = true;
            UpdateStatusIndicator();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            AppendLog("SYSTEM", "Kayıt başlatıldı.", Color.FromArgb(129, 199, 132));
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            isLogging = false;
            UpdateStatusIndicator();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            AppendLog("SYSTEM", "Kayıt durduruldu.", Color.FromArgb(239, 154, 154));
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rtbLog.Text))
            {
                MessageBox.Show("Kaydedilecek veri yok.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Metin Dosyası (*.txt)|*.txt",
                Title = "Kayıtları Kaydet",
                FileName = $"KeyLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8);
                    sw.Write(rtbLog.Text);
                    MessageBox.Show($"Kayıtlar başarıyla kaydedildi:\n{sfd.FileName}",
                        "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya kaydedilirken hata oluştu:\n{ex.Message}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ==================== KEY EVENT HANDLERS ====================
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!isLogging) return;

            string modifiers = GetModifierString(e);
            string info = $"Tuş: {e.KeyCode,-20} | KeyValue: {e.KeyValue,-5} | Modifiers: {modifiers}";
            AppendLog("KeyDown  ⬇", info, Color.FromArgb(100, 181, 246)); // light blue
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (!isLogging) return;

            string modifiers = GetModifierString(e);
            string info = $"Tuş: {e.KeyCode,-20} | KeyValue: {e.KeyValue,-5} | Modifiers: {modifiers}";
            AppendLog("KeyUp    ⬆", info, Color.FromArgb(255, 183, 77)); // orange
        }

        private void Form1_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!isLogging) return;

            string charDisplay = char.IsControl(e.KeyChar)
                ? $"[Control: 0x{(int)e.KeyChar:X2}]"
                : e.KeyChar.ToString();

            string info = $"Karakter: {charDisplay,-18} | ASCII: {(int)e.KeyChar,-5}";
            AppendLog("KeyPress ✎", info, Color.FromArgb(206, 147, 216)); // purple
        }

        // ==================== HELPERS ====================
        private string GetModifierString(KeyEventArgs e)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (e.Control) parts.Add("Ctrl");
            if (e.Shift) parts.Add("Shift");
            if (e.Alt) parts.Add("Alt");
            return parts.Count > 0 ? string.Join(" + ", parts) : "Yok";
        }

        private void AppendLog(string eventType, string message, Color color)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string line = $"[{timestamp}]  {eventType}  |  {message}";

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(line + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        private void UpdateStatusIndicator()
        {
            if (isLogging)
            {
                pnlStatusIndicator.BackColor = Color.FromArgb(76, 175, 80); // green
                lblStatusText.Text = "Durum: Kayıt Yapılıyor...";
                lblStatusText.ForeColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                pnlStatusIndicator.BackColor = Color.Gray;
                lblStatusText.Text = "Durum: Durduruldu";
                lblStatusText.ForeColor = Color.Gray;
            }
            pnlStatusIndicator.Invalidate();
        }
    }
}
