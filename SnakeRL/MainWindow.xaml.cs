using SnakeRL.GameLogic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeRL
{
    public partial class MainWindow : Window
    {
        const int GridW = 12, GridH = 12, Cell = 48;
        Game _game = new(GridW, GridH);
        DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(120) };
        int _score = 0;
        bool _gameRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            Width = GridW * Cell + 16;
            Height = GridH * Cell + 39 + 30;

            _timer.Tick += (_, __) => GameTick();
            UpdateScore();
            Draw();
        }

        void GameTick()
        {
            var (reward, done) = _game.Step(_game.LastAction);
            if (reward > 0) _score += (int)reward;
            if (done)
            {
                MessageBox.Show($"Игра окончена! Очки: {_score}", "SnakeRL");
                _score = 0;
                _game.Reset();
            }
            UpdateScore();
            Draw();
        }

        void StartGame()
        {
            if (_gameRunning) return;
            _gameRunning = true;
            StartOverlay.Visibility = Visibility.Collapsed;
            _timer.Start();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        void UpdateScore()
        {
            ScoreLabel.Content = $"Очки: {_score}  |  Длина: {_game.Snake.Count}";
        }

        void Draw()
        {
            GameCanvas.Children.Clear();
            // сетка
            for (int x = 0; x < GridW; x++)
                for (int y = 0; y < GridH; y++)
                {
                    var r = new Rectangle { Width = Cell - 1, Height = Cell - 1, Fill = Brushes.DimGray };
                    Canvas.SetLeft(r, x * Cell);
                    Canvas.SetTop(r, y * Cell);
                    GameCanvas.Children.Add(r);
                }
            // еда
            var food = new Rectangle { Width = Cell - 6, Height = Cell - 6, Fill = Brushes.Red, RadiusX = 6, RadiusY = 6 };
            Canvas.SetLeft(food, _game.FoodX * Cell + 3);
            Canvas.SetTop(food, _game.FoodY * Cell + 3);
            GameCanvas.Children.Add(food);
            // змейка
            foreach (var (x, y) in _game.Snake)
            {
                var s = new Rectangle { Width = Cell - 4, Height = Cell - 4, Fill = Brushes.LimeGreen };
                Canvas.SetLeft(s, x * Cell + 2);
                Canvas.SetTop(s, y * Cell + 2);
                GameCanvas.Children.Add(s);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    StartGame();
                    _game.LastAction = Dir.Up; break;
                case Key.D:
                case Key.Right:
                    StartGame();
                    _game.LastAction = Dir.Right; break;
                case Key.S:
                case Key.Down:
                    StartGame();
                    _game.LastAction = Dir.Down; break;
                case Key.A:
                case Key.Left:
                    StartGame();
                    _game.LastAction = Dir.Left; break;
            }
        }
    }
}
