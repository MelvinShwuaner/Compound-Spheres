using System;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// a tile on a sphere, storing its position in 3d space, x and y coordinates on its grid, and rotation
    /// </summary>
    public struct SphereTile : IEquatable<SphereTile>, IFormattable, IComparable<SphereTile>
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
        public Vector3 Color { get; private set; }
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
            Color = Vector4.one;
            Scale = Vector3.one;
            TextureIndex = 0;
            Rotation = row.SphereManager.GetSphereTileRotation(this);
        }
        /// <summary>
        /// Updates and Returns the Color
        /// </summary>
        public Vector3 UpdateColor()
        {
            Color = (Vector4)Manager.SphereTileColor(this);
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
        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null);
        }
        /// <summary>
        /// returns a string representing this sphere tile
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            format ??= "F5";
            return $"Tile {X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)} Managed By {Row}";
        }
        /// <summary>
        /// if the X coordinates are different, compares them and returns the result, otherwise it compares the Y coordinates and returns the result
        /// </summary>
        public int CompareTo(SphereTile other)
        {
            if (X != other.X)
            {
                return X.CompareTo(other.X);
            }
            return Y.CompareTo(other.Y);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals((SphereTile)obj);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// returns true if both coordinates are the same and are managed by the same manager
        /// </summary>
        public bool Equals(SphereTile other)
        {
            return CompareTo(other) == 0 && other.Manager == Manager;
        }
        /// <summary>
        /// returns true if both coordinates are the same and are managed by the same manager
        /// </summary>
        public static bool operator ==(SphereTile Tile, SphereTile Tile2){
            return Tile.Equals(Tile2);
        }
        /// <summary>
        /// returns true if both coordinates are different or are managed by different managers
        /// </summary>
        public static bool operator !=(SphereTile Tile, SphereTile Tile2)
        {
            return !Tile.Equals(Tile2);
        }
        /// <inheritdoc/>
        public static bool operator >(SphereTile Tile, SphereTile Tile2)
        {
            return Tile.CompareTo(Tile2) > 0;
        }
        /// <inheritdoc/>
        public static bool operator <(SphereTile Tile, SphereTile Tile2)
        {
            return Tile.CompareTo(Tile2) < 0;
        }
        /// <inheritdoc/>
        public static bool operator >=(SphereTile Tile, SphereTile Tile2)
        {
            return Tile.CompareTo(Tile) >= 0;
        }
        /// <inheritdoc/>
        public static bool operator <=(SphereTile Tile, SphereTile Tile2)
        {
            return Tile.CompareTo(Tile2) <= 0;
        }
    }
}