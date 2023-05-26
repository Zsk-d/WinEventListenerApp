using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using WelLib.util;
using WinEventListener.util;
using static WelLib.util.SystemUtil;

namespace WinEventListener.service.impl
{
    internal class ApiCenterCtlService : WelService
    {

        public static string CONFIG_KEY_SELF_DEVICE_NAME = "selfDeviceName";
        public static string CONFIG_KEY_NOTI_DEVICE_NAME = "notiDeviceName";

        public static string CONFIG_KEY_SYSTEM_LOCK_EVENT_SWITCH = "systemLockEventLs";
        public static string CONFIG_KEY_SYSTEM_UNLOCK_EVENT_SWITCH = "systemUnlockEventLs";
        public static string CONFIG_KEY_LOCK_CTL_SWITCH = "lockCtl";

        private bool lockSwitch;
        private bool unlockSwitch;

        [DllImport("user32")]
        public static extern bool LockWorkStation();

        private string thisDeviceCode;
        private string notifyDeviceCode;
        private ServiceStatus serviceStatus = ServiceStatus.STOP;

        private bool lockCtlSwitch = false;

        private Thread runningThread;

        private void startRecv()
        {
            this.serviceStatus = ServiceStatus.RUNNING;
            ApiCenterUtil.sendSpkMsg("开始监听公司电脑", notifyDeviceCode);
            sendSelfStatus("screenLock", true);
            
            try
            {
                this.recvCtlMsg();
            }
            catch (Exception e) { }
            finally
            {
                this.serviceStatus = ServiceStatus.STOP;
                ApiCenterUtil.sendSpkMsg("公司电脑监听结束", notifyDeviceCode);
                this.runningThread = null;
            }
        }

        private void sendSelfStatus(string statusName, object statusValue)
        {
            ApiCenterUtil.sendStatusMsg(thisDeviceCode, statusName, statusValue);
        }

        private void onSessionEvent(SessionSwitchReason reason)
        {
            if (this.unlockSwitch && reason == SessionSwitchReason.SessionUnlock)
            {
                ApiCenterUtil.sendSpkMsg("公司电脑解锁", notifyDeviceCode);
                sendSelfStatus("screenLock", false);
            }
            else if (this.lockSwitch && reason == SessionSwitchReason.SessionLock)
            {
                sendSelfStatus("screenLock", true);
            }
        }

        public string getName()
        {
            return "ApiCenter远程控制监听服务";
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool run(ServiceParams args)
        {
            this.thisDeviceCode = args.get<string>(CONFIG_KEY_SELF_DEVICE_NAME, "hwpc");
            this.notifyDeviceCode = args.get<string>(CONFIG_KEY_NOTI_DEVICE_NAME, "op8p");
            lockSwitch = args.get<bool>(CONFIG_KEY_SYSTEM_LOCK_EVENT_SWITCH, false);
            unlockSwitch = args.get<bool>(CONFIG_KEY_SYSTEM_UNLOCK_EVENT_SWITCH, false);
            this.lockCtlSwitch = args.get<bool>(CONFIG_KEY_LOCK_CTL_SWITCH, false);
            if (lockSwitch || unlockSwitch)
            {
                MySessionSwitchEventHandler mySessionSwitchEventHandler = new MySessionSwitchEventHandler(lockSwitch, unlockSwitch, new MySessionSwitchEventHandler.SessionEvent(onSessionEvent));
                SessionSwitchEvent.start(mySessionSwitchEventHandler);
            }
            // apicenter 监听
            if (this.runningThread == null)
            {
                // reload时 thread不为null
                this.runningThread = new Thread(startRecv);
                this.runningThread.Start();
            }
            return true;
        }

        public bool stop()
        {
            if (this.serviceStatus == ServiceStatus.RUNNING)
            {
                this.tryStopSessionSwitch();
                ApiCenterUtil.sendCtlMsg("exit", thisDeviceCode);
            }
            return true;
        }

        public ServiceStatus getStatus()
        {
            return this.serviceStatus;
        }

        public bool reload(ServiceParams args)
        {
            this.tryStopSessionSwitch();
            this.run(args);
            return true;
        }

        private void tryStopSessionSwitch()
        {
            if (this.lockSwitch || this.unlockSwitch)
            {
                SessionSwitchEvent.stop();
            }
        }

        private void recvCtlMsg()
        {
            this.sendSelfStatus("ctlOn", true);
            while (true)
            {
                try
                {
                    string res = ApiCenterUtil.getCtlMsg(thisDeviceCode);
                    if (res != null && !"".Equals(res))
                    {
                        if ("exit".Equals(res))
                        {
                            break;
                        }
                        string[] ctls = res.Split('_');
                        if ("do".Equals(ctls[0]))
                        {
                            // pc控制命令, 等待
                            if (this.lockCtlSwitch && "lock".Equals(ctls[1]))
                            {
                                LockWorkStation();
                            }
                            //else if ("wifiadb".Equals(ctls[1]))
                            //{
                            //    // 开始连接调试
                            //    string ip = ctls[2];
                            //    autoConnectAndroidWifiAdb(ip);
                            //}
                        }
                    }
                    else
                    {
                        // 无信息
                        //Thread.Sleep(60);
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(60);
                }
            }
            this.sendSelfStatus("ctlOn", false);
        }

        public bool isRunning()
        {
            return this.getStatus() == ServiceStatus.RUNNING;
        }
    }

    /// <summary>
    /// 系统session事件处理器
    /// </summary>
    internal class MySessionSwitchEventHandler : WelEventHandler
    {
        private bool lockSwitch;
        private bool unlockSwitch;

        public delegate void SessionEvent(SessionSwitchReason reason);

        private SessionEvent sessionEvent;
        public MySessionSwitchEventHandler(bool lockSwitch, bool unlockSwitch, SessionEvent sessionEvent)
        {
            this.lockSwitch = lockSwitch;
            this.unlockSwitch = unlockSwitch;
            this.sessionEvent = sessionEvent;
        }
        public void onEvent(object sender, EventArgs eventArgs)
        {
            var reason = ((SessionSwitchEventArgs)eventArgs).Reason;
            this.sessionEvent(reason);
        }
    }

}
