using SnakeRL.GameLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        List<PlayerProfile> _profiles = new();
        PlayerProfile? _currentProfile;
        string ProfilesPath => System.IO.Path.Combine(AppContext.BaseDirectory, "players.json");

        public MainWindow()
        {
            InitializeComponent();
            Width = GridW * Cell + 16;
            Height = GridH * Cell + 39 + 30;

            _timer.Tick += (_, __) => GameTick();
            UpdateScore();
            Draw();
            LoadProfiles();
        }

        void LoadProfiles()
        {
            if (File.Exists(ProfilesPath))
            {
                try
                {
                    var json = File.ReadAllText(ProfilesPath);
                    var list = JsonSerializer.Deserialize<List<PlayerProfile>>(json);
                    if (list != null)
                        _profiles = list;
                }
                catch { }
            }
            PlayerComboBox.ItemsSource = _profiles;
            if (_profiles.Count > 0) PlayerComboBox.SelectedIndex = 0;
        }

        void SaveProfiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ProfilesPath, json);
            }
            catch { }
        }

        void GameTick()
        {
            var (reward, done) = _game.Step(_game.LastAction);
            if (reward > 0) _score += (int)reward;
            if (done)
            {
                _timer.Stop();
                _gameRunning = false;
                StartOverlay.Visibility = Visibility.Visible;
                if (_currentProfile != null)
                {
                    if (_game.Snake.Count > _currentProfile.BestLength)
                        _currentProfile.BestLength = _game.Snake.Count;
                    SaveProfiles();
                }
                _score = 0;
                _game.Reset();
            }
            UpdateScore();
            Draw();
        }

        void StartGame()
        {
            if (_gameRunning) return;

            var name = NewPlayerTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                _currentProfile = _profiles.FirstOrDefault(p => p.Name == name);
                if (_currentProfile == null)
                {
                    _currentProfile = new PlayerProfile { Name = name };
                    _profiles.Add(_currentProfile);
                    PlayerComboBox.Items.Refresh();
                }
                PlayerComboBox.SelectedItem = _currentProfile;
                NewPlayerTextBox.Clear();
            }
            else if (PlayerComboBox.SelectedItem is PlayerProfile selected)
            {
                _currentProfile = selected;
            }

            if (_currentProfile != null)
            {
                _currentProfile.GamesPlayed++;
                SaveProfiles();
            }

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
