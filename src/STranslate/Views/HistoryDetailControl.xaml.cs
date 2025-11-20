using STranslate.Core;
using STranslate.ViewModels;

namespace STranslate.Views;

public partial class HistoryDetailControl
{
    public HistoryDetailControl(HistoryModel historyModel)
    {
        InitializeComponent();

        var vm = new HistoryDetailViewModel(historyModel);
        DataContext = vm;
    }
}
