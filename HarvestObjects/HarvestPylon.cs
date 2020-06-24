using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestPylon : HarvestObject
    {
        public HarvestPylon(Entity entity, MapController mapController) : base(entity, mapController)
        {
            entity.GetComponent<Beam>();
        }

        public override void Draw()
        {
            var drawPos = GetScreenDrawPos();
            MapController.DrawBoxOnMap(drawPos, 0.3f, EnergyColor);
        }
    }
}