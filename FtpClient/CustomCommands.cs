using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FtpClient
{
    static class CustomCommands
    {
        private static RoutedUICommand _aboutCommand;

        public static RoutedUICommand About
        {
            get
            {
                return _aboutCommand;
            }
        }

        static CustomCommands()
        {
            _aboutCommand = new RoutedUICommand("About", "About",
                typeof(Window),
                new InputGestureCollection { new KeyGesture(Key.F1) });
        }
    }
}
