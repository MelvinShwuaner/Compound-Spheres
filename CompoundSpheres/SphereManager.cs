using System;
using System.Collections;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// The Manager For Your Compound Sphere
    /// </summary>
    public class SphereManager : MonoBehaviour, IEnumerable, IDisposable
    {
        /// <summary>
        /// a spheretile at x and y coordinates
        /// </summary>
        public SphereTile this[int x, int y] => SphereTiles[x, y];
        /// <summary>
        /// a Row at an X position
        /// </summary>
        public SphereRow this[int x] => SphereRows[x];
        internal SphereTile[,] SphereTiles;
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

        #region MeshStuff
        /// <summary>
        /// The Mesh used, every tile has the same mesh
        /// </summary>
        public Mesh SphereTileMesh { private set; get; }
        /// <summary>
        /// The material used, every tile has the same material
        /// </summary>
        /// <remarks>MUST have a compound sphere shader applied!</remarks>
        public Material Material { private set; get; }
        internal GraphicsBuffer commandBuf;
        #endregion
        #region Settings
        GetSphereTilePosition SphereTilePos;
        GetSphereTileRotation getSphereTileRotation;
        GetSphereTileScale getSphereTileScale;
        GetSphereTileTexture getSphereTileTexture;
        GetSphereTileColor getSphereTileColor;
        GetDisplayMode getdisplaymode;
        GetCameraRange GetCameraRange;
        #endregion
        private void OnDestroy()
        {
            Dispose();
        }
        ~SphereManager()
        {
            Dispose();
        }
        internal SphereManager Init(int cols, int rows, SphereManagerSettings sphereManagerSettings)
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
            Material.SetTexture("TextureArray", sphereManagerSettings.TextureArray);
            Radius = Rows / (2 * Mathf.PI);
            SphereTiles = new SphereTile[cols, rows];
            SphereRows = new SphereRow[rows];
            commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            GraphicsBuffer.IndirectDrawIndexedArgs[] commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            commandData[0].indexCountPerInstance = SphereTileMesh.GetIndexCount(0);
            commandData[0].instanceCount = (uint)Cols;
            commandBuf.SetData(commandData);

            return this;
        }
        /// <summary>
        /// clamps a position + change to the Y Axis
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

        public IEnumerator GetEnumerator()
        {
            return SphereRows.GetEnumerator();
        }

        public void Dispose()
        {
            foreach (SphereRow row in this)
            {
                row.Dispose();
            }
            commandBuf.Release();
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
            public static SphereManager CreateSphereManager(int cols, int rows, SphereManagerSettings sphereManagerSettings, string Name = "SphereManager")
            {
                if(cols <= 0 || rows <= 0)
                {
                    throw new ArgumentException("Cols And Rows must be above 0 when creating a sphere manager");
                }
                GameObject SphereManager = new GameObject(Name);
                SphereManager Manager = SphereManager.AddComponent<SphereManager>().Init(cols, rows, sphereManagerSettings);
                for (int X = 0; X < rows; X++)
                {
                    SphereRow row = Manager.SphereRows[X] = new SphereRow(Manager, X);
                    for (int Y = 0; Y < cols; Y++)
                    {
                        Manager.SphereTiles[X, Y] = new(X, Y, row);
                    }
                    row.Begin();
                }
                sphereManagerSettings.Initiation(Manager);
                return Manager;
            }
        }
    }
}