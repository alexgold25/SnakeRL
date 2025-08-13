using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeRL.GameLogic
{
    public enum Dir { Up = 0, Right = 1, Down = 2, Left = 3 }

    public class Game
    {
        readonly int W, H;
        readonly Random rnd = new();
        public LinkedList<(int x, int y)> Snake = new();
        public int FoodX, FoodY;
        public Dir LastAction = Dir.Right;

        public Game(int w, int h) { W = w; H = h; Reset(); }

        public int[] Reset()
        {
            Snake.Clear();
            int sx = W / 2, sy = H / 2;
            Snake.AddFirst((sx, sy));
            Snake.AddLast((sx - 1, sy));
            PlaceFood();
            LastAction = Dir.Right;
            return GetObsFlat();
        }

        void PlaceFood()
        {
            do { FoodX = rnd.Next(W); FoodY = rnd.Next(H); }
            while (Snake.Any(s => s.x == FoodX && s.y == FoodY));
        }

        public (double reward, bool done) Step(Dir a)
        {
            // запрет разворота на 180°
            if ((LastAction == Dir.Up && a == Dir.Down) ||
                (LastAction == Dir.Down && a == Dir.Up) ||
                (LastAction == Dir.Left && a == Dir.Right) ||
                (LastAction == Dir.Right && a == Dir.Left))
                a = LastAction;
            LastAction = a;

            var head = Snake.First!.Value;
            var nx = head.x + (a == Dir.Left ? -1 : a == Dir.Right ? 1 : 0);
            var ny = head.y + (a == Dir.Up ? -1 : a == Dir.Down ? 1 : 0);

            // выход за границы или укус себя
            if (nx < 0 || ny < 0 || nx >= W || ny >= H || Snake.Any(s => s.x == nx && s.y == ny))
                return (-1.0, true);

            Snake.AddFirst((nx, ny));
            double reward = -0.01;
            if (nx == FoodX && ny == FoodY)
            {
                reward = 1.0;
                PlaceFood();
            }
            else
            {
                Snake.RemoveLast();
            }
            return (reward, false);
        }

        public int[] GetObsFlat()
        {
            var obs = new int[W * H];
            foreach (var (x, y) in Snake) obs[y * W + x] = 1;
            obs[FoodY * W + FoodX] = 2;
            return obs;
        }
    }
}
