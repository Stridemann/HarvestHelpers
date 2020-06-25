using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects;
using HarvestHelpers.HarvestObjects.Base;
using ImGuiNET;
using SharpDX;
using Color = SharpDX.Color;
using nuVector2 = System.Numerics.Vector2;
using RectangleF = SharpDX.RectangleF;

namespace HarvestHelpers
{
    public class HarvestHelpersCore : BaseSettingsPlugin<Settings>
    {
        private readonly ConcurrentDictionary<uint, HarvestObject> _objects =
            new ConcurrentDictionary<uint, HarvestObject>();

        private readonly Vector2 _defaultGroveCenter = new Vector2(379, 402); //Default pos of Metadata/Terrain/Leagues/Harvest/Objects/SoulTree on The Sacred Grove
        private Vector2 _groveCenter;//Current pos of Metadata/Terrain/Leagues/Harvest/Objects/SoulTree

        private MapController _mapController;
        private readonly Stopwatch _updateStopwatch = Stopwatch.StartNew();
        private readonly StringBuilder _errorStringBuilder = new StringBuilder();


        public override bool Initialise()
        {
            _mapController = new MapController(Graphics, DirectoryFullName, Settings);
            Input.RegisterKey(Keys.LButton);
            Input.RegisterKey(Settings.Toggle.Value);
            Settings.FixOutOfScreen.OnPressed += FixOutOfScreen;
            ResetCenter();
            return true;
        }

        private void FixOutOfScreen()
        {
            Settings.PosX = Settings.PosY = 100;
            Settings.Width = Settings.Height = 500;
        }

        private void ResetCenter()
        {
            _groveCenter = new Vector2(Single.MinValue, Single.MinValue);
            _mapController.CoordsOffset = Vector2.Zero;
        }

        public override void AreaChange(AreaInstance area)
        {
            ResetCenter();
        }


        public override void Render()
        {
            var isOnMap = Vector2.Distance(GameController.Player.GridPos, _groveCenter) < 400;
            if (!isOnMap)
                return;
            
            if (Settings.Toggle.PressedOnce())
                Settings.IsShown = !Settings.IsShown;

            UpdateAndValidate();
            if (!Settings.IsShown)
            {
                Graphics.DrawText($"HarvestHelpers is hidden. Press '{Settings.Toggle.Value}' to show window",
                    new nuVector2(500, 5), Color.Gray);
                return;
            }

            Graphics.DrawText($"Hide: press '{Settings.Toggle.Value}' button",
                new nuVector2(Settings.PosX, Settings.PosY - 17), Color.White);

            ImGui.SetNextWindowPos(new nuVector2(Settings.PosX, Settings.PosY), ImGuiCond.Once, nuVector2.Zero);
            ImGui.SetNextWindowSize(new nuVector2(Settings.Width - 20, Settings.Height), ImGuiCond.Always);

            var _opened = true;
            if (ImGui.Begin($"{Name}", ref _opened,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground |
                //ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | 
                //ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus | 
                ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings))
            {
                DrawWindowContent();


                var pos = ImGui.GetWindowPos();
                Settings.PosX = pos.X;
                Settings.PosY = pos.Y;

                var size = ImGui.GetWindowSize();
                Settings.Width = size.X + 20;
                Settings.Height = size.Y;

                var diff = Settings.Width - Settings.Height;
                Settings.Width -= diff / 2;
                Settings.Height += diff / 2;
            }

            ImGui.End();
        }

        private void DrawWindowContent()
        {
            var mapDrawFrame = new RectangleF(Settings.PosX, Settings.PosY + 20, Settings.Width,
                Settings.Height - 20);

            _mapController.Draw(mapDrawFrame);

            Graphics.DrawText("Resize ->", new nuVector2(mapDrawFrame.Right - 80, mapDrawFrame.Bottom - 15),
                Color.White);

            var playerPos = GameController.Player.GridPos;
            var drawPos = _mapController.GridPosToMapPos(playerPos);
            _mapController.DrawBoxOnMap(drawPos, 0.8f, Color.Red);

            foreach (var harvestObject in _objects.Values)
                harvestObject.DrawObject();
            
            DrawControls();
        }

        private void UpdateAndValidate()
        {
            if (_updateStopwatch.ElapsedMilliseconds > Settings.UpdateDelayMs.Value)
            {
                _updateStopwatch.Restart();
                _errorStringBuilder.Clear();
                foreach (var harvestObject in _objects.Values)
                    harvestObject.Update(_errorStringBuilder);
            }

            if (_errorStringBuilder.Length > 0)
                Graphics.DrawText(_errorStringBuilder.ToString(),
                    new nuVector2(10, 200),
                    Color.Red);
        }

        private bool _mouseDown;
        private bool _mouseClick;

        private void DrawControls()
        {
            _mouseClick = false;
            if (Input.IsKeyDown(Keys.LButton))
            {
                if (!_mouseDown)
                {
                    _mouseDown = true;
                    _mouseClick = true;
                }
            }
            else
            {
                _mouseDown = false;
            }


            const float posX = 580 + Constants.GRID_STEP;
            const float posY = 315 - Constants.GRID_STEP;

            var buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY + Constants.GRID_STEP));
            _mapController.DrawTextOnMap("Hide layer", buttonPos, Color.White, 15, FontAlign.Center);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY));
            Settings.DrawPylons = DrawButton("P", buttonPos, Settings.DrawPylons);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP));
            Settings.DrawCollectors = DrawButton("C", buttonPos, Settings.DrawCollectors);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 2));
            Settings.DrawDispensers = DrawButton("D", buttonPos, Settings.DrawDispensers);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 3));
            Settings.DrawStorage = DrawButton("S", buttonPos, Settings.DrawStorage);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 4));
            Settings.DrawLinks = DrawButton("/", buttonPos, Settings.DrawLinks);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 5));
            Settings.DrawSeeds = DrawButton("O", buttonPos, Settings.DrawSeeds);
        }

        private bool DrawButton(string name, Vector2 pos, bool value)
        {
            _mapController.DrawTextOnMap(name, pos, Color.Black, 15, FontAlign.Center);

            var rect = _mapController.DrawBoxOnMap(pos, 0.9f, value ? Color.White : Color.Gray);

            if (_mouseClick && rect.Contains(Input.MousePosition))
            {
                return !value;
            }

            return value;
        }

        public override void EntityAdded(Entity entity)
        {
            if (entity.League != LeagueType.Harvest)
                return;

            if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/MonsterSeed")
                _objects[entity.Id] = new HarvestSeed(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/StorageTank")
                _objects[entity.Id] = new HarvestTank(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/Extractor")
                _objects[entity.Id] = new HarvestCollector(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/Irrigator"
            ) //TODO: Check current_state is not 0 (available_fluid required_fluid). auto_irrigating is 1
                _objects[entity.Id] = new HarvestDispenser(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/Pole")
                _objects[entity.Id] = new HarvestPylon(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/HarvestPipeBeamEffect")
                _objects[entity.Id] = new HarvestBeamLink(entity, _mapController);
            else if (entity.Path == "Metadata/Terrain/Leagues/Harvest/Objects/SoulTree")
            {
                _groveCenter = entity.GridPos;
                _mapController.CoordsOffset = _groveCenter - _defaultGroveCenter;
            }
        }

        public override void EntityRemoved(Entity entity)
        {
            _objects.TryRemove(entity.Id, out _);
        }
    }
}