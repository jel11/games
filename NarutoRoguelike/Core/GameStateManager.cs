using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NarutoRoguelike.Data;
using NarutoRoguelike.Entities;
using NarutoRoguelike.Managers;
using NarutoRoguelike.Rendering;
using NarutoRoguelike.Systems;
using NarutoRoguelike.UI;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Core
{
    // ── GameState abstract base ───────────────────────────────────────────────────

    public abstract class GameState
    {
        protected readonly GameStateManager Manager;

        protected GameState(GameStateManager manager)
        {
            Manager = manager;
        }

        public virtual void OnEnter()  { }
        public virtual void OnExit()   { }
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch sb, GameTime gameTime);
    }

    // ── GameStateManager ─────────────────────────────────────────────────────────

    public class GameStateManager
    {
        private readonly Game           _game;
        private readonly Stack<GameState> _stack = new();

        public GameStateManager(Game game) { _game = game; }

        public GameState? Current => _stack.Count > 0 ? _stack.Peek() : null;

        public void PushState(GameState state)
        {
            _stack.Push(state);
            state.OnEnter();
        }

        public void PopState()
        {
            if (_stack.Count == 0) return;
            _stack.Peek().OnExit();
            _stack.Pop();
        }

        public void ReplaceState(GameState state)
        {
            if (_stack.Count > 0)
            {
                _stack.Peek().OnExit();
                _stack.Pop();
            }
            _stack.Push(state);
            state.OnEnter();
        }

        public void ClearAndPush(GameState state)
        {
            while (_stack.Count > 0)
            {
                _stack.Peek().OnExit();
                _stack.Pop();
            }
            _stack.Push(state);
            state.OnEnter();
        }

        public void ExitGame() => _game.Exit();

        public void Update(GameTime gameTime) => Current?.Update(gameTime);

        public void Draw(SpriteBatch sb, GameTime gameTime) => Current?.Draw(sb, gameTime);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MainMenuState
    // ═══════════════════════════════════════════════════════════════════════════

    public class MainMenuState : GameState
    {
        private readonly MainMenuScreen        _menu;
        private readonly NarutoContentManager  _content;
        private readonly InputHandler          _input;
        private readonly Renderer              _renderer;

        public MainMenuState(GameStateManager manager,
                             NarutoContentManager content,
                             InputHandler input,
                             Renderer renderer)
            : base(manager)
        {
            _content  = content;
            _input    = input;
            _renderer = renderer;
            _menu     = new MainMenuScreen(content.GetTexture("pixel"),
                                           content.GetFont("default"),
                                           Constants.SCREEN_WIDTH,
                                           Constants.SCREEN_HEIGHT);
        }

        public override void Update(GameTime gameTime)
        {
            _menu.Update(_input);
            if (!_menu.SelectionMade) return;

            switch ((MainMenuScreen.Choice)_menu.SelectedOption)
            {
                case MainMenuScreen.Choice.NewGame:
                    Manager.ReplaceState(new PlayingState(Manager, _content, _input, _renderer, seed: Environment.TickCount));
                    break;

                case MainMenuScreen.Choice.Continue:
                    var save = SaveManager.Load();
                    if (save != null)
                        Manager.ReplaceState(new PlayingState(Manager, _content, _input, _renderer, save));
                    // else: no save — do nothing (menu re-renders)
                    break;

                case MainMenuScreen.Choice.Quit:
                    Manager.ExitGame();
                    break;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            _renderer.DrawMenu(b => _menu.Draw(b));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PlayingState
    // ═══════════════════════════════════════════════════════════════════════════

    public class PlayingState : GameState
    {
        // ── Dependencies ──────────────────────────────────────────────────────────
        private readonly NarutoContentManager _content;
        private readonly InputHandler         _input;
        private readonly Renderer             _renderer;
        private readonly Random               _rng;

        // ── World ─────────────────────────────────────────────────────────────────
        private Map         _map    = null!;
        private Player      _player = null!;
        private List<Enemy> _enemies     = new();
        private List<Item>  _groundItems = new();
        private Camera      _camera = null!;

        // ── Systems ───────────────────────────────────────────────────────────────
        private readonly TurnSystem  _turns  = new();
        private readonly MessageLog  _log    = new();

        // ── State ─────────────────────────────────────────────────────────────────
        private int  _floor   = 1;
        private int  _seed;
        private bool _inventoryOpen;

        // ── Constructor: new game ─────────────────────────────────────────────────
        public PlayingState(GameStateManager manager,
                            NarutoContentManager content,
                            InputHandler input,
                            Renderer renderer,
                            int seed)
            : base(manager)
        {
            _content  = content;
            _input    = input;
            _renderer = renderer;
            _seed     = seed;
            _rng      = new Random(seed);
        }

        // ── Constructor: continue from save ──────────────────────────────────────
        public PlayingState(GameStateManager manager,
                            NarutoContentManager content,
                            InputHandler input,
                            Renderer renderer,
                            SaveGame save)
            : this(manager, content, input, renderer, save.MapSeed)
        {
            _floor = save.Floor;
            // Seed rng deterministically from save
            _rng = new Random(save.MapSeed + save.Floor * 1000 + save.TurnNumber);
        }

        public override void OnEnter()
        {
            GenerateFloor(_floor, _seed + _floor * 137);
            _log.Add($"Floor {_floor} — good luck, ninja!", Constants.NarutoOrange);
        }

        // ── Floor generation ──────────────────────────────────────────────────────

        private void GenerateFloor(int floor, int floorSeed)
        {
            _map = MapGenerator.Generate(Constants.MAP_WIDTH, Constants.MAP_HEIGHT, floorSeed);

            // Spawn player at first room centre
            var start = _map.Rooms.Count > 0 ? _map.Rooms[0].Center : new Point(2, 2);
            _player = new Player(start.X, start.Y)
            {
                Texture     = _content.GetTexture("player"),
                CurrentFloor = floor
            };

            _player.Health.OnDeath += OnPlayerDeath;

            // Enemies
            _enemies.Clear();
            int enemyCount = 3 + floor * 2;
            var spawnPoints = MapGenerator.GetEnemySpawnPoints(_map, enemyCount);
            var templates   = new List<EnemyTemplate>(EnemyDatabase.ForFloor(floor));

            foreach (var pt in spawnPoints)
            {
                if (templates.Count == 0) break;
                var tmpl  = templates[_rng.Next(templates.Count)];
                var enemy = new Enemy(tmpl.Name, tmpl.Rank, pt.X, pt.Y)
                {
                    Texture = _content.GetTexture(tmpl.TextureKey)
                };
                _enemies.Add(enemy);
            }

            // Ground items
            _groundItems.Clear();
            var itemPoints = MapGenerator.GetItemSpawnPoints(_map);
            var itemTmplList = new List<ItemTemplate>(ItemDatabase.ForFloor(floor));

            foreach (var pt in itemPoints)
            {
                if (itemTmplList.Count == 0) break;
                var tmpl = itemTmplList[_rng.Next(itemTmplList.Count)];
                var item = new Item(tmpl.Name, tmpl.Type, tmpl.Slot, pt.X, pt.Y)
                {
                    Texture      = _content.GetTexture("item"),
                    AttackBonus  = tmpl.AttackBonus,
                    DefenseBonus = tmpl.DefenseBonus,
                    HPBonus      = tmpl.HPBonus,
                    ChakraBonus  = tmpl.ChakraBonus,
                    HealAmount   = tmpl.HealAmount,
                    ChakraAmount = tmpl.ChakraAmount,
                    Description  = tmpl.Description,
                };
                _groundItems.Add(item);
            }

            // Camera
            int vpW = Constants.VIEWPORT_TILES_X * Constants.TILE_SIZE;
            int vpH = (Constants.SCREEN_HEIGHT - Constants.HUD_HEIGHT);
            _camera = new Camera(vpW - Constants.LOG_WIDTH, vpH,
                                 Constants.MAP_WIDTH  * Constants.TILE_SIZE,
                                 Constants.MAP_HEIGHT * Constants.TILE_SIZE);
            _camera.Follow(_player);

            // Turn system
            _turns.RegisterEntities(_player, _enemies);

            // Initial FOV
            _map.ComputeFOV(_player.GridX, _player.GridY, Constants.FOV_RADIUS);
        }

        // ── Update ────────────────────────────────────────────────────────────────

        public override void Update(GameTime gameTime)
        {
            _renderer.Particles.Update(gameTime);

            if (_inventoryOpen)
            {
                HandleInventory();
                return;
            }

            // Pause
            if (_input.IsJustPressed(Keys.Escape) || _input.IsButtonJustPressed(Buttons.Start))
            {
                Manager.PushState(new PausedState(Manager, _content, _input, _renderer, this));
                return;
            }

            // Process turns
            while (!_turns.PlayerTurnPending && _player.IsAlive)
                ProcessEnemyTurn();

            if (_turns.PlayerTurnPending && _player.IsAlive)
                ProcessPlayerTurn();
        }

        private void ProcessPlayerTurn()
        {
            bool acted = false;

            // Inventory
            if (_input.IsJustPressed(Keys.I))
            {
                _inventoryOpen = true;
                _player.IsInventoryOpen = true;
                return;
            }

            // Pick up item
            if (_input.IsJustPressed(Keys.G) || _input.IsJustPressed(Keys.OemComma))
            {
                PickUpItemAtPlayer();
                acted = true;
            }

            // Jutsu keys 1-7
            int jutsuKey = _input.GetJutsuKey();
            if (jutsuKey > 0)
            {
                int idx = jutsuKey - 1;
                if (idx < _player.KnownJutsu.Count)
                {
                    bool used = JutsuSystem.ActivateJutsu(
                        _player.KnownJutsu[idx], _player, _enemies, _map, _log, _rng);
                    if (used)
                    {
                        _renderer.Particles.EmitChakra(
                            new Vector2(_player.GridX * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f,
                                        _player.GridY * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f));
                        acted = true;
                    }
                }
            }

            // Movement / attack
            if (!acted)
            {
                var (dx, dy) = _input.GetMoveDirection();
                if (dx != 0 || dy != 0)
                {
                    TryMovePlayer(dx, dy);
                    acted = true;
                }

                // Wait (skip turn)
                if (_input.IsJustPressed(Keys.OemPeriod) || _input.IsJustPressed(Keys.NumPad5))
                    acted = true;
            }

            // Stairs
            if (!acted)
            {
                if (_input.IsJustPressed(Keys.OemPeriod) && _player.GridX == _map.StairsDownPos.X && _player.GridY == _map.StairsDownPos.Y)
                {
                    DescendStairs();
                    acted = true;
                }
                else if (_input.IsJustPressed(Keys.OemComma) && _player.GridX == _map.StairsUpPos.X && _player.GridY == _map.StairsUpPos.Y)
                {
                    AscendStairs();
                    acted = true;
                }
            }

            if (acted)
            {
                _player.OnTurnEnd();
                CleanUpDead();
                _turns.RemoveDead();
                _turns.EndPlayerTurn();
            }
        }

        private void ProcessEnemyTurn()
        {
            var entity = _turns.GetCurrentEntity();
            if (entity == null) { _turns.Advance(); return; }

            if (entity is Enemy enemy && enemy.IsAlive)
            {
                var (dx, dy) = AISystem.Update(enemy, _player, _map, _log, _rng);
                if (dx != 0 || dy != 0)
                    TryMoveEnemy(enemy, dx, dy);
            }

            _turns.Advance();
        }

        // ── Movement ──────────────────────────────────────────────────────────────

        private void TryMovePlayer(int dx, int dy)
        {
            int nx = _player.GridX + dx;
            int ny = _player.GridY + dy;

            // Attack enemy at target
            var target = FindEnemyAt(nx, ny);
            if (target != null)
            {
                CombatSystem.Attack(_player, target, _log, _rng);
                _renderer.Particles.EmitImpact(
                    new Vector2(nx * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f,
                                ny * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f));
                return;
            }

            // Move
            if (_map.IsWalkable(nx, ny))
            {
                _player.GridX = nx;
                _player.GridY = ny;
                _map.ComputeFOV(_player.GridX, _player.GridY, Constants.FOV_RADIUS);
                _camera.Follow(_player);

                // Stair hint
                ref var tile = ref _map.GetTile(nx, ny);
                if (tile.Type == TileType.StairsDown)
                    _log.Add("You see stairs leading down (press . to descend).", Constants.UIText);
                else if (tile.Type == TileType.StairsUp)
                    _log.Add("You see stairs leading up (press , to ascend).", Constants.UIText);
            }
        }

        private void TryMoveEnemy(Enemy enemy, int dx, int dy)
        {
            int nx = enemy.GridX + dx;
            int ny = enemy.GridY + dy;

            if (_player.GridX == nx && _player.GridY == ny) return; // would attack — handled by AISystem

            if (_map.IsWalkable(nx, ny) && FindEnemyAt(nx, ny) == null)
            {
                enemy.GridX = nx;
                enemy.GridY = ny;
            }
        }

        // ── Inventory ─────────────────────────────────────────────────────────────

        private void HandleInventory()
        {
            var inv = _renderer.GetInventory();
            inv.Update(_input, _player);

            if (inv.CloseRequested)
            {
                _inventoryOpen          = false;
                _player.IsInventoryOpen = false;
                return;
            }

            if (inv.Action == InventoryScreen.PendingAction.Use && inv.ActionItem != null)
                ItemSystem.Use(_player, inv.ActionItem, _log);
            else if (inv.Action == InventoryScreen.PendingAction.Drop && inv.ActionItem != null)
                ItemSystem.Drop(_player, inv.ActionItem, _groundItems, _log);
            else if (inv.Action == InventoryScreen.PendingAction.Equip && inv.ActionItem != null)
                ItemSystem.Equip(_player, inv.ActionItem, _log);
        }

        private void PickUpItemAtPlayer()
        {
            Item? item = null;
            foreach (var i in _groundItems)
                if (i.GridX == _player.GridX && i.GridY == _player.GridY) { item = i; break; }

            if (item == null) { _log.Add("Nothing to pick up here.", Constants.UIText); return; }
            ItemSystem.PickUp(_player, item, _groundItems, _log);
            _renderer.Particles.EmitHeal(
                new Vector2(_player.GridX * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f,
                            _player.GridY * Constants.TILE_SIZE + Constants.TILE_SIZE / 2f));
        }

        // ── Stairs ────────────────────────────────────────────────────────────────

        private void DescendStairs()
        {
            _floor++;
            if (_floor > 10)
            {
                Manager.ReplaceState(new VictoryState(Manager, _content, _input, _renderer));
                return;
            }
            _log.Add($"Descending to Floor {_floor}…", Constants.UIHighlight);
            GenerateFloor(_floor, _seed + _floor * 137);
        }

        private void AscendStairs()
        {
            if (_floor <= 1) { _log.Add("You can't go higher — this is Floor 1.", Constants.UIText); return; }
            _floor--;
            _log.Add($"Ascending to Floor {_floor}…", Constants.UIHighlight);
            GenerateFloor(_floor, _seed + _floor * 137);
        }

        // ── Death handler ─────────────────────────────────────────────────────────

        private void OnPlayerDeath()
        {
            _log.Add("You have been defeated! Press any key…", Constants.AkatsukiRed);
            Manager.ReplaceState(new GameOverState(Manager, _content, _input, _renderer, _floor, _player.Level));
        }

        // ── Cleanup ───────────────────────────────────────────────────────────────

        private void CleanUpDead()
        {
            foreach (var e in _enemies)
            {
                if (!e.IsAlive && e.Health.IsDead)
                {
                    _player.GainXP(e.XPValue, _log);
                    // Small chance to drop an item
                    if (_rng.NextDouble() < 0.3)
                        SpawnRandomItem(e.GridX, e.GridY);
                }
            }
            _enemies.RemoveAll(e => !e.IsAlive);
        }

        private void SpawnRandomItem(int x, int y)
        {
            var tmplList = new List<ItemTemplate>(ItemDatabase.ForFloor(_floor));
            if (tmplList.Count == 0) return;
            var tmpl = tmplList[_rng.Next(tmplList.Count)];
            _groundItems.Add(new Item(tmpl.Name, tmpl.Type, tmpl.Slot, x, y)
            {
                Texture      = _content.GetTexture("item"),
                AttackBonus  = tmpl.AttackBonus,
                DefenseBonus = tmpl.DefenseBonus,
                HPBonus      = tmpl.HPBonus,
                ChakraBonus  = tmpl.ChakraBonus,
                HealAmount   = tmpl.HealAmount,
                ChakraAmount = tmpl.ChakraAmount,
                Description  = tmpl.Description,
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private Enemy? FindEnemyAt(int x, int y)
        {
            foreach (var e in _enemies)
                if (e.IsAlive && e.GridX == x && e.GridY == y) return e;
            return null;
        }

        // ── Draw ──────────────────────────────────────────────────────────────────

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            _renderer.DrawWorld(_map, _camera, _player, _enemies, _groundItems, _log, _floor, gameTime);

            if (_inventoryOpen)
            {
                // Inventory is drawn as an overlay on top of the world
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                _renderer.GetInventory().Draw(sb, _player, Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
                sb.End();
            }
        }

        // ── Save snapshot ─────────────────────────────────────────────────────────

        public SaveGame CreateSaveGame() => new SaveGame
        {
            Floor      = _floor,
            MapSeed    = _seed,
            TurnNumber = _turns.TurnNumber,
            Player     = SaveGame.FromPlayer(_player),
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PausedState
    // ═══════════════════════════════════════════════════════════════════════════

    public class PausedState : GameState
    {
        private readonly PauseMenuScreen       _menu;
        private readonly NarutoContentManager  _content;
        private readonly InputHandler          _input;
        private readonly Renderer              _renderer;
        private readonly PlayingState          _playState;

        public PausedState(GameStateManager manager,
                           NarutoContentManager content,
                           InputHandler input,
                           Renderer renderer,
                           PlayingState playState)
            : base(manager)
        {
            _content   = content;
            _input     = input;
            _renderer  = renderer;
            _playState = playState;
            _menu      = new PauseMenuScreen(content.GetTexture("pixel"),
                                             content.GetFont("default"),
                                             Constants.SCREEN_WIDTH,
                                             Constants.SCREEN_HEIGHT);
        }

        public override void Update(GameTime gameTime)
        {
            _menu.Update(_input);
            if (_input.IsJustPressed(Keys.Escape)) { Manager.PopState(); return; }
            if (!_menu.SelectionMade) return;

            switch ((PauseMenuScreen.Choice)_menu.SelectedOption)
            {
                case PauseMenuScreen.Choice.Resume:
                    Manager.PopState();
                    break;

                case PauseMenuScreen.Choice.SaveQuit:
                    SaveManager.Save(_playState.CreateSaveGame());
                    Manager.ClearAndPush(new MainMenuState(Manager, _content, _input, _renderer));
                    break;

                case PauseMenuScreen.Choice.QuitNoSave:
                    Manager.ClearAndPush(new MainMenuState(Manager, _content, _input, _renderer));
                    break;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            // Draw the world behind (frozen)
            _playState.Draw(sb, gameTime);
            // Overlay the pause menu
            _renderer.DrawMenu(b => _menu.Draw(b));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GameOverState
    // ═══════════════════════════════════════════════════════════════════════════

    public class GameOverState : GameState
    {
        private readonly GameOverScreen        _screen;
        private readonly NarutoContentManager  _content;
        private readonly InputHandler          _input;
        private readonly Renderer              _renderer;

        public GameOverState(GameStateManager manager,
                             NarutoContentManager content,
                             InputHandler input,
                             Renderer renderer,
                             int floor, int level)
            : base(manager)
        {
            _content  = content;
            _input    = input;
            _renderer = renderer;
            _screen   = new GameOverScreen(content.GetTexture("pixel"),
                                           content.GetFont("default"),
                                           Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT,
                                           floor, level);
        }

        public override void Update(GameTime gameTime)
        {
            _screen.Update(_input);
            if (!_screen.SelectionMade) return;

            switch ((GameOverScreen.Choice)_screen.SelectedOption)
            {
                case GameOverScreen.Choice.Retry:
                    SaveManager.DeleteSave();
                    Manager.ReplaceState(new PlayingState(Manager, _content, _input, _renderer,
                                                           seed: Environment.TickCount));
                    break;
                case GameOverScreen.Choice.MainMenu:
                    SaveManager.DeleteSave();
                    Manager.ClearAndPush(new MainMenuState(Manager, _content, _input, _renderer));
                    break;
                case GameOverScreen.Choice.Quit:
                    Manager.ExitGame();
                    break;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            _renderer.DrawMenu(b => _screen.Draw(b));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VictoryState
    // ═══════════════════════════════════════════════════════════════════════════

    public class VictoryState : GameState
    {
        private readonly VictoryScreen         _screen;
        private readonly NarutoContentManager  _content;
        private readonly InputHandler          _input;
        private readonly Renderer              _renderer;

        public VictoryState(GameStateManager manager,
                            NarutoContentManager content,
                            InputHandler input,
                            Renderer renderer)
            : base(manager)
        {
            _content  = content;
            _input    = input;
            _renderer = renderer;
            _screen   = new VictoryScreen(content.GetTexture("pixel"),
                                          content.GetFont("default"),
                                          Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
        }

        public override void Update(GameTime gameTime)
        {
            _screen.Update(_input);
            if (!_screen.SelectionMade) return;

            switch ((VictoryScreen.Choice)_screen.SelectedOption)
            {
                case VictoryScreen.Choice.MainMenu:
                    Manager.ClearAndPush(new MainMenuState(Manager, _content, _input, _renderer));
                    break;
                case VictoryScreen.Choice.Quit:
                    Manager.ExitGame();
                    break;
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            _renderer.DrawMenu(b => _screen.Draw(b));
        }
    }
}
