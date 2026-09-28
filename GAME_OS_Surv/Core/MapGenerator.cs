using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    // ─── ГЕНЕРАТОР КАРТЫ ───────────────────────────────────────────
    public class MapGenerator
    {
        public int Width { get; }
        public int Height { get; }
        public int[,] Grid { get; private set; }
        public int[,] Heatmap { get; private set; }

        public MapGenerator(int width, int height)
        {
            Width = width; Height = height;
            Grid = new int[width, height];
        }

        public void Generate(int fillPercent)
        {
            Random rng = new Random();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Grid[x, y] = rng.Next(0, 100) < fillPercent ? 1 : 0;
            for (int i = 0; i < 3; i++) Smooth();
        }

        private void Smooth()
        {
            int[,] tmp = new int[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    int n = GetNeighbors(x, y);
                    if (n > 4) tmp[x, y] = 1;
                    else if (n < 4) tmp[x, y] = 0;
                    else tmp[x, y] = Grid[x, y];
                }
            Grid = tmp;
        }

        private int GetNeighbors(int gx, int gy)
        {
            int count = 0;
            for (int x = gx - 1; x <= gx + 1; x++)
                for (int y = gy - 1; y <= gy + 1; y++)
                {
                    if (x >= 0 && x < Width && y >= 0 && y < Height)
                    { if (x != gx || y != gy) count += Grid[x, y]; }
                    else count++;
                }
            return count;
        }

        public void RemoveDeadEnds()
        {
            bool[,] visited = new bool[Width, Height];
            var caves = new List<List<Point>>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (!visited[x, y] && Grid[x, y] == 0)
                        caves.Add(GetRegionPoints(x, y, visited));

            if (caves.Count > 1)
            {
                caves.Sort((a, b) => b.Count.CompareTo(a.Count));
                for (int i = 1; i < caves.Count; i++)
                    foreach (Point p in caves[i]) Grid[p.X, p.Y] = 1;
            }
        }

        private List<Point> GetRegionPoints(int sx, int sy, bool[,] visited)
        {
            var points = new List<Point>();
            var queue = new Queue<Point>();
            queue.Enqueue(new Point(sx, sy));
            visited[sx, sy] = true;
            int[] dx = { 0, 0, 1, -1 }, dy = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                points.Add(p);
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i], ny = p.Y + dy[i];
                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height
                        && !visited[nx, ny] && Grid[nx, ny] == 0)
                    { visited[nx, ny] = true; queue.Enqueue(new Point(nx, ny)); }
                }
            }
            return points;
        }

        public void ExpandPassages()
        {
            int[,] newGrid = (int[,])Grid.Clone();
            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                    if (Grid[x, y] == 0)
                        for (int ix = -1; ix <= 1; ix++)
                            for (int iy = -1; iy <= 1; iy++)
                                newGrid[x + ix, y + iy] = 0;
            Grid = newGrid;
        }

        public void UpdateHeatmap(int tx, int ty)
        {
            Heatmap = new int[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Heatmap[x, y] = 9999;

            if (tx < 0 || tx >= Width || ty < 0 || ty >= Height) return;

            var queue = new Queue<Point>();
            Heatmap[tx, ty] = 0;
            queue.Enqueue(new Point(tx, ty));
            int[] dx = { 0, 0, 1, -1 }, dy = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int d = Heatmap[p.X, p.Y];
                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i], ny = p.Y + dy[i];
                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && Grid[nx, ny] == 0)
                        if (Heatmap[nx, ny] > d + 1)
                        { Heatmap[nx, ny] = d + 1; queue.Enqueue(new Point(nx, ny)); }
                }
            }
        }
    }
}