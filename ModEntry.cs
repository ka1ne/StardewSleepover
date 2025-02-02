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
        }

        private Dictionary<string, List<BedInfo>> bedAreas = new Dictionary<string, List<BedInfo>>
        {
            {"AnimalShop", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(12, 4, 2, 3), Owner = "Marnie" },  // Marnie's bed
                new BedInfo { Area = new Rectangle(23, 3, 3, 3), Owner = "Shane" }    // Shane's bed
            }},
            {"Manor", new List<BedInfo> {
                new BedInfo { Area = new Rectangle(8, 5, 2, 3), Owner = "Lewis" }     // Lewis's bed
            }}
        };

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Input.CursorMoved += OnCursorMoved;  // Add cursor tracking
            
            // Register debug commands
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
            // Map locations to their owners
            Dictionary<string, string> locationOwners = new Dictionary<string, string>
            {
                {"HaleyHouse", "Haley"},
                {"SamHouse", "Sam"},
                {"SebastianRoom", "Sebastian"},
                {"CarpentersHouse", "Robin"},
                {"ScienceHouse", "Maru"},
                {"JoshHouse", "Alex"},
                {"ArchaeologyHouse", "Penny"},
                {"Trailer", "Pam"},
                {"Manor", "Lewis"},
                {"LeahHouse", "Leah"},
                {"ElliottHouse", "Elliott"},
                {"WizardHouse", "Wizard"},
                {"HarveyRoom", "Harvey"},
                {"AnimalShop", "Marnie"},
                {"Ranch", "Marnie"}
            };

            Monitor.Log($"Checking location: {location.Name}", LogLevel.Debug);
            if (locationOwners.TryGetValue(location.Name, out string owner))
                return owner;

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
            if (!Context.IsWorldReady) return;

            // Fix for ICursorPosition error
            Vector2 cursorPos = new Vector2(e.NewPosition.ScreenPixels.X, e.NewPosition.ScreenPixels.Y);
            Vector2 tile = cursorPos / 64f;
            Monitor.Log($"Cursor at tile: ({(int)tile.X}, {(int)tile.Y})", LogLevel.Debug);
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
    }
}