using System;
using Игра.Entities;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Игра
{
    public class Map
    {
        public Map(string[] textCells, string playerName, int enemiesCount = 0)
        {
            Cells = ParseMaze(textCells);
            Rows = Cells.GetLength(0);
            Columns = Cells.GetLength(1);
            Energy = PathFinder.ShortestPathLength(this) + 2;
            Player = new Entity(playerName, StartPoint, Energy);
            Enemies = new Entity[enemiesCount];
            InitialiseEnemies();
        }   
        public readonly MapCell[,] Cells;
        public Point StartPoint { get; private set; }
        public  Point EndPoint { get; private set; }
        public Dictionary<Point,(Artefacts artefactType, Point spriteLocation, bool isTaken, Action effect)> artefacts = 
            new Dictionary<Point, (Artefacts, Point, bool, Action)>();
        public LinkedList<Point> peaks = new LinkedList<Point>();
        public readonly Entity Player;
        public readonly Entity[] Enemies;
        public readonly int Energy;
        public readonly int Rows;
        public readonly int Columns;

        private void InitialiseEnemies()
        {
            for (int i = 0; i < Enemies.Length; i++)
                Enemies[i] = new Entity("skeleton", Player.Position, 0);
            for (int i = 0; i < Enemies.Length; i++)
               Enemies[i] = new Entity("skeleton", GenerateFreePoint(), 0);
        }
        private Point GenerateFreePoint()
        {
            var random = new Random();
            var enemyPoint = new Point(0,1);
            while (!InBounds(enemyPoint) || Cells[enemyPoint.Y, enemyPoint.X] == MapCell.Wall || 
                Player.Position == enemyPoint || Enemies.Any(e => e.Position == enemyPoint))
            {
                enemyPoint.X = random.Next(0, Columns);
                enemyPoint.Y = random.Next(1, Rows);
            }
            return enemyPoint;
        }
        private MapCell[,] ParseMaze(string[] rows) 
        {
            var mapCells = new MapCell[rows.Length + 2, rows.Max(str => str.Length)];
            for (int i = 0; i < rows[0].Length; i++)
                mapCells[0, i] = MapCell.Wall;
            for (int row = 0; row < rows.Length; row++)
            {
                for (int column = 0; column < rows[row].Length; column++)
                {
                    if (rows[row][column] == ' ')
                        mapCells[row + 1, column] = MapCell.Empty;
                    else if (rows[row][column] == '#')
                        mapCells[row + 1, column] = MapCell.Wall;
                    else if (rows[row][column] == 'S')
                    {
                        mapCells[row + 1, column] = MapCell.Empty;
                        StartPoint = new Point(column, row + 1);
                    }
                    else if (rows[row][column] == 'A')
                    {
                        mapCells[row + 1, column] = MapCell.Artefact;
                        CreateArtefact(new Point(column, row + 1));
                    }
                    else if (rows[row][column] == 'P')
                    {
                        mapCells[row + 1, column] = MapCell.Peaks;
                        peaks.AddLast(new Point(column, row + 1));
                    }
                    else
                    {
                        mapCells[row + 1, column] = MapCell.Empty;
                        EndPoint = new Point(column, row + 1);
                    }
                }
            }
            for (int i = 0; i < rows[0].Length; i++)
                mapCells[rows.Length + 1, i] = MapCell.Wall;
            return mapCells;
        }
        private void CreateArtefact(Point point) 
        {
            var random = new Random();
            var artefact = (Artefacts)random.Next(0, 3);
            switch (artefact)
            {
                case Artefacts.Damage:
                    artefacts[point] = (Artefacts.Damage, new Point(0,0), false, () => Player.Damage *= 2);
                    break;
                case Artefacts.Hp:
                    artefacts[point] = (Artefacts.Hp, new Point(768 / 8 * 5,864 / 9*3), false, () => Player.Health *= 2);
                    break;
                case Artefacts.Energy:
                    artefacts[point] = (Artefacts.Energy, new Point(768 / 8 * 3, 864 / 9 * 8), false, () => Player.CurrentEnergy *= 2);
                    break;
            }
        }
        public bool InBounds(Point point) => (point.X >= 0 && point.X <= Columns - 1) && (point.Y >= 0 && point.Y <= Rows - 1);
    }
}
