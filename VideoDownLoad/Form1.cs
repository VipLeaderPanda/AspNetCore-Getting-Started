using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace VideoDownLoad
{
    public partial class Form1 : Form
    {
        #region 全局成员

        //存放下载列表
        List<SynFileInfo> m_SynFileInfoList;
        Dictionary<string,Head> heads;
        List<WebClient> wbs;
        private string author_id = string.Empty;
        private string author_image = string.Empty;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        #region 初始化GridView

        void InitDataGridView(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;//是否自动创建列
            dgv.AllowUserToAddRows = false;//是否允许添加行(默认：true)
            dgv.AllowUserToDeleteRows = false;//是否允许删除行(默认：true)
            dgv.AllowUserToResizeColumns = false;//是否允许调整大小(默认：true)
            dgv.AllowUserToResizeRows = false;//是否允许调整行大小(默认：true)
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//列宽模式(当前填充)(默认：DataGridViewAutoSizeColumnsMode.None)
            dgv.BackgroundColor = System.Drawing.Color.White;//背景色(默认：ControlDark)
            dgv.BorderStyle = BorderStyle.Fixed3D;//边框样式(默认：BorderStyle.FixedSingle)
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;//单元格边框样式(默认：DataGridViewCellBorderStyle.Single)
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;//列表头样式(默认：DataGridViewHeaderBorderStyle.Single)
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;//是否允许调整列大小(默认：DataGridViewColumnHeadersHeightSizeMode.EnableResizing)
            dgv.ColumnHeadersHeight = 30;//列表头高度(默认：20)
            dgv.MultiSelect = false;//是否支持多选(默认：true)
            dgv.ReadOnly = true;//是否只读(默认：false)
            dgv.RowHeadersVisible = false;//行头是否显示(默认：true)
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;//选择模式(默认：DataGridViewSelectionMode.CellSelect)
        }

        #endregion
        #region 添加GridView列

        /// <summary>
        /// 正在同步列表
        /// </summary>
        void AddGridViewColumns(DataGridView dgv)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DocID",
                HeaderText = "文件ID",
                Visible = false,
                Name = "DocID"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                DataPropertyName = "DocName",
                HeaderText = "文件名",
                Name = "DocName",
                Width = 300
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "FileSize",
                HeaderText = "大小",
                Name = "FileSize",
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SynSpeed",
                HeaderText = "速度",
                Name = "SynSpeed"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SynProgress",
                HeaderText = "进度",
                Name = "SynProgress"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "DownPath",
                HeaderText = "下载地址",
                Visible = false,
                Name = "DownPath"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "SavePath",
                HeaderText = "保存地址",
                Visible = false,
                Name = "SavePath"
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Async",
                HeaderText = "是否异步",
                Visible = false,
                Name = "Async"
            });
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化DataGridView相关属性
            InitDataGridView(dgvDownLoad);
            //添加DataGridView相关列信息
            AddGridViewColumns(dgvDownLoad);
            m_SynFileInfoList = new List<SynFileInfo>();
            heads = new Dictionary<string, Head>();
            wbs = new List<WebClient>();
            for(int i=1;i<60;i++)
            {
                cb_minute.Items.Add(i);
            }
            cb_minute.SelectedIndex=4;
        }

        private void btn_get_Click(object sender, EventArgs e)
        {
            //清空行数据
            dgvDownLoad.Rows.Clear();
            heads.Clear();
            try
            {
                string url = txt_url.Text;
                Encoding encoding = Encoding.UTF8;
                using (WebClient wc = new WebClient())
                {
                    using (Stream resStream = wc.OpenRead(url))
                    {
                        using (StreamReader sr = new StreamReader(resStream, encoding))
                        {
                            string result_text = sr.ReadToEnd();
                            result_text = result_text.Substring(result_text.IndexOf("author_info") - 2);
                            result_text = result_text.Substring(0, result_text.IndexOf("logid") + 20);

                            VideoURLInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoURLInfo>(result_text);
                            //添加列表(建立多个任务)
                            List<ArrayList> arrayListList = new List<ArrayList>();
                            author_image = info.author_info.avatar;
                            foreach (var item in info.video.results)
                            {
                                string path = System.IO.Directory.GetCurrentDirectory() + @"\downloads\" + info.author_info.id;

                                // 如果不存在就创建file文件夹
                                if (!Directory.Exists(path))
                                {
                                    if (path != null) Directory.CreateDirectory(path);
                                }
                                int pl_set = (int)cb_minute.SelectedItem;
                                int pl_get = int.Parse(item.content.duration.Split(':')[0]);
                                if(pl_set < pl_get)
                                {
                                    continue;
                                }
                                string video_src = item.content.video_src;
                                if (video_src.Contains("?"))
                                {
                                    video_src = video_src.Substring(0, video_src.IndexOf('?'));
                                }
                                string savefile = string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), info.author_info.id, Path.GetFileName(video_src));
                                if (item.content.displaytype_exinfo != null)
                                {
                                    arrayListList.Add(new ArrayList(){
                                    item.content.pvid,//文件id
                                    Path.GetFileName(item.content.video_src),//文件名称
                                    FileOperate.GetAutoSizeString(item.content.displaytype_exinfo.video_info.fileSizeInByte,2),//文件大小
                                    "0 KB/S",//下载速度
                                    "0%",//下载进度
                                    item.content.video_src,//远程服务器下载地址
                                    savefile,//本地保存地址
                                    true//是否异步
                                 });
                                }
                                else
                                {
                                   
                                    arrayListList.Add(new ArrayList(){
                                    item.content.pvid,//文件id
                                    Path.GetFileName(video_src),//文件名称
                                    "未知",//文件大小
                                    "0 KB/S",//下载速度
                                    "0%",//下载进度
                                    item.content.video_src,//远程服务器下载地址
                                   savefile,//本地保存地址
                                    true//是否异步
                                 });
                                }
                                heads.Add(item.content.pvid, new Head() { vid = item.content.vid , author = item.content.author, publish_time = item.content.publish_time, title = item.content.title,fileName= Path.GetFileName(video_src) });
                            }
                            foreach (ArrayList arrayList in arrayListList)
                            {
                                int rowIndex = dgvDownLoad.Rows.Add(arrayList.ToArray());
                                arrayList[2] = 0;
                                arrayList.Add(dgvDownLoad.Rows[rowIndex]);
                                //取出列表中的行信息保存列表集合(m_SynFileInfoList)中
                                m_SynFileInfoList.Add(new SynFileInfo(arrayList.ToArray()));
                            }
                            author_id = info.author_info.id;
                            string url2 = string.Format("https://haokan.baidu.com/author/{0}?_format=json&rn=16&ctime={1}&_api=1", info.author_info.id, info.video.ctime);
                            getVideoInfo2(url2, info.author_info.id);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void getVideoInfo2(string url,string author_id)
        {
           
            Encoding encoding = Encoding.UTF8;
            using (WebClient wc = new WebClient())
            {
                using (Stream resStream = wc.OpenRead(url))
                {
                    using (StreamReader sr = new StreamReader(resStream, encoding))
                    {
                        string result_text = sr.ReadToEnd();
                       

                        VideoURLInfo2 info = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoURLInfo2>(result_text);
                        if(info.data.response.results.Count==0)
                        {
                            return;
                        }
                        //添加列表(建立多个任务)
                        List<ArrayList> arrayListList = new List<ArrayList>();

                        foreach (var item in info.data.response.results)
                        {
                            int pl_set = (int)cb_minute.SelectedItem;
                            int pl_get = int.Parse(item.content.duration.Split(':')[0]);
                            if (pl_set < pl_get)
                            {
                                continue;
                            }
                            string video_src = item.content.video_src;
                            if (video_src.Contains("?"))
                            {
                                video_src = video_src.Substring(0, video_src.IndexOf('?'));
                            }
                            string savefile =string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory() ,author_id, Path.GetFileName(video_src));
                            if (item.content.displaytype_exinfo != null)
                            {
                                arrayListList.Add(new ArrayList(){
                                    item.content.pvid,//文件id
                                    Path.GetFileName(item.content.video_src),//文件名称
                                    FileOperate.GetAutoSizeString(item.content.displaytype_exinfo.video_info.fileSizeInByte,2),//文件大小
                                    "0 KB/S",//下载速度
                                    "0%",//下载进度
                                    item.content.video_src,//远程服务器下载地址
                                    savefile,//本地保存地址
                                    true//是否异步
                                 });
                            }
                            else
                            {
                               
                                arrayListList.Add(new ArrayList(){
                                    item.content.pvid,//文件id
                                    Path.GetFileName(video_src),//文件名称
                                    "未知",//文件大小
                                    "0 KB/S",//下载速度
                                    "0%",//下载进度
                                    item.content.video_src,//远程服务器下载地址
                                    savefile,//本地保存地址
                                    true//是否异步
                                 });
                            }
                            heads.Add(item.content.pvid, new Head() {vid = item.content.vid, author = item.content.author, publish_time = item.content.publish_time, title = item.content.title, fileName= Path.GetFileName(video_src) });
                        }
                        foreach (ArrayList arrayList in arrayListList)
                        {
                            int rowIndex = dgvDownLoad.Rows.Add(arrayList.ToArray());
                            arrayList[2] = 0;
                            arrayList.Add(dgvDownLoad.Rows[rowIndex]);
                            //取出列表中的行信息保存列表集合(m_SynFileInfoList)中
                            m_SynFileInfoList.Add(new SynFileInfo(arrayList.ToArray()));
                        }
                        string url2 = string.Format("https://haokan.baidu.com/author/{0}?_format=json&rn=16&ctime={1}&_api=1", author_id, info.data.response.ctime);
                        getVideoInfo2(url2,author_id);
                    }
                }
            }
        }

        #region 检查网络状态

        //检测网络状态
        [DllImport("wininet.dll")]
        extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测网络状态
        /// </summary>
        bool isConnected()
        {
            int I = 0;
            bool state = InternetGetConnectedState(out I, 0);
            return state;
        }

        #endregion

        private void btn_download_Click(object sender, EventArgs e)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + @"\downloads";

            // 如果不存在就创建file文件夹
            if (!Directory.Exists(path))
            {
                if (path != null) Directory.CreateDirectory(path);
            }
            string headfile = string.Format(@"{0}\\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.txt");
            if(File.Exists(headfile))
            {
                File.Delete(headfile);
            }
            //判断网络连接是否正常
            if (isConnected())
            {
                //设置不可用
                btn_download.Enabled = false;
                //设置最大活动线程数以及可等待线程数
                ThreadPool.SetMaxThreads(3, 3);
                //判断是否还存在任务
                if (m_SynFileInfoList.Count <= 0) return;
                using (WebClient client = new WebClient())
                {
                    string savefile = string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.jpg");

                    client.DownloadFile(new Uri(author_image), savefile);
                }
                wbs.Clear();
                foreach (SynFileInfo m_SynFileInfo in m_SynFileInfoList)
                {
                    
                   
                    //启动下载任务
                    StartDownLoad(m_SynFileInfo);
                }
            }
            else
            {
                MessageBox.Show("网络异常!");
            }
        }

        #region 使用WebClient下载文件

        /// <summary>
        /// HTTP下载远程文件并保存本地的函数
        /// </summary>
        void StartDownLoad(object o)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)o;
            m_SynFileInfo.LastTime = DateTime.Now;
            //再次new 避免WebClient不能I/O并发 
            WebClient client = new WebClient();
            wbs.Add(client);
            if (m_SynFileInfo.Async)
            {
                //异步下载
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                
                client.DownloadFileAsync(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath, m_SynFileInfo);
                
            }
            else client.DownloadFile(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath);
        }

        /// <summary>
        /// 下载进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfo.SynProgress = e.ProgressPercentage + "%";
            double secondCount = (DateTime.Now - m_SynFileInfo.LastTime).TotalSeconds;
            m_SynFileInfo.SynSpeed = FileOperate.GetAutoSizeString(Convert.ToDouble(e.BytesReceived / secondCount), 2) + "/s";
            //更新DataGridView中相应数据显示下载进度
            m_SynFileInfo.RowObject.Cells["SynProgress"].Value = m_SynFileInfo.SynProgress;
            //更新DataGridView中相应数据显示下载速度(总进度的平均速度)
            m_SynFileInfo.RowObject.Cells["SynSpeed"].Value = m_SynFileInfo.SynSpeed;
        }

        /// <summary>
        /// 下载完成调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //到此则一个文件下载完毕
           
                SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
                m_SynFileInfoList.Remove(m_SynFileInfo);
            if (!e.Cancelled)
            {
                string content = string.Empty;
                string savefile = string.Format(@"{0}\\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.txt");
                content = JsonConvert.SerializeObject(heads[m_SynFileInfo.DocID]);
                using (FileStream fs = new FileStream(savefile, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                        sw.WriteLine(content);
                }
            }
            if (m_SynFileInfoList.Count <= 0)
            {
                //此时所有文件下载完毕
                btn_download.Enabled = true;

               


               

            }
        }

        #endregion

      
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            foreach(var item in wbs)
            {
                item.CancelAsync();
            }
        }
    }
}
