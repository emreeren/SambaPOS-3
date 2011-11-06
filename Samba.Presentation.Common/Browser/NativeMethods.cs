namespace Samba.Presentation.Common.Browser
{
  static class NativeMethods
  {
    public enum OLECMDF
    {
      // Fields
      OLECMDF_DEFHIDEONCTXTMENU = 0x20,
      OLECMDF_ENABLED = 2,
      OLECMDF_INVISIBLE = 0x10,
      OLECMDF_LATCHED = 4,
      OLECMDF_NINCHED = 8,
      OLECMDF_SUPPORTED = 1
    }

    public enum OLECMDID
    {
      // Fields
      OLECMDID_PAGESETUP = 8,
      OLECMDID_PRINT = 6,
      OLECMDID_PRINTPREVIEW = 7,
      OLECMDID_PROPERTIES = 10,
      OLECMDID_SAVEAS = 4,
      // OLECMDID_SHOWSCRIPTERROR = 40
    }
    public enum OLECMDEXECOPT
    {
      // Fields
      OLECMDEXECOPT_DODEFAULT = 0,
      OLECMDEXECOPT_DONTPROMPTUSER = 2,
      OLECMDEXECOPT_PROMPTUSER = 1,
      OLECMDEXECOPT_SHOWHELP = 3
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public class POINT
    //{
    //  public int x;
    //  public int y;
    //  public POINT() { }
    //  public POINT(int x, int y)
    //  {
    //    this.x = x;
    //    this.y = y;
    //  }
    //}

    //[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), ComVisible(true)]
    //public interface IOleCommandTarget
    //{
    //  [return: MarshalAs(UnmanagedType.I4)]
    //  [PreserveSig]
    //  int QueryStatus(ref Guid pguidCmdGroup, int cCmds, [In, Out] NativeMethods.OLECMD prgCmds, [In, Out] IntPtr pCmdText);
    //  [return: MarshalAs(UnmanagedType.I4)]
    //  [PreserveSig]
    //  int Exec(ref Guid pguidCmdGroup, int nCmdID, int nCmdexecopt, [In, MarshalAs(UnmanagedType.LPArray)] object[] pvaIn, ref int pvaOut);
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public class OLECMD
    //{
    //  [MarshalAs(UnmanagedType.U4)]
    //  public int cmdID;
    //  [MarshalAs(UnmanagedType.U4)]
    //  public int cmdf;
    //  public OLECMD()
    //  {
    //  }
    //}

    //public const int S_FALSE = 1;
    //public const int S_OK = 0;

 
  }
}
