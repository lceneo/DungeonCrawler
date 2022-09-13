using System;
using System.Drawing;
using Игра.Entities;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Игра
{
    public partial class MazeGame : Form
    {
        public MazeGame()
        {
            InitialiseMenu();
            for (int i = 0; i < 5; i++)
                BattleForm._scenes[i] = new Map(new string[]{"S               ",
                                                         "                ",
                                                         "                ",
                                                         "               E"}, "hero", 2);
        }
        public MazeGame(int index, Entity player, Size formSize)
        {
            Size = formSize;
            _index = index;
            _mazes[_index].Player.Health = player.Health;
            _mazes[_index].Player.CurrentEnergy += player.CurrentEnergy;
            _mazes[_index].Player.Damage = player.Damage;
            InitialiseGame();
        }
        public static readonly Map[] _mazes = new Map[5] { new Map(new string[]
             {
                "S P       #",
                "#A        #",
                "E         #"
             }, "hero"), new Map(new string[]
             {
                 "SE#",
                "#P#",
                "#  "
             }, "hero"),
        new Map(new string[]
        {
            "S   P ",
            "    P ",
            "A        ",
            "########E"
        },"hero"),
        new Map(new string[]
        {
            "PSP",
            "PSP",
            "PSP",
            "PEP",
        },"hero"),
         new Map(new string[]
             {
                "S P       #",
                "#A        #",
                "E         #"
             }, "hero")
        };
        private static int _index;
        private static Bitmap _mainMenuBg = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\main menu.jpg");
        private static Bitmap _wallSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\wall.png");
        private static Bitmap _floorSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\floor.png");
        private static Bitmap _peaksSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\peaks.png");
        private static Bitmap _itemsSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\items.png");
        private static Bitmap _hudSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\hud.png");
        private static Bitmap _controllersSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\controllers.png");
        private static Bitmap _backArrowSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\backArrow.png");
        private static Bitmap _contollersPbSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\controllersPb.png");
        private static Bitmap _startGamePbSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
           + @"\Sprites\startGamePb.png");
        private static PictureBox _hudPb;
        private static PictureBox _hero;
        private static Entity _player;
        private static int _peakSprite = 0;
        private static bool _isDead = false;
        private static bool _isStopped = false;
        private static bool _animationIsExecuted = false;
        public void InitialiseMenu()
        {
            DoubleBuffered = true;
            Action<object, PaintEventArgs> dd = (s, e) => e.Graphics.DrawImage(_mainMenuBg, 0, 0, Size.Width, Size.Height);
            var drawBg = new PaintEventHandler(dd);
            var drawControllers = new PaintEventHandler((s, e) => e.Graphics.DrawImage(_controllersSprites,
                new Rectangle(Point.Empty, Size), new Rectangle(Point.Empty, new Size(1080, 456)), GraphicsUnit.Pixel));
            Paint += drawBg;
            var pbStart = new PictureBox()
            {
                Location = new Point(Size.Width / 3, Size.Height / 3),
                Size = new Size(Size.Width / 3, Size.Height / 6),
                BackgroundImage = _startGamePbSprites,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Transparent
            };
            var pbControllers = new PictureBox()
            {
                Location = new Point(pbStart.Location.X, pbStart.Bottom + Height / 15),
                Size = pbStart.Size,
                BackgroundImage = _contollersPbSprites,
                BackgroundImageLayout = ImageLayout.Stretch,
                BackColor = Color.Transparent
            };
            Controls.Add(pbStart);
            Controls.Add(pbControllers);
            var resizeEvent = new EventHandler((s, e) =>
            {
                pbStart.Location = new Point(Size.Width / 3, Size.Height / 3);
                pbStart.Size = new Size(Size.Width / 3, Size.Height / 6);
                pbControllers.Location = new Point(pbStart.Location.X, pbStart.Bottom + Height / 15);
                pbControllers.Size = pbStart.Size;
                Invalidate();
            });
            Resize += resizeEvent;
            pbStart.Click += (s, e) =>
            {
                Paint -= drawBg;
                Controls.Remove(pbStart);
                Controls.Remove(pbControllers);
                Resize -= resizeEvent;
                InitialiseGame();
            };
            pbControllers.Click += (s, e) =>
            {
                Paint += drawControllers;
                pbStart.Hide();
                pbControllers.Hide();
                var pbBack = new PictureBox()
                {
                    Location = new Point(Width / 10 * 9, 0),
                    Size = new Size(Width / 10, Height / 10),
                    BackgroundImage = _backArrowSprites,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    BackColor = Color.Transparent
                };
                var resizeArrow = new EventHandler((s, e) =>
                {
                    pbBack.Location = new Point(Width / 10 * 9, 0);
                    pbBack.Size = new Size(Width / 10, Height / 10);
                });
                pbBack.Click += (s, e) =>
                {
                    Paint -= drawControllers;
                    Controls.Remove(pbBack);
                    pbStart.Show();
                    pbControllers.Show();
                    Invalidate();
                };
                Resize += resizeArrow;
                Controls.Add(pbBack);
                Invalidate();
            };
        }
        public void InitialiseGame()
        {
            DoubleBuffered = true;
            _player = _mazes[_index].Player;
            _hero = new PictureBox()
            {
                BackColor = Color.Transparent,
                Location = new Point(Size.Width / _mazes[_index].Columns
                     * _player.Position.X, Size.Height / _mazes[_index].Rows * _player.Position.Y),
                Size = new Size(Size.Width / _mazes[_index].Columns, Size.Height / _mazes[_index].Rows),
                Visible = true
            };
            _hudPb = new PictureBox() { BackColor = Color.Transparent };
            Controls.Add(_hero);
            Controls.Add(_hudPb);
            DrawInstructions();
            Resize += (s, e) =>
            {
                Invalidate();
                ResizeOrChangePosition();
            };
            KeyUp += KeyPressed;
            StartIdleAnimation();
            StartPeaksAnimation();
        }

        public async void StartIdleAnimation()
        {
            var currentAnimation = -1;
            while (true)
            {
                if (_isStopped)
                    return;
                if (currentAnimation == 5)
                    currentAnimation = -1;
                currentAnimation++;
                _player.Frame = (currentAnimation, Animations.Idle);
                _hero.Invalidate();
                await Task.Delay(100);
            }
        }
        public async void StartPeaksAnimation()
        {
            while (true)
            {
                Invalidate();
                _hudPb.Invalidate();
                _peakSprite += _peaksSprites.Width / 4;
                if (_peakSprite == _peaksSprites.Width)
                    _peakSprite = 0;
                await Task.Delay(200);
            }
        }

        public void KeyPressed(object s, KeyEventArgs e)
        {
            var keyPressed = e.KeyData;
            if (_animationIsExecuted && keyPressed != Keys.R)
                return;
            switch (keyPressed)
            {
                case Keys.W:
                case Keys.Up:
                    ChangePosition(0, -1);
                    break;
                case Keys.S:
                case Keys.Down:
                    ChangePosition(0, 1);
                    break;
                case Keys.A:
                case Keys.Left:
                    ChangePosition(-1, 0);
                    break;
                case Keys.D:
                case Keys.Right:
                    ChangePosition(1, 0);
                    break;
                case Keys.R:
                    RestartLevel();
                    break;
            }
        }
        public void RestartLevel()
        {
            var timer = new Timer() { Interval = 600 };
            timer.Tick += (s, e) =>
            {
                if (_player.Frame.Item2 == Animations.Dead)
                {
                    _player.Frame = (0, Animations.Idle);
                    _hero.Invalidate();
                }
                _player.Position = _mazes[_index].StartPoint;
                _player.CurrentEnergy = _mazes[_index].Energy;
                _player.Damage = 1;
                _player.Health = 3;
                _player.CurrentEnergy = _mazes[_index].Energy;
                foreach (var artefact in _mazes[_index].artefacts)
                    _mazes[_index].artefacts[artefact.Key] = (artefact.Value.artefactType, artefact.Value.spriteLocation, false, artefact.Value.effect);
                _isDead = false;
                ResizeOrChangePosition();
                _isStopped = false;
                _animationIsExecuted = false;
                StartIdleAnimation();
                timer.Stop();
            };
            timer.Start();
        }
        public async void PerformAnimation(Animations animationType, int requiredFrames, (int, Animations) lastFrame)
        {
            _animationIsExecuted = true;
            _isStopped = true;
            _player.Frame = (0, animationType);
            while (_player.Frame.Item1 < requiredFrames)
            {
                _hero.Invalidate();
                await Task.Delay(100);
                _player.Frame = (_player.Frame.Item1 + 1, animationType);
            }
            _player.Frame = lastFrame;
            _hero.Invalidate();
            if (!_isDead)
            {
                _isStopped = false;
                StartIdleAnimation();
                _animationIsExecuted = false;
            }
        }
        public void ChangeLevel()
        {
            if (_index < BattleForm._scenes.Length)
            {
                new BattleForm(_index, _mazes[_index].Player, Size).Show();
                this.OnDeactivate(null);
                this.Hide();
            }
        }
        public async void ChangePosition(int x, int y)
        {
            var newPoint = new Point(_player.Position.X + x, _player.Position.Y + y);
            if (!_mazes[_index].InBounds(newPoint) || _mazes[_index].Cells[newPoint.Y, newPoint.X] == MapCell.Wall)
                return;
            _player.Position = newPoint;
            _player.CurrentEnergy--;
            if (_mazes[_index].Cells[newPoint.Y, newPoint.X] == MapCell.Empty)
            {
                PerformAnimation(Animations.Running, 6, (0, Animations.Idle));
                await Task.Delay(300);
                ResizeOrChangePosition();
            }
            else if (_mazes[_index].Cells[newPoint.Y, newPoint.X] == MapCell.Peaks)
            {
                PerformAnimation(Animations.Running, 6, (0, Animations.Idle));
                await Task.Delay(300);
                ResizeOrChangePosition();
                _player.Health = 0;
            }
            else if (_mazes[_index].Cells[newPoint.Y, newPoint.X] == MapCell.Artefact)
            {
                PerformAnimation(Animations.Running, 6, (0, Animations.Idle));
                await Task.Delay(380);
                ResizeOrChangePosition();
                if (_mazes[_index].artefacts.ContainsKey(newPoint) && _mazes[_index].artefacts[newPoint].isTaken == false)
                {
                    await Task.Delay(300);
                    _mazes[_index].artefacts[newPoint] = (_mazes[_index].artefacts[newPoint].artefactType, _mazes[_index].artefacts[newPoint]
                        .spriteLocation, true, _mazes[_index].artefacts[newPoint].effect);
                    _mazes[_index].artefacts[newPoint].effect();
                    PerformAnimation(Animations.PickingUp, 3, (0, Animations.Idle));
                }
            }
            if (newPoint == _mazes[_index].EndPoint)
            {
                MessageBox.Show("Victory");
                ChangeLevel();
            }
            else if (_player.Health == 0 || _player.CurrentEnergy == 0)
            {
                _isDead = true;
                await Task.Delay(380);
                PerformAnimation(Animations.Dead, 6, (5, Animations.Dead));
                await Task.Delay(680);
                return;
            }
        }

        public void ResizeOrChangePosition()
        {
            _hero.Location = new Point(Size.Width / _mazes[_index].Columns * _player.Position.X, Size.Height / _mazes[_index].Rows *
                _player.Position.Y);
            _hero.Size = new Size(Size.Width / _mazes[_index].Columns, Size.Height / _mazes[_index].Rows);
        }
        public void DrawInstructions()
        {
            _hudPb.Paint += (s, e) =>
            {
                var singleItemSize = new Size(Size.Width / 27, Size.Height / 15);
                _hudPb.Size = new Size(singleItemSize.Width * Math.Max(_player.CurrentEnergy, _player.Health), singleItemSize.Height * 3);
                _hudPb.Location = new Point(ClientSize.Width - singleItemSize.Width * Math.Max(_player.CurrentEnergy, _player.Health), 0);
                var g = e.Graphics;
                var y = 0;
                for (int i = 0, x = _hudPb.Size.Width - singleItemSize.Width; i < _player.Health; i++, x -= singleItemSize.Width)
                    g.DrawImage(_hudSprites, new Rectangle(new Point(x, y), singleItemSize), new Rectangle(Point.Empty, new Size(16, 16)),
                        GraphicsUnit.Pixel);
                y += singleItemSize.Height;
                for (int i = 0, x = _hudPb.Size.Width - singleItemSize.Width; i < _player.CurrentEnergy; i++, x -= singleItemSize.Width)
                    g.DrawImage(_hudSprites, new Rectangle(new Point(x, y), singleItemSize), new Rectangle(new Point(16, 0), new Size(16, 16)),
                        GraphicsUnit.Pixel);
            };
            _hero.Paint += (s, e) =>
            {
                var singleSizeBlock = new Size(Size.Width / _mazes[_index].Columns, Size.Height / _mazes[_index].Rows);
                var g = e.Graphics;
                g.DrawImage(_player.Model, new Rectangle(new Point(0, 0), singleSizeBlock), new Rectangle(new Point(_player.Model.Width / 6
                    * _player.Frame.Item1, _player.Model.Height / 17 * (byte)_player.Frame.Item2), new Size(_player.Model.Width / 6,
                    _player.Model.Height / 17)), GraphicsUnit.Pixel);
            };
            Paint += (s, e) =>
            {
                var singleSizeBlock = new Size(Size.Width / _mazes[_index].Columns, Size.Height / _mazes[_index].Rows);
                var g = e.Graphics;
                var floorX = 0;
                for (int rows = 0; rows < _mazes[_index].Rows; rows++)
                {
                    for (int columns = 0; columns < _mazes[_index].Columns; columns++)
                    {
                        if (_mazes[_index].Cells[rows, columns] == MapCell.Wall)
                            g.DrawImage(_wallSprites, new Rectangle(new Point(singleSizeBlock.Width * columns, singleSizeBlock.Height * rows),
                                        singleSizeBlock), new Rectangle(new Point(0, 0), new Size(_wallSprites.Width - 1, _wallSprites.Height - 1)),
                                        GraphicsUnit.Pixel);
                        else if (_mazes[_index].Cells[rows, columns] == MapCell.Peaks)
                            g.DrawImage(_peaksSprites, new Rectangle(new Point(singleSizeBlock.Width * columns, singleSizeBlock.Height * rows),
                                        singleSizeBlock), new Rectangle(new Point(_peakSprite, 0),
                                        new Size(_peaksSprites.Width / 4 - 1, _peaksSprites.Height - 1)), GraphicsUnit.Pixel);
                        else if (_mazes[_index].Cells[rows, columns] == MapCell.Artefact)
                        {
                            if (_mazes[_index].artefacts[new Point(columns, rows)].isTaken)
                                g.DrawImage(_floorSprites, new Rectangle(new Point(singleSizeBlock.Width * columns, singleSizeBlock.Height * rows),
                                        singleSizeBlock), new Rectangle(new Point(0, 0), new Size(_floorSprites.Width / 4 - 1, _floorSprites.Height - 1)),
                                        GraphicsUnit.Pixel);
                            else
                                g.DrawImage(_itemsSprites, new Rectangle(new Point(singleSizeBlock.Width * columns, singleSizeBlock.Height * rows),
                                            new Size(singleSizeBlock.Width, singleSizeBlock.Height)), new Rectangle(_mazes[_index].artefacts[new Point(columns, rows)].spriteLocation,
                                            new Size(_itemsSprites.Width / 8 - 1, _itemsSprites.Height / 9 - 1)), GraphicsUnit.Pixel);
                        }
                        else
                        {
                            g.DrawImage(_floorSprites, new Rectangle(new Point(singleSizeBlock.Width * columns, singleSizeBlock.Height * rows),
                                        singleSizeBlock), new Rectangle(new Point(floorX, 0), new Size(_floorSprites.Width / 4 - 1, _floorSprites.Height - 1)),
                                        GraphicsUnit.Pixel);
                            floorX += _floorSprites.Width / 4;
                            if (floorX >= _floorSprites.Width)
                                floorX = 0;
                        }
                    }
                    floorX = 0;
                }
            };
        }
    }
}