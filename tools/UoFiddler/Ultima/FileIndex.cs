using System;
using System.Collections;
using System.IO;

namespace Ultima
{
	public sealed class FileIndex
	{
        private Entry3D[] m_Index;
        private Stream m_Stream;

        public Entry3D[] Index { get { return m_Index; } }
        public Stream Stream { get { return m_Stream; } }
        
		public Stream Seek( int index, out int length, out int extra, out bool patched )
		{
			if ( index < 0 || index >= m_Index.Length )
			{
				length = extra = 0;
				patched = false;
				return null;
			}

			Entry3D e = m_Index[index];

			if ( e.lookup < 0 )
			{
				length = extra = 0;
				patched = false;
				return null;
			}

			length = e.length & 0x7FFFFFFF;
			extra = e.extra;

			if ( (e.length & (1 << 31)) != 0 )
			{
				patched = true;

				Verdata.Stream.Seek( e.lookup, SeekOrigin.Begin );
				return Verdata.Stream;
			}
			else if ( m_Stream == null )
			{
				length = extra = 0;
				patched = false;
				return null;
			}

			patched = false;

			m_Stream.Seek( e.lookup, SeekOrigin.Begin );
			return m_Stream;
		}

		public FileIndex( string idxFile, string mulFile, int length, int file )
		{
			m_Index = new Entry3D[length];

            string idxPath=null;
            string mulPath=null;
            if (Files.MulPath == null)
                Files.LoadMulPath();
            if (Files.MulPath.Count > 0)
            {
                idxPath = Files.MulPath[idxFile.ToLower()].ToString();
                mulPath = Files.MulPath[mulFile.ToLower()].ToString();
                if (!File.Exists(idxPath))
                    idxPath = null;
                if (!File.Exists(mulPath))
                    mulPath = null;
            }

            if (idxPath != null && mulPath != null)
            {
                using (FileStream index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader bin = new BinaryReader(index);
                    m_Stream = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    int count = (int)(index.Length / 12);

                    for (int i = 0; i < count && i < length; ++i)
                    {
                        m_Index[i].lookup = bin.ReadInt32();
                        m_Index[i].length = bin.ReadInt32();
                        m_Index[i].extra = bin.ReadInt32();
                    }

                    for (int i = count; i < length; ++i)
                    {
                        m_Index[i].lookup = -1;
                        m_Index[i].length = -1;
                        m_Index[i].extra = -1;
                    }
                }
            }
            else
            {
                m_Stream = null;
                return;
            }
			Entry5D[] patches = Verdata.Patches;

			for ( int i = 0; i < patches.Length; ++i )
			{
				Entry5D patch = patches[i];

				if ( patch.file == file && patch.index >= 0 && patch.index < length )
				{
					m_Index[patch.index].lookup = patch.lookup;
					m_Index[patch.index].length = patch.length | (1 << 31);
					m_Index[patch.index].extra = patch.extra;
				}
			}
		}
	}

	public struct Entry3D
	{
		public int lookup;
		public int length;
		public int extra;
	}
}