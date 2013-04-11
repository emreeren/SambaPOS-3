namespace Fluentscript.Lib.Templating
{
    /// <summary>
    /// Interface for a Template engine.
    /// </summary>
    public interface ITemplateEngine
    {
        /// <summary>
        /// Initialize with the script.
        /// </summary>
        /// <param name="script"></param>
        void Init(string script);


        /// <summary>
        /// Render the script initialized with.
        /// </summary>
        /// <returns></returns>
        string Render();


        /// <summary>
        /// Render the script supplied.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        string Render(string script);
    }
}
