using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using MetaFly.Summer.Generic;
using MetaFly.Summer.IO;
using BMGenTool.Info;
using NPOI.SS.Util;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;

namespace BMGenTool.Common
{
    public class CIReportExcel
    {
        bool hasTGM = false;
        static int dashlinecolidx = 69;//dashline with no background
        static int input4defaultcolidx = 36;//col of 不匹配以上
        static int[] mergedcolinfo = { 3, 34, 36, 66};//pairs of (start col, end col)

        public static int[] cols = { 0, 1, 2, 3, 35, 36, 67, 68 };
        public static int[] defaultmsgcols = { 0, 1, 2, 3, 67, 68 };

        private Dictionary<string, List<List<string>>> processData = new Dictionary<string, List<List<string>>>();
        public int getsheetnum()
        {
            return processData.Count();
        }

        private IWorkbook workBook;

        private ICellStyle dashstyle;
        private Dictionary<int, ICellStyle> colstyles = new Dictionary<int, ICellStyle>();

        /// <summary>
        /// set value of members defaultstyle backgroundstyle dashstyle
        /// get the style from the input work at certain page and certain line
        /// the certain col is defined as the class static int member
        /// </summary>
        /// <param name="work"></param>
        /// <param name="page"></param>
        /// <param name="line"></param>
        private void getstyles(IWorkbook work,int page, int line)
        {
            if (null == work)
            {
                return;
            }
            
            IRow row = work.GetSheetAt(page).GetRow(line);
            foreach (int col in cols)
            {
                colstyles[col] = row.GetCell(col).CellStyle;
            }
            dashstyle = row.GetCell(dashlinecolidx).CellStyle;
        }

        /// <summary>
        /// set the cell value and style.
        /// now set style of each cell, has not found the method of set col or row style
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="val"></param>
        /// <param name="daseed">if the cell has dash line</param>
        /// <param name="background">if the cell has background</param>
        private void setcellvaluestyle(IRow row, int col, string val, bool daseed = false)
        {
            ICell cell = row.GetCell(col);
            if (null == cell)
            {
                cell = row.CreateCell(col);
            }
            
            if (true == daseed)
            {
                cell.CellStyle = dashstyle;
            }
            else
            {
                if (colstyles.ContainsKey(col))
                {
                    cell.CellStyle = colstyles[col];
                }
                else
                {
                    TraceMethod.RecordInfo($"get no cellstyle of col{col}");
                }
            }
            cell.SetCellValue(val);
        }

        public static IWorkbook createworkbook(FileStream sw)
        {
            if (".xls" == Path.GetExtension(sw.Name))
            {
                return new HSSFWorkbook(sw);
            }
            else if (".xlsx" == Path.GetExtension(sw.Name))
            {
                return new XSSFWorkbook(sw);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// output the processData to excel
        /// excel is genrate accorint to input templatefile
        /// </summary>
        /// <param name="templatefile"></param>
        /// <param name="outputpath"></param>
        public void generateExcel(string templatefile, string outputpath)
        {
            const int templatepage = 3;
            const int formatline = 11;
            const int datastartline = 11;
            const int notestartline = 12;
            const int existdataline = notestartline - datastartline;
            
            string outputFullName = Path.Combine(outputpath, Path.GetFileName(templatefile));
            if (false == File.Exists(templatefile))
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR,
                    $"CI-LEU report templelate file is missing {templatefile}, please check!");
                return;
            }
            using (FileStream sw = new FileStream(templatefile, FileMode.Open, FileAccess.ReadWrite))
            {
                workBook = createworkbook(sw);
                if (null == workBook)
                {
                    return;
                }
                
                getstyles(workBook, templatepage, formatline);

                int sheetidx = 1;
                foreach(var page in processData)
                {
                    ISheet sheet = workBook.CloneSheet(templatepage);
                    workBook.SetSheetName(templatepage + sheetidx, $"{page.Key}联锁区");
                    ++sheetidx;

                    int recordnum = processData[page.Key].Count();
                    sheet.ShiftRows(notestartline, sheet.LastRowNum, recordnum - existdataline, true, true);
                    for (int wor = 0; wor < recordnum; ++wor)
                    {
                        IRow row = sheet.CreateRow(datastartline + wor);
                        if (null == processData[page.Key][wor])
                        {
                            //empty row need do nothing
                        }
                        else if (defaultmsgcols.Count() == processData[page.Key][wor].Count)
                        {
                            setDefaultMsgRow(row, processData[page.Key][wor]);
                        }
                        else if (cols.Count() == processData[page.Key][wor].Count)
                        {
                            setRow(row, processData[page.Key][wor], cols);
                        }
                        else
                        {
                            TraceMethod.Record(TraceMethod.TraceKind.ERROR,
                                $"get invalid data reocrd cols count {processData[page.Key][wor].Count}\n"
                                + $"{processData[page.Key][wor].ToString()}");
                        }
                    }
                    //set MergedRegion, but the existed line should not set again or excel will raise error while open
                    for (int rowidx =0; rowidx< recordnum - existdataline; rowidx++)
                    {
                        for (int mergedidx = 0; mergedidx < mergedcolinfo.Count() / 2; ++mergedidx)
                        {
                            CellRangeAddress region = new CellRangeAddress(
                                datastartline + existdataline + rowidx, datastartline + existdataline + rowidx,
                                mergedcolinfo[mergedidx * 2], mergedcolinfo[mergedidx * 2 + 1]);
                            sheet.AddMergedRegion(region);
                        }
                    }
                }
                workBook.RemoveSheetAt(templatepage);

                FileStream sw1 = new FileStream(outputFullName, FileMode.Create, FileAccess.ReadWrite);
                workBook.Write(sw1);
                sw.Close();
                sw1.Close();
            }
        }
        /// <summary>
        /// 对外构造函数，用于生成报告
        /// </summary>
        /// <param name="templatefile">报告模板文件</param>
        /// <param name="outputdir">报告输出路径</param>
        /// <param name="data">要输出的数据</param>
        public CIReportExcel(string templatefile,string folderpath, List<LEU> leus, List<LEU_filtered_values.leu> data, bool hasbin = false)
        {
            hasTGM = hasbin;
            getdataprocess(leus, data, folderpath);
            if (processData.Count > 0)
            {
                generateExcel(templatefile, folderpath);
            }
        }

        /// <summary>
        /// set data in record to cols of input row
        /// </summary>
        /// <param name="row"></param>
        /// <param name="record"></param>
        /// <param name="cols"></param>
        private void setRow(IRow row, List<string> record, int[] cols)
        {
            int i = 0;
            if (record.Count != cols.Length)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, 
                    $"call setMsgRow error, record count {record.Count} != cols count {cols.Length}");
                return;
            }
            foreach (int col in cols)
            {
                setcellvaluestyle(row, col, record[i]);
                ++i;
            }
        }
        /// <summary>
        /// set data in record to the row of default message type
        /// </summary>
        /// <param name="row"></param>
        /// <param name="record"></param>
        private void setDefaultMsgRow(IRow row, List<string> record)
        {
            setRow(row, record, defaultmsgcols);
            setcellvaluestyle(row, input4defaultcolidx, "不匹配以上所有情况");
            setcellvaluestyle(row, dashlinecolidx, string.Empty, daseed:true);
        }

        /// <summary>
        /// read compiled messages from Tgm file
        /// </summary>
        /// <param name="tgmfilefullname">fullname of the tgm file</param>
        /// <param name="msgnum">nums of message you want to get, [0, 127]</param>
        /// <returns></returns>
        public static List<string> readcompiledmsg(string tgmfilefullname, ushort msgnum)
        {
            List<string> msgs = new List<string>();
            if (false == File.Exists(tgmfilefullname))
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"readcompiledmsg Error. input {tgmfilefullname} is not exist");
                return msgs;
            }
            if ( msgnum > (ushort)127)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"readcompiledmsg Error: input msgnum={msgnum} should in [0,127]");
                return msgs;
            }
            using (StreamReader tgmfile = new StreamReader(tgmfilefullname))
            {
                string buff = tgmfile.ReadToEnd();
                string pattern = @".TGML ([0-9a-fA-F\s\r\n]*)";

                int i = 0;
                foreach (Match match in Regex.Matches(buff, pattern))
                {
                    if (i == msgnum)
                    {//read the first msgnum msgs is enough
                        break;
                    }
                    msgs.Add(match.Groups[1].ToString().Trim().Replace("\n", " ").Replace("\r", ""));
                    ++i;
                }
            }
            if (msgnum != (ushort)msgs.Count)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"readcompiledmsg {tgmfilefullname} Error. Please Check:\r\n" +
                    $"get msg num {msgs.Count} != {msgnum}");
                return msgs;
            }
            return msgs;
        }
        /// <summary>
        /// get values from input data
        /// and set the values in member processData for write to excel
        /// </summary>
        /// <param name="leulist"></param>
        /// <param name="data">data in LEU_Result_Filtered_Values.xml</param>
        /// <param name="msgfiledir">path of LEUBinary which include all TGM files of all LEUs</param>
        /// <returns></returns>
        private void getdataprocess(List<LEU> leulist, List<LEU_filtered_values.leu> data, string msgfiledir)
        {
            if (null == leulist || 0 == leulist.Count)
            {
                return;
            }
            var groupedList = leulist.GroupBy(r => r.CI_Name);//1 sheet == 1 CI area
            foreach (var group in groupedList)
            {
                List<List<string>> pagedata = new List<List<string>>();//one page has leus in same group
                if(null != data)
                {
                    foreach (LEU leuname in group)
                    {
                        if (-1 == data.FindIndex(l => l.NAME == leuname.Name))
                        {
                            TraceMethod.Record(TraceMethod.TraceKind.WARNING,
                                $"can't find leu[{leuname.Name}] in file LEU_Result_Filtered_Values.xml, CI-LEU report will lack this LEU");
                            continue;
                        }
                        LEU_filtered_values.leu leu = data.Find(l => l.NAME == leuname.Name);
                        foreach (LEU_filtered_values.leu.BEACON b in leu.beaconList)
                        {
                            pagedata.AddRange(getrecordfrombeacon(b, leu.NAME, msgfiledir, hasTGM));
                        }
                    }
                }
                processData[group.Key] = pagedata;
            }
        }
        /// <summary>
        /// get records of 1 beacon
        /// </summary>
        /// <param name="b"></param>
        /// <param name="leuname"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<List<string>> getrecordfrombeacon(LEU_filtered_values.leu.BEACON b, string leuname, string path, bool hasBin)
        {
            List<List<string>> pagedata = new List<List<string>>();
            {
                foreach (LEU_filtered_values.leu.BEACON.MESSAGE msg in b.msgList)
                {
                    List<string> record = new List<string>();
                    record.Add(leuname);
                    record.Add(b.NAME);
                    record.Add(b.ID);
                    record.Add(b.Variants_inputs);
                    record.Add(msg.Combined_sections.getReportString());
                    record.Add(msg.VarState);
                    record.Add(msg.getReportMsgString());
                    record.Add(string.Empty);//the last one is compiled message

                    pagedata.Add(record);
                }
                //modify default msg(items and order)
                {
                    List<string> defaultmsg = pagedata[0].DeepCopy();
                    pagedata.RemoveAt(0);
                    defaultmsg.RemoveAt(5);//default not need Combined_sections
                    defaultmsg.RemoveAt(4);//default not need Variants_inputs
                    pagedata.Add(defaultmsg);
                    
                }
                //modify the compiled message
                if(hasBin)
                {
                    string tgm = $"{path}//{leuname}//telgen_{b.outNum}.TGM";
                    List<string> msglist = CIReportExcel.readcompiledmsg(tgm, (ushort)b.msgList.Count);
                    for (int i = 0; i < pagedata.Count(); ++i)
                    {
                        if (msglist.Count() > i)
                        {
                            pagedata[i].RemoveAt(pagedata[i].Count-1);
                            pagedata[i].Add(msglist[i]);
                        }
                    }
                }
                //add null row of each beacon
                pagedata.Add(null);
            }
            return pagedata;
        }

    }
}
