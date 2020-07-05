using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
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

        private readonly Vector2
            _defaultGroveCenter =
                new Vector2(379,
                    402); //Default pos of Metadata/Terrain/Leagues/Harvest/Objects/SoulTree on The Sacred Grove

        private Vector2 _groveCenter; //Current pos of Metadata/Terrain/Leagues/Harvest/Objects/SoulTree

        private MapController _mapController;
        private readonly Stopwatch _updateStopwatch = Stopwatch.StartNew();
        private readonly StringBuilder _errorStringBuilder = new StringBuilder();
        private readonly long[] _availableFluid = new long[3];
        private readonly long[] _requiredFluid = new long[3];
        private readonly long[] _fluidAmount = new long[3];
        private readonly long[] _fluidCapacity = new long[3];
        private readonly Color[] _fluidColors;

        private readonly List<Tuple<string, string, Color>> _craftListHighlight;

        public HarvestHelpersCore()
        {
            _fluidColors = new[]
            {
                Constants.Purple,
                Constants.Yellow,
                Constants.Blue,
            };

            _craftListHighlight = new List<Tuple<string, string, Color>>
            {
                new Tuple<string, string, Color>("<white>{Change} a <white>{Unique}", "Roll unique", Color.Orange),
                new Tuple<string, string, Color>("<white>{Randomise} the numeric values", "Randomize values", Color.White),
            };
        }

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
            DrawInventSeeds();
            var isOnMap = Vector2.Distance(GameController.Player.GridPos, _groveCenter) < 400;
            if (!isOnMap)
            {
                return;
            }

            if (Settings.Toggle.PressedOnce())
                Settings.IsShown = !Settings.IsShown;

            UpdateAndValidate();
            CheckCraftWindow();

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

        private void CheckCraftWindow()
        {
            var craftWindow = GameController.Game.IngameState.IngameUi.ReadObjectAt<Element>(0x790);
            if (!craftWindow.IsVisible)
                return;

            var craftWindowRect = craftWindow.GetClientRect();
            var drawTextPos = craftWindowRect.TopRight;

            var craftList = craftWindow.ReadObjectAt<Element>(0x2A8);
            //var dumpSb = new StringBuilder();
            var drawFrame = false;
            foreach (var craftListChild in craftList.Children)
            {
                if (craftListChild.ChildCount < 4)
                    continue;

                var craftText = craftListChild[3].Text;
                //dumpSb.AppendLine(craftText);

                foreach (var tuple in _craftListHighlight)
                {
                    if (craftText.StartsWith(tuple.Item1))
                    {
                        drawFrame = true;
                        Graphics.DrawText(tuple.Item2, drawTextPos, tuple.Item3, 20);
                        drawTextPos.Y += 20;
                    }
                }

                if (!craftText.StartsWith("<white>{Reforge}") && !craftText.StartsWith("<white>{Remove}") &&
                    !craftText.StartsWith("<white>{Randomise}"))
                {
                    drawFrame = true;
                }
            }

            if (drawFrame)
            {
                Graphics.DrawFrame(craftWindowRect, Color.Yellow, 1);
            }

            //File.WriteAllText(Path.Combine(DirectoryFullName, "craftDump.txt"), dumpSb.ToString());
        }

        private readonly Stopwatch _inventSeedsDelayStopwatch = Stopwatch.StartNew();
        private readonly int[] _inventSeeds = new int[3 * 4];

        private void DrawInventSeeds()
        {
            var skillRect = GameController.Game.IngameState.IngameUi.SkillBar.GetClientRect();
            var drawRect = skillRect;
            drawRect.Width = 50;
            drawRect.Height = 20;
            drawRect.Y -= 40;
            drawRect.X += 100;

            for (var i = 0; i < 3; i++)
            {
                Graphics.DrawBox(drawRect, _fluidColors[i]);
                Graphics.DrawText(_inventSeeds[i].ToString(), drawRect.Center.Translate(0, -6), Color.Black,
                    FontAlign.Center);
                drawRect.X += drawRect.Width;
            }

            if (_inventSeedsDelayStopwatch.ElapsedMilliseconds < 2000)
                return;
            _inventSeedsDelayStopwatch.Restart();

            var items =
                GameController.Game.IngameState.ServerData.GetPlayerInventoryBySlot(InventorySlotE.MainInventory1);

            _inventSeeds[0] = 0;
            _inventSeeds[1] = 0;
            _inventSeeds[2] = 0; //just for T1

            foreach (var item in items.Items)
            {
                var metadata = item.Metadata;
                if (!metadata.StartsWith("Metadata/Items/Harvest/HarvestSeed"))
                    continue;

                var endsIndex = metadata.LastIndexOf('T');
                if (endsIndex == -1)
                    continue;

                var tier = int.Parse(metadata[endsIndex + 1].ToString());
                if (tier < 1 || tier > 4)
                    continue;

                if (tier != 1) //remove this if you gonna add T2 and T3 etc
                    continue;

                var colorStr = metadata.Substring(endsIndex + 2);
                var color = -1;
                if (colorStr == "Red")
                    color = 0;
                else if (colorStr == "Green")
                    color = 1;
                else if (colorStr == "Blue")
                    color = 2;

                if (color == -1)
                    continue;

                var stack = item.GetComponent<Stack>();
                if (stack == null)
                    continue;

                _inventSeeds[color] += stack.Size;
            }
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

                _availableFluid[0] = 0;
                _availableFluid[1] = 0;
                _availableFluid[2] = 0;
                _requiredFluid[0] = 0;
                _requiredFluid[1] = 0;
                _requiredFluid[2] = 0;
                _fluidAmount[0] = 0;
                _fluidAmount[1] = 0;
                _fluidAmount[2] = 0;
                _fluidCapacity[0] = 0;
                _fluidCapacity[1] = 0;
                _fluidCapacity[2] = 0;

                foreach (var harvestObject in _objects.Values)
                {
                    harvestObject.Update(_errorStringBuilder);

                    var energyType = harvestObject.EnergyType - 1;
                    if (energyType >= 0 && energyType < 3)
                    {
                        _availableFluid[energyType] += harvestObject.AvailableFluid;
                        _requiredFluid[energyType] += harvestObject.RequiredFluid;
                        _fluidAmount[energyType] += harvestObject.FluidAmount;
                        _fluidCapacity[energyType] += harvestObject.FluidCapacity;
                    }
                }
            }

            if (_errorStringBuilder.Length > 0)
                Graphics.DrawText(_errorStringBuilder.ToString(),
                    new nuVector2(10, 200),
                    Color.Red);

            var windowRectangle = GameController.Window.GetWindowRectangle();
            var drawPos = new Vector2(windowRectangle.X + windowRectangle.Width / 2, windowRectangle.Height - 50);
            var barWidth = Settings.StorageBoxWidth;

            for (var i = 0; i < 3; i++)
            {
                var result = _availableFluid[i] - _requiredFluid[i];
                var rect = new RectangleF(drawPos.X + (i - 1) * barWidth - barWidth / 2, drawPos.Y, barWidth - 2, 20);
                var isFine = result > 0;

                Graphics.DrawBox(rect, Color.DimGray);

                // Nice to know what color it is if no progress bar
                Graphics.DrawFrame(rect, _fluidColors[i], 1);

                var progressDelta = (float) _fluidAmount[i] / _fluidCapacity[i];
                var progressRect = rect;
                progressRect.Width = rect.Width * progressDelta;
                Graphics.DrawBox(progressRect, _fluidColors[i]);

                //Graphics.DrawFrame(rect, isFine ? Color.Green : Color.Red, 2);

                var testPos = new nuVector2(rect.X + 5, rect.Y - 15);
                var textSize = Graphics.DrawText($"Available: {_availableFluid[i]} Required:{_requiredFluid[i]}",
                    testPos, isFine ? Color.White : Color.Red);
                Graphics.DrawBox(new RectangleF(testPos.X, testPos.Y, textSize.X, textSize.Y), Color.Black);

                var center = rect.Center;
                testPos = new nuVector2(center.X, center.Y - 7);
                textSize = Graphics.DrawText($"Fill: {progressDelta:P1} ({_fluidAmount[i]} of {_fluidCapacity[i]})",
                    testPos, isFine ? Color.White : Color.Red, 15, FontAlign.Center);
                Graphics.DrawBox(new RectangleF(testPos.X - textSize.X / 2 - 5, testPos.Y, textSize.X + 10, textSize.Y),
                    Color.Black);
            }
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

            var buttonPos = new Vector2(Settings.PosX + 30, Settings.PosY + 30);
            _mapController.DrawTextOnMap("Hide layer", buttonPos, Color.White, 15, FontAlign.Center);

            buttonPos.Y += 20;
            Settings.DrawPylons = DrawButton("P", buttonPos, Settings.DrawPylons);

            buttonPos.Y += 20;
            Settings.DrawCollectors = DrawButton("C", buttonPos, Settings.DrawCollectors);

            buttonPos.Y += 20;
            Settings.DrawDispensers = DrawButton("D", buttonPos, Settings.DrawDispensers);

            buttonPos.Y += 20;
            Settings.DrawStorage = DrawButton("S", buttonPos, Settings.DrawStorage);

            buttonPos.Y += 20;
            Settings.DrawLinks = DrawButton("/", buttonPos, Settings.DrawLinks);

            buttonPos.Y += 20;
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