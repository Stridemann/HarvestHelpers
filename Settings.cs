using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace HarvestHelpers
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Update interval (milliseconds)")]
        public RangeNode<int> UpdateDelayMs { get; set; } = new RangeNode<int>(1000, 0, 10000);

        public float PosX { get; set; } = 100;
        public float PosY { get; set; } = 100;
        public float Width { get; set; } = 500;
        public float Height { get; set; } = 500;
    }
}