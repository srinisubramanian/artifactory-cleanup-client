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
using System.Threading;
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
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

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
            if (listView1.CheckedItems.Count <= 0)
            {
                MessageBox.Show("Select at least 1 artifact to delete", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Delete " + listView1.CheckedItems.Count + " checked artifacts?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (backgroundWorker1.IsBusy != true)
                {
                    var checkedItems = new List<Uri>();
                    foreach (ListViewItem item in listView1.CheckedItems)
                    {
                        {
                            checkedItems.Add(new Uri(item.SubItems[1].Text));
                        }
                    }
                    this.label1.Text = "Starting";
                    this.button1.Enabled = false;
                    //this.button2.Enabled = false;
                    button2.Text = "Cancel";
                    this.button3.Enabled = false;
                    this.button4.Enabled = false;

                    // Start the asynchronous operation.
                    backgroundWorker1.RunWorkerAsync(new object [] {checkedItems});
                }
            }
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


        private void button4_Click(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            bool bSelect = (button4.Text.Equals("Select All"));
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = bSelect;
            }
            listView1.EndUpdate();
            button4.Text = bSelect ? "Deselect All" : "Select All";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = !item.Checked;
            }
            listView1.EndUpdate();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get all checked items and delete
            object[] parameters = e.Argument as object[];
            List<Uri> checkedItems = parameters[0] as List<Uri>;

            // Get all checked items and delete
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                int done = 0;
                int total = checkedItems.Count;
                foreach (Uri url in checkedItems)
                {
                    {
                        WebClient clt = new WebClient();
                        clt.Credentials = new NetworkCredential(AuthUser, AuthPwd);
                        String json = clt.UploadString(url, "DELETE", "");
                        done++;
                        worker.ReportProgress(100 * done / total);

                        if ((worker.CancellationPending == true))
                        {
                            e.Cancel = true;
                            break;
                        }
                    }
                }

            }
            catch (WebException ex)
            {
                Console.WriteLine("Error deleting: " + ex.Message);
            }
            finally
            {
            }
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Refresh the UI
            if (!e.Cancelled)
            {
                this.label1.Text = "Completed";
                this.button1.Enabled = true;
                this.button3.Enabled = true;
                this.button4.Enabled = true;

                listView1.BeginUpdate();
                foreach (ListViewItem item in listView1.CheckedItems)
                {
                    {
                        listView1.Items.Remove(item);
                    }
                }

                listView1.EndUpdate();
            }
            else
            {
                this.label1.Text = "Cancelled";
            }

            // Enable close always
            button2.Text = "Close"; 
            this.button2.Enabled = true;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.label1.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (button2.Text.Equals("Close"))
                Close();

            // Cancel the asynchronous operation. 
            this.backgroundWorker1.CancelAsync();

            // Disable the Cancel button.
            button2.Text = "Close";
            button2.Enabled = false;
        }
    }
}
