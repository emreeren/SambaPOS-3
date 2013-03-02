namespace Samba.Presentation.Common.Commands
{
    public interface ICategoryCommand : ICaptionCommand
    {
        string Category { get; set; }
        string ImageSource { get; set; }
        int Order { get; set; }
    }
}
