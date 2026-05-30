using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// The Manager For Your Compound Sphere
    /// </summary>
    public class SphereManager : ManagerBase<SphereTile>, IEnumerable
    {
        /// <summary>
        /// a spheretile at x and y coordinates
        /// </summary>
        public override SphereTile this[int x, int y] => Tiles[(x*Cols)+y];
        /// <summary>
        /// a Row at an X position
        /// </summary>
        public SphereRow this[int x] => SphereRows[x];
        
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
        /// Rows / 2PI
        /// </summary>
        public float Radius { private set; get; }
        /// <summary>
        /// 2 * Radius
        /// </summary>
        public float Diameter => 2 * Radius;

        public override int RowCount => Rows;

        #region MeshStuff
        internal GraphicsBuffer commandBuf;
        
        
        #endregion
        #region Settings
        GetCameraRange GetCameraRange;
        protected GetSphereTilePosition SphereTilePos;
        protected Buffer<float> Textures;
        protected GetSphereTileTexture getSphereTileTexture;
        #endregion
        protected override void OnDestroy()
        {
            base.OnDestroy();
            commandBuf.Release();
            Textures.Dispose();
        }
        internal SphereManager Init(int rows, int cols, SphereManagerSettings sphereManagerSettings)
        {
            Cols = cols;
            Rows = rows;
            Tiles = new SphereTile[rows * cols];

            base.Init(sphereManagerSettings);
            GetCameraRange = sphereManagerSettings.GetCameraRange;
            
            Material.SetTexture("TextureArray", sphereManagerSettings.TextureArray);
            Textures = new Buffer<float>(GraphicsBuffer.Target.Structured, TotalTiles, Material, "Textures");
            getSphereTileTexture = sphereManagerSettings.GetSphereTileTexture;

            Radius = Rows / (2 * Mathf.PI);
            ComputeShader.SetFloat("Radius", Radius);
            SphereRows = new SphereRow[rows];

            commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            commandData[0].indexCountPerInstance = SphereTileMesh.GetIndexCount(0);
            commandData[0].instanceCount = (uint)Cols;
            commandBuf.SetData(commandData);

            return this;
        }
        internal override void Begin()
        {
            base.Begin();
            Textures.Set((int i) => Tiles[i].UpdateTexture());
        }
        /// <summary>
        /// Sets the Mesh and updates the Sphere
        /// </summary>
        public void SetMesh(Mesh mesh)
        {
            SphereTileMesh = mesh;
            commandData[0].indexCountPerInstance = SphereTileMesh.GetIndexCount(0);
            commandBuf.SetData(commandData);
        }
        readonly GraphicsBuffer.IndirectDrawIndexedArgs[] commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        void SetRenderAmount(uint Amount)
        {
            commandData[0].instanceCount = Amount;
            commandBuf.SetData(commandData);
        }
        private SphereManager() { }
        
        /// <summary>
        /// all rows that are in the camera range draw their tiles
        /// </summary>
        /// <remarks>the camera x and y positions are the camera's position on the 2d grid, NOT its actual coordinates</remarks>
        public void DrawTiles(int CameraX, int CameraY)
        {
            GetCameraRange(this, out Range Row, out Range Col);
            Material.SetFloat("ShouldRenderTextures", (int)getdisplaymode());
            uint MaxCol = (uint)Mathf.Clamp(Col.Max + CameraY, 0, Cols);
            int MinCol = Mathf.Clamp(CameraY + Col.Min, 0, Cols);
            SetRenderAmount(MaxCol - (uint)MinCol);
            Material.SetInteger("Col", MinCol);
            for (int i = Row.Min; i < Row.Max; i++)
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
            Material.SetFloat("ShouldRenderTextures", (int)getdisplaymode());
            Material.SetInteger("Col", 0);
            SetRenderAmount((uint)Cols);
            foreach (SphereRow row in this)
            {
                row.DrawTiles();
            }
        }
        /// <summary>
        /// the 3D Position of a Position on the grid, along with a Height (-1 in a dynamicmanager)
        /// </summary>
        public Vector3 SphereTilePosition(float X, float Y, float Height = 0)
        {
            return SphereTilePos(this, X, Y, Height);
        }
        /// <summary>
        /// the sphere manager acts as a list of sphere rows
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return SphereRows.GetEnumerator();
        }
        /// <summary>
        /// marks a tile's color to be refreshed
        /// </summary>
        public void UpdateColor(int X, int Y)
        {
           SetColorDirty((X*Cols) +Y);
        }
        /// <summary>
        /// marks a tile's matrix to be refreshed
        /// </summary>
        public void UpdateScale(int X, int Y)
        {
           UpdateScale((X * Cols) + Y);
        }
        /// <summary>
        /// marks a tile's texture to be refreshed
        /// </summary>
        public void UpdateTexture(int X, int Y)
        {
            UpdateTexture((X * Cols) + Y);
        }
        public int sphereTileTexture(SphereTile sphereTile)
        {
            return getSphereTileTexture(sphereTile);
        }
        /// <summary>
        /// updates a tiles texture
        /// </summary>
        public override void UpdateTexture(int I)
        {
            Textures[I] = Tiles[I].UpdateTexture();
        }
        public override void RefreshTextures()
        {
            Textures.Refresh();
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
                        Manager.Tiles[(X * cols) + Y] = new SphereTile(X, Y, row);
                    }
                }
                Manager.Begin();
                sphereManagerSettings.Initiation(Manager);
                return Manager;
            }
        }
    }
}