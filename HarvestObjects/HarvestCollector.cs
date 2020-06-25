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


        public override string ObjectName { get; } = "Collector";
        public override void Draw()
        {
            if (!MapController.Settings.DrawCollectors)
                return;

            MapController.DrawFrameOnMap(ScreenDrawPos, 4.9f, 2, EnergyColor);
            MapController.DrawBoxOnMap(ScreenDrawPos, 0.9f, EnergyColor);
            MapController.DrawTextOnMap("C", ScreenDrawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}