﻿#if !SMAPI_1_x
using Microsoft.Xna.Framework;

namespace StardewModdingAPI.Framework
{
    /// <summary>Defines a position on a given map at different reference points.</summary>
    internal class CursorPosition : ICursorPosition
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The pixel position relative to the top-left corner of the visible screen.</summary>
        public Vector2 ScreenPixels { get; }

        /// <summary>The tile position under the cursor relative to the top-left corner of the map.</summary>
        public Vector2 Tile { get; }

        /// <summary>The tile position that the game considers under the cursor for purposes of clicking actions. This may be different than <see cref="Tile"/> if that's too far from the player.</summary>
        public Vector2 GrabTile { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="screenPixels">The pixel position relative to the top-left corner of the visible screen.</param>
        /// <param name="tile">The tile position relative to the top-left corner of the map.</param>
        /// <param name="grabTile">The tile position that the game considers under the cursor for purposes of clicking actions.</param>
        public CursorPosition(Vector2 screenPixels, Vector2 tile, Vector2 grabTile)
        {
            this.ScreenPixels = screenPixels;
            this.Tile = tile;
            this.GrabTile = grabTile;
        }
    }
}
#endif
