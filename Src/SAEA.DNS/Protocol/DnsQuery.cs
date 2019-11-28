﻿/****************************************************************************
*项目名称：SAEA.DNS
*CLR 版本：3.0
*机器名称：WENLI-PC
*命名空间：SAEA.DNS.Protocol
*类 名 称：DnsQuery
*版 本 号： v5.0.0.1
*创建人： yswenli
*电子邮箱：wenguoli_520@qq.com
*创建时间：2019/11/28 22:43:28
*描述：
*=====================================================================
*修改时间：2019/11/28 22:43:28
*修 改 人： yswenli
*版 本 号： v5.0.0.1
*描    述：
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace SAEA.DNS.Protocol
{
    /// <summary>
    /// 
    /// https://www.nslookuptool.com/chs/
    /// </summary>
    public class DnsQuery
    {
        List<byte> _data = new List<byte>();

        public byte[] Name { get; set; }

        public QueryType Type { get; set; }

        public ushort Class { get; set; } = 1;

        public DnsQuery(string url, QueryType type)
        {

        }

        public byte[] ToBytes()
        {
            return _data.ToArray();
        }
    }
}
