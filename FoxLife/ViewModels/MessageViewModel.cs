using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DevExpress.Mvvm;

namespace FoxLife.ViewModels
{
    internal class MessageViewModel : ViewModelBase
    {

        public int Id { get; set; }
        public string Message { get; set; }
        public Brush? Background { get; set; }
        public bool StartDeleteAnimation { get; set; } = false;

        public enum MessageType 
        {
            Error, 
            Success,
            Warning
        }

        public MessageViewModel(int id,string message, MessageType type, int msDelay)
        {
            Id = id;
            Message = message;
            if (msDelay != -1)
            {
                msDelay -= 1000;
                if (msDelay < 0) msDelay = 0;

                Task.Run(async () =>
                {
                    await Task.Delay(msDelay);

                    StartDeleteAnimation = true; 
                }).ConfigureAwait(false);
            }

            switch (type)
            {
                case MessageType.Error:
                    Background = new SolidColorBrush(Colors.IndianRed) { Opacity = 0.4 };
                    break;
                case MessageType.Success:
                    Background = new SolidColorBrush(Colors.LawnGreen) { Opacity = 0.4 };
                    break;
                case MessageType.Warning:
                    Background = new SolidColorBrush(Color.FromRgb(244,208,63)) { Opacity = 0.4 };
                    break;
            }
        }

    }
}
