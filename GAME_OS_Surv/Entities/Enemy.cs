using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public class Enemy
    {
        public float X, Y;
        public float Speed = 2.5f;
        public int Size = 20;
        public int HP;
        public int MaxHP;
        public int Damage;
        public int ExpReward;
        public bool IsDead => HP <= 0;
        public void Init(int wave)
        {
            MaxHP = 20 + wave * 10;
            HP = MaxHP;
            Speed = 2.5f + wave * 0.1f;
            Damage = 8 + wave * 2;
            ExpReward = 10 + wave * 2;
        }

        public void TakeDamage(int amount) { HP -= amount; }

        public void Update(MapGenerator map)
        {
            if (map.Heatmap == null) return;

            int gx = (int)((X + Size / 2f) / 32);
            int gy = (int)((Y + Size / 2f) / 32);

            float moveX = 0, moveY = 0;

            if (gx >= 0 && gx < map.Width && gy >= 0 && gy < map.Height)
            {
                int bestDist = map.Heatmap[gx, gy];
                Point bestStep = new Point(gx, gy);
                int[] dx = { 0, 0, 1, -1 }, dy = { 1, -1, 0, 0 };

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
            g.FillRectangle(Brushes.Red, X - cam.X, Y - cam.Y, Size, Size);

            int barW = Size;
            int filledW = (int)(barW * ((float)HP / MaxHP));
            g.FillRectangle(Brushes.DarkGray, X - cam.X, Y - cam.Y - 6, barW, 4);
            g.FillRectangle(Brushes.Lime, X - cam.X, Y - cam.Y - 6, filledW, 4);
        }
    }
}