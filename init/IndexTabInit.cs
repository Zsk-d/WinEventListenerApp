using System;
using System.Windows.Forms;
using WelLib.util;
using WinEventListener.util;

namespace WinEventListenerApp.init
{
    /// <summary>
    /// 总览页面初始化
    /// </summary>
    public class IndexTabInit
    {
        private static string AUTO_RUN_NAME = "WelApp";
        // 配置信息
        private static string TAB_CONFIG_NAME = "IndexTab";
        private static string CONFIG_KEY_AUTO_RUN = "autoRun";

        private IndexForm indexForm;
        private PjConfig.Config config = PjConfig.getConfig(TAB_CONFIG_NAME);
        public IndexTabInit(IndexForm indexForm)
        {
            this.indexForm = indexForm;
        }

        public void init()
        {
            this.initAutoRun();
            // 退出按钮事件
            this.indexForm.exitBtn.Click += ExitBtn_Click;
            // 设置右键菜单事件
            foreach (MenuItem item in this.indexForm.MainNotifyIcon.ContextMenu.MenuItems)
            {
                if (item.Text == "退出")
                {
                    item.Click += Context_Menu_Item_Exit_Click;
                }
            }
        }
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            this.exitApp();
        }

        private void Context_Menu_Item_Exit_Click(object sender, EventArgs e)
        {
            this.exitApp();
        }

        /// <summary>
        /// 开机启动开关
        /// </summary>
        private void initAutoRun()
        {
            this.indexForm.autoRunSwitch.Checked = (bool)this.config.get(CONFIG_KEY_AUTO_RUN, false);
            this.indexForm.autoRunSwitch.CheckedChanged += AutoRunSwitch_CheckedChanged;
        }
        private void AutoRunSwitch_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = ((System.Windows.Forms.CheckBox)sender).Checked;
            try
            {
                if (isChecked)
                {
                    SystemUtil.setAutoRun(AUTO_RUN_NAME, this.GetType().Assembly.Location);
                }
                else
                {
                    SystemUtil.removeAutoRun(AUTO_RUN_NAME);
                }
                this.config.set(CONFIG_KEY_AUTO_RUN, isChecked);
            }
            catch (Exception ex)
            {
                this.indexForm.showMessage("操作失败" , ex.Message);
            }
        }

        private void exitApp()
        {
            // 通知各服务关闭
            this.indexForm.notifyServiceStop();
            Application.Exit();
            System.Environment.Exit(0);
        }
    }
}
