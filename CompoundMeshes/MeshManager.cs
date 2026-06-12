using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundMeshes
{
    public class MeshManager : MonoBehaviour
    {
        public ComputeShader ComputeShader { get; protected set; }
        /// <summary>
        /// The material used, every tile has the same material
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use addcustombuffer, since the manager will manage the buffer for you</remarks>
        public Material Material { protected set; get; }
        /// <summary>
        /// the total amount of tiles
        /// </summary>
        public int MeshCount;
        /// <summary>
        /// The Mesh used, every tile has the same mesh
        /// </summary>
        public Mesh Mesh { protected set; get; }
        Dictionary<string, IBuffer> Buffers = new Dictionary<string, IBuffer>();
        protected virtual void OnDestroy()
        {
            foreach (var buffer in Buffers)
            {
                buffer.Value.Dispose();
            }
            Handler.Dispose();
        }
        public MeshHandler Handler;
        MeshManager Init(ManagerSettings Settings)
        {
            Mesh = Settings.Mesh;
            Material = Settings.Material;
            ComputeShader = Settings.ComputeShader;
            Handler = Settings.Handler;
            Handler.Prepare(this);
            foreach (IBufferData buffer in Settings.Buffers)
            {
                AddBuffer(buffer);
            }
            return this;
        }
        public void SetComputeProperty(string name, float value)
        {
            ComputeShader.SetFloat(name, value);
        }
        public void SetComputeProperty(string name, int value)
        {
            ComputeShader.SetInt(name, value);
        }
        public void SetComputeProperty(string kernel, string name, Texture value)
        {
            ComputeShader.SetTexture(ComputeShader.FindKernel(kernel), name, value);
        }
        public void SetProperty(string name,  float value)
        {
            Material.SetFloat(name, value);
        }
        public void SetProperty(string name, Texture value)
        {
            Material.SetTexture(name, value);
        }
        /// <summary>
        /// destroys the manager and its game object, and frees up all memory
        /// </summary>
        public void Destroy()
        {
            Destroy(gameObject);
        }
        /// <summary>
        /// a non-generic method of adding a buffer
        /// </summary>
        public IBuffer AddBuffer(IBufferData data)
        {
            IBuffer buffer = data.GetBuffer(this);
            Buffers.Add(data.Name, buffer);
            return buffer;
        }
        /// <summary>
        /// refresh all of the scales, textures and colors, matrixes
        /// </summary>
        public void RefreshAll()
        {
            foreach (var buffer in Buffers)
            {
                buffer.Value.Refresh();
            }
        }
        public void Refresh(string Name)
        {
            Buffers[Name].Refresh();
        }
        public void Update(string Name, int Index)
        {
            Buffers[Name].Update(Index);
        }
        public void UpdateAll(int Index)
        {
            foreach (var buffer in Buffers)
            {
                buffer.Value.Update(Index);
            }
        }
        public void SetSize(int Size)
        {
            foreach (var buffer in Buffers)
            {
                buffer.Value.Enlarge(Size);
            }
        }
        public void DrawMeshes()
        {
            Handler.DrawMeshes();
        }
        public static MeshManager CreateMeshManager(ManagerSettings Settings, string name = "Mesh Manager")
        {
            MeshManager Manager = new GameObject(name).AddComponent<MeshManager>().Init(Settings);
            return Manager;
        }
    }
    public interface MeshHandler : IDisposable
    {
        public void Prepare(MeshManager Manager);
        public void DrawMeshes();
    }
}
