using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class Map
    {
        private TileMatrix m_Tiles;
        private int m_FileIndex, m_MapID;
        private int m_Width, m_Height;

        private static bool m_UseDiff;

        public static bool UseDiff { get { return m_UseDiff; } set { m_UseDiff = value; } }

        public static Map Felucca = new Map(0, 0, 6144, 4096);
        public static Map Trammel = new Map(0, 1, 6144, 4096);
        public static readonly Map Ilshenar = new Map(2, 2, 2304, 1600);
        public static readonly Map Malas = new Map(3, 3, 2560, 2048);
        public static readonly Map Tokuno = new Map(4, 4, 1448, 1448);


        public Map(int fileIndex, int mapID, int width, int height)
        {
            m_FileIndex = fileIndex;
            m_MapID = mapID;
            m_Width = width;
            m_Height = height;
        }

        /// <summary>
        /// Sets cache-vars to null
        /// </summary>
        public static void Reload()
        {
            Felucca.m_Black = null;
            Felucca.m_Cache = null;
            Felucca.m_Cache_NoStatics = null;
            Felucca.m_Tiles = null;
            Trammel.m_Black = null;
            Trammel.m_Cache = null;
            Trammel.m_Cache_NoStatics = null;
            Trammel.m_Tiles = null;
            Ilshenar.m_Black = null;
            Ilshenar.m_Cache = null;
            Ilshenar.m_Cache_NoStatics = null;
            Ilshenar.m_Tiles = null;
            Malas.m_Black = null;
            Malas.m_Cache = null;
            Malas.m_Cache_NoStatics = null;
            Malas.m_Tiles = null;
            Tokuno.m_Black = null;
            Tokuno.m_Cache = null;
            Tokuno.m_Cache_NoStatics = null;
            Tokuno.m_Tiles = null;
        }
        public bool LoadedMatrix
        {
            get
            {
                return (m_Tiles != null);
            }
        }

        public TileMatrix Tiles
        {
            get
            {
                if (m_Tiles == null)
                    m_Tiles = new TileMatrix(m_FileIndex, m_MapID, m_Width, m_Height);

                return m_Tiles;
            }
        }

        public int Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        /// <summary>
        /// Returns Bitmap with Statics
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <returns></returns>
        public Bitmap GetImage(int x, int y, int width, int height)
        {
            return GetImage(x, y, width, height, true);
        }

        /// <summary>
        /// Returns Bitmap
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="statics">8x8 Block</param>
        /// <returns></returns>
        public Bitmap GetImage(int x, int y, int width, int height, bool statics)
        {
            Bitmap bmp = new Bitmap(width << 3, height << 3, PixelFormat.Format16bppRgb555);

            GetImage(x, y, width, height, bmp, statics);

            return bmp;
        }

        private short[][][] m_Cache;
        private short[][][] m_Cache_NoStatics;
        private short[] m_Black;

        private short[] GetRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = this.Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                if (m_Black == null)
                    m_Black = new short[64];

                return m_Black;
            }

            short[][][] cache = (statics ? m_Cache : m_Cache_NoStatics);

            if (cache == null)
            {
                if (statics)
                    m_Cache = cache = new short[m_Tiles.BlockHeight][][];
                else
                    m_Cache_NoStatics = cache = new short[m_Tiles.BlockHeight][][];
            }

            if (cache[y] == null)
                cache[y] = new short[m_Tiles.BlockWidth][];

            short[] data = cache[y][x];

            if (data == null)
                cache[y][x] = data = RenderBlock(x, y, statics);

            return data;
        }

        private unsafe short[] RenderBlock(int x, int y, bool drawStatics)
        {
            short[] data = new short[64];

            Tile[] tiles = m_Tiles.GetLandBlock(x, y);

            fixed (short* pColors = RadarCol.Colors)
            {
                fixed (int* pHeight = TileData.HeightTable)
                {
                    fixed (Tile* ptTiles = tiles)
                    {
                        Tile* pTiles = ptTiles;

                        fixed (short* pData = data)
                        {
                            short* pvData = pData;

                            if (drawStatics)
                            {
                                HuedTile[][][] statics = drawStatics ? m_Tiles.GetStaticBlock(x, y) : null;

                                for (int k = 0, v = 0; k < 8; ++k, v += 8)
                                {
                                    for (int p = 0; p < 8; ++p)
                                    {
                                        int highTop = -255;
                                        int highZ = -255;
                                        int highID = 0;
                                        int highHue = 0;
                                        int z, top;

                                        HuedTile[] curStatics = statics[p][k];

                                        if (curStatics.Length > 0)
                                        {
                                            fixed (HuedTile* phtStatics = curStatics)
                                            {
                                                HuedTile* pStatics = phtStatics;
                                                HuedTile* pStaticsEnd = pStatics + curStatics.Length;

                                                while (pStatics < pStaticsEnd)
                                                {
                                                    z = pStatics->m_Z;
                                                    top = z + pHeight[pStatics->ID & 0x3FFF];

                                                    if (top > highTop || (z > highZ && top >= highTop))
                                                    {
                                                        highTop = top;
                                                        highZ = z;
                                                        highID = pStatics->ID;
                                                        highHue = pStatics->Hue;
                                                    }

                                                    ++pStatics;
                                                }
                                            }
                                        }

                                        top = pTiles->m_Z;

                                        if (top > highTop)
                                        {
                                            highID = pTiles->m_ID;
                                            highHue = 0;
                                        }

                                        if (highHue == 0)
                                            *pvData++ = pColors[highID];
                                        else
                                            *pvData++ = Hues.GetHue(highHue - 1).Colors[(pColors[highID] >> 10) & 0x1F];

                                        ++pTiles;
                                    }
                                }
                            }
                            else
                            {
                                Tile* pEnd = pTiles + 64;

                                while (pTiles < pEnd)
                                    *pvData++ = pColors[(pTiles++)->m_ID];
                            }
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Draws in given Bitmap with Statics 
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="bmp">8x8 Block</param>
        public unsafe void GetImage(int x, int y, int width, int height, Bitmap bmp)
        {
            GetImage(x, y, width, height, bmp, true);
        }

        /// <summary>
        /// Draws in given Bitmap
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="bmp"></param>
        /// <param name="statics"></param>
        public unsafe void GetImage(int x, int y, int width, int height, Bitmap bmp, bool statics)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width << 3, height << 3), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb555);
            int stride = bd.Stride;
            int blockStride = stride << 3;

            byte* pStart = (byte*)bd.Scan0;

            for (int oy = 0, by = y; oy < height; ++oy, ++by, pStart += blockStride)
            {

                int* pRow0 = (int*)(pStart + (0 * stride));
                int* pRow1 = (int*)(pStart + (1 * stride));
                int* pRow2 = (int*)(pStart + (2 * stride));
                int* pRow3 = (int*)(pStart + (3 * stride));
                int* pRow4 = (int*)(pStart + (4 * stride));
                int* pRow5 = (int*)(pStart + (5 * stride));
                int* pRow6 = (int*)(pStart + (6 * stride));
                int* pRow7 = (int*)(pStart + (7 * stride));

                for (int ox = 0, bx = x; ox < width; ++ox, ++bx)
                {
                    short[] data = GetRenderedBlock(bx, by, statics);

                    fixed (short* pData = data)
                    {
                        int* pvData = (int*)pData;

                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;

                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;

                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;

                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;

                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;

                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;

                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;

                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                    }
                }
            }

            bmp.UnlockBits(bd);
        }
        private struct StaticTile
        {
            public short graphic;
            public byte x;
            public byte y;
            public sbyte z;
            public short hue;
        }
        public static void DefragStatics(string path, int map, int width, int height, bool remove)
        {
            string indexPath = Files.GetFilePath("staidx{0}.mul", map);
            FileStream m_Index;
            BinaryReader m_IndexReader;
            if (indexPath != null)
            {
                m_Index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_IndexReader = new BinaryReader(m_Index);
            }
            else
                return;

            string staticsPath = Files.GetFilePath("statics{0}.mul", map);

            FileStream m_Statics;
            BinaryReader m_StaticsReader;
            if (staticsPath != null)
            {
                m_Statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_StaticsReader = new BinaryReader(m_Statics);
            }
            else
                return;


            int blockx = width >> 3;
            int blocky = height >> 3;

            string idx = Path.Combine(path, String.Format("staidx{0}.mul", map));
            string mul = Path.Combine(path, String.Format("statics{0}.mul", map));
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx))
                {
                    using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        using (BinaryWriter binmul = new BinaryWriter(fsmul))
                        {
                            for (int x = 0; x < blockx; x++)
                            {
                                for (int y = 0; y < blocky; y++)
                                {
                                    try
                                    {
                                        m_IndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                        int lookup = m_IndexReader.ReadInt32();
                                        int length = m_IndexReader.ReadInt32();
                                        int extra = m_IndexReader.ReadInt32();

                                        if (lookup < 0 || length <= 0)
                                        {
                                            binidx.Write((int)-1); // lookup
                                            binidx.Write((int)-1); // length
                                            binidx.Write((int)-1); // extra
                                        }
                                        else
                                        {
                                            m_Statics.Seek(lookup, SeekOrigin.Begin);

                                            int fsmullength = (int)fsmul.Position;
                                            int count = length / 7;
                                            if (!remove) //without duplicate remove
                                            {
                                                bool firstitem = true;
                                                for (int i = 0; i < count; i++)
                                                {
                                                    short graphic = m_StaticsReader.ReadInt16();
                                                    byte sx = m_StaticsReader.ReadByte();
                                                    byte sy = m_StaticsReader.ReadByte();
                                                    sbyte sz = m_StaticsReader.ReadSByte();
                                                    short shue = m_StaticsReader.ReadInt16();
                                                    if ((graphic >= 0) && (graphic < 0x4000)) //legal?
                                                    {
                                                        if (firstitem)
                                                        {
                                                            binidx.Write((int)fsmul.Position); //lookup
                                                            firstitem = false;
                                                        }
                                                        binmul.Write(graphic);
                                                        binmul.Write(sx);
                                                        binmul.Write(sy);
                                                        binmul.Write(sz);
                                                        binmul.Write(shue);
                                                    }
                                                }
                                            }
                                            else //with duplicate remove
                                            {
                                                StaticTile[] tilelist = new StaticTile[count];
                                                int j = 0;
                                                for (int i = 0; i < count; i++)
                                                {
                                                    StaticTile tile = new StaticTile();
                                                    tile.graphic = m_StaticsReader.ReadInt16();
                                                    tile.x = m_StaticsReader.ReadByte();
                                                    tile.y = m_StaticsReader.ReadByte();
                                                    tile.z = m_StaticsReader.ReadSByte();
                                                    tile.hue = m_StaticsReader.ReadInt16();
                                                    if ((tile.graphic >= 0) && (tile.graphic < 0x4000))
                                                    {
                                                        bool first = true;
                                                        for (int k = 0; k < j; k++)
                                                        {
                                                            if ((tilelist[k].graphic == tile.graphic)
                                                                && ((tilelist[k].x == tile.x) && (tilelist[k].y == tile.y))
                                                                && (tilelist[k].z == tile.z)
                                                                && (tilelist[k].hue == tile.hue))
                                                            {
                                                                first = false;
                                                                break;
                                                            }
                                                        }
                                                        if (first)
                                                        {
                                                            tilelist[j] = tile;
                                                            j++;
                                                        }
                                                    }
                                                }
                                                if (j > 0)
                                                {
                                                    binidx.Write((int)fsmul.Position); //lookup
                                                    for (int i = 0; i < j; i++)
                                                    {
                                                        binmul.Write(tilelist[i].graphic);
                                                        binmul.Write(tilelist[i].x);
                                                        binmul.Write(tilelist[i].y);
                                                        binmul.Write(tilelist[i].z);
                                                        binmul.Write(tilelist[i].hue);
                                                    }
                                                }
                                            }

                                            fsmullength = (int)fsmul.Position - fsmullength;
                                            if (fsmullength > 0)
                                            {
                                                binidx.Write(fsmullength); //length
                                                binidx.Write(extra); //extra
                                            }
                                            else
                                            {
                                                binidx.Write((int)-1); //lookup
                                                binidx.Write((int)-1); //length
                                                binidx.Write((int)-1); //extra
                                            }
                                        }
                                    }
                                    catch // fill the rest
                                    {
                                        binidx.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                        for (; x < blockx; x++)
                                        {
                                            for (; y < blocky; y++)
                                            {
                                                binidx.Write((int)-1); //lookup
                                                binidx.Write((int)-1); //length
                                                binidx.Write((int)-1); //extra
                                            }
                                            y = 0;
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            m_IndexReader.Close();
            m_StaticsReader.Close();
        }

        public static void RewriteMap(string path, int map, int width, int height)
        {
            string mapPath = Files.GetFilePath("map{0}.mul", map);
            FileStream m_map;
            BinaryReader m_mapReader;
            if (mapPath != null)
            {
                m_map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_mapReader = new BinaryReader(m_map);
            }
            else
                return;

            int blockx = width >> 3;
            int blocky = height >> 3;

            string mul = Path.Combine(path, String.Format("map{0}.mul", map));
            using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binmul = new BinaryWriter(fsmul))
                {
                    for (int x = 0; x < blockx; x++)
                    {
                        for (int y = 0; y < blocky; y++)
                        {
                            try
                            {
                                m_mapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = m_mapReader.ReadInt32();
                                binmul.Write(header);
                                for (int i = 0; i < 64; i++)
                                {
                                    short tileid = m_mapReader.ReadInt16();
                                    sbyte z = m_mapReader.ReadSByte();
                                    if ((tileid < 0) || (tileid >= 0x4000))
                                        tileid = 0;
                                    if (z < -128)
                                        z = -128;
                                    if (z > 127)
                                        z = 127;
                                    binmul.Write(tileid);
                                    binmul.Write(z);
                                }
                            }
                            catch //fill rest
                            {
                                binmul.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                for (; x < blockx; x++)
                                {
                                    for (; y < blocky; y++)
                                    {
                                        binmul.Write((int)0);
                                        for (int i = 0; i < 64; i++)
                                        {
                                            binmul.Write((short)0);
                                            binmul.Write((sbyte)0);
                                        }
                                    }
                                    y = 0;
                                }
                                return;
                            }
                        }
                    }
                }
            }
            m_mapReader.Close();
        }

        public void ReportInvisStatics(string reportfile)
        {
            reportfile = Path.Combine(reportfile,String.Format("staticReport-{0}.csv",m_MapID));
            using (StreamWriter Tex = new StreamWriter(new FileStream(reportfile, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                Tex.WriteLine("x;y;z;Static");
                for (int x = 0; x < m_Width; x++)
                {
                    for (int y = 0; y < m_Height; y++)
                    {
                        Tile currtile = Tiles.GetLandTile(x, y);
                        foreach (HuedTile currstatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (currstatic.Z < currtile.Z)
                            {
                                if (TileData.ItemTable[currstatic.ID & 0x3FFF].Height + currstatic.Z < currtile.Z)
                                    Tex.WriteLine(String.Format("{0};{1};{2};0x{3:X}", x, y, currstatic.Z, currstatic.ID & 0x3FFF));
                            }
                        }
                    }
                }
                
            }
        }
    }
}