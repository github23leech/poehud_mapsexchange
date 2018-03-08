using System;
using PoeHUD.Plugins;
using PoeHUD.Hud.Settings;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;

namespace MapsExchange
{
    public class MapsExchangeSettings : SettingsBase
    {
        public MapsExchangeSettings()
        {
            Enable = false;
            BordersWidth = new RangeNode<float>(3, 1, 10);
            Spacing = new RangeNode<float>(1, 0, 10);
            UncompletedMapsColor = new ColorNode(Color.Red);
            ShowPenalty = true;
        }

        [Menu("Borders Width")]
        public RangeNode<float> BordersWidth { get; set; }
        [Menu("Spacing")]
        public RangeNode<float> Spacing { get; set; }
        [Menu("Show Penalty On Hover")]
        public ToggleNode ShowPenalty { get; set; }

        [Menu("Not completed Maps Color")]
        public ColorNode UncompletedMapsColor { get; set; }
    }
}
