using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WelLib.util;
using WinEventListener.util;

namespace WinEventListenerApp.init
{
    /// <summary>
    /// 总览页面初始化
    /// </summary>
    public class FileWatchTabInit
    {
        // 配置信息
        private static string TAB_CONFIG_NAME = "FileWatchTab";
        private static string CONFIG_KEY_OPEN = "open";
        private static string CONFIG_KEY_FILE_PATH = "filePath";

        private IndexForm indexForm;
        private PjConfig.Config config = PjConfig.getConfig(TAB_CONFIG_NAME);
        public FileWatchTabInit(IndexForm indexForm)
        {
            this.indexForm = indexForm;
        }

        public void init()
        {
            this.initOpen();
            this.initSaveBtn();
            this.initFileSelector();
        }

        private void initFileSelector()
        {
            this.indexForm.fileWatchPathTextBox.Text = this.config.get(CONFIG_KEY_FILE_PATH, "").ToString();
            this.indexForm.fileWatchPathTextBox.KeyPress += FileWatchPathTextBox_KeyPress;
            this.indexForm.fileWatchFileSelectorBtn.MouseClick += FileWatchFileSelectorBtn_MouseClick;
        }

        private void FileWatchPathTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.setSaveBtnEnable(true);
        }

        private void FileWatchFileSelectorBtn_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                this.indexForm.fileWatchPathTextBox.Text = fbd.SelectedPath;
                this.setSaveBtnEnable(true);
            }
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.RestoreDirectory = true;
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    this.indexForm.fileWatchPathTextBox.Text = dialog.FileName;
            //    this.setSaveBtnEnable(true);
            //}
        }

        /// <summary>
        /// 开机启动开关
        /// </summary>
        private void initOpen()
        {
            this.indexForm.fileWatchTitlePanel.MouseClick += FileWatchTitlePanel_MouseClick;
            this.indexForm.fileWatchOpenSwitch.Checked = (bool)this.config.get(CONFIG_KEY_OPEN, false);
            this.indexForm.fileWatchOpenSwitch.CheckedChanged += FileWatchOpenSwitch_CheckedChanged;
            this.indexForm.fileWatchConfigPanel.Enabled = this.indexForm.fileWatchOpenSwitch.Checked;
        }

        private void FileWatchTitlePanel_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.indexForm.fileWatchOpenSwitch.Checked = !this.indexForm.fileWatchOpenSwitch.Checked;
        }

        private void initSaveBtn()
        {
            this.indexForm.fileWatchSaveBtn.Click += FileWatchSaveBtn_Click;
        }

        private void FileWatchSaveBtn_Click(object sender, EventArgs e)
        {
            // 配置检查
            if (!Directory.Exists(this.indexForm.fileWatchPathTextBox.Text))
            {
                this.indexForm.showMessage("配置错误", "监听路径不存在");
                return;
            }
            // 批量保存配置
            Dictionary<string, object> newConfig = new Dictionary<string, object>
            {
                { CONFIG_KEY_OPEN, this.indexForm.fileWatchOpenSwitch.Checked },
                { CONFIG_KEY_FILE_PATH, this.indexForm.fileWatchPathTextBox.Text }
            };
            this.config.set(newConfig);
            this.setSaveBtnEnable(false);
        }

        private void FileWatchOpenSwitch_CheckedChanged(object sender, EventArgs e)
        {
            this.indexForm.fileWatchConfigPanel.Enabled = this.indexForm.fileWatchOpenSwitch.Checked;
            this.setSaveBtnEnable(true);
        }

        private void setSaveBtnEnable(bool enable)
        {
            this.indexForm.fileWatchSaveBtn.Enabled = enable;
        }
    }
}
