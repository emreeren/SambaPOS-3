namespace Samba.Services
{
    public interface ISettingReplacer
    {
        string ReplaceSettingValue(string template, string value);
    }
}