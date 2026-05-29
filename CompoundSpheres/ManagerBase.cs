using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundSpheres
{
    public abstract class ManagerBase<T> : MonoBehaviour where T : TileBase
    {
        internal T[] Tiles;
        public abstract int RowCount { get; }
        /// <summary>
        /// a spheretile at x and y coordinates
        /// </summary>
        public abstract T this[int x, int y] { get; }
        /// <summary>
        /// the total amount of tiles
        /// </summary>
        public int TotalTiles => Tiles.Length;
        /// <summary>
        /// The Mesh used, every tile has the same mesh
        /// </summary>
        public Mesh SphereTileMesh { protected set; get; }
        protected GetSphereTileRotation<T> getSphereTileRotation;
        protected GetSphereTileScale<T> getSphereTileScale;
        protected GetSphereTileTexture<T> getSphereTileTexture;
        protected GetSphereTileColor<T> getSphereTileColor;
        /// <summary>
        /// The material used, every tile has the same material
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use addcustombuffer, since the manager will manage the buffer for you</remarks>
        public Material Material { protected set; get; }
        protected Buffer<Matrix4x4> Matrixes;
        protected Buffer<Color32> Colors;
        protected Buffer<Vector3> Scales;
        protected Dictionary<string, IBuffer> CustomBuffers;
        protected GetDisplayMode getdisplaymode;
        protected virtual void OnDestroy()
        {
            if (CustomBuffers != null)
            {
                foreach (var buffer in CustomBuffers)
                {
                    buffer.Value.Dispose();
                }
            }
            Scales.Dispose();
            Colors.Dispose();
            Matrixes.Dispose();
        }
        protected void Init(ManagerSettings<T> Settings)
        {
            SphereTileMesh = Settings.SphereTileMesh;
            Material = Settings.SphereTileMaterial;
            getdisplaymode = Settings.GetDisplayMode;
            getSphereTileColor = Settings.GetSphereTileColor;
            getSphereTileTexture = Settings.GetSphereTileTexture;
            getSphereTileRotation = Settings.GetSphereTileRotation;
            getSphereTileScale = Settings.GetSphereTileScale;

            Matrixes = new Buffer<Matrix4x4>(GraphicsBuffer.Target.Structured, TotalTiles, Material, "Matrixes");
            Colors = new Buffer<Color32>(GraphicsBuffer.Target.Structured, TotalTiles, Material, "Colors");
            Scales = new Buffer<Vector3>(GraphicsBuffer.Target.Structured, TotalTiles, Material, "Scales");

            if (Settings.CustomBuffers != null)
            {
                foreach (IBufferData buffer in Settings.CustomBuffers)
                {
                    AddCustomBuffer(buffer);
                }
            }
        }
        /// <summary>
        /// clamps a position + change to the X Axis
        /// </summary>
        public float Clamp(float Pos, float Change)
        {
            Pos += Change;
            if (Pos < 0)
            {
                return RowCount + Pos;
            }
            return Pos % RowCount;
        }
        /// <summary>
        /// destroys the manager and its game object, and frees up all memory
        /// </summary>
        public void Destroy()
        {
            Destroy(gameObject);
        }
        /// <summary>
        /// a non-generic method of adding a custom buffer
        /// </summary>
        public IBuffer AddCustomBuffer(IBufferData data)
        {
            IBuffer buffer = data.GetBuffer(Material, TotalTiles);
            CustomBuffers ??= new Dictionary<string, IBuffer>();
            CustomBuffers.Add(data.Name, buffer);
            return buffer;
        }
        /// <summary>
        /// updates a tiles custom property
        /// </summary>
        public void UpdateCustom(string Name, int I)
        {
            CustomBuffers[Name].Update(I);
        }
        /// <summary>
        /// refreshes a custom buffer
        /// </summary>
        public void RefreshCustom(string Name)
        {
            CustomBuffers[Name].Refresh();
        }
        /// <summary>
        /// updates a tiles scale
        /// </summary>
        public void UpdateScale(int I)
        {
           Scales[I] = Tiles[I].UpdateScale();
        }
        /// <summary>
        /// updates a tiles color
        /// </summary>
        public void UpdateColor(int I)
        {
            Colors[I] = Tiles[I].UpdateColor();
        }
        /// <summary>
        /// refresh the matrix array
        /// </summary>
        public void RefreshScales()
        {
            Scales.Refresh();
        }
        /// <summary>
        /// refresh the matrix array
        /// </summary>
        public void RefreshMatrixes()
        {
            Matrixes.Refresh();
        }
        /// <summary>
        /// refresh the color array
        /// </summary>
        public void RefreshColors()
        {
            Colors.Refresh();
        }
        /// <summary>
        /// refresh the texture array
        /// </summary>
        public abstract void RefreshTextures();
        public abstract void UpdateTexture(int I);
        /// <summary>
        /// refresh all of the scales, textures and colors, matrixes
        /// </summary>
        public void RefreshAll()
        {
            RefreshScales();
            RefreshColors();
            RefreshTextures();
            RefreshMatrixes();
        }
        /// <summary>
        /// refresh all of the custom buffers
        /// </summary>
        public void RefreshAllCustom()
        {
            foreach (var buffer in CustomBuffers)
            {
                buffer.Value.Refresh();
            }
        }
        public T GetTile(int Index)
        {
            return Tiles[Index];
        }
        internal virtual void Begin()
        {
            Matrixes.Set((int i) => Tiles[i].Matrix);
            Scales.Set((int i) => Tiles[i].UpdateScale());
            Colors.Set((int i) => Tiles[i].UpdateColor());
            
        }
        /// <summary>
        /// gets the rotation of a spheretile
        /// </summary>
        public Quaternion GetSphereTileRotation(T SphereTile)
        {
            return getSphereTileRotation(SphereTile);
        }
        /// <summary>
        /// the scale of a spheretile
        /// </summary>
        public Vector3 SphereTileScale(T SphereTile)
        {
            return getSphereTileScale(SphereTile);
        }
        /// <summary>
        /// the color of a spheretile
        /// </summary>
        /// <remarks>the Alpha Component is NOT USED.</remarks>
        public Color32 SphereTileColor(T SphereTile)
        {
            return getSphereTileColor(SphereTile);
        }
        /// <summary>
        /// the Index of the texture in the textures array that this spheretile has
        /// </summary>
        /// <param name="SphereTile"></param>
        /// <returns></returns>
        public int SphereTileTexture(T SphereTile)
        {
            return getSphereTileTexture(SphereTile);
        }
    }
    public abstract class TileBase
    {
        /// <summary>
        /// the X position on the grid, which is its row
        /// </summary>
        public int X { get; internal set; }
        /// <summary>
        /// the Y position on the grid, which is its column
        /// </summary>
        public int Y { get; internal set; }
        /// <summary>
        /// the rotation
        /// </summary>
        public Quaternion Rotation { get; protected set; }
        /// <summary>
        /// the Position in 3D space
        /// </summary>
        public Vector3 Position { get; protected set; }
        /// <summary>
        /// the scale of this tile, this is not constant and can change
        /// </summary>
        public Vector3 Scale { get; protected set; }
        /// <summary>
        /// the color of this tile, represented by vector4
        /// </summary>
        public Color32 Color { get; protected set; }
        /// <summary>
        /// the texture index of this sphere tile in the managers texture array
        /// </summary>
        public int TextureIndex { get; protected set; }
        /// <summary>
        /// a Matrix4x4 representing the position, rotation of the sphere tile
        /// </summary>
        public Matrix4x4 Matrix => Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation);
        /// <summary>
        /// Updates and Returns the Color
        /// </summary>
        public abstract Color32 UpdateColor();
        /// <summary>
        /// Updates and Returns the Matrix
        /// </summary>
        public abstract Matrix4x4 UpdateMatrix();
        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public abstract Vector3 UpdateScale();
        /// <summary>
        /// Updates and Returns the texture index
        /// </summary>
        public abstract int UpdateTexture();
    }
}
