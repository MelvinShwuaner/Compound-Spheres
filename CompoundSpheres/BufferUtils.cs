﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// a interface meant so the manager can control custom buffers of different types
    /// </summary>
    public interface IBuffer
    {
        /// <summary>
        /// disposes the custom buffer
        /// </summary>
        public void Dispose();
        /// <summary>
        /// marks a tile to be refreshed
        /// </summary>
        public void Update(int I);
        /// <summary>
        /// refreshes the buffer
        /// </summary>
        public void Refresh();
    }
    /// <summary>
    /// a class for managing a buffer efficiently
    /// </summary>
    public class CustomBuffer<T> : IBuffer where T : struct
    {
        /// <summary>
        /// the manager managing this custom buffer
        /// </summary>
        public SphereManager Manager;
        /// <summary>
        /// the buffer this is managing
        /// </summary>
        public GraphicsBuffer Buffer;
        readonly HashSet<int> ToUpdate;
        readonly GetCustomData<T> getCustomData;
        /// <summary>
        /// refreshes all of the data
        /// </summary>
        public void Refresh()
        {
            Buffer.UpdateBuffer(ToUpdate, (int i) => getCustomData(Manager.SphereTiles[i]));
        }
        internal CustomBuffer(SphereManager Manager, GraphicsBuffer Buffer, GetCustomData<T> getdata)
        {
            getCustomData = getdata;
            this.Manager = Manager;
            ToUpdate = new HashSet<int>();
            this.Buffer = Buffer;
        }
        /// <inheritdoc/>
        public void Update(int I)
        {
            ToUpdate.Add(I);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
    /// <summary>
    /// a class for managing buffers
    /// </summary>
    public static class BufferUtils
    {
        /// <summary>
        /// sets a buffer, updating values in the list ToUpdate, NOT efficent for updating buffers, only call this to create buffers
        /// </summary>
        /// <remarks>calling this function many times at once may lead to lag</remarks>
        public static void SetBuffer<T>(this GraphicsBuffer Buffer, int Count, Func<int, T> function) where T : struct
        {
            T[] Array = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                Array[i] = function(i);
            }
            Buffer.SetData(Array);
        }
        /// <summary>
        /// Updates a buffer
        /// </summary>
        public static void UpdateBuffer<T>(this GraphicsBuffer buffer, HashSet<int> ToUpdate, Func<int, T> Function) where T : struct
        {
            if (ToUpdate == null || ToUpdate.Count == 0) return;

            var sorted = UnityEngine.Pool.ListPool<int>.Get();
            sorted.AddRange(ToUpdate);
            sorted.Sort();

            T[] Array = new T[ToUpdate.Count];
            Parallel.For(0, ToUpdate.Count, (int i) => Array[i] = Function(sorted[i]) );
            int BufferSize = 1;
            int ArrayStart = 0;
            int startIndex = sorted[0];
            int lastIndex = startIndex;
            for (int i = 1; i < sorted.Count; i++)
            {
                int index = sorted[i];
                if (index-lastIndex == 1)
                {
                    BufferSize++;
                }
                else
                {
                    buffer.SetData(Array, ArrayStart, startIndex, BufferSize);
                    startIndex = index;
                    ArrayStart = i;
                    BufferSize = 1;
                }
                lastIndex = index;
            }
            if (BufferSize > 0)
            {
                buffer.SetData(Array, ArrayStart, startIndex, BufferSize);
            }
            ToUpdate.Clear();
            UnityEngine.Pool.ListPool<int>.Release(sorted);
        }
    }
}
