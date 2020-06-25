using ExileCore.PoEMemory.MemoryObjects;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestSeed : HarvestObject
    {
        public HarvestSeed(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }
        public override string ObjectName { get; } = "Seed";

        public override void Draw()
        {
            if (!MapController.Settings.DrawSeeds)
                return;

            var color = Color.White;
            if (IsHatched)
                color = Color.Gray;
            else if (IsReadyToHatch)
                color = Color.LightGreen;

            MapController.DrawBoxOnMap(ScreenDrawPos, 0.5f, color);
        }
    }
}