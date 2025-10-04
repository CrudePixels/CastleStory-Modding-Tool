using System.Collections.Generic;
using CastleStoryModdingTool.ModIntegrations;

namespace CastleStoryModdingTool.ModDefinitions
{
    public static class LadderModDefinition
    {
        public static IModIntegration CreateIntegration()
        {
            var modifications = new List<FileModification>
            {
                // LADDERS CANNOT BE ADDED - Assets were completely removed from the game
                // This mod is disabled until ladder assets can be restored from Unity project
                // For now, we'll just add a comment explaining the situation
                new FileModification
                {
                    RelativePath = "Info/Lua/LUI/Meta/Meta_WoodBlock.lua",
                    Type = FileModificationType.InsertBefore,
                    InsertMarker = "return _t",
                    Content = @"
-- LadderMod: DISABLED - Ladder assets were completely removed from Castle Story
-- The game no longer contains the necessary Unity assets (models, animations, prefabs)
-- To restore ladders, the original Unity assets would need to be re-compiled into the game
"
                }
            };

            return new FileModificationIntegration("LadderMod", modifications);
        }
    }
}
