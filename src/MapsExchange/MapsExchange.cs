using System.Collections.Generic;
using System.Linq;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Elements;
using PoeHUD.Models;
using SharpDX;
using PoeHUD.Poe.Components;
using System;
using SharpDX.Direct3D9;
using PoeHUD.Framework;
using PoeHUD.Poe.RemoteMemoryObjects;

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

        private Dictionary<string, int> CachedDropLvl = new Dictionary<string, int>();

        public override void Render()
        {
            DrawPlayerInvMaps();

            var ingameState = GameController.Game.IngameState;
            var stash = ingameState.ServerData.StashPanel;
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


            HiglightAllMaps(items);

            if (CurrentStashAddr != visibleStash.Address)
            {
                CurrentStashAddr = visibleStash.Address;
                UpdateData(items);
            }

            HiglightExchangeMaps();

            /* doesn't work
            if(ingameState.UIHover != null)
            {
                var inventElement = ingameState.UIHover.AsObject<NormalInventoryItem>();
                if(inventElement.Item != null)
                LogMessage(inventElement.Item.Path, 0);
                HiglightAllMaps(new List<NormalInventoryItem>() { inventElement });
            }
            */
        }

        private void DrawPlayerInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                List<NormalInventoryItem> playerInvItems = new List<NormalInventoryItem>();
                var inventoryZone = ingameState.IngameUi.InventoryPanel[PoeHUD.Models.Enums.InventoryIndex.PlayerInventory].InventoryUiElement;

                foreach (Element element in inventoryZone.Children)
                {
                    var inventElement = element.AsObject<NormalInventoryItem>();

                    if (inventElement.InventPosX < 0 || inventElement.InventPosY < 0)
                    {
                        continue;
                    }
                    playerInvItems.Add(inventElement);
                }

                HiglightAllMaps(playerInvItems);
            }
        }

        private void UpdateData(List<NormalInventoryItem> items)
        {
            MapItems = new List<MapItem>();
            var passed = GameController.Game.IngameState.ServerData.BonusCompletedAreas;


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


                var mapItem = new MapItem(bit.BaseName, drawRect);
                var mapComponent = item.GetComponent<PoeHUD.Poe.Components.Map>();
                mapItem.Completed = passed.Contains(mapComponent.Area);
                mapItem.Penalty = LevelXpPenalty(bit.DropLevel);
                MapItems.Add(mapItem);
            }


            var sortedMaps = (from demoClass in MapItems
                            //where demoClass.Tier >= Settings.MinTier && demoClass.Tier <= Settings.MaxTier
                            //TODO: Check tiers (or nobody need it?)
                        group demoClass by demoClass.Name
                                into groupedDemoClass
                        select groupedDemoClass
                             ).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());



            int colorCounter = 0;
            foreach (var group in sortedMaps)
            {
                int count = group.Value.Count;
                int take = count / 3;
                take *= 3;

                var grabMaps = group.Value.Take(take);

                foreach (var dropMap in grabMaps)
                {
                    dropMap.DrawColor = SelectColors[colorCounter];
                }

                colorCounter++;
            }
        }

        private void HiglightExchangeMaps()
        {
            var color = Color.Green;
            color.A = 255;

            foreach (var drapMap in MapItems)
            {
                Graphics.DrawFrame(drapMap.DrawRect, Settings.BordersWidth, drapMap.DrawColor);

                if (!drapMap.Completed)
                {
                  
                    Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/circle.png"), drapMap.DrawRect, color);

                    /*

                    
                    Graphics.DrawLine(drapMap.DrawRect.TopLeft, drapMap.DrawRect.BottomRight, 1, color);
                    Graphics.DrawLine(drapMap.DrawRect.TopRight, drapMap.DrawRect.BottomLeft, 1, color);
                    */
                }
            }
        }



        private void HiglightAllMaps(List<NormalInventoryItem> items)
        {
            foreach (var item in items)
            {
                var entity = item.Item;
                if (item == null) continue;

                BaseItemType bit = GameController.Files.BaseItemTypes.Translate(entity.Path);
                if (bit == null) continue;
                if (bit.ClassName != "Map") continue;

                var drawRect = item.GetClientRect();
                //Graphics.DrawFrame(drawRect, 1, Color.Gray);
            
                

                if (Settings.ShowPenalty.Value)
                {
                    var penalty = LevelXpPenalty(bit.DropLevel);
                    var textColor = Color.Lerp(Color.Red, Color.Green, (float)penalty);
                 
                    textColor.A = (byte)(255f * (1 - penalty) * 1.5f);
                    Graphics.DrawText($"{penalty:p0}", 20, drawRect.BottomLeft, textColor, FontDrawFlags.Left | FontDrawFlags.Bottom);
                }
            }
        }




        private double LevelXpPenalty(int arenaLevel)
        {
            int characterLevel = GameController.Player.GetComponent<Player>().Level;
            double safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            double effectiveDifference = Math.Max(Math.Abs(characterLevel - arenaLevel) - safeZone, 0);
            double xpMultiplier = Math.Max(Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5), 0.01);
            return xpMultiplier;
        }


        public class MapItem
        {
            public MapItem(string Name, RectangleF DrawRect)
            {
                this.Name = Name;
                this.DrawRect = DrawRect;
            }
            public string Name;
            public double Penalty;
            public RectangleF DrawRect;
            public Color DrawColor = Color.Transparent;
            public bool Completed;
        }
    }
}
