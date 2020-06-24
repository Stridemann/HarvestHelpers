using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestBeamLink : HarvestObject
    {
        public Vector2 BeamStart { get; }
        public Vector2 EndStart { get; }

        public HarvestBeamLink(Entity entity, MapController mapController) : base(entity, mapController)
        {
            var beam = entity.GetComponent<Beam>();
            BeamStart = beam.BeamStart.WorldToGrid();
            EndStart = beam.BeamEnd.WorldToGrid();
        }

        public override void Draw()
        {
            var pos1 = MapController.GridPosToMapPos(BeamStart);
            var pos2 = MapController.GridPosToMapPos(EndStart);
            MapController.DrawLine(pos1, pos2, 1, EnergyColor);
        }
    }
}