using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Collections.Generic;

namespace StardewSleepover
{
    public class ModEntry : Mod
    {
        private readonly int REQUIRED_HEARTS = 6;

        private class BedInfo
        {
            public Rectangle Area { get; set; }
            public string Owner { get; set; }
            public string SecondaryOwner { get; set; }  // For couples' beds
        }

        private class LocationInfo
        {
            public string MapName { get; set; }
            public string[] ValidNames { get; set; }
        }

        private Dictionary<string, List<BedInfo>> bedAreas = new Dictionary<string, List<BedInfo>>
        {
            {"AnimalShop", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(12, 4, 2, 3), Owner = "Marnie" },  // Marnie's bed
                new BedInfo { Area = new Rectangle(23, 3, 3, 3), Owner = "Shane" }    // Shane's bed
            }},
            {"Manor", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(24, 5, 3, 2), Owner = "Lewis" }    // Lewis's bed: 24,5 to 26,6
            }},
            {"LeahHouse", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(9, 4, 3, 1), Owner = "Leah" }      // Leah's bed: 9,4 to 11,4
            }},
            {"SamHouse", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(24, 7, 3, 2), Owner = "Sam" },     // Sam's bed: 24,7 to 26,8
                new BedInfo { 
                    Area = new Rectangle(28, 4, 4, 2), 
                    Owner = "Jodi",
                    SecondaryOwner = "Kent"
                }     // Jodi & Kent's bed: 28,4 to 31,5
            }},
            {"SeedShop", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(1, 5, 2, 2), Owner = "Abigail" },   // Abigail's bed: 1,5 to 2,6
                new BedInfo { 
                    Area = new Rectangle(16, 3, 3, 2), 
                    Owner = "Caroline",
                    SecondaryOwner = "Pierre"
                }  // Pierre & Caroline's bed: 16,3 to 18,4
            }},
            {"GeorgeHouse", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(23, 3, 3, 3), Owner = "Alex" },     // Alex's bed: 23,3 to 25,5
                new BedInfo {
                    Area = new Rectangle(9, 3, 3, 2),
                    Owner = "George",
                    SecondaryOwner = "Evelyn"
                }      // George & Evelyn's bed
            }},
            {"Trailer", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(8, 10, 3, 3), Owner = "Pam" }      // Pam's bed: 8,10 to 10,12
            }},
            {"ElliottHouse", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(17, 5, 2, 2), Owner = "Elliott" }  // Elliott's bed: 17,5 to 18,6
            }},
            {"HaleyHouse", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(4, 4, 3, 2), Owner = "Haley" },    // Haley's bed: 4,4 to 6,5
                new BedInfo { Area = new Rectangle(22, 3, 3, 2), Owner = "Emily" }    // Emily's bed: 22,3 to 24,4
            }},
            {"Saloon", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(16, 3, 3, 2), Owner = "Gus" }      // Gus's bed: 16,3 to 18,4
            }},
            {"HarveyRoom", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(15, 5, 3, 2), Owner = "Harvey" }   // Harvey's bed: 15,5 to 17,6
            }},
            {"SebastianRoom", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(16, 8, 3, 3), Owner = "Sebastian" } // Sebastian's bed: 16,8 to 18,10
            }},
            {"ScienceHouse", new List<BedInfo> {
                new BedInfo { 
                    Area = new Rectangle(13, 3, 4, 2), 
                    Owner = "Robin",
                    SecondaryOwner = "Demetrius"
                },  // Robin & Demetrius's bed: 13,3 to 16,4
                new BedInfo { Area = new Rectangle(2, 3, 2, 2), Owner = "Maru" }      // Maru's bed: 2,3 to 3,4
            }}
        };

        private Dictionary<string, LocationInfo> locationMappings = new Dictionary<string, LocationInfo>
        {
            {"HaleyHouse", new LocationInfo { 
                MapName = "FarmHouse", 
                ValidNames = new[] {"HaleyHouse"} 
            }},
            {"SamHouse", new LocationInfo { 
                MapName = "FarmHouse", 
                ValidNames = new[] {"SamHouse"} 
            }},
            // Add mappings for each house...
        };

        private string currentHouseOwner = null;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Player.Warped += OnWarped;  // Add warp tracking
            
            helper.ConsoleCommands.Add("sleepover_debug", "Shows tile info at cursor position", DebugTileInfo);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            GameLocation location = Game1.currentLocation;
            if (location == null || location.IsFarm)
                return;

            // Only check if the player pressed the action button
            if (!e.Button.IsActionButton())
                return;

            Vector2 playerTile = Game1.player.Position / 64f;
            Monitor.Log($"Player at: {playerTile} in {location.Name}", LogLevel.Debug);

            // Check if player is in a bed area
            if (bedAreas.TryGetValue(location.Name, out List<BedInfo> bedInfos))
            {
                foreach (var bedInfo in bedInfos)
                {
                    if (bedInfo.Area.Contains((int)playerTile.X, (int)playerTile.Y))
                    {
                        Monitor.Log($"Found bed area in {location.Name}!", LogLevel.Debug);
                        HandleBedInteraction(location, new Vector2(bedInfo.Area.X, bedInfo.Area.Y));
                        return;
                    }
                }
            }
        }

        private bool IsBed(Furniture furniture)
        {
            string furnitureName = furniture.Name.ToLower();
            bool isBed = furnitureName.Contains("bed") || 
                        furnitureName.Contains("single") ||
                        furnitureName.Contains("double") ||
                        furnitureName.Contains("king") ||
                        furnitureName.Contains("queen") ||
                        furniture is BedFurniture;
                        
            Monitor.Log($"Checking furniture '{furnitureName}' - Is bed? {isBed}", LogLevel.Debug);
            return isBed;
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // Wake up if it's morning
            if (Game1.player.isInBed.Value && Game1.timeOfDay == 600)
            {
                Game1.player.isInBed.Value = false;
                Game1.currentLocation = Game1.getLocationFromName("FarmHouse");
                Game1.player.Position = new Vector2(Game1.player.Position.X, Game1.player.Position.Y);
            }
        }

        private void HandleBedInteraction(GameLocation location, Vector2 bedPosition)
        {
            // Get the owner of the room/bed
            string owner = GetBedOwner(location);
            if (string.IsNullOrEmpty(owner))
            {
                Monitor.Log("This bed doesn't belong to anyone.", LogLevel.Debug);
                Game1.chatBox?.addMessage("This bed doesn't belong to anyone.", Color.Red);
                return;
            }

            // Check friendship level
            if (!Game1.player.friendshipData.ContainsKey(owner))
            {
                string message = $"You don't know {owner} well enough to sleep here.";
                Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
                Game1.chatBox?.addMessage(message, Color.Red);
                return;
            }

            int hearts = Game1.player.friendshipData[owner].Points / 250;
            if (hearts < REQUIRED_HEARTS)
            {
                string message = $"You need {REQUIRED_HEARTS} hearts with {owner} to sleep here.";
                Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
                Game1.chatBox?.addMessage(message, Color.Red);
                return;
            }

            // Allow sleeping
            if (Game1.timeOfDay >= 2000 || (Game1.isRaining && Game1.timeOfDay >= 1800))
            {
                Game1.chatBox?.addMessage($"Sleeping in {owner}'s bed...", Color.Green);
                
                // Set bed position for proper animation
                Game1.player.Position = new Vector2(bedPosition.X * 64, bedPosition.Y * 64);
                
                // Show the sleep dialog like the normal bed does
                Game1.currentLocation.createQuestionDialogue(
                    "Do you want to go to sleep for the night?",
                    Game1.currentLocation.createYesNoResponses(),
                    (who, whichAnswer) =>
                    {
                        if (whichAnswer == "Yes")
                        {
                            Game1.player.isInBed.Value = true;
                            Game1.player.doEmote(24);
                            Game1.NewDay(600f);
                        }
                        Game1.activeClickableMenu = null;
                    }
                );
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("You can only sleep after 8:00 PM (or 6:00 PM if raining).", HUDMessage.error_type));
            }
        }

        private string GetBedOwner(GameLocation location)
        {
            Monitor.Log($"Checking location: {location.Name}, Map: {location.mapPath.Value}", LogLevel.Debug);

            // Get the base name of the location (strip instance numbers)
            string baseName = location.Name;
            while (baseName.Length > 0 && char.IsDigit(baseName[baseName.Length - 1]))
            {
                baseName = baseName.Substring(0, baseName.Length - 1);
            }

            // First try to find a matching location mapping
            foreach (var mapping in locationMappings)
            {
                if (location.mapPath.Value.Contains(mapping.Value.MapName) && 
                    mapping.Value.ValidNames.Contains(baseName))
                {
                    // Found a match, now get the owner
                    if (locationOwners.TryGetValue(mapping.Key, out string owner))
                        return owner;
                }
            }

            // Fallback to direct location name check
            if (locationOwners.TryGetValue(location.Name, out string directOwner))
                return directOwner;
            if (locationOwners.TryGetValue(baseName, out directOwner))
                return directOwner;

            return null;
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            GameLocation location = Game1.currentLocation;
            if (bedAreas.TryGetValue(location.Name, out List<BedInfo> bedInfos))
            {
                foreach (var bedInfo in bedInfos)
                {
                    // Convert tile coordinates to screen coordinates
                    Vector2 screenPos = Game1.GlobalToLocal(new Vector2(bedInfo.Area.X * 64, bedInfo.Area.Y * 64));
                    Rectangle screenRect = new Rectangle(
                        (int)screenPos.X,
                        (int)screenPos.Y,
                        bedInfo.Area.Width * 64,
                        bedInfo.Area.Height * 64
                    );

                    // Draw a semi-transparent rectangle around the bed area
                    e.SpriteBatch.Draw(
                        Game1.staminaRect,  // Using the game's white pixel texture
                        screenRect,
                        Color.Red * 0.3f    // Semi-transparent red
                    );

                    // Draw the coordinates as text for debugging
                    e.SpriteBatch.DrawString(
                        Game1.smallFont,
                        $"Bed Area: {bedInfo.Area}",
                        new Vector2(screenPos.X, screenPos.Y - 20),
                        Color.Yellow
                    );
                }
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            // Remove cursor position spam logging
            if (!Context.IsWorldReady) return;

            Vector2 cursorPos = new Vector2(e.NewPosition.ScreenPixels.X, e.NewPosition.ScreenPixels.Y);
            Vector2 tile = cursorPos / 64f;
            // Only log when Debug command is active
        }

        private void DebugTileInfo(string command, string[] args)
        {
            if (!Context.IsWorldReady) return;

            Vector2 cursorTile = Game1.currentCursorTile;
            // Fix for getTileLocation error
            Vector2 playerTile = Game1.player.Position / 64f;
            
            Monitor.Log($"Debug Info:", LogLevel.Info);
            Monitor.Log($"Cursor tile: ({(int)cursorTile.X}, {(int)cursorTile.Y})", LogLevel.Info);
            Monitor.Log($"Player tile: ({(int)playerTile.X}, {(int)playerTile.Y})", LogLevel.Info);
            Monitor.Log($"Current location: {Game1.currentLocation.Name}", LogLevel.Info);
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Enhanced warp tracking debug info
            Monitor.Log($"=== WARP EVENT ===", LogLevel.Debug);
            Monitor.Log($"From: {e.OldLocation.Name} ({e.OldLocation.mapPath.Value})", LogLevel.Debug);
            Monitor.Log($"To: {e.NewLocation.Name} ({e.NewLocation.mapPath.Value})", LogLevel.Debug);
            Monitor.Log($"Player Position: ({(int)(Game1.player.Position.X/64)}, {(int)(Game1.player.Position.Y/64)})", LogLevel.Debug);
            
            // Reset house owner when leaving a house
            if (e.OldLocation.Name != e.NewLocation.Name)
            {
                Monitor.Log($"Leaving previous location, clearing owner", LogLevel.Debug);
                currentHouseOwner = null;
            }

            // Check if we've warped into someone's house
            if (e.NewLocation.Name == "HaleyHouse") currentHouseOwner = "Haley";
            else if (e.NewLocation.Name == "SamHouse") currentHouseOwner = "Sam";
            // ... add other house mappings

            if (currentHouseOwner != null)
            {
                Monitor.Log($"House Owner detected: {currentHouseOwner}", LogLevel.Debug);
            }
            Monitor.Log($"==================", LogLevel.Debug);
        }
    }
}