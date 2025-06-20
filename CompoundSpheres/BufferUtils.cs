using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Update(int x, int y);
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
            Buffer.UpdateBuffer(ToUpdate, (int i) => getCustomData(Manager.SphereTiles[i]), Manager.BufferSize);
        }
        internal CustomBuffer(SphereManager Manager, GraphicsBuffer Buffer, GetCustomData<T> getdata)
        {
            getCustomData = getdata;
            this.Manager = Manager;
            ToUpdate = new HashSet<int>();
            this.Buffer = Buffer;
        }
        /// <inheritdoc/>
        public void Update(int x, int y)
        {
            ToUpdate.Add((x * Manager.Cols) + y);
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
        public static void UpdateBuffer<T>(this GraphicsBuffer buffer, HashSet<int> ToUpdate, Func<int, T> Function, int MaxDist = 1) where T : struct
        {
            if (ToUpdate == null || ToUpdate.Count == 0) return;
            var sorted = ToUpdate.OrderBy(kvp => kvp).ToList();
            List<T> currentGroup = new List<T>();
            int startIndex = sorted[0];
            int lastIndex = startIndex;
            currentGroup.Add(Function(sorted[0]));
            for (int i = 1; i < sorted.Count; i++)
            {
                int index = sorted[i];
                if (index-lastIndex <= MaxDist)
                {
                    for (int j = lastIndex + 1; j <= index; j++)
                    {
                        currentGroup.Add(Function(j));
                    }
                }
                else
                {
                    buffer.SetData(currentGroup.ToArray(), 0, startIndex, currentGroup.Count);
                    currentGroup.Clear();
                    currentGroup.Add(Function(sorted[i]));
                    startIndex = index;
                }
                lastIndex = index;
            }
            if (currentGroup.Count > 0)
            {
                buffer.SetData(currentGroup.ToArray(), 0, startIndex, currentGroup.Count);
            }
            ToUpdate.Clear();
        }
    }
}
