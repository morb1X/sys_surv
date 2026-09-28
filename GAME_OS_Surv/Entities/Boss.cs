using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public class BossBullet
    {
        public float X, Y;
        public float DX, DY;
        public float Speed = 6f;
        public int Size = 10;
        public bool Dead = false;

        public void Update(MapGenerator map)
        {
            X += DX * Speed;
            Y += DY * Speed;
            int gx = (int)(X / 32);
            int gy = (int)(Y / 32);
            if (gx < 0 || gx >= map.Width || gy < 0 || gy >= map.Height || map.Grid[gx, gy] == 1)
                Dead = true;
        }

        public void Draw(Graphics g, Camera cam)
        {
            g.FillEllipse(Brushes.Magenta,
                X - cam.X - Size / 2, Y - cam.Y - Size / 2, Size, Size);
        }
    }

    public class Boss
    {
        public float X, Y;
        public float Speed = 1.8f;
        public int Size = 40;
        public int HP = 20;
        public int MaxHP = 20;
        public bool IsDead => HP <= 0;
        private int _shootTimer = 0;
        private const int ShootInterval = 90;
        public List<BossBullet> Bullets = new List<BossBullet>();

        public void TakeDamage() { HP--; }

        public void Update(MapGenerator map, float playerX, float playerY)
        {
            if (map.Heatmap == null) return;

            int gx = (int)((X + Size / 2f) / 32);
            int gy = (int)((Y + Size / 2f) / 32);
            float moveX = 0, moveY = 0;

            if (gx >= 0 && gx < map.Width && gy >= 0 && gy < map.Height)
            {
                int bestDist = map.Heatmap[gx, gy];
                Point bestStep = new Point(gx, gy);
                int[] dx = { 0, 0, 1, -1 };
                int[] dy = { 1, -1, 0, 0 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = gx + dx[i], ny = gy + dy[i];
                    if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height)
                        if (map.Heatmap[nx, ny] < bestDist)
                        { bestDist = map.Heatmap[nx, ny]; bestStep = new Point(nx, ny); }
                }

                float tx = bestStep.X * 32 + 6f;
                float ty = bestStep.Y * 32 + 6f;
                if (X < tx) moveX = Speed;
                else if (X > tx) moveX = -Speed;
                if (Y < ty) moveY = Speed;
                else if (Y > ty) moveY = -Speed;
            }

            if (!IsColliding(X + moveX, Y, map)) X += moveX;
            if (!IsColliding(X, Y + moveY, map)) Y += moveY;

            _shootTimer++;
            if (_shootTimer >= ShootInterval)
            {
                ShootAtPlayer(playerX, playerY);
                _shootTimer = 0;
            }

            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(map);
                if (Bullets[i].Dead) Bullets.RemoveAt(i);
            }
        }

        private void ShootAtPlayer(float playerX, float playerY)
        {
            float cx = X + Size / 2f;
            float cy = Y + Size / 2f;
            float pcx = playerX + 12;
            float pcy = playerY + 12;

            float mainDX = pcx - cx;
            float mainDY = pcy - cy;
            float len = (float)Math.Sqrt(mainDX * mainDX + mainDY * mainDY);
            if (len == 0) return;
            mainDX /= len; mainDY /= len;

            float[] angles = { -0.35f, 0f, 0.35f };
            foreach (float angle in angles)
            {
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);
                float bdx = mainDX * cos - mainDY * sin;
                float bdy = mainDX * sin + mainDY * cos;
                Bullets.Add(new BossBullet { X = cx, Y = cy, DX = bdx, DY = bdy });
            }
        }

        private bool IsColliding(float newX, float newY, MapGenerator map)
        {
            int tileSize = 32;
            float p = 2f;
            int[] cx = { (int)((newX + p) / tileSize), (int)((newX + Size - p) / tileSize) };
            int[] cy = { (int)((newY + p) / tileSize), (int)((newY + Size - p) / tileSize) };
            foreach (int x in cx)
                foreach (int y in cy)
                {
                    if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    { if (map.Grid[x, y] == 1) return true; }
                    else return true;
                }
            return false;
        }

        public void Draw(Graphics g, Camera cam)
        {
            float cx = X - cam.X + Size / 2f;
            float cy = Y - cam.Y + Size / 2f;
            float r = Size / 2f;
            Point[] diamond = {
            new Point((int)cx,       (int)(cy - r)),
            new Point((int)(cx + r), (int)cy),
            new Point((int)cx,       (int)(cy + r)),
            new Point((int)(cx - r), (int)cy)
        };
            g.FillPolygon(Brushes.Purple, diamond);
            g.DrawPolygon(new Pen(Color.Magenta, 2), diamond);

            int barW = 60;
            int filledW = (int)(barW * ((float)HP / MaxHP));
            g.FillRectangle(Brushes.DarkGray, X - cam.X - 10, Y - cam.Y - 10, barW, 6);
            g.FillRectangle(Brushes.Magenta, X - cam.X - 10, Y - cam.Y - 10, filledW, 6);

            foreach (var b in Bullets) b.Draw(g, cam);
        }
    }
}
