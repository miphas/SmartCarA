using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Smart_Car
{
    class XML
    {
        public int[] data = new int[8];
        public void write()
        {
            XmlDocument xmldoc;
            XmlElement xmlelem;

            /*创建dom对象*/
            xmldoc = new XmlDocument();

            /*加入XML的声明段落,<?xml version="1.0" encoding="utf-8"?>*/
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(xmldecl);

            /*加入根节点data*/
            xmlelem = xmldoc.CreateElement("", "data", "");
            xmldoc.AppendChild(xmlelem);

            /*创建根节点下四个子节点：urg、con、dr、cam*/
            XmlNode root = xmldoc.SelectSingleNode("data");//查找<data> ,并把它作为根节点。
            XmlElement xe_urg = xmldoc.CreateElement("urg");//创建一个<urg>节点
            XmlElement xe_con = xmldoc.CreateElement("con");//创建一个<con>节点
            XmlElement xe_dr = xmldoc.CreateElement("dr");//创建一个<dr>节点
            XmlElement xe_cam = xmldoc.CreateElement("cam");//创建一个<dr>节点

            /*创建urg子节点，并在子节点中添加数据*/
            XmlElement xe_urg_serial = xmldoc.CreateElement("urg_serial");
            XmlElement xe_urg_baud = xmldoc.CreateElement("urg_baud");
            xe_urg_serial.InnerText = Convert.ToString(data[0]);
            xe_urg_baud.InnerText = Convert.ToString(data[1]);
            xe_urg.AppendChild(xe_urg_serial);
            xe_urg.AppendChild(xe_urg_baud);
            root.AppendChild(xe_urg);

            /*创建con子节点，并在子节点中添加数据*/
            XmlElement xe_con_serial = xmldoc.CreateElement("con_serial");
            XmlElement xe_con_baud = xmldoc.CreateElement("con_baud");
            xe_con_serial.InnerText = Convert.ToString(data[2]);
            xe_con_baud.InnerText = Convert.ToString(data[3]);
            xe_con.AppendChild(xe_con_serial);
            xe_con.AppendChild(xe_con_baud);
            root.AppendChild(xe_con);

            /*创建dr子节点，并在子节点中添加数据*/
            XmlElement xe_dr_serial = xmldoc.CreateElement("dr_serial");
            XmlElement xe_dr_baud = xmldoc.CreateElement("dr_baud");
            xe_dr_serial.InnerText = Convert.ToString(data[4]);
            xe_dr_baud.InnerText = Convert.ToString(data[5]);
            xe_dr.AppendChild(xe_dr_serial);
            xe_dr.AppendChild(xe_dr_baud);
            root.AppendChild(xe_dr);

            /*创建cam子节点，并在子节点中添加数据*/
            XmlElement xe_cam_serial = xmldoc.CreateElement("cam_serial");
            XmlElement xe_cam_baud = xmldoc.CreateElement("cam_baud");
            xe_cam_serial.InnerText = Convert.ToString(data[6]);
            xe_cam_baud.InnerText = Convert.ToString(data[7]);
            xe_cam.AppendChild(xe_cam_serial);
            xe_cam.AppendChild(xe_cam_baud);
            root.AppendChild(xe_cam);

            /*保存文件*/
            xmldoc.Save(@"../../Config/Serial_Config.xml");
        }
        public void read()
        {
  //          int[] data = new int[8];
            /*创建xml读取流对象，并加载文件*/
            XmlDocument doc = new XmlDocument();
            doc.Load(@"../../Config/Serial_Config.xml");

            /*获取根节点*/
            XmlNode root = doc.SelectSingleNode("data");
            XmlNodeList xnl_1 = root.ChildNodes;//xnl_i节点为urg,con,dr,cam

            foreach (XmlNode xn1_1 in xnl_1)
            {
                XmlElement xe_1 = (XmlElement)xn1_1;
                if (xe_1.Name == "urg")//检测到urg节点
                {
                    XmlNodeList xnl_2 = xe_1.ChildNodes;//xnl_2节点为urg_serial和urg_baud
                    data[0] = Convert.ToInt32(xnl_2.Item(0).InnerText);
                    data[1] = Convert.ToInt32(xnl_2.Item(1).InnerText);
                }
                else if (xe_1.Name == "con")
                {
                    XmlNodeList xnl_2 = xe_1.ChildNodes;//xnl_2节点为con_serial和con_baud
                    data[2] = Convert.ToInt32(xnl_2.Item(0).InnerText);
                    data[3] = Convert.ToInt32(xnl_2.Item(1).InnerText);
                }
                else if (xe_1.Name == "dr")
                {
                    XmlNodeList xnl_2 = xe_1.ChildNodes;//xnl_2节点为dr_serial和dr_baud
                    data[4] = Convert.ToInt32(xnl_2.Item(0).InnerText);
                    data[5] = Convert.ToInt32(xnl_2.Item(1).InnerText);
                }
                else if (xe_1.Name == "cam")
                {
                    XmlNodeList xnl_2 = xe_1.ChildNodes;//xnl_2节点为dr_serial和dr_baud
                    data[6] = Convert.ToInt32(xnl_2.Item(0).InnerText);
                    data[7] = Convert.ToInt32(xnl_2.Item(1).InnerText);
                }
            }
        }
    }
}
