using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float BaseSpeed { get; set; } = 5f;
        public float SprintSpeed { get; set; } = 9f;
        public bool IsSprinting { get; set; } = false;
        public int Size { get; set; } = 24;
        public int HP { get; private set; } = 100;
        public int MaxHP { get; private set; } = 100;
        public bool IsDead => HP <= 0;

        public Direction FacingDirection { get; private set; } = Direction.Right;


        private int _damageCooldown = 0;
        private const int DamageCooldownMax = 60; // 1 секунда иммунитета

        public void SetHP(int value) { HP = Math.Clamp(value, 0, MaxHP); }
        public void AddMaxHP(int amount) { MaxHP += amount; HP += amount; }
        public void TakeDamage(int amount = 10)
        {
            if (_damageCooldown > 0) return;
            HP -= amount;
            if (HP < 0) HP = 0;
            _damageCooldown = DamageCooldownMax;
        }

        public void Move(bool up, bool down, bool left, bool right, bool sprint, MapGenerator map)
        {
            if (_damageCooldown > 0) _damageCooldown--;

            float currentSpeed = sprint ? SprintSpeed : BaseSpeed;
            IsSprinting = sprint;

            float dx = 0, dy = 0;
            if (up) { dy -= 1; FacingDirection = Direction.Up; }
            if (down) { dy += 1; FacingDirection = Direction.Down; }
            if (left) { dx -= 1; FacingDirection = Direction.Left; }
            if (right) { dx += 1; FacingDirection = Direction.Right; }

            if (dx != 0 && dy != 0) { dx *= 0.7071f; dy *= 0.7071f; }

            float nextX = X + dx * currentSpeed;
            float nextY = Y + dy * currentSpeed;

            if (!IsColliding(nextX, Y, map)) X = nextX;
            if (!IsColliding(X, nextY, map)) Y = nextY;
        }

        private bool IsColliding(float newX, float newY, MapGenerator map)
        {
            int tileSize = 32;
            float padding = 2f;
            float l = newX + padding, r = newX + Size - padding;
            float t = newY + padding, b = newY + Size - padding;

            int[] cx = { (int)(l / tileSize), (int)(r / tileSize) };
            int[] cy = { (int)(t / tileSize), (int)(b / tileSize) };

            foreach (int x in cx)
                foreach (int y in cy)
                {
                    if (x < 0 || x >= map.Width || y < 0 || y >= map.Height) return true;
                    if (map.Grid[x, y] == 1) return true;
                }
            return false;
        }

        public void Draw(Graphics g, Camera cam)
        {
            // Мигание при уроне
            if (_damageCooldown > 0 && (_damageCooldown / 6) % 2 == 0)
                g.FillRectangle(Brushes.White, X - cam.X, Y - cam.Y, Size, Size);
            else
                g.FillRectangle(Brushes.Lime, X - cam.X, Y - cam.Y, Size, Size);

            // Индикатор направления
            float cx = X - cam.X + Size / 2f;
            float cy = Y - cam.Y + Size / 2f;
            float ex = cx, ey = cy;
            switch (FacingDirection)
            {
                case Direction.Up: ey -= 14; break;
                case Direction.Down: ey += 14; break;
                case Direction.Left: ex -= 14; break;
                case Direction.Right: ex += 14; break;
            }
            g.DrawLine(new Pen(Color.White, 2), cx, cy, ex, ey);
        }
    }
    public enum Direction { Up, Down, Left, Right }// сюда вставляешь класс
}
