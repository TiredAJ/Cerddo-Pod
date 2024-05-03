using System.Collections.Concurrent;
using System.Diagnostics;

namespace Utilities;

public class Logger
{
    private static ConcurrentQueue<(DateTime Stamp, string Msg)> FileTemp = new();
    private static ConcurrentQueue<(DateTime Stamp, string Msg)> Log = new();
    private static object Lock = new();

    public static async Task PushLog(string _Msg)
    {
        await Task.Run(() =>
        {
            var MSG = (DateTime.Now, _Msg);

            Log.Enqueue(MSG);

            SendLog_Debug(MSG);
            SendLog_StdOut(MSG);
            SendLog_File(MSG);
        });
    }

    private static void SendLog_Debug((DateTime Stamp, string Msg) _Msg)
    { Debug.WriteLine(_Msg.ToString()); }

    private static void SendLog_StdOut((DateTime Stamp, string Msg) _Msg)
    { Console.WriteLine(_Msg.ToString()); }

    private static void SendLog_File((DateTime Stamp, string Msg) _Msg)
    {
        //FileTemp.Enqueue(_Msg);

        //lock (Lock)
        //{
        //    while (!FileTemp.IsEmpty)
        //    {

        //    }
        //}
    }
}

static class Extensions
{
    internal static string ToString(this (DateTime Stamp, string Msg) _Msg)
    { return $"[{_Msg.Stamp.ToString("dd/MM/yyyy HH:mm:ss")}]: {_Msg.Msg}"; }
}