using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public class MenuForm : Form
    {
        private bool[,] _cells;
        private bool[,] _next;
        private int _cols, _rows;
        private const int CellSize = 16;
        private System.Windows.Forms.Timer _timer;
        private Random _rng = new Random();

        // Логотип — положи файл logo.png рядом с .exe
        private Image _logo;

        private Rectangle _btnPlay;
        private Rectangle _btnExit;
        private bool _hoverPlay = false;
        private bool _hoverExit = false;

        public MenuForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            int sw = Screen.PrimaryScreen.Bounds.Width;
            int sh = Screen.PrimaryScreen.Bounds.Height;

            _cols = sw / CellSize + 1;
            _rows = sh / CellSize + 1;
            _cells = new bool[_cols, _rows];
            _next = new bool[_cols, _rows];

            // Случайный старт для игры жизни
            for (int x = 0; x < _cols; x++)
                for (int y = 0; y < _rows; y++)
                    _cells[x, y] = _rng.Next(0, 4) == 0;

            // Кнопки по центру экрана
            int btnW = 220, btnH = 55;
            int cx = sw / 2 - btnW / 2;
            _btnPlay = new Rectangle(cx, sh / 2 - 10, btnW, btnH);
            _btnExit = new Rectangle(cx, sh / 2 - 10 + 75, btnW, btnH);

            // Загрузка логотипа
            try { _logo = Image.FromFile("logo.png"); }
            catch { _logo = null; }

            _timer = new System.Windows.Forms.Timer { Interval = 100 };
            _timer.Tick += (s, e) => { StepLife(); this.Invalidate(); };
            _timer.Start();

            this.MouseMove += OnMouseMove;
            this.MouseClick += OnMouseClick;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Application.Exit(); };
        }
        private void StepLife()
        {
            for (int x = 0; x < _cols; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    int n = CountNeighbors(x, y);
                    if (_cells[x, y])
                        _next[x, y] = n == 2 || n == 3;
                    else
                        _next[x, y] = n == 3;
                }
            }
            var tmp = _cells; _cells = _next; _next = tmp;
        }

        private int CountNeighbors(int cx, int cy)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = (cx + dx + _cols) % _cols;
                    int ny = (cy + dy + _rows) % _rows;
                    if (_cells[nx, ny]) count++;
                }
            return count;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool hp = _btnPlay.Contains(e.Location);
            bool he = _btnExit.Contains(e.Location);
            if (hp != _hoverPlay || he != _hoverExit)
            {
                _hoverPlay = hp; _hoverExit = he;
                this.Invalidate();
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (_btnPlay.Contains(e.Location))
            {
                _timer.Stop();
                var game = new Form1();
                game.FormClosed += (s, ev) => Application.Exit();
                this.Hide();
                game.Show();
            }
            else if (_btnExit.Contains(e.Location))
                Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int sw = this.ClientSize.Width;
            int sh = this.ClientSize.Height;

            // Фон
            g.FillRectangle(Brushes.Black, 0, 0, sw, sh);

            // Клетки игры жизни
            var cellBrush = new SolidBrush(Color.FromArgb(40, 0, 180, 0));
            for (int x = 0; x < _cols; x++)
                for (int y = 0; y < _rows; y++)
                    if (_cells[x, y])
                        g.FillRectangle(cellBrush,
                            x * CellSize, y * CellSize, CellSize - 1, CellSize - 1);

            // Логотип над кнопками
            if (_logo != null)
            {
                int logoW = 400, logoH = 150;
                int logoX = sw / 2 - logoW / 2;
                int logoY = _btnPlay.Y - logoH - 40;
                g.DrawImage(_logo, logoX, logoY, logoW, logoH);
            }
            else
            {
                // Заглушка если файла нет
                g.DrawString("GAME OS SURV",
                    new Font("Stencil", 36, FontStyle.Bold),
                    Brushes.LimeGreen,
                    sw / 2 - 180, _btnPlay.Y - 100);
            }

            // Кнопка Играть
            DrawButton(g, _btnPlay, "SPIEL", _hoverPlay);

            // Кнопка Выйти
            DrawButton(g, _btnExit, "BEENDEN", _hoverExit);
        }

        private void DrawButton(Graphics g, Rectangle rect, string text, bool hover)
        {
            Color bg = hover ? Color.FromArgb(200, 0, 160, 0) : Color.FromArgb(140, 0, 100, 0);
            Color border = hover ? Color.LimeGreen : Color.DarkGreen;

            g.FillRectangle(new SolidBrush(bg), rect);
            g.DrawRectangle(new Pen(border, 2), rect);

            var font = new Font("Stencil", 18, FontStyle.Bold);
            SizeF ts = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.LimeGreen,
                rect.X + (rect.Width - ts.Width) / 2,
                rect.Y + (rect.Height - ts.Height) / 2);
        }
    }
}