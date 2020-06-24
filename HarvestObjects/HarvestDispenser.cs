using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestDispenser : HarvestObject
    {
        public HarvestDispenser(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }

        public override void Draw()
        {
            var drawPos = GetScreenDrawPos();
            MapController.DrawFrameOnMap(drawPos, 4.9f, 2, EnergyColor);
            MapController.DrawBoxOnMap(drawPos, 0.8f, EnergyColor);
            MapController.DrawTextOnMap("D", drawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}