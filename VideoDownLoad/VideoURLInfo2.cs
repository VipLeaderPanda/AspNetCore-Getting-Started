using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoDownLoad
{
    public class VideoURLInfo2
    {
        public int errno
        {
            get; set;
        }
        public string error
        {
            get; set;
        }
        public dataInfo data
        {
            get;set;
        }
        public class dataInfo
        {
            public VideoInfo response
            {
                get; set;
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
                            get; set;
                        }
                        public string title
                        {
                            get; set;
                        }
                        public string duration
                        {
                            get;set;
                        }
                        public string publish_time
                        {
                            get; set;
                        }
                        public string video_src
                        {
                            get; set;
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
        }
    }
}
