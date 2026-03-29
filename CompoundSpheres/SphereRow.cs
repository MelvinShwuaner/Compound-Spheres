using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
namespace CompoundSpheres
{
    /// <summary>
    /// sphere rows control the displaying of tiles
    /// </summary>
    public class SphereRow : IEnumerable
    {
        /// <summary>
        /// the manager of this row
        /// </summary>
        public readonly SphereManager SphereManager;
        /// <summary>
        /// get a sphere tile at this row and column i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public SphereTile this[int i] => SphereManager[Row, i];
        /// <summary>
        /// the number of tiles in this row
        /// </summary>
        public int Cols => SphereManager.Cols;
        /// <summary>
        /// the X coordinate of this row
        /// </summary>
        public readonly int Row;
        /// <summary>
        /// the material properties for this specific row
        /// </summary>
        /// <remarks>dont add custom buffers directly to this, instead use Manager.addcustombuffer, since the manager will manage the buffer for you </remarks>
        public MaterialPropertyBlock Properties => _rp.matProps;
        internal SphereRow(SphereManager manager, int Row)
        {
            SphereManager = manager;
            this.Row = Row;
            _rp = new RenderParams(manager.Material)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
                matProps = new MaterialPropertyBlock()
            };
            Properties.SetInteger("Row", Row * Cols);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Cols; i++)
            {
                yield return this[i];
            }
        }
        /// <summary>
        /// draw the spheretiles
        /// </summary>
        public void DrawTiles()
        {
            Graphics.RenderMeshIndirect(_rp, SphereManager.SphereTileMesh, SphereManager.commandBuf, 1);
        }
        private RenderParams _rp;
    }
}