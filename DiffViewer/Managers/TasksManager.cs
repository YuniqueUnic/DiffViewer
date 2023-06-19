using System;
using System.Threading.Tasks;

namespace DiffViewer.Managers;

internal class TasksManager
{
    private static void IsNullMethod<TResult>(Func<TResult> func , string name , string prompt)
    {
        if( func is null )
        {
            App.Logger.Error($"{prompt}Task `{name}` failed: {nameof(func)} is null");
            throw new ArgumentNullException(nameof(func));
        }
    }
    private static void IsNullMethod(Action action , string name , string prompt)
    {
        if( action is null )
        {
            App.Logger.Error($"{prompt}Task `{name}` failed: {nameof(action)} is null");
            throw new ArgumentNullException(nameof(action));
        }
    }

    /// <summary>
    /// Runs a task with better logging.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="name">The name for logging.</param>
    /// <param name="prompt">The prompt for logging.</param>
    /// <param name="catchException">Whether to catch exceptions.</param>
    public static void RunTask(
        Action action ,
        string name = nameof(Action) ,
        string prompt = ">>> " ,
        bool catchException = false , bool throwException = true)
    {
        IsNullMethod(action , name , prompt);

        App.Logger.Information($"{prompt}Task `{name}` began.");

        if( catchException )
        {
            try
            {
                action();
            }
            catch( Exception e )
            {
                App.Logger.Error(e , $"{prompt}Task `{name}` failed: {e.Message}");
                if( throwException ) throw;
            }
        }
        else
        {
            action();
        }

        App.Logger.Information($"{prompt}Task `{name}` done.");
    }

    /// <summary>
    /// Runs a task with better logging return a value.
    /// </summary>
    /// <param name="func">The action to be executed.</param>
    /// <param name="name">The name for logging.</param>
    /// <param name="prompt">The prompt for logging.</param>
    /// <param name="catchException">Whether to catch exceptions.</param>
    public static TResult RunTaskWithReturn<TResult>(
        Func<TResult> func ,
        string name = nameof(Action) ,
        string prompt = ">>> " ,
        bool catchException = false , bool throwException = true)
    {
        IsNullMethod(func , name , prompt);

        App.Logger.Information($"{prompt}Task `{name}` began.");

        if( catchException )
        {
            try
            {
                return func();
            }
            catch( Exception e )
            {
                App.Logger.Error(e , $"{prompt}Task `{name}` failed: {e.Message}");
                if( throwException ) throw;
            }
        }

        TResult result = func();

        App.Logger.Information($"{prompt}Task `{name}` done.");

        return result;
    }

    /// <summary>
    /// Runs a task asynchronously with better logging.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="name">The name for logging.</param>
    /// <param name="prompt">The prompt for logging.</param>
    /// <param name="catchException">Whether to catch exceptions.</param>
    public static async Task RunTaskAsync(
        Action action ,
        string name = nameof(Action) ,
        string prompt = ">>> " ,
        bool catchException = false , bool throwException = true)
    {
        IsNullMethod(action , name , prompt);

        App.Logger.Information($"{prompt}Task `{name}` began.");

        if( catchException )
        {
            try
            {
                await Task.Run(action);
            }
            catch( Exception e )
            {
                App.Logger.Error(e , $"{prompt}Task `{name}` failed: {e.Message}");
                if( throwException ) throw;
            }
        }
        else
        {
            await Task.Run(action);
        }

        App.Logger.Information($"{prompt}Task `{name}` done.");
    }

    /// <summary>
    /// Runs a task asynchronously with better logging and return a value.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="name">The name for logging.</param>
    /// <param name="prompt">The prompt for logging.</param>
    /// <param name="catchException">Whether to catch exceptions.</param>
    public static async Task<TResult> RunTaskWithReturnAsync<TResult>(
        Func<TResult> func ,
        string name = nameof(Action) ,
        string prompt = ">>> " ,
        bool catchException = false , bool throwException = true)
    {
        IsNullMethod(func , name , prompt);

        App.Logger.Information($"{prompt}Task `{name}` began.");

        if( catchException )
        {
            try
            {
                return await Task.Run(func);
            }
            catch( Exception e )
            {
                App.Logger.Error(e , $"{prompt}Task `{name}` failed: {e.Message}");
                if( throwException ) throw;
            }
        }

        TResult result = await Task.Run(func);

        App.Logger.Information($"{prompt}Task `{name}` done.");
        return result;
    }
}