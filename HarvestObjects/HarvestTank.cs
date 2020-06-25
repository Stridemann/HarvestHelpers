using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestTank : HarvestObject
    {
        public HarvestTank(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }


        public override string ObjectName { get; } = "Tank";

        public override void Draw()
        {
            if (!MapController.Settings.DrawStorage)
                return;

            MapController.DrawBoxOnMap(ScreenDrawPos, 0.8f, EnergyColor);
            MapController.DrawTextOnMap("S", ScreenDrawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}