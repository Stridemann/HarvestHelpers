using System.Diagnostics.Contracts;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using SharpDX;

namespace HarvestHelpers.HarvestObjects.Base
{
    public class HarvestObject
    {
        public HarvestObject(Entity entity, MapController mapController)
        {
            Entity = entity;
            MapController = mapController;
            GridPos = entity.GridPos;
            Update();
        }

        protected MapController MapController { get; }
        public Entity Entity { get; }
        public Vector2 GridPos { get; }
        public Color EnergyColor { get; set; } = Color.Red;

        public void Update()
        {
            UpdateEnergyColor();
        }

        private void UpdateEnergyColor()
        {
            if (!Entity.IsValid)
                return;

            var stateMachine = Entity.GetComponent<StateMachine>();

            if (stateMachine == null)
                return;

            EnergyColor = Constants.OutOfRange;
            switch (stateMachine.EnergyType)
            {
                case 0:
                    EnergyColor = Constants.Neutral;
                    break;
                case 1:
                    EnergyColor = Constants.Purple;
                    break;
                case 2:
                    EnergyColor = Constants.Yellow;
                    break;
                case 3:
                    EnergyColor = Constants.Blue;
                    break;
            }
        }

        protected Vector2 GetScreenDrawPos()
        {
            return MapController.GridPosToMapPos(GridPos);
        }

        public virtual void Draw()
        {
        }
    }
}