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
    public partial class RepList : Form
    {
        public String ArtifactoryURL { get; set; }
        public String AuthUser { get; set; }
        public String AuthPwd { get; set; }

        private ProgressBar progress;

        public RepList()
        {
            InitializeComponent();
        }

        private void RepList_Load(object sender, EventArgs e)
        {
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

            progress = null;

            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd-MM-yyyy hh:mm:ss tt";
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.CustomFormat = "dd-MM-yyyy hh:mm:ss tt";
            dateTimePicker2.Value = DateTime.Now;

            // Disable all the controls till a successful login
            listBox1.Enabled = false;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the values
            ArtifactoryURL = textBox1.Text;
            AuthUser = textBox2.Text;
            AuthPwd = textBox3.Text;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Load the virtual repositories
                Uri URL = new Uri(ArtifactoryURL + Constants.RepositoryURL);
                WebClient clt = new WebClient();
                clt.Credentials = new NetworkCredential(AuthUser, AuthPwd);
                byte[] buffer = clt.DownloadData(URL);
                String json = System.Text.Encoding.UTF8.GetString(buffer);
                List<Repository> reps = JsonConvert.DeserializeObject<List<Repository>>(json);
                listBox1.Items.Clear();

                foreach (Repository rep in reps) {
                    listBox1.Items.Add(rep.key);
                }

                // Enable all controls
                listBox1.Enabled = true;
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;

                // Disable login and the rest
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                String rep = listBox1.GetItemText(listBox1.SelectedItem).Trim().Replace("/", String.Empty);
                if (!String.IsNullOrEmpty(rep))
                {
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);
                    TimeSpan tNow = dateTimePicker2.Value.ToUniversalTime() - epoch;
                    UInt64 msecondsTo = (UInt64) tNow.TotalMilliseconds;

                    TimeSpan tFrom = dateTimePicker1.Value.ToUniversalTime() - epoch;
                    UInt64 msecondsFrom = (UInt64) tFrom.TotalMilliseconds;

                    Uri URLSearch = new Uri(ArtifactoryURL + Constants.SearchURL + "from=" + msecondsFrom + "&to=" + msecondsTo + "&repos=" + rep);

                    WebClient clt = new WebClient();
                    clt.Credentials = new NetworkCredential(AuthUser, AuthPwd);

                    // Get the artifacts and add
                    byte[] buffer = clt.DownloadData(URLSearch);
                    String json = System.Text.Encoding.UTF8.GetString(buffer);

                    DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(json);
                    DataTable dataTable = dataSet.Tables["results"];
                    if (dataTable.Rows.Count > 0)
                    {
                        ArtifactList delform = new ArtifactList();
                        delform.ArtifactoryURL = ArtifactoryURL;
                        delform.Repository = rep;
                        delform.AuthUser = AuthUser;
                        delform.AuthPwd = AuthPwd;
                        delform.artifactList = dataTable;
                        delform.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("No artifacts found for the search", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Select repository", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (System.Net.HttpWebResponse) ex.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                    MessageBox.Show("No artifacts found", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete empty folders?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (backgroundWorker1.IsBusy != true)
                {
                    progress = new ProgressBar();
                    backgroundWorker1.RunWorkerAsync(new object[] { listBox1.GetItemText(listBox1.SelectedItem).Trim().Replace("/", String.Empty) });
                    progress.ShowDialog();
                    progress = null;
                }
            }
        }

        private void RecursivelyDeleteAllEmptyFolders(String repository)
        {
            try
            {
                // Load the virtual repositories
                Uri URL = new Uri(ArtifactoryURL + Constants.StorageURL + repository);
                WebClient clt = new WebClient();
                clt.Credentials = new NetworkCredential(AuthUser, AuthPwd);
                byte[] buffer = clt.DownloadData(URL);
                String json = System.Text.Encoding.UTF8.GetString(buffer);
                FolderInfo fldrInfo = JsonConvert.DeserializeObject<FolderInfo>(json);
                foreach (Folder fldr in fldrInfo.children)
                {
                    if (fldr.folder.ToLower().Equals("true"))
                        RecursivelyDeleteAllEmptyFolders(repository + fldr.uri);
                }
                // Check if folder empty by getting info again
                buffer = clt.DownloadData(URL);
                json = System.Text.Encoding.UTF8.GetString(buffer);
                fldrInfo = JsonConvert.DeserializeObject<FolderInfo>(json);
                if (fldrInfo.children.Count == 0)
                {
                    // Empty folder
                    json = clt.UploadString(fldrInfo.uri.Replace("/api/storage", String.Empty), "DELETE", "");
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "Error deleting", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            String repository = parameters[0] as String;

            RecursivelyDeleteAllEmptyFolders(repository);

            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(100);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Close();
        }

    }
}
