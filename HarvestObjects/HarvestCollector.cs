using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestCollector : HarvestObject
    {
        public HarvestCollector(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }

        public override void Draw()
        {
            if (!MapController.DrawCollectors)
                return;
            var drawPos = GetScreenDrawPos();
            MapController.DrawFrameOnMap(drawPos, 4.9f, 2, EnergyColor);
            MapController.DrawBoxOnMap(drawPos, 0.9f, EnergyColor);
            MapController.DrawTextOnMap("C", drawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}