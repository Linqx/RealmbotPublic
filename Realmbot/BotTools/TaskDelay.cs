using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BotTools {
    public class TaskHelper {
        private static readonly ConcurrentDictionary<TaskCompletionSource<bool>, Timer> s_timers =
            new ConcurrentDictionary<TaskCompletionSource<bool>, Timer>();

        public static Task<bool> Delay(int millisecondsDelay) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            if (millisecondsDelay <= 0) {
                tcs.SetResult(true);
                return tcs.Task;
            }

            Timer timer = new Timer();
            timer.Mode = TimerMode.OneShot;
            timer.Period = millisecondsDelay;

            s_timers.TryAdd(tcs, timer);

            timer.Tick += (sender, e) => {
                tcs.SetResult(true);
                Timer empty;
                s_timers.TryRemove(tcs, out empty);
            };

            timer.Start();

            return tcs.Task;
        }
    }

    /// <summary>
    /// Defines constants for the multimedia Timer's event types.
    /// </summary>
    public enum TimerMode {
        /// <summary>
        /// Timer event occurs once.
        /// </summary>
        OneShot,

        /// <summary>
        /// Timer event occurs periodically.
        /// </summary>
        Periodic
    }

    /// <summary>
    /// Represents information about the multimedia Timer's capabilities.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCaps {
        /// <summary>
        /// Minimum supported period in milliseconds.
        /// </summary>
        public int periodMin;

        /// <summary>
        /// Maximum supported period in milliseconds.
        /// </summary>
        public int periodMax;
    }

    /// <summary>
    /// Represents the Windows multimedia timer.
    /// </summary>
    public sealed class Timer : IComponent {

        #region IDisposable Members

        /// <summary>
        /// Frees timer resources.
        /// </summary>
        public void Dispose() {
            #region Guard

            if (disposed) return;

            #endregion

            if (IsRunning) Stop();

            disposed = true;

            OnDisposed(EventArgs.Empty);
        }

        #endregion

        #region Timer Members

        #region Delegates

        // Represents the method that is called by Windows when a timer event occurs.
        private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        // Represents methods that raise events.
        private delegate void EventRaiser(EventArgs e);

        #endregion

        #region Win32 Multimedia Timer Functions

        // Gets timer capabilities.
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps,
            int sizeOfTimerCaps);

        // Creates and starts the timer.
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution,
            TimeProc proc, int user, int mode);

        // Stops and destroys the timer.
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        // Indicates that the operation was successful.
        private const int TIMERR_NOERROR = 0;

        #endregion

        #region Fields

        // Timer identifier.
        private int timerID;

        // Timer mode.
        private volatile TimerMode mode;

        // Period between timer events in milliseconds.
        private volatile int period;

        // Timer resolution in milliseconds.
        private volatile int resolution;

        // Called by Windows when a timer periodic event occurs.
        private TimeProc timeProcPeriodic;

        // Called by Windows when a timer one shot event occurs.
        private TimeProc timeProcOneShot;

        // Represents the method that raises the Tick event.
        private EventRaiser tickRaiser;

        // Indicates whether or not the timer is running.

        // Indicates whether or not the timer has been disposed.
        private volatile bool disposed;

        // The ISynchronizeInvoke object to use for marshaling events.
        private ISynchronizeInvoke synchronizingObject;

        // For implementing IComponent.

        // Multimedia timer capabilities.
        private static readonly TimerCaps caps;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the Timer has started;
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when the Timer has stopped;
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when the time period has elapsed.
        /// </summary>
        public event EventHandler Tick;

        #endregion

        #region Construction

        /// <summary>
        /// Initialize class.
        /// </summary>
        static Timer() {
            // Get multimedia timer capabilities.
            timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
        }

        /// <summary>
        /// Initializes a new instance of the Timer class with the specified IContainer.
        /// </summary>
        /// <param name="container">
        /// The IContainer to which the Timer will add itself.
        /// </param>
        public Timer(IContainer container) {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            container.Add(this);

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the Timer class.
        /// </summary>
        public Timer() {
            Initialize();
        }

        ~Timer() {
            if (IsRunning)
                // Stop and destroy timer.
                timeKillEvent(timerID);
        }

        // Initialize timer with default values.
        private void Initialize() {
            mode = TimerMode.Periodic;
            period = Capabilities.periodMin;
            resolution = 1;

            IsRunning = false;

            timeProcPeriodic = TimerPeriodicEventCallback;
            timeProcOneShot = TimerOneShotEventCallback;
            tickRaiser = OnTick;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// The timer has already been disposed.
        /// </exception>
        /// <exception cref="TimerStartException">
        /// The timer failed to start.
        /// </exception>
        public void Start() {
            #region Require

            if (disposed) throw new ObjectDisposedException("Timer");

            #endregion

            #region Guard

            if (IsRunning) return;

            #endregion

            // If the periodic event callback should be used.
            if (Mode == TimerMode.Periodic)
                // Create and start timer.
                timerID = timeSetEvent(Period, Resolution, timeProcPeriodic, 0, (int) Mode);
            // Else the one shot event callback should be used.
            else
                // Create and start timer.
                timerID = timeSetEvent(Period, Resolution, timeProcOneShot, 0, (int) Mode);

            // If the timer was created successfully.
            if (timerID != 0) {
                IsRunning = true;

                if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                    SynchronizingObject.BeginInvoke(
                        new EventRaiser(OnStarted),
                        new object[] {EventArgs.Empty});
                else
                    OnStarted(EventArgs.Empty);
            }
            else {
                throw new TimerStartException("Unable to start multimedia Timer.");
            }
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public void Stop() {
            #region Require

            if (disposed) throw new ObjectDisposedException("Timer");

            #endregion

            #region Guard

            if (!IsRunning) return;

            #endregion

            // Stop and destroy timer.
            int result = timeKillEvent(timerID);

            Debug.Assert(result == TIMERR_NOERROR);

            IsRunning = false;

            if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
                SynchronizingObject.BeginInvoke(
                    new EventRaiser(OnStopped),
                    new object[] {EventArgs.Empty});
            else
                OnStopped(EventArgs.Empty);
        }

        #region Callbacks

        // Callback method called by the Win32 multimedia timer when a timer
        // periodic event occurs.
        private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2) {
            if (synchronizingObject != null)
                synchronizingObject.BeginInvoke(tickRaiser, new object[] {EventArgs.Empty});
            else
                OnTick(EventArgs.Empty);
        }

        // Callback method called by the Win32 multimedia timer when a timer
        // one shot event occurs.
        private void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2) {
            if (synchronizingObject != null) {
                synchronizingObject.BeginInvoke(tickRaiser, new object[] {EventArgs.Empty});
                Stop();
            }
            else {
                OnTick(EventArgs.Empty);
                Stop();
            }
        }

        #endregion

        #region Event Raiser Methods

        // Raises the Disposed event.
        private void OnDisposed(EventArgs e) {
            EventHandler handler = Disposed;

            if (handler != null) handler(this, e);
        }

        // Raises the Started event.
        private void OnStarted(EventArgs e) {
            EventHandler handler = Started;

            if (handler != null) handler(this, e);
        }

        // Raises the Stopped event.
        private void OnStopped(EventArgs e) {
            EventHandler handler = Stopped;

            if (handler != null) handler(this, e);
        }

        // Raises the Tick event.
        private void OnTick(EventArgs e) {
            EventHandler handler = Tick;

            if (handler != null) handler(this, e);
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the object used to marshal event-handler calls.
        /// </summary>
        public ISynchronizeInvoke SynchronizingObject {
            get {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                return synchronizingObject;
            }
            set {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                synchronizingObject = value;
            }
        }

        /// <summary>
        /// Gets or sets the time between Tick events.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>   
        public int Period {
            get {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                return period;
            }
            set {
                #region Require

                if (disposed)
                    throw new ObjectDisposedException("Timer");
                if (value < Capabilities.periodMin || value > Capabilities.periodMax)
                    throw new ArgumentOutOfRangeException("Period", value,
                        "Multimedia Timer period out of range.");

                #endregion

                period = value;

                if (IsRunning) {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        /// Gets or sets the timer resolution.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>        
        /// <remarks>
        /// The resolution is in milliseconds. The resolution increases 
        /// with smaller values; a resolution of 0 indicates periodic events 
        /// should occur with the greatest possible accuracy. To reduce system 
        /// overhead, however, you should use the maximum value appropriate 
        /// for your application.
        /// </remarks>
        public int Resolution {
            get {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                return resolution;
            }
            set {
                #region Require

                if (disposed)
                    throw new ObjectDisposedException("Timer");
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Resolution", value,
                        "Multimedia timer resolution out of range.");

                #endregion

                resolution = value;

                if (IsRunning) {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        /// Gets the timer mode.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// If the timer has already been disposed.
        /// </exception>
        public TimerMode Mode {
            get {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                return mode;
            }
            set {
                #region Require

                if (disposed) throw new ObjectDisposedException("Timer");

                #endregion

                mode = value;

                if (IsRunning) {
                    Stop();
                    Start();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Timer is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the timer capabilities.
        /// </summary>
        public static TimerCaps Capabilities => caps;

        #endregion

        #endregion

        #region IComponent Members

        public event EventHandler Disposed;

        public ISite Site { get; set; } = null;

        #endregion

    }

    /// <summary>
    /// The exception that is thrown when a timer fails to start.
    /// </summary>
    public class TimerStartException : ApplicationException {
        /// <summary>
        /// Initializes a new instance of the TimerStartException class.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception. 
        /// </param>
        public TimerStartException(string message) : base(message) {
        }
    }
}