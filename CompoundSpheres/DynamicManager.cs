using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CompoundSpheres
{
    /// <summary>
    /// A dynamic version of the sphere manager. has rows which can have varied lengths which can interchange, and toggle spheretiles
    /// </summary>
    public class DynamicManager : ManagerBase<DynamicTile>
    {
        DynamicRow[] Rows;
        public override DynamicTile this[int x, int y] => Rows[x][y];

        public override int RowCount => Rows.Length;
        Texture2D Atlas;
        Rect[] UVs;

        public DynamicManager Init(DynamicManagerSettings Settings, int Rows, int InitialCols)
        {
            Tiles = new DynamicTile[InitialCols*Rows];
            Atlas = Settings.Atlas;
            UVs = Settings.UVs;
            this.Rows = new DynamicRow[Rows];
            base.Init(Settings);
            GetRange = Settings.GetCameraRange;
            Material.SetTexture("Atlas", Atlas);
            return this;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var Row in Rows)
            {
                Row.Dispose();
            }
        }
        public void Enlarge(int Amount)
        {
            DynamicTile[] temp = new DynamicTile[Tiles.Length + Amount];
            Tiles.CopyTo(temp, 0);
            Tiles = temp;
            Scales.Enlarge(TotalTiles);
        }
        public void DrawTiles(int CameraX)
        {
            GetRange(this, out Range Range);
            Material.SetFloat("ShouldRenderTextures", (int)getdisplaymode());
            for (int i = Range.Min; i < Range.Max; i++)
            {
                int I = (int)Clamp(CameraX, i);
                Rows[I].DrawTiles();
            }
        }

        public override void RefreshTextures()
        {
            throw new NotImplementedException();
        }

        public override void UpdateTexture(int I)
        {
            throw new NotImplementedException();
        }

        GetCameraRangeDynamic GetRange;
        public static class Creator
        {
            public static DynamicManager CreateDynamicManager(int Rows, int InitialCols, DynamicManagerSettings Settings, string Name = "Dynamic Manager")
            {
                DynamicManager manager = new GameObject(Name).AddComponent<DynamicManager>().Init(Settings, Rows, InitialCols);
                for (int i = 0; i < Rows; i++)
                {
                    var Row = manager.Rows[i] = new DynamicRow(manager, i, InitialCols, Instantiate(Settings.Culler));
                    for (int j = 0; j < InitialCols; j++)
                    {
                        int index = i * InitialCols + j;
                        manager.Tiles[index] = new DynamicTile(index, manager);
                    }
                }
                return manager;
            }
        }
    }
}
