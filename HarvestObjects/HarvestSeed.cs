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

        public override void Draw()
        {
            var drawPos = GetScreenDrawPos();
            MapController.DrawBoxOnMap(drawPos, 0.5f, Color.White);
        }
    }
}