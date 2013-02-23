using Samba.Presentation.Common.Commands;

namespace Samba.Presentation.Common.ModelBase
{
    public interface IEditableCollection
    {
        ICaptionCommand AddItemCommand { get; set; }
        ICaptionCommand EditItemCommand { get; set; }
        ICaptionCommand DeleteItemCommand { get; set; }
    }
}
