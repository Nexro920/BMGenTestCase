using MetaFly.Summer.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestKit;

namespace BMGenTool.Info
{
    class Program
    {
        static void Main(string[] args)
        {
            //if test one case can open the log to console
            TraceMethod.SetDefaultTraceType(new TraceMethod.TraceType[] { TraceMethod.TraceType.CONCOLE, TraceMethod.TraceType.FILE});
            //TraceMethod.SetDefaultTraceType(new TraceMethod.TraceType[] { TraceMethod.TraceType.FILE });
            int cmd = 0;
            Console.WriteLine("input the test case number (0 then quit; 111 then run all cases)：");

            do{
                cmd = 4;//*111;/*/int.Parse(Console.ReadLine());
                //cmd = Convert.ToInt16(Console.ReadLine());
                if (cmd == 111)
                {
                    TestCaseParser.RunAllTestCase();
                }
                else if (cmd.Equals(0))
                {
                    break;
                }
                else if (TestCaseParser.IsTestCaseExist(cmd))
                {
                    TestCaseParser.RunTestCase(cmd);
                }
                else
                {
                    Console.WriteLine($"input {cmd} is invalid!");
                }
                TestCaseParser.Report();
                Console.ReadLine();
                Console.WriteLine("请输入需要执行的用例编号，退出输入“0”：");
            } while(true);
        }
    }
}
