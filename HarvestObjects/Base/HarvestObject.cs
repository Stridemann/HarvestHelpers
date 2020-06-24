using System.Collections.Generic;
using System.Diagnostics.Contracts;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using SharpDX;

namespace HarvestHelpers.HarvestObjects.Base
{
    public class HarvestObject
    {
        private IList<StateMachineState> _states;

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
        public bool IsReadyToHatch { get; private set; }
        public bool IsHatched { get; private set; }
        public long FluidAmount { get; private set; }

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

            _states = stateMachine.ReadStates();

            EnergyColor = Constants.OutOfRange;
            IsReadyToHatch = false;
            IsHatched = false;


            foreach (var state in _states)
            {
                switch (state.Name)
                {
                    case "ready_to_hatch":
                        IsReadyToHatch = state.Value == 1;
                        break;
                    case "hatched":
                        IsHatched = state.Value > 1;
                        break;
                    case "colour":
                        switch (state.Value & int.MaxValue)
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
                            default:
                                EnergyColor = Constants.OutOfRange;
                                break;
                        }

                        break;
                    case "fluid_amount":
                        FluidAmount = state.Value;
                        break;
                }
            }


            var targetable = Entity.GetComponent<Targetable>();
            if (targetable != null && targetable.isTargeted)
                EnergyColor = Constants.Highlighted;
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