using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TimeTracker.Model;
using TimeTracker.Properties;

namespace TimeTracker.Form
{
    /// <summary>
    /// Main application form class
    /// </summary>
    public partial class Application : System.Windows.Forms.Form
    {
        /// <summary>
        /// Data file Extension
        /// </summary>
        const String FILE_EXT = "timetracker";

        /// <summary>
        /// Default data file name
        /// </summary>
        const String FILE_NAME = "table";

        /// <summary>
        /// Maximum length of category name
        /// </summary>
        const int CATEGORY_MAXLENGTH = 255;

        /// <summary>
        /// Contains tracker data
        /// </summary>
        private BindingList<TimeTrackerData> Data;

        /// <summary>
        /// Tracking service that handles a single tracker
        /// </summary>
        private TrackingService TrackingService;

        /// <summary>
        /// Timer used for UI refreshes
        /// </summary>
        private Timer RefreshTimer;

        /// <summary>
        /// Tooltip used in the form
        /// </summary>
        private ToolTip toolTip = new ToolTip();

        /// <summary>
        /// Opened file (or stream)
        /// </summary>
        private FileInfo file;

        /// <summary>
        /// Saves the initial culture
        /// </summary>
        private static readonly CultureInfo defaultCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Stores whether the current state is saved
        /// </summary>
        private bool isSaved = true;

        /// <summary>
        /// Main form of the application starts here
        /// </summary>
        public Application()
        {
            Data = new BindingList<TimeTrackerData>();

            TrackingService = new TrackingService();

            RefreshTimer = new Timer
            {
                Interval = 100
            };
            RefreshTimer.Tick += new System.EventHandler(RefreshTrackingInfo);

#if false
            // Test data
            TrackedDataCategory cat = new TrackedDataCategory("TestCat");
            Data.Add(new TimeTrackerData(DateTimeOffset.Now.AddHours(14), cat));
            Data.Add(new TimeTrackerData(DateTimeOffset.Now.AddDays(14), cat));
            Data.Add(new TimeTrackerData(DateTimeOffset.Now.AddDays(14).AddMinutes(8), cat));
            Data.Add(new TimeTrackerData(DateTimeOffset.Now.AddHours(2).AddSeconds(66), cat));
            Data.Add(new TimeTrackerData(DateTimeOffset.Now));
            Data.Add(new TimeTrackerData(DateTimeOffset.Now.AddSeconds(81), cat));
#endif

            InitializeComponent();

            // TODO: Finish add and copy buttons
            // Remove not implemented buttons
            this.toolStripMain.Items.RemoveByKey("addToolStripButton");
            this.toolStripMain.Items.RemoveByKey("copyToolStripButton");

            this.dataGridViewMain.DataSource = Data;
            this.categoryToolStripComboBox.MaxLength = CATEGORY_MAXLENGTH;


            this.Refresh();
            Data.ListChanged += new ListChangedEventHandler(DataListChanged);
            RefreshTitle();
            RefreshTrackingButtons();
            RefreshEditButtons();
            RefreshFileButtons();
            RefreshStatistics();
            LoadSettings();

            BuildLanguageSelection();
        }

        /// <summary>
        /// Fills the language selection menu
        /// </summary>
        private void BuildLanguageSelection()
        {
            Dictionary<string, string> items = new Dictionary<string, string>(){
                {"none", Resources.Application_language_default },
                {"en",  "English" },
                {"cs",  "Čeština" },
            };

            foreach (var item in items)
            {
                ResourceManager resourceManager = new ResourceManager(typeof(Resources));
                string localizedLanguageName = resourceManager.GetString("Application_language_" + item.Key);
                var menuItem = new ToolStripMenuItem
                {
                    Text = localizedLanguageName == null ? item.Value : string.Format("{0} ({1})", localizedLanguageName, item.Value),
                    Checked = Settings.Default.language == item.Key,
                };
                menuItem.Click += (sender, e) => LanguageItemClicked(sender, e, item.Key);

                this.languageToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void LanguageItemClicked(object sender, EventArgs e, string language)
        {
            // set the language
            Settings.Default.language = language;

            if (!(sender is ToolStripMenuItem))
            {
                return;
            }

            // reset all items
            var items = this.languageToolStripMenuItem.DropDownItems;
            foreach (ToolStripMenuItem item in items)
            {
                if (!(item is ToolStripMenuItem))
                {
                    continue;
                }

                var menuItem = (ToolStripMenuItem)item;
                menuItem.Checked = false;
            }

            // check clicked item
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            clickedItem.Checked = true;

            // warn user that they need to restart
            MessageBox.Show(this, Resources.Application_languageChangedMessageBox_Message, Resources.Application_languageChangedMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Loads the application's settings
        /// </summary>
        private void LoadSettings()
        {
            // set the menu options to correct values
            this.alwaysOnTopToolStripMenuItem.Checked = Settings.Default.alwaysOnTop;
            this.showInTaskbarToolStripMenuItem.Checked = Settings.Default.showInTaskbar;
            this.showInNotificationAreaToolStripMenuItem.Checked = Settings.Default.showInNotificationArea;

            // run the event handlers
            this.alwaysOnTopToolStripMenuItem_CheckedChanged(null, null);
            this.showInTaskbarToolStripMenuItem_CheckedChanged(null, null);
            this.showInNotificationAreaToolStripMenuItem_CheckedChanged(null, null);
        }

        /// <summary>
        /// Saves the application's settings
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.alwaysOnTop = this.alwaysOnTopToolStripMenuItem.Checked;
            Settings.Default.showInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
            Settings.Default.showInNotificationArea = this.showInNotificationAreaToolStripMenuItem.Checked;

            Settings.Default.Save();
        }

        /// <summary>
        /// Sets application title dynamically
        /// </summary>
        private void RefreshTitle()
        {
            var text = ProductName;

            if (file != null && file.Exists && file.Name.Length > 0)
            {
                var modifier = isSaved ? "" : "*";
                text = String.Format("{1}{2} - {0}", text, file.Name, modifier);
            }

            this.Text = text;
            notifyIcon.Text = text;
        }

        private void RefreshFileButtons()
        {
            bool saveAvailable = SaveAvailable();
            this.saveToolStripButton.Enabled = saveAvailable;
            this.saveToolStripMenuItem.Enabled = saveAvailable;
            this.closeToolStripMenuItem.Enabled = file != null;

            // nothing to save
            this.saveAsToolStripMenuItem.Enabled = Data.Count > 0;
        }

        private void RefreshStatistics()
        {
            TimeSpan statTotal = Data.Sum(value => value.GetTimeElapsed());
            this.statsTotalText.Text = String.Format(Properties.Resources.Application_statsTotal_Text, statTotal.Format());

            DataGridView grid = this.dataGridViewMain;

            if (grid.SelectedRows.Count > 1)
            {
                var selectionEnumerator = grid.SelectedRows.GetEnumerator();
                TimeSpan statSelection = new TimeSpan();
                while (selectionEnumerator.MoveNext())
                {
                    var row = (DataGridViewRow)selectionEnumerator.Current;
                    var data = (TimeTrackerData)row.DataBoundItem;
                    statSelection = statSelection.Add(data.GetTimeElapsed());
                }

                this.statsSelectedText.Text = String.Format(Properties.Resources.Application_statsSelected_Text, grid.SelectedRows.Count, statSelection.Format());
                this.statsSelectedText.Visible = true;
            }
            else
            {
                this.statsSelectedText.Visible = false;
            }

            if (grid.SelectedRows.Count == 1)
            {
                var selected = (TimeTrackerData)grid.SelectedRows[0].DataBoundItem;
                var category = selected.Category;
                TimeSpan statCategory = Data.Where(value => category == null ? value.Category == null : value.Category != null && value.Category.Equals(category)).Sum(value => value.GetTimeElapsed());

                this.statsCategoryText.Text = String.Format(Properties.Resources.Application_statsCategory_Text, category == null ? "" : category.Name, statCategory.Format());
                this.statsCategoryText.Visible = true;
            }
            else
            {
                this.statsCategoryText.Visible = false;
            }
        }

        /// <summary>
        /// Called on data list change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataListChanged(object sender, ListChangedEventArgs e)
        {
            isSaved = false;
            RefreshFileButtons();
            RefreshTitle();
            RefreshStatistics();
        }


        /// <summary>
        /// Exit application when user clicks on File > Exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();

        }

        /// <summary>
        /// Display "about" dialog box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void dataGridViewMain_SelectionChanged(object sender, EventArgs e)
        {
            RefreshEditButtons();
            RefreshStatistics();
        }

        /// <summary>
        /// Refreshes grid edit buttons when necessary to reflect options available to do with that data
        /// </summary>
        private void RefreshEditButtons()
        {
            DataGridView grid = this.dataGridViewMain;
            var count = grid.SelectedRows.Count;

            // Decide on delete button
            if (count < 1)
            {
                deleteToolStripButton.Enabled = false;
            }
            else
            {
                deleteToolStripButton.Enabled = true;
            }
        }

        private void deleteToolStripButton_Click(object sender, EventArgs e)
        {
            DataGridView grid = this.dataGridViewMain;
            var count = grid.SelectedRows.Count;

            var result = MessageBox.Show(this, String.Format(Properties.Resources.Application_deleteMessageBox_Message, count),
                Resources.Application_deleteMessageBox_Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                foreach (DataGridViewRow selectedRow in grid.SelectedRows)
                {
                    Data.Remove(selectedRow.DataBoundItem as TimeTrackerData);
                }
                RefreshCategoryPicker();
            }

        }

        private void startTrackingToolStripButton_Click(object sender, EventArgs e)
        {
            TrackingService.Start();
            RefreshTimer.Start();
            RefreshTrackingButtons();
            this.trackingStartTimeToolStripTextBox.Text = TrackingService.StartTime.LocalDateTime.ToString("h\\:mm\\:ss");
            this.trackingElapsedTimeToolStripTextBox.Text = TrackingService.Elapsed;
        }

        private void stopTrackingToolStripButton_Click(object sender, EventArgs e)
        {
            RefreshTimer.Stop();

            TimeTrackerData item = TrackingService.Stop();
            // fill in category
            if (categoryToolStripComboBox.Text.Length > 0)
            {
                item.Category = new TrackedDataCategory(categoryToolStripComboBox.Text.Trim(' '));
            }

            Data.Add(item);
            RefreshTrackingButtons();
            RefreshCategoryPicker();
        }

        private void RefreshTrackingButtons()
        {
            var tracking = TrackingService.Tracking;

            this.startTrackingToolStripButton.Enabled = !tracking;
            this.stopTrackingToolStripButton.Enabled = tracking;
        }

        private void RefreshTrackingInfo(object sender, EventArgs e)
        {
            this.trackingElapsedTimeToolStripTextBox.Text = TrackingService.Elapsed;
        }

        /// <summary>
        /// Saves the current table to file
        /// </summary>
        /// <param name="forceOpenSaveWindow">Whether to open the save dialogue even when the target path is already known</param>
        private void Save(bool forceOpenSaveWindow = false)
        {
            if (forceOpenSaveWindow || file == null)
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    DefaultExt = FILE_EXT,
                    FileName = "table",
                    Filter = String.Format("TimeTracker files (*.{0})|*.{0}|All files (*.*)|*.*", FILE_EXT),

                    // use directory with "current" file if available
                    InitialDirectory = file == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : file.DirectoryName
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    file = new FileInfo(dialog.FileName);
                }
            }

            if (file != null)
            {
                StreamWriter fs = null;
                try
                {
                    file.Delete();
                    fs = file.AppendText();
                    fs.Write(DataSerializer.Serialize(Data));
                }
                catch (Exception)
                {
                    MessageBox.Show(this, Properties.Resources.Application_fileErrorMessageBox_Message,
                    Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                finally
                {
                    fs.Close();
                }
                isSaved = true;
            }

            RefreshFileButtons();
            RefreshTitle();
        }

        /// <summary>
        /// Opens an existing file with table data
        /// </summary>
        private void Open()
        {
            SaveIfNecessary();

            OpenFileDialog dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                DefaultExt = FILE_EXT,
                FileName = FILE_NAME,
                Filter = String.Format("TimeTracker files (*.{0})|*.{0}|All files (*.*)|*.*", FILE_EXT),

                // use directory with "current" file if available
                InitialDirectory = file == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : file.DirectoryName
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                file = new FileInfo(dialog.FileName);
            }

            if (file != null)
            {
                if (!file.Exists)
                {
                    MessageBox.Show(this, Resources.Application_nonexistentFileMessageBox_Message,
                    Resources.Application_nonexistentFileMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StreamReader fs = null;
                try
                {
                    Data.Clear();
                    fs = file.OpenText();

                    string line;
                    while ((line = fs.ReadLine()) != null)
                    {
                        TimeTrackerData value = DataSerializer.DeserializeValue(line, CATEGORY_MAXLENGTH);
                        Data.Add(value);
                    }
                }
                catch (DeserializationException)
                {
                    MessageBox.Show(this, Resources.Application_fileErrorMessageBox_Message,
                    Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    file = null;

                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show(this, Resources.Application_fileErrorMessageBox_Message,
                    Resources.Application_fileErrorMessageBox_Caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                finally
                {
                    fs.Close();
                }
                isSaved = true;
            }

            RefreshFileButtons();
            RefreshTitle();
            RefreshCategoryPicker();
        }

        /// <summary>
        /// Checks whether there are any changes to the table, offering the user the option to save them
        /// </summary>
        /// <returns></returns>
        private DialogResult SaveIfNecessary()
        {
            if (SaveAvailable())
            {
                var result = MessageBox.Show(this, Properties.Resources.Application_unsavedMessageBox_Message,
                    Resources.Application_unsavedMessageBox_Caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Save();
                }

                return result;
            }

            return DialogResult.Abort;
        }

        /// <summary>
        /// Checks whether save should be available
        /// </summary>
        /// <returns></returns>
        private bool SaveAvailable()
        {
            return !isSaved && Data.Count > 0;
        }

        /// <summary>
        /// Application closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SaveIfNecessary() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// Collects all categories from the data set and returns a unique set
        /// </summary>
        /// <returns>The set of categories</returns>
        private HashSet<TrackedDataCategory> GetUsedCategories()
        {
            var result = new HashSet<TrackedDataCategory>();
            foreach (var value in Data)
            {
                if (value.Category == null)
                {
                    continue;
                }

                result.Add(value.Category);
            }

            return result;
        }

        /// <summary>
        /// Updates the category picker with newly-collected categories
        /// </summary>
        private void RefreshCategoryPicker()
        {
            var items = this.categoryToolStripComboBox.Items;
            items.Clear();
            items.AddRange(GetUsedCategories().ToArray());
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveIfNecessary();
            Data.Clear();

            // No need to close handles here, FileInfo doesn't use them
            file = null;
            isSaved = true;

            RefreshFileButtons();
            RefreshEditButtons();
            RefreshTitle();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // It turns out that the "new" action does exactly the same as the "close" action
            closeToolStripMenuItem_Click(sender, e);
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // shortcut for the "new" menu item
            newToolStripMenuItem_Click(sender, e);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            // shortcut for the "open" menu item
            openToolStripMenuItem_Click(sender, e);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            // shortcut for the "save" menu item
            saveToolStripMenuItem_Click(sender, e);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(true);
        }

        /// <summary>
        /// Draws a custom message inside the grid view when it's empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewMain_Paint(object sender, PaintEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.Rows.Count == 0)
            {
                var font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

                // figure out label position
                System.Drawing.SizeF labelSize = e.Graphics.MeasureString(Resources.Application_noDataLabel, font);
                float vertPos = (grid.Width - labelSize.Width) / 2;
                float horizPos = (grid.Height + grid.ColumnHeadersHeight - labelSize.Height) / 2;

                e.Graphics.DrawString(Resources.Application_noDataLabel, font, System.Drawing.Brushes.DimGray, new System.Drawing.PointF(vertPos < 0 ? 0 : vertPos, horizPos < grid.ColumnHeadersHeight ? grid.ColumnHeadersHeight : horizPos));
            }
        }

        private void dataGridViewMain_Resize(object sender, EventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (grid.Rows.Count == 0)
            {
                // repaint whole area to make sure our custom text gets drawn in correct place
                grid.Invalidate();
            }
        }

        private void categoryToolStripComboBox_TextUpdate(object sender, EventArgs e)
        {
            // make sure that there are no invalid characters in the category field
            ToolStripComboBox box = (ToolStripComboBox)sender;
            var text = box.Text;
            var original = text;

            Regex regex = new Regex("[^-_: \\w]");
            text = regex.Replace(text, "");

            if (original != text)
            {
                toolTip.Hide(this.categoryToolStripComboBox.Control);
                toolTip.Show(String.Format(Resources.Application_categoryToolTip_Text, "-_: "), this.categoryToolStripComboBox.Control, 5000);
            }

            if (text.Length > CATEGORY_MAXLENGTH)
            {
                text = text.Substring(0, CATEGORY_MAXLENGTH);
            }

            box.Text = text;
        }

        private void alwaysOnTopToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        private void showInTaskbarToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.ShowInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
        }

        private void showInNotificationAreaToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.notifyIcon.Visible = this.showInNotificationAreaToolStripMenuItem.Checked;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // restore
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = this.showInTaskbarToolStripMenuItem.Checked;
            }
            else
            {
                // hide
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }
    }
}