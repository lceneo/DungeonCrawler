using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Игра
{
    public static class PathFinder
    {
         public static int ShortestPathLength(Map map) 
         {
            var queue = new Queue<Point>();
            var dictionaryPath = new Dictionary<Point, int>();
            var visitedPoints = new HashSet<Point>();
            queue.Enqueue(map.StartPoint);
            dictionaryPath[map.StartPoint] = 0;
            visitedPoints.Add(map.StartPoint);
            while (queue.Count > 0) 
            {
                var currentPoint = queue.Dequeue();
                if (currentPoint == map.EndPoint)
                    return dictionaryPath[currentPoint];
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (Math.Abs(y + x) != 1)
                            continue;
                        var newPoint = new Point(currentPoint.X + x, currentPoint.Y + y);
                        if (map.InBounds(newPoint) && map.Cells[newPoint.Y, newPoint.X] != MapCell.Wall && !visitedPoints.Contains(newPoint))
                        {
                            queue.Enqueue(newPoint);
                            dictionaryPath[newPoint] = dictionaryPath[currentPoint] + 1;
                            visitedPoints.Add(newPoint);
                        }
                    }
                }
            }
            throw new ArgumentException("It's impossible to get to the Exit!");
         }
    }
}
