using System;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// a tile on a sphere, storing its position in 3d space, x and y coordinates on its grid, and rotation
    /// </summary>
    public readonly struct SphereTile : IEquatable<SphereTile>, IFormattable, IComparable<SphereTile>
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
        public Vector3 Scale => Manager.SphereTileScale(this);
        /// <summary>
        /// the color of this tile, represented by vector4
        /// </summary>
        public Vector4 Color => Manager.SphereTileColor(this);
        public int TextureIndex => Manager.SphereTileTexture(this);
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
            Rotation = row.SphereManager.GetSphereTileRotation(this);
        }
        public override string ToString()
        {
            return ToString(null, null);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            format ??= "F5";
            return $"Tile {X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)} Managed By {Manager.gameObject.name}";
        }
        public int CompareTo(SphereTile other)
        {
            if (X != other.X)
            {
                return X.CompareTo(other.X);
            }
            if (Y != other.Y)
            {
                return Y.CompareTo(other.Y);
            }
            return 0;
        }
        public override bool Equals(object obj)
        {
            return Equals((SphereTile)obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public bool Equals(SphereTile other)
        {
            return CompareTo(other) == 0 && other.Manager == Manager;
        }
        public static implicit operator int(SphereTile Tile)
        {
            return (Tile.X * Tile.Row.Cols) + Tile.Y;
        }
        public static implicit operator Vector4(SphereTile Tile)
        {
            return Tile.Color;
        }
        public static implicit operator Matrix4x4(SphereTile tile)
        {
            return Matrix4x4.TRS(tile.Position, tile.Rotation, tile.Scale);
        }
        public static bool operator ==(SphereTile Tile, SphereTile Tile2){
            return Tile.Equals(Tile2);
        }
        public static bool operator !=(SphereTile Tile, SphereTile Tile2)
        {
            return !Tile.Equals(Tile2);
        }
    }
}