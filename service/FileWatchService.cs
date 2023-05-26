using System;
using System.Collections.Generic;
using System.IO;
using WinEventListener.util;

namespace WinEventListener.service.impl
{

    internal class FileWatchService :WelService
    {
        private ServiceStatus status = ServiceStatus.STOP;
        private FileWatcher fileWatcher;

        public bool isRunning()
        {
            return this.getStatus() == ServiceStatus.RUNNING;
        }

        public bool reload(ServiceParams args)
        {
            throw new NotImplementedException();
        }

        public void run(string dirPath)
        {
            if (this.status == ServiceStatus.RUNNING)
            {
                throw new Exception("服务已在运行");
            }
            // 开启文件监听
            this.fileWatcher = new FileWatcher();
            this.fileWatcher.watch(dirPath, dirPath, new MyFileWatcherEvent());
            this.status = ServiceStatus.RUNNING;
        }

        public bool run(ServiceParams args)
        {
            throw new NotImplementedException();
        }

        string WelService.getName()
        {
            return "文件监听服务";
        }

        public ServiceStatus getStatus()
        {
            return this.status;
        }

        bool WelService.run(ServiceParams args)
        {
            throw new System.NotImplementedException();
        }

        bool WelService.stop()
        {
            if (this.status == ServiceStatus.RUNNING)
            {
                this.fileWatcher.stop();
            }
            return true;
        }
    }

    public class MyFileWatcherEvent : CustomerFileWatchEvent
    {
        public void onChanged(WatcherChangeTypes changeTypes, string fullPath, string name)
        {
        }

        public void onCreated(WatcherChangeTypes changeTypes, string fullPath, string name)
        {
        }

        public void onDeleted(WatcherChangeTypes changeTypes, string fullPath, string name)
        {
        }
    }
}
