using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public static class LEU_filtered_valuesExtend
    {
        private static string convertString(StringData section, string title)
        {
            if (null != section)
            {
                return title + " " + section.Value + "\r\n";
            }
            return "";
        }
        public static string getReportString(this LEU_filtered_values.leu.BEACON.MESSAGE.COMBINED_SECTIONS instance)
        {
            return convertString(instance.Upstream_section, "Upstream_section")
                + convertString(instance.Reopening_section, "Reopening_section")
                + convertString(instance.Approach_section, "Approach_section")
                + convertString(instance.Overlap_section, "Overlap_section");
        }
        public static string getReportMsgString(this LEU_filtered_values.leu.BEACON.MESSAGE instance)
        {
            if (null != instance.Interoperable)
            {
                string buff = instance.Interoperable;
                buff = buff.Substring(0, buff.Length -3).ToUpper();
                string tail = " FF";
                while (buff.EndsWith(tail))
                {
                    buff = buff.Substring(0, buff.Length - 3);
                }
                return buff;
            }
            return string.Empty;
        }
    }
    
}

