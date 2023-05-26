using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WelLib.util;
using WinEventListener.service;
using WinEventListener.service.impl;
using WinEventListener.util;

namespace WinEventListenerApp.init
{
    public class ApiCenterCtlTabInit
    {
        private bool initing = false;
        // 配置信息
        private static string TAB_CONFIG_NAME = "ApiCenterCtlTab";
        private static string CONFIG_KEY_OPEN = "open";
        

        private IndexForm indexForm;
        private PjConfig.Config config = PjConfig.getConfig(TAB_CONFIG_NAME);

        private WelService ctlService;

        public ApiCenterCtlTabInit(IndexForm indexForm)
        {
            this.indexForm = indexForm;
        }

        public void init()
        {
            this.initing = true;
            this.initOpen();
            this.initSaveBtn();
            this.initSwitch();
            this.initDeviceName();
            this.initing = false;
            this.ctlService = new ApiCenterCtlService();
            bool open = this.indexForm.apiCenterCtlOpenSwitch.Checked;
            Dictionary<string, object> nowConfig = this.getConfigDic();
            this.onConfigChange(open, nowConfig);
        }
        private void initOpen()
        {
            this.indexForm.apiCenterCtlitlePanel.MouseClick += TitlePanel_MouseClick;
            this.indexForm.apiCenterCtlOpenSwitch.Checked = (bool)this.config.get(CONFIG_KEY_OPEN, false);

            this.indexForm.apiCenterCtlOpenSwitch.CheckedChanged += OpenSwitch_CheckedChanged;
            this.indexForm.apiCenterConfigPanel.Enabled = this.indexForm.apiCenterCtlOpenSwitch.Checked;
        }
        private void initSwitch()
        {
            this.indexForm.apiCenterCtlSystemLsUnlockEventSwitch.CheckStateChanged += ApiCenterCtlSystemEventSwitch_CheckStateChanged;
            this.indexForm.apiCenterCtlSystemLsUnlockEventSwitch.Checked = (bool)this.config.get(ApiCenterCtlService.CONFIG_KEY_SYSTEM_UNLOCK_EVENT_SWITCH, false);
            this.indexForm.apiCenterCtlLsSystemLockEventSwitch.CheckStateChanged += ApiCenterCtlSystemEventSwitch_CheckStateChanged;
            this.indexForm.apiCenterCtlLsSystemLockEventSwitch.Checked = (bool)this.config.get(ApiCenterCtlService.CONFIG_KEY_SYSTEM_LOCK_EVENT_SWITCH, false);
            this.indexForm.apiCenterCtlLockCtlSwitch.Checked = (bool)this.config.get(ApiCenterCtlService.CONFIG_KEY_LOCK_CTL_SWITCH, false);
            this.indexForm.apiCenterCtlLockCtlSwitch.CheckStateChanged += ApiCenterCtlSystemEventSwitch_CheckStateChanged;
        }
        private void initDeviceName()
        {
            this.indexForm.apiCenterCtlSelfDeviceNameInput.Text = this.config.get(ApiCenterCtlService.CONFIG_KEY_SELF_DEVICE_NAME, "").ToString();
            this.indexForm.apiCenterCtlNotiDeviceNameInput.Text = this.config.get(ApiCenterCtlService.CONFIG_KEY_NOTI_DEVICE_NAME, "").ToString();
        }

        private void ApiCenterCtlSystemEventSwitch_CheckStateChanged(object sender, EventArgs e)
        {
            this.configChange();
        }

        private void TitlePanel_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.indexForm.apiCenterCtlOpenSwitch.Checked = !this.indexForm.apiCenterCtlOpenSwitch.Checked;
        }

        private void initSaveBtn()
        {
            this.indexForm.apiCenterCtlSaveBtn.Click += SaveBtn_Click;
        }
        private Dictionary<string, object> getConfigDic()
        {
            bool open = this.indexForm.apiCenterCtlOpenSwitch.Checked;
            string selfDeviceName = this.indexForm.apiCenterCtlSelfDeviceNameInput.Text;
            string notiDeviceName = this.indexForm.apiCenterCtlNotiDeviceNameInput.Text;
            bool lockLs = this.indexForm.apiCenterCtlLsSystemLockEventSwitch.Checked;
            bool unlockLs = this.indexForm.apiCenterCtlSystemLsUnlockEventSwitch.Checked;
            bool lockCtl = this.indexForm.apiCenterCtlLockCtlSwitch.Checked;
            return new Dictionary<string, object>
            {
                { CONFIG_KEY_OPEN, open },
                { ApiCenterCtlService.CONFIG_KEY_SELF_DEVICE_NAME, selfDeviceName },
                { ApiCenterCtlService.CONFIG_KEY_NOTI_DEVICE_NAME, notiDeviceName },

                { ApiCenterCtlService.CONFIG_KEY_SYSTEM_LOCK_EVENT_SWITCH,  lockLs },
                { ApiCenterCtlService.CONFIG_KEY_SYSTEM_UNLOCK_EVENT_SWITCH, unlockLs },

                { ApiCenterCtlService.CONFIG_KEY_LOCK_CTL_SWITCH, lockCtl },
            };
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            bool open = this.indexForm.apiCenterCtlOpenSwitch.Checked;
            Dictionary<string, object> newConfig = this.getConfigDic();
            this.config.set(newConfig);
            this.setSaveBtnEnable(false);
            this.onConfigChange(open, newConfig);
        }
        /// <summary>
        /// 监听配置变化事件
        /// </summary>
        private void onConfigChange(bool open, Dictionary<string, object> serviceParams = null)
        {
            if (open && this.ctlService == null)
            {
                this.ctlService = new ApiCenterCtlService();
            }
            // 先检查目前开启状态
            if (open && this.ctlService.isRunning())
            {
                this.ctlService.reload(new ServiceParams(serviceParams));
            }
            else if (!open && this.ctlService.isRunning())
            {
                this.ctlService.stop();
                this.indexForm.unRegService(this.ctlService);
            }else if (open)
            {
                this.ctlService.run(new ServiceParams(serviceParams));
                this.indexForm.regService(this.ctlService);
            }
        }

        private void OpenSwitch_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = this.indexForm.apiCenterCtlOpenSwitch.Checked;
            this.indexForm.apiCenterConfigPanel.Enabled = isChecked;
            if (!isChecked)
            {
                this.onConfigChange(false);
                // 保存配置
                Dictionary<string, object> newConfig = this.getConfigDic();
                this.config.set(newConfig);
            }
            else
            {
                this.configChange();
            }
        }

        private void setSaveBtnEnable(bool enable)
        {
            this.indexForm.apiCenterCtlSaveBtn.Enabled = enable;
        }

        private void configChange()
        {
            if (!this.initing)
            {
                this.setSaveBtnEnable(true);
            }
        }
    }
}
