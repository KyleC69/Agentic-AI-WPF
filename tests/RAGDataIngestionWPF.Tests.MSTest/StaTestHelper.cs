using System.Runtime.ExceptionServices;

namespace RAGDataIngestionWPF.Tests.MSTest;

internal static class StaTestHelper
{
    public static void Run(Action action)
    {
        Exception error = null;

        Thread thread = new(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    public static T Run<T>(Func<T> action)
    {
        T result = default;
        Run(() => result = action());
        return result;
    }
}
