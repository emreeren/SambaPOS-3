using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Tasks;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    public class TaskCommand
    {
        public TaskCommand(string s)
        {
            DisplayName = s;
            if (s.Contains(":"))
            {
                var parts = s.Split(new[] { ':' }, 2);
                CommandName = parts[0];
                CommandValue = parts[1];
                DisplayName = CommandName;
            }
            else CommandName = s;
            if (CommandName.Contains("="))
            {
                var parts = CommandName.Split(new[] { '=' }, 2);
                DisplayName = parts[0];
                CommandName = parts[1];
            }
        }

        public string CommandName { get; set; }
        public string CommandValue { get; set; }
        public string DisplayName { get; set; }

        public string GetCommandValue(Task task)
        {
            var value = CommandValue;
            if (!string.IsNullOrWhiteSpace(CommandValue) && Regex.IsMatch(value, "\\[(.*)\\]"))
            {
                value = Regex.Match(value, "\\[(.*)\\]").Groups[1].Value;
                value = task.GetCustomDataValue(value);
            }
            if (string.IsNullOrEmpty(value))
                value = task.Content;
            return value;
        }
    }
}