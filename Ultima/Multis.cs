using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Ultima
{
    public sealed class Multis
    {
        private static MultiComponentList[] m_Components = new MultiComponentList[0x2000];
        private static FileIndex m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);

        public enum ImportType
        {
            TXT,
            UOA,
            WSC,
            MULTICACHE,
            UOADESIGN
        }

        /// <summary>
        /// ReReads multi.mul
        /// </summary>
        public static void Reload()
        {
            m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);
            m_Components = new MultiComponentList[0x2000];
        }

        /// <summary>
        /// Gets <see cref="MultiComponentList"/> of multi
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MultiComponentList GetComponents(int index)
        {
            MultiComponentList mcl;

            index &= 0x1FFF;

            if (index >= 0 && index < m_Components.Length)
            {
                mcl = m_Components[index];

                if (mcl == null)
                    m_Components[index] = mcl = Load(index);
            }
            else
                mcl = MultiComponentList.Empty;

            return mcl;
        }

        public static MultiComponentList Load(int index)
        {
            try
            {
                int length, extra;
                bool patched;
                Stream stream = m_FileIndex.Seek(index, out length, out extra, out patched);

                if (stream == null)
                    return MultiComponentList.Empty;

                return new MultiComponentList(new BinaryReader(stream), length / 12);
            }
            catch
            {
                return MultiComponentList.Empty;
            }
        }

        public static void Remove(int index)
        {
            m_Components[index] = MultiComponentList.Empty;
        }

        public static void Add(int index, MultiComponentList comp)
        {
            m_Components[index] = comp;
        }

        public static MultiComponentList ImportFromFile(int index, string FileName, Multis.ImportType type, bool centeritem)
        {
            try
            {
                return m_Components[index] = new MultiComponentList(FileName, type, centeritem);
            }
            catch
            {
                return m_Components[index] = MultiComponentList.Empty;
            }
        }

        public static MultiComponentList LoadFromFile(string FileName, Multis.ImportType type)
        {
            try
            {
                return new MultiComponentList(FileName, type, false);
            }
            catch
            {
                return MultiComponentList.Empty;
            }
        }

        public static ArrayList LoadFromCache(string FileName)
        {
            ArrayList multilist = new ArrayList();
            using (StreamReader ip = new StreamReader(FileName))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    string[] split = Regex.Split(line, @"\s+");
                    if (split.Length == 7)
                    {
                        int count = Convert.ToInt32(split[2]);
                        multilist.Add(new MultiComponentList(ip, count));
                    }
                }
            }
            return multilist;
        }

        private static string ReadUOAString(BinaryReader bin)
        {
            byte flag = bin.ReadByte();

			if(flag == 0)
				return null;
			else
				return bin.ReadString();
        }
        public static ArrayList LoadFromDesigner(string FileName)
        {
            ArrayList multilist = new ArrayList();
            string root = Path.GetFileNameWithoutExtension(FileName);
            string idx = String.Format("{0}.idx", root);
            string bin = String.Format("{0}.bin", root);
            if ((!File.Exists(idx)) || (!File.Exists(bin)))
                return multilist;
            using (FileStream idxfs = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.Read),
                              binfs = new FileStream(bin, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader idxbin = new BinaryReader(idxfs),
                                    binbin = new BinaryReader(binfs))
                {
                    int count = idxbin.ReadInt32();
                    int version = idxbin.ReadInt32();

                    for (int i = 0; i < count; ++i)
                    {
                        Object[] data = new Object[2];
                        switch (version)
                        {
                            case 0:
                                data[0] = ReadUOAString(idxbin);
                                ArrayList arr = new ArrayList();
                                data[0] += "-"+ReadUOAString(idxbin);
                                data[0] += "-"+ReadUOAString(idxbin);
                                int width = idxbin.ReadInt32();
                                int height = idxbin.ReadInt32();
                                int uwidth = idxbin.ReadInt32();
                                int uheight = idxbin.ReadInt32();
                                long filepos = idxbin.ReadInt64();
                                int reccount = idxbin.ReadInt32();

                                binbin.BaseStream.Seek(filepos, SeekOrigin.Begin);
                                int index, x, y, z, level, hue;
                                for (int j = 0; j < reccount; ++j)
                                {
                                    index = x = y = z = level = hue = 0;
                                    int compVersion = binbin.ReadInt32();
                                    switch (compVersion)
                                    {
                                        case 0:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            level = binbin.ReadInt32();
                                            break;

                                        case 1:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            level = binbin.ReadInt32();
                                            hue = binbin.ReadInt32();
                                            break;
                                    }
                                    MultiComponentList.MultiTileEntry tempitem = new MultiComponentList.MultiTileEntry();
                                    tempitem.m_ItemID = (short)index;
                                    tempitem.m_Flags = 1;
                                    tempitem.m_OffsetX = (short)x;
                                    tempitem.m_OffsetY = (short)y;
                                    tempitem.m_OffsetZ = (short)z;
                                    arr.Add(tempitem);

                                }
                                data[1] = new MultiComponentList(arr);
                                break;
                                
                        }
                        multilist.Add(data);
                    }
                }
                return multilist;
            }
        }

        public static void Save(string path)
        {
            string idx = Path.Combine(path, "multi.idx");
            string mul = Path.Combine(path, "multi.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int index = 0; index < 0x2000; index++)
                    {
                        MultiComponentList comp = GetComponents(index);

                        if (comp == MultiComponentList.Empty)
                        {
                            binidx.Write((int)-1); // lookup
                            binidx.Write((int)-1); // length
                            binidx.Write((int)-1); // extra
                        }
                        else
                        {
                            binidx.Write((int)fsmul.Position); //lookup
                            binidx.Write((int)comp.SortedTiles.Length * 12); //length
                            binidx.Write((int)-1); //extra
                            for (int i = 0; i < comp.SortedTiles.Length; i++)
                            {
                                binmul.Write((short)comp.SortedTiles[i].m_ItemID);
                                binmul.Write((short)comp.SortedTiles[i].m_OffsetX);
                                binmul.Write((short)comp.SortedTiles[i].m_OffsetY);
                                binmul.Write((short)comp.SortedTiles[i].m_OffsetZ);
                                binmul.Write((int)comp.SortedTiles[i].m_Flags);
                            }
                        }
                    }
                }
            }
        }
    }

    public sealed class MultiComponentList
    {
        private Point m_Min, m_Max, m_Center;
        private int m_Width, m_Height, m_maxHeight, m_Surface;
        private MTile[][][] m_Tiles;
        private MultiTileEntry[] m_SortedTiles;

        public static readonly MultiComponentList Empty = new MultiComponentList();

        public Point Min { get { return m_Min; } }
        public Point Max { get { return m_Max; } }
        public Point Center { get { return m_Center; } }
        public int Width { get { return m_Width; } }
        public int Height { get { return m_Height; } }
        public MTile[][][] Tiles { get { return m_Tiles; } }
        public int maxHeight { get { return m_maxHeight; } }
        public MultiTileEntry[] SortedTiles { get { return m_SortedTiles; } }
        public int Surface { get { return m_Surface; } }

        public struct MultiTileEntry
        {
            public short m_ItemID;
            public short m_OffsetX, m_OffsetY, m_OffsetZ;
            public int m_Flags;
        }

        /// <summary>
        /// Returns Bitmap of Multi
        /// </summary>
        /// <returns></returns>
        public Bitmap GetImage()
        {
            return GetImage(-1);
        }

        /// <summary>
        /// Returns Bitmap of Multi to maxheight
        /// </summary>
        /// <param name="maxheight"></param>
        /// <returns></returns>
        public Bitmap GetImage(int maxheight)
        {
            if (m_Width == 0 || m_Height == 0)
                return null;

            int xMin = 1000, yMin = 1000;
            int xMax = -1000, yMax = -1000;

            for (int x = 0; x < m_Width; ++x)
            {
                for (int y = 0; y < m_Height; ++y)
                {
                    MTile[] tiles = m_Tiles[x][y];

                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        Bitmap bmp = Art.GetStatic(tiles[i].ID - 0x4000);

                        if (bmp == null)
                            continue;

                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= tiles[i].Z * 4;
                        py -= bmp.Height;

                        if (px < xMin)
                            xMin = px;

                        if (py < yMin)
                            yMin = py;

                        px += bmp.Width;
                        py += bmp.Height;

                        if (px > xMax)
                            xMax = px;

                        if (py > yMax)
                            yMax = py;
                    }
                }
            }

            Bitmap canvas = new Bitmap(xMax - xMin, yMax - yMin);
            Graphics gfx = Graphics.FromImage(canvas);
            gfx.Clear(Color.White);
            for (int x = 0; x < m_Width; ++x)
            {
                for (int y = 0; y < m_Height; ++y)
                {
                    MTile[] tiles = m_Tiles[x][y];

                    for (int i = 0; i < tiles.Length; ++i)
                    {

                        Bitmap bmp = Art.GetStatic(tiles[i].ID - 0x4000);

                        if (bmp == null)
                            continue;
                        if ((tiles[i].Z) > maxheight)
                            continue;
                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= tiles[i].Z * 4;
                        py -= bmp.Height;
                        px -= xMin;
                        py -= yMin;

                        gfx.DrawImageUnscaled(bmp, px, py, bmp.Width, bmp.Height);
                    }

                    int tx = (x - y) * 22;
                    int ty = (x + y) * 22;
                    tx -= xMin;
                    ty -= yMin;
                }
            }

            gfx.Dispose();

            return canvas;
        }

        public MultiComponentList(BinaryReader reader, int count)
        {
            m_Min = m_Max = Point.Empty;

            m_SortedTiles = new MultiTileEntry[count];

            for (int i = 0; i < count; ++i)
            {
                m_SortedTiles[i].m_ItemID = (short)(reader.ReadInt16() & 0x3FFF);
                m_SortedTiles[i].m_OffsetX = reader.ReadInt16();
                m_SortedTiles[i].m_OffsetY = reader.ReadInt16();
                m_SortedTiles[i].m_OffsetZ = reader.ReadInt16();
                m_SortedTiles[i].m_Flags = reader.ReadInt32();

                MultiTileEntry e = m_SortedTiles[i];

                if (e.m_OffsetX < m_Min.X)
                    m_Min.X = e.m_OffsetX;

                if (e.m_OffsetY < m_Min.Y)
                    m_Min.Y = e.m_OffsetY;

                if (e.m_OffsetX > m_Max.X)
                    m_Max.X = e.m_OffsetX;

                if (e.m_OffsetY > m_Max.Y)
                    m_Max.Y = e.m_OffsetY;

                if (e.m_OffsetZ > m_maxHeight)
                    m_maxHeight = e.m_OffsetZ;
            }
            ConvertList();
            reader.Close();
        }

        public MultiComponentList(string FileName, Multis.ImportType Type, bool centeritem)
        {
            m_Min = m_Max = Point.Empty;
            int itemcount;
            switch (Type)
            {
                case Multis.ImportType.TXT:
                    itemcount = 0;
                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            itemcount++;
                        }
                    }
                    if (centeritem)
                        itemcount++;
                    m_SortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    if (centeritem)
                        itemcount++;
                    m_Min.X = 10000;
                    m_Min.Y = 10000;
                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            string[] split = line.Split(' ');

                            string tmp = split[0];
                            tmp = tmp.Replace("0x", "");

                            m_SortedTiles[itemcount].m_ItemID = short.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                            m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[1]);
                            m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[2]);
                            m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[3]);
                            m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[4]);

                            MultiTileEntry e = m_SortedTiles[itemcount];

                            if (e.m_OffsetX < m_Min.X)
                                m_Min.X = e.m_OffsetX;

                            if (e.m_OffsetY < m_Min.Y)
                                m_Min.Y = e.m_OffsetY;

                            if (e.m_OffsetX > m_Max.X)
                                m_Max.X = e.m_OffsetX;

                            if (e.m_OffsetY > m_Max.Y)
                                m_Max.Y = e.m_OffsetY;

                            if (e.m_OffsetZ > m_maxHeight)
                                m_maxHeight = e.m_OffsetZ;

                            itemcount++;
                        }
                        int centerx = m_Max.X - (m_Max.X - m_Min.X) / 2;
                        int centery = m_Max.Y - (m_Max.Y - m_Min.Y) / 2;

                        m_Min = m_Max = Point.Empty;
                        int i = 0;
                        if (centeritem)
                            i++;
                        for (; i < m_SortedTiles.Length; i++)
                        {
                            m_SortedTiles[i].m_OffsetX -= (short)centerx;
                            m_SortedTiles[i].m_OffsetY -= (short)centery;
                            if (m_SortedTiles[i].m_OffsetX < m_Min.X)
                                m_Min.X = m_SortedTiles[i].m_OffsetX;
                            if (m_SortedTiles[i].m_OffsetX > m_Max.X)
                                m_Max.X = m_SortedTiles[i].m_OffsetX;

                            if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
                                m_Min.Y = m_SortedTiles[i].m_OffsetY;
                            if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
                                m_Max.Y = m_SortedTiles[i].m_OffsetY;
                        }
                    }
                    break;
                case Multis.ImportType.UOA:
                    itemcount = 0;

                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            itemcount++;
                            if (itemcount == 4)
                            {
                                string[] split = line.Split(' ');
                                itemcount = Convert.ToInt32(split[0]);
                                break;
                            }
                        }
                    }
                    if (centeritem)
                        itemcount++;
                    m_SortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    if (centeritem)
                        itemcount++;
                    m_Min.X = 10000;
                    m_Min.Y = 10000;
                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        int i = -1;
                        while ((line = ip.ReadLine()) != null)
                        {
                            i++;
                            if (i < 4)
                                continue;
                            string[] split = line.Split(' ');

                            m_SortedTiles[itemcount].m_ItemID = Convert.ToInt16(split[0]);
                            m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[1]);
                            m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[2]);
                            m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[3]);
                            m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[4]);

                            MultiTileEntry e = m_SortedTiles[itemcount];

                            if (e.m_OffsetX < m_Min.X)
                                m_Min.X = e.m_OffsetX;

                            if (e.m_OffsetY < m_Min.Y)
                                m_Min.Y = e.m_OffsetY;

                            if (e.m_OffsetX > m_Max.X)
                                m_Max.X = e.m_OffsetX;

                            if (e.m_OffsetY > m_Max.Y)
                                m_Max.Y = e.m_OffsetY;

                            if (e.m_OffsetZ > m_maxHeight)
                                m_maxHeight = e.m_OffsetZ;

                            itemcount++;
                        }
                        int centerx = m_Max.X - (m_Max.X - m_Min.X) / 2;
                        int centery = m_Max.Y - (m_Max.Y - m_Min.Y) / 2;

                        m_Min = m_Max = Point.Empty;
                        i = 0;
                        if (centeritem)
                            i++;
                        for (; i < m_SortedTiles.Length; i++)
                        {
                            m_SortedTiles[i].m_OffsetX -= (short)centerx;
                            m_SortedTiles[i].m_OffsetY -= (short)centery;
                            if (m_SortedTiles[i].m_OffsetX < m_Min.X)
                                m_Min.X = m_SortedTiles[i].m_OffsetX;
                            if (m_SortedTiles[i].m_OffsetX > m_Max.X)
                                m_Max.X = m_SortedTiles[i].m_OffsetX;

                            if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
                                m_Min.Y = m_SortedTiles[i].m_OffsetY;
                            if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
                                m_Max.Y = m_SortedTiles[i].m_OffsetY;
                        }
                    }

                    break;
                case Multis.ImportType.WSC:
                    itemcount = 0;
                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.StartsWith("SECTION WORLDITEM"))
                                itemcount++;
                        }
                    }
                    if (centeritem)
                        itemcount++;
                    m_SortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    if (centeritem)
                        itemcount++;
                    m_Min.X = 10000;
                    m_Min.Y = 10000;
                    using (StreamReader ip = new StreamReader(FileName))
                    {
                        string line;
                        MultiTileEntry tempitem = new MultiTileEntry();
                        tempitem.m_ItemID = -1;
                        tempitem.m_Flags = 1;
                        while ((line = ip.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.StartsWith("SECTION WORLDITEM"))
                            {
                                if (tempitem.m_ItemID != -1)
                                {
                                    m_SortedTiles[itemcount] = tempitem;
                                    itemcount++;
                                }
                                tempitem.m_ItemID = -1;
                            }
                            else if (line.StartsWith("ID"))
                            {
                                line = line.Remove(0, 2);
                                line = line.Trim();
                                tempitem.m_ItemID = Convert.ToInt16(line);
                            }
                            else if (line.StartsWith("X"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.m_OffsetX = Convert.ToInt16(line);
                                if (tempitem.m_OffsetX < m_Min.X)
                                    m_Min.X = tempitem.m_OffsetX;
                                if (tempitem.m_OffsetX > m_Max.X)
                                    m_Max.X = tempitem.m_OffsetX;
                            }
                            else if (line.StartsWith("Y"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.m_OffsetY = Convert.ToInt16(line);
                                if (tempitem.m_OffsetY < m_Min.Y)
                                    m_Min.Y = tempitem.m_OffsetY;
                                if (tempitem.m_OffsetY > m_Max.Y)
                                    m_Max.Y = tempitem.m_OffsetY;
                            }
                            else if (line.StartsWith("Z"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.m_OffsetZ = Convert.ToInt16(line);
                                if (tempitem.m_OffsetZ > m_maxHeight)
                                    m_maxHeight = tempitem.m_OffsetZ;

                            }
                        }
                        if (tempitem.m_ItemID != -1)
                            m_SortedTiles[itemcount] = tempitem;

                        int centerx = m_Max.X - (m_Max.X - m_Min.X) / 2;
                        int centery = m_Max.Y - (m_Max.Y - m_Min.Y) / 2;

                        m_Min = m_Max = Point.Empty;
                        int i = 0;
                        if (centeritem)
                            i++;
                        for (; i < m_SortedTiles.Length; i++)
                        {
                            m_SortedTiles[i].m_OffsetX -= (short)centerx;
                            m_SortedTiles[i].m_OffsetY -= (short)centery;
                            if (m_SortedTiles[i].m_OffsetX < m_Min.X)
                                m_Min.X = m_SortedTiles[i].m_OffsetX;
                            if (m_SortedTiles[i].m_OffsetX > m_Max.X)
                                m_Max.X = m_SortedTiles[i].m_OffsetX;

                            if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
                                m_Min.Y = m_SortedTiles[i].m_OffsetY;
                            if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
                                m_Max.Y = m_SortedTiles[i].m_OffsetY;
                        }
                    }
                    break;
            }
            if (centeritem)
            {
                m_SortedTiles[0].m_ItemID = 0x1; // insert invis center item
                m_SortedTiles[0].m_OffsetX = 0;
                m_SortedTiles[0].m_OffsetY = 0;
                m_SortedTiles[0].m_Flags = 0;
            }
            ConvertList();
        }

        public MultiComponentList(ArrayList arr)
        {
            m_Min = m_Max = Point.Empty;
            int itemcount = arr.Count;
            m_SortedTiles = new MultiTileEntry[itemcount];
            m_Min.X = 10000;
            m_Min.Y = 10000;
            int i=0;
            foreach (MultiTileEntry entry in arr)
            {
                if (entry.m_OffsetX < m_Min.X)
                    m_Min.X = entry.m_OffsetX;

                if (entry.m_OffsetY < m_Min.Y)
                    m_Min.Y = entry.m_OffsetY;

                if (entry.m_OffsetX > m_Max.X)
                    m_Max.X = entry.m_OffsetX;

                if (entry.m_OffsetY > m_Max.Y)
                    m_Max.Y = entry.m_OffsetY;

                if (entry.m_OffsetZ > m_maxHeight)
                    m_maxHeight = entry.m_OffsetZ;
                m_SortedTiles[i] = entry;

                i++;
            }
            arr.Clear();
            int centerx = m_Max.X - (m_Max.X - m_Min.X) / 2;
            int centery = m_Max.Y - (m_Max.Y - m_Min.Y) / 2;

            m_Min = m_Max = Point.Empty;
            for (i=0; i < m_SortedTiles.Length; i++)
            {
                m_SortedTiles[i].m_OffsetX -= (short)centerx;
                m_SortedTiles[i].m_OffsetY -= (short)centery;
                if (m_SortedTiles[i].m_OffsetX < m_Min.X)
                    m_Min.X = m_SortedTiles[i].m_OffsetX;
                if (m_SortedTiles[i].m_OffsetX > m_Max.X)
                    m_Max.X = m_SortedTiles[i].m_OffsetX;

                if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
                    m_Min.Y = m_SortedTiles[i].m_OffsetY;
                if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
                    m_Max.Y = m_SortedTiles[i].m_OffsetY;
            }
            ConvertList();
        }

        public MultiComponentList(StreamReader stream, int count)
        {
            string line;
            int itemcount = 0;
            m_Min = m_Max = Point.Empty;
            m_SortedTiles = new MultiTileEntry[count];
            m_Min.X = 10000;
            m_Min.Y = 10000;

            while ((line = stream.ReadLine()) != null)
            {
                string[] split = Regex.Split(line, @"\s+");
                m_SortedTiles[itemcount].m_ItemID = Convert.ToInt16(split[0]);
                m_SortedTiles[itemcount].m_Flags = Convert.ToInt32(split[1]);
                m_SortedTiles[itemcount].m_OffsetX = Convert.ToInt16(split[2]);
                m_SortedTiles[itemcount].m_OffsetY = Convert.ToInt16(split[3]);
                m_SortedTiles[itemcount].m_OffsetZ = Convert.ToInt16(split[4]);
                
                MultiTileEntry e = m_SortedTiles[itemcount];

                if (e.m_OffsetX < m_Min.X)
                    m_Min.X = e.m_OffsetX;
                if (e.m_OffsetY < m_Min.Y)
                    m_Min.Y = e.m_OffsetY;
                if (e.m_OffsetX > m_Max.X)
                    m_Max.X = e.m_OffsetX;
                if (e.m_OffsetY > m_Max.Y)
                    m_Max.Y = e.m_OffsetY;
                if (e.m_OffsetZ > m_maxHeight)
                    m_maxHeight = e.m_OffsetZ;

                itemcount++;
                if (itemcount == count)
                    break;
                
            }
            int centerx = m_Max.X - (m_Max.X - m_Min.X) / 2;
            int centery = m_Max.Y - (m_Max.Y - m_Min.Y) / 2;

            m_Min = m_Max = Point.Empty;
            int i = 0;
            for (; i < m_SortedTiles.Length; i++)
            {
                m_SortedTiles[i].m_OffsetX -= (short)centerx;
                m_SortedTiles[i].m_OffsetY -= (short)centery;
                if (m_SortedTiles[i].m_OffsetX < m_Min.X)
                    m_Min.X = m_SortedTiles[i].m_OffsetX;
                if (m_SortedTiles[i].m_OffsetX > m_Max.X)
                    m_Max.X = m_SortedTiles[i].m_OffsetX;

                if (m_SortedTiles[i].m_OffsetY < m_Min.Y)
                    m_Min.Y = m_SortedTiles[i].m_OffsetY;
                if (m_SortedTiles[i].m_OffsetY > m_Max.Y)
                    m_Max.Y = m_SortedTiles[i].m_OffsetY;
            }
            ConvertList();
        }

        private void ConvertList()
        {
            m_Center = new Point(-m_Min.X, -m_Min.Y);
            m_Width = (m_Max.X - m_Min.X) + 1;
            m_Height = (m_Max.Y - m_Min.Y) + 1;

            MTileList[][] tiles = new MTileList[m_Width][];
            m_Tiles = new MTile[m_Width][][];

            for (int x = 0; x < m_Width; ++x)
            {
                tiles[x] = new MTileList[m_Height];
                m_Tiles[x] = new MTile[m_Height][];

                for (int y = 0; y < m_Height; ++y)
                    tiles[x][y] = new MTileList();
            }

            for (int i = 0; i < m_SortedTiles.Length; ++i)
            {
                int xOffset = m_SortedTiles[i].m_OffsetX + m_Center.X;
                int yOffset = m_SortedTiles[i].m_OffsetY + m_Center.Y;

                tiles[xOffset][yOffset].Add((short)(m_SortedTiles[i].m_ItemID + 0x4000), (sbyte)m_SortedTiles[i].m_OffsetZ, (sbyte)m_SortedTiles[i].m_Flags);
            }

            m_Surface = 0;
            for (int x = 0; x < m_Width; ++x)
            {
                for (int y = 0; y < m_Height; ++y)
                {
                    m_Tiles[x][y] = tiles[x][y].ToArray();
                    if (m_Tiles[x][y].Length > 1)
                        Array.Sort(m_Tiles[x][y]);
                    if (m_Tiles[x][y].Length > 0)
                        m_Surface++;
                }
            }
        }

        public MultiComponentList(MTileList[][] newtiles, int count, int width, int height)
        {
            m_Min = m_Max = Point.Empty;
            m_SortedTiles = new MultiTileEntry[count];
            m_Center = new Point((width / 2) - 1, (height / 2) - 1);
            if (m_Center.X < 0)
                m_Center.X = width / 2;
            if (m_Center.Y < 0)
                m_Center.Y = height / 2;
            m_maxHeight = -128;

            bool centerfound = false;
            if (newtiles[m_Center.X][m_Center.Y].Count > 0)
            {
                for (int i = 0; i < newtiles[m_Center.X][m_Center.Y].Count; i++)
                {
                    if ((newtiles[m_Center.X][m_Center.Y].Get(i).ID == 0x4001) && 
                        (newtiles[m_Center.X][m_Center.Y].Get(i).Z == 0))
                    {
                        m_SortedTiles[0].m_OffsetX = 0;
                        m_SortedTiles[0].m_OffsetY = 0;
                        m_SortedTiles[0].m_OffsetZ = 0;
                        m_SortedTiles[0].m_Flags = 0;
                        m_SortedTiles[0].m_ItemID = 0x1;
                        centerfound = true;
                        newtiles[m_Center.X][m_Center.Y].Set(i, -1, 0);
                        break;
                    }
                }
            }
            int counter = 1;
            if (!centerfound)
            {
                if (MessageBox.Show("No invisible Center Item found do you want to add it?", 
                    "Multi Save", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                    MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    m_SortedTiles = new MultiTileEntry[count + 1];
                    m_SortedTiles[0].m_ItemID = 0x1; // insert invis center item
                    m_SortedTiles[0].m_OffsetX = 0;
                    m_SortedTiles[0].m_OffsetY = 0;
                    m_SortedTiles[0].m_Flags = 0;
                }
                else
                    counter = 0;
            }
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MTile[] tiles = newtiles[x][y].ToArray();
                    for (int i = 0; i < tiles.Length; i++)
                    {
                        if (tiles[i].ID>=0)
                        {
                            m_SortedTiles[counter].m_ItemID = (short)(tiles[i].ID & 0x3FFF);
                            m_SortedTiles[counter].m_OffsetX = (short)(x - m_Center.X);
                            m_SortedTiles[counter].m_OffsetY = (short)(y - m_Center.Y);
                            m_SortedTiles[counter].m_OffsetZ = (short)(tiles[i].Z);
                            m_SortedTiles[counter].m_Flags = (int)tiles[i].Flag;
                            
                            if (m_SortedTiles[counter].m_OffsetX < m_Min.X)
                                m_Min.X = m_SortedTiles[counter].m_OffsetX;
                            if (m_SortedTiles[counter].m_OffsetX > m_Max.X)
                                m_Max.X = m_SortedTiles[counter].m_OffsetX;
                            if (m_SortedTiles[counter].m_OffsetY < m_Min.Y)
                                m_Min.Y = m_SortedTiles[counter].m_OffsetY;
                            if (m_SortedTiles[counter].m_OffsetY > m_Max.Y)
                                m_Max.Y = m_SortedTiles[counter].m_OffsetY;
                            if (m_SortedTiles[counter].m_OffsetZ > m_maxHeight)
                                m_maxHeight = m_SortedTiles[counter].m_OffsetZ;
                            counter++;
                        }
                    }
                }
            }
            ConvertList();
        }

        private MultiComponentList()
        {
            m_Tiles = new MTile[0][][];
        }

        public void ExportToTextFile(string FileName)
        {
            using (StreamWriter Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < m_SortedTiles.Length; ++i)
                {
                    Tex.WriteLine(String.Format("0x{0:X} {1} {2} {3} {4}",
                                m_SortedTiles[i].m_ItemID,
                                m_SortedTiles[i].m_OffsetX,
                                m_SortedTiles[i].m_OffsetY,
                                m_SortedTiles[i].m_OffsetZ,
                                m_SortedTiles[i].m_Flags));
                }
            }
        }

        public void ExportToWscFile(string FileName)
        {
            using (StreamWriter Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < m_SortedTiles.Length; ++i)
                {
                    Tex.WriteLine(String.Format("SECTION WORLDITEM {0}", i));
                    Tex.WriteLine("{");
                    Tex.WriteLine(String.Format("\tID\t{0}", m_SortedTiles[i].m_ItemID));
                    Tex.WriteLine(String.Format("\tX\t{0}", m_SortedTiles[i].m_OffsetX));
                    Tex.WriteLine(String.Format("\tY\t{0}", m_SortedTiles[i].m_OffsetY));
                    Tex.WriteLine(String.Format("\tZ\t{0}", m_SortedTiles[i].m_OffsetZ));
                    Tex.WriteLine("\tColor\t0");
                    Tex.WriteLine("}");

                }
            }
        }

        public void ExportToUOAFile(string FileName)
        {
            using (StreamWriter Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                Tex.WriteLine("6 version");
                Tex.WriteLine("1 template id");
                Tex.WriteLine("-1 item version");
                Tex.WriteLine(String.Format("{0} num components", m_SortedTiles.Length));
                for (int i = 0; i < m_SortedTiles.Length; ++i)
                {
                    Tex.WriteLine(String.Format("{0} {1} {2} {3} {4}",
                                m_SortedTiles[i].m_ItemID,
                                m_SortedTiles[i].m_OffsetX,
                                m_SortedTiles[i].m_OffsetY,
                                m_SortedTiles[i].m_OffsetZ,
                                m_SortedTiles[i].m_Flags));
                }
            }
        }
    }
}