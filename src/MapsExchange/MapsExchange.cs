using System.Collections.Generic;
using System.Linq;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Elements;
using PoeHUD.Models;
using SharpDX;

namespace MapsExchange
{
    public class MapsExchange : BaseSettingsPlugin<MapsExchangeSettings>
    {
        private List<MapItem> MapItems = new List<MapItem>();

        private Color[] SelectColors;

        public override void Initialise()
        {
            SelectColors = new Color[]
            {
                Color.Aqua,
                Color.Blue,
                Color.BlueViolet,
                Color.Brown,
                Color.BurlyWood,
                Color.CadetBlue,
                Color.Chartreuse,
                Color.Chocolate,
                Color.Coral,
                Color.CornflowerBlue,
                Color.Cornsilk,
                Color.Crimson,
                Color.Cyan,
                Color.DarkBlue,
                Color.DarkCyan,
                Color.DarkGoldenrod,
                Color.DarkGray,
                Color.DarkGreen,
                Color.DarkKhaki,
                Color.DarkMagenta,
                Color.DarkOliveGreen,
                Color.DarkOrange,
                Color.DarkOrchid,
                Color.DarkRed,
                Color.DarkSalmon,
                Color.DarkSeaGreen,
                Color.DarkSlateBlue,
                Color.DarkSlateGray,
                Color.DarkTurquoise,
                Color.DarkViolet,
                Color.DeepPink,
                Color.DeepSkyBlue,
                Color.DimGray,
                Color.DodgerBlue,
                Color.Firebrick,
                Color.FloralWhite,
                Color.ForestGreen,
                Color.Fuchsia,
                Color.Gainsboro,
                Color.GhostWhite,
                Color.Gold,
                Color.Goldenrod,
                Color.Gray,
                Color.Green,
                Color.GreenYellow,
                Color.Honeydew,
                Color.HotPink,
                Color.IndianRed,
                Color.Indigo,
                Color.Ivory,
                Color.Khaki,
                Color.Lavender,
                Color.LavenderBlush,
                Color.LawnGreen,
                Color.LemonChiffon,
                Color.LightBlue,
                Color.LightCoral,
                Color.LightCyan,
                Color.LightGoldenrodYellow,
                Color.LightGray,
                Color.LightGreen,
                Color.LightPink,
                Color.LightSalmon,
                Color.LightSeaGreen,
                Color.LightSkyBlue,
                Color.LightSlateGray,
                Color.LightSteelBlue,
                Color.LightYellow,
                Color.Lime,
                Color.LimeGreen,
                Color.Linen,
                Color.Magenta,
                Color.Maroon,
                Color.MediumAquamarine,
                Color.MediumBlue,
                Color.MediumOrchid,
                Color.MediumPurple,
                Color.MediumSeaGreen,
                Color.MediumSlateBlue,
                Color.MediumSpringGreen,
                Color.MediumTurquoise,
                Color.MediumVioletRed,
                Color.MidnightBlue,
                Color.MintCream,
                Color.MistyRose,
                Color.Moccasin,
                Color.NavajoWhite,
                Color.Navy,
                Color.OldLace,
                Color.Olive,
                Color.OliveDrab,
                Color.Orange,
                Color.OrangeRed,
                Color.Orchid,
                Color.PaleGoldenrod,
                Color.PaleGreen,
                Color.PaleTurquoise,
                Color.PaleVioletRed,
                Color.PapayaWhip,
                Color.PeachPuff,
                Color.Peru,
                Color.Pink,
                Color.Plum,
                Color.PowderBlue,
                Color.Purple,
                Color.Red,
                Color.RosyBrown,
                Color.RoyalBlue,
                Color.SaddleBrown,
                Color.Salmon,
                Color.SandyBrown,
                Color.SeaGreen,
                Color.SeaShell,
                Color.Sienna,
                Color.Silver,
                Color.SkyBlue,
                Color.SlateBlue,
                Color.SlateGray,
                Color.Snow,
                Color.SpringGreen,
                Color.SteelBlue,
                Color.Tan,
                Color.Teal,
                Color.Thistle,
                Color.Tomato,
                Color.Transparent,
                Color.Turquoise,
                Color.Violet,
                Color.Wheat,
                Color.White,
                Color.WhiteSmoke,
                Color.Yellow,
                Color.YellowGreen
            };
        }

        private long CurrentStashAddr;
        private Dictionary<string, List<MapItem>> DrawMaps;
        public override void Render()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel;
            if (!stash.IsVisible)
            {
                CurrentStashAddr = -1;
                return;
            }

            var visibleStash = stash.VisibleStash;

            if (visibleStash == null)
                return;

            var items = visibleStash.VisibleInventoryItems;
            if (items == null)
            {
                CurrentStashAddr = -1;
                return;
            }


            if (CurrentStashAddr != visibleStash.Address)
            {
                LogMessage("Update", 3);
                CurrentStashAddr = visibleStash.Address;
                UpdateData(items);
            }

            HiglightMaps();
        }

        private void UpdateData(List<NormalInventoryItem> items)
        {

            MapItems = new List<MapItem>();

            foreach (var invItem in items)
            {
                var item = invItem.Item;
                if (item == null) continue;

                BaseItemType bit = GameController.Files.BaseItemTypes.Translate(item.Path);
                if (bit == null) continue;

                if (bit.ClassName != "Map") continue;

                float width = Settings.BordersWidth;
                float spacing = Settings.Spacing;

                var drawRect = invItem.GetClientRect();
                drawRect.X += width / 2 + spacing;
                drawRect.Y += width / 2 + spacing;
                drawRect.Width -= width + spacing * 2;
                drawRect.Height -= width + spacing * 2;

                MapItems.Add(new MapItem(bit.BaseName, drawRect));
            }


            DrawMaps = (from demoClass in MapItems
                            //where demoClass.Tier >= Settings.MinTier && demoClass.Tier <= Settings.MaxTier
                            //TODO: Check tiers (or nobody need it?)
                        group demoClass by demoClass.Name
                                into groupedDemoClass
                        select groupedDemoClass
                             ).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

        }

        private void HiglightMaps()
        {
            if (DrawMaps == null) return;
            int colorCounter = 0;
            foreach (var group in DrawMaps)
            {
                int count = group.Value.Count;
                int take = count / 3;
                take *= 3;

                var grabMaps = group.Value.Take(take);
                DrawGroup(grabMaps, SelectColors[colorCounter]);
                colorCounter++;
            }
        }

        private void DrawGroup(IEnumerable<MapItem> grabMaps, Color color)
        {
            foreach (var dropMap in grabMaps)
            {
                Graphics.DrawFrame(dropMap.DrawRect, Settings.BordersWidth, color);
            }
        }

        public class MapItem
        {
            public MapItem(string Name, RectangleF DrawRect)
            {
                this.Name = Name;
                this.DrawRect = DrawRect;
            }
            public string Name;
            public RectangleF DrawRect;
        }
    }
}
