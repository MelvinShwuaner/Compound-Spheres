using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace CompoundSpheres
{
    public class DynamicRow : IEnumerable
    {
        int[] flags;
        HashSet<int> _enabled, indices;
        ComputeShader Culler;
        ComputeBuffer VisibleIDs;
        ComputeBuffer VisibilityFlags;
        Buffer<int> IndicesBuffer;
        int Kernel;
        /// <summary>
        /// the manager of this row
        /// </summary>
        public readonly DynamicManager Manager;
        /// <summary>
        /// get a sphere tile at this row and column i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DynamicTile this[int i] => Manager.Tiles[IndicesBuffer[i]];
        /// <summary>
        /// the number of tiles in this row
        /// </summary>
        public int Cols { get; internal set; }
        /// <summary>
        /// the X coordinate of this row
        /// </summary>
        public readonly int Row;
        /// <summary>
        /// the material properties for this specific row
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use Manager.addcustombuffer, since the manager will manage the buffer for you </remarks>
        public readonly MaterialPropertyBlock Properties = new MaterialPropertyBlock();
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        ComputeBuffer argsBuffer;
        Mesh Mesh => Manager.SphereTileMesh;
        Material Material => Manager.Material;

        internal DynamicRow(DynamicManager manager, int Row, int Cols, ComputeShader Culler)
        {
            Manager = manager;
            this.Row = Row;
            this.Cols = Cols;

            this.Culler = Culler;
            Kernel = Culler.FindKernel("CSMain");

            flags = new int[this.Cols];
            for (int i = 0; i < this.Cols; i++)
                flags[i] = 1;

            VisibilityFlags = new ComputeBuffer(this.Cols, sizeof(int));
            VisibilityFlags.SetData(flags);

            VisibleIDs = new ComputeBuffer(this.Cols, sizeof(uint),
                ComputeBufferType.Append);

            args[0] = Mesh.GetIndexCount(0);
            args[1] = 0;
            args[2] = Mesh.GetIndexStart(0);
            args[3] = (uint)Mesh.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);

            this.Culler.SetBuffer(Kernel, "_VisibilityFlags", VisibilityFlags);
            this.Culler.SetBuffer(Kernel, "_VisibleInstanceIDs", VisibleIDs);
            this.Culler.SetInt("_InstanceCount", this.Cols);

            Properties.SetBuffer("_VisibleInstanceIDs", VisibleIDs);

            IndicesBuffer = new Buffer<int>(GraphicsBuffer.Target.Structured, this.Cols, Properties, "IndicesBuffer");
        }
        public void UpdateIndices()
        {
            IndicesBuffer.Refresh();
        }
        public void SetIndice(int indice, int Index)
        {
            IndicesBuffer[indice] = Index;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Cols; i++)
            {
                yield return this[i];
            }
        }
        internal void Dispose()
        {
            VisibilityFlags?.Release();
            VisibleIDs?.Release();
            argsBuffer?.Release();
            IndicesBuffer?.Dispose();
        }
        /// <summary>
        /// draw the spheretiles
        /// </summary>
        public void DrawTiles()
        {
            VisibleIDs.SetCounterValue(0);
            Culler.Dispatch(Kernel, Mathf.CeilToInt(Cols / 64f), 1, 1);
            ComputeBuffer.CopyCount(VisibleIDs, argsBuffer, sizeof(uint));

            Graphics.DrawMeshInstancedIndirect(Mesh, 0, Material, worldBounds, argsBuffer, 0, Properties);
        }
    }
}
