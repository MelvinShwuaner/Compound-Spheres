using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Collections;
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
        public SphereTile this[int i] => SphereManager.SphereTiles[Row, i];
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
        /// <remarks>dont add custom buffers directly to this, instead use addcustombuffer, since the manager will release its memory automatically for you </remarks>
        public MaterialPropertyBlock Properties => _rp.matProps;
        internal SphereRow(SphereManager manager, int Row)
        {
            SphereManager = manager;
            _matrices = new HashSet<int>();
            _colors = new HashSet<int>();
            _textures = new HashSet<int>();
            this.Row = Row;
            _rp = new RenderParams(manager.Material)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000),
                matProps = new MaterialPropertyBlock()
            };
            Matrixes = new ComputeBuffer(Cols, 64, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            Colors = new ComputeBuffer(Cols, 16, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            Textures = new ComputeBuffer(Cols, 4, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        }
        /// <summary>
        /// update a tile's matrix, color, and texture
        /// </summary>
        public void UpdateTile(int i)
        {
            UpdateMatrix(i);
            UpdateColor(i);
            UpdateTexture(i);
        }
        /// <summary>
        /// update a tiles object to world matrix, which is its position, rotation and scale
        /// </summary>
        /// <param name="i"></param>
        public void UpdateMatrix(int i)
        {
            _matrices.Add(i);
        }
        /// <summary>
        /// update a tiles color
        /// </summary>
        /// <param name="i"></param>
        public void UpdateColor(int i)
        {
            _colors.Add(i);
        }
        /// <summary>
        /// update a tiles texture
        /// </summary>
        /// <param name="i"></param>
        public void UpdateTexture(int i)
        {
            _textures.Add(i);
        }
        internal void Begin()
        {
            UpdateAllTiles();
            Properties.SetBuffer("Colors", Colors);
            Properties.SetBuffer("Textures", Textures);
            Properties.SetBuffer("Matrixes", Matrixes);
            RefreshAll();
        }
        /// <summary>
        /// update all tiles
        /// </summary>
        public void UpdateAllTiles()
        {
            for (int i = 0; i < Cols; i++)
            {
                UpdateTile(i);
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Cols; i++)
            {
                yield return this[i];
            }
        }
        /// <summary>
        /// refresh all of the matrixes, textures and colors
        /// </summary>
        /// <remarks>updating a tile's property updates the property array, refreshing the property array sends the updated property array to the GPU </remarks>
        public void RefreshAll()
        {
            RefreshMatrixes();
            RefreshColors();
            RefreshTextures();
        }
        /// <summary>
        /// refresh the matrix array
        /// </summary>
        public void RefreshMatrixes()
        {
            NativeArray<Matrix4x4> array = Matrixes.BeginWrite<Matrix4x4>(0, Cols);
            foreach(var i in _matrices)
            {
                array[i] = this[i];
            }
            Matrixes.EndWrite<Matrix4x4>(_matrices.Count);
            _matrices.Clear();
        }
        /// <summary>
        /// refresh the color array
        /// </summary>
        public void RefreshColors()
        {
            NativeArray<Vector4> array = Colors.BeginWrite<Vector4>(0, Cols);
            foreach (var i in _colors)
            {
                array[i] = this[i];
            }
            Colors.EndWrite<Vector4>(_colors.Count);
            _colors.Clear();
        }
        /// <summary>
        /// adds a custom buffer to this sphererow, which is then accessed by the GPU
        /// </summary>
        /// <param name="Name">the name of the buffer in the custom shader, must be unique</param>
        /// <param name="getcustomdata">a function that returns a struct to be stored in the buffer</param>
        /// <param name="Size">the size of each variable in the buffer, in bytes. for example if you are storing floats it will be 4 because floats take up 4 bytes</param>
        /// <returns>a compute buffer, to refresh it call buffer.setdata</returns>
        /// <remarks>your compute buffer will be automatically released from memory once sphere is destroyed</remarks>
        public CustomBuffer<T> AddCustomBuffer<T>(string Name, GetCustomData<T> getcustomdata, int Size) where T : struct
        {
            ComputeBuffer Buffer = new ComputeBuffer(Cols, Size, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            Properties.SetBuffer(Name, Buffer);
            CustomBuffers ??= new List<ComputeBuffer>();
            CustomBuffers.Add(Buffer);
            return new CustomBuffer<T>(this, Buffer, getcustomdata);
        }
        /// <summary>
        /// refresh the texture array
        /// </summary>
        public void RefreshTextures()
        {
            NativeArray<float> array = Textures.BeginWrite<float>(0, Cols);
            foreach (var i in _textures)
            {
                array[i] = this[i].TextureIndex;
            }
            Textures.EndWrite<float>(_textures.Count);
            _textures.Clear();
        }
        internal void Finish()
        {
            Matrixes.Release();
            Colors.Release();
            Textures.Release();
            CustomBuffers?.ForEach((m) => m.Release());
            _matrices = null;
            _colors = null;
            _textures = null;
            _rp.matProps = null;
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
            return $"Row {Row.ToString(format, formatProvider)} managed by {SphereManager.gameObject.name}";
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
        private HashSet<int> _matrices, _colors, _textures;
        private ComputeBuffer Matrixes, Colors, Textures;
        private List<ComputeBuffer> CustomBuffers;
    }
    /// <summary>
    /// a class for managing a buffer efficiently
    /// </summary>
    public class CustomBuffer<T> where T : struct
    {
        /// <summary>
        /// the row this is managing
        /// </summary>
        public SphereRow Row;
        /// <summary>
        /// the buffer this is managing
        /// </summary>
        public ComputeBuffer Buffer;
        readonly HashSet<int> ToUpdate;
        readonly GetCustomData<T> getCustomData;
        /// <summary>
        /// refreshes all of the data
        /// </summary>
        public void Refresh()
        {
            NativeArray<T> array = Buffer.BeginWrite<T>(0, Row.Cols);
            foreach (var i in ToUpdate)
            {
                array[i] = getCustomData(Row[i]);
            }
            Buffer.EndWrite<T>(ToUpdate.Count);
            ToUpdate.Clear();
        }
        internal CustomBuffer(SphereRow Row, ComputeBuffer Buffer, GetCustomData<T> getdata)
        {
            getCustomData = getdata;
            ToUpdate = new HashSet<int>();
            this.Row = Row;
            this.Buffer = Buffer;
        }
        /// <summary>
        /// marks a sphere tile to be refreshed
        /// </summary>
        public void Update(int i)
        {
            ToUpdate.Add(i);
        }
    }
}