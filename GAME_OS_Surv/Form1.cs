using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public partial class Form1 : Form
    {
        private Player _player;
        private Camera _camera;
        private MapGenerator _map;
        private int _wave = 1;

        private System.Windows.Forms.Timer _gameTimer;

        private HashSet<Keys> _pressedKeys = new HashSet<Keys>();

        private List<Enemy> _enemies = new List<Enemy>();
        private List<Bullet> _bullets = new List<Bullet>();

        private int _spawnTimer = 0;
        private int _heatmapTimer = 0;
        private int _shootTimer = 0;
        private int _gameTicks = 0;
        private Random _rng = new Random();

        private bool _isPaused = false;

        private bool _devConsoleOpen = false;
        private string _consoleInput = "";
        private List<string> _consoleLog = new List<string>();
        private bool _godMode = false;
        private int _spawnInterval = 600;

        private Rectangle _btnResume;
        private Rectangle _btnMainMenu;
        private bool _hoverResume = false;
        private bool _hoverMainMenu = false;

        private int _xp = 0;
        private int _level = 1;
        private int _xpToNextLevel = 100;
        private float _xpMultiplier = 1f;
        private int _playerDamage = 10;
        private int _shootInterval = 30;
        private bool _piercing = false;
        private bool _upgradeScreenOpen = false;
        private List<Upgrade> _currentUpgradeChoices = new List<Upgrade>();
        private Rectangle[] _upgradeButtons = new Rectangle[3];
        private bool[] _hoverUpgrade = new bool[3];

        private Boss _boss = null;
        private bool _bossSpawned = false;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            InitializeComponent();

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            _camera = new Camera { ViewportWidth = screenWidth, ViewportHeight = screenHeight };
            _map = new MapGenerator(100, 100);
            _map.Generate(45);
            _map.RemoveDeadEnds();
            _map.ExpandPassages();

            bool spawned = false;
            for (int x = 0; x < _map.Width && !spawned; x++)
                for (int y = 0; y < _map.Height && !spawned; y++)
                    if (_map.Grid[x, y] == 0)
                    {
                        _player = new Player { X = x * 32, Y = y * 32 };
                        spawned = true;
                    }

            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
            this.Focus();

            /*this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Escape) TogglePause();
                _pressedKeys.Add(e.KeyCode);
            };*/
            this.KeyUp += (s, e) => _pressedKeys.Remove(e.KeyCode);
            this.Activated += (s, e) => this.Focus();

            this.MouseMove += (s, e) => {
                if (_isPaused)
                {
                    _hoverResume = _btnResume.Contains(e.Location);
                    _hoverMainMenu = _btnMainMenu.Contains(e.Location);
                    this.Invalidate();
                }
                if (_upgradeScreenOpen)
                {
                    for (int i = 0; i < 3; i++)
                        _hoverUpgrade[i] = _upgradeButtons[i].Contains(e.Location);
                    this.Invalidate();
                }
            };

            this.MouseClick += (s, e) => {
                this.Focus();
                if (_upgradeScreenOpen)
                {
                    for (int i = 0; i < _currentUpgradeChoices.Count; i++)
                    {
                        if (_upgradeButtons[i].Contains(e.Location))
                        {
                            ApplyUpgrade(_currentUpgradeChoices[i]);
                            return;
                        }
                    }

                }
                if (_isPaused)
                {
                    if (_btnResume.Contains(e.Location)) TogglePause();
                    else if (_btnMainMenu.Contains(e.Location))
                    {
                        _gameTimer.Stop();
                        this.Hide();
                        var menu = new MenuForm();
                        menu.Show();
                    }
                }
            };

            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Escape)
                {
                    if (_devConsoleOpen) _devConsoleOpen = false;
                    else TogglePause();
                }
                else if (e.KeyCode == Keys.F1) // консоль на F1 вместо тильды
                {
                    _devConsoleOpen = !_devConsoleOpen;
                    if (_devConsoleOpen && !_isPaused) _gameTimer.Stop();
                    else if (!_isPaused) _gameTimer.Start();
                    this.Invalidate();
                }
                else if (_devConsoleOpen)
                {
                    if (e.KeyCode == Keys.Back && _consoleInput.Length > 0)
                        _consoleInput = _consoleInput.Substring(0, _consoleInput.Length - 1);
                    else if (e.KeyCode == Keys.Enter)
                        ExecuteConsoleCommand(_consoleInput.Trim());
                    else if (e.KeyValue >= 32 && e.KeyValue <= 126)
                        _consoleInput += (char)e.KeyValue;
                    this.Invalidate();
                }
                else _pressedKeys.Add(e.KeyCode);
            };


            this.LostFocus += (s, e) => _pressedKeys.Clear();
            this.Cursor = Cursors.Cross;

        }

        private void ExecuteConsoleCommand(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            _consoleLog.Add("> " + input);
            string[] parts = input.ToLower().Split(' ');
            string cmd = parts[0];

            switch (cmd)
            {
                case "god":
                    _godMode = !_godMode;
                    _consoleLog.Add(_godMode ? "GOD MODE: ON" : "GOD MODE: OFF");
                    break;

                case "spawn":
                    int count = 1;
                    if (parts.Length > 1) int.TryParse(parts[1], out count);
                    for (int i = 0; i < count; i++) SpawnEnemy();
                    _consoleLog.Add($"Enemies spawned: {count}");
                    break;

                case "levelup":
                    int levels = 1;
                    if (parts.Length > 1) int.TryParse(parts[1], out levels);
                    for (int i = 0; i < levels; i++)
                    {
                        _level++;
                        _xpToNextLevel = (int)(_xpToNextLevel * 1.4f);
                        _currentUpgradeChoices = GetRandomUpgrades();
                        _upgradeScreenOpen = true;
                        _gameTimer.Stop();
                    }
                    _consoleLog.Add($"Level increased to {_level}");
                    break;

                case "spawnrate":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int rate))
                    {
                        _spawnInterval = Math.Max(10, rate);
                        _consoleLog.Add($"Spawnrate: {_spawnInterval} Tics (~{_spawnInterval / 60f:0.0}с)");
                    }
                    else _consoleLog.Add("Using: spawnrate <тики>");
                    break;

                case "kill":
                    _enemies.Clear();
                    _consoleLog.Add("All enemies cleared");
                    break;

                case "hp":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int hp))
                    {
                        _player.SetHP(Math.Clamp(hp, 1, _player.MaxHP));
                        _consoleLog.Add($"Player HP: {hp}");
                    }
                    else _consoleLog.Add("Using: hp <1-5>");
                    break;

                case "help":
                    _consoleLog.Add("--- Commands ---");
                    _consoleLog.Add("god          - God mode");
                    _consoleLog.Add("spawn [N]    - spawn N enemies");
                    _consoleLog.Add("spawnrate N  - spawnrate in tics");
                    _consoleLog.Add("kill         - clear all enemies");
                    _consoleLog.Add("hp N         - set HP (1-5)");
                    _consoleLog.Add("levelup [N]  - increase leven to");
                    _consoleLog.Add("spawnboss    - spawn boss");
                    break;

                default:
                    _consoleLog.Add($"Unkown command: {cmd}. Write 'help'");
                    break;

                case "spawnboss":
                    if (!_bossSpawned)
                    {
                        SpawnBoss();
                        _bossSpawned = true;
                        _consoleLog.Add("Boss has been spawned!");
                    }
                    else _consoleLog.Add("Boss is already active");
                    break;
            }

            _consoleInput = "";
            if (_consoleLog.Count > 12) _consoleLog.RemoveAt(0);
        }

        private void TogglePause()
        {
            if (_devConsoleOpen) return;
            _isPaused = !_isPaused;
            if (_isPaused) _gameTimer.Stop();
            else { _gameTimer.Start(); this.Focus(); }
            this.Invalidate();
        }



        private void GameLoop(object sender, EventArgs e)
        {
            
            UpdateLogic();
            this.Invalidate();
        }

        private void SpawnEnemy()
        {
            if (_enemies.Count >= 10) return;

            int attempts = 0;
            int viewTiles = 12; // минимальное расстояние от игрока в тайлах

            while (attempts < 200)
            {
                attempts++;

                // Спавн за пределами экрана
                int side = _rng.Next(4); // 0=top 1=bottom 2=left 3=right
                int gridX, gridY;

                int playerTileX = (int)(_player.X / 32);
                int playerTileY = (int)(_player.Y / 32);

                switch (side)
                {
                    case 0: // сверху
                        gridX = playerTileX + _rng.Next(-20, 21);
                        gridY = playerTileY - viewTiles - _rng.Next(0, 6);
                        break;
                    case 1: // снизу
                        gridX = playerTileX + _rng.Next(-20, 21);
                        gridY = playerTileY + viewTiles + _rng.Next(0, 6);
                        break;
                    case 2: // слева
                        gridX = playerTileX - viewTiles - _rng.Next(0, 6);
                        gridY = playerTileY + _rng.Next(-20, 21);
                        break;
                    default: // справа
                        gridX = playerTileX + viewTiles + _rng.Next(0, 6);
                        gridY = playerTileY + _rng.Next(-20, 21);
                        break;
                }

                if (gridX < 0 || gridX >= _map.Width || gridY < 0 || gridY >= _map.Height) continue;

                if (_map.Grid[gridX, gridY] == 0)
                {
                    var e = new Enemy { X = gridX * 32, Y = gridY * 32 };
                    e.Init(_wave);
                    _enemies.Add(e);
                    return;
                }
            }
        }

        private void CheckLevelUp()
        {
            if (_xp >= _xpToNextLevel)
            {
                _xp -= _xpToNextLevel;
                _level++;
                _xpToNextLevel = (int)(_xpToNextLevel * 1.4f);
                _currentUpgradeChoices = GetRandomUpgrades();
                _upgradeScreenOpen = true;
                _gameTimer.Stop();
            }
        }

        private List<Upgrade> GetRandomUpgrades()
        {
            Image iconHP = null;
            Image iconXP = null;
            Image iconSpeed = null;
            Image iconDamage = null;
            Image iconPiercing = null;
            Image iconMoveSpeed = null;

            string iconDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"Resources"); // <-- Укажи новое имя папки

            try { iconHP = new Bitmap(System.IO.Path.Combine(iconDir, "hp.png")); } catch { }
            try { iconXP = Image.FromFile(System.IO.Path.Combine(iconDir, "exp.png")); } catch { }
            try { iconSpeed = Image.FromFile(System.IO.Path.Combine(iconDir, "bull.png")); } catch { }
            try { iconDamage = Image.FromFile(System.IO.Path.Combine(iconDir, "dmg.png")); } catch { }
            try { iconPiercing = Image.FromFile(System.IO.Path.Combine(iconDir, "pier.png")); } catch { }
            try { iconMoveSpeed = Image.FromFile(System.IO.Path.Combine(iconDir, "speed.png")); } catch { }

            var all = new List<Upgrade>
            {
                new Upgrade("Max HP +25",   "Increased maximum Healthpoints", Color.Red,       UpgradeType.MaxHP)
                { Icon = iconHP },
                new Upgrade("EXP x1.2",      "Increased exp from enemies killed",       Color.Gold,      UpgradeType.XPBonus)
                { Icon = iconXP },
                new Upgrade("Attack speed", "Attack speed increased by 20%",     Color.Cyan,      UpgradeType.AttackSpeed)
                { Icon = iconSpeed },
                new Upgrade("Damage +10",       "Increased attack damage",   Color.Orange,    UpgradeType.Damage)
                { Icon = iconDamage },
                new Upgrade("Piercing",     "Attacks pierces enemies",       Color.Magenta,   UpgradeType.Piercing)
                { Icon = iconPiercing },
                new Upgrade("Speed +0.5",  "Incresed movement speed",     Color.LimeGreen, UpgradeType.Speed)
                { Icon = iconMoveSpeed },
            };

            var result = new List<Upgrade>();
            while (result.Count < 3 && all.Count > 0)
            {
                int idx = _rng.Next(all.Count);
                result.Add(all[idx]);
                all.RemoveAt(idx);
            }
            return result;
        }

        private void ApplyUpgrade(Upgrade u)
        {
            switch (u.Type)
            {
                case UpgradeType.MaxHP: _player.AddMaxHP(25); break;
                case UpgradeType.XPBonus: _xpMultiplier *= 1.2f; break;
                case UpgradeType.AttackSpeed: _shootInterval = Math.Max(20, (int)(_shootInterval * 0.8f)); break;
                case UpgradeType.Damage: _playerDamage += 10; break;
                case UpgradeType.Piercing: _piercing = true; break;
                case UpgradeType.Speed: _player.BaseSpeed += 0.5f; break;
            }
            _upgradeScreenOpen = false;
            _gameTimer.Start();
            this.Focus();
        }

        private void UpdateLogic()
        {
            if (_player.IsDead || _isPaused) return;

            _gameTicks++;
            _wave = 1 + _gameTicks / 7200;

            bool up = _pressedKeys.Contains(Keys.W);
            bool down = _pressedKeys.Contains(Keys.S);
            bool left = _pressedKeys.Contains(Keys.A);
            bool right = _pressedKeys.Contains(Keys.D);

            bool sprint = _pressedKeys.Contains(Keys.ShiftKey);
            _player.Move(up, down, left, right, sprint, _map);
            _camera.Follow(_player.X, _player.Y, _map.Width * 32, _map.Height * 32);

            // Heatmap раз в 3 тика
            _heatmapTimer++;
            if (_heatmapTimer >= 3)
            {
                _map.UpdateHeatmap(
    (int)((_player.X + _player.Size / 2f) / 32),
    (int)((_player.Y + _player.Size / 2f) / 32));
                _heatmapTimer = 0;
            }

            // Спавн раз в 5 секунд (~300 тиков)
            _spawnTimer++;
            if (_spawnTimer > _spawnInterval)
            {
                int toSpawn = 10 - _enemies.Count;
                for (int i = 0; i < toSpawn; i++) SpawnEnemy();
                _spawnTimer = 0;
            }

            // Автострельба раз в секунду (~60 тиков)
            _shootTimer++;
            if (_shootTimer >= _shootInterval)
            {
                Shoot();
                _shootTimer = 0;
            }

            // Обновляем пули
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                _bullets[i].Update(_map);
                if (_bullets[i].Dead) _bullets.RemoveAt(i);
            }

            // Обновляем врагов
            // Обновляем врагов
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Update(_map);

                // Урон от врага игроку при столкновении
                if (IsOverlapping(_player.X, _player.Y, _player.Size,
                                   _enemies[i].X, _enemies[i].Y, _enemies[i].Size))
                {
                    if (!_godMode) _player.TakeDamage(_enemies[i].Damage);
                    if (_player.IsDead)
                    {
                        _gameTimer.Stop();
                        ShowGameOver();
                        return;
                    }
                }

                // Проверка попаданий пуль во врагов
                for (int b = _bullets.Count - 1; b >= 0; b--)
                {
                    if (b >= _bullets.Count) continue;

                    if (IsOverlapping(
                        _bullets[b].X - _bullets[b].Size / 2f,
                        _bullets[b].Y - _bullets[b].Size / 2f,
                        _bullets[b].Size,
                        _enemies[i].X,
                        _enemies[i].Y,
                        _enemies[i].Size))
                    {
                        bool piercing = _bullets[b].Piercing;
                        _enemies[i].TakeDamage(_playerDamage);

                        if (!piercing)
                            _bullets.RemoveAt(b);

                        if (i < _enemies.Count && _enemies[i].IsDead)
                        {
                            _xp += (int)(_enemies[i].ExpReward * _xpMultiplier);
                            _enemies.RemoveAt(i);
                            CheckLevelUp();
                            i--;
                            break;
                        }
                    }
                }
            }
            // Спавн босса на 5 минутах (18000 тиков)
            if (!_bossSpawned && _gameTicks >= 18000)
            {
                SpawnBoss();
                _bossSpawned = true;
            }

            // Обновление босса
            if (_boss != null && !_boss.IsDead)
            {
                _boss.Update(_map, _player.X, _player.Y);

                // Урон от пуль босса
                for (int b = _boss.Bullets.Count - 1; b >= 0; b--)
                {
                    if (IsOverlapping(
                        _boss.Bullets[b].X - _boss.Bullets[b].Size / 2f,
                        _boss.Bullets[b].Y - _boss.Bullets[b].Size / 2f,
                        _boss.Bullets[b].Size,
                        _player.X, _player.Y, _player.Size))
                    {
                        if (!_godMode) _player.TakeDamage(20);
                        _boss.Bullets[b].Dead = true;
                        _boss.Bullets.RemoveAt(b);
                        if (_player.IsDead) { _gameTimer.Stop(); ShowGameOver(); return; }
                    }
                }

                // Урон от столкновения с боссом
                if (IsOverlapping(_player.X, _player.Y, _player.Size,
                                   _boss.X, _boss.Y, _boss.Size))
                {
                    if (!_godMode) _player.TakeDamage(15);
                    if (_player.IsDead) { _gameTimer.Stop(); ShowGameOver(); return; }
                }

                // Пули игрока в босса
                for (int b = _bullets.Count - 1; b >= 0; b--)
                {
                    if (IsOverlapping(
                        _bullets[b].X - _bullets[b].Size / 2f,
                        _bullets[b].Y - _bullets[b].Size / 2f,
                        _bullets[b].Size,
                        _boss.X, _boss.Y, _boss.Size))
                    {
                        _boss.TakeDamage();
                        if (!_bullets[b].Piercing) _bullets.RemoveAt(b);
                    }
                }
            }
        }

        private void SpawnBoss()
        {
            for (int x = 0; x < _map.Width; x++)
                for (int y = 0; y < _map.Height; y++)
                    if (_map.Grid[x, y] == 0)
                    {
                        _boss = new Boss { X = x * 32 + 200, Y = y * 32 + 200 };
                        return;
                    }
        }

        private void Shoot()
        {
            float dx = 0, dy = 0;
            switch (_player.FacingDirection)
            {
                case Direction.Up: dy = -1; break;
                case Direction.Down: dy = 1; break;
                case Direction.Left: dx = -1; break;
                case Direction.Right: dx = 1; break;
            }

            float spawnX = _player.X + _player.Size / 2f + dx * 30;
            float spawnY = _player.Y + _player.Size / 2f + dy * 30;

            // Проверяем что место спавна не в стене
            int gx = (int)(spawnX / 32);
            int gy = (int)(spawnY / 32);
            if (gx < 0 || gx >= _map.Width || gy < 0 || gy >= _map.Height) return;
            if (_map.Grid[gx, gy] == 1) return;

            _bullets.Add(new Bullet { X = spawnX, Y = spawnY, DX = dx, DY = dy, Piercing = _piercing });
        }

        private bool IsOverlapping(float ax, float ay, float aSize,
                                     float bx, float by, float bSize)
        {
            return ax < bx + bSize &&
                   ax + aSize > bx &&
                   ay < by + bSize &&
                   ay + aSize > by;
        }

        private void ShowGameOver()
        {
            _gameTimer.Stop();
            int seconds = _gameTicks / 60;
            var result = MessageBox.Show(
                $"You died!\nSurvived time: {seconds / 60:00}:{seconds % 60:00}\n\nPlay again?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes) RestartGame();
            else
            {
                this.Hide();
                var menu = new MenuForm();
                menu.Show();
            }
        }

        private void RestartGame()
        {
            _enemies.Clear();
            _bullets.Clear();
            _spawnTimer = 0;
            _heatmapTimer = 0;
            _shootTimer = 0;
            _gameTicks = 0;
            _isPaused = false;

            _map = new MapGenerator(100, 100);
            _map.Generate(45);
            _map.RemoveDeadEnds();
            _map.ExpandPassages();

            bool spawned = false;
            for (int x = 0; x < _map.Width && !spawned; x++)
                for (int y = 0; y < _map.Height && !spawned; y++)
                    if (_map.Grid[x, y] == 0)
                    {
                        _player = new Player { X = x * 32, Y = y * 32 };
                        spawned = true;
                    }

            _gameTimer.Start();
            _xp = 0;
            _level = 1;
            _xpToNextLevel = 100;
            _xpMultiplier = 1f;
            _playerDamage = 10;
            _shootInterval = 60;
            _piercing = false;
            _upgradeScreenOpen = false;
            _wave = 1;

            _boss = null;
            _bossSpawned = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int tileSize = 32;

            int startX = Math.Max(0, (int)(_camera.X / tileSize));
            int startY = Math.Max(0, (int)(_camera.Y / tileSize));
            int endX = Math.Min(_map.Width - 1, startX + (_camera.ViewportWidth / tileSize) + 1);
            int endY = Math.Min(_map.Height - 1, startY + (_camera.ViewportHeight / tileSize) + 1);

            for (int x = startX; x <= endX; x++)
                for (int y = startY; y <= endY; y++)
                    if (_map.Grid[x, y] == 1)
                        g.FillRectangle(Brushes.DarkGreen,
                            x * tileSize - _camera.X,
                            y * tileSize - _camera.Y,
                            tileSize - 1, tileSize - 1);

            foreach (var bullet in _bullets) bullet.Draw(g, _camera);
            foreach (var enemy in _enemies) enemy.Draw(g, _camera);
            _player.Draw(g, _camera);
            if (_boss != null && !_boss.IsDead) _boss.Draw(g, _camera);

            DrawHUD(g);
            if (_isPaused) DrawPauseScreen(g);
            if(_devConsoleOpen) DrawConsole(g); 
            if (_upgradeScreenOpen) DrawUpgradeScreen(g); 
        }
        private void DrawConsole(Graphics g)
        {
            int sw = this.ClientSize.Width;
            int sh = this.ClientSize.Height;
            int consoleH = 300;
            int y0 = sh - consoleH;

            g.FillRectangle(new SolidBrush(Color.FromArgb(210, 0, 0, 0)),
                0, y0, sw, consoleH);
            g.DrawLine(Pens.LimeGreen, 0, y0, sw, y0);

            var font = new Font("Console", 11);

            // Заголовок
            g.DrawString("[ DEV CONSOLE ]  F1 - Schliessen",
                new Font("Console", 11, FontStyle.Bold),
                Brushes.LimeGreen, 10, y0 + 6);

            // God mode индикатор
            if (_godMode)
                g.DrawString("GOD MODE ON",
                    new Font("Consolas", 11, FontStyle.Bold),
                    Brushes.Yellow, sw - 160, y0 + 6);

            // Лог
            for (int i = 0; i < _consoleLog.Count; i++)
                g.DrawString(_consoleLog[i], font, Brushes.White,
                    10, y0 + 30 + i * 18);

            // Поле ввода
            g.DrawLine(Pens.Gray, 0, sh - 30, sw, sh - 30);
            g.DrawString("> " + _consoleInput + "_", font, Brushes.LimeGreen, 10, sh - 26);
        }
        private void DrawPauseScreen(Graphics g)
        {
            int sw = this.ClientSize.Width;
            int sh = this.ClientSize.Height;

            g.FillRectangle(new SolidBrush(Color.FromArgb(140, 0, 0, 0)), 0, 0, sw, sh);

            var fontBig = new Font("Stencil", 42, FontStyle.Bold);
            var fontSmall = new Font("Stencil", 16);

            string pauseText = "PAUSE";
            SizeF ps = g.MeasureString(pauseText, fontBig);
            g.DrawString(pauseText, fontBig, Brushes.White,
                sw / 2 - ps.Width / 2, sh / 2 - 140);

            int btnW = 220, btnH = 50;
            int cx = sw / 2 - btnW / 2;
            _btnResume = new Rectangle(cx, sh / 2 - 30, btnW, btnH);
            _btnMainMenu = new Rectangle(cx, sh / 2 + 40, btnW, btnH);

            DrawPauseButton(g, _btnResume, "CONTINUE", _hoverResume);
            DrawPauseButton(g, _btnMainMenu, "HAUPTMENU", _hoverMainMenu);

            string hint = "ESC - CONTINUE";
            SizeF hs = g.MeasureString(hint, fontSmall);
            g.DrawString(hint, fontSmall, Brushes.Gray,
                sw / 2 - hs.Width / 2, sh / 2 + 110);
        }

        private void DrawPauseButton(Graphics g, Rectangle rect, string text, bool hover)
        {
            Color bg = hover ? Color.FromArgb(200, 0, 160, 0) : Color.FromArgb(140, 0, 100, 0);
            Color border = hover ? Color.LimeGreen : Color.DarkGreen;
            g.FillRectangle(new SolidBrush(bg), rect);
            g.DrawRectangle(new Pen(border, 2), rect);
            var font = new Font("Stencil", 16, FontStyle.Bold);
            SizeF ts = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.White,
                rect.X + (rect.Width - ts.Width) / 2,
                rect.Y + (rect.Height - ts.Height) / 2);
        }

        private void DrawUpgradeScreen(Graphics g)
        {
            int sw = this.ClientSize.Width;
            int sh = this.ClientSize.Height;

            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), 0, 0, sw, sh);

            var fontTitle = new Font("Arial", 32, FontStyle.Bold);
            var fontLevel = new Font("Arial", 16);
            var fontName = new Font("Arial", 14, FontStyle.Bold);
            var fontDesc = new Font("Arial", 11);

            string title = $"LEVEL {_level}!";
            SizeF ts = g.MeasureString(title, fontTitle);
            g.DrawString(title, fontTitle, Brushes.Gold, sw / 2 - ts.Width / 2, sh / 2 - 220);

            string sub = "Choose new upgrade:";
            SizeF ss = g.MeasureString(sub, fontLevel);
            g.DrawString(sub, fontLevel, Brushes.White, sw / 2 - ss.Width / 2, sh / 2 - 160);

            int cardW = 180, cardH = 220;
            int totalW = cardW * 3 + 40 * 2;
            int startX = sw / 2 - totalW / 2;
            int cardY = sh / 2 - cardH / 2 - 20;

            for (int i = 0; i < _currentUpgradeChoices.Count; i++)
            {
                var u = _currentUpgradeChoices[i];
                int cx = startX + i * (cardW + 40);
                _upgradeButtons[i] = new Rectangle(cx, cardY, cardW, cardH);

                bool hover = _hoverUpgrade[i];
                Color bg = hover ? Color.FromArgb(220, 30, 30, 30) : Color.FromArgb(160, 15, 15, 15);

                g.FillRectangle(new SolidBrush(bg), _upgradeButtons[i]);
                g.DrawRectangle(new Pen(hover ? u.Color : Color.Gray, hover ? 3 : 1), _upgradeButtons[i]);

                // Иконка — замени FillRectangle на DrawImage когда будут свои иконки
                int iconSize = 64;
                int iconX = cx + cardW / 2 - iconSize / 2;
                int iconY = cardY + 20;
                if (u.Icon != null)
                    g.DrawImage(u.Icon, iconX, iconY, iconSize, iconSize);
                else
                {
                    g.FillRectangle(new SolidBrush(u.Color), iconX, iconY, iconSize, iconSize);
                    g.DrawRectangle(Pens.White, iconX, iconY, iconSize, iconSize);
                }

                SizeF ns = g.MeasureString(u.Name, fontName);
                g.DrawString(u.Name, fontName, Brushes.White,
                    cx + cardW / 2 - ns.Width / 2, iconY + iconSize + 12);

                g.DrawString(u.Description, fontDesc, Brushes.LightGray,
                    new RectangleF(cx + 5, iconY + iconSize + 38, cardW - 10, 80));
            }
        }

        private void DrawHUD(Graphics g)
        {
            // Фон HUD
            // Фон HUD — увеличь высоту
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), 10, 10, 260, 160);

            // HP текст и полоска
            g.DrawString($"HP: {_player.HP} / {_player.MaxHP}", new Font("Arial", 11, FontStyle.Bold), Brushes.White, 18, 18);
            g.FillRectangle(Brushes.DarkGray, 18, 36, 220, 12);
            g.FillRectangle(Brushes.Red, 18, 36, (int)(220 * ((float)_player.HP / _player.MaxHP)), 12);
            g.DrawRectangle(Pens.Gray, 18, 36, 220, 12);

            // Таймер стрельбы
            g.DrawString("SHOT:", new Font("Arial", 10), Brushes.White, 18, 54);
            g.FillRectangle(Brushes.DarkGray, 100, 56, 150, 10);
            g.FillRectangle(Brushes.Yellow, 100, 56, (int)(150 * (1f - (_shootTimer / (float)_shootInterval))), 10);
            g.DrawRectangle(Pens.Gray, 100, 56, 150, 10);

            // Время
            int totalSeconds = _gameTicks / 60;
            g.DrawString($"TIME: {totalSeconds / 60:00}:{totalSeconds % 60:00}", new Font("Arial", 11, FontStyle.Bold), Brushes.Cyan, 18, 72);

            // Спринт
            string sprintStr = _player.IsSprinting ? "SPRINT [ON]" : "SPRINT [SHIFT]";
            Brush sprintColor = _player.IsSprinting ? Brushes.Yellow : Brushes.Gray;
            g.DrawString(sprintStr, new Font("Arial", 10), sprintColor, 18, 92);

            // XP
            g.DrawString($"LVL. {_level}  XP: {_xp} / {_xpToNextLevel}", new Font("Arial", 10), Brushes.Gold, 18, 110);
            g.FillRectangle(Brushes.DarkGoldenrod, 18, 126, 220, 8);
            g.FillRectangle(Brushes.Gold, 18, 126, (int)(220 * ((float)_xp / _xpToNextLevel)), 8);
            g.DrawRectangle(Pens.Gray, 18, 126, 220, 8);

            // Босс таймер
            if (!_bossSpawned)
            {
                int ticksLeft = 18000 - _gameTicks;
                int secLeft = Math.Max(0, ticksLeft / 60);
                g.DrawString($"BOSS SPAWNS: {secLeft / 60:00}:{secLeft % 60:00}", new Font("Arial", 10), Brushes.Magenta, 18, 140);
            }
            else if (_boss != null && !_boss.IsDead)
                g.DrawString("!! BOSS IS ACTIVE !!", new Font("Arial", 10, FontStyle.Bold), Brushes.Magenta, 18, 140);
            else
                g.DrawString("BOSS WAS DEFEATED!", new Font("Arial", 10), Brushes.Cyan, 18, 140);


        }
    }


}