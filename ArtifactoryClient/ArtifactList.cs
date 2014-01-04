using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ArtifactoryClient
{
    public partial class ArtifactList : Form
    {
        public ArtifactList()
        {
            InitializeComponent();
        }

        public DataTable artifactList {get; set;}
        public String ArtifactoryURL { get; set; }
        public String Repository { get; set; }
        public String AuthUser { get; set; }
        public String AuthPwd { get; set; }

        private void ArtifactList_Load(object sender, EventArgs e)
        {
            // Set the view to show details.
            listView1.View = View.Details;

            // Display check boxes.
            listView1.CheckBoxes = true;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;

            Cursor.Current = Cursors.WaitCursor;

            listView1.Columns.Add("Created", 150, HorizontalAlignment.Left);
            listView1.Columns.Add("Artifact", -2, HorizontalAlignment.Left);

            listView1.Items.Clear();
            foreach (DataRow row in artifactList.Rows)
            {
                var listViewItem = new ListViewItem(row["Created"].ToString());
                listViewItem.SubItems.Add(row["uri"].ToString().Replace("/api/storage", String.Empty));
                listView1.Items.Add(listViewItem);
            }
            ResizeColumnHeaders();

            Cursor.Current = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete selected artifacts?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;

                // Get all checked items and delete
                String artifact = "";
                try
                {
                    var removedItems = new List<ListViewItem>();
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.Checked)
                        {
                            artifact = item.SubItems[1].Text;
                            Uri URL = new Uri(artifact);
                            WebClient clt = new WebClient();
                            clt.Credentials = new NetworkCredential(AuthUser, AuthPwd);
                            String json = clt.UploadString(URL, "DELETE", "");
                            removedItems.Add(item);
                        }
                    }

                    // Now refresh the screen
                    listView1.BeginUpdate();
                    foreach (ListViewItem item in removedItems)
                        listView1.Items.Remove(item);
                    listView1.EndUpdate();

                }
                catch (WebException ex)
                {
                    MessageBox.Show(ex.Message, "Error deleting " + artifact, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

            }
            else
            {
                MessageBox.Show("Select at least 1 artifact to delete", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ResizeColumnHeaders()
        {
            listView1.BeginUpdate();
            for (int i = 0; i < this.listView1.Columns.Count - 1; i++) this.listView1.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView1.Columns[this.listView1.Columns.Count - 1].Width = -2;
            listView1.EndUpdate();
        }

        private void Resize_End(object sender, EventArgs e)
        {
            ResizeColumnHeaders();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
