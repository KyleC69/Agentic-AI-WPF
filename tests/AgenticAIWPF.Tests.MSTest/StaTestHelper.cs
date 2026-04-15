// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         StaTestHelper.cs
// Author: Kyle L. Crowder
// Build Num: 213002



using System.Runtime.ExceptionServices;




namespace AgenticAIWPF.Tests.MSTest;





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