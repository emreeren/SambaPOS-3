using System;
using System.Threading;

namespace Samba.Infrastructure.Cron
{
    public class CronEventArgs : EventArgs
    {
        public CronObject CronObject { get; set; }
    }

    public sealed class CronObject : IDisposable
    {
        public delegate void CronEvent(object sender, CronEventArgs e);
        public event CronEvent OnCronTrigger;
        public event CronEvent OnStarted;
        public event CronEvent OnStopped;
        public event CronEvent OnThreadAbort;

        private readonly CronObjectDataContext _cronObjectDataContext;
        private readonly Guid _id = Guid.NewGuid();
        private readonly object _startStopLock = new object();
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private Thread _thread;
        private bool _isStarted;
        private bool _isStopRequested;
        private DateTime _nextCronTrigger;

        public Guid Id { get { return _id; } }
        public object Object { get { return _cronObjectDataContext.Object; } }
        public DateTime LastTigger { get { return _cronObjectDataContext.LastTrigger; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CronObject"/> class.
        /// </summary>
        /// <param name="cronObjectDataContext">The cron object data context.</param>
        public CronObject(CronObjectDataContext cronObjectDataContext)
        {
            if (cronObjectDataContext == null)
            {
                throw new ArgumentNullException("cronObjectDataContext");
            }
            if (cronObjectDataContext.Object == null)
            {
                throw new ArgumentException("cronObjectDataContext.Object");
            }
            if (cronObjectDataContext.CronSchedules == null || cronObjectDataContext.CronSchedules.Count == 0)
            {
                throw new ArgumentException("cronObjectDataContext.CronSchedules");
            }
            _cronObjectDataContext = cronObjectDataContext;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            lock (_startStopLock)
            {
                // Can't start if already started.
                //
                if (_isStarted)
                {
                    return false;
                }
                _isStarted = true;
                _isStopRequested = false;

                // This is a long running process. Need to run on a thread
                //	outside the thread pool.
                //
                _thread = new Thread(ThreadRoutine);
                _thread.Start();
            }

            // Raise the started event.
            //
            if (OnStarted != null)
            {
                OnStarted(this, new CronEventArgs { CronObject = this });
            }

            return true;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            lock (_startStopLock)
            {
                // Can't stop if not started.
                //
                if (!_isStarted)
                {
                    return false;
                }
                _isStarted = false;
                _isStopRequested = true;

                // Signal the thread to wake up early
                //
                _wh.Set();

                // Wait for the thread to join.
                //
                if (!_thread.Join(5000))
                {
                    _thread.Abort();

                    // Raise the thread abort event.
                    //
                    if (OnThreadAbort != null)
                    {
                        OnThreadAbort(this, new CronEventArgs { CronObject = this });
                    }
                }
            }

            // Raise the stopped event.
            //
            if (OnStopped != null)
            {
                OnStopped(this, new CronEventArgs { CronObject = this });
            }
            return true;
        }

        /// <summary>
        /// Cron object thread routine.
        /// </summary>
        private void ThreadRoutine()
        {
            // Continue until stop is requested.
            //
            while (!_isStopRequested)
            {
                // Determine the next cron trigger
                //
                DetermineNextCronTrigger(out _nextCronTrigger);

                TimeSpan sleepSpan = _nextCronTrigger - DateTime.Now;
                if (sleepSpan.TotalMilliseconds < 0)
                {
                    // Next trigger is in the past. Trigger the right away.
                    //
                    sleepSpan = new TimeSpan(0, 0, 0, 0, 50);
                }

                // Wait here for the timespan or until I am triggered
                //	to wake up.
                //
                if (!_wh.WaitOne(sleepSpan.TotalMilliseconds < int.MaxValue ? (int)sleepSpan.TotalMilliseconds : int.MaxValue))
                {
                    // Timespan is up...raise the trigger event
                    //
                    if (OnCronTrigger != null)
                    {
                        OnCronTrigger(this, new CronEventArgs { CronObject = this });
                    }

                    // Update the last trigger time.
                    //
                    _cronObjectDataContext.LastTrigger = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Determines the next cron trigger.
        /// </summary>
        /// <param name="nextTrigger">The next trigger.</param>
        private void DetermineNextCronTrigger(out DateTime nextTrigger)
        {
            nextTrigger = DateTime.MaxValue;
            foreach (CronSchedule cronSchedule in _cronObjectDataContext.CronSchedules)
            {
                DateTime thisTrigger;
                if (cronSchedule.GetNext(LastTigger, out thisTrigger))
                {
                    if (thisTrigger < nextTrigger)
                    {
                        nextTrigger = thisTrigger;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_wh != null)
                _wh.Dispose();
        }
    }
}