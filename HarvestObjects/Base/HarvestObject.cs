using System.Collections.Generic;
using System.Text;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;

namespace HarvestHelpers.HarvestObjects.Base
{
    public abstract class HarvestObject
    {
        private IList<StateMachineState> _states;
        private string _validateError;

        public HarvestObject(Entity entity, MapController mapController)
        {
            Entity = entity;
            MapController = mapController;
            GridPos = entity.GridPos;
            UpdateEnergyColor();
        }

        public abstract string ObjectName { get; }
        protected MapController MapController { get; }
        public Entity Entity { get; }
        public Vector2 GridPos { get; }
        public Vector2 ScreenDrawPos { get; private set; }
        public Color EnergyColor { get; set; } = Color.Red;
        public long EnergyType { get; set; }
        public bool IsReadyToHatch { get; private set; }
        public bool IsHatched { get; private set; }
        public long FluidAmount { get; private set; }
        public long AvailableFluid { get; private set; }
        public long RequiredFluid { get; private set; }
        public long CurrentState { get; private set; }
        public bool AutoIrrigating { get; private set; }

        public void Update(StringBuilder sb)
        {
            UpdateEnergyColor();
            _validateError = Validate();
            if (!string.IsNullOrEmpty(_validateError))
            {
                sb.AppendLine($"{ObjectName}: {_validateError}");
            }
        }

        public virtual string Validate()
        {
            return string.Empty;
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
            AvailableFluid = 0;
            RequiredFluid = 0;
            CurrentState = 0;
            AutoIrrigating = false;

            foreach (var state in _states)
                switch (state.Name)
                {
                    case "ready_to_hatch":
                        IsReadyToHatch = state.Value == 1;
                        break;
                    case "hatched":
                        IsHatched = state.Value > 1;
                        break;
                    case "colour":
                        EnergyType = state.Value & int.MaxValue;
                        switch (EnergyType)
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

                    case "available_fluid":
                        AvailableFluid = state.Value;
                        break;
                    case "required_fluid":
                        RequiredFluid = state.Value;
                        break;
                    case "current_state":
                        CurrentState = state.Value;
                        break;
                    case "auto_irrigating":
                        AutoIrrigating = state.Value > 0;
                        break;
                }


            var targetable = Entity.GetComponent<Targetable>();
            if (targetable != null && targetable.isTargeted)
                EnergyColor = Constants.Highlighted;
        }


        public virtual void DrawObject()
        {
            ScreenDrawPos = MapController.GridPosToMapPos(GridPos);
            Draw();

            if (!string.IsNullOrEmpty(_validateError))
            {
                MapController.DrawTextOnMap(_validateError, ScreenDrawPos, Color.Red, 15, FontAlign.Center);
            }
        }

        public virtual void Draw()
        {
        }
    }
}