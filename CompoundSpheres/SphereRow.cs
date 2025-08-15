using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// sphere rows control the displaying of tiles
    /// </summary>
    public class SphereRow : IEnumerable, IEquatable<SphereRow>, IComparable<SphereRow>, IFormattable
    {
        /// <summary>
        /// the manager of this row
        /// </summary>
        public readonly SphereManager SphereManager;
        /// <summary>
        /// get a sphere tile at this row and column i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public SphereTile this[int i] => SphereManager[Row, i];
        /// <summary>
        /// the number of tiles in this row
        /// </summary>
        public int Cols => SphereManager.Cols;
        /// <summary>
        /// the X coordinate of this row
        /// </summary>
        public readonly int Row;
        /// <summary>
        /// the material properties for this specific row
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use Manager.addcustombuffer, since the manager will manage the buffer for you </remarks>
        public MaterialPropertyBlock Properties => _rp.matProps;
        internal SphereRow(SphereManager manager, int Row)
        {
            SphereManager = manager;
            this.Row = Row;
            _rp = new RenderParams(manager.Material)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
                matProps = new MaterialPropertyBlock()
            };
            Properties.SetInteger("Row", Row * Cols);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Cols; i++)
            {
                yield return this[i];
            }
        }
        /// <summary>
        /// draw the spheretiles
        /// </summary>
        public void DrawTiles()
        {
            Graphics.RenderMeshIndirect(_rp, SphereManager.SphereTileMesh, SphereManager.commandBuf, 1);
        }
        /// <summary>
        /// returns true if both rows are managed by the same manager and are at the same X cord
        /// </summary>
        public bool Equals(SphereRow other)
        {
            return CompareTo(other) == 0 && other.SphereManager == SphereManager;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals((SphereRow)obj);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// returns 1 if the X cord is bigger then the other, 0 if both are the same and -1 if other's x cord is bigger
        /// </summary>
        public int CompareTo(SphereRow other)
        {
            return Row.CompareTo(other.Row);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null);
        }
        /// <summary>
        /// creates a string that represents this row
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            format ??= "F5";
            return $"Row {Row.ToString(format, formatProvider)} managed by {SphereManager}";
        }
        /// <summary>
        /// returns true if both rows are managed by the same manager and are at the same X cord
        /// </summary>
        public static bool operator ==(SphereRow Tile, SphereRow Tile2)
        {
            return Tile.Equals(Tile2);
        }
        /// <summary>
        /// returns true if both rows are not managed by the same manager or are at different X cords
        /// </summary>
        public static bool operator !=(SphereRow Tile, SphereRow Tile2)
        {
            return !Tile.Equals(Tile2);
        }
        /// <summary>
        /// returns true if the x cord is bigger
        /// </summary>
        public static bool operator >(SphereRow Tile, SphereRow Tile2)
        {
            return Tile.CompareTo(Tile2) > 0;
        }
        /// <summary>
        /// returns true if the x cord is smaller
        /// </summary>
        public static bool operator <(SphereRow Tile, SphereRow Tile2)
        {
            return Tile.CompareTo(Tile2) < 0;
        }
        /// <summary>
        /// returns true if the x cord is equal or bigger
        /// </summary>
        public static bool operator >=(SphereRow Tile, SphereRow Tile2)
        {
            return Tile.CompareTo(Tile) >= 0;
        }
        /// <summary>
        /// returns true if the x cord is equal or smaller
        /// </summary>
        public static bool operator <=(SphereRow Tile, SphereRow Tile2)
        {
            return Tile.CompareTo(Tile2) <= 0;
        }
        private RenderParams _rp;
    }
}