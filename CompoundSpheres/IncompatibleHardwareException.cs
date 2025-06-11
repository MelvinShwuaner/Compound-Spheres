using System;

namespace CompoundSpheres
{
    /// <summary>
    /// Not all GPU'S support compute shaders or indirect mesh instancing, if that happens, throw this
    /// </summary>
    /// <remarks>
    /// compute shaders are also not supported by the built in render pipeline
    /// </remarks>
    public class IncompatibleHardwareException : Exception
    {
        ///<inheritdoc/>
        public IncompatibleHardwareException() : this("Your Hardware is not compatible with Compound Spheres! you dont have a GPU, or your GPU does not support Compute Shaders or Indirect Mesh Instancing!") { }
        ///<inheritdoc/>
        public IncompatibleHardwareException(string message) : base(message) { }
        ///<inheritdoc/>
        public IncompatibleHardwareException(string message, Exception inner) : base(message, inner) { }
    }
}