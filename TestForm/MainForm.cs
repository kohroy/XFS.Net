﻿using System;
using System.Windows.Forms;
using XFSNet;
using XFSNet.IDC;
using XFSNet.PIN;
using XFSNet.CDM;
using XFSNet.SIU;

namespace TestForm
{
    public partial class MainForm : Form
    {
        private IDC device = new IDC();
        private PIN pin = new PIN();
        private CDM cdm = new CDM();
        private SIU siu = new SIU();
        public MainForm()
        {
            int p = XFSUtil.ParseVersionString("3.16", "3.30");
            string s = p.ToString("X");
            InitializeComponent();
            Controls.Add(device);
            device.RegisterComplete += Device_RegisterComplete;
            device.RegisterError += Device_RegisterError;
            device.OpenError += Device_OpenError;
            //device.Open("IDCARDUNIT1");
            pin.PINKey += Pin_PINKey;
            // pin.Open("PINPAD1");
            cdm.DispenComplete += Cdm_DispenComplete;
            //cdm.Open("CURRENCYDISPENSER1");
            siu.Open("SIU");
        }

        private void Cdm_DispenComplete()
        {
            cdm.Present();
        }

        private void Pin_PINKey(string obj)
        {
            Console.WriteLine(obj);
        }

        private void Device_OpenError(int obj)
        {
            Console.WriteLine(obj);
        }

        private void Device_RegisterError(int obj)
        {
            Console.WriteLine(obj);
        }

        private void Device_RegisterComplete()
        {
            Console.WriteLine("RegisterComplete");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            device.ReadRawData(IDCDataSource.WFS_IDC_TRACK2 | IDCDataSource.WFS_IDC_TRACK3);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            device.EjectCard();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pin.GetData(0, false, PINDefinition.NumerKeys | XFSPINKey.WFS_PIN_FK_ENTER | XFSPINKey.WFS_PIN_FK_CANCEL, XFSPINKey.WFS_PIN_FK_UNUSED);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pin.Cancel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            device.Cancel();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            cdm.Dispense(200);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            siu.SetGuidLight(GuidLights.WFS_SIU_NOTESDISPENSER, LightControl.WFS_SIU_MEDIUM_FLASH);
        }
    }
}
