using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SamacharFeedReader.Commands
{
    class ShowAllCommand : CommandBase<ShowAllCommand>
    {
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            ((SamacharFeedReader.MainWindow)Application.Current.MainWindow).downloadedFeedsWnd.showActivated();
        }
    }
}
