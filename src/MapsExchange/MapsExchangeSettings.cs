﻿using System;
using PoeHUD.Plugins;
using PoeHUD.Hud.Settings;

namespace MapsExchange
{
    public class MapsExchangeSettings : SettingsBase
    {
        public MapsExchangeSettings()
        {
            Enable = false;
            BordersWidth = new RangeNode<float>(3, 1, 10);
            Spacing = new RangeNode<float>(1, 0, 10);
            ShowPenalty = true;
        }

        [Menu("Borders Width")]
        public RangeNode<float> BordersWidth { get; set; }
        [Menu("Spacing")]
        public RangeNode<float> Spacing { get; set; }
        [Menu("Show Penalty On Hover")]
        public ToggleNode ShowPenalty { get; set; }
    }
}
