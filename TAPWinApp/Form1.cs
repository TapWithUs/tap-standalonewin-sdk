using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using TAPWin;

namespace TAPWinApp
{
    public partial class Form1 : Form
    {

        private bool once;

        public Form1()
        {
            this.once = true;
            InitializeComponent();
            
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this.once)
            {
                this.once = false;
                TAPManager.Instance.OnMoused += this.OnMoused;
                TAPManager.Instance.OnTapped += this.OnTapped;
                TAPManager.Instance.OnTapConnected += this.OnTapConnected;
                TAPManager.Instance.OnTapDisconnected += this.OnTapDisconnected;
                TAPManager.Instance.OnAirGestured += this.OnAirGestured;
                TAPManager.Instance.OnChangedAirGestureState += this.OnChangedAirGestureState;
                TAPManager.Instance.Start();
            }
            
            
        }

        private void OnTapped(string identifier, int tapcode)
        {
            this.LogLine(identifier + " tapped " + tapcode.ToString());
        }

        private void OnTapConnected(string identifier, string name, int fw)
        {
            this.LogLine(identifier + " connected. (" + name + ", fw " + fw.ToString() + ")");
        }

        private void OnTapDisconnected(string identifier)
        {
            this.LogLine(identifier + " disconnected.");
        }

        private void OnMoused(string identifier, int vx, int vy, bool isMouse)
        {
            if (isMouse)
            {
                this.LogLine(identifier + " moused (" + vx.ToString() + "," + vy.ToString() + ")");
            }
            
        }

        private void LogLine(string line)
        {
            this.Invoke((MethodInvoker)delegate
            {
                textBox1.AppendText(line + Environment.NewLine);
               
            });
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TAPManager.Instance.Vibrate(new int[] { 100, 300, 100 });
        }

        private void OnAirGestured(string identifier, TAPAirGesture airGesture)
        {
            Console.WriteLine("AirGestured " + identifier + ", code: " + (int)airGesture);
        }

        private void OnChangedAirGestureState(string identifier, bool isInAirGestureState)
        {
            Console.WriteLine("Changed AirGesture State " + identifier + "isInAirGestureState: " + isInAirGestureState.ToString());
        }
    }
}
