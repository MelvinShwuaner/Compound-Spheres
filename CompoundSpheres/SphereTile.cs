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
        /// the 1D coordinates of a sphere tile
        /// </summary>
        public int Index => (Manager.Cols * X) + Y;
        /// <summary>
        /// the Row this tile is Im
        /// </summary>
        public SphereRow Row { get; private set;  }
        /// <summary>
        /// the Manager of this tile
        /// </summary>
        public SphereManager Manager => Row.Manager;
        
        internal SphereTile(int X, int Y, SphereRow row)
        {
            Row = row;
            this.X = X;
            this.Y = Y;
            Position = Manager.SphereTilePosition(X, Y);
            Rotation = Quaternion.identity;
            Color = default;
            Scale = Vector3.one;
            TextureIndex = 0;
            Rotation = Manager.GetSphereTileRotation(this);
        }
        /// <summary>
        /// Updates and Returns the Matrix
        /// </summary>
        public override Matrix4x4 UpdateMatrix()
        {
            Position = Manager.SphereTilePosition(X, Y);
            Rotation = Manager.GetSphereTileRotation(this);
            return Matrix;
        }
        /// <summary>
        /// Updates and Returns the Color
        /// </summary>
        public override Color32 UpdateColor()
        {
            Color = Manager.SphereTileColor(this);
            return Color;
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
        public override int UpdateTexture()
        {
            TextureIndex = Manager.SphereTileTexture(this);
            return TextureIndex;
        }
    }
}