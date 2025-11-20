using CommunityToolkit.Mvvm.ComponentModel;
using STranslate.Core;

namespace STranslate.ViewModels;

public partial class HistoryDetailViewModel(HistoryModel historyModel) : ObservableObject
{
    public HistoryModel HistoryModel { get; } = historyModel;
}
