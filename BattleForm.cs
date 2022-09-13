using System;
using System.Collections.Generic;
using System.Drawing;
using Игра.Entities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Игра
{
    public partial class BattleForm : Form
    {
        public BattleForm(int index, Entity player, Size formSize)
        {
            Size = formSize;
            _index = index;
            _scenes[_index].Player.Health = player.Health;
            _scenes[_index].Player.CurrentEnergy = player.CurrentEnergy;
            _scenes[_index].Player.Damage = player.Damage;
            _enemies = new PictureBox[_scenes[_index].Enemies.Length];
            Initialise();
        }
        private readonly PictureBox _hero = new PictureBox() { BackColor = Color.Transparent };
        private static Bitmap _bgSprite = new Bitmap(Image.FromFile(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\bg.png"));
        private static Bitmap _hudSprites = new Bitmap(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent
            + @"\Sprites\hud.png");
        private static PictureBox _hudPb;
        public static Map[] _scenes = new Map[5];
        private  PictureBox[] _enemies;
        private static bool _isRunning = false;
        private static int _index;
        public BattleForm()
        {
            Initialise();
            InitializeComponent();
        }
        public void Initialise()
        {
            _hero.Click += (s, e) => PerformBattleAnimation(_hero, _scenes[_index].Player, Animations.Fighting2, (0, Animations.Idle), 5);
            DoubleBuffered = true;
            for (int i = 0; i < _enemies.Length; i++)
            {
                _enemies[i] = new PictureBox() { BackColor = Color.Transparent };
                _hero.Controls.Add(_enemies[i]);
            }
            _hero.Location = new Point(0, ClientSize.Height / 2 + ClientSize.Height / 2 / _scenes[_index].Rows);
            _hero.Size = new Size(ClientSize.Width, ClientSize.Height - _hero.Location.Y);
            for (int i = 0; i < _enemies.Length; i++)
            {
                var j = i;
                _enemies[j].MouseClick += (s, e) =>
                {
                    if (_scenes[_index].Enemies[j].isDead)
                        return;
                    var distance = Math.Sqrt((_scenes[_index].Player.Position.X - _scenes[_index].Enemies[j].Position.X) * (_scenes[_index].Player.Position.X -
                        _scenes[_index].Enemies[j].Position.X) + (_scenes[_index].Player.Position.Y - _scenes[_index].Enemies[j].Position.Y) * (_scenes[_index].Player.Position.Y -
                        _scenes[_index].Enemies[j].Position.Y));
                    if (distance > 1)
                        return;
                    if (_scenes[_index].Player.isDead)
                        return;
                    PerformBattleAnimation(_hero, _scenes[_index].Player, Animations.Fighting2, (0, Animations.Idle), 5);
                    _scenes[_index].Enemies[j].isHitted = true;
                    PerformBattleAnimation(_enemies[j], _scenes[_index].Enemies[j], Animations.Fighting3, (0, Animations.Idle), 2);
                    _scenes[_index].Enemies[j].Health--;
                    if (_scenes[_index].Enemies[j].Health == 0)
                    {
                        _scenes[_index].Enemies[j].isDead = true;
                        PerformBattleAnimation(_enemies[j], _scenes[_index].Enemies[j], Animations.Dead, (3, Animations.Dead), 3);
                    }
                };
            }
            _hudPb = new PictureBox() { BackColor = Color.Transparent };
            DrawInstructions();
            ResizeAndChangeHeroAndEnemiesPosition();
            Controls.Add(_hero);
            Controls.Add(_hudPb);
            KeyDown += MoveHero;
            Resize += (s, e) =>
            {
                _hero.Location = new Point(0, ClientSize.Height / 2 + ClientSize.Height / 2 / _scenes[_index].Rows);
                _hero.Size = new Size(ClientSize.Width, ClientSize.Height - _hero.Location.Y);
                _hero.Invalidate();
                ResizeAndChangeHeroAndEnemiesPosition();
                Invalidate();
            };
            StartMonstersMove();
        }
        public async void HitPlayer(PictureBox enemyPb, Entity enemy)
        {
            PerformBattleAnimation(enemyPb, enemy, Animations.Fighting2, (0, Animations.Idle), 3);
            await Task.Delay(200);
            var playerHitted = PerformBattleAnimation(_hero, _scenes[_index].Player, Animations.Fighting3, (0, Animations.Idle), 4);
            _scenes[_index].Player.Health--;
            if (_scenes[_index].Player.Health == 0)
            {
                _scenes[_index].Player.isDead = true;
                await playerHitted;
                PerformBattleAnimation(_hero, _scenes[_index].Player, Animations.Dead, (5, Animations.Dead), 5);
                _hero.Invalidate();
            }
        }
        public async void StartMonstersMove()
        {
            while (true)
            {
                _hudPb.Invalidate();
                var list = new List<Task<(int, Point)>>();
                for (int i = 0; i < _scenes[_index].Enemies.Length; i++)
                {
                    var j = i;
                    list.Add(Task.Run(() => _scenes[_index].Enemies[j].GetBestPoint(_scenes[_index], j)));
                }
                while (list.Count > 0)
                {
                    var currentTask = await Task.WhenAny(list);
                    var result = currentTask.Result;
                    if (!_scenes[_index].Enemies[result.Item1].isHitted && !_scenes[_index].Enemies[result.Item1].isDead)
                    {
                        if (result.Item2 == _scenes[_index].Enemies[result.Item1].Position)
                        {
                            HitPlayer(_enemies[result.Item1], _scenes[_index].Enemies[result.Item1]);
                            break;
                        }
                        _scenes[_index].Enemies[result.Item1].Position = result.Item2;
                        ResizeAndChangeHeroAndEnemiesPosition();
                        PerformBattleAnimation(_enemies[result.Item1], _scenes[_index].Enemies[result.Item1], Animations.Running, (0, Animations.Idle), 3);
                    }
                    list.Remove(currentTask);
                    if (_scenes[_index].Player.isDead)
                        return;
                }
                await Task.Delay(1000);
            }
        }
        public void ResizeAndChangeHeroAndEnemiesPosition()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                _enemies[i].Location = new Point(ClientSize.Width / _scenes[_index].Columns * _scenes[_index].Enemies[i].Position.X, _hero.Size.Height
                    / _scenes[_index].Rows * _scenes[_index].Enemies[i].Position.Y);
                _enemies[i].Size = new Size(_hero.Size.Width / 8, ClientSize.Height / 4);
            }
        }
        public void DrawInstructions()
        {
            _hudPb.Paint += (s, e) =>
            {
                var singleItemSize = new Size(Size.Width / 27, Size.Height / 15);
                _hudPb.Size = new Size(singleItemSize.Width * Math.Max(_scenes[_index].Player.CurrentEnergy, _scenes[_index].Player.Health), singleItemSize.Height * 3);
                _hudPb.Location = new Point(ClientSize.Width - singleItemSize.Width * Math.Max(_scenes[_index].Player.CurrentEnergy, _scenes[_index].Player.Health), 0);
                var g = e.Graphics;
                var y = 0;
                for (int i = 0, x = _hudPb.Size.Width - singleItemSize.Width; i < _scenes[_index].Player.Health; i++, x -= singleItemSize.Width)
                    g.DrawImage(_hudSprites, new Rectangle(new Point(x, y), singleItemSize), new Rectangle(Point.Empty, new Size(16, 16)),
                        GraphicsUnit.Pixel);
                y += singleItemSize.Height;
                for (int i = 0, x = _hudPb.Size.Width - singleItemSize.Width; i < _scenes[_index].Player.CurrentEnergy; i++, x -= singleItemSize.Width)
                    g.DrawImage(_hudSprites, new Rectangle(new Point(x, y), singleItemSize), new Rectangle(new Point(16, 0), new Size(16, 16)),
                        GraphicsUnit.Pixel);
            };
            for (int i = 0; i < _enemies.Length; i++)
            {
                var y = i;
                _enemies[y].Paint += (s, e) => e.Graphics.DrawImage(_scenes[_index].Enemies[y].Model, new Rectangle(Point.Empty, _enemies[y].Size),
                    new Rectangle(new Point(104 * _scenes[_index].Enemies[y].Frame.Item1, 63 * (byte)_scenes[_index].Enemies[y].Frame.Item2),
                    new Size(104, 63)), GraphicsUnit.Pixel);
            }

            Paint += (s, e) => e.Graphics.DrawImage(_bgSprite, new Rectangle(Point.Empty, new Size(ClientSize.Width,
            ClientSize.Height)));
            _hero.Paint += (s, e) => e.Graphics.DrawImage(_scenes[_index].Player.Model, new Rectangle(new Point(_hero.Width /
                _scenes[_index].Columns * _scenes[_index].Player.Position.X, _hero.Height / _scenes[_index].Rows *
                _scenes[_index].Player.Position.Y), new Size(_hero.Size.Width / 8, ClientSize.Height / 4)),
                new Rectangle(new Point(69 * _scenes[_index].Player.Frame.Item1, 44 * (byte)_scenes[_index].Player.Frame.Item2),
                new Size(69, 44)), GraphicsUnit.Pixel);
        }
        public async Task PerformBattleAnimation(PictureBox entityPb, Entity entity, Animations animationType, (int, Animations) lastFrame,
            int requiredFrames)
        {
            if (_scenes[_index].Player.isDead && animationType != Animations.Dead)
                return;
            else if (entity == _scenes[_index].Player)
                _isRunning = true;
            else if (animationType == Animations.Running && entity.isHitted)
                return;
            while (entity.Frame.Item1 < requiredFrames)
            {
                if (entity.isDead && animationType != Animations.Dead)
                    return;
                entityPb.Invalidate();
                entity.Frame = (entity.Frame.Item1 + 1, animationType);
                await Task.Delay(100);
            }
            entity.Frame = lastFrame;
            entityPb.Invalidate();
            if (entity == _scenes[_index].Player)
                _isRunning = false;
            entity.isHitted = false;
        }
        public async void ChangePosition(int x, int y)
        {
            var newPoint = new Point(_scenes[_index].Player.Position.X + x, _scenes[_index].Player.Position.Y + y);
            if (!_scenes[_index].InBounds(newPoint) || _scenes[_index].Cells[newPoint.Y, newPoint.X] == MapCell.Wall)
                return;
            _scenes[_index].Player.Position = newPoint;
            PerformBattleAnimation(_hero, _scenes[_index].Player, Animations.Running, (0, Animations.Idle), 5);
            await Task.Delay(250);
            _hero.Invalidate();
            await Task.Delay(250);
            if (newPoint == _scenes[_index].EndPoint && _scenes[_index].Enemies.All(e => e.isDead))
            {
                if (_index + 1 < MazeGame._mazes.Length)
                {
                    new MazeGame(_index + 1, _scenes[_index].Player, Size).Show();
                    this.OnDeactivate(null);
                    this.Hide();
                }
                else
                    Application.Exit();
            }

        }
        public void MoveHero(object sender, KeyEventArgs e)
        {
            if (_isRunning || _scenes[_index].Player.isDead)
                return;
            var keyPressed = e.KeyData;
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
                case Keys.D:
                case Keys.Right:
                    ChangePosition(1, 0);
                    break;
                case Keys.A:
                case Keys.Left:
                    ChangePosition(-1, 0);
                    break;
            }
        }
    }
}
