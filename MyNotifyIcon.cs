using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ClipboardDetector
{
    public class MyNotifyIcon
    {
        private const string StartUpKey = "ClipboardDetector";

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenu1;
        private ToolStripMenuItem autoStartUpMenuItem;
        private ToolStripMenuItem reloadMenuItem;
        private ToolStripMenuItem quitMenuItem;
        private System.ComponentModel.IContainer components;
        private List<ToolStripMenuItem> m_MenuItems = new List<ToolStripMenuItem>();

        public Action NotifyIconDoubleClick;
        public Action ReloadDataClick;
        public Action QuitClick;
        public Action<Item> OnItemClick;
        private Item[] m_Items;

        public void Init()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new ContextMenuStrip();

            autoStartUpMenuItem = new ToolStripMenuItem("开机自启动");
            autoStartUpMenuItem.Checked = IsAutoStartUp();
            autoStartUpMenuItem.Click += MenuItem_AutoStartUp_Click;

            reloadMenuItem = new ToolStripMenuItem("重载数据");
            reloadMenuItem.Click += OnReloadClick;

            quitMenuItem = new ToolStripMenuItem("退出");
            quitMenuItem.Click += OnQuitClick;

            ResetItems(null, 0);

            // Create the NotifyIcon.
            this.notifyIcon1 = new NotifyIcon(this.components);
            this.notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            this.notifyIcon1.ContextMenuStrip = contextMenu1;
            notifyIcon1.Text = "hello world";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
        }


        internal void ResetItems(List<Item> items, int selectIdx)
        {
            contextMenu1.Items.Clear();
            m_Items = items?.ToArray();
            m_MenuItems.Clear();
            if (m_Items != null && m_Items.Length > 0)
            {
                selectIdx = Math.Clamp(selectIdx, 0, m_Items.Length - 1);
                for (var i = 0; i < m_Items.Length; i++)
                {
                    var menuItem = CreateMenuItem(m_Items[i], i, i == selectIdx);
                    m_MenuItems.Add(menuItem);
                    contextMenu1.Items.Add(menuItem);
                }
            }

            contextMenu1.Items.Add("-");
            contextMenu1.Items.Add(autoStartUpMenuItem);
            contextMenu1.Items.Add(reloadMenuItem);
            contextMenu1.Items.Add(quitMenuItem);
        }

        private ToolStripMenuItem CreateMenuItem(Item item, int idx, bool isChecked)
        {
            var menuItem = new ToolStripMenuItem();
            menuItem.Text = item.name;
            menuItem.Checked = isChecked;
            menuItem.Click += (object sender, EventArgs e) =>
            {
                for (var i = 0; i < m_MenuItems.Count; i++)
                    m_MenuItems[i].Checked = i == idx;
                OnItemClick?.Invoke(item);
            };
            return menuItem;
        }

        private void OnReloadClick(object sender, EventArgs e)
        {
            ReloadDataClick?.Invoke();
        }

        private void OnQuitClick(object sender, EventArgs e)
        {
            QuitClick?.Invoke();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            NotifyIconDoubleClick?.Invoke();
        }


        private void MenuItem_AutoStartUp_Click(object sender, EventArgs e)
        {
            autoStartUpMenuItem.Checked = !autoStartUpMenuItem.Checked;
            Console.WriteLine(autoStartUpMenuItem.Checked);
            ChangeAutoStartUp(autoStartUpMenuItem.Checked);
        }

        private string GetCurrentPath()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location);
            var filename = Path.Combine(dir, assembly.GetName().Name + ".exe");
            return filename;
        }

        private bool IsAutoStartUp()
        {
            string path = GetCurrentPath();
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            var oldPath = rk2.GetValue(StartUpKey) as String;
            rk2.Close();
            rk.Close();
            return oldPath == path;
        }

        private void ChangeAutoStartUp(bool isSet)
        {
            string path = GetCurrentPath();
            // 注意这里推荐使用 CurrentUser 而不是 LocalMachine
            // LocalMachine 需要注册权限
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (isSet)
                rk2.SetValue(StartUpKey, path);
            else
                rk2.DeleteValue(StartUpKey, false);
            rk2.Close();
            rk.Close();
        }

    }
}
