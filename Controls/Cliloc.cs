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

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class Cliloc : UserControl
    {
        public Cliloc()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refmarker = this;
            source = new BindingSource();
        }

        #region Var's
        private static Cliloc refmarker;
        private static StringList cliloc;
        private static BindingSource source;
        private int lang;
        private SortOrder sortorder;
        private int sortcolumn;
        private bool Loaded = false;

        /// <summary>
        /// Sets Language and loads cliloc
        /// </summary>
        private int Lang
        {
            get { return lang; }
            set
            {
                lang = value;
                switch (value)
                {
                    case 0: 
                        cliloc = new StringList("enu");
                        break;
                    case 1:
                        cliloc = new StringList("deu");
                        break;
                    case 2:
                        TestCustomLang("cliloc.custom1");
                        cliloc = new StringList("custom1");
                        break;
                    case 3:
                        TestCustomLang("cliloc.custom2");
                        cliloc = new StringList("custom2");
                        break;
                }
                    
            }
        }
        #endregion

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        public void Reload()
        {
            if (!Loaded)
                return;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            Loaded = true;
            sortorder = SortOrder.Ascending;
            sortcolumn = 0;
            LangComboBox.SelectedIndex=0;
            Lang = 0;
            cliloc.Entries.Sort(new StringList.NumberComparer(false));
            source.DataSource = cliloc.Entries;
            dataGridView1.DataSource = source;
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].Width = 60;
                dataGridView1.Columns[2].ReadOnly = true;
            }
            dataGridView1.Refresh();
            if (Files.GetFilePath("cliloc.custom1") != null)
                LangComboBox.Items[2] = String.Format("Custom 1 ({0})", Path.GetExtension(Files.GetFilePath("cliloc.custom1")));
            else
                LangComboBox.Items[2] = "Custom 1";
            if (Files.GetFilePath("cliloc.custom2") != null)
                LangComboBox.Items[3] = String.Format("Custom 2 ({0})", Path.GetExtension(Files.GetFilePath("cliloc.custom2")));
            else
                LangComboBox.Items[3] = "Custom 2";

            this.Cursor = Cursors.Default;
        }

        private void TestCustomLang(string what)
        {
            if (Files.GetFilePath(what) == null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Title = "Choose Cliloc file to open";
                dialog.CheckFileExists = true;
                dialog.Filter = "cliloc files (cliloc.*)|cliloc.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Files.SetMulPath(dialog.FileName, what);
                    LangComboBox.BeginUpdate();
                    if (what == "cliloc.custom1")
                        LangComboBox.Items[2] = String.Format("Custom 1 ({0})", Path.GetExtension(dialog.FileName));
                    else
                        LangComboBox.Items[3] = String.Format("Custom 2 ({0})", Path.GetExtension(dialog.FileName));
                    LangComboBox.EndUpdate();
                }
            }
        }

        private void onLangChange(object sender, EventArgs e)
        {
            if (LangComboBox.SelectedIndex != Lang)
            {
                Lang = LangComboBox.SelectedIndex;
                sortorder = SortOrder.Ascending;
                sortcolumn = 0;
                cliloc.Entries.Sort(new StringList.NumberComparer(false));
                source.DataSource = cliloc.Entries;
                if (dataGridView1.Columns.Count > 0)
                {
                    dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                    dataGridView1.Columns[0].Width = 60;
                    dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                    dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                    dataGridView1.Columns[2].Width = 60;
                    dataGridView1.Columns[2].ReadOnly = true;
                }
                dataGridView1.Refresh();
            }
        }

        private void GotoNr(object sender, EventArgs e)
        {
            int nr;
            if (Int32.TryParse(GotoEntry.Text.ToString(), NumberStyles.Integer, null, out nr))
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ((int)dataGridView1.Rows[i].Cells[0].Value == nr)
                    {
                        dataGridView1.Rows[i].Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = i;
                        return;
                    }
                }
            }
            MessageBox.Show(
                "Number not found.", 
                "Goto", 
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void FindEntryClick(object sender, EventArgs e)
        {
            string find = FindEntry.Text.ToString();
            for (int i = (dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected)+1); i < dataGridView1.Rows.Count; i++)
            {
                if ((dataGridView1.Rows[i].Cells[1].Value.ToString().IndexOf(find))!=-1)
                {
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }
            MessageBox.Show(
                "Entry not found.", 
                "Find Entry", 
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string FileName;
            if (cliloc.Language == "custom1")
                FileName = Path.Combine(path, String.Format("Cliloc{0}", Path.GetExtension(Files.GetFilePath("cliloc.custom1"))));
            else if (cliloc.Language=="custom2")
                FileName = Path.Combine(path, String.Format("Cliloc{0}", Path.GetExtension(Files.GetFilePath("cliloc.custom2"))));
            else
                FileName = Path.Combine(path, String.Format("Cliloc.{0}",cliloc.Language));
            cliloc.SaveStringList(FileName);
            dataGridView1.Columns[sortcolumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            sortcolumn = 0;
            sortorder = SortOrder.Ascending;
            dataGridView1.Refresh();
            MessageBox.Show(
                String.Format("CliLoc saved to {0}",FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onCell_dbClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int CellNr = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
                string CellText = (string)dataGridView1.Rows[e.RowIndex].Cells[1].Value;
                new ClilocDetail(CellNr, CellText).Show();
            }
        }

        private void OnClick_AddEntry(object sender, EventArgs e)
        {
            new ClilocAdd().Show();
        }

        private void OnClick_DeleteEntry(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                cliloc.Entries.RemoveAt(dataGridView1.SelectedCells[0].OwningRow.Index);
                dataGridView1.Refresh();
            }
        }

        private void OnHeaderClicked(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (sortcolumn == e.ColumnIndex)
            {
                if (sortorder == SortOrder.Ascending)
                    sortorder = SortOrder.Descending;
                else
                    sortorder = SortOrder.Ascending;
            }
            else
            {
                sortorder = SortOrder.Ascending;
                dataGridView1.Columns[sortcolumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortorder;
            sortcolumn = e.ColumnIndex;

            if (e.ColumnIndex == 0)
                cliloc.Entries.Sort(new StringList.NumberComparer(sortorder == SortOrder.Descending));
            else if (e.ColumnIndex == 1)
                cliloc.Entries.Sort(new StringList.TextComparer(sortorder == SortOrder.Descending));
            else
                cliloc.Entries.Sort(new StringList.FlagComparer(sortorder == SortOrder.Descending));

            dataGridView1.Refresh();
        }

        private void OnChange(object sender, DataGridViewCellEventArgs e)
        {
            if (((StringEntry)cliloc.Entries[e.RowIndex]).Text == null)
                ((StringEntry)cliloc.Entries[e.RowIndex]).Text = "";
        }

        #region Public Interface for ClilocAdd

        public static void SaveEntry(int number, string text)
        {
            for (int i = 0; i < cliloc.Entries.Count; i++)
            {
                if (((StringEntry)cliloc.Entries[i]).Number == number)
                {
                    ((StringEntry)cliloc.Entries[i]).Text = text;
                    ((StringEntry)cliloc.Entries[i]).Flag = StringEntry.CliLocFlag.Modified;
                    refmarker.dataGridView1.Refresh();
                    refmarker.dataGridView1.Rows[i].Selected = true;
                    refmarker.dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }
        }

        public static bool IsNumberFree(int number)
        {
            foreach (StringEntry entry in cliloc.Entries)
            {
                if (entry.Number == number)
                    return false;
            }
            return true;
        }

        public static void AddEntry(int number)
        {
            int index = 0;
            foreach (StringEntry entry in cliloc.Entries)
            {
                if (entry.Number > number)
                {
                    cliloc.Entries.Insert(index, new StringEntry(number, "", StringEntry.CliLocFlag.Custom));
                    refmarker.dataGridView1.Refresh();
                    refmarker.dataGridView1.Rows[index].Selected = true;
                    refmarker.dataGridView1.FirstDisplayedScrollingRowIndex = index;
                    return;
                }
                ++index;
            }
        }
        #endregion
    }
}
