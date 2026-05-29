using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// An interface meant so the manager can control custom buffers of different types
    /// </summary>
    public interface IBuffer : IDisposable
    {
        /// <summary>
        /// updates a item
        /// </summary>
        public void Update(int I);
        /// <summary>
        /// refreshes the buffer
        /// </summary>
        public void Refresh();
    }
    public abstract class BufferBase<T> : IDisposable where T : struct
    {
        public GraphicsBuffer buffer { get; private set; }
        public readonly Material Material;
        public readonly MaterialPropertyBlock Property;
        public readonly string Name;
        protected NativeArray<T> Data;
        protected bool[] Dirty;
        protected bool IsDirty = false;
        public int Size => Data.Length;
        public BufferBase(GraphicsBuffer Buffer, Material material, string name, int Length)
        {
            buffer = Buffer;
            Material = material;
            Name = name;
            Material?.SetBuffer(Name, buffer);
            Dirty = new bool[Length];
            Data = new NativeArray<T>(Length, Allocator.Persistent);
        }
        public BufferBase(GraphicsBuffer Buffer, MaterialPropertyBlock material, string name, int Length)
        {
            buffer = Buffer;
            Property = material;
            Name = name;
            Property.SetBuffer(Name, buffer);
            Dirty = new bool[Length];
            Data = new NativeArray<T>(Length, Allocator.Persistent);
        }
        public void Dispose()
        {
            buffer.Dispose();
        }
        protected void SetData(int Start, int Count)
        {
            buffer.SetData(Data, Start, Start, Count);
        }
        public void Enlarge(int NewSize)
        {
            GraphicsBuffer newBuffer = new GraphicsBuffer(buffer.target, NewSize, Marshal.SizeOf<T>());
            NativeArray<T> temp = new NativeArray<T>(NewSize, Allocator.Temp);

            bool[] dirty = new bool[NewSize];
            Dirty.CopyTo(dirty, 0);
            Dirty = dirty;

            Data.CopyTo(temp);
            buffer.Dispose();
            buffer = newBuffer;
            buffer.SetData(temp);
            Data.Dispose();
            Data = temp;
            if (Material != null)
            {
                Material.SetBuffer(Name, buffer);
            }
            else
            {
                Property.SetBuffer(Name, buffer);
            }
        }
    }
    public class MultiBuffer<T> : BufferBase<T> where T : struct
    {
        public MultiBuffer(GraphicsBuffer.Target target, int Length, int ItemSize, Material material, string name) : base(new GraphicsBuffer(target, Length*ItemSize, Marshal.SizeOf<T>()), material, name, Length*ItemSize) { this.ItemSize = ItemSize; }
        public MultiBuffer(GraphicsBuffer.Target target, int Length, int ItemSize, MaterialPropertyBlock material, string name) : base(new GraphicsBuffer(target, Length*ItemSize, Marshal.SizeOf<T>()), material, name, Length*ItemSize) { this.ItemSize = ItemSize; }
        public MultiBuffer(GraphicsBuffer Buffer, Material material, string name, int length, int ItemSize) : base(Buffer, material, name, length*ItemSize) { this.ItemSize = ItemSize; }
        public readonly int ItemSize = 1;
        void Check(int index)
        {
            if ((index+1) * ItemSize > Size)
            {
                Enlarge((index+1) * 2 * ItemSize);
            }
        }
        /// <summary>
        /// writes to the Data Array using a Function. 
        /// </summary>
        public void Write(int Index, BufferFunction<T> Function)
        {
            Check(Index);
            Dirty[Index] = true;
            IsDirty = true;
            Function(Index, Data, ItemSize);
        }
        /// <summary>
        /// Reads from the Data Array using a Function. 
        /// </summary>
        public void Read(int Index, BufferFunction<T> Function)
        {
            Check(Index);
            Function(Index, Data, ItemSize);
        }
        /// <summary>
        /// Updates a Buffer
        /// </summary>
        public void Refresh()
        {
            if (!IsDirty) return;

            int bufferSize = 0;
            int arrayStart = -1;
            int lastIndex = -1;
            for (int i = 0; i < Data.Length; i++)
            {
                if (!Dirty[i]) continue;

                if (arrayStart == -1)
                {
                    arrayStart = i;
                    bufferSize = 1;
                    lastIndex = i;
                }
                else if (i - lastIndex == 1)
                {
                    bufferSize++;
                    lastIndex = i;
                }
                else
                {
                    SetData(arrayStart * ItemSize, bufferSize * ItemSize);
                    arrayStart = i;
                    bufferSize = 1;
                    lastIndex = i;
                }

                Dirty[i] = false;
            }

            if (arrayStart != -1)
                SetData(arrayStart * ItemSize, bufferSize * ItemSize);

            IsDirty = false;
        }
    }
    public class Buffer<T> : BufferBase<T>, IDisposable where T : struct
    {
        public T this[int Index]
        {
            get
            {
                return Data[Index];
            }
            set
            {
                MarkDirty(Index);
                Data[Index] = value;
            }
        }
        void MarkDirty(int index) {
            if(index >= Size)
            {
                Enlarge(index * 2);
            }
            IsDirty = true;
            Dirty[index] = true;
        }
        public Buffer(GraphicsBuffer.Target target, int Length, Material material, string name) : base(new GraphicsBuffer(target, Length, Marshal.SizeOf<T>()), material, name, Length) { }
        public Buffer(GraphicsBuffer.Target target, int Length, MaterialPropertyBlock material, string name) : base(new GraphicsBuffer(target, Length, Marshal.SizeOf<T>()), material, name, Length) { }

        public void Refresh()
        {
            if(!IsDirty) return;

            int bufferSize = 0;
            int arrayStart = -1;
            int lastIndex = -1;

            for (int i = 0; i < Data.Length; i++)
            {
                if (!Dirty[i]) continue;

                if (arrayStart == -1)
                {
                    arrayStart = i;
                    bufferSize = 1;
                    lastIndex = i;
                }
                else if (i - lastIndex == 1)
                {
                    bufferSize++;
                    lastIndex = i;
                }
                else
                {
                    SetData(arrayStart, bufferSize);
                    arrayStart = i;
                    bufferSize = 1;
                    lastIndex = i;
                }

                Dirty[i] = false;
            }

            if (arrayStart != -1)
                SetData(arrayStart, bufferSize);

            IsDirty = false;
        }
        /// <summary>
        /// sets a buffer, updating values in the Data Array, NOT efficent for updating buffers, only call this to create buffers
        /// </summary>
        public void Set(Func<int, T> function)
        {
            Parallel.For(0, Size, (int i) =>
            {
                Data[i] = function(i);
            });
            buffer.SetData(Data);
        }
    }
    /// <summary>
    /// a class for managing a buffer efficiently and dynamically
    /// </summary>
    public class WrappeMultiBuffer<T> : IBuffer where T : struct
    {
        /// <summary>
        /// the buffer this is managing
        /// </summary>
        public MultiBuffer<T> Buffer;
        readonly BufferFunction<T> getCustomData;
        /// <summary>
        /// refreshes all of the data
        /// </summary>
        public void Refresh()
        {
           Buffer.Refresh();
        }
        internal WrappeMultiBuffer(GraphicsBuffer Buffer, BufferFunction<T> getdata, int ItemLength, int InitialLength, string name, Material material)
        {
            getCustomData = getdata;
            this.Buffer = new MultiBuffer<T>(Buffer, material, name, InitialLength, ItemLength);
        }
        /// <inheritdoc/>
        public void Update(int I)
        {
            Buffer.Write(I, getCustomData);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
    /// <summary>
    /// a class for managing a buffer efficiently
    /// </summary>
    public class WrappedBuffer<T> : IBuffer where T : struct
    {
        /// <summary>
        /// the buffer this is managing
        /// </summary>
        public Buffer<T> Buffer;
        readonly GetCustomData<T> getCustomData;
        /// <summary>
        /// refreshes all of the data
        /// </summary>
        public void Refresh()
        {
            Buffer.Refresh();
        }
        internal WrappedBuffer(Buffer<T> Buffer, GetCustomData<T> getdata)
        {
            getCustomData = getdata;
            this.Buffer = Buffer;
        }
        /// <inheritdoc/>
        public void Update(int I)
        {
            Buffer[I] = getCustomData(I);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
