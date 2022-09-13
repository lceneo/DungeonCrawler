using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Игра.Entities
{
    public class Entity
    {
        public Entity(string name, Point position, int energy)
        {
            Name = name;
            Position = position;
            InitialPosition = position;
            Damage = 1;
            Health = 3;
            CurrentEnergy = energy;
            Model = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent + @$"\Sprites\{Name}.png");
            Frame = (0, 0);
        }
        public readonly string Name;
        public Point Position { get; set; }
        public readonly Point InitialPosition;
        public double Damage { get; set; }
        public int Health { get; set; }
        public int CurrentEnergy { get; set; }
        public bool isHitted = false;
        public bool isDead = false;

        public readonly Bitmap Model;
        public (int, Animations) Frame { get; set; }
        public async Task<(int, Point)> GetBestPoint(Map maze, int index)
        {
            var result = await Task.Run(() =>
            {
                var point = maze.Player.Position;
                var bestPoint = Position;
                var smallestDistance = Math.Sqrt((Position.X - point.X) * (Position.X - point.X) +
                                (Position.Y - point.Y) * (Position.Y - point.Y));
                if (smallestDistance == 1)
                    return (index, bestPoint);
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (Math.Abs(x + y) == 1)
                        {
                            var newPoint = new Point(Position.X + x, Position.Y + y);
                            if (!maze.InBounds(newPoint) || maze.Cells[newPoint.Y, newPoint.X] == MapCell.Wall)
                                continue;
                            var newDistance = Math.Sqrt((newPoint.X - point.X) * (newPoint.X - point.X) +
                                (newPoint.Y - point.Y) * (newPoint.Y - point.Y));
                            if (newDistance < smallestDistance)
                            {
                                smallestDistance = newDistance;
                                bestPoint = newPoint;
                            }
                        }
                    }
                }
                return (index, bestPoint);
            });
            return result;
        }

    }
}
