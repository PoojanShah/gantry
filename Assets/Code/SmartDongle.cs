///   ***** BEGIN LICENSE BLOCK *****
///
///   Version: MPL 1.1/GPL 2.0/LGPL 2.1
///
///   The contents of this file are subject to the Mozilla Public License
///   Version 1.1 (the "License"); you may not use this file except in
///   compliance with the License. You may obtain a copy of the License at
///   http://www.mozilla.org/MPL/
///
///   Software distributed under the License is distributed on an "AS IS" basis,
///   WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
///   for the specific language governing rights and limitations under the
///   License.
///
///   The Original Code is MicroWorks, Inc. code.
///
///   The Initial Developer of the Original Code is MicroWorks, Inc. Portions
///   created by MicroWorks, Inc. are Copyright (C) 2008
///   MicroWorks, Inc. All Rights Reserved.
///
///   Contributor(s):
///
///   Alternatively, the contents of this file may be used under the terms of
///   either the GNU General Public License Version 2 or later (the "GPL"), or
///   the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
///   in which case the provisions of the GPL or the LGPL are applicable instead
///   of those above. If you wish to allow use of your version of this file only
///   under the terms of either the GPL or the LGPL, and not to allow others to
///   use your version of this file under the terms of the MPL, indicate your
///   decision by deleting the provisions above and replace them with the notice
///   and other provisions required by the GPL or the LGPL. If you do not delete
///   the provisions above, a recipient may use your version of this file under
///   the terms of any one of the MPL, the GPL or the LGPL.
///
///   ***** END LICENSE BLOCK *****

using System;
using System.Threading;
using System.Runtime.InteropServices;

/// <summary>
/// SmartDongle Class for C# Version 3.3
///
/// MicroWorks, Inc.
/// 2808 North Cole Road
/// Boise, ID 83704
///
/// http://www.smartdongle.com
/// </summary>

public class SmartDongle // SmartDongle USB Software Key Class
{
    public static Mutex SmartDongleMutex = new Mutex(false, "SmartDongleMutex");

    static SmartDongle()
    {
        ClassGuid = new Guid(0x5c98f9f9, 0xc082, 0x431a, 0xa0, 0xad, 0xb3, 0x2f, 0x70, 0xbd, 0x26, 0x89);
    }

    /* Valid SmartDongle serial numbers will always be this length */
    public const int CP2_SERIALNUM_LENGTH = 12;

    private class Device
    {
        public Device()
        {
            signature = new byte[CY7C637XX.SIG_LENGTH];
        }

        public Byte[] signature;
        public IntPtr device;
        public Byte options;

        public Byte minor;
        public Byte major;
    }

    /* SmartDongle's GUID */
    static Guid ClassGuid;

    public sealed class UserOption
    {
        /* Three on-chip SmartDongle encrpytion modes to choose from.
        *  This provide protection for SmartDongle on-board memory only.
        *  Select QTS_OPTION_CRYPT_IO to encrypt data written to SmartDongle
        *  user memory.
        */
        public const byte QTS_OPTION_ON_CHIP_EPROM_PROTECT_DISABLE = 0x01;
        public const byte QTS_OPTION_ON_CHIP_EPROM_PROTECT_FAST = 0x02;
        public const byte QTS_OPTION_ON_CHIP_EPROM_PROTECT_STRONG = 0x00;
    }

    /// <summary>
    /// Given the correct keys,
    /// P1 and P2, will read SmartDongle memory into buffer.
    /// </summary>
    public static SmartDnglError Read(
        UInt64 P1,
        UInt64 P2,
        UInt16 address,
        byte[] buffer,
        int size)
    {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        int bufferSize = size;
        Device dongle = new Device();


        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        /* Attempt to read user EEPROM space.
        *  On failure, attempt to disable dongle and return error.
        */
        error = Command.ReadEepromUserSpace(
            dongle,
            P1,
            P2,
            address,
            buffer,
            ref bufferSize);

        if (error != SmartDnglError.UskOk) {
            Command.DisableUsk(dongle.device);
        }
        else {
            error = Command.DisableUsk(dongle.device);
        }

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// Given the correct keys,
    /// P1 and P2, will write buffer into SmartDongle memory.
    /// </summary>
    public static SmartDnglError Write(
        UInt64 P1,
        UInt64 P2,
        UInt16 address,
        byte[] buffer,
        int size)
    {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        int bufferSize = size;
        Device dongle = new Device();

        /* An exception would occurr if size argument exceeded buffer length */
        if (buffer.Length < size) {
            SmartDongleMutex.ReleaseMutex();
            return SmartDnglError.UskErrBadArgument;
        }

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        /* Attempt to write to user EEPROM space.
        *  On failure, attempt to disable dongle and return error.
        */
        error = Command.WriteEepromUserSpace(
            dongle,
            P1,
            P2,
            address,
            buffer,
            ref bufferSize);

        if (error != SmartDnglError.UskOk)
            Command.DisableUsk(dongle.device);
        else
            error = Command.DisableUsk(dongle.device);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// Reset, wait for SmartDongle to come ready.
    /// </summary>
    public static SmartDnglError Reset()
    {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();
        int retries;

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        Command.Reset(dongle.device);
        Win32Methods.CloseHandle(dongle.device);

        Thread.Sleep(CY7C637XX.QTS_PLUG_N_PLAY_WAIT);

        for (retries = 0; retries < CY7C637XX.QTS_READY_RETRIES; retries++) {
            error = OpenUsbDev(ref dongle);
            if (error != SmartDnglError.UskOk) {
                Thread.Sleep(CY7C637XX.QTS_READY_WAIT);
                continue;
            }

            error = IOCTL_Command.isDeviceReady(dongle.device, 0);
            if (error == SmartDnglError.UskOk)
                break;

            Win32Methods.CloseHandle(dongle.device);
            Thread.Sleep(CY7C637XX.QTS_READY_WAIT);
        }

        Win32Methods.CloseHandle(dongle.device);

        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return SmartDnglError.UskResetComeready;
        }

        SmartDongleMutex.ReleaseMutex();
        return SmartDnglError.UskOk;
    }

    /// <summary>
    /// Read the SmartDongle onboard memory encryption mode.
    /// </summary
    public static SmartDnglError ReadCryptMode(ref byte mode)
    {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();


        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        error = IOCTL_Command.getMode(dongle.device, out mode);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// Change the SmartDongle onboard memory encryption mode.
    /// </summary>
    public static SmartDnglError WriteCryptMode(
        UInt64 P1,
        UInt64 P2,
        byte mode)
    {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();
        byte currentMode;
        int retries;


        /* Mask out extraneous bits */
        mode &=
            UserOption.QTS_OPTION_ON_CHIP_EPROM_PROTECT_DISABLE |
            UserOption.QTS_OPTION_ON_CHIP_EPROM_PROTECT_FAST;

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        /* Get fixed options */
        error = IOCTL_Command.getOpt(ref dongle);
        if (error != SmartDnglError.UskOk) {
            Win32Methods.CloseHandle(dongle.device);
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        /* v1.50 SmartDongle must be reset if mode change */
        if (((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT2) == 0) &&
             ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) > 0))
        {

            /* Get current encryption mode */
            error = IOCTL_Command.getMode(dongle.device, out currentMode);
            if (error != SmartDnglError.UskOk) {
                Win32Methods.CloseHandle(dongle.device);
                SmartDongleMutex.ReleaseMutex();
                return error;
            }

            /* Change mode only if current mode  mismatches requested mode. */
            if (mode != currentMode) {
                Command.Reset(dongle.device);
                Win32Methods.CloseHandle(dongle.device);

                Thread.Sleep(CY7C637XX.QTS_PLUG_N_PLAY_WAIT);

                for (retries = 0; retries < CY7C637XX.QTS_READY_RETRIES; retries++) {
                    error = OpenUsbDev(ref dongle);
                    if (error != SmartDnglError.UskOk)
                        continue;

                    error = IOCTL_Command.isDeviceReady(dongle.device, 0);
                    if (error == SmartDnglError.UskOk)
                        break;

                    Win32Methods.CloseHandle(dongle.device);
                    Thread.Sleep(CY7C637XX.QTS_READY_WAIT);
                }

                if (error != SmartDnglError.UskOk) {
                    Win32Methods.CloseHandle(dongle.device);
                    SmartDongleMutex.ReleaseMutex();
                    return SmartDnglError.UskResetComeready;
                }
            }

            /* Requested mode same as current mode, return. */
            else {
                Win32Methods.CloseHandle(dongle.device);
                SmartDongleMutex.ReleaseMutex();
                return SmartDnglError.UskOk;
            }
        }

        /* SmartDongle must be enabled for a mode change */
        error = Command.EnableUsk(dongle, P1, P2);
        if (error != SmartDnglError.UskOk) {
            Win32Methods.CloseHandle(dongle.device);
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        /* Set the encryption mode */
        error = IOCTL_Command.setMode(dongle.device, mode);

        Command.DisableUsk(dongle.device);

        Win32Methods.CloseHandle(dongle.device);
        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// SmartDongle Red LED on.
    /// </summary>
    public static SmartDnglError Red() {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        error = Command.Red(dongle.device);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// SmartDongle Green LED on.
    /// </summary>
    public static SmartDnglError Green() {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        error = Command.Green(dongle.device);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// SmartDongle LED off.
    /// </summary>
    public static SmartDnglError Off() {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device();

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        error = Command.Off(dongle.device);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    /// <summary>
    /// Retrieve SmartDongle serial number.
    /// </summary>
    public static SmartDnglError GetSerialNumber(ref String serialNumber) {
        try {
            SmartDongleMutex.WaitOne();
        } catch (AbandonedMutexException ex) {
            Console.WriteLine("Abandoned Mutex Detected. Continuing.");
            Reset();
        }

        SmartDnglError error;
        Device dongle = new Device(); 

        error = OpenUsbDev(ref dongle);
        if (error != SmartDnglError.UskOk) {
            SmartDongleMutex.ReleaseMutex();
            return error;
        }

        error = IOCTL_Command.getSerialNumber(dongle.device, ref serialNumber);

        Win32Methods.CloseHandle(dongle.device);

        SmartDongleMutex.ReleaseMutex();
        return error;
    }

    private sealed class CY7C637XX
    {
        /* User memory page size */
        public const int EEPROMPAGESIZE = 16;

        /* Number of prefetch pages in an enabled SmartDongle */
        public const int CP2_PAGE_PREFETCH_ENABLED = 8;

        /* Length of serial number USB descriptor string */
        public const int CP2_SER_NUMB_STRING_LENGTH = 2 + (CP2_SERIALNUM_LENGTH * 2);

        /* usb100.h */
        public const byte USB_STRING_DESCRIPTOR_TYPE = 0x03;

        /* 32 bit signature for read/write access */
        public const int SIG_LENGTH = 4;

        /* 16 bit address */
        public const int ADDR_LENGTH = 2;

        /* 64 bit keys */
        public const int SECRETS_KEY_LEN = 8;


        /* Fixed options on SmartDongle queried by GetOpt() */

        /* SmartDongle v1.70 or above, extended functionality */
        public const byte QTS_OPTION_EXTENDED_SUPPORT2 = 0x20;

        /* SmartDongle v1.50 or above, extended functionality */
        public const byte QTS_OPTION_EXTENDED_SUPPORT = 0x80;

        /* SmartDongle requires a new key for each access */
        public const byte QTS_OPTION_STRICT_ONLY = 0x40;

        /* Firmware option bitmap */
        public const byte QTS_FIRMWARE_EXTENDED_SUPPORT2 = 0x40;
        public const byte QTS_FIRMWARE_EXTENDED_SUPPORT = 0x08;
        public const byte QTS_FIRMWARE_STRICT_ONLY = 0x04;

        public const byte QTS_READ_PAGE_READY_MASK =
            QTSRDPAGEVALID | QTSRDADDRESSRANGE | QTSRDDENY | QTSRDEEPROMERROR;

        public const byte QTS_WRITE_ERROR_MASK =
            QTSWRADDRESSRANGE | QTSWRDENY | QTSWREEPROMERROR;

        public const byte QTS_READ_ERROR_MASK = QTSRDEEPROMERROR;

        /* sd_helper.h */
        public const int QTS_IOCTL_RETRYS = 5;
        public const int QTS_IOCTL_FAIL_WAIT = 50;  /* 100 nanosecond ticks */
        public const int QTS_IOCTL_BUFFSIZE = 1024;

        public const int QTS_READ_RETRIES = 3;
        public const int QTS_WRITE_RETRIES = 3;
        public const int QTS_SCAN_RETRIES = 10;

        public const int QTS_READY_RETRIES = 8;
        public const int QTS_READY_WAIT = 250;
        public const int QTS_PLUG_N_PLAY_WAIT = 1000;

        public const int QTS_MAXPREFETCH = 128;


        /* Various timeouts (in mSec).  */

        /* Max time period to wait for an EEPROM page */
        public const int QTSPAGEWAIT = 2000;
        /* Max time period to wait for a state change */
        public const int QTSSTATEWAIT = 2500;
        /* Max time period to wait for Time Write Cycle. */
        public const int QTSTWCWAIT = 2000;
        /* Max time period to wait for task_loop */
        public const int QTSBUSYWAIT = 5000;

        /* Each USB low speed packet is 8 bytes */
        public const int QTSPACKETSIZE = 8;

        public const int QTSMAXBUFFERLENGTH = 255;

        /* Page flag bits: IOCTL_QTSDONGLE_GET_PAGE_FLAGS */
        public const byte QTSRDPAGEVALID = 0x01;
        public const byte QTSWRPAGEVALID = 0x02;

        /* EEPROM read page errors */
        public const byte QTSRDADDRESSRANGE = 0x04;
        public const byte QTSRDDENY = 0x08;
        public const byte QTSRDEEPROMERROR = 0x10;  /* SPI EPROM read timeout */

        /* EEPROM write page errors */
        public const byte QTSWRADDRESSRANGE = 0x20;
        public const byte QTSWRDENY = 0x40;
        public const byte QTSWREEPROMERROR = 0x80; /* SPI EPROM write timeout */


        /* EEPROM data error bits: IOCTL_QTSDONGLE_GET_EEPROM_ERROR_FLAG */

        /* EEPROM SPI access timeout */
        public const byte QTSRDTIMEOUT = 0x01;

        /* EEPROM SPI access timeout and data miscompare */
        public const byte QTSWRTIMEOUT = 0x10;
        public const byte QTSWRDATA = 0x20;

        /* The Time Write Cycle status
        * The status of the Time Write Cycle register.
        * If the register indicates that a write cycle is
        * active, an EEPROM read or write is denied.
        * IOCTL_QTSDONGLE_GET_TWC_FLAG
        */
        public const byte QTSTWCACTIVE = 0xFF;
        public const byte QTSTWCINACTIVE = 0x00;

        /* Task loop activity flag IOCTL_QTSDONGLE_GET_BUSY_FLAG */
        public const byte QTSREADY = 0x00;
        public const byte QTSBUSY = 0xFF;

        /* The 8 bit ouput port address, the CY7C63221 has P0.0 - P0.8, and
        *  P2.0, P2.1 (XTALIN, XTALOUT disabled)
        */
        public const byte QTSPORT0 = 0x00;
        public const byte QTSPORT1 = 0x01;
        public const byte QTSPORT2 = 0x02;

        /* The 8 bit output port LED drive is active low, using lines 0 and 1 */
        public const byte QTSLEDPORT = QTSPORT0;
        public const byte QTSLEDOFF = 0xFF;
        public const byte QTSGREEN = 0xFE;
        public const byte QTSRED = 0xFD;

        /* Security states */
        public const byte QTSDISABLED = 0x00;
        public const byte QTSENABLED = 0x01;

        /* Key states for host/dongle authentication handshake */
        public const byte QTSINITIAL = 0x00;
        public const byte QTSREPLY = 0x01;

        /* Bitmap of SmartDongle status byte. */
        public const byte QTSKEYSTATEBIT = 0x80;
        public const byte QTSKEYINITIAL = 0x00;
        public const byte QTSKEYREPLY = 0x80;

        public const byte QTSTWCSTATEBIT = 0x04;
        public const byte QTSTWCBUSY = 0x04;
        public const byte QTSTWCDONE = 0x00;

        public const byte QTSBUSYSTATEBIT = 0x20;
        public const byte QTSISBUSY = 0x20;
        public const byte QTSNOTBUSY = 0x00;

        /* Read SmartDongle's extended functionality bits are mapped to
        *  eprom error bits.
        *
        *  This bit will be set in v1.50. Indicates that new
        *  functionality is supported.
        */
        public const byte QTSEXTSUPPORT = 0x08;

        /* Read SmartDongle's extended functionality bits are mapped to
        *  eprom error bits.
        *
        *   If this bit is set in v1.50, strict access only.
        */
        public const byte QTSSTRICTONLY = 0x04;

        CY7C637XX() { }


        public static byte[] SignatureBytesFromKey(UInt64 key)
        {
            byte tmp;
            byte[] data;


            data =
                BitConverter.GetBytes((UInt32)((key & 0xFFFFFFFF00000000) >> 32));

            /* SmartDongle is expecting two big-endian shorts */
            tmp = data[2];
            data[2] = data[0];
            data[0] = tmp;

            tmp = data[3];
            data[3] = data[1];
            data[1] = tmp;

            return data;
        }

        public static byte[] BytesFromKey(UInt64 key)
        {
            int i, j;
            byte[] data;
            byte tmp;

            data = BitConverter.GetBytes((UInt64)key);

            for (i = 0, j = 7; i < 4; i++, j--)
            {
                tmp = data[i];
                data[i] = data[j];
                data[j] = tmp;
            }

            return data;
        }
    }

    private static int getEpochTime()
    {
        TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
        return (int)t.TotalSeconds;
    }

    /* a = a + b + carry_in   function returns true if carry out generated */
    private static bool add_bytes(ref byte a, byte b, bool carryIn)
    {
        UInt16 x, y, z;
        byte[] tmp = new byte[4];
        bool addBytes;


        tmp[0] = a;
        y = BitConverter.ToUInt16(tmp, 0);

        tmp[0] = b;
        z = BitConverter.ToUInt16(tmp, 0);

        x = (UInt16)(y + z);

        if (carryIn)
        {
            x++;
        }

        if ((x & 0x100) > 0)
        {
            x &= 0xff;
            addBytes = true;
        }
        else
        {
            addBytes = false;
        }

        tmp = BitConverter.GetBytes(x);

        a = tmp[0];

        return addBytes;
    }

    private static bool shift_byte(ref byte a, bool carryIn)
    {
        UInt32 x;
        byte[] tmp = new byte[4];
        bool shiftByte;


        tmp[0] = a;
        x = BitConverter.ToUInt32(tmp, 0);

        /* Return true if carry out */
        if ((x & 0x01) > 0)
        {
            x &= 0xfe;
            shiftByte = true;
        }
        else
        {
            shiftByte = false;
        }

        /* Shift right */
        x >>= 1;

        /* Carry in */
        if (carryIn)
        {
            x |= 0x80;
        }

        tmp = BitConverter.GetBytes(x);

        a = tmp[0];

        return shiftByte;
    }

    /* Shift 128 bits, (var64_2, var64_3)
    *  Returns true if a '1' was shifted out.
    */
    private static bool logical_right_shift_product(
        ref byte[] var64_2, ref byte[] var64_3)
    {
        int i;
        bool carry;


        carry = false;

        for (i = 0; i <= 7; i++)
        {
            carry = shift_byte(ref var64_2[i], carry);
        }

        for (i = 0; i <= 7; i++)
        {
            carry = shift_byte(ref var64_3[i], carry);
        }

        return carry;
    }

    /* Add var64_1 and var64_2, result in var64_2, ignore carry out */
    private static void add_64X64(ref byte[] var64_1, ref byte[] var64_2)
    {
        int i;
        bool carry;


        carry = false;

        for (i = 7; i >= 0; i--)
        {
            carry = add_bytes(ref var64_2[i], var64_1[i], carry);
        }
    }

    private static UInt64 Multiply(UInt64 Multiplier, UInt64 Multiplicand)
    {
        int i, j;
        byte[] tmp = new byte[8];

        byte[] var64_1 = new byte[8];
        byte[] var64_2 = new byte[8];
        byte[] var64_3 = new byte[8];


        /* Convert big endian to little endian */
        tmp = BitConverter.GetBytes(Multiplicand);
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            var64_1[j] = tmp[i];
        }

        for (i = 0; i <= 7; i++)
        {
            var64_2[1] = 0x00;
        }

        /* Convert big endian to little endian */
        tmp = BitConverter.GetBytes(Multiplier);
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            var64_3[j] = tmp[i];
        }

        /* Shift and test multiplication */
        for (i = 0; i < 64; i++)
        {
            if (logical_right_shift_product(ref var64_2, ref var64_3))
            {
                add_64X64(ref var64_1, ref var64_2);
            }
        }

        /* Final shift */
        logical_right_shift_product(ref var64_2, ref var64_3);

        /* Convert back to big endian */
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            tmp[i] = var64_3[j];
        }

        return BitConverter.ToUInt64(tmp, 0);
    }

    /* Addend1 + Addend2 */
    private static UInt64 Add(UInt64 Addend1, UInt64 Addend2)
    {
        int i, j;
        byte[] tmp = new byte[8];

        byte[] var64_1 = new byte[8];
        byte[] var64_2 = new byte[8];
        byte[] var64_3 = new byte[8];


        /* Convert big endian to little endian */
        tmp = BitConverter.GetBytes(Addend1);
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            var64_1[j] = tmp[i];
        }

        /* Convert big endian to little endian */
        tmp = BitConverter.GetBytes(Addend2);
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            var64_2[j] = tmp[i];
        }

        add_64X64(ref var64_1, ref var64_2);

        /* Convert back to big endian */
        for (i = 0, j = 7; i <= 7; i++, j--)
        {
            tmp[i] = var64_2[j];
        }

        return BitConverter.ToUInt64(tmp, 0);
    }

    private static SmartDnglError OpenUsbDev(ref Device dongle)
    {
        /* From WINNT.h */
        const int FILE_SHARE_READ = 0x00000001;
        const int FILE_SHARE_WRITE = 0x00000002;

        /* From WinBase.h */
        const int OPEN_EXISTING = 3;

        const int DIGCF_INTERFACEDEVICE = 0x00000010;
        const int DIGCF_PRESENT = 0x00000002;

        const int ERROR_INSUFFICIENT_BUFFER = 122;

        const int INVALID_HANDLE_VALUE = -1;

        bool success;
        int deviceInfoSet;
        Win32Methods.SP_DEVICE_INTERFACE_DATA devInterfaceData;
        Win32Methods.SP_DEVICE_INTERFACE_DETAIL_DATA devInterfaceDetailData = new Win32Methods.SP_DEVICE_INTERFACE_DETAIL_DATA();
        int devInterface;
        int requiredLength;
        IntPtr buffer;
        IntPtr pDevicePath;
        string devicePath;


        devInterfaceData = new Win32Methods.SP_DEVICE_INTERFACE_DATA();
        devInterfaceData.cbSize =
            Marshal.SizeOf(typeof(Win32Methods.SP_DEVICE_INTERFACE_DATA));

        deviceInfoSet = Win32Methods.SetupDiGetClassDevs(
            ClassGuid.ToByteArray(),
            IntPtr.Zero,
            IntPtr.Zero,
            DIGCF_PRESENT | DIGCF_INTERFACEDEVICE);
        if (deviceInfoSet == INVALID_HANDLE_VALUE)
        {
            return SmartDnglError.UskErrOpen;
        }

        devInterface = 0;

        success = Win32Methods.SetupDiEnumDeviceInterfaces(
            deviceInfoSet,
            IntPtr.Zero,
            ClassGuid.ToByteArray(),
            devInterface,
            ref devInterfaceData);
        if (!success)
        {
            Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return SmartDnglError.UskErrOpen;
        }

        /* Get the required buffer size, allocate pointer of returned
        *  size and call SetupDiGetDeviceInterfaceDetail() again.
        */
        success =
            Win32Methods.SetupDiGetDeviceInterfaceDetail(
            deviceInfoSet,
            ref devInterfaceData,
            IntPtr.Zero,
            0,
            out requiredLength,
            IntPtr.Zero);
        if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
        {
            Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return SmartDnglError.UskErrNoDevice;
        }

        // Fix for x64 compatibility.
        // On x64, Marshal.SizeOf() returns the wrong size since we have to use Pack=1 to get it to work when running as a 32 bit process
        // on x64 Windows. If built as x64, or AnyCPU running on x64 Windows, we need to set the size to 8 for it to
        // work correctly.
        if (IntPtr.Size == 4)
        {
            devInterfaceDetailData.cbSize =
                Marshal.SizeOf(
                    typeof(Win32Methods.SP_DEVICE_INTERFACE_DETAIL_DATA));
        }
        else
        {
            devInterfaceDetailData.cbSize = 8;
        }

        buffer = Marshal.AllocHGlobal(requiredLength);

        Marshal.StructureToPtr(devInterfaceDetailData, buffer, false);

        success = Win32Methods.SetupDiGetDeviceInterfaceDetail(
            deviceInfoSet,
            ref devInterfaceData,
            buffer,
            requiredLength,
            out requiredLength,
            IntPtr.Zero);
        if (!success)
        {
            Marshal.FreeHGlobal(buffer);
            Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return SmartDnglError.UskErrNoDevice;
        }

        /* Retrieve the DevicePath from
        *  SP_DEVICE_INTERFACE_DETAIL_DATA
        *  4 == Offset of first DevicePath character
        */
        pDevicePath = (IntPtr)((int)buffer + 4);
        devicePath = Marshal.PtrToStringAuto(pDevicePath);

        /* Open handle to physical device */
        dongle.device =
            Win32Methods.CreateFile(
            devicePath.ToCharArray(),
            0,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            IntPtr.Zero,
            OPEN_EXISTING,
            0,
            IntPtr.Zero);
        if (Marshal.GetLastWin32Error() != 0)
        {
            Marshal.FreeHGlobal(buffer);
            Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return SmartDnglError.UskErrOpen;
        }

        Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
        Marshal.FreeHGlobal(buffer);
        return SmartDnglError.UskOk;
    }

    sealed private class Win32Methods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public Int32 cbSize;
            public char DevicePath;
        }

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int
            SetupDiGetClassDevs(
                byte[] ClassGuid,
                IntPtr Enumerator,
                IntPtr hwndParent,
                int Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool
            SetupDiDestroyDeviceInfoList(int DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool
            SetupDiEnumDeviceInterfaces(
                int DeviceInfoSet,
                IntPtr DeviceInfoData,
                byte[] ClassGuid,
                int MemberIndex,
                ref Win32Methods.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool
            SetupDiGetDeviceInterfaceDetail(
                int DeviceInfoSet,
                ref Win32Methods.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                IntPtr DeviceInterfaceDetailData,
                int DeviceInterfaceDetailDataSize,
                out int RequiredSize,
                IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool
            SetupDiGetDeviceInterfaceDetail(
            int DeviceInfoSet,
            ref Win32Methods.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            byte[] DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize,
            out int RequiredSize,
            IntPtr DeviceInfoData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            char[] lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CloseHandle(IntPtr hObject);

        /* DeviceIoControl: Four overloaded functions for convenience. */
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            UInt32 dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            UInt32 dwIoControlCode,
            byte[] lpInBuffer,
            int nInBufferSize,
            IntPtr lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            UInt32 dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            byte[] lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            UInt32 dwIoControlCode,
            byte[] lpInBuffer,
            int nInBufferSize,
            byte[] lpOutBuffer,
            int nOutBufferSize,
            out int lpBytesReturned,
            IntPtr lpOverlapped);

        private Win32Methods() { }
    }

    sealed class Command
    {
        /* Read from an unlocked SmartDongle */
        public static SmartDnglError ReadEepromUserSpace(
            Device dongle,
            UInt64 P1,
            UInt64 P2,
            UInt16 address,
            byte[] buffer,
            ref int size)
        {
            byte flags;
            SmartDnglError error;
            int i, j, retries;
            UInt16 offset;
            byte[] out_buffer = new byte[8 * CY7C637XX.EEPROMPAGESIZE];
            byte[] address_bytes;
            byte[] page0;
            bool firstPrefetch;
            bool singlePrefetch;
            int prefetches;
            int prefetchSize;
            int to_load;
            int bytesReturned = 0;


            error = IOCTL_Command.getOpt(ref dongle);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            prefetches = 1;
            prefetchSize = 0;
            singlePrefetch = true;
            error = SmartDnglError.UskOk;

            /* User may request address that is inside a page.
            *  Get the offset from the first page.
            *  page0[0] is the offset.
            */
            page0 = BitConverter.GetBytes(address);
            page0[0] &= 0x0f;

            /* Mask out lower address nibble */
            address &= 0xFFF0;

            /* Int16 address to big endian byte array */
            address_bytes = BitConverter.GetBytes(address);

            to_load = size;
            size = 0;

            for (i = 0, firstPrefetch = true; i < prefetches; i++)
            {

                offset = (UInt16)((i * prefetchSize) + address);

                /* Must obtain new key for each read transaction if strict
                *  access required by SmartDongle.
                *  Extended functionality must be supported in firmware.
                */
                if ((dongle.options & CY7C637XX.QTS_OPTION_STRICT_ONLY) != 0)
                {
                    error = EnableUsk(dongle, P1, P2);
                }
                else if (firstPrefetch)
                {
                    error = EnableUsk(dongle, P1, P2);
                }

                /* Return on error */
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                for (retries = 0; retries < CY7C637XX.QTS_READ_RETRIES; retries++)
                {

                    /* Request prefetch starting at offset */
                    error =
                        IOCTL_Command.requestEpromPrefetch(dongle.device, offset);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }

                    /* Return error status if request failed */
                    error = IOCTL_Command.waitForPageRead(
                        dongle.device, CY7C637XX.QTSPAGEWAIT, out flags);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }

                    /* Don't retry on an address range error */
                    if ((flags & CY7C637XX.QTS_READ_ERROR_MASK) > 0)
                    {
                        error = SmartDnglError.UskErrEpromRead;

                        if ((flags &
                            (CY7C637XX.QTSRDDENY | CY7C637XX.QTSRDEEPROMERROR)) > 0)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    /* Read buffered pages */
                    error = IOCTL_Command.readEpromPrefetch(
                        dongle, out_buffer, out bytesReturned);
                    if (error == SmartDnglError.UskOk)
                    {
                        break;
                    }

                    error = EnableUsk(dongle, P1, P2);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }
                }

                /* return on error */
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                /* Use the number of bytes returned calculated the number of
                *  prefetches needed.
                *  Calculate using the offset into the first EPROM page.
                */
                if (firstPrefetch)
                {
                    /* Sanity check
                    *  default to single page of prefetch.
                    *  the 1.5 returns 1024 but has only 8 bytes of data.
                    */
                    if ((bytesReturned < CY7C637XX.EEPROMPAGESIZE) ||
                        (bytesReturned > CY7C637XX.QTS_MAXPREFETCH))
                    {
                        prefetchSize = CY7C637XX.EEPROMPAGESIZE;
                    }
                    else
                    {
                        prefetchSize = bytesReturned;
                    }

                    prefetches = (to_load + page0[0]) / prefetchSize;

                    /* Another prefetch for remainder */
                    if (((to_load + page0[0]) % prefetchSize) > 0)
                    {
                        prefetches++;
                    }

                    /* A single prefetch is read with regard to page 0 offset and
                    *  data length remainder.
                    */
                    if (prefetches > 1)
                    {
                        singlePrefetch = false;
                    }
                    else
                    {
                        singlePrefetch = true;
                    }
                }

                /*
                    Process prefetches.
                */
                if (singlePrefetch)
                {

                    for (j = 0; j < to_load; j++)
                    {
                        buffer[j] = out_buffer[j + page0[0]];
                    }

                    size = to_load;
                }
                else if ((to_load + page0[0]) >= prefetchSize)
                {

                    if (firstPrefetch)
                    {
                        for (j = 0; j < (prefetchSize - page0[0]); j++)
                        {
                            buffer[j] = out_buffer[j + page0[0]];
                        }

                        size += prefetchSize - page0[0];
                        to_load -= prefetchSize - page0[0];
                    }
                    else
                    {

                        for (j = 0; j < prefetchSize; j++)
                        {
                            buffer[j + size] = out_buffer[j];
                        }

                        size += prefetchSize;
                        to_load -= prefetchSize;
                    }
                }
                else
                {

                    /*
                        Load remainder.
                        A "first prefetch" here would be a "single prefetch"
                    */
                    for (j = 0; j < to_load; j++)
                    {
                        buffer[j + size] = out_buffer[j];
                    }

                    size += to_load;
                    to_load = 0;
                }

                firstPrefetch = false;
            }

            return SmartDnglError.UskOk;
        }

        /*
            Write to SmartDongle user memory 
        */
        public static SmartDnglError WriteEepromUserSpace(
            Device dongle,
            UInt64 P1,
            UInt64 P2,
            UInt16 address,
            byte[] buffer,
            ref int size)
        {
            byte flags;
            SmartDnglError error = SmartDnglError.UskOk;
            int to_send, sent, received;
            int i, j, retries, pages;
            UInt16 offset;
            byte[] page = new byte[CY7C637XX.EEPROMPAGESIZE];
            byte[] address_bytes;
            byte[] page0;
            bool firstPage;
            bool singlePage;


            error = IOCTL_Command.getOpt(ref dongle);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* User may request address that is inside a page.
            *  Get the offset from the first page.
            *  page0[0] is the offset.
            */
            page0 = BitConverter.GetBytes(address);
            page0[0] &= 0x0f;

            /* Mask out lower address nibble */
            address &= 0xFFF0;

            /* Int16 address to big endian byte array */
            address_bytes = BitConverter.GetBytes(address);

            to_send = size;
            size = 0;

            /* Calculate number of write pages with respect to any offset into the
            *  first EPROM page.
            */
            pages = (to_send + (int)page0[0]) / CY7C637XX.EEPROMPAGESIZE;
            if (((to_send + (int)page0[0]) % CY7C637XX.EEPROMPAGESIZE) > 0)
            {
                pages++;
            }

            /* If a single page is written,
              *  page 0 offset and the data length remainder is regarded.
              */
            if (pages > 1)
            {
                singlePage = false;
            }
            else
            {
                singlePage = true;
            }

            for (i = 0, firstPage = true; i < pages; i++)
            {

                offset = (UInt16)((i * CY7C637XX.EEPROMPAGESIZE) + address);

                /* First and last page - single page. */
                if (singlePage)
                {

                    /* Retrieve current EPROM page if entire page not overwritten. */
                    if (to_send < CY7C637XX.EEPROMPAGESIZE)
                    {
                        received = CY7C637XX.EEPROMPAGESIZE;
                        error = ReadEepromUserSpace(
                            dongle, P1, P2, offset, page, ref received);

                        /* Skip size check if IOCTL error */
                        if (error == SmartDnglError.UskOk)
                        {
                            if (received != CY7C637XX.EEPROMPAGESIZE)
                            {
                                error = SmartDnglError.UskErrEpromRead;
                            }
                        }
                    }

                    for (j = 0; j < to_send; j++)
                    {
                        page[j + (int)page0[0]] = buffer[j];
                    }

                    sent = to_send;
                }
                else if (!(i == (pages - 1)))
                {

                    /* Process all pages but the last page here.
                    *  Address an offset into first page,
                    *  then fetch and overwrite tail end of first page.
                    */
                    if ((firstPage) && (page0[0] > 0))
                    {

                        received = CY7C637XX.EEPROMPAGESIZE;
                        error = ReadEepromUserSpace(
                            dongle, P1, P2, offset, page, ref received);

                        /* Skip size check if IOCTL error */
                        if (error == SmartDnglError.UskOk)
                        {
                            if (received != CY7C637XX.EEPROMPAGESIZE)
                            {
                                error = SmartDnglError.UskErrEpromRead;
                            }
                        }

                        for (j = 0; j < (CY7C637XX.EEPROMPAGESIZE - (int)page0[0]); j++)
                        {
                            page[j + (int)page0[0]] = buffer[j];
                        }

                        sent = CY7C637XX.EEPROMPAGESIZE - (int)page0[0];
                    }
                    else
                    {

                        /* No offset, simply overwrite entire buffer. */
                        for (j = 0; j < CY7C637XX.EEPROMPAGESIZE; j++)
                        {
                            page[j] = buffer[j + size];
                        }

                        sent = CY7C637XX.EEPROMPAGESIZE;
                    }
                }
                else
                {

                    /* Last page.
                    *
                    * Retrieve current EPROM page if
                    *  entire page not to be overwritten.
                    */
                    if (to_send < CY7C637XX.EEPROMPAGESIZE)
                    {

                        received = CY7C637XX.EEPROMPAGESIZE;
                        error = ReadEepromUserSpace(
                            dongle, P1, P2, offset, page, ref received);

                        /* Skip size check if IOCTL error */
                        if (error == SmartDnglError.UskOk)
                        {
                            if (received != CY7C637XX.EEPROMPAGESIZE)
                            {
                                error = SmartDnglError.UskErrEpromRead;
                            }
                        }
                    }

                    for (j = 0; j < to_send; j++)
                    {
                        page[j] = buffer[j + size];
                    }

                    sent = to_send;
                }

                /* Return on error */
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                /* Must obtain new key for each write transaction if strict
                *  access if required by SmartDongle.
                *  Extended functionality must be supported in firmware.
                */
                if ((dongle.options & CY7C637XX.QTS_OPTION_STRICT_ONLY) != 0)
                {
                    error = EnableUsk(dongle, P1, P2);
                }
                else if (firstPage)
                {
                    error = EnableUsk(dongle, P1, P2);
                }

                /* Return on error */
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                for (retries = 0; retries < CY7C637XX.QTS_WRITE_RETRIES; retries++)
                {

                    /* Write the page */
                    error = IOCTL_Command.writeEpromPage(dongle, offset, page);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }

                    error = IOCTL_Command.waitForBusy(
                        dongle.device, CY7C637XX.QTSBUSYWAIT);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }

                    /* Any errors writing page to EEPROM? */
                    error = IOCTL_Command.readPageFlags(dongle.device, out flags);
                    if (error != SmartDnglError.UskOk)
                    {
                        break;
                    }

                    /* Don't retry if an address range error */
                    if ((flags &= CY7C637XX.QTS_WRITE_ERROR_MASK) > 0)
                    {
                        error = SmartDnglError.UskErrEepromWrite;

                        if ((flags &
                            (CY7C637XX.QTSWRDENY | CY7C637XX.QTSWREEPROMERROR)) > 0)
                        {

                            /* Ignore error */
                            IOCTL_Command.waitForTwc(
                                dongle.device, CY7C637XX.QTSTWCWAIT);
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    /* Wait here for Twc (Time Write cycle) to reset */
                    error =
                        IOCTL_Command.waitForTwc(dongle.device, CY7C637XX.QTSTWCWAIT);

                    break;
                }

                /* Return on error */
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                to_send -= sent;
                size += sent;

                firstPage = false;
            }

            return SmartDnglError.UskOk;
        }

        /* Enable Dongle by stepping through the required States, using Keys. */
        public static SmartDnglError EnableUsk(
            Device dongle, UInt64 P1, UInt64 P2)
        {
            UInt64 challenge;
            UInt64 X;
            byte state;
            SmartDnglError error;
            string sn = new string('0', SmartDongle.CP2_SERIALNUM_LENGTH); ;
            int i;
            int numLoops;

            IOCTL_Command.getSerialNumber(dongle.device, ref sn);
            numLoops = getLoopCount(sn);

            /* Get fixed options */
            error = IOCTL_Command.getOpt(ref dongle);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            IOCTL_Command.getDriverVersion(ref dongle);

            /*
                Reset key state machine here for all but >=v1.50 SmartDongles
            */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) == 0)
            {

                error = IOCTL_Command.resetKeyState(dongle.device);
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                error = IOCTL_Command.waitForKeyState(
                    dongle.device, CY7C637XX.QTSINITIAL, CY7C637XX.QTSSTATEWAIT);
                if (error != SmartDnglError.UskOk)
                {

                    /* Was there a fatal EPROM read error?
                    *  Ignore readPageFlags IOCTL error
                    */
                    if (IOCTL_Command.readPageFlags(dongle.device, out state) ==
                        SmartDnglError.UskOk)
                    {

                        if ((state & CY7C637XX.QTS_READ_ERROR_MASK) > 0)
                        {
                            return SmartDnglError.UskErrEpromRead;
                        }
                    }

                    return error;
                }
            }

            /* Request the dongle create challenge. */
            error = IOCTL_Command.requestHostKey(dongle.device);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* Dongle's internally generated number to challenge host ready? */
            error = IOCTL_Command.waitForKeyState(
                dongle.device, CY7C637XX.QTSREPLY, CY7C637XX.QTSSTATEWAIT);
            if (error != SmartDnglError.UskOk)
            {

                /* Was there a fatal EPROM read error?
                *  Ignore readPageFlags IOCTL error here
                */
                if (IOCTL_Command.readPageFlags(dongle.device, out state) ==
                    SmartDnglError.UskOk)
                {

                    if ((state & CY7C637XX.QTS_READ_ERROR_MASK) > 0)
                    {
                        return SmartDnglError.UskErrEpromRead;
                    }
                }

                return error;
            }

            /* Get the SmartDongle's challenge. */
            error = IOCTL_Command.getKey(dongle.device, out challenge);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            X = challenge;
            for (i = 0; i < numLoops; i++) {
                X = Multiply(X, P1);
                X = Add(X, P2);
            }
            /* Reply to challenge */
            error = IOCTL_Command.sendKey(ref dongle, X);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* When key state sequences back to 0 (QTSINITIAL),
            *  the host response has been verified.
            *  The security state should be 1 (QTSENABLED) if the
            *  host reply is correct.
            */
            error = IOCTL_Command.waitForKeyState(
                dongle.device, CY7C637XX.QTSINITIAL, CY7C637XX.QTSSTATEWAIT);
            if (error != SmartDnglError.UskOk)
            {

                /* Was there a fatal EPROM read error?
                *  Ignore readPageFlags IOCTL error here
                */
                if (IOCTL_Command.readPageFlags(dongle.device, out state) ==
                    SmartDnglError.UskOk)
                {

                    if ((state & CY7C637XX.QTS_READ_ERROR_MASK) > 0)
                    {
                        return SmartDnglError.UskErrEpromRead;
                    }
                }

                return error;
            }

            /* Is the host verified, dongle ENABLED? */
            error = IOCTL_Command.readSecurityState(dongle.device, out state);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }
            if (state != CY7C637XX.QTSENABLED)
            {
                return SmartDnglError.UskErrHostValidate;
            }

            /* Force a v1.50 or greater SmartDongle to identify itself. */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) != 0)
            {
                error = ValidateUsk(dongle, P1, P2);
                if (error != SmartDnglError.UskOk)
                {
                    IOCTL_Command.resetKeyState(dongle.device);
                }
            }

            return error;
        }

        public static int getLoopCount(string serial) {
            int loops = 1;
            char serialID = serial.ToCharArray()[0];
            switch (serialID) {
                case '0':
                    loops = 1;
                    break;
                case '1':
                    loops = 3;
                    break;
                case '2':
                    loops = 5;
                    break;
                case '3':
                    loops = 7;
                    break;
                case '4':
                    loops = 11;
                    break;
                case '5':
                    loops = 13;
                    break;
                case '6':
                    loops = 17;
                    break;
                case '7':
                    loops = 19;
                    break;
                case '8':
                    loops = 23;
                    break;
                case '9':
                    loops = 29;
                    break;
                case 'A':
                    loops = 31;
                    break;
                case 'B':
                    loops = 37;
                    break;
                case 'C':
                    loops = 41;
                    break;
                case 'D':
                    loops = 43;
                    break;
                case 'E':
                    loops = 47;
                    break;
                case 'F':
                    loops = 53;
                    break;
                default:
                    loops = 1;
                    break;
            }
            return loops;
        }

        /* Key state and Security state to 0 locks SmartDongle */
        public static SmartDnglError DisableUsk(IntPtr device)
        {
            return IOCTL_Command.resetKeyState(device);
        }

        /* For SmartDongle v1.50.
        *  force a SmartDongle to reply to a host challenge.
        *  Useful if a user does not write any secrets to SmartDongle's user
        *  memory and simply checks for presence of a SmartDongle.
        *
        *  SmartDongle must be enabled to support a host challenge.
        */
        public static SmartDnglError ValidateUsk(
            Device dongle, UInt64 P1, UInt64 P2)
        {
            UInt64 challenge, reply, expected_reply;
            byte flags;
            SmartDnglError error;
            string sn = new string('0', SmartDongle.CP2_SERIALNUM_LENGTH); ;
            int i;
            int numLoops;

            /* For v1.50 or greater SmartDongles only */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) == 0)
            {
                return SmartDnglError.UskErrRequiresNewerSmartDongleVersion;
            }

            /* Make a 64 bit challenge */
            Random rand = new Random(getEpochTime());
            challenge = (UInt64)rand.Next();
            challenge |= (UInt64)rand.Next() << 16;
            challenge |= (UInt64)rand.Next() << 32;
            challenge |= (UInt64)rand.Next() << 48;

            /* High bit must be set */
            challenge |= 0x8000000000000000;

            IOCTL_Command.getSerialNumber(dongle.device, ref sn);
            numLoops = getLoopCount(sn);

            /* Calculate expected reply from SmartDongle */
            expected_reply = challenge;
            for (i = 0; i < numLoops; i++) {
                expected_reply = Multiply(expected_reply, P1);
                expected_reply = Add(expected_reply, P2);
            }

            /* Send challenge.
            *  Assuming key state was set to 0 at termination of
            *  SmartDongle enable sequence.
            */
            error = IOCTL_Command.sendChallenge(dongle, challenge);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* Wait for SmartDongle's reply to host challenge */
            error = IOCTL_Command.waitForKeyState(
                dongle.device, CY7C637XX.QTSREPLY, CY7C637XX.QTSSTATEWAIT);
            if (error != SmartDnglError.UskOk)
            {

                /* Was there a fatal EPROM read error?
                *  Ignore readPageFlags IOCTL error here
                */
                if (IOCTL_Command.readPageFlags(dongle.device, out flags) ==
                    SmartDnglError.UskOk)
                {

                    if ((flags & CY7C637XX.QTS_READ_ERROR_MASK) > 0)
                    {
                        return SmartDnglError.UskErrEpromRead;
                    }
                }

                return error;
            }

            /* Get the SmartDongle's reply to challenge. */
            error = IOCTL_Command.getKey(dongle.device, out reply);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            if (expected_reply != reply)
            {
                return SmartDnglError.UskErrUskValidate;
            }

            return SmartDnglError.UskOk;
        }

        /* Reset SmartDongle */
        public static SmartDnglError Reset(IntPtr device)
        {
            return IOCTL_Command.softReset(device);
        }

        /* Output port control for LED */
        public static SmartDnglError Green(IntPtr device)
        {
            return IOCTL_Command.writePort(device, CY7C637XX.QTSLEDPORT, CY7C637XX.QTSGREEN);
        }

        public static SmartDnglError Red(IntPtr device)
        {
            return IOCTL_Command.writePort(device, CY7C637XX.QTSLEDPORT, CY7C637XX.QTSRED);
        }

        public static SmartDnglError Off(IntPtr device)
        {
            return IOCTL_Command.writePort(device, CY7C637XX.QTSLEDPORT, CY7C637XX.QTSLEDOFF);
        }

        Command() { }
    }

    private class IOCTL_Command
    {
        /* Send a challenge to SmartDongle. */
        public static SmartDnglError sendChallenge(Device dongle, UInt64 challenge)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;
            IntPtr buffer;


            /* For v1.50 or greater SmartDongles only */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) == 0)
            {
                return SmartDnglError.UskErrRequiresNewerSmartDongleVersion;
            }

            buffer = Marshal.AllocHGlobal(Marshal.SizeOf(challenge));
            Marshal.WriteInt64(buffer, (Int64)challenge);

            /* Wait for task_loop processing to finish */
            error = waitForBusy(dongle.device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* Driver does not queue requests, retry on fail */
            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_SEND_KEY(),
                        buffer,
                        CY7C637XX.SECRETS_KEY_LEN,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            Marshal.FreeHGlobal(buffer);

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlSendKey;
            }

            return SmartDnglError.UskOk;
        }

        /* Send a Reply calculated from SmartDongle's Challenge. */
        public static SmartDnglError sendKey(ref Device dongle, UInt64 reply)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;
            byte[] buffer = new byte[CY7C637XX.SECRETS_KEY_LEN];
            UInt64 Y;


            /* If signed read/write is supported */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) != 0)
            {
                dongle.signature = CY7C637XX.SignatureBytesFromKey(reply);

                /* Use 32 bit signature if using >=2.3.x drivers */
                if (((dongle.major == 2) && (dongle.minor >= 3)) ||
                    (dongle.major >= 3))
                {

                    reply &= 0x00000000FFFFFFFF;
                }
                else
                {

                    /* Strict access.
                    *  Requires SmartDongle driver version 2.3.0 or greater.
                    */
                    if ((dongle.options & CY7C637XX.QTS_OPTION_STRICT_ONLY) != 0)
                    {
                        return SmartDnglError.UskErrRequiresNewerDriver;
                    }

                    /* Make a 64 bit challenge */
                    Random rand = new Random(getEpochTime());
                    Y = (UInt64)rand.Next();
                    Y <<= 52;
                    reply ^= Y;
                }
            }

            /* Must convert UInt64 into byte array since C# expects a
            *  value in little endian form (if marshaling to IntPtr).
            */
            buffer = BitConverter.GetBytes(reply);

            /* Wait for task_loop processing to finish */
            error = waitForBusy(dongle.device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            /* Driver does not queue requests, retry on fail */
            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_SEND_KEY(),
                        buffer,
                        CY7C637XX.SECRETS_KEY_LEN,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlSendKey;
            }

            return SmartDnglError.UskOk;
        }

        /* Get the SmartDongle's Challenge. */
        public static SmartDnglError getKey(IntPtr device, out UInt64 challenge)
        {
            int i;
            SmartDnglError error = SmartDnglError.UskOk;
            int bytesReturned = 0;
            bool status = false;
            IntPtr b1 = Marshal.AllocHGlobal(CY7C637XX.SECRETS_KEY_LEN);
            byte[] b2 = new byte[CY7C637XX.SECRETS_KEY_LEN];


            /* Wait for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                challenge = 0;
                return error;
            }

            /* Driver does not queue requests, retry on fail */
            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_KEY(),
                        IntPtr.Zero,
                        0,
                        b1,
                        CY7C637XX.SECRETS_KEY_LEN,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if ((status == false) || (bytesReturned < CY7C637XX.SECRETS_KEY_LEN))
            {
                challenge = 0;
                error = SmartDnglError.UskErrIoctlGetKey;
            }
            else
            {

                /* Must marshal bytes into byte array since C# expects a
                *  value in little endian form (if marshaling from IntPtr).
                */
                for (i = 0; i < CY7C637XX.SECRETS_KEY_LEN; i++)
                {
                    b2[i] = Marshal.ReadByte(b1, i);
                }

                challenge = BitConverter.ToUInt64(b2, 0);
            }

            Marshal.FreeHGlobal(b1);
            return error;
        }

        /* Write to Port 0-3, must be set by firmware as an OUT port. */
        public static SmartDnglError writePort(
            IntPtr device, byte port, byte color)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] in_buffer = new byte[2];


            in_buffer[0] = port;
            in_buffer[1] = color;

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_WRITE_PORT(),
                        in_buffer,
                        2,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlQtsDongleSendPort;
            }

            return SmartDnglError.UskOk;
        }

        /* Get the SmartDongle's security state */
        public static SmartDnglError readSecurityState(
            IntPtr device, out byte state)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_SECURITY_STATE(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            state = out_buffer[0];

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetSecurityState;
            }

            return SmartDnglError.UskOk;
        }

        /* Reset the SmartDongle's security state to 0  (Disabled) */
        public static SmartDnglError resetSecurityState(IntPtr device)
        {
            int i;
            int bytesReturned = 0;
            bool status = true;


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_RESET_SECURITY_STATE(),
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlResetSecurityState;
            }

            return SmartDnglError.UskOk;
        }

        /* Reset the SmartDongle's key state to 0 (QTSINITIAL) */
        public static SmartDnglError resetKeyState(IntPtr device)
        {
            int i;
            SmartDnglError error;
            int bytesReturned = 0;
            bool status = true;


            /* Wait for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_RESET_KEY_STATE(),
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlResetKeyState;
            }

            return SmartDnglError.UskOk;
        }

        /* Request the SmartDongle to create a Challenge. */
        public static SmartDnglError requestHostKey(IntPtr device)
        {
            int i;
            SmartDnglError error;
            int bytesReturned = 0;
            bool status = true;


            /* Wait for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_SEND_HOST_VERIFY_REQUEST(),
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlSendHostVerifyRequest;
            }

            return SmartDnglError.UskOk;
        }

        /* Send a page of data out to the SmartDongle's write buffer.
        *  Use readPageFlags to poll the status of the EEPROM write buffer.
        */
        public static SmartDnglError writeEpromPage(Device dongle, UInt16 address, byte[] page)
        {
            int i, j;
            int bytesReturned = 0;
            bool status = true;
            int in_size;
            byte[] address_bytes = new byte[CY7C637XX.ADDR_LENGTH];
            byte[] buffer = new byte[2 * CY7C637XX.EEPROMPAGESIZE];


            /* UInt16 address to big endian byte array */
            address_bytes = BitConverter.GetBytes(address);


            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) != 0)
            {

                /* 32 bit Signature */
                for (i = 0, j = 0; i < CY7C637XX.SIG_LENGTH; i++, j++)
                {
                    buffer[j] = dongle.signature[i];
                }

                /* EEPROM address */
                for (i = 0; i < CY7C637XX.ADDR_LENGTH; i++, j++)
                {
                    buffer[j] = address_bytes[i];
                }

                /* 16 bytes of page data */
                for (i = 0; i < CY7C637XX.EEPROMPAGESIZE; i++, j++)
                {
                    buffer[j] = page[i];
                }

                in_size =
                    CY7C637XX.SIG_LENGTH +
                    CY7C637XX.ADDR_LENGTH +
                    CY7C637XX.EEPROMPAGESIZE;
            }
            else
            {

                /* EEPROM address + 16 bytes of page data */
                for (i = 0, j = 0; i < CY7C637XX.ADDR_LENGTH; i++, j++)
                {
                    buffer[j] = address_bytes[i];
                }

                for (i = 0; i < CY7C637XX.EEPROMPAGESIZE; i++, j++)
                {
                    buffer[j] = page[i];
                }

                in_size = CY7C637XX.ADDR_LENGTH + CY7C637XX.EEPROMPAGESIZE;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_WRITE_EEPROM(),
                        buffer,
                        in_size,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlWriteEeprom;
            }

            return SmartDnglError.UskOk;
        }

        /* Send the address of a page in EEPROM.
        *  Will be read on the next readEpromPrefetch.
        */
        public static SmartDnglError requestEpromPrefetch(
            IntPtr device, UInt16 address)
        {
            int i;
            int bytesReturned = 0;
            byte[] address_bytes = new byte[CY7C637XX.ADDR_LENGTH];
            bool status = true;


            /* UInt16 address to big endian byte array */
            address_bytes = BitConverter.GetBytes(address);


            /* Driver does not queue requests, retry on fail */
            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_REQUEST_PAGE(),
                        address_bytes,
                        CY7C637XX.ADDR_LENGTH,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlRequestPage;
            }

            return SmartDnglError.UskOk;
        }

        /* Get the data currently in the SmartDongle's EEPROM page read buffer.
        *  Use readPageFlags to poll the status of the EEPROM read/write buffer.
        *
        *  Note: Data in, data out in USB parlance is relative to the host,
        *  Data direction in C# is relative to the called function.
        */
        public static SmartDnglError readEpromPrefetch(
            Device dongle, byte[] buffer, out int size)
        {
            int i;
            int bytesReturned = 0;
            bool status = true;


            /* If the SmartDongle supports "signed" read/write packets. */
            if ((dongle.options & CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT) != 0)
            {

                /* If using the 2.3.0 driver.
                *  Send the 32 bit "signature" in the output buffer.
                */
                if (((dongle.major == 2) && (dongle.minor >= 3)) ||
                    (dongle.major >= 3))
                {

                    byte[] in_buffer = new byte[CY7C637XX.QTSPACKETSIZE];

                    /* 32 bit Signature */
                    in_buffer[0] = dongle.signature[2];
                    in_buffer[1] = dongle.signature[3];
                    in_buffer[2] = dongle.signature[0];
                    in_buffer[3] = dongle.signature[1];

                    for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
                    {
                        status =
                            Win32Methods.DeviceIoControl(
                                dongle.device,
                                IOCTL_QTSDONGLE_READ_EEPROM(),
                                in_buffer,
                                CY7C637XX.QTSPACKETSIZE,
                                buffer,
                                8 * CY7C637XX.EEPROMPAGESIZE,
                                out bytesReturned,
                                IntPtr.Zero);

                        if (status)
                        {
                            break;
                        }

                        Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
                    }
                }
                else
                {

                    /* 12 bit signature */

                    int out_size = (int)BitConverter.ToUInt16(dongle.signature, 0);
                    out_size = (out_size & 0xfff0) >> 4;

                    /* Fix the out size value.
                    *  A key value below the size of the data to be read will
                    *  returns an error from the bus driver.
                    */
                    if (out_size <
                        (CY7C637XX.CP2_PAGE_PREFETCH_ENABLED *
                        CY7C637XX.EEPROMPAGESIZE))
                    {
                        out_size |= 0x100;
                    }

                    for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
                    {
                        status =
                            Win32Methods.DeviceIoControl(
                                dongle.device,
                                IOCTL_QTSDONGLE_READ_EEPROM(),
                                IntPtr.Zero,
                                0,
                                buffer,
                                out_size,
                                out bytesReturned,
                                IntPtr.Zero);

                        if (status)
                        {
                            break;
                        }

                        Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
                    }
                }
            }
            else
            {

                for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
                {
                    status =
                        Win32Methods.DeviceIoControl(
                            dongle.device,
                            IOCTL_QTSDONGLE_READ_EEPROM(),
                            IntPtr.Zero,
                            0,
                            buffer,
                            8 * CY7C637XX.EEPROMPAGESIZE,
                            out bytesReturned,
                            IntPtr.Zero);

                    if (status)
                    {
                        break;
                    }

                    Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
                }
            }

            if (status == false)
            {
                size = 0;
                return SmartDnglError.UskErrIoctlReadEeprom;
            }

            size = bytesReturned;

            return SmartDnglError.UskOk;
        }

        /* Reboot SmartDongle, ignore return error.*/
        public static SmartDnglError softReset(IntPtr device)
        {
            int bytesReturned;


            Win32Methods.DeviceIoControl(
                device,
                IOCTL_QTSDONGLE_SOFT_RESET(),
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                out bytesReturned,
                IntPtr.Zero);

            return SmartDnglError.UskOk;
        }

        /* For SmartDongle >=v1.50 using >= v2.3 driver only.
        *  This will configure the SmartDongle for Strict Access.
        *  A new key and 32 bit signatures are required for each user
        *  read/write transaction against user memory.
        *
        *  Caution: This configuration is permanent.
        *           SmartDongle must be sent back to the manufacturer to
        *           be reset to use the older drivers (<= v2.2.0).
        */
        public static SmartDnglError setStrictOnly(ref Device dongle)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;


            /* Prevent customer from burning memory with repeated requests. */
            if ((dongle.options & CY7C637XX.QTS_OPTION_STRICT_ONLY) != 0)
            {
                return SmartDnglError.UskOk;
            }

            /* Requires >=2.3.x drivers */
            if (!(((dongle.major == 2) && (dongle.minor >= 3)) ||
                (dongle.major >= 3)))
            {
                return SmartDnglError.UskErrRequiresNewerDriver;
            }

            /* Wait for task_loop processing to finish */
            error = waitForBusy(dongle.device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_SET_STRICT(),
                        IntPtr.Zero,
                        0,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlSetStrict;
            }

            /* Update SmartDongle options */
            dongle.options |= CY7C637XX.QTS_OPTION_STRICT_ONLY;

            return SmartDnglError.UskOk;
        }

        /* Read SmartDongle's current eprom bits.
        *
        *  This is the encryption for the data on the SmartDongle memory that
        *  protects the memory from a physical attack only.
        *
        *  Modes:   0x00 QTSSTRONGCRYPT    AES Encryption
        *           0x01 QTSCRYPTDISABLED  No Encryption
        *           0x02 QTSFASTCRYPT      Fast, lightwieght encryption
        */
        public static SmartDnglError getMode(IntPtr device, out byte mode)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            /* Wait for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                mode = 0x00;
                return error;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_MODE(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            mode = out_buffer[0];

            /* Mask out extraneous bits */
            mode &=
                UserOption.QTS_OPTION_ON_CHIP_EPROM_PROTECT_DISABLE |
                UserOption.QTS_OPTION_ON_CHIP_EPROM_PROTECT_FAST;

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetMode;
            }

            return SmartDnglError.UskOk;
        }


        /* Set SmartDongle eprom mode bits.
        *
        *  This is the encryption for the data on the SmartDongle
        *  that protects the EEPROM memory from a physical attack.
        *
        *  Modes:   0x00 QTSSTRONGCRYPT    AES Encryption
        *           0x01 QTSCRYPTDISABLED  No Encryption
        *           0x02 QTSFASTCRYPT      Fast, lightweight encryption.
        *
        *  All other bits are "don't care"
        */
        public static SmartDnglError setMode(IntPtr device, byte mode)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;
            byte[] in_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            /*  for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            in_buffer[0] = mode;

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_SET_MODE(),
                        in_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        IntPtr.Zero,
                        0,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlSetMode;
            }

            return SmartDnglError.UskOk;
        }

        /* Read SmartDongle's extended functionality bits.
        *  Mapped with eprom error bits.
        */
        public static SmartDnglError getOpt(ref Device dongle)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];
            byte fixedOptions;


            /* clear options */
            dongle.options = 0;

            /* Wait for task_loop processing to finish */
            error = waitForBusy(dongle.device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_GET_EEPROM_ERROR_FLAG(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            fixedOptions = out_buffer[0];

            /*
                Map firmware flags to options flags
            */

            if ((fixedOptions & CY7C637XX.QTS_FIRMWARE_EXTENDED_SUPPORT2) > 0)
            {
                dongle.options |= CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT2;
            }

            if ((fixedOptions & CY7C637XX.QTS_FIRMWARE_EXTENDED_SUPPORT) > 0)
            {
                dongle.options |= CY7C637XX.QTS_OPTION_EXTENDED_SUPPORT;
            }

            if ((fixedOptions & CY7C637XX.QTS_FIRMWARE_STRICT_ONLY) > 0)
            {
                dongle.options |= CY7C637XX.QTS_OPTION_STRICT_ONLY;
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetEepromErrorFlag;
            }

            return SmartDnglError.UskOk;
        }

        /* Some functionality only supported in new, unsigned driver. */
        public static SmartDnglError getDriverVersion(ref Device dongle)
        {
            int i;
            int bytesReturned;
            bool status = true;
            byte[] out_buffer = new byte[8 * CY7C637XX.EEPROMPAGESIZE];


            /* Driver does not queue requests, retry on fail */
            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        dongle.device,
                        IOCTL_QTSDONGLE_GET_DRIVER_REVISION(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        8 * CY7C637XX.EEPROMPAGESIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetDriverRevision;
            }

            /* Copy over version byte fields. */
            dongle.minor = out_buffer[2];
            dongle.major = out_buffer[3];

            return SmartDnglError.UskOk;
        }

        /* Get SmartDongle serial number. */
        public static SmartDnglError getSerialNumber(
            IntPtr device, ref String serialNumber)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = true;
            byte[] in_buffer = new byte[CY7C637XX.QTSPACKETSIZE];
            byte[] out_buffer = new byte[8 * CY7C637XX.EEPROMPAGESIZE];
            char[] sn = new char[CP2_SERIALNUM_LENGTH];


            /* Wait for task_loop processing to finish */
            error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
            if (error != SmartDnglError.UskOk)
            {
                return error;
            }

            in_buffer[0] = 0x03; /* Index for serial number fixed at 0x03 */

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_STRING_DESCRIPTOR(),
                        in_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out_buffer,
                        8 * CY7C637XX.EEPROMPAGESIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetStringDescriptor;
            }

            /* Returned USB descriptor string is a predefined length. */
            if (out_buffer[0] != CY7C637XX.CP2_SER_NUMB_STRING_LENGTH)
            {
                return SmartDnglError.UskErrDataFormat;
            }

            for (i = 0; i < CP2_SERIALNUM_LENGTH; i++)
            {
                sn[i] = (char)out_buffer[2 + (i * 2)];
            }

            serialNumber = new string(sn);

            return SmartDnglError.UskOk;
        }

        /* Wait for EPROM access status.
        *  Will indicate that requested EPROM pages are ready or
        *  an error has occurred.
        */
        public static SmartDnglError waitForPageRead(
            IntPtr device, int period, out byte flags)
        {
            byte data = new byte();
            SmartDnglError error;
            int trys = 100;
            int i;

            if (period <= 0)
            {
                flags = 0;
                return SmartDnglError.UskErrUnknown;
            }

            for (i = 0; i < trys; i++)
            {

                error = readPageFlags(device, out data);
                if (error != SmartDnglError.UskOk)
                {
                    flags = 0;
                    return error;
                }

                /* Any read-related bit set mean ready */
                if ((data & CY7C637XX.QTS_READ_PAGE_READY_MASK) > 0)
                {
                    flags = data;
                    return SmartDnglError.UskOk;
                }

                Thread.Sleep(period / trys);
            }

            flags = 0;

            return SmartDnglError.UskErrEepromReadTimeout;
        }

        /* Get SmartDongle's EPROM access status byte. */
        public static SmartDnglError readPageFlags(IntPtr device, out byte flags)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_PAGE_FLAGS(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state flags will be in first byte */
            flags = out_buffer[0];

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetPageFlags;
            }

            return SmartDnglError.UskOk;
        }

        /* Wait for a particular challenge/request state.
        *
        *  Note: Removed the host challenge as in v1.41, so states are:
        *
        *  initial state:  0
        *  challege state: v1.41:0x02  v1.42:0x01
        */
        public static SmartDnglError waitForKeyState(
            IntPtr device, byte state, int period)
        {
            byte data = new byte();
            bool key_state, required_state;
            SmartDnglError error;
            int trys = 100;
            int i;
            const byte USK_KEY_STATE_MASK = 0x03;


            /* boolean */
            required_state = ((state & USK_KEY_STATE_MASK) > 0);

            if (period <= 0)
            {
                return SmartDnglError.UskErrUnknown;
            }

            for (i = 0; i < trys; i++)
            {

                error = readKeyState(device, out data);
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                /* boolean */
                key_state = ((data & USK_KEY_STATE_MASK) > 0);

                if (required_state == key_state)
                {
                    return SmartDnglError.UskOk;
                }

                Thread.Sleep(period / trys);
            }

            return SmartDnglError.UskErrKeyStateTimeout;
        }

        static SmartDnglError readKeyState(IntPtr device, out byte state)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_KEY_STATE(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            state = out_buffer[0];

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetKeyState;
            }

            return SmartDnglError.UskOk;
        }

        /* Wait Time Write Cycle flag to reset. */
        public static SmartDnglError waitForTwc(IntPtr device, int period)
        {
            byte data = new byte();
            SmartDnglError error;
            int trys = 100;
            int i;

            if (period <= 0)
            {
                return SmartDnglError.UskErrUnknown;
            }

            for (i = 0; i < trys; i++)
            {

                error = readTwcFlag(device, out data);
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                if (data == CY7C637XX.QTSREADY)
                {
                    return SmartDnglError.UskOk;
                }

                Thread.Sleep(period / trys);
            }

            return SmartDnglError.UskErrTwcResetTimeout;
        }

        static SmartDnglError readTwcFlag(IntPtr device, out byte state)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_TWC_FLAG(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            state = out_buffer[0];

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetTwcFlag;
            }

            return SmartDnglError.UskOk;
        }

        /* Wait for busy flag to be reset,
        *  Backwards compatible with <=1.40 Firmware
        */
        public static SmartDnglError waitForBusy(IntPtr device, int period)
        {
            byte data = new byte();
            SmartDnglError error;
            int trys = 100;
            int i;


            if (period <= 0)
            {
                return SmartDnglError.UskErrUnknown;
            }

            for (i = 0; i < trys; i++)
            {

                error = readBusyFlag(device, out data);
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }

                if (data == CY7C637XX.QTSREADY)
                {
                    return SmartDnglError.UskOk;
                }

                Thread.Sleep(period / trys);
            }

            return SmartDnglError.UskErrBusyTimeout;
        }

        static SmartDnglError readBusyFlag(IntPtr device, out byte state)
        {
            int i;
            int bytesReturned;
            bool status = false;
            byte[] out_buffer = new byte[CY7C637XX.QTSPACKETSIZE];


            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_BUSY_FLAG(),
                        IntPtr.Zero,
                        0,
                        out_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            /* If status is true, state will be in first byte */
            state = out_buffer[0];

            if (status == false)
            {
                return SmartDnglError.UskErrIoctlGetBusyFlag;
            }

            return SmartDnglError.UskOk;
        }

        /* Attempt an innocuous command.
        *
        *  Return with status within dwMilliseconds.
        *  Time argument for device mutex access, set to zero if
        *  simply querying for "is this SmartDongle present?".
        */
        public static SmartDnglError isDeviceReady(IntPtr device, int dwMilliseconds)
        {
            int i;
            SmartDnglError error;
            int bytesReturned;
            bool status = true;
            byte[] in_buffer = new byte[CY7C637XX.QTSPACKETSIZE];
            byte[] out_buffer = new byte[8 * CY7C637XX.EEPROMPAGESIZE];


            if (dwMilliseconds > 0)
            {
                /* Wait for task_loop processing to finish */
                error = waitForBusy(device, CY7C637XX.QTSBUSYWAIT);
                if (error != SmartDnglError.UskOk)
                {
                    return error;
                }
            }

            in_buffer[0] = 0x03; /* Index for serial number fixed at 0x03 */

            for (i = 0; i < CY7C637XX.QTS_IOCTL_RETRYS; i++)
            {
                status =
                    Win32Methods.DeviceIoControl(
                        device,
                        IOCTL_QTSDONGLE_GET_STRING_DESCRIPTOR(),
                        in_buffer,
                        CY7C637XX.QTSPACKETSIZE,
                        out_buffer,
                        8 * CY7C637XX.EEPROMPAGESIZE,
                        out bytesReturned,
                        IntPtr.Zero);

                if (status)
                {
                    break;
                }

                Thread.Sleep(CY7C637XX.QTS_IOCTL_FAIL_WAIT);
            }

            if (status == false)
            {
                return SmartDnglError.UskErrNoDevice;
            }

            /* USB descriptor string is a predetermined  length. */
            if ((out_buffer[0] != CY7C637XX.CP2_SER_NUMB_STRING_LENGTH) ||
                 (out_buffer[1] != CY7C637XX.USB_STRING_DESCRIPTOR_TYPE))
            {
                return SmartDnglError.UskErrNoDevice;
            }

            return SmartDnglError.UskOk;
        }

        /* From winioctl.h */
        const UInt32 FILE_DEVICE_UNKNOWN = 0x00000022;
        const UInt32 METHOD_BUFFERED = 0;
        const UInt32 FILE_ANY_ACCESS = 0;

        private static UInt32 CTL_CODE(
            UInt32 DeviceType, UInt32 Function, UInt32 Method, UInt32 Access)
        {
            return (
                (DeviceType) << 16) |
                ((Access) << 14) |
                ((Function) << 2) |
                (Method);
        }

        /* From sd_user.h */
        private const UInt32 QTSDONGLE_IOCTL_INDEX = 0x0800;

        private static UInt32 IOCTL_QTSDONGLE_GET_DEVICE_DESCRIPTOR()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 1,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_CONFIG_DESCRIPTOR()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 2,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_RESET_PIPE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 6,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_DRIVER_REVISION()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 7,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_STRING_DESCRIPTOR()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 8,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SEND_KEY()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x0A,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_KEY()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x0B,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_WRITE_PORT()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x0C,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_READ_EEPROM()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x0E,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_WRITE_EEPROM()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x0F,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_SECURITY_STATE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x11,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_RESET_SECURITY_STATE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x12,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_KEY_STATE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x13,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_RESET_KEY_STATE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x14,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SEND_HOST_VERIFY_REQUEST()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x15,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_REQUEST_PAGE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x16,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_PAGE_FLAGS()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x17,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_TWC_FLAG()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x18,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_BUSY_FLAG()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x19,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_EEPROM_ERROR_FLAG()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x1A,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SOFT_RESET()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x1C,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SEND_PORT()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x1D,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_FLAGS()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x1E,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SET_MODE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x1F,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_GET_MODE()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x20,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        private static UInt32 IOCTL_QTSDONGLE_SET_STRICT()
        {
            return CTL_CODE(
                FILE_DEVICE_UNKNOWN,
                QTSDONGLE_IOCTL_INDEX + 0x22,
                METHOD_BUFFERED,
                FILE_ANY_ACCESS);
        }

        IOCTL_Command() { }
    }

    public enum SmartDnglError
    {
        /* From sd_uskerr.h */
        UskOk = 0,
        UskErrOpen,
        UskErrIsOpen,
        UskErrMem,
        UskErrEepromSize,
        UskErr5,
        UskErrSecurityStateTimeout,
        UskErrKeyStateTimeout,
        UskErrTwcResetTimeout,
        UskErrBusyTimeout,
        UskErrNotEnabled,
        UskErrEepromReadTimeout,
        UskErrEpromRead,
        UskErrEepromWrite,
        UskErrUskValidate,
        UskErrHostValidate,
        UskErrDisable,
        UskErrDataFormat,
        UskErrIoctlGetPipeInfo,
        UskErrIoctlGetDeviceDescriptor,
        UskErrIoctlGetConfigDescriptor,
        UskErrIoctlRegisterNotifyEvent,
        UskErrIoctlResetDevice,
        UskErrIoctlResetPipe,
        UskErrIoctlGetDriverRevision,
        UskErrIoctlGetStringDescriptor,
        UskErr26,
        UskErrIoctlSendKey,
        UskErrIoctlGetKey,
        UskErrIoctlWritePort,
        UskErrIoctlReadPort,
        UskErrIoctlReadEeprom,
        UskErrIoctlWriteEeprom,
        UskErrIoctlGetSecurityState,
        UskErrIoctlResetSecurityState,
        UskErrIoctlGetKeyState,
        UskErrIoctlResetKeyState,
        UskErrIoctlSendHostVerifyRequest,
        UskErrIoctlRequestPage,
        UskErrIoctlGetPageFlags,
        UskErrIoctlGetTwcFlag,
        UskErrIoctlGetBusyFlag,
        UskErrIoctlGetEepromErrorFlag,
        UskErr43,
        UskErr44,
        UskErrIoctlQtsDongleSendPort,
        UskErrIoctlQtsDongleGetFlags,
        UskErrIoctlSetMode,
        UskErrIoctlGetMode,
        UskErrNoDevice,

        UskErrBadArgument = 63,

        UskErr80 = 80,
        UskErrIoctlSetStrict,
        UskErrRequiresNewerDriver,
        UskErrRequiresNewerSmartDongleVersion,
        UskWarnModePageBoundary,
        UskWarnCryptPageBoundary,

        UskResetComeready,

        UskErrUserDataVerify,

        UskErrUnknown = 255
    }

    public static string GetErrorString(SmartDnglError error)
    {
        switch (error)
        {
            case SmartDnglError.UskOk:
                return "no errors";

            case SmartDnglError.UskErrOpen:
                return "Error: Can't open SmartDongle.";

            case SmartDnglError.UskErrIsOpen:
                return "Error: SmartDongle is already open.";

            /* Memory allocation error is a system error */
            case SmartDnglError.UskErrMem:
                return "Error: Memory allocation error.";

            case SmartDnglError.UskErrEepromSize:
                return "Error: Address offset or buffer exceeds SmartDongle memory size.";

            case SmartDnglError.UskErr5:
                return "Error deprecated or reserved, this is an incorrect error value";

            case SmartDnglError.UskErrSecurityStateTimeout:
                return "Error: Timeout waiting for security state change.";

            case SmartDnglError.UskErrKeyStateTimeout:
                return "Error: Timeout waiting for key state change";

            case SmartDnglError.UskErrTwcResetTimeout:
                return "Error: Timeout waiting for TWC to reset";

            case SmartDnglError.UskErrBusyTimeout:
                return "Error: Timeout waiting for job in firmware task loop to finish.";

            case SmartDnglError.UskErrNotEnabled:
                return "Error: Illegal command, SmartDongle needs to be unlocked first.";

            case SmartDnglError.UskErrEepromReadTimeout:
                return "Error: Timeout waiting for SmartDongle memory read.";

            case SmartDnglError.UskErrEpromRead:
                return "Error: SmartDongle memory Read error.";

            case SmartDnglError.UskErrEepromWrite:
                return "Error: SmartDongle memory Write error.";

            case SmartDnglError.UskErrUskValidate:
                return "Error: SmartDongle failed host challenge";

            case SmartDnglError.UskErrHostValidate:
                return "Error: Bad keys, could not unlock SmartDongle";

            case SmartDnglError.UskErrDisable:
                return "Error: Unable to lock SmartDongle.";

            case SmartDnglError.UskErrDataFormat:
                return "Error: Bad Serial Number Format read from SmartDongle.";


            /* IOCTL Errors */
            case SmartDnglError.UskErrIoctlGetPipeInfo:
                return "Error: IOCTL failed: IOCTL_GET_PIPE_INFO";

            case SmartDnglError.UskErrIoctlGetDeviceDescriptor:
                return "Error: IOCTL failed: IOCTL_GET_DEVICE_DESCRIPTOR";

            case SmartDnglError.UskErrIoctlGetConfigDescriptor:
                return "Error: IOCTL failed: IOCTL_GET_CONFIG_DESCRIPTOR";

            case SmartDnglError.UskErrIoctlRegisterNotifyEvent:
                return "Error: IOCTL failed: IOCTL_REGISTER_NOTIFY_EVENT";

            case SmartDnglError.UskErrIoctlResetDevice:
                return "Error: IOCTL failed: IOCTL_RESET_DEVICE";

            case SmartDnglError.UskErrIoctlResetPipe:
                return "Error: IOCTL failed: IOCTL_RESET_PIPE";

            case SmartDnglError.UskErrIoctlGetDriverRevision:
                return "Error: IOCTL failed: IOCTL_GET_DRIVER_REVISION";

            case SmartDnglError.UskErrIoctlGetStringDescriptor:
                return "Error: IOCTL failed: IOCTL_GET_STRING_DESCRIPTOR";

            case SmartDnglError.UskErr26:
                return "Error deprecated or reserved, this is an incorrect error value";

            case SmartDnglError.UskErrIoctlSendKey:
                return "Error: IOCTL failed: IOCTL_SEND_KEY";

            case SmartDnglError.UskErrIoctlGetKey:
                return "Error: IOCTL failed: IOCTL_GET_KEY";

            case SmartDnglError.UskErrIoctlWritePort:
                return "Error: IOCTL failed: IOCTL_WRITE_PORT";

            case SmartDnglError.UskErrIoctlReadPort:
                return "Error: IOCTL failed: IOCTL_READ_PORT";

            case SmartDnglError.UskErrIoctlReadEeprom:
                return "Error: IOCTL failed: IOCTL_READ_EEPROM";

            case SmartDnglError.UskErrIoctlWriteEeprom:
                return "Error: IOCTL failed: IOCTL_WRITE_EEPROM";

            case SmartDnglError.UskErrIoctlGetSecurityState:
                return "Error: IOCTL failed: IOCTL_GET_SECURITY_STATE";

            case SmartDnglError.UskErrIoctlResetSecurityState:
                return "Error: IOCTL failed: IOCTL_RESET_SECURITY_STATE";

            case SmartDnglError.UskErrIoctlGetKeyState:
                return "Error: IOCTL failed: IOCTL_GET_KEY_STATE";

            case SmartDnglError.UskErrIoctlResetKeyState:
                return "Error: IOCTL failed: IOCTL_RESET_KEY_STATE";

            case SmartDnglError.UskErrIoctlSendHostVerifyRequest:
                return "Error: IOCTL failed: IOCTL_SEND_HOST_VERIFY_REQUEST";

            case SmartDnglError.UskErrIoctlRequestPage:
                return "Error: IOCTL failed: IOCTL_REQUEST_PAGE";

            case SmartDnglError.UskErrIoctlGetPageFlags:
                return "Error: IOCTL failed: IOCTL_GET_PAGE_FLAGS";

            case SmartDnglError.UskErrIoctlGetTwcFlag:
                return "Error: IOCTL failed: IOCTL_GET_TWC_FLAG";

            case SmartDnglError.UskErrIoctlGetBusyFlag:
                return "Error: IOCTL failed: IOCTL_GET_BUSY_FLAG";

            case SmartDnglError.UskErrIoctlGetEepromErrorFlag:
                return "Error: IOCTL failed: IOCTL_GET_EEPROM_ERROR_FLAG";

            case SmartDnglError.UskErr43:
                return "Error deprecated or reserved, this is an incorrect error value";

            case SmartDnglError.UskErr44:
                return "Error deprecated or reserved, this is an incorrect error value";

            case SmartDnglError.UskErrIoctlQtsDongleSendPort:
                return "Error: IOCTL failed: IOCTL_QTSDONGLE_SEND_PORT";

            case SmartDnglError.UskErrIoctlQtsDongleGetFlags:
                return "Error: IOCTL failed: IOCTL_QTSDONGLE_GET_FLAGS";

            case SmartDnglError.UskErrIoctlSetMode:
                return "Error: IOCTL failed: IOCTL_SET_MODE";

            case SmartDnglError.UskErrIoctlGetMode:
                return "Error: IOCTL failed: IOCTL_GET_MODE";

            /* No device found.  */
            case SmartDnglError.UskErrNoDevice:
                return "Error: No SmartDongles found.";

            case SmartDnglError.UskErrBadArgument:
                return "Error: One or more function arguments incorrect.";


            case SmartDnglError.UskErr80:
                return "Error deprecated or reserved, this is an incorrect error value";

            /* For SmartDongle v1.50 */
            case SmartDnglError.UskErrIoctlSetStrict:
                return "Error: IOCTL failed: IOCTL_QTSDONGLE_SET_STRICT";

            case SmartDnglError.UskErrRequiresNewerDriver:
                return "Error: An attempt to use 2.3 driver functionality with an older driver";

            case SmartDnglError.UskErrRequiresNewerSmartDongleVersion:
                return "Error: An attempt to use new 1.50 SmartDongle functionality with an older dongle.";

            case SmartDnglError.UskWarnModePageBoundary:
                return "Warning: Data written to user memory after encryption mode change was not on a page boundary.";

            case SmartDnglError.UskWarnCryptPageBoundary:
                return "Warning: Encrypted data written to user memory was not on a page boundary.";

            case SmartDnglError.UskResetComeready:
                return "Warning: SmartDongle did not come ready after a reset.";

            case SmartDnglError.UskErrUserDataVerify:
                return "Error: Failed attempt to find SmartDongle with given keys and matching user data.";

            /* Unknown. */
            case SmartDnglError.UskErrUnknown:
                return "An unknown error occurred";

            default:
                return "An unknown error occurred";
        }
    }

} //end SmartDongle class
