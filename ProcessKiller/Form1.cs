using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.ServiceProcess;
using System.Management;

namespace ProcessKiller
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer tmr;
        private double[] CPU = new double[count];
        private Process[] localAll;
        private static int count = 20;
        private Process topProc = new Process();
        private System.ServiceProcess.ServiceController[] services;
        private Process[] maxProc = new Process[count];

        public Form1()
        {

            localAll = Process.GetProcesses();
            InitializeComponent();
            tmr = new System.Timers.Timer();
            tmr.Interval = 1000;
            tmr.Elapsed += Tick;
            tmr.Start();

            for (int i = 0; i < count; i++)
            {
                CPU[i] = 0;
            }
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {


                if (localAll.Length > 0)
                {
                    int[] max = new int[count];


                    for (int i = 0; i < count; i++)
                    {
                        max[i] = 0;
                    }

                    foreach (Process proc in localAll)
                    {
                        bool brk = false;
                        int i = 0;

                        while ((i < count) && (!brk))
                        {
                            if (proc.PrivateMemorySize64 / 1024 / 1024 >= max[i])
                            {
                                for (int j = count - 1; j > i; j--)
                                {
                                    max[j] = max[j - 1];
                                    maxProc[j] = maxProc[j - 1];
                                }
                                max[i] = (int)(proc.PrivateMemorySize64 / 1024 / 1024);
                                maxProc[i] = proc;
                                brk = true;
                            }
                            i++;
                        }
                    }


                    richTextBox1.Clear();

                    //foreach (Process proc in localAll)
                    //{
                    //    richTextBox1.AppendText(proc.ProcessName + "  " + (proc.PrivateMemorySize64 / 1024 / 1024).ToString()
                    //        + "  " + (proc.TotalProcessorTime.TotalMilliseconds).ToString() + "\n");

                    //    //+ proc.TotalProcessorTime.ToString() + "  " 
                    //}

                    //int k = 0;
                    //foreach (Process proc in maxProc)
                    //{
                    //    richTextBox1.AppendText(proc.ProcessName + "  " + (proc.PrivateMemorySize64 / 1024 / 1024).ToString()
                    //        + "  " + ((proc.TotalProcessorTime.TotalMilliseconds - CPU[k]) / 10).ToString() + "\n");

                    //    CPU[k] = proc.TotalProcessorTime.TotalMilliseconds;
                    //    k++;
                    //    //+ proc.TotalProcessorTime.ToString() + "  " 
                    //}

                    for (int l = 0; l < count; l++)
                    {
                        richTextBox1.AppendText((maxProc[l].PrivateMemorySize64 / 1024 / 1024).ToString()
                           + "  " + ((maxProc[l].TotalProcessorTime.TotalMilliseconds - CPU[l]) / 40).ToString()
                           + "  " + (maxProc[l].ProcessName + "    "));


                        richTextBox1.AppendText(maxProc[l].SessionId + "\n");



                        CPU[l] = maxProc[l].TotalProcessorTime.TotalMilliseconds;
                    }
                    topProc = maxProc[0];
                }
            });
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            localAll = Process.GetProcesses();
            services = System.ServiceProcess.ServiceController.GetServices();
            richTextBox2.Clear();

            try
            {
                foreach (ServiceController scTemp in services)
                {
                    ManagementObject service = new ManagementObject(@"Win32_service.Name='" + scTemp.ServiceName + "'");
                    object o = service.GetPropertyValue("ProcessId");
                    int processId = (int)((UInt32)o);
                    Process process = Process.GetProcessById(processId);

                    if (processId != 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (processId == maxProc[i].Id)
                                richTextBox2.AppendText("Service: " + scTemp.ServiceName + "  Process ID: " + processId + "\n");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            topProc.Kill();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ServiceController ServiceCtrl = new ServiceController(textBox1.Text);
            ServiceCtrl.Stop();
        }
    }
}
