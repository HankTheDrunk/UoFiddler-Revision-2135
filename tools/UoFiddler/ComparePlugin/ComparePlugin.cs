﻿using System.Windows.Forms;
using ComparePlugin;
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

using PluginInterface;

namespace FiddlerPlugin
{
    public class ComparePlugin : IPlugin
    {
        string myName = "ComparePlugin";
        string myDescription = "\r\nCompares 2 art files\r\nCompares 2 CliLocs\r\n(Adds 3 new Tabs)";
        string myAuthor = "Turley";
        string myVersion = "1.1.0";
        IPluginHost myHost = null;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get { return myName; } }
        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get { return myDescription; } }
        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get { return myAuthor; } }
        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get { return myVersion; } }
        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get { return myHost; } set { myHost = value; } }

        public override void Initialize()
        {
        }

        public override void Dispose()
        {
        }

        public override void Reload()
        {
        }

        public override void OnDesignChange()
        {
        }

        public override void ModifyTabPages(TabControl tabcontrol)
        {
            TabPage page = new TabPage();
            page.Tag = tabcontrol.TabCount+1;
            page.Text = "Compare Items";
            CompareItem compArt = new CompareItem();
            compArt.Dock = System.Windows.Forms.DockStyle.Fill;
            page.Controls.Add(compArt);
            tabcontrol.TabPages.Add(page);

            TabPage page2 = new TabPage();
            page2.Tag = tabcontrol.TabCount + 1;
            page2.Text = "Compare Land";
            CompareLand compLand = new CompareLand();
            compLand.Dock = System.Windows.Forms.DockStyle.Fill;
            page2.Controls.Add(compLand);
            tabcontrol.TabPages.Add(page2);

            TabPage page3 = new TabPage();
            page3.Tag = tabcontrol.TabCount + 1;
            page3.Text = "Compare CliLocs";
            CompareCliLoc compCli = new CompareCliLoc();
            compCli.Dock = System.Windows.Forms.DockStyle.Fill;
            page3.Controls.Add(compCli);
            tabcontrol.TabPages.Add(page3);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
        }

        public override void ModifyItemShowContextMenu(ContextMenuStrip strip)
        {
        }
    }
}
