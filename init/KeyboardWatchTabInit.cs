﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WelLib.hook;
using WelLib.util;
using WinEventListener.util;

namespace WinEventListenerApp.init
{
    /// <summary>
    /// 总览页面初始化
    /// </summary>
    public class KeyboardWatchTabInit
    {
        // 配置信息
        private static string TAB_CONFIG_NAME = "KeyBoardWatchTab";
        private static string CONFIG_KEY_OPEN = "open";

        private static string KEY_PRESS_COUNT_MAP_PATH = FileUtil.getPjRootDir() + "\\keyboard-press-count.json";

        private IndexForm indexForm;
        private PjConfig.Config config = PjConfig.getConfig(TAB_CONFIG_NAME);
        private Dictionary<string, object> keyPressCountMap;

        private KeyboardHook keyboardHook = null;
        private int currentKeyPressCount = 0;
        private Thread autoSaveThread;

        public KeyboardWatchTabInit(IndexForm indexForm)
        {
            this.indexForm = indexForm;
        }

        public void init()
        {
            this.initOpen();
            this.initSaveBtn();
            this.initMap();
            this.checkWatch(this.indexForm.keyboardWatchSwitch.Checked);
        }
        
        private void initMap()
        {
            this.keyPressCountMap = FileUtil.loadMap(KEY_PRESS_COUNT_MAP_PATH);
        }

        private void autoSave()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000 * 60 * 5);
                    if (this.currentKeyPressCount > 100)
                    {
                        this.currentKeyPressCount = 0;
                        FileUtil.saveObjectJson(this.keyPressCountMap, KEY_PRESS_COUNT_MAP_PATH);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 开机启动开关
        /// </summary>
        private void initOpen()
        {
            this.indexForm.keyboardWatchSwitchPanel.MouseClick += OpenPanel_MouseClick;
            this.indexForm.keyboardWatchSwitch.Checked = (bool)this.config.get(CONFIG_KEY_OPEN, false);
            this.indexForm.keyboardWatchSwitch.CheckedChanged += OpenSwitch_CheckedChanged;
        }

        private void OpenPanel_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.indexForm.keyboardWatchSwitch.Checked = !this.indexForm.keyboardWatchSwitch.Checked;
        }

        private void initSaveBtn()
        {
            this.indexForm.keyboardWatchSaveBtn.Click += SaveBtn_Click;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            bool isOpen = this.indexForm.keyboardWatchSwitch.Checked;
            // 批量保存配置
            Dictionary<string, object> newConfig = new Dictionary<string, object>
            {
                { CONFIG_KEY_OPEN, isOpen }
            };
            this.config.set(newConfig);
            this.setSaveBtnEnable(false);
            // 检查监听状态
            this.checkWatch(isOpen);
        }
        private void OnKeyPress(KeyboardHook.HookStruct hookStruct, out bool handle)
        {
            handle = false;
            string key = hookStruct.vkCode.ToString();
            if (!this.keyPressCountMap.ContainsKey(key))
            {
                this.keyPressCountMap.Add(key, 0);
            }
            this.keyPressCountMap[key] = Convert.ToDouble(this.keyPressCountMap[key]) + 0.5;
            //Console.WriteLine(ObjectUtil.ObjectToJson(this.keyPressCountMap));
            this.currentKeyPressCount ++;
        }
        private void checkWatch(bool isOpen)
        {
            if (isOpen)
            {
                if (this.keyboardHook == null)
                {
                    this.keyboardHook = new KeyboardHook();
                    this.keyboardHook.InstallHook(this.OnKeyPress);
                    this.autoSaveThread = new Thread(autoSave);
                    this.autoSaveThread.Start();
                }
            }
            else
            {
                if (this.keyboardHook != null)
                {
                    this.keyboardHook.UninstallHook();
                    this.keyboardHook = null;
                    this.autoSaveThread.Interrupt();
                    this.autoSaveThread = null;
                }
            }
        }


         private void OpenSwitch_CheckedChanged(object sender, EventArgs e)
        {
            this.setSaveBtnEnable(true);
        }

        private void setSaveBtnEnable(bool enable)
        {
            this.indexForm.keyboardWatchSaveBtn.Enabled = enable;
        }
    }
}