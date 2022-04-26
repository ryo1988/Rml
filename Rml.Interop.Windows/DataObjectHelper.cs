using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;

namespace Rml.Interop.Windows;

internal static class Util
{
    private static readonly Encoding Encoding = Encoding.GetEncoding("Shift-jis");

    public static string ToSJisString<T>(ref this T value)
    where T : struct
    {
        return MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToSJisString();
    }

    public static string ToSJisString(this ReadOnlySpan<byte> value)
    {
        return Encoding.GetString(value);
    }
}

public static class DataObjectHelper
{
    private static class Windows
    {
        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern ILockBytes? CreateILockBytesOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease);


        [DllImport("OLE32.DLL", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern IStorage? StgCreateDocfileOnILockBytes(ILockBytes? plkbyt, uint grfMode, uint reserved);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000000B-0000-0000-C000-000000000046")]
        public interface IStorage
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            IStream CreateStream([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName,
                [In] [MarshalAs(UnmanagedType.U4)] int grfMode, [In] [MarshalAs(UnmanagedType.U4)] int reserved1,
                [In] [MarshalAs(UnmanagedType.U4)] int reserved2);

            [return: MarshalAs(UnmanagedType.Interface)]
            IStream OpenStream([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1,
                [In] [MarshalAs(UnmanagedType.U4)] int grfMode, [In] [MarshalAs(UnmanagedType.U4)] int reserved2);

            [return: MarshalAs(UnmanagedType.Interface)]
            IStorage CreateStorage([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName,
                [In] [MarshalAs(UnmanagedType.U4)] int grfMode, [In] [MarshalAs(UnmanagedType.U4)] int reserved1,
                [In] [MarshalAs(UnmanagedType.U4)] int reserved2);

            [return: MarshalAs(UnmanagedType.Interface)]
            IStorage OpenStorage([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr pstgPriority,
                [In] [MarshalAs(UnmanagedType.U4)] int grfMode, IntPtr snbExclude,
                [In] [MarshalAs(UnmanagedType.U4)] int reserved);

            void CopyTo(int ciidExclude, [In] [MarshalAs(UnmanagedType.LPArray)] Guid[]? pIidExclude, IntPtr snbExclude,
                [In] [MarshalAs(UnmanagedType.Interface)]
                IStorage stgDest);

            void MoveElementTo([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName,
                [In] [MarshalAs(UnmanagedType.Interface)]
                IStorage stgDest,
                [In] [MarshalAs(UnmanagedType.BStr)] string pwcsNewName,
                [In] [MarshalAs(UnmanagedType.U4)] int grfFlags);

            void Commit(int grfCommitFlags);
            void Revert();

            void EnumElements([In] [MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2,
                [In] [MarshalAs(UnmanagedType.U4)] int reserved3,
                [MarshalAs(UnmanagedType.Interface)] out object ppVal);

            void DestroyElement([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName);

            void RenameElement([In] [MarshalAs(UnmanagedType.BStr)] string pwcsOldName,
                [In] [MarshalAs(UnmanagedType.BStr)] string pwcsNewName);

            void SetElementTimes([In] [MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] FILETIME pctime,
                [In] FILETIME patime, [In] FILETIME pmtime);

            void SetClass([In] ref Guid clsid);
            void SetStateBits(int grfStateBits, int grfMask);
            void Stat([Out] out STATSTG pStatStg, int grfStatFlag);
        }

        [ComImport]
        [Guid("0000000A-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ILockBytes
        {
            void ReadAt([In] [MarshalAs(UnmanagedType.U8)] long ulOffset,
                [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
                byte[] pv,
                [In] [MarshalAs(UnmanagedType.U4)] int cb, [Out] [MarshalAs(UnmanagedType.LPArray)] int[]? pcbRead);

            void WriteAt([In] [MarshalAs(UnmanagedType.U8)] long ulOffset, IntPtr pv,
                [In] [MarshalAs(UnmanagedType.U4)] int cb, [Out] [MarshalAs(UnmanagedType.LPArray)] int[] pcbWritten);

            void Flush();
            void SetSize([In] [MarshalAs(UnmanagedType.U8)] long cb);

            void LockRegion([In] [MarshalAs(UnmanagedType.U8)] long libOffset,
                [In] [MarshalAs(UnmanagedType.U8)] long cb, [In] [MarshalAs(UnmanagedType.U4)] int dwLockType);

            void UnlockRegion([In] [MarshalAs(UnmanagedType.U8)] long libOffset,
                [In] [MarshalAs(UnmanagedType.U8)] long cb, [In] [MarshalAs(UnmanagedType.U4)] int dwLockType);

            void Stat([Out] out STATSTG pstatstg, [In] [MarshalAs(UnmanagedType.U4)] int grfStatFlag);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FileGroupDescriptor
        {
            public uint cItems;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct FileDescriptor
        {
            [StructLayout(LayoutKind.Sequential, Size = 260, Pack = 1)]
            private struct FileNameType
            {
            }

            [FieldOffset(4 + 16 + 8 + 8 + 4 + 8 + 8 + 8 + 4 + 4)]
            private FileNameType cFileName;
            public string FileName => cFileName.ToSJisString();
        }
    }

    public static object? GetData(System.Windows.IDataObject dataObject, string format, bool autoConvert)
    {
        switch (format)
        {
            case "FileGroupDescriptor":
            {
                var memoryStream = dataObject.GetData("FileGroupDescriptor", autoConvert) as MemoryStream;
                if (memoryStream is null)
                    return null;
                var span = memoryStream.ToArray().AsSpan();
                var fileGroupDescriptor = MemoryMarshal.AsRef<Windows.FileGroupDescriptor>(span);

                var fileNames = new string[fileGroupDescriptor.cItems];

                span = span[Unsafe.SizeOf<Windows.FileGroupDescriptor>()..];

                for (var i = 0; i < fileGroupDescriptor.cItems; i++)
                {
                    var fileDescriptor = MemoryMarshal.AsRef<Windows.FileDescriptor>(span);

                    fileNames[i] = fileDescriptor.FileName;

                    span = span[Unsafe.SizeOf<Windows.FileDescriptor>()..];
                }

                return fileNames;
            }
            default:
                throw new NotSupportedException();
        }
    }

    public static MemoryStream GetData(System.Windows.IDataObject dataObject, string format, int index)
    {
        var formatetc = new FORMATETC
        {
            cfFormat = (short)DataFormats.GetDataFormat(format).Id,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = index,
            ptd = new IntPtr(0),
            tymed = TYMED.TYMED_ISTREAM | TYMED.TYMED_ISTORAGE | TYMED.TYMED_HGLOBAL
        };

        var comUnderlyingDataObject = (System.Runtime.InteropServices.ComTypes.IDataObject)dataObject;
        comUnderlyingDataObject.GetData(ref formatetc, out var medium);

        switch (medium.tymed)
        {
            case TYMED.TYMED_ISTORAGE:
                Windows.IStorage? iStorage = null;
                Windows.IStorage? iStorage2 = null;
                Windows.ILockBytes? iLockBytes = null;
                try
                {
                    iStorage = (Windows.IStorage)Marshal.GetObjectForIUnknown(medium.unionmember);

                    iLockBytes = Windows.CreateILockBytesOnHGlobal(IntPtr.Zero, true) ??
                                 throw new InvalidOperationException();
                    iStorage2 = Windows.StgCreateDocfileOnILockBytes(iLockBytes, 0x00001012, 0) ??
                                throw new InvalidOperationException();

                    iStorage.CopyTo(0, null, IntPtr.Zero, iStorage2);
                    iLockBytes.Flush();
                    iStorage2.Commit(0);

                    iLockBytes.Stat(out var iLockBytesStat, 1);

                    var value = new byte[(int)iLockBytesStat.cbSize];
                    iLockBytes.ReadAt(0, value, value.Length, null);

                    return new MemoryStream(value);
                }
                finally
                {
                    if (iStorage2 is not null) Marshal.ReleaseComObject(iStorage2);
                    if (iLockBytes is not null) Marshal.ReleaseComObject(iLockBytes);
                    if (iStorage is not null) Marshal.ReleaseComObject(iStorage);
                }

            case TYMED.TYMED_ISTREAM:
                IStream? iStream = null;
                try
                {
                    iStream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                    Marshal.Release(medium.unionmember);

                    iStream.Stat(out var iStreamStat, 0);

                    var value = new byte[(int)iStreamStat.cbSize];
                    iStream.Read(value, value.Length, IntPtr.Zero);

                    return new MemoryStream(value);
                }
                finally
                {
                    if (iStream is not null) Marshal.ReleaseComObject(iStream);
                }
            default:
                throw new NotSupportedException();
        }
    }
}