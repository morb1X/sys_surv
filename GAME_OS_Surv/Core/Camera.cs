using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    // ─── КАМЕРА ────────────────────────────────────────────────────
    public class Camera
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }

        public void Follow(float targetX, float targetY, int mapWidthPx, int mapHeightPx)
        {
            X = targetX - ViewportWidth / 2f;
            Y = targetY - ViewportHeight / 2f;
            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            if (X > mapWidthPx - ViewportWidth) X = mapWidthPx - ViewportWidth;
            if (Y > mapHeightPx - ViewportHeight) Y = mapHeightPx - ViewportHeight;
        }
    }
}