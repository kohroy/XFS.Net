﻿using System;
using System.Runtime.InteropServices;

namespace XFSNet.SIU
{
    public class SIU : XFSDeviceBase
    {
        public void SetGuidLight(GuidLights pos, LightControl con)
        {
            SetGuidLight((int)pos, con);
        }
        public void SetGuidLight(int pos, LightControl con)
        {
            WFSSIUSETGUIDLIGHT guidLight = new WFSSIUSETGUIDLIGHT
            {
                fwCommand = con,
                wGuidLight = (ushort)pos
            };
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WFSSIUSETGUIDLIGHT)));
            Marshal.StructureToPtr(guidLight, ptr, false);
            int hResult = XfsApi.WFSAsyncExecute(hService, SIUDefinition.WFS_CMD_SIU_SET_GUIDLIGHT, ptr, 0, Handle, ref requestID);
        }
        protected override int GetEventClass()
        {
            return base.GetEventClass() & (~XFSDefinition.USER_EVENTS);
        }
    }
}
