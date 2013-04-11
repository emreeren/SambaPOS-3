namespace Fluentscript.Lib._Core
{
   /// <summary>
    /// Combines a boolean succes/fail flag with a error/status message and an object.
    /// </summary>
    public class BoolMsgObj
    {
        /// <summary>
        /// Item associated with boolean message.
        /// </summary>
        private object _item;

		
		/// <summary>
        /// Success / failure ?
        /// </summary>
        public readonly bool Success;

		
        /// <summary>
        /// Error message for failure, status message for success.
        /// </summary>
        public readonly string Message;


        /// <summary>
        /// True message.
        /// </summary>
        public static readonly BoolMsgObj True = new BoolMsgObj(null, true, string.Empty);


        /// <summary>
        /// False message.
        /// </summary>
        public static readonly BoolMsgObj False = new BoolMsgObj(null, false, string.Empty);
        

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolMessageItem&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="message">The message.</param>
        public BoolMsgObj(object item, bool success, string message)
        {
            _item = item;
			Success = success;
			Message = message;
        }


        /// <summary>
        /// Return readonly item.
        /// </summary>
        public object Item
        {
            get { return _item; }
        }
    }
}
