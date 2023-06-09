using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WelLib.hook;
using WelLib.util;
using WinEventListener.service.impl;
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
        private static string CONFIG_KEYPRESS_HIS_UPLOAD_URL = "keypressHisUploadUrl";
        private static string CONFIG_MOUSEPRESS_HIS_UPLOAD_URL = "mouseHisUploadUrl";

        private static string KEY_PRESS_COUNT_MAP_PATH = FileUtil.getPjRootDir() + "\\keyboard-press-count.json";

        private IndexForm indexForm;
        private PjConfig.Config config = PjConfig.getConfig(TAB_CONFIG_NAME);
        private PjConfig.Config apiCenterConfig = PjConfig.getConfig(ApiCenterCtlTabInit.TAB_CONFIG_NAME);
        private Dictionary<string, object> keyPressCountMap;
        private List<MousePressUploadData> mousePressUploadDataTmp;

        private KeyboardMouseHook keyboardHook;
        private MouseHook mouseHook;
        private int currentKeyPressCount = 0;
        private Thread autoSaveThread;
        private string keypressCountUploadUrl;
        private string mousePressCountUploadUrl;

        private string deviceName = "hwpc";

        public KeyboardWatchTabInit(IndexForm indexForm)
        {
            this.indexForm = indexForm;
        }

        public void init()
        {
            this.initOpen();
            this.initSaveBtn();
            this.initConfig();
            this.initMap();
            this.checkWatch(this.indexForm.keyboardWatchSwitch.Checked);
            this.mousePressUploadDataTmp = new List<MousePressUploadData>();

            this.deviceName = apiCenterConfig.get(ApiCenterCtlService.CONFIG_KEY_SELF_DEVICE_NAME, "hwpc").ToString();
        }
        
        private void initMap()
        {
            this.keyPressCountMap = FileUtil.loadMap(KEY_PRESS_COUNT_MAP_PATH);
        }
        private void initConfig()
        {
            this.keypressCountUploadUrl = this.config.get(CONFIG_KEYPRESS_HIS_UPLOAD_URL, "http://localhost:8888/keyboardPressLog").ToString();
            this.mousePressCountUploadUrl = this.config.get(CONFIG_MOUSEPRESS_HIS_UPLOAD_URL, "http://localhost:8888/mousePressLog").ToString();
            this.indexForm.keyboardWatchUploadUrlLabel.Text = keypressCountUploadUrl;
            this.indexForm.mouseWatchUploadUrlLabel.Text = mousePressCountUploadUrl;
            this.indexForm.keyboardWatchUploadUrlLabel.TextChanged += Item_Changed;
            this.indexForm.mouseWatchUploadUrlLabel.TextChanged += Item_Changed;
        }

        private void autoSave()
        {
            while (true)
            {
                try
                {
                    if (this.currentKeyPressCount > 200)
                    {
                        this.currentKeyPressCount = 0;
                        FileUtil.saveObjectJson(this.keyPressCountMap, KEY_PRESS_COUNT_MAP_PATH);
                        // 上传记录
                        KeypressHisUploadData data = new KeypressHisUploadData();
                        data.deviceName = this.deviceName;
                        data.his = ObjectUtil.ObjectToJson(this.keyPressCountMap);
                        Request.post(this.keypressCountUploadUrl, data);

                        // 上传鼠标点击记录
                        if (this.mousePressUploadDataTmp.Count > 0)
                        {
                            Request.post(this.mousePressCountUploadUrl, this.mousePressUploadDataTmp);
                            this.mousePressUploadDataTmp.Clear();
                        }
                    }
                    try
                    {
                        Thread.Sleep(1000 * 60 * 5);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(1000 * 60 * 1);
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
            this.indexForm.keyboardWatchSwitch.CheckedChanged += Item_Changed;
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
            this.keypressCountUploadUrl = this.indexForm.keyboardWatchUploadUrlLabel.Text;
            this.mousePressCountUploadUrl = this.indexForm.mouseWatchUploadUrlLabel.Text;
            // 批量保存配置
            Dictionary<string, object> newConfig = new Dictionary<string, object>
            {
                { CONFIG_KEY_OPEN, isOpen},
                { CONFIG_KEYPRESS_HIS_UPLOAD_URL, keypressCountUploadUrl },
                { CONFIG_MOUSEPRESS_HIS_UPLOAD_URL, mousePressCountUploadUrl }
            };
            this.config.set(newConfig);
            this.setSaveBtnEnable(false);
            // 检查监听状态
            this.checkWatch(isOpen);
        }
        private void OnKeyPress(KeyboardMouseHook.HookStruct hookStruct, out bool handle)
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
                    this.keyboardHook = new KeyboardMouseHook();
                    this.keyboardHook.InstallHook(this.OnKeyPress);
                    this.autoSaveThread = new Thread(autoSave);
                    this.autoSaveThread.Start();

                    mouseHook = new MouseHook();
                    mouseHook.Subscribe(mouseKeyPressHandle);
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

                    mouseHook.Unsubscribe();
                    mouseHook = null;
                }
            }
        }

        private void mouseKeyPressHandle(MouseButtons button, int x, int y)
        {
            //Console.WriteLine("MouseDown: \t{0}; \t X: \t{1} Y: \t{2}", button, x, y);
            string key = null;
            if (button == MouseButtons.Left)
            {
                key = "l";
            }
            else if (button == MouseButtons.Right)
            {
                key = "r";
            }
            else if (button == MouseButtons.Middle)
            {
                key = "m";
            }
            if (key != null && !this.keyPressCountMap.ContainsKey(key))
            {
                this.keyPressCountMap.Add(key, 0);
            }
            this.keyPressCountMap[key] = Convert.ToDouble(this.keyPressCountMap[key]) + 0.5;
            this.currentKeyPressCount ++;

            this.mousePressUploadDataTmp.Add(new MousePressUploadData() { key = key,x = x,y = y, deviceName = this.deviceName });
        }


         private void Item_Changed(object sender, EventArgs e)
        {
            this.setSaveBtnEnable(true);
        }

        private void setSaveBtnEnable(bool enable)
        {
            this.indexForm.keyboardWatchSaveBtn.Enabled = enable;
        }
    }
    
    internal struct KeypressHisUploadData
    {
        public string deviceName;
        public string his;
    }

    internal struct MousePressUploadData
    {
        public string deviceName;
        public string key;
        public int x;
        public int y;
    }
}
