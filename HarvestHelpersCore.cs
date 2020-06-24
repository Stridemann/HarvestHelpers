using System.Collections.Concurrent;
using System.Diagnostics;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects;
using HarvestHelpers.HarvestObjects.Base;
using ImGuiNET;
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

        public override bool Initialise()
        {
            _mapController = new MapController(Graphics, DirectoryFullName);
            _isOnMap = GameController.Area.CurrentArea.Name == "The Sacred Grove";
            return true;
        }


        public override void AreaChange(AreaInstance area)
        {
            _isOnMap = area.Name == "The Sacred Grove";
        }


        public override void Render()
        {
            if (!_isOnMap)
                return;

            ImGui.SetNextWindowPos(new nuVector2(Settings.PosX, Settings.PosY), ImGuiCond.Once, nuVector2.Zero);
            ImGui.SetNextWindowSize(new nuVector2(Settings.Width, Settings.Height), ImGuiCond.Once);
            var opened = true;

            if (ImGui.Begin($"{Name}", ref opened,
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
                Settings.Width = size.X;
                Settings.Height = size.Y;
            }

            ImGui.End();
        }

        private void DrawWindowContent()
        {
            var windowDrawFrame = new RectangleF(Settings.PosX + 5, Settings.PosY + 20, Settings.Width - 10,
                Settings.Height - 25);

            _mapController.Draw(windowDrawFrame);

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