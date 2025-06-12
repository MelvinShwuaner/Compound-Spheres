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
        /// <summary>
        /// the texture index of this sphere tile in the managers texture array
        /// </summary>
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
            return $"Tile {X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)} Managed By {Manager.gameObject.name}";
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
        /// the 1D coordinates of a sphere tile
        /// </summary>
        public static implicit operator int(SphereTile Tile)
        {
            return (Tile.X * Tile.Row.Cols) + Tile.Y;
        }
        /// <summary>
        /// the color of the sphere tile
        /// </summary>
        public static implicit operator Vector4(SphereTile Tile)
        {
            return Tile.Color;
        }
        /// <summary>
        /// a Matrix4x4 representing the position, scale, rotation of the sphere tile
        /// </summary>
        public static implicit operator Matrix4x4(SphereTile tile)
        {
            return Matrix4x4.TRS(tile.Position, tile.Rotation, tile.Scale);
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