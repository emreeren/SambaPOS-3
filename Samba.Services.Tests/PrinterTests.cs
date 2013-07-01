using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Samba.Services.Implementations.PrinterModule.Formatters;

namespace Samba.Services.Tests
{
    [TestFixture]
    class PrinterTests
    {
        [Test]
        public void CanMeasureText()
        {
            var formatter = new JustifyAlignFormatterBySize("", 42, true, new[] { 15, 5 });
            var size = formatter.GetSize("---");
            var size2 = formatter.GetSize("   ");
            Assert.AreEqual(size, size2);
            var maxText = formatter.GetMaxText(42);
            var testText1 = "123456789012345678901234567890 123456789012";
            var textText2 = "- 1 招牌蟹黄小笼包	                   9.00";
            var textText3 = "- 1 招牌蟹黄小笼包                   9.00";

            Assert.AreEqual(formatter.GetSize(testText1), formatter.GetSize(textText2));
        }
    }
}
