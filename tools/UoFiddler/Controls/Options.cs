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

using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;

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
        private static ArrayList m_PluginsToLoad;
        private static Dictionary<string, bool> m_LoadedUltimaClass = new Dictionary<string, bool>()
        {
            {"Animations",false},
            {"Animdata", false},
            {"Art", false},
            {"ASCIIFont", false},
            {"UnicodeFont", false},
            {"Gumps", false},
            {"Hues", false},
            {"Light", false},
            {"Map", false},
            {"Multis", false},
            {"Skills", false},
            {"Sound", false},
            {"Speech", false},
            {"StringList", false},
            {"Texture", false},
            {"TileData", false},
            {"RadarColor",false}
        };

        private static Dictionary<string, bool> m_ChangedUltimaClass = new Dictionary<string, bool>()
        {
            {"Animations",false},
            {"Animdata", false},
            {"Art", false},
            {"ASCIIFont", false},
            {"UnicodeFont", false},
            {"Gumps", false},
            {"Hues", false},
            {"Light", false},
            {"Map", false},
            {"Multis", false},
            {"Skills", false},
            {"Sound", false},
            {"Speech", false},
            {"CliLoc", false},
            {"Texture", false},
            {"TileData", false},
            {"RadarColor",false}
        };

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

        /// <summary>
        /// Definies which Plugins to load on startup
        /// </summary>
        public static ArrayList PluginsToLoad
        {
            get { return m_PluginsToLoad; }
            set { m_PluginsToLoad = value; }
        }

        /// <summary>
        /// Definies which muls are loaded
        /// <para>
        /// <list type="bullet">
        /// <item>Animations</item>
        /// <item>Animdata</item>
        /// <item>Art</item>
        /// <item>ASCIIFont</item>
        /// <item>Gumps</item>
        /// <item>Hues</item>
        /// <item>Light</item>
        /// <item>Map</item>
        /// <item>Multis</item>
        /// <item>Skills</item>
        /// <item>Sound</item>
        /// <item>Speech</item>
        /// <item>StringList</item>
        /// <item>Texture</item>
        /// <item>TileData</item>
        /// <item>UnicodeFont</item>
        /// <item>RadarColor</item>
        /// </list>
        /// </para>
        /// </summary>
        public static Dictionary<string, bool> LoadedUltimaClass
        {
            get { return m_LoadedUltimaClass; }
            set { m_LoadedUltimaClass = value; }
        }

        /// <summary>
        /// Definies which muls have unsaved changes
        /// <para>
        /// <list type="bullet">
        /// <item>Animations</item>
        /// <item>Animdata</item>
        /// <item>Art</item>
        /// <item>ASCIIFont</item>
        /// <item>Gumps</item>
        /// <item>Hues</item>
        /// <item>Light</item>
        /// <item>Map</item>
        /// <item>Multis</item>
        /// <item>Skills</item>
        /// <item>Sound</item>
        /// <item>Speech</item>
        /// <item>StringList</item>
        /// <item>Texture</item>
        /// <item>TileData</item>
        /// <item>UnicodeFont</item>
        /// <item>RadarColor</item>
        /// </list>
        /// </para>
        /// </summary>
        public static Dictionary<string, bool> ChangedUltimaClass
        {
            get { return Options.m_ChangedUltimaClass; }
            set { Options.m_ChangedUltimaClass = value; }
        }

        public static Hues HueControl { get; set; }
    }
}
