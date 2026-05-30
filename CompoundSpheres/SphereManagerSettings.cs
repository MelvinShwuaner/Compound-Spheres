using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

namespace CompoundSpheres
{
    public class ComputeGraphicsBufferData<T> : IBufferData where T : struct
    {
        public string Name { get; set; }
        public int Kernel;
        public string ComputeName;
        public int Threads;
        public IBuffer GetBuffer(ManagerRoot ManagerData)
        {
           return new ComputeGraphicsBuffer<T>(ManagerData.ComputeShader, ManagerData.Material, Kernel, ComputeName, Name, ManagerData.TotalTiles, Threads);
        }
        public ComputeGraphicsBufferData(string Name, string ComputeName, int Kernel, int Threads)
        {
            this.Name = Name;
            this.Kernel = Kernel;
            this.ComputeName = ComputeName;
            this.Threads = Threads;
        }
    }
    public class ComputeBufferData<T> : IBufferData where T : struct
    {
        public string Name { get; private set; }
        public GetCustomData<T> getCustomData;
        public IBuffer GetBuffer(ManagerRoot ManagerData)
        {
            return new WrappedComputeBuffer<T>(new ComputeBuffer<T>(ManagerData.ComputeShader, ManagerData.MatrixKernel, Name, ManagerData.TotalTiles), getCustomData);
        }
        public ComputeBufferData(string Name, GetCustomData<T> getCustomData)
        {
           this.Name = Name;
           this.getCustomData = getCustomData;
        }
    }
    /// <summary>
    /// a shared buffer, whose index's are independent from the tiles. can enlarge dynamically
    /// </summary>
    public class DynamicBufferData<T> : IBufferData where T : struct
    {
        /// <summary>
        /// a function that returns your custom data for each sphere tile
        /// </summary>
        public readonly BufferFunction<T> getCustomData;
        /// <summary>
        /// the name of this buffer, in your custom shader
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the size of each item in the buffer. 
        /// </summary>
        public readonly int ItemLength;
        /// <summary>
        /// the size of the data being stored, in bytes
        /// </summary>
        public readonly int Size;
        /// <summary>
        /// a custom buffer configuration, which tells the sphere manager to add a new compute buffer
        /// </summary>
        /// <param name="Name">the name of this buffer, in your custom shader</param>
        /// <param name="ItemLength">the size of each item in the buffer. </param>
        /// <param name="getCustomData">a function that returns your custom data for each sphere tile</param>
        public DynamicBufferData(string Name, int ItemLength, BufferFunction<T> getCustomData)
        {
            this.Name = Name;
            Size = Marshal.SizeOf<T>();
            this.ItemLength = ItemLength;
            this.getCustomData = getCustomData;
        }
        /// <inheritdoc/>
        public IBuffer GetBuffer(ManagerRoot Manager)
        {
            GraphicsBuffer Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Manager.TotalTiles * ItemLength, Size);
            Manager.Material.SetBuffer(Name, Buffer);
            return new WrappedMultiBuffer<T>(Buffer, getCustomData, ItemLength, Manager.TotalTiles, Name, Manager.Material);
        }
    }
    /// <summary>
    /// a interface so the manager can import custom buffers of different types
    /// </summary>
    public interface IBufferData {
        /// <summary>
        /// adds a custom buffer to a manager and returns it
        /// </summary>
        IBuffer GetBuffer(ManagerRoot ManagerData);
        /// <summary>
        /// the name of the custom buffer
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// a custom buffer configuration, which tells the sphere manager to add a new graphics buffer
    /// </summary>
    public class CustomBufferData<T> : IBufferData where T : struct
    {
        /// <summary>
        /// a function that returns your custom data for each sphere tile
        /// </summary>
        public readonly GetCustomData<T> getCustomData;
        /// <summary>
        /// the name of this buffer, in your custom shader
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the size of the data being stored, in bytes
        /// </summary>
        public readonly int Size;
        /// <summary>
        /// a custom buffer configuration, which tells the sphere manager to add a new compute buffer
        /// </summary>
        /// <param name="Name">the name of this buffer, in your custom shader</param>
        /// <param name="getCustomData">a function that returns your custom data for each sphere tile</param>
        public CustomBufferData(string Name, GetCustomData<T> getCustomData)
        {
            this.Name = Name;
            Size = Marshal.SizeOf<T>();
            this.getCustomData = getCustomData;
        }
        /// <inheritdoc/>
        public IBuffer GetBuffer(ManagerRoot manager)
        {
            return new WrappedBuffer<T>(new Buffer<T>(GraphicsBuffer.Target.Structured, manager.TotalTiles, manager.Material, Name), getCustomData);
        }
    }
    public struct Range
    {
        public int Min;
        public int Max;
        public Range(int Min, int Max)
        {
            this.Min = Min;
            this.Max = Max;
        }
    }
    /// <summary>
    /// the mode which indicates how tiles are displayed
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// only the colors are displayed on each tile
        /// </summary>
        ColorOnly = 0,
        /// <summary>
        /// only the textures are displayed on each tile
        /// </summary>
        TextureOnly = 1,
        /// <summary>
        /// the textures are displayed, and the color is applied over them
        /// </summary>
        ColoredTexture = 2,
        /// <summary>
        /// the color is added to the texture
        /// </summary>
        ColorAndTexture = 3
    }
    /// <summary>
    /// a function that reads/writes the Buffer at Index*ItemSize with the data of the item at Index, this is used for multi buffers. ItemSize is the size of each item in the buffer
    /// </summary>
    /// <remarks>Index, the index of the item in the buffer</remarks>
    public delegate void BufferFunction<T>(int Index, NativeArray<T> Buffer, int ItemSize) where T : struct;
    /// <summary>
    /// a function that returns a custom data
    /// </summary>
    /// <remarks>Index, the index of the tile in the manager</remarks>
    public delegate T GetCustomData<T>(int Index) where T : struct;
    /// <summary>
    /// converts a X,Y position and height to a position on the Sphere
    /// </summary>
    public delegate Vector3 GetSphereTilePosition(SphereManager Manager, float x, float y, float height);
    /// <summary>
    /// converts a X,Y position and height to a position on the Sphere
    /// </summary>
    public delegate Vector3 GetDynamicPosition(DynamicManager Manager, int Index);
    /// <summary>
    /// the scale of a spheretile, called everytime its Row updates it
    /// </summary>
    public delegate Vector3 GetSphereTileScale<T>(T SphereTile) where T : TileBase;
    /// <summary>
    /// the Index of a spheretiles texture in the sphere managers texture array
    /// </summary>
    public delegate int GetSphereTileTexture(SphereTile SphereTile);
    /// <summary>
    /// if true, textures will be displayed on all tiles, if false only colors will be displayed
    /// </summary>
    public delegate DisplayMode GetDisplayMode();
    /// <summary>
    /// the Range of Rows around the camera that draw their tiles
    /// </summary>
    public delegate void GetCameraRange(SphereManager SphereManager, out Range Rows, out Range Cols);
    /// <summary>
    /// the Range of Rows around the camera that draw their tiles
    /// </summary>
    public delegate void GetCameraRangeDynamic(DynamicManager SphereManager, out Range Rows);
    /// <summary>
    /// called once the manager is created
    /// </summary>
    public delegate void Initiation(SphereManager Manager);
    /// <summary>
    /// called once the manager is created
    /// </summary>
    public delegate void DynamicInitiation(DynamicManager Manager);
    /// <summary>
    /// The Settings of a sphere manager, only used when it is created, you cannot update it after
    /// </summary>
    /// <remarks>every single setting must be set! default settings are stored in <see cref="DefaultSettings"/></remarks>
    public class SphereManagerSettings : ManagerSettings<SphereTile>
    {
        /// <summary>
        /// the Index of a spheretiles texture in the sphere managers texture array
        /// </summary>
        public GetSphereTileTexture GetSphereTileTexture;
        /// <summary>
        /// the array of textures the manager can display, a spheretile displays a texture at index getspheretiletexture
        /// </summary>
        public Texture2DArray TextureArray;
        /// <summary>
        /// the Range of Rows around the camera that draw their tiles
        /// </summary>
        public GetCameraRange GetCameraRange;
        /// <summary>
        /// called once the manager is created
        /// </summary>
        public Initiation Initiation;
        /// <summary>
        /// Settings for a sphere manager
        /// </summary>
        /// <param name="Initiation">the function called when the sphere manager is created</param>
        /// <param name="getSphereTileScale">gets the scale of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileTexture">how the textures and colors are rendered on each tile, called everytime they are drawn</param>
        /// <param name="getdisplaymode">if true, tiles will display their textures, otherwise they only display their color, called everytime tiles are rendered</param>
        /// <param name="Textures">an array of textures that can be displayed, every texture must have same format provided and size</param>
        /// <param name="Format">The Texture format</param>
        /// <param name="mesh">the spheretile mesh, default quad</param>
        /// <param name="material">The Material of the Mesh</param>
        /// <param name="getCameraRange">gets the range of rows that are displayede around the cameraparam</param>
        /// <param name="custombuffers">a list of custom buffers this manager's shader has (Optional)</param>
        /// <remarks>the Material MUST have the compound sphere shader or another shader with same functionality</remarks>
        public SphereManagerSettings(Initiation Initiation, ComputeShader MatrixCalculater, string matrixKernel, string colorkernel, GetSphereTileScale<SphereTile> getSphereTileScale, GetSphereTileTexture getSphereTileTexture, GetDisplayMode getdisplaymode, Texture2D[] Textures, TextureFormat Format, Mesh mesh, Material material, GetCameraRange getCameraRange, List<IBufferData> custombuffers = null)
        {
            this.ComputeShader = MatrixCalculater;
            this.MatrixKernel = matrixKernel;
            ColorKernel = colorkernel;
            GetSphereTileScale = getSphereTileScale;
            GetSphereTileTexture = getSphereTileTexture;
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            TextureArray = new Texture2DArray(Textures[0].width, Textures[0].height, Textures.Length, Format, true, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat
            };
            for (int i = 0; i < Textures.Length; i++)
            {
                TextureArray.SetPixels32(Textures[i].GetPixels32(), i);
            }
            TextureArray.Apply();
            CustomBuffers = custombuffers;
        }
        /// <summary>
        /// Settings for a sphere manager
        /// </summary>
        /// <param name="Initiation">the function called when the sphere manager is created</param>
        /// <param name="getSphereTileScale">gets the scale of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileTexture">gets the index of a texture in the Textures Array, of the tile, called everytime it is refreshed</param>
        /// <param name="getdisplaymode">how the textures and colors are rendered on each tile, called everytime they are drawn</param>
        /// <param name="Textures">an array of textures that can be displayed, you must create this and apply it first</param>
        /// <param name="mesh">the spheretile mesh, default quad</param>
        /// <param name="material">The Material of the Mesh</param>
        /// <param name="custombuffers">a list of custom buffers this manager's shader has (Optional)</param>
        /// <param name="getCameraRange">gets the range of rows that are displayede around the cameraparam</param>
        /// <remarks>the Material MUST have the compound sphere shader or another shader with same functionality</remarks>
        public SphereManagerSettings(Initiation Initiation, ComputeShader MatrixCalculater, string matrixkernel, string colorkernel, GetSphereTileScale<SphereTile> getSphereTileScale, GetSphereTileTexture getSphereTileTexture, GetDisplayMode getdisplaymode, Texture2DArray Textures, Mesh mesh, Material material, GetCameraRange getCameraRange, List<IBufferData> custombuffers = null)
        {
            this.ComputeShader = MatrixCalculater;
            this.MatrixKernel = matrixkernel;
            ColorKernel = colorkernel;
            GetSphereTileScale = getSphereTileScale;
            GetSphereTileTexture = getSphereTileTexture;
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            TextureArray = Textures;
            CustomBuffers = custombuffers;
        }
    }
    public class DynamicManagerSettings : ManagerSettings<DynamicTile>
    {
        public ComputeShader Culler;
        public GetCameraRangeDynamic GetCameraRange;
        public Texture2D Atlas;
        public DynamicInitiation Initiation;
        public GetDynamicPosition GetTilePosition;
        public Rect[] UVs;
        public DynamicManagerSettings(DynamicInitiation Initiation, ComputeShader MatrixCalculater, GetSphereTileScale<DynamicTile> getSphereTileScale, GetDisplayMode getdisplaymode, Texture2D Atlas, Rect[] UVs, Mesh mesh, Material material, ComputeShader Culler, GetCameraRangeDynamic getCameraRange, List<IBufferData> custombuffers = null)
        {
            this.ComputeShader = MatrixCalculater;
            GetSphereTileScale = getSphereTileScale;
            
            
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            this.GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            this.Culler = Culler;
            this.UVs = UVs;
            this.Atlas = Atlas;
            CustomBuffers = custombuffers;
        }
        public DynamicManagerSettings(DynamicInitiation Initiation, ComputeShader MatrixCalculater, GetSphereTileScale<DynamicTile> getSphereTileScale, GetDisplayMode getdisplaymode, Texture2D[] Textures, Mesh mesh, Material material, ComputeShader Culler, GetCameraRangeDynamic getCameraRange, List<IBufferData> custombuffers = null)
        {
            this.ComputeShader = MatrixCalculater;
            GetSphereTileScale = getSphereTileScale;
           
            
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            this.GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            this.Culler = Culler;
            CustomBuffers = custombuffers;
            Atlas = new Texture2D(1, 1);
            UVs = Atlas.PackTextures(Textures, 2, 4096);
        }
    }
    public class ManagerSettings<T> where T : TileBase
    {
        /// <summary>
        /// the mesh used to display tiles
        /// </summary>
        public Mesh SphereTileMesh;
        /// <summary>
        /// the Material of the meshes, must have the compound spheres shader to work!
        /// </summary>
        public Material SphereTileMaterial;
        /// <summary>
        /// a list of custom buffers this manager's shader has (Optional)
        /// </summary>
        public List<IBufferData> CustomBuffers;
        /// <summary>
        /// how the textures and colors are rendered on each tile, called everytime they are drawn
        /// </summary>
        public GetDisplayMode GetDisplayMode;
        /// <summary>
        /// the scale of a spheretile, called everytime its Row updates it
        /// </summary>
        public GetSphereTileScale<T> GetSphereTileScale;
        /// <summary>
        /// 
        /// </summary>
        public ComputeShader ComputeShader;
        public string MatrixKernel;
        public string ColorKernel;
    }
}