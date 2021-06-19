using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace umamusumeKeyCtl.Util
{
    /// <summary>
    /// This is a normal label, but it sends an event when it is pressed for a while.
    /// </summary>
    public class HoldableLabel : Label
    {
        /// <summary>
        /// Duration that is required to invoke event.
        /// </summary>
        public TimeSpan HoldDuration { get; set; } = TimeSpan.FromSeconds(1);
        
        /// <summary>
        /// Occurs when the left button up after long pressed.
        /// </summary>
        public event MouseButtonEventHandler MouseLeftButtonHoldUp;
        
        /// <summary>
        /// Occurs when being long pressed while left button is down.
        /// </summary>
        public event MouseButtonEventHandler MouseLeftButtonHold;
        
        private DispatcherTimer timer = new();
        private bool _holded = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            timer = new DispatcherTimer()
            {
                Interval = HoldDuration
            };
            
            timer.Tick += delegate
            {
                _holded = true;
                MouseLeftButtonHold?.Invoke(this, e);
                timer.Stop();
            };
            timer.Start();
            
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            timer.Stop();
            if (_holded)
            {
                MouseLeftButtonHoldUp?.Invoke(this, e);
                _holded = false;
            }
            
            base.OnMouseLeftButtonUp(e);
        }
    }
}