using System;
using System.Runtime.InteropServices;

namespace XFSNet.PIN
{
    public unsafe class PIN : XFSDeviceBase
    {
        public event Action<int> GetDataError;
        public event Action<string> PINKey;
        public void GetData(ushort maxLen, bool autoEnd, XFSPINKey activeKeys, XFSPINKey terminateKeys, XFSPINKey activeFDKs = XFSPINKey.WFS_PIN_FK_UNUSED,
            XFSPINKey terminateFDKs = XFSPINKey.WFS_PIN_FK_UNUSED)
        {
            WFSPINGETDATA inputData = new WFSPINGETDATA
            {
                usMaxLen = maxLen,
                bAutoEnd = autoEnd,
                ulActiveFDKs = activeFDKs,
                ulActiveKeys = activeKeys,
                ulTerminateFDKs = terminateFDKs,
                ulTerminateKeys = terminateKeys
            };
            int len = Marshal.SizeOf(typeof(WFSPINGETDATA));
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(inputData, ptr, false);
            int hResult = XfsApi.WFSAsyncExecute(hService, PINDefinition.WFS_CMD_PIN_GET_DATA, ptr, 0, Handle, ref requestID);
            Marshal.FreeHGlobal(ptr);
            if (hResult != XFSDefinition.WFS_SUCCESS)
            {
                OnGetDataError(hResult);
            }
        }
        protected override void OnExecuteComplete(ref WFSRESULT result)
        {
            switch(result.dwCommandCodeOrEventID)
            {
                case PINDefinition.WFS_CMD_PIN_GET_DATA:
                    if (result.hResult != XFSDefinition.WFS_SUCCESS)
                    {
                        OnGetDataError(result.hResult);
                    }

                    break;
            }
        }
        protected override void OnExecuteEvent(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case PINDefinition.WFS_EXEE_PIN_KEY:
                    WFSPINKEY key = new XFSNet.PIN.WFSPINKEY();
                    XFSUtil.PtrToStructure(result.lpBuffer, ref key);
                    OnPINKey(ref key);
                    break;
            }
        }
        protected virtual void OnGetDataError(int code)
        {
            GetDataError?.Invoke(code);
        }
        protected virtual void OnPINKey(ref WFSPINKEY key)
        {
            PINKey?.Invoke(key.ulDigit.ToString().Substring(11));
        }
    }
}
