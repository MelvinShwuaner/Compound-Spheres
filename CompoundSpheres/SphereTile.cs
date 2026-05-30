using System;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// a tile on a sphere, storing its position in 3d space, x and y coordinates on its grid, and rotation
    /// </summary>
    public class SphereTile : TileBase
    {
        /// <summary>
        /// the Row this tile is Im
        /// </summary>
        public SphereRow Row { get; private set;  }
        /// <summary>
        /// the Manager of this tile
        /// </summary>
        public SphereManager Manager => Row.Manager;
        public int TextureIndex { get; private set; }
        internal SphereTile(int X, int Y, SphereRow row) : base((row.Cols * X) + Y)
        {
            Row = row;
            this.X = X;
            this.Y = Y;
            Color = default;
            Scale = Vector3.one;
        }
        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public override Vector3 UpdateScale()
        {
            Scale = Manager.SphereTileScale(this);
            return Scale;
        }
        /// <summary>
        /// Updates and Returns the texture index
        /// </summary>
        public int UpdateTexture()
        {
            TextureIndex = Manager.sphereTileTexture(this);
            return TextureIndex;
        }
    }
}