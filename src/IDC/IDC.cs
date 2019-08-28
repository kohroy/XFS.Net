using System;
using System.Runtime.InteropServices;

namespace XFSNet.IDC
{
    public unsafe class IDC : XFSDeviceBase
    {
        public event Action<int> ReadRawDataError;
        public event Action<IDCCardData[]> ReadRawDataComplete;
        public event Action EjectComplete;
        public event Action<int> EjectError;
        public event Action MediaInserted;
        public event Action MediareMoved;
        protected override void OnExecuteComplete(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case IDCDefinition.WFS_CMD_IDC_READ_RAW_DATA:
                    if (result.hResult == XFSDefinition.WFS_SUCCESS)
                    {
                        WFSIDCCardData[] data = XFSUtil.XFSPtrToArray<WFSIDCCardData>(result.lpBuffer);
                        IDCCardData[] outerData = new IDCCardData[data.Length];
                        for (int i = 0; i < data.Length; ++i)
                        {
                            outerData[i] = new IDCCardData
                            {
                                DataSource = data[i].wDataSource,
                                WriteMethod = data[i].fwWriteMethod,
                                Status = data[i].wStatus
                            };
                            if (data[i].ulDataLength > 0)
                            {
                                outerData[i].Data = new byte[data[i].ulDataLength];
                                for (int j = 0; j < data[i].ulDataLength; ++j)
                                {
                                    outerData[i].Data[j] = Marshal.ReadByte(data[i].lpbData, j);
                                }
                            }
                        }
                        OnReadRawDataComplete(outerData);
                    }
                    else
                    {
                        OnReadRawDataError(result.hResult);
                    }
                    break;
                case IDCDefinition.WFS_CMD_IDC_EJECT_CARD:
                    if (result.hResult == XFSDefinition.WFS_SUCCESS)
                    {
                        OnEjectComplete();
                    }
                    else
                    {
                        OnEjectError(result.hResult);
                    }
                    break;
            }
        }
        protected override void OnExecuteEvent(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case IDCDefinition.WFS_EXEE_IDC_MEDIAINSERTED:
                    OnMediaInserted();
                    break;
            }
        }
        protected override void OnServiceEvent(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case IDCDefinition.WFS_SRVE_IDC_MEDIAREMOVED:
                    OnMediareMoved();
                    break;
            }
        }
        public void ReadRawData(IDCDataSource sources)
        {
            int hResult = XfsApi.WFSAsyncExecute(hService, IDCDefinition.WFS_CMD_IDC_READ_RAW_DATA, new IntPtr(&sources), 0,
                Handle, ref requestID);
            if (hResult != XFSDefinition.WFS_SUCCESS)
            {
                OnReadRawDataError(hResult);
            }
        }
        public void EjectCard()
        {
            int hResult = XfsApi.WFSAsyncExecute(hService, IDCDefinition.WFS_CMD_IDC_EJECT_CARD, IntPtr.Zero, 0, Handle, ref requestID);
            if (hResult != XFSDefinition.WFS_SUCCESS)
            {
                OnEjectError(hResult);
            }
        }
        #region Event handler
        protected virtual void OnReadRawDataError(int code)
        {
            ReadRawDataError?.Invoke(code);
        }
        protected virtual void OnReadRawDataComplete(IDCCardData[] data)
        {
            ReadRawDataComplete?.Invoke(data);
        }
        protected virtual void OnEjectError(int code)
        {
            EjectError?.Invoke(code);
        }
        protected virtual void OnEjectComplete()
        {
            EjectComplete?.Invoke();
        }
        protected virtual void OnMediaInserted()
        {
            MediaInserted?.Invoke();
        }
        protected virtual void OnMediareMoved()
        {
            MediareMoved?.Invoke();
        }
        #endregion
    }
}
