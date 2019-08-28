﻿using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace XFSNet.CDM
{
    public unsafe class CDM : XFSDeviceBase
    {
        public event Action<int> DispenseError;
        public event Action DispenComplete;
        public event Action<int> PresentError;
        public event Action PresentComplete;
        public event Action ItemsTaken;
        public void Dispense(int amount, bool present = false, string currency = "CNY")
        {
            WFSCDMDENOMINATION denomination = new WFSCDMDENOMINATION
            {
                cCurrencyID = currency.ToArray(),
                lpulValues = IntPtr.Zero,
                ulAmount = amount,
                usCount = 0
            };
            WFSCDMDISPENSE dispense = new WFSCDMDISPENSE
            {
                bPresent = present,
                fwPosition = OutputPosition.WFS_CDM_POSNULL,
                usMixNumber = 1,
                lpDenomination = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WFSCDMDENOMINATION)))
            };
            Marshal.StructureToPtr(denomination, dispense.lpDenomination, false);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WFSCDMDISPENSE)));
            Marshal.StructureToPtr(dispense, ptr, false);
            int hResult = XfsApi.WFSAsyncExecute(hService, CDMDefinition.WFS_CMD_CDM_DISPENSE, ptr, 0, Handle, ref requestID);
            Marshal.FreeHGlobal(dispense.lpDenomination);
            Marshal.FreeHGlobal(ptr);
            if (hResult != XFSDefinition.WFS_SUCCESS)
            {
                OnDispenseError(hResult);
            }
        }
        public void Present()
        {
            OutputPosition pos = OutputPosition.WFS_CDM_POSNULL;
            int hResult = XfsApi.WFSAsyncExecute(hService, CDMDefinition.WFS_CMD_CDM_PRESENT, new IntPtr(&pos), 0, Handle, ref requestID);
            if (hResult != XFSDefinition.WFS_SUCCESS)
            {
                OnPresetError(hResult);
            }
        }
        protected override void OnExecuteComplete(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case CDMDefinition.WFS_CMD_CDM_DISPENSE:
                    if (result.hResult == XFSDefinition.WFS_SUCCESS)
                    {
                        OnDispenComplete();
                    }
                    else
                    {
                        OnDispenseError(result.hResult);
                    }

                    break;
                case CDMDefinition.WFS_CMD_CDM_PRESENT:
                    if (result.hResult == XFSDefinition.WFS_SUCCESS)
                    {
                        OnPresentComplete();
                    }
                    else
                    {
                        OnPresetError(result.hResult);
                    }

                    break;
            }
        }
        protected override void OnServiceEvent(ref WFSRESULT result)
        {
            switch (result.dwCommandCodeOrEventID)
            {
                case CDMDefinition.WFS_SRVE_CDM_ITEMSTAKEN:
                    OnItemsTaken();
                    break;
            }
        }
        protected virtual void OnDispenseError(int code)
        {
            DispenseError?.Invoke(code);
        }
        protected virtual void OnDispenComplete()
        {
            DispenComplete?.Invoke();
        }
        protected virtual void OnPresetError(int code)
        {
            PresentError?.Invoke(code);
        }
        protected virtual void OnPresentComplete()
        {
            PresentComplete?.Invoke();
        }
        protected virtual void OnItemsTaken()
        {
            ItemsTaken?.Invoke();
        }
    }
}
