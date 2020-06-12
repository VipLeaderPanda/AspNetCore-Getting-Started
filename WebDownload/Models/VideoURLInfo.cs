using System.Collections.Generic;

namespace WebDownload.Models
{
    public class VideoURLInfo
    {
        public author_infoClass author_info
        {
            get;set;
        }
        public int login_status
        {
            get; set;
        }
        public string logid
        {
            get; set;
        }
        public VideoInfo video
        {
            get;set;
        }
        public class VideoInfo
        {
            public int response_count
            {
                get; set;
            }
            public string ctime
            {
                get; set;
            }
            public List<resultsInfo> results
            {
                get; set;
            }
            public class resultsInfo
            {
                public string tplName
                {
                    get; set;
                }
                public contentInfo content
                {
                    get; set;
                }
                public class contentInfo
                {
                    public string vid
                    {
                        get; set;
                    }
                    public string pvid
                    {
                        get; set;
                    }
                    public string author
                    {
                        get;set;
                    }
                    public string title
                    {
                        get;set;
                    }
                    public string publish_time
                    {
                        get;set;
                    }
                    public string video_src
                    {
                        get; set;
                    }
                    public string duration
                    {
                        get;set;
                    }
                    public displaytype_exinfoClass displaytype_exinfo
                    {
                        get; set;
                    }
                    public class displaytype_exinfoClass
                    {
                        public int sourceid
                        {
                            get; set;
                        }
                        public video_infoClass video_info
                        {
                            get; set;
                        }
                        public class video_infoClass
                        {
                            public int fileSizeInByte
                            {
                                get; set;
                            }
                        }
                    }
                }
            }
        }
        public class author_infoClass
        {
            public string id
            {
                get;set;
            }
            public string avatar
            {
                get;set;
            }
        }
    }
}
