using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestPylon : HarvestObject
    {
        public override string ObjectName { get; } = "Pylon";
        public HarvestPylon(Entity entity, MapController mapController) : base(entity, mapController)
        {
            entity.GetComponent<Beam>();
        }

        public override void Draw()
        {
            if (!MapController.Settings.DrawPylons)
                return;

            MapController.DrawBoxOnMap(ScreenDrawPos, 0.3f, EnergyColor);
        }
    }
}