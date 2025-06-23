using Foundation;
using HomeInventory.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace HomeInventory.iOS.Services
{
    public class ToastMessage : IToastMessage
    {
        const double LONG_DELAY = 3.5;
        const double SHORT_DELAY = 2.0;

        NSTimer alertDelay;
        UIAlertController alert;

        public void LongAlert(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }

        public void ShortAlert(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }

        private void ShowAlert(string message, double seconds)
        {
            alertDelay = NSTimer.CreateScheduledTimer(seconds, (obj) =>
            {
                DismissMessage();
            });
            alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void DismissMessage()
        {
            alert?.DismissViewController(true, null);
            alertDelay?.Dispose();
        }
    }
}