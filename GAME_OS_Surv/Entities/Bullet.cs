using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    // ─── ПУЛЯ ──────────────────────────────────────────────────────
    public class Bullet
    {
        public float X, Y;
        public float DX, DY;
        public float Speed = 10f; // замедлили чтобы не перепрыгивала
        public int Size = 8;     // увеличили хитбокс
        public bool Dead = false;
        public bool Piercing = false;

        public void Update(MapGenerator map)
        {
            X += DX * Speed;
            Y += DY * Speed;

            int gx = (int)(X / 32);
            int gy = (int)(Y / 32);

            if (gx < 0 || gx >= map.Width || gy < 0 || gy >= map.Height)
            {
                Dead = true;
                return;
            }

            if (map.Grid[gx, gy] == 1)
                Dead = true;
        }

        public void Draw(Graphics g, Camera cam)
        {
            // Жёлтый кружок
            g.FillEllipse(Brushes.Yellow,
                X - cam.X - Size / 2,
                Y - cam.Y - Size / 2,
                Size, Size);

            // Красный хитбокс — временно для отладки
            //g.DrawRectangle(Pens.Red,
            //    X - cam.X, Y - cam.Y, Size, Size);
        }
    }
}