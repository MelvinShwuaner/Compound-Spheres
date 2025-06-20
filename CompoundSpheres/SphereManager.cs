using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// The Manager For Your Compound Sphere
    /// </summary>
    public class SphereManager : MonoBehaviour, IEnumerable, IFormattable
    {
        /// <summary>
        /// a spheretile at x and y coordinates
        /// </summary>
        public SphereTile this[int x, int y] => SphereTiles[(x*Cols)+y];
        /// <summary>
        /// a Row at an X position
        /// </summary>
        public SphereRow this[int x] => SphereRows[x];
        internal SphereTile[] SphereTiles;
        internal SphereRow[] SphereRows;
        /// <summary>
        /// The X Axis
        /// </summary>
        public int Rows { private set; get; }
        /// <summary>
        /// The Y Axis
        /// </summary>
        public int Cols { private set; get; }
        /// <summary>
        /// the total amount of tiles on the sphere
        /// </summary>
        public int TotalTiles => Cols * Rows;
        /// <summary>
        /// Rows / 2PI
        /// </summary>
        public float Radius { private set; get; }
        /// <summary>
        /// 2 * Radius
        /// </summary>
        public float Diameter => 2 * Radius;

        #region MeshStuff
        /// <summary>
        /// The Mesh used, every tile has the same mesh
        /// </summary>
        public Mesh SphereTileMesh { private set; get; }
        /// <summary>
        /// The material used, every tile has the same material
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use addcustombuffer, since the manager will manage the buffer for you</remarks>
        public Material Material { private set; get; }
        internal GraphicsBuffer commandBuf;
        private GraphicsBuffer Matrixes, Colors, Textures;
        internal HashSet<int> _matrices, _colors, _textures;
        private Dictionary<string, IBuffer> CustomBuffers;
        #endregion
        #region Settings
        GetSphereTilePosition SphereTilePos;
        GetSphereTileRotation getSphereTileRotation;
        GetSphereTileScale getSphereTileScale;
        GetSphereTileTexture getSphereTileTexture;
        GetSphereTileColor getSphereTileColor;
        GetDisplayMode getdisplaymode;
        GetCameraRange GetCameraRange;
        internal int BufferSize;
        #endregion
        private void OnDestroy()
        {
            if (CustomBuffers != null)
            {
                foreach (var buffer in CustomBuffers)
                {
                    buffer.Value.Dispose();
                }
            }
            Matrixes.Release();
            Colors.Release();
            Textures.Release();
            commandBuf.Release();
            SphereRows = null;
            SphereTiles = null;
        }
        /// <summary>
        /// destroys the sphere manager and its game object, and frees up all memory
        /// </summary>
        public void Destroy()
        {
            Destroy(gameObject);
        }
        internal SphereManager Init(int rows, int cols, SphereManagerSettings sphereManagerSettings)
        {
            Cols = cols;
            Rows = rows;

            SphereTileMesh = sphereManagerSettings.SphereTileMesh;
            Material = sphereManagerSettings.SphereTileMaterial;
            getSphereTileColor = sphereManagerSettings.GetSphereTileColor;
            getSphereTileTexture = sphereManagerSettings.GetSphereTileTexture;
            getSphereTileRotation = sphereManagerSettings.GetSphereTileRotation;
            getSphereTileScale = sphereManagerSettings.GetSphereTileScale;
            SphereTilePos = sphereManagerSettings.getspheretileposition;
            getdisplaymode = sphereManagerSettings.GetDisplayMode;
            GetCameraRange = sphereManagerSettings.GetCameraRange;
            BufferSize = sphereManagerSettings.BufferSize;
            Material.SetTexture("TextureArray", sphereManagerSettings.TextureArray);
            Radius = Rows / (2 * Mathf.PI);
            SphereTiles = new SphereTile[rows * cols];
            SphereRows = new SphereRow[rows];

            commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            GraphicsBuffer.IndirectDrawIndexedArgs[] commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            commandData[0].indexCountPerInstance = SphereTileMesh.GetIndexCount(0);
            commandData[0].instanceCount = (uint)Cols;
            commandBuf.SetData(commandData);
            Matrixes = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, TotalTiles, 64);
            Colors = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, TotalTiles, 12);
            Textures = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, TotalTiles, 4);
            _colors = new HashSet<int>();
            _matrices = new HashSet<int>();
            _textures = new HashSet<int>();
            Material.SetBuffer("Colors", Colors);
            Material.SetBuffer("Textures", Textures);
            Material.SetBuffer("Matrixes", Matrixes);

            if(sphereManagerSettings.CustomBuffers != null)
            {
                foreach(IBufferData buffer in sphereManagerSettings.CustomBuffers)
                {
                    AddCustomBuffer(buffer);
                }
            }
            return this;
        }
        private SphereManager() { }
        /// <summary>
        /// clamps a position + change to the X Axis
        /// </summary>
        public float Clamp(float Pos, float Change)
        {
            int Max = Rows;
            Pos += Change;
            if (Pos < 0)
            {
                return Max + Pos;
            }
            return Pos % Max;
        }
        /// <summary>
        /// all rows that are in the camera range draw their tiles
        /// </summary>
        /// <remarks>the camera x and y positions are the camera's position on the 2d grid, NOT its actual coordinates</remarks>
        public void DrawTiles(int CameraX)
        {
            GetCameraRange(this, out int Min, out int Max);
            Material.SetFloat("ShouldRenderTextures", (int)getdisplaymode(this));
            for (int i = Min; i < Max; i++)
            {
                int I = (int)Clamp(CameraX, i);
                SphereRows[I].DrawTiles();
            }
        }
        /// <summary>
        /// draws all tiles, even if they are not visible, NOT RECOMMENDED!
        /// </summary>
        public void DrawAllTiles()
        {
            Material.SetFloat("ShouldRenderTextures", (int)getdisplaymode(this));
            foreach (SphereRow row in this)
            {
                row.DrawTiles();
            }
        }
        /// <summary>
        /// the 3D Position of a Position on the grid, along with a Height
        /// </summary>
        public Vector3 SphereTilePosition(float X, float Y, float Height = 0)
        {
            return SphereTilePos(this, X, Y, Height);
        }
        /// <summary>
        /// gets the rotation of a spheretile
        /// </summary>
        public Quaternion GetSphereTileRotation(SphereTile SphereTile)
        {
            return getSphereTileRotation(SphereTile);
        }
        /// <summary>
        /// the scale of a spheretile
        /// </summary>
        public Vector3 SphereTileScale(SphereTile SphereTile)
        {
            return getSphereTileScale(SphereTile);
        }
        /// <summary>
        /// the color of a spheretile
        /// </summary>
        public Color SphereTileColor(SphereTile SphereTile)
        {
            return getSphereTileColor(SphereTile);
        }
        /// <summary>
        /// the Index of the texture in the textures array that this spheretile has
        /// </summary>
        /// <param name="SphereTile"></param>
        /// <returns></returns>
        public int SphereTileTexture(SphereTile SphereTile)
        {
            return getSphereTileTexture(SphereTile);
        }
        /// <summary>
        /// the sphere manager acts as a list of sphere rows
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return SphereRows.GetEnumerator();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null);
        }
        /// <inheritdoc/>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
            return gameObject.name.ToString(formatProvider);
        }
        /// <summary>
        /// refresh all of the matrixes, textures and colors
        /// </summary>
        public void RefreshAll()
        {
            RefreshMatrixes();
            RefreshColors();
            RefreshTextures();
        }
        internal void Begin()
        {
            Matrixes.SetBuffer<Matrix4x4>(TotalTiles, (int i) => SphereTiles[i]);
            Colors.SetBuffer<Vector3>(TotalTiles, (int i) => SphereTiles[i]);
            Textures.SetBuffer<float>(TotalTiles, (int i) => SphereTiles[i]);
        }
        /// <summary>
        /// refresh the matrix array
        /// </summary>
        public void RefreshMatrixes()
        {
            Matrixes.UpdateBuffer<Matrix4x4>(_matrices, (int i) => SphereTiles[i], BufferSize);
        }
        /// <summary>
        /// refresh the color array
        /// </summary>
        public void RefreshColors()
        {
            Colors.UpdateBuffer<Vector3>(_colors, (int i) => SphereTiles[i], BufferSize);
        }
        /// <summary>
        /// refresh the texture array
        /// </summary>
        public void RefreshTextures()
        {
            Textures.UpdateBuffer<float>(_textures, (int i) => SphereTiles[i], BufferSize);
        }
        /// <summary>
        /// marks a tile's color to be refreshed
        /// </summary>
        public void UpdateColor(int X, int Y)
        {
            _colors.Add((X*Cols) +Y);
        }
        /// <summary>
        /// marks a tile's matrix to be refreshed
        /// </summary>
        public void UpdateMatrix(int X, int Y)
        {
            _matrices.Add((X * Cols) + Y);
        }
        /// <summary>
        /// marks a tile's texture to be refreshed
        /// </summary>
        public void UpdateTexture(int X, int Y)
        {
            _textures.Add((X * Cols) + Y);
        }
        /// <summary>
        /// refreshes a custom buffer
        /// </summary>
        public void RefreshCustom(string Name)
        {
            CustomBuffers[Name].Refresh();
        }
        /// <summary>
        /// marks a tile's custom property to be updated
        /// </summary>
        public void UpdateCustom(string Name, int X, int Y)
        {
            CustomBuffers[Name].Update(X, Y);
        }
        /// <summary>
        /// adds a custom buffer to this sphererow, which is then accessed by the GPU
        /// </summary>
        /// <param name="Name">the name of the buffer in the custom shader, must be unique</param>
        /// <param name="getcustomdata">a function that returns a struct to be stored in the buffer</param>
        /// <param name="Size">the size of each variable in the buffer, in bytes. for example if you are storing floats it will be 4 because floats take up 4 bytes</param>
        /// <returns>a compute buffer, to update it call buffer.update and buffer.refresh</returns>
        /// <remarks>your compute buffer will be automatically released from memory once sphere is destroyed</remarks>
        public CustomBuffer<T> AddCustomBuffer<T>(string Name, GetCustomData<T> getcustomdata, int Size) where T : struct
        {
            GraphicsBuffer Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, Cols, Size);
            Material.SetBuffer(Name, Buffer);
            CustomBuffer<T> buffer = new CustomBuffer<T>(this, Buffer, getcustomdata);
            CustomBuffers ??= new Dictionary<string, IBuffer>();
            CustomBuffers.Add(Name, buffer);
            return buffer;
        }
        /// <summary>
        /// a non-generic method of adding a custom buffer
        /// </summary>
        public IBuffer AddCustomBuffer(IBufferData data)
        {
            IBuffer buffer = data.GetBuffer(this);
            CustomBuffers ??= new Dictionary<string, IBuffer>();
            CustomBuffers.Add(data.GetName(), buffer);
            return buffer;
        }
        /// <summary>
        /// Creates Spheremanagers
        /// </summary>
        public static class Creator
        {
            /// <summary>
            /// Creates a sphere manager, cols and rows represent the size of the grid the manager displays
            /// </summary>
            /// <exception cref="ArgumentException">cols and rows must be above 0</exception>
            /// <remarks>this will create a game object with a sphere manager attached to it!</remarks>
            public static SphereManager CreateSphereManager(int rows, int cols, SphereManagerSettings sphereManagerSettings, string Name = "SphereManager")
            {
                if(cols <= 0 || rows <= 0)
                {
                    throw new ArgumentException("Cols And Rows must be above 0 when creating a sphere manager");
                }
                GameObject SphereManager = new GameObject(Name);
                SphereManager Manager = SphereManager.AddComponent<SphereManager>().Init(rows, cols, sphereManagerSettings);
                for (int X = 0; X < rows; X++)
                {
                    SphereRow row = Manager.SphereRows[X] = new SphereRow(Manager, X);
                    for (int Y = 0; Y < cols; Y++)
                    {
                        Manager.SphereTiles[(X * cols) + Y] = new SphereTile(X, Y, row);
                    }
                }
                Manager.Begin();
                sphereManagerSettings.Initiation(Manager);
                return Manager;
            }
        }
    }
}