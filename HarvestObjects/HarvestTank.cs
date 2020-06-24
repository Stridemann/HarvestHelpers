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

        public override void Draw()
        {
            var drawPos = GetScreenDrawPos();
            MapController.DrawBoxOnMap(drawPos, 0.8f, EnergyColor);
            MapController.DrawTextOnMap("S", drawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}