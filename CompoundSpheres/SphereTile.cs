using System;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// a tile on a sphere, storing its position in 3d space, x and y coordinates on its grid, and rotation
    /// </summary>
    public struct SphereTile
    {
        /// <summary>
        /// the X position on the grid, which is its row
        /// </summary>
        public readonly int X;
        /// <summary>
        /// the Y position on the grid, which is its column
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// the rotation
        /// </summary>
        public readonly Quaternion Rotation;
        /// <summary>
        /// the Position in 3D space
        /// </summary>
        public readonly Vector3 Position;
        /// <summary>
        /// the scale of this tile, this is not constant and can change
        /// </summary>
        public Vector3 Scale { get; private set; }
        /// <summary>
        /// the color of this tile, represented by vector4
        /// </summary>
        public Color32 Color { get; private set; }
        /// <summary>
        /// the texture index of this sphere tile in the managers texture array
        /// </summary>
        public int TextureIndex { get; private set; }
        /// <summary>
        /// a Matrix4x4 representing the position, rotation of the sphere tile
        /// </summary>
        public Matrix4x4 Matrix => Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation);
        /// <summary>
        /// the 1D coordinates of a sphere tile
        /// </summary>
        public int Index => (X * Row.Cols) + Y;
        /// <summary>
        /// the Row this tile is Im
        /// </summary>
        public readonly SphereRow Row;
        /// <summary>
        /// the Manager of this tile
        /// </summary>
        public SphereManager Manager => Row.SphereManager;
        internal SphereTile(int X, int Y, SphereRow row)
        {
            Row = row;
            this.X = X;
            this.Y = Y;
            Position = row.SphereManager.SphereTilePosition(X, Y);
            Rotation = Quaternion.identity;
            Color = default;
            Scale = Vector3.one;
            TextureIndex = 0;
            Rotation = row.SphereManager.GetSphereTileRotation(this);
        }
        /// <summary>
        /// Updates and Returns the Color
        /// </summary>
        public Color32 UpdateColor()
        {
            Color = Manager.SphereTileColor(this);
            return Color;
        }
        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public Vector3 UpdateScale()
        {
            Scale = Manager.SphereTileScale(this);
            return Scale;
        }
        /// <summary>
        /// Updates and Returns the texture index
        /// </summary>
        public int UpdateTexture()
        {
            TextureIndex = Manager.SphereTileTexture(this);
            return TextureIndex;
        }
    }
}