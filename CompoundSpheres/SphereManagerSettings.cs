using UnityEngine;

namespace CompoundSpheres
{
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
        ColoredTexture = 2
    }
    /// <summary>
    /// converts a X,Y position and height to a position on the Sphere
    /// </summary>
    public delegate Vector3 GetSphereTilePosition(SphereManager Manager, float x, float y, float height);
    /// <summary>
    /// the rotation of any spheretile, position is made then the rotation is made
    /// </summary>
    public delegate Quaternion GetSphereTileRotation(SphereTile SphereTile);
    /// <summary>
    /// the scale of a spheretile, called everytime its Row updates it
    /// </summary>
    public delegate Vector3 GetSphereTileScale(SphereTile SphereTile);
    /// <summary>
    /// the Index of a spheretiles texture in the sphere managers texture array
    /// </summary>
    public delegate int GetSphereTileTexture(SphereTile SphereTile);
    /// <summary>
    /// the color of a spheretile
    /// </summary>
    public delegate Color GetSphereTileColor(SphereTile sphereTile);
    /// <summary>
    /// if true, textures will be displayed on all tiles, if false only colors will be displayed
    /// </summary>
    public delegate DisplayMode GetDisplayMode(SphereManager SphereManager);
    /// <summary>
    /// the Range of Rows around the camera that draw their tiles
    /// </summary>
    public delegate void GetCameraRange(SphereManager SphereManager, out int Min, out int Max);
    /// <summary>
    /// called once the manager is created
    /// </summary>
    public delegate void Initiation(SphereManager Manager);
    /// <summary>
    /// The Settings of a sphere manager, only used when it is created, you cannot update it after
    /// </summary>
    /// <remarks>every single setting must be set! default settings are stored in <see cref="DefaultSettings"/></remarks>
    public struct SphereManagerSettings
    {
        /// <summary>
        /// converts a X,Y position and height to a position on the Sphere
        /// </summary>
        public GetSphereTilePosition getspheretileposition;
        /// <summary>
        /// the rotation of any spheretile, position is made then the rotation is made
        /// </summary>
        public GetSphereTileRotation GetSphereTileRotation;
        /// <summary>
        /// the scale of a spheretile, called everytime its Row updates it
        /// </summary>
        public GetSphereTileScale GetSphereTileScale;
        /// <summary>
        /// the Index of a spheretiles texture in the sphere managers texture array
        /// </summary>
        public GetSphereTileTexture GetSphereTileTexture;
        /// <summary>
        /// the color of a spheretile
        /// </summary>
        public GetSphereTileColor GetSphereTileColor;
        /// <summary>
        /// the mesh used to display tiles
        /// </summary>
        public Mesh SphereTileMesh;
        /// <summary>
        /// the Material of the meshes, must have the compound spheres shader to work!
        /// </summary>
        public Material SphereTileMaterial;
        /// <summary>
        /// how the textures and colors are rendered on each tile, called everytime they are drawn
        /// </summary>
        public GetDisplayMode GetDisplayMode;
        /// <summary>
        /// the Range of Rows around the camera that draw their tiles
        /// </summary>
        public GetCameraRange GetCameraRange;
        /// <summary>
        /// the array of textures the manager can display, a spheretile displays a texture at index getspheretiletexture
        /// </summary>
        public Texture2DArray TextureArray;
        /// <summary>
        /// called once the manager is created
        /// </summary>
        public Initiation Initiation;
        /// <summary>
        /// Settings for a sphere manager
        /// </summary>
        /// <param name="Initiation">the function called when the sphere manager is created</param>
        /// <param name="getSphereTilePosition">converts a x,y coordinate on a grid and a Height variable to a coordinate on the sphere</param>
        /// <param name="getSphereTileRotation">gets the rotation of a sphere tile, called once</param>
        /// <param name="getSphereTileScale">gets the scale of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileColor">gets the color of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileTexture">how the textures and colors are rendered on each tile, called everytime they are drawn</param>
        /// <param name="getdisplaymode">if true, tiles will display their textures, otherwise they only display their color, called everytime tiles are rendered</param>
        /// <param name="Textures">an array of textures that can be displayed, every texture must have same format provided and size</param>
        /// <param name="Format">The Texture format</param>
        /// <param name="mesh">the spheretile mesh, default quad</param>
        /// <param name="material">The Material of the Mesh</param>
        /// <param name="getCameraRange">gets the range of rows that are displayede around the cameraparam</param>
        /// <remarks>the Material MUST have the compound sphere shader or another shader with same functionality</remarks>
        public SphereManagerSettings(Initiation Initiation, GetSphereTilePosition getSphereTilePosition, GetSphereTileRotation getSphereTileRotation, GetSphereTileScale getSphereTileScale, GetSphereTileColor getSphereTileColor, GetSphereTileTexture getSphereTileTexture, GetDisplayMode getdisplaymode, Texture2D[] Textures, TextureFormat Format, Mesh mesh, Material material, GetCameraRange getCameraRange)
        {
            getspheretileposition = getSphereTilePosition;
            GetSphereTileRotation = getSphereTileRotation;
            GetSphereTileScale = getSphereTileScale;
            GetSphereTileTexture = getSphereTileTexture;
            GetSphereTileColor = getSphereTileColor;
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            TextureArray = new Texture2DArray(Textures[0].width, Textures[0].height, Textures.Length, Format, true, false);
            TextureArray.filterMode = FilterMode.Bilinear;
            TextureArray.wrapMode = TextureWrapMode.Repeat;
            for (int i = 0; i < Textures.Length; i++)
            {
                TextureArray.SetPixels32(Textures[i].GetPixels32(), i);
            }
            TextureArray.Apply();
        }
        /// <summary>
        /// Settings for a sphere manager
        /// </summary>
        /// <param name="Initiation">the function called when the sphere manager is created</param>
        /// <param name="getSphereTilePosition">converts a x,y coordinate on a grid and a Height variable to a coordinate on the sphere</param>
        /// <param name="getSphereTileRotation">gets the rotation of a sphere tile, called once</param>
        /// <param name="getSphereTileScale">gets the scale of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileColor">gets the color of the tile, called everytime it is refreshed</param>
        /// <param name="getSphereTileTexture">gets the index of a texture in the Textures Array, of the tile, called everytime it is refreshed</param>
        /// <param name="getdisplaymode">how the textures and colors are rendered on each tile, called everytime they are drawn</param>
        /// <param name="Textures">an array of textures that can be displayed, you must create this and apply it first</param>
        /// <param name="mesh">the spheretile mesh, default quad</param>
        /// <param name="material">The Material of the Mesh</param>
        /// <param name="getCameraRange">gets the range of rows that are displayede around the cameraparam</param>
        /// <remarks>the Material MUST have the compound sphere shader or another shader with same functionality</remarks>
        public SphereManagerSettings(Initiation Initiation, GetSphereTilePosition getSphereTilePosition, GetSphereTileRotation getSphereTileRotation, GetSphereTileScale getSphereTileScale, GetSphereTileColor getSphereTileColor, GetSphereTileTexture getSphereTileTexture, GetDisplayMode getdisplaymode, Texture2DArray Textures, Mesh mesh, Material material, GetCameraRange getCameraRange)
        {
            getspheretileposition = getSphereTilePosition;
            GetSphereTileRotation = getSphereTileRotation;
            GetSphereTileScale = getSphereTileScale;
            GetSphereTileTexture = getSphereTileTexture;
            GetSphereTileColor = getSphereTileColor;
            SphereTileMesh = mesh;
            SphereTileMaterial = material;
            GetDisplayMode = getdisplaymode;
            GetCameraRange = getCameraRange;
            this.Initiation = Initiation;
            TextureArray = Textures;
        }
    }
}