/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace FiddlerControls
{
    public sealed class Options
    {
        private static int m_ArtItemSizeWidth = 48;
        private static int m_ArtItemSizeHeight = 48;
        private static bool m_ArtItemClip = true;
        private static bool m_DesignAlternative = false;
        private static string m_MapCmd = ".goforce";
        // {1} x {2} y {3} z {4} mapid {5} mapname
        private static string m_MapArgs = "{1} {2} {3} {4}";
        private static string[] m_MapNames = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno" };

        /// <summary>
        /// Definies Element Width in ItemShow
        /// </summary>
        public static int ArtItemSizeWidth
        {
            get { return m_ArtItemSizeWidth; }
            set { m_ArtItemSizeWidth = value; }
        }
        
        /// <summary>
        /// Definies Element Height in ItemShow
        /// </summary>
        public static int ArtItemSizeHeight
        {
            get { return m_ArtItemSizeHeight; }
            set { m_ArtItemSizeHeight = value; }
        }

        /// <summary>
        /// Definies if Element should be clipped or shrinked in ItemShow
        /// </summary>
        public static bool ArtItemClip
        {
            get { return m_ArtItemClip; }
            set { m_ArtItemClip = value; }
        }

        /// <summary>
        /// Definies if alternative Controls should be used 
        /// </summary>
        public static bool DesignAlternative
        {
            get { return m_DesignAlternative; }
            set { m_DesignAlternative = value; }
        }

        /// <summary>
        /// Definies the cmd to Send Client to Loc
        /// </summary>
        public static string MapCmd
        {
            get { return m_MapCmd; }
            set { m_MapCmd = value; }
        }

        /// <summary>
        /// Definies the args for Send Client
        /// </summary>
        public static string MapArgs
        {
            get { return m_MapArgs; }
            set { m_MapArgs = value; }
        }

        /// <summary>
        /// Definies the MapNames
        /// </summary>
        public static string[] MapNames 
        { 
            get { return m_MapNames; } 
            set { m_MapNames = value; } 
        }
    }
}