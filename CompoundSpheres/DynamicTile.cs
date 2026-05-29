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
        public DynamicRow DynamicRow { get; internal set; }

        public DynamicManager Manager => DynamicRow.Manager;
        public readonly int Index;
        public int Row => DynamicRow.Row;
        public int Col { get; internal set;  }
        public DynamicTile(int Row, int Col, DynamicRow DynamicRow)
        {
            this.DynamicRow = DynamicRow;
            this.Index = DynamicRow.Cols * Row + Col;
            this.Col = Col;
        }
        /// <summary>
        /// Updates and Returns the Color
        /// </summary>
        public override Color32 UpdateColor()
        {
            Color = Manager.SphereTileColor(this);
            return Color;
        }

        public override Matrix4x4 UpdateMatrix()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates and Returns the Scale
        /// </summary>
        public override Vector3 UpdateScale()
        {
            Scale = Manager.SphereTileScale(this);
            return Scale;
        }
        /// <summary>
        /// Updates and Returns the texture index
        /// </summary>
        public override int UpdateTexture()
        {
            TextureIndex = Manager.SphereTileTexture(this);
            return TextureIndex;
        }
    }
}
