using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CounterHooker
{
    public partial class mainForm : Form
    {
        bool runnning = false;
        CancellationTokenSource cts = null;
        CancellationToken ct = CancellationToken.None;
        Task task = null;
        const int MOUSEEVENTF_MOVE = 0x0001;

        public mainForm()
        {
            InitializeComponent();
            this.runnning = false;
            this.statusTextBox.ForeColor = Color.WhiteSmoke;
            this.statusTextBox.BackColor = Color.Red;
            this.statusTextBox.Text = "Not sending...";
            this.startStopButton.Text = "START";
        }


        private void startStopButton_Click(object sender, EventArgs e)
        {
            if (this.runnning)
            {
                this.runnning = !this.runnning;
                // do stop
                this.statusTextBox.ForeColor = Color.WhiteSmoke;
                this.statusTextBox.BackColor = Color.Red;
                this.statusTextBox.Text = "Stopping...";
                this.startStopButton.Enabled = false;
                this.startStopButton.Text = "START";

                try
                {
                    this.cts.Cancel();
                    this.cts.Dispose();
                }catch(Exception stopException)
                {
                    MessageBox.Show("ERROR", stopException.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                this.runnning = !this.runnning;
                // do go
                this.statusTextBox.ForeColor = Color.Black;
                this.statusTextBox.BackColor = Color.Lime;
                this.statusTextBox.Text = "Sending...";
                this.startStopButton.Text = "STOP";

                try
                {
                    this.cts = new CancellationTokenSource();
                    this.ct = this.cts.Token;
                    this.task = new Task(sendKeyEventLoop, this.ct);
                    this.task.Start();
                }catch(Exception startException)
                {
                    MessageBox.Show("ERROR", startException.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public delegate void DelegateUpdateStatus();
        private void UpdateStatus()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateUpdateStatus(this.UpdateStatus));
                return;
            }
            this.statusTextBox.Text = "Not sending...";
            this.startStopButton.Enabled = true;
        }


        private void sendKeyEventLoop(object obj)
        {
            CancellationToken ct = (CancellationToken)obj;
            Random rd = new System.Random();
            while (!ct.IsCancellationRequested)
            {
                Win32API.Input input = new Win32API.Input();
                input.Type = 0;
                input.ui.Mouse.Flags = MOUSEEVENTF_MOVE;
                input.ui.Mouse.Data = 0;
                input.ui.Mouse.X = 1;
                input.ui.Mouse.Y = 1;
                input.ui.Mouse.Time = 0;
                input.ui.Mouse.ExtraInfo = Win32API.GetMessageExtraInfo();
                Win32API.SendInput(1, ref input, Marshal.SizeOf(input));

                Thread.Sleep(100);

                input = new Win32API.Input();
                input.Type = 0;
                input.ui.Mouse.Flags = MOUSEEVENTF_MOVE;
                input.ui.Mouse.Data = 0;
                input.ui.Mouse.X = -1;
                input.ui.Mouse.Y = -1;
                input.ui.Mouse.Time = 0;
                input.ui.Mouse.ExtraInfo = Win32API.GetMessageExtraInfo();
                Win32API.SendInput(1, ref input, Marshal.SizeOf(input));

                //Thread.Sleep(rd.Next(30000, 100001));
                Thread.Sleep(rd.Next(3000, 10001));

            }

            this.UpdateStatus();
        }
    }
    public class Win32API
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public int Type;
            public InputUnion ui;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput Mouse;
            [FieldOffset(0)]
            public HardwareInput Hardware;
        }
        
        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public extern static int SendInput(int nInputs, ref Input pInputs, int cbsize);
        
        [DllImport("user32.dll", SetLastError = true)]
        public extern static IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        public extern static bool SetCursorPos(int x, int y);
    }
}
