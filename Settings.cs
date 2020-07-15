using System.Windows.Forms;
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

        [Menu("Storage Tank Width")]
        public RangeNode<float> StorageBoxWidth { get; set; } = new RangeNode<float>(200f, 0f, 1000f);

        public float PosX { get; set; } = 100;
        public float PosY { get; set; } = 100;
        public float Width { get; set; } = 500;
        public float Height { get; set; } = 500;

        [Menu("Fix Out Of Screen (lost window)")]
        public ButtonNode FixOutOfScreen { get; set; } = new ButtonNode();

        [Menu("Hide toggle")]
        public HotkeyNode Toggle { get; set; } = new HotkeyNode(Keys.Oemtilde);
        public bool IsShown { get; set; }

        public bool DrawDispensers { get; set; }
        public bool DrawPylons { get; set; } = true;
        public bool DrawCollectors { get; set; } = true;
        public bool DrawStorage { get; set; } = true;
        public bool DrawLinks { get; set; } = true;
        public bool DrawSeeds { get; set; } = true;
        public bool DrawHorticrafting { get; set; } = true;
        public bool DrawFlower { get; set; } = true;
    }
}