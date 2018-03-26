using System.Collections.Generic;
using System.Linq;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Elements;
using PoeHUD.Models;
using SharpDX;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.EntityComponents;
using System;
using SharpDX.Direct3D9;
using PoeHUD.Framework;
using PoeHUD.Poe.RemoteMemoryObjects;
using System.Threading;
using System.Windows.Forms;

namespace MapsExchange
{
    public class MapsExchange : BaseSettingsPlugin<MapsExchangeSettings>
    {
        private PoeTradeProcessor TradeProcessor = new PoeTradeProcessor();
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
            API.SubscribePluginEvent("StashUpdate", ExternalUpdateStashes);

            PoeHUD.Hud.Menu.MenuPlugin.KeyboardMouseEvents.MouseDownExt += KeyboardMouseEvents_MouseDownExt;
        }


        private bool MouseClick;
        private void KeyboardMouseEvents_MouseDownExt(object sender, Gma.System.MouseKeyHook.MouseEventExtArgs e)
        {
            MouseClick = true;
        }

        private void ExternalUpdateStashes(object[] args)
        {
            if (!Settings.Enable.Value) return;
            Thread.Sleep(70);
            CurrentStashAddr = -1;
        }

        public override void OnPluginDestroyForHotReload()
        {
            PoeHUD.Hud.Menu.MenuPlugin.KeyboardMouseEvents.MouseDownExt -= KeyboardMouseEvents_MouseDownExt;
            API.UnsubscribePluginEvent("StashUpdate");
        }

        private long CurrentStashAddr;

        private Dictionary<string, int> CachedDropLvl = new Dictionary<string, int>();
        
        public override void Render()
        {
            DrawPlayerInvMaps();
            DrawAtlasMaps();

            var ingameState = GameController.Game.IngameState;
            var stash = ingameState.ServerData.StashPanel;
            if (stash.IsVisible)
            {
                var visibleStash = stash.VisibleStash;

                if (visibleStash != null)
                {
                    var items = visibleStash.VisibleInventoryItems;
                    if (items != null)
                    {
                        HiglightExchangeMaps();
                        HiglightAllMaps(items);

                        if (CurrentStashAddr != visibleStash.Address)
                        {
                            CurrentStashAddr = visibleStash.Address;
                            bool updateMapsCount = Settings.MapTabNode.VisibleIndex == stash.IndexVisibleStash;
                            UpdateData(items, updateMapsCount);
                        }
                    }
                    else
                    {
                        CurrentStashAddr = -1;
                    }
                }
            }
            else
            {
                CurrentStashAddr = -1;
            }

            MouseClick = false;
        }

        private bool LastVisible;
        private List<WorldArea> CompletedMaps;
        private List<WorldArea> BonusCompletedMaps;
        private List<WorldArea> ShapeUpgradedMaps;
        private void DrawAtlasMaps()
        {
            if (!Settings.ShowOnAtlas.Value) return;

            var atlas = GameController.Game.IngameState.IngameUi.AtlasPanel;
            if(LastVisible != atlas.IsVisible || CompletedMaps == null)
            {
                LastVisible = atlas.IsVisible;
                if (LastVisible)
                {
                    CompletedMaps = GameController.Game.IngameState.ServerData.CompletedAreas;
                    BonusCompletedMaps = GameController.Game.IngameState.ServerData.BonusCompletedAreas;
                    ShapeUpgradedMaps = GameController.Game.IngameState.ServerData.ShapedMaps;
                    ScanPlayerInventForShapersOrb();
                }
            }

            if (!atlas.IsVisible) return;

            var root = atlas.GetChildAtIndex(0);
            var rootPos = new Vector2(root.X, root.Y);
            var scale = root.Scale;

            foreach (var atlasMap in GameController.Files.AtlasNodes.EntriesList)
            {
                var area = atlasMap.Area;
                var mapName = area.Name;
                if (mapName.Contains("Realm")) continue;

                var centerPos = (atlasMap.Pos * 5.69f + rootPos) * scale;


                var textPos = centerPos;
                textPos.Y -= 30 * scale;
                var testSize = (int)Math.Round(Settings.TextSize.Value * scale);
                var fontFlags = FontDrawFlags.Center | FontDrawFlags.Bottom;


                byte textTransp;
                Color textBgColor;
                bool fill;
                Color fillColor;
                if (BonusCompletedMaps.Contains(area))
                {
                    textTransp = Settings.BonusCompletedTextTransparency.Value;
                    textBgColor = Settings.BonusCompletedTextBg.Value;
                    fill = Settings.BonusCompletedFilledCircle.Value;
                    fillColor = Settings.BonusCompletedFillColor.Value;
                }
                else if (CompletedMaps.Contains(area))
                {
                    textTransp = Settings.CompletedTextTransparency.Value;
                    textBgColor = Settings.CompletedTextBg.Value;
                    fill = Settings.CompletedFilledCircle.Value;
                    fillColor = Settings.CompletedFillColor.Value;
                }
                else
                {
                    textTransp = Settings.UnCompletedTextTransparency.Value;
                    textBgColor = Settings.UnCompletedTextBg.Value;
                    fill = Settings.UnCompletedFilledCircle.Value;
                    fillColor = Settings.UnCompletedFillColor.Value;
                }

                Color textColor = Settings.WhiteMapColor.Value;

                if (area.AreaLevel >= 78)
                    textColor = Settings.RedMapColor.Value;
                else if (area.AreaLevel >= 73)
                    textColor = Settings.YellowMapColor.Value;

                textColor.A = textTransp;

                Graphics.DrawText(mapName, testSize, textPos, textColor, fontFlags);

                var mapNameSize = Graphics.MeasureText(mapName, testSize, fontFlags);
                mapNameSize.Width += 5;
                var nameBoxRect = new RectangleF(textPos.X - mapNameSize.Width / 2, textPos.Y - mapNameSize.Height, mapNameSize.Width, mapNameSize.Height);
                Graphics.DrawBox(nameBoxRect, textBgColor);

                if (WinApi.IsKeyDown(Keys.ControlKey))
                {
                    var upgraded = ShapeUpgradedMaps.Contains(area);
                    var areaLvlColor = Color.White;
                    var areaLvl = area.AreaLevel;

                    if (upgraded)
                    {
                        areaLvl += 5;
                        areaLvlColor = Color.Orange;
                    }

                    var penalty = LevelXpPenalty(areaLvl);
                    var penaltyTextColor = Color.Lerp(Color.Red, Color.Green, (float)penalty);
                    var labelText = $"{penalty:p0}";
                    var textSize = Graphics.MeasureText(labelText, testSize, FontDrawFlags.Left | FontDrawFlags.Bottom);
                    textSize.Width += 6;
                    var penaltyRect = new RectangleF(textPos.X + mapNameSize.Width / 2, textPos.Y - textSize.Height, textSize.Width, textSize.Height);
                    Graphics.DrawBox(penaltyRect, Color.Black);
                    Graphics.DrawText(labelText, testSize, penaltyRect.Center, penaltyTextColor, FontDrawFlags.Center | FontDrawFlags.VerticalCenter);

                    labelText = $"{areaLvl}";
                    textSize = Graphics.MeasureText(labelText, testSize, FontDrawFlags.Right | FontDrawFlags.Bottom);
                    penaltyRect = new RectangleF(textPos.X - mapNameSize.Width / 2 - textSize.Width, textPos.Y - textSize.Height, textSize.Width, textSize.Height);
                    Graphics.DrawBox(penaltyRect, Color.Black);
                    Graphics.DrawText(labelText, testSize, penaltyRect.Center, areaLvlColor, FontDrawFlags.Center | FontDrawFlags.VerticalCenter);


                    if(Settings.ShowBuyButton.Value)
                    {
                        var butTextWidth = 50 * scale;
                        var buyButtonRect = new RectangleF(textPos.X - butTextWidth / 2, textPos.Y - testSize * 2, butTextWidth, testSize);
                        //Graphics.DrawBox(buyButtonRect, Color.Black);
                        Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/buy.png"), buyButtonRect, new Color(255, 255, 255, 200));
                        //Graphics.DrawText("Buy$", testSize, buyButtonRect.Center, Color.Yellow, FontDrawFlags.Center | FontDrawFlags.VerticalCenter);

                        if (MouseClick)
                        {
                            var prevMousePos = Mouse.GetCursorPosition();
                            var windowOffset = GameController.Window.GetWindowRectangle().TopLeft;

                            if (buyButtonRect.Contains(prevMousePos - windowOffset))
                            {
                                string uniqTest = Memory.ReadStringU(Memory.ReadLong(atlasMap.Address + 0x3c, 0));
                                TradeProcessor.OpenBuyMap(mapName, !string.IsNullOrEmpty(uniqTest) && uniqTest.Contains("Uniq"));
                            }
                        }
                    }
                }

                var imgRectSize = 60 * scale;
                var imgDrawRect = new RectangleF(centerPos.X - imgRectSize / 2, centerPos.Y - imgRectSize / 2, imgRectSize, imgRectSize);

                if (InventShapersOrbs.Count > 0)
                {
                    var areaLvl = area.AreaLevel;

                    if (!ShapeUpgradedMaps.Contains(area))
                    {
                        var tier = areaLvl - 67;

                        if (InventShapersOrbs.Contains(tier))
                        {
                            var shapedRect = imgDrawRect;
                            var sizeOffset = 30 * scale;
                            shapedRect.Left -= sizeOffset;
                            shapedRect.Right += sizeOffset;
                            shapedRect.Top -= sizeOffset;
                            shapedRect.Bottom += sizeOffset;
                            Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/AtlasMapCircle.png"), shapedRect, new Color(155, 0, 255, 255));
                        }
                    }
                }

                if (fill)
                    Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/AtlasMapCircleFilled.png"), imgDrawRect, fillColor);

                Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/AtlasMapCircle.png"), imgDrawRect, Color.Black);


     

                if(Settings.ShowAmount.Value)
                {
                    if (Settings.MapStashAmount.ContainsKey(mapName))
                    {
                        var amount = Settings.MapStashAmount[mapName];
                        var mapCountSize = Graphics.MeasureText(amount.ToString(), testSize, fontFlags);
                        mapCountSize.Width += 6;
                        Graphics.DrawBox(new RectangleF(centerPos.X - mapCountSize.Width / 2, centerPos.Y - mapCountSize.Height / 2, mapCountSize.Width, mapCountSize.Height), Color.Black);
                        textColor.A = 255;
                        Graphics.DrawText(amount.ToString(), testSize, centerPos, textColor, FontDrawFlags.Center | FontDrawFlags.VerticalCenter);
                    }
                }
            }
        }

        private List<int> InventShapersOrbs = new List<int>();
        private void ScanPlayerInventForShapersOrb()
        {
            InventShapersOrbs.Clear();
            var playerInvent = GameController.Game.IngameState.ServerData.GetPlayerInventoryByType(InventoryTypeE.Main);

            foreach(var item in playerInvent.Items)
            {
                var path = item.Path;
                if (string.IsNullOrEmpty(path) || !path.Contains("/MapUpgrades/")) continue;
                if (!item.HasComponent<Base>()) continue;

                var baseName = item.GetComponent<Base>().Name;
                if (string.IsNullOrEmpty(baseName) || !baseName.Contains("Shaper's Orb")) continue;

                var tierStr = baseName.Replace("Shaper's Orb (Tier ", string.Empty).Replace(")", string.Empty);
                int tierValue;
                if (int.TryParse(tierStr, out tierValue))
                    InventShapersOrbs.Add(tierValue);
            }
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

        private void UpdateData(List<NormalInventoryItem> items, bool checkAmount)
        {
            MapItems = new List<MapItem>();

            if (checkAmount)
                Settings.MapStashAmount.Clear();

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

                string baseName = bit.BaseName;

                var mapItem = new MapItem(baseName, drawRect);
                var mapComponent = item.GetComponent<PoeHUD.Poe.Components.Map>();

                if (checkAmount)
                {
                    var areaName = mapComponent.Area.Name;
                    if (!Settings.MapStashAmount.ContainsKey(areaName))
                        Settings.MapStashAmount.Add(areaName, 0);
                    Settings.MapStashAmount[areaName]++;
                }

                mapItem.Penalty = LevelXpPenalty(mapComponent.Area.AreaLevel);
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
            foreach (var drapMap in MapItems)
            {
                Graphics.DrawFrame(drapMap.DrawRect, Settings.BordersWidth, drapMap.DrawColor);
            }
        }




        private void HiglightAllMaps(List<NormalInventoryItem> items)
        {
            IngameState ingameState = GameController.Game.IngameState;
            ServerData serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shEld = serverData.ShaperElderAreas;
            var upgradedMaps = serverData.ShapedMaps;

            bool disableOnHover = false;
            var hoverRect = ingameState.UIHover;
            RectangleF disableOnHoverRect = new RectangleF();

            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();
            if (inventoryItemIcon != null)
            {
                Element tooltip = inventoryItemIcon.Tooltip;
                if(tooltip != null)
                {
                    disableOnHover = true;
                    disableOnHoverRect = tooltip.GetClientRect();
                }
            }


            foreach (var item in items)
            {
                if (item == null) continue;
                var entity = item.Item;
                if (entity == null) continue;

                BaseItemType bit = GameController.Files.BaseItemTypes.Translate(entity.Path);
                if (bit == null) continue;
                if (bit.ClassName != "Map") continue;
                var mapComponent = entity.GetComponent<PoeHUD.Poe.Components.Map>();

                var drawRect = item.GetClientRect();

                if (disableOnHover && disableOnHoverRect.Intersects(drawRect))
                    continue;

                var offset = 3;
                drawRect.Top += offset;
                drawRect.Bottom -= offset;
                drawRect.Right -= offset;
                drawRect.Left += offset;

                int Completed = 0;

                if (comp.Contains(mapComponent.Area))
                    Completed++;
                if (bonusComp.Contains(mapComponent.Area))
                    Completed++;

                var ShaperElder = shEld.Contains(mapComponent.Area);


                if (Completed == 0)
                    Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/circle2.png"), drawRect, Color.Red);
                else if (Completed == 1)
                    Graphics.DrawPluginImage(System.IO.Path.Combine(PluginDirectory, "images/circle2.png"), drawRect, Color.Yellow);


                if (ShaperElder)
                {
                    var bgRect = drawRect;
                    bgRect.Left = bgRect.Right - 25;
                    bgRect.Bottom = bgRect.Top + 17;

                    Graphics.DrawBox(bgRect, Color.Black);
                    Graphics.DrawText("S/E", 15, drawRect.TopRight, Color.Yellow, FontDrawFlags.Top | FontDrawFlags.Right);
                }

                if (Settings.ShowPenalty.Value)
                {
                    var mods = entity.GetComponent<Mods>();
                    var areaLvl = mapComponent.Area.AreaLevel;

                    if (mods.ItemMods.Any(x => x.Name == "MapShaped"))//Shaper orb upgraded
                        areaLvl += 5;

                    var penalty = LevelXpPenalty(areaLvl);
                    var textColor = Color.Lerp(Color.Red, Color.Green, (float)penalty);
                    //textColor.A = (byte)(255f * (1f - (float)penalty) * 20f);

                    var labelText = $"{penalty:p0}";
                    var textSize = Graphics.MeasureText(labelText, 20, FontDrawFlags.Center | FontDrawFlags.Bottom);

                    Graphics.DrawBox(new RectangleF(drawRect.X + drawRect.Width / 2 - textSize.Width / 2, drawRect.Y + drawRect.Height - textSize.Height, textSize.Width, textSize.Height), Color.Black);

                    Graphics.DrawText(labelText, 20, new Vector2(drawRect.Center.X, drawRect.Bottom),
                        textColor, FontDrawFlags.Center | FontDrawFlags.Bottom);
                }
            }
        }

        private double LevelXpPenalty(int arenaLevel)
        {
            int characterLevel = GameController.Player.GetComponent<Player>().Level;

            float effectiveArenaLevel = arenaLevel < 71 ? arenaLevel : ArenaEffectiveLevels[arenaLevel];
            double safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            double effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95)//For player levels equal to or higher than 95:
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);
            return xpMultiplier;
        }

        Dictionary<int, float> ArenaEffectiveLevels = new Dictionary<int, float>()
        {
            {71, 70.94f},
            {72, 71.82f},
            {73, 72.64f},
            {74, 73.4f},
            {75, 74.1f},
            {76, 74.74f},
            {77, 75.32f},
            {78, 75.84f},
            {79, 76.3f},
            {80, 76.7f},
            {81, 77.04f},
            {82, 77.32f},
            {83, 77.54f},
            {84, 77.7f}
        };

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
        }
    }
}
