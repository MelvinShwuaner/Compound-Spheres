using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// stores default settings
    /// </summary>
    public static class DefaultSettings
    {
        /// <summary>
        /// maps a x,y coordinate on a grid to a cylndrical coordinate, with height increasing the radius, X being PHI and y being the Z coordinate
        /// </summary>
        public static Vector3 CartesianToCylindrical(SphereManager manager, float X, float Y, float Height = 0)
        {
            float phi = X / manager.Rows * (2f * Mathf.PI);
            float x = (manager.Radius+Height) * Mathf.Cos(phi);
            float y = (manager.Radius+Height) * Mathf.Sin(phi);
            float z = Y;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rotates objects on a cylinder away from their center, the center having the same Z coordinate
        /// </summary>
        public static Quaternion CylindricalRotation(SphereTile SphereTile)
        {
            return Quaternion.LookRotation((Vector2)SphereTile.Position) * Quaternion.Euler(0, -180, 0);
        }
        /// <summary>
        /// creates a cylinder with diamater as the manager and length as the managers Cols
        /// </summary>
        /// <param name="Manager"></param>
        public static void CylindricalInitiation(SphereManager Manager)
        {
            GameObject Cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Cylinder.transform.SetPositionAndRotation(new Vector3(0, 0, Manager.Cols / 2), Quaternion.Euler(-90, 0, 0));
            Cylinder.transform.localScale = new Vector3(Manager.Diameter, Manager.Cols / 2, Manager.Diameter);
            Cylinder.transform.parent = Manager.transform;
        }
        /// <summary>
        /// always display colored textures by default
        /// </summary>
        public static DisplayMode DefaultMode(SphereManager Manager) { return DisplayMode.ColoredTexture; }
        /// <summary>
        /// by default, render one half of the cylinder (which is facing the camera)
        /// </summary>
        public static void DefaultRange(SphereManager SphereManager, out int Min, out int Max) { Min = -(SphereManager.Rows / 4); Max = SphereManager.Rows / 4; }
        /// <summary>
        /// default tile size is One
        /// </summary>
        public static Vector3 DefaultScale(SphereTile Tile) { return Vector3.one; }
        /// <summary>
        /// default tile color is white
        /// </summary>
        public static Color DefaultColor(SphereTile Tile)
        {
            return Color.white;
        }
        /// <summary>
        /// default format in textures
        /// </summary>
        public static readonly TextureFormat DefaultFormat = TextureFormat.RGBA32;
        /// <summary>
        /// default texture is a circle
        /// </summary>
        public static Texture2D DefaultTexture(SphereTile Tile) { return Resources.Load<Texture2D>("Library/unity default resources/box"); }
        /// <summary>
        /// the default mesh on the sphere
        /// </summary>
        public static Mesh DefaultMesh { get { return Resources.Load<Mesh>("Library/unity default resources/Quad"); } }
    }
}