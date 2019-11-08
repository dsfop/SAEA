﻿using CCWin;
using SAEA.Common;
using SAEA.FTP;
using SAEA.FTPTest.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAEA.FTPTest
{
    public partial class FtpClientForm : Skin_Mac
    {
        FTPClient _client = null;

        public FtpClientForm()
        {
            InitializeComponent();

            textBox1_TextChanged(null, null);
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            splitContainer2.Panel2.Enabled = false;
            try
            {
                var ip = skinWaterTextBox1.Text;
                var port = int.Parse(skinWaterTextBox2.Text);
                var username = skinWaterTextBox3.Text;
                var pwd = skinWaterTextBox4.Text;

                _client = new FTPClient(ip, port, username, pwd);

                Task.Run(() =>
                {
                    try
                    {
                        _client.Connect();

                        Log("连接到FTP成功");

                        splitContainer2.BeginInvoke(new Action(() =>
                        {
                            splitContainer2.Panel2.Enabled = true;
                            textBox2.Text = "/";
                            textBox2_TextChanged(null, null);
                        }));

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("连接到FTP失败，ex:" + ex.Message);
                        Log("连接到FTP失败", ex.Message);

                        this.BeginInvoke(new Action(() =>
                        {
                            groupBox1.Enabled = true;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接到FTP失败，ex:" + ex.Message);
                Log("连接到FTP失败", ex.Message);
                groupBox1.Enabled = true;
            }
        }

        public void Log(string operationName, string msg = "")
        {
            var action = new Action(() =>
            {
                logTxt.Text = $"{DateTime.Now.ToString("HH:mm:ss.fff")}\t{operationName} \t{msg}{Environment.NewLine}{logTxt.Text}";
                logTxt.Select(0, 1);
                logTxt.ScrollToCaret();
            });

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(action));
            }
            else
            {
                action.Invoke();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var filePath = textBox1.Text;

            List<ListInfo> listInfos = new List<ListInfo>();

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    var dirs = PathHelper.GetDirectories(filePath, out List<FileInfo> files);

                    if (dirs != null)
                    {
                        foreach (var item in dirs)
                        {
                            listInfos.Add(new ListInfo()
                            {
                                FileName = item.Name,
                                Type = "文件夹"
                            });
                        }
                    }

                    if (files != null)
                    {
                        foreach (var item in files)
                        {
                            listInfos.Add(new ListInfo()
                            {
                                FileName = item.Name,
                                Type = "文件",
                                Size = item.Length
                            });
                        }
                    }

                    listInfos = listInfos.OrderBy(b => b.FileName).ToList();
                }
                catch (Exception ex)
                {
                    Log("访问本地目录：" + filePath, ex.Message);
                }
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = listInfos;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            var filePath = textBox2.Text;

            Task.Run(() =>
            {
                try
                {
                    var list = _client.Dir(textBox2.Text, FTP.Model.DirType.MLSD);

                    if (list != null && list.Any())
                    {
                        List<ListInfo> listInfos = new List<ListInfo>();

                        foreach (var item in list)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                var arr = item.Split(";", StringSplitOptions.RemoveEmptyEntries);

                                if (arr.Length == 3)
                                {
                                    var type = (arr[0] == "type=dir" ? "文件夹" : "文件");

                                    var size = 0L;

                                    if (type == "文件")
                                    {
                                        size = _client.FileSize(arr[2]);
                                    }

                                    listInfos.Add(new ListInfo()
                                    {
                                        FileName = arr[2],
                                        Type = type,
                                        Size = size
                                    });
                                }
                            }
                        }

                        listInfos = listInfos.OrderBy(b => b.FileName).ToList();

                        dataGridView2.BeginInvoke(new Action(() =>
                        {
                            dataGridView2.DataSource = null;
                            dataGridView2.DataSource = listInfos;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Log("初始化ftpserver列表失败", ex.Message);
                }
            });
        }

        #region context menus
        private void parentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dir = PathHelper.GetPreDir(textBox1.Text);

            if (dir != null)
            {
                textBox1.Text = dir.FullName;
            }

        }

        private void parentToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion


        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString() == "文件夹")
                {
                    var fileName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                    textBox1.Text = Path.Combine(textBox1.Text, fileName);
                }
            }
            else
            {
                var dir = PathHelper.GetPreDir(textBox1.Text);

                if (dir != null)
                {
                    textBox1.Text = dir.FullName;
                }
            }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView2.Rows[e.RowIndex].Cells[2].Value.ToString() == "文件夹")
                {
                    var path = textBox2.Text;

                    var fileName = dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();

                    Task.Run(() =>
                    {
                        try
                        {
                            if (_client.ChangeDir(fileName))
                            {
                                var cp = _client.CurrentDir();

                                textBox2.Invoke(new Action(() =>
                                {
                                    textBox2.Text = cp;
                                }));
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    });

                }
            }
            else
            {
                var dir = PathHelper.GetPreDir(textBox1.Text);

                if (dir != null)
                {
                    textBox1.Text = dir.FullName;
                }
            }
        }
    }
}
