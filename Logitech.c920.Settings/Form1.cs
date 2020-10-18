using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using DirectShowLib;

namespace Logitech.c920.Settings
{
    public partial class Form1 : Form
    {
        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int OleCreatePropertyFrame( IntPtr hwndOwner, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string lpszCaption, int cObjects,[MarshalAs(UnmanagedType.Interface, ArraySubType=UnmanagedType.IUnknown)] ref object ppUnk, int cPages,IntPtr lpPageClsID, int lcid, int dwReserved, IntPtr lpvReserved);
        //IMediaControl mediaControl = null;
        //IGraphBuilder graphBuilder = null;
        IBaseFilter theDevice = null;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (DsDevice ds in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                comboBox1.Items.Add(ds.Name);
            }

            //Select first combobox item
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private IBaseFilter CreateFilter(Guid category, string friendlyname)
        {
            object source = null;
            Guid iid = typeof(IBaseFilter).GUID;
            foreach (DsDevice device in DsDevice.GetDevicesOfCat(category))
            {
                if (device.Name.CompareTo(friendlyname) == 0)
                {
                    device.Mon.BindToObject(null, null, ref iid, out source);
                    break;
                }
            }

            return (IBaseFilter)source;
        }

        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //Release COM objects
            if (theDevice != null)
            {
                Marshal.ReleaseComObject(theDevice);
                theDevice = null;
            }
            //Create the filter for the selected video input device
            string devicepath = comboBox1.SelectedItem.ToString();
            rtbStatus.AppendText($"Selected {devicepath}");
            rtbStatus.AppendText(Environment.NewLine);

            theDevice = CreateFilter(FilterCategory.VideoInputDevice, devicepath);
        }

        private void btnShowProperties_Click(object sender, EventArgs e)
        {
            if (theDevice != null)
            {
                DisplayPropertyPage(theDevice);
            }
        }

        private void DisplayPropertyPage(IBaseFilter theDevice)
        {
            ISpecifyPropertyPages pProp = theDevice as ISpecifyPropertyPages;
            int hr = 0;

            if (pProp == null)
            {
                return;
            }

            DsCAUUID caGUID;
            hr = pProp.GetPages(out caGUID);
            DsError.ThrowExceptionForHR(hr);

            object oDevice = (object)theDevice;            
            hr = OleCreatePropertyFrame(this.Handle, 0, 0, "", 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);
           
            Marshal.FreeCoTaskMem(caGUID.pElems);
            Marshal.ReleaseComObject(pProp);
        }
    }
}
