//---------------------------------------------------------------------------
// 
// File: HtmlParser.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Parser for Html-to-Xaml converter
//
//---------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace Samba.Services.Implementations.PrinterModule.HtmlConverter
{
    /// <summary>
    /// HtmlParser class accepts a string of possibly badly formed Html, parses it and returns a string
    /// of well-formed Html that is as close to the original string in content as possible
    /// </summary>

    internal class HtmlParser
    {
        // ---------------------------------------------------------------------
        //
        // Constructors
        //
        // ---------------------------------------------------------------------

        #region Constructors

        /// <summary>
        /// Constructor. Initializes the _htmlLexicalAnalayzer element with the given input string
        /// </summary>
        /// <param name="inputString">
        /// string to parsed into well-formed Html
        /// </param>
        private HtmlParser(string inputString)
        {
            // Create an output xml document
            _document = new XmlDocument();

            // initialize open tag stack
            _openedElements = new Stack<XmlElement>();

            _pendingInlineElements = new Stack<XmlElement>();

            // initialize lexical analyzer
            _htmlLexicalAnalyzer = new HtmlLexicalAnalyzer(inputString);

            // get first token from input, expecting text
            _htmlLexicalAnalyzer.GetNextContentToken();
        }

        #endregion Constructors

        // ---------------------------------------------------------------------
        //
        // Internal Methods
        //
        // ---------------------------------------------------------------------

        #region Internal Methods

        /// <summary>
        /// Instantiates an HtmlParser element and calls the parsing function on the given input string
        /// </summary>
        /// <param name="htmlString">
        /// Input string of pssibly badly-formed Html to be parsed into well-formed Html
        /// </param>
        /// <returns>
        /// XmlElement rep
        /// </returns>
        internal static XmlElement ParseHtml(string htmlString)
        {
            HtmlParser htmlParser = new HtmlParser(htmlString);

            XmlElement htmlRootElement = htmlParser.ParseHtmlContent();

            return htmlRootElement;
        }

        // .....................................................................
        //
        // Html Header on Clipboard
        //
        // .....................................................................

        // Html header structure.
        //      Version:1.0
        //      StartHTML:000000000
        //      EndHTML:000000000
        //      StartFragment:000000000
        //      EndFragment:000000000
        //      StartSelection:000000000
        //      EndSelection:000000000
        internal const string HtmlHeader = "Version:1.0\r\nStartHTML:{0:D10}\r\nEndHTML:{1:D10}\r\nStartFragment:{2:D10}\r\nEndFragment:{3:D10}\r\nStartSelection:{4:D10}\r\nEndSelection:{5:D10}\r\n";
        internal const string HtmlStartFragmentComment = "<!--StartFragment-->";
        internal const string HtmlEndFragmentComment = "<!--EndFragment-->";

        /// <summary>
        /// Extracts Html string from clipboard data by parsing header information in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        /// String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        /// String containing only the Html data part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFromClipboardData(string htmlDataString)
        {
            int startHtmlIndex = htmlDataString.IndexOf("StartHTML:");
            if (startHtmlIndex < 0)
            {
                return "ERROR: Urecognized html header";
            }
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startHtmlIndex = Int32.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
            if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length)
            {
                return "ERROR: Urecognized html header";
            }

            int endHtmlIndex = htmlDataString.IndexOf("EndHTML:");
            if (endHtmlIndex < 0)
            {
                return "ERROR: Urecognized html header";
            }
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endHtmlIndex = Int32.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
            if (endHtmlIndex > htmlDataString.Length)
            {
                endHtmlIndex = htmlDataString.Length;
            }

            return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
        }

        /// <summary>
        /// Adds Xhtml header information to Html data string so that it can be placed on clipboard
        /// </summary>
        /// <param name="htmlString">
        /// Html string to be placed on clipboard with appropriate header
        /// </param>
        /// <returns>
        /// String wrapping htmlString with appropriate Html header
        /// </returns>
        internal static string AddHtmlClipboardHeader(string htmlString)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // each of 6 numbers is represented by "{0:D10}" in the format string
            // must actually occupy 10 digit positions ("0123456789")
            int startHTML = HtmlHeader.Length + 6 * ("0123456789".Length - "{0:D10}".Length);
            int endHTML = startHTML + htmlString.Length;
            int startFragment = htmlString.IndexOf(HtmlStartFragmentComment, 0);
            if (startFragment >= 0)
            {
                startFragment = startHTML + startFragment + HtmlStartFragmentComment.Length;
            }
            else
            {
                startFragment = startHTML;
            }
            int endFragment = htmlString.IndexOf(HtmlEndFragmentComment, 0);
            if (endFragment >= 0)
            {
                endFragment = startHTML + endFragment;
            }
            else
            {
                endFragment = endHTML;
            }

            // Create HTML clipboard header string
            stringBuilder.AppendFormat(HtmlHeader, startHTML, endHTML, startFragment, endFragment, startFragment, endFragment);

            // Append HTML body.
            stringBuilder.Append(htmlString);

            return stringBuilder.ToString();
        }

        #endregion Internal Methods

        // ---------------------------------------------------------------------
        //
        // Private methods
        //
        // ---------------------------------------------------------------------

        #region Private Methods

        private void InvariantAssert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion error: " + message);
            }
        }

        /// <summary>
        /// Parses the stream of html tokens starting
        /// from the name of top-level element.
        /// Returns XmlElement representing the top-level
        /// html element
        /// </summary>
        private XmlElement ParseHtmlContent()
        {
            // Create artificial root elelemt to be able to group multiple top-level elements
            // We create "html" element which may be a duplicate of real HTML element, which is ok, as HtmlConverter will swallow it painlessly..
            XmlElement htmlRootElement = _document.CreateElement("html", XhtmlNamespace);
            OpenStructuringElement(htmlRootElement);

            while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF)
            {
                if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.OpeningTagStart)
                {
                    _htmlLexicalAnalyzer.GetNextTagToken();
                    if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                    {
                        string htmlElementName = _htmlLexicalAnalyzer.NextToken.ToLower();
                        _htmlLexicalAnalyzer.GetNextTagToken();

                        // Create an element
                        XmlElement htmlElement = _document.CreateElement(htmlElementName, XhtmlNamespace);

                        // Parse element attributes
                        ParseAttributes(htmlElement);

                        if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.EmptyTagEnd || HtmlSchema.IsEmptyElement(htmlElementName))
                        {
                            // It is an element without content (because of explicit slash or based on implicit knowledge aboout html)
                            AddEmptyElement(htmlElement);
                        }
                        else if (HtmlSchema.IsInlineElement(htmlElementName))
                        {
                            // Elements known as formatting are pushed to some special
                            // pending stack, which allows them to be transferred
                            // over block tags - by doing this we convert
                            // overlapping tags into normal heirarchical element structure.
                            OpenInlineElement(htmlElement);
                        }
                        else if (HtmlSchema.IsBlockElement(htmlElementName) || HtmlSchema.IsKnownOpenableElement(htmlElementName))
                        {
                            // This includes no-scope elements
                            OpenStructuringElement(htmlElement);
                        }
                        else
                        {
                            // Do nothing. Skip the whole opening tag.
                            // Ignoring all unknown elements on their start tags.
                            // Thus we will ignore them on closinng tag as well.
                            // Anyway we don't know what to do withthem on conversion to Xaml.
                        }
                    }
                    else
                    {
                        // Otherwise - we skip the angle bracket and continue parsing the content as if it is just text.
                        //  Add the following asserion here, right? or output "<" as a text run instead?:
                        // InvariantAssert(false, "Angle bracket without a following name is not expected");
                    }
                }
                else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.ClosingTagStart)
                {
                    _htmlLexicalAnalyzer.GetNextTagToken();
                    if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                    {
                        string htmlElementName = _htmlLexicalAnalyzer.NextToken.ToLower();

                        // Skip the name token. Assume that the following token is end of tag,
                        // but do not check this. If it is not true, we simply ignore one token
                        // - this is our recovery from bad xml in this case.
                        _htmlLexicalAnalyzer.GetNextTagToken();

                        CloseElement(htmlElementName);
                    }
                }
                else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Text)
                {
                    AddTextContent(_htmlLexicalAnalyzer.NextToken);
                }
                else if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Comment)
                {
                    AddComment(_htmlLexicalAnalyzer.NextToken);
                }

                _htmlLexicalAnalyzer.GetNextContentToken();
            }

            // Get rid of the artificial root element
            if (htmlRootElement.FirstChild is XmlElement &&
                htmlRootElement.FirstChild == htmlRootElement.LastChild &&
                htmlRootElement.FirstChild.LocalName.ToLower() == "html")
            {
                htmlRootElement = (XmlElement)htmlRootElement.FirstChild;
            }

            return htmlRootElement;
        }

        private XmlElement CreateElementCopy(XmlElement htmlElement)
        {
            XmlElement htmlElementCopy = _document.CreateElement(htmlElement.LocalName, XhtmlNamespace);
            for (int i = 0; i < htmlElement.Attributes.Count; i++)
            {
                XmlAttribute attribute = htmlElement.Attributes[i];
                htmlElementCopy.SetAttribute(attribute.Name, attribute.Value);
            }
            return htmlElementCopy;
        }

        private void AddEmptyElement(XmlElement htmlEmptyElement)
        {
            Contract.Requires(htmlEmptyElement != null);
            InvariantAssert(_openedElements.Count > 0, "AddEmptyElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
            XmlElement htmlParent = _openedElements.Peek();
            htmlParent.AppendChild(htmlEmptyElement);
        }

        private void OpenInlineElement(XmlElement htmlInlineElement)
        {
            _pendingInlineElements.Push(htmlInlineElement);
        }

        // Opens structurig element such as Div or Table etc.
        private void OpenStructuringElement(XmlElement htmlElement)
        {
            // Close all pending inline elements
            // All block elements are considered as delimiters for inline elements
            // which forces all inline elements to be closed and re-opened in the following
            // structural element (if any).
            // By doing that we guarantee that all inline elements appear only within most nested blocks
            if (HtmlSchema.IsBlockElement(htmlElement.LocalName))
            {
                while (_openedElements.Count > 0 && HtmlSchema.IsInlineElement(_openedElements.Peek().LocalName))
                {
                    XmlElement htmlInlineElement = _openedElements.Pop();
                    InvariantAssert(_openedElements.Count > 0, "OpenStructuringElement: stack of opened elements cannot become empty here");

                    _pendingInlineElements.Push(CreateElementCopy(htmlInlineElement));
                }
            }

            // Add this block element to its parent
            if (_openedElements.Count > 0)
            {
                XmlElement htmlParent = _openedElements.Peek();

                // Check some known block elements for auto-closing (LI and P)
                if (HtmlSchema.ClosesOnNextElementStart(htmlParent.LocalName, htmlElement.LocalName))
                {
                    _openedElements.Pop();
                    htmlParent = _openedElements.Count > 0 ? _openedElements.Peek() : null;
                }

                if (htmlParent != null)
                {
                    // Actually we never expect null - it would mean two top-level P or LI (without a parent).
                    // In such weird case we will loose all paragraphs except the first one...
                    htmlParent.AppendChild(htmlElement);
                }
            }

            // Push it onto a stack
            _openedElements.Push(htmlElement);
        }

        private bool IsElementOpened(string htmlElementName)
        {
            foreach (XmlElement openedElement in _openedElements)
            {
                if (openedElement.LocalName == htmlElementName)
                {
                    return true;
                }
            }
            return false;
        }

        private void CloseElement(string htmlElementName)
        {
            // Check if the element is opened and already added to the parent
            InvariantAssert(_openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            // Check if the element is opened and still waiting to be added to the parent
            if (_pendingInlineElements.Count > 0 && _pendingInlineElements.Peek().LocalName == htmlElementName)
            {
                // Closing an empty inline element.
                XmlElement htmlInlineElement = _pendingInlineElements.Pop();
                InvariantAssert(_openedElements.Count > 0, "CloseElement: Stack of opened elements cannot be empty, as we have at least one artificial root element");
                XmlElement htmlParent = _openedElements.Peek();
                htmlParent.AppendChild(htmlInlineElement);
                return;
            }
            else if (IsElementOpened(htmlElementName))
            {
                while (_openedElements.Count > 1) // we never pop the last element - the artificial root
                {
                    // Close all unbalanced elements.
                    XmlElement htmlOpenedElement = _openedElements.Pop();

                    if (htmlOpenedElement.LocalName == htmlElementName)
                    {
                        return;
                    }

                    if (HtmlSchema.IsInlineElement(htmlOpenedElement.LocalName))
                    {
                        // Unbalances Inlines will be transfered to the next element content
                        _pendingInlineElements.Push(CreateElementCopy(htmlOpenedElement));
                    }
                }
            }

            // If element was not opened, we simply ignore the unbalanced closing tag
            return;
        }

        private void AddTextContent(string textContent)
        {
            OpenPendingInlineElements();

            InvariantAssert(_openedElements.Count > 0, "AddTextContent: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            XmlElement htmlParent = _openedElements.Peek();
            XmlText textNode = _document.CreateTextNode(textContent);
            htmlParent.AppendChild(textNode);
        }

        private void AddComment(string comment)
        {
            OpenPendingInlineElements();

            InvariantAssert(_openedElements.Count > 0, "AddComment: Stack of opened elements cannot be empty, as we have at least one artificial root element");

            XmlElement htmlParent = _openedElements.Peek();
            XmlComment xmlComment = _document.CreateComment(comment);
            htmlParent.AppendChild(xmlComment);
        }

        // Moves all inline elements pending for opening to actual document
        // and adds them to current open stack.
        private void OpenPendingInlineElements()
        {
            if (_pendingInlineElements.Count > 0)
            {
                XmlElement htmlInlineElement = _pendingInlineElements.Pop();

                OpenPendingInlineElements();

                InvariantAssert(_openedElements.Count > 0, "OpenPendingInlineElements: Stack of opened elements cannot be empty, as we have at least one artificial root element");

                XmlElement htmlParent = _openedElements.Peek();
                htmlParent.AppendChild(htmlInlineElement);
                _openedElements.Push(htmlInlineElement);
            }
        }

        private void ParseAttributes(XmlElement xmlElement)
        {
            while (_htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EOF && //
                _htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.TagEnd && //
                _htmlLexicalAnalyzer.NextTokenType != HtmlTokenType.EmptyTagEnd)
            {
                // read next attribute (name=value)
                if (_htmlLexicalAnalyzer.NextTokenType == HtmlTokenType.Name)
                {
                    string attributeName = _htmlLexicalAnalyzer.NextToken;
                    _htmlLexicalAnalyzer.GetNextEqualSignToken();

                    _htmlLexicalAnalyzer.GetNextAtomToken();

                    string attributeValue = _htmlLexicalAnalyzer.NextToken;
                    xmlElement.SetAttribute(attributeName, attributeValue);
                }
                _htmlLexicalAnalyzer.GetNextTagToken();
            }
        }

        #endregion Private Methods


        // ---------------------------------------------------------------------
        //
        // Private Fields
        //
        // ---------------------------------------------------------------------

        #region Private Fields

        internal const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

        private HtmlLexicalAnalyzer _htmlLexicalAnalyzer;

        // document from which all elements are created
        private XmlDocument _document;

        // stack for open elements
        Stack<XmlElement> _openedElements;
        Stack<XmlElement> _pendingInlineElements;

        #endregion Private Fields
    }
}
