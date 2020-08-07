using System;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HarvestHelpers.HarvestObjects.Base;
using SharpDX;

namespace HarvestHelpers.HarvestObjects
{
    public class HarvestDispenser : HarvestObject
    {
        public HarvestDispenser(Entity entity, MapController mapController) : base(entity, mapController)
        {
        }

        public override void Draw()
        {
            if (!MapController.Settings.DrawDispensers)
                return;

            //MapController.DrawFrameOnMap(ScreenDrawPos, 4.9f, 2, EnergyColor);
            MapController.DrawBoxOnMap(ScreenDrawPos, 0.8f, EnergyColor);
            MapController.DrawTextOnMap("D", ScreenDrawPos, Color.Black, 15, FontAlign.Center);
        }

        public override string ObjectName { get; } = "Dispenser";

        public override string Validate()
        {
            var error = string.Empty;

            //if (CurrentState == 0 && RequiredFluid != 0 && RequiredFluid < AvailableFluid)
            //{
            //    error = "Turned off";
            //}

            return error;
        }
    }
}