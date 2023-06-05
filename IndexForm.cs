using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinEventListener.service;
using WinEventListenerApp.init;

namespace WinEventListenerApp
{
    public partial class IndexForm : Form
    {
        private List<WelService> regedServices = new List<WelService>();
        public IndexForm()
        {
            InitializeComponent();
            try
            {
                this.customerUIInit();
            }
            catch (Exception e)
            {
                this.showMessage("启动失败",e.Message);
            }
        }

        public void showMessage(string title, string content)
        {
            MessageBox.Show(content, title);
        }
        public bool regService(WelService welService)
        {
            if (!regedServices.Contains(welService))
            {
                regedServices.Add(welService);
            }
            return true;
        }
        public bool unRegService(WelService welService)
        {
            if (regedServices.Contains(welService))
            {
                regedServices.Remove(welService);
            }
            return true;
        }
        public void notifyServiceStop()
        {
            foreach(var service in this.regedServices)
            {
                service.stop();
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void customerUIInit()
        {
            // 绑定显示隐藏窗口事件
            this.SizeChanged += IndexForm_OnSizeChaned;
            // 初始化托盘左键点击事件
            this.MainNotifyIcon.MouseClick += MainNotifyIcon_Click;
            // 拦截关闭事件
            this.FormClosing += IndexForm_FormClosing;
            // 托盘绑定右键菜单
            this.MainNotifyIcon.ContextMenu = this.MainContextMenu;
            new IndexTabInit(this).init();
            new FileWatchTabInit(this).init();
            new KeyboardWatchTabInit(this).init();
            new ApiCenterCtlTabInit(this).init();
            // 默认不显示主窗口
            //this.showNotifyIcon();
            //WindowState = FormWindowState.Minimized;
        }

        private void IndexForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.showNotifyIcon();
            e.Cancel = true;
        }

        private void MainNotifyIcon_Click(object sender, MouseEventArgs e)
        {
            // 判断左键右键
            if (e.Button == MouseButtons.Left)
            {
                this.WindowState = FormWindowState.Normal;
                this.hideNotifyIcon();
            }
            else if(e.Button == MouseButtons.Right)
            {

            }
        }

        /// <summary>
        /// 主界面窗口大小变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void IndexForm_OnSizeChaned(object sender, EventArgs args)
        {
            // 最小化时缩小到托盘
            if (WindowState == FormWindowState.Minimized)
            {
            }
            else
            {
            }
        }

        private void showNotifyIcon()
        {
            this.ShowInTaskbar = false;
            this.MainNotifyIcon.Visible = true;
        }
        private void hideNotifyIcon()
        {
            this.ShowInTaskbar = true;
            this.MainNotifyIcon.Visible = false;
        }
    }
}