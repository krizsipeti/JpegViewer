using System;
using System.Threading.Tasks;

namespace JpegViewer.App.Core.Interfaces
{
    /// <summary>
    /// Provides an abstraction for executing actions on a dispatcher thread, ensuring thread-safe operations in
    /// environments where thread affinity is required, such as UI frameworks.
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// True if the calling thread has access to the dispatcher.
        /// </summary>
        bool CheckAccess();

        /// <summary>
        /// Calls the specified action on the dispatcher thread.
        /// </summary>
        void Invoke(Action action);

        /// <summary>
        /// Calls the specified async action on the dispatcher thread.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        Task InvokeAsync(Func<Task> func);
    }
}
