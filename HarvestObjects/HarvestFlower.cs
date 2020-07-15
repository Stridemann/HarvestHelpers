using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestFlower : HarvestObject
    {
        public HarvestFlower(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }


        public override string ObjectName { get; } = "Flower";

        public override void Draw()
        {
            if (!MapController.Settings.DrawFlower)
                return;

            MapController.DrawBoxOnMap(ScreenDrawPos, 0.8f, new Color(45,45,45,255));
            MapController.DrawTextOnMap("F", ScreenDrawPos, Color.LimeGreen, 150, FontAlign.Center);
        }
    }
}