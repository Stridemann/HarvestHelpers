using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
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

        private MapController _mapController;
        private bool _isOnMap;
        private readonly Stopwatch _updateStopwatch = Stopwatch.StartNew();
        private bool _opened = true;

        public override bool Initialise()
        {
            _mapController = new MapController(Graphics, DirectoryFullName);
            _isOnMap = GameController.Area.CurrentArea.Name == "The Sacred Grove";
            Input.RegisterKey(Keys.LButton);
            Input.RegisterKey(Settings.Toggle.Value);
            Settings.FixOutOfScreen.OnPressed += FixOutOfScreen;
            return true;
        }

        private void FixOutOfScreen()
        {
            Settings.PosX = Settings.PosY = 100;
            Settings.Width = Settings.Height = 500;
        }


        public override void AreaChange(AreaInstance area)
        {
            _isOnMap = area.Name == "The Sacred Grove";
        }


        public override void Render()
        {
            if (!_isOnMap)
                return;

            if (Settings.Toggle.PressedOnce())
                _opened = !_opened;

            if (!_opened)
            {
                Graphics.DrawText($"HarvestHelpers is hidden. Press '{Settings.Toggle.Value}' to show window",
                    new nuVector2(500, 5), Color.Gray);
                return;
            }

            Graphics.DrawText($"Hide: press '{Settings.Toggle.Value}' button",
                new nuVector2(Settings.PosX, Settings.PosY - 17), Color.White);

            ImGui.SetNextWindowPos(new nuVector2(Settings.PosX, Settings.PosY), ImGuiCond.Once, nuVector2.Zero);
            ImGui.SetNextWindowSize(new nuVector2(Settings.Width - 20, Settings.Height), ImGuiCond.Always);


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
                harvestObject.Draw();

            if (_updateStopwatch.ElapsedMilliseconds > Settings.UpdateDelayMs.Value)
            {
                _updateStopwatch.Restart();
                foreach (var harvestObject in _objects.Values)
                    harvestObject.Update();
            }

            DrawControls();
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
            _mapController.DrawPylons = DrawButton("P", buttonPos, _mapController.DrawPylons);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP));
            _mapController.DrawCollectors = DrawButton("C", buttonPos, _mapController.DrawCollectors);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 2));
            _mapController.DrawDispensers = DrawButton("D", buttonPos, _mapController.DrawDispensers);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 3));
            _mapController.DrawStorage = DrawButton("S", buttonPos, _mapController.DrawStorage);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 4));
            _mapController.DrawLinks = DrawButton("/", buttonPos, _mapController.DrawLinks);

            buttonPos = _mapController.GridPosToMapPos(new Vector2(posX, posY - Constants.GRID_STEP * 5));
            _mapController.DrawSeeds = DrawButton("O", buttonPos, _mapController.DrawSeeds);
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
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/Irrigator")
                _objects[entity.Id] = new HarvestDispenser(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/Pole")
                _objects[entity.Id] = new HarvestPylon(entity, _mapController);
            else if (entity.Path == "Metadata/MiscellaneousObjects/Harvest/HarvestPipeBeamEffect")
                _objects[entity.Id] = new HarvestBeamLink(entity, _mapController);
        }

        public override void EntityRemoved(Entity entity)
        {
            _objects.TryRemove(entity.Id, out _);
        }
    }
}