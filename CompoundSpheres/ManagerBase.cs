using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundSpheres
{
    public abstract class ManagerRoot : MonoBehaviour {
        public ComputeShader ComputeShader { get; protected set; }
        public int MatrixKernel { get; protected set; }
        public int ColorKernel { get; protected set; }
        /// <summary>
        /// The material used, every tile has the same material
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use addcustombuffer, since the manager will manage the buffer for you</remarks>
        public Material Material { protected set; get; }
        public abstract int TotalTiles { get; }
    }
    public abstract class ManagerBase<T> : ManagerRoot where T : TileBase
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
        public override int TotalTiles => Tiles.Length;
        /// <summary>
        /// The Mesh used, every tile has the same mesh
        /// </summary>
        public Mesh SphereTileMesh { protected set; get; }
        protected GetSphereTileScale<T> getSphereTileScale;
        protected ComputeBuffer<Vector2> Positions;
        protected Buffer<Vector3> Scales;
        protected ComputeGraphicsBuffer<Matrix4x4> Matrixes;
        protected ComputeGraphicsBuffer<Color32> Colors;
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
            Matrixes.Dispose();
            Positions.Dispose();
            Scales.Dispose();
            Colors.Dispose();
        }
        protected void Init(ManagerSettings<T> Settings)
        {
            SphereTileMesh = Settings.SphereTileMesh;
            Material = Settings.SphereTileMaterial;
            getdisplaymode = Settings.GetDisplayMode;
            
            getSphereTileScale = Settings.GetSphereTileScale;
            ComputeShader = Settings.ComputeShader;
            MatrixKernel = Settings.ComputeShader.FindKernel(Settings.MatrixKernel);
            ColorKernel = Settings.ComputeShader.FindKernel(Settings.ColorKernel);

            Positions = new ComputeBuffer<Vector2>(Settings.ComputeShader, MatrixKernel, "InputPositions", TotalTiles);

            Matrixes = new ComputeGraphicsBuffer<Matrix4x4>(ComputeShader, Material, MatrixKernel, "OutputMatrices", "Matrixes", TotalTiles, 64);
            Colors = new ComputeGraphicsBuffer<Color32>(ComputeShader, Material, ColorKernel, "OutputColors", "Colors", TotalTiles, 64);

            Scales = new Buffer<Vector3>(GraphicsBuffer.Target.Structured, TotalTiles, Material, "Scales");

            if (Settings.CustomBuffers != null)
            {
                foreach (IBufferData buffer in Settings.CustomBuffers)
                {
                    AddCustomBuffer(buffer);
                }
            }
        }
        public void SetComputeProperty(string name, float value)
        {
            ComputeShader.SetFloat(name, value);
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
            IBuffer buffer = data.GetBuffer(this);
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
        public void SetColorDirty(int I)
        {
            Colors.Update(I);
        }
        /// <summary>
        /// refresh the matrix array
        /// </summary>
        public void RefreshScales()
        {
            Scales.Refresh();
        }
        public void SetMatrixDirty(int I)
        {
            Matrixes.Update(I);
        }
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
            Positions.Set((int i) => Tiles[i].Position); ;
            ComputeShader.Dispatch(MatrixKernel, TotalTiles/64, 1, 1);
            ComputeShader.Dispatch(ColorKernel, TotalTiles / 64, 1, 1);
            Scales.Set((int i) => Tiles[i].UpdateScale());
        }
        /// <summary>
        /// the scale of a spheretile
        /// </summary>
        public Vector3 SphereTileScale(T SphereTile)
        {
            return getSphereTileScale(SphereTile);
        }
    }
    public abstract class TileBase
    {
        protected TileBase(int Index)
        {
            this.Index = Index;
        }
        public readonly int Index;
        public Vector2 Position => new Vector2(X, Y);
        /// <summary>
        /// the X position on the grid, which is its row
        /// </summary>
        public int X { get; internal set; }
        /// <summary>
        /// the Y position on the grid, which is its column
        /// </summary>
        public int Y { get; internal set; }
        /// <summary>
        /// the scale of this tile, this is not constant and can change
        /// </summary>
        public Vector3 Scale { get; protected set; }
        /// <summary>
        /// the color of this tile, represented by vector4
        /// </summary>
        public Color32 Color { get; internal set; }
        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public abstract Vector3 UpdateScale();
    }
}
