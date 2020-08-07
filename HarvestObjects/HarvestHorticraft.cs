using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestHorticraft: HarvestObject
    {
        public HarvestHorticraft(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }


        public override string ObjectName { get; } = "Horticraft";

        public override void Draw()
        {
            if (!MapController.Settings.DrawHorticrafting)
                return;

            MapController.DrawBoxOnMap(ScreenDrawPos, 0.8f, EnergyColor);
            MapController.DrawTextOnMap("H", ScreenDrawPos, Color.Black, 150, FontAlign.Center);
        }
    }
}