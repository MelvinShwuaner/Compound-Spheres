using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundSpheres
{
    public class DynamicTile : TileBase
    {
        public DynamicRow? DynamicRow { get; internal set; } = null;

        public readonly DynamicManager Manager;
        public int Row => DynamicRow?.Row ?? -1;
        public int Col { get; internal set; } = -1;
        public DynamicTile(int Index, DynamicManager Manager) : base(Index)
        {
            this.Manager = Manager;
        }


        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public override Vector3 UpdateScale()
        {
            Scale = Manager.SphereTileScale(this);
            return Scale;
        }
    }
}
