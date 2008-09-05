using System;
using System.Xml;
using System.Web;

using CR_Documentor.Reflector;
using CR_Documentor.Transformation.Syntax;
using CR_Documentor.Xml;

using SP = DevExpress.CodeRush.StructuralParser;

namespace CR_Documentor.Transformation.SandcastlePrototype
{
	/// <summary>
	/// Renders documentation in the Sandcastle "Prototype" document style.
	/// </summary>
	public class Engine : TransformEngine
	{
		#region Engine Variables

		/// <summary>
		/// Cached copy of the base HTML so it doesn't get retrieved repeatedly.
		/// </summary>
		private string _baseHtml = null;

		/// <summary>
		/// Path to the embedded resource containing the base HTML document for this transform engine.
		/// </summary>
		protected const string ResourceBaseHtmlDocument = "CR_Documentor.Transformation.SandcastlePrototype.BaseDocument.html";

		#endregion



		#region Engine Implementation

		#region Overrides

		/// <summary>
		/// Gets the base HTML document for the MSDN transform engine.
		/// </summary>
		/// <returns>The complete HTML required for initializing a browser to use the Sandcastle "prototype" transform engine.</returns>
		protected override string GetHtmlPageTemplate()
		{
			// TODO: Figure out how to handle images/assets that appear in the HTML. Extract to a temporary location?
			// TODO: When images/assets are available, pull the CSS and scripts out of the base HTML document.
			if (this._baseHtml == null)
			{
				// Lazy-initialize the base HTML. Doesn't matter if it's thread-safe.
				this._baseHtml = EmbeddedResource.ReadEmbeddedResourceString(System.Reflection.Assembly.GetExecutingAssembly(), ResourceBaseHtmlDocument);
			}
			return this._baseHtml;
		}

		/// <summary>
		/// Registers all tag handlers with the rendering event system.
		/// </summary>
		/// <seealso cref="CR_Documentor.Transformation.TransformEngine.RegisterCommentTagHandlers"/>
		protected override void RegisterCommentTagHandlers()
		{
			this.AddCommentTagHandler(DefaultCommentHandlerKey, new EventHandler<CommentMatchEventArgs>(this.HtmlPassThrough));
			this.AddCommentTagHandler("c", new EventHandler<CommentMatchEventArgs>(this.C));
			this.AddCommentTagHandler("code", new EventHandler<CommentMatchEventArgs>(this.Code));
			this.AddCommentTagHandler("example", new EventHandler<CommentMatchEventArgs>(this.ApplyTemplates));
			this.AddCommentTagHandler("exclude", new EventHandler<CommentMatchEventArgs>(this.IgnoreComment));
			this.AddCommentTagHandler("exception", new EventHandler<CommentMatchEventArgs>(this.Exception));
			this.AddCommentTagHandler("include", new EventHandler<CommentMatchEventArgs>(this.Include));
			this.AddCommentTagHandler("list", new EventHandler<CommentMatchEventArgs>(this.List));
			this.AddCommentTagHandler("member", new EventHandler<CommentMatchEventArgs>(this.Member));
			this.AddCommentTagHandler("note", new EventHandler<CommentMatchEventArgs>(this.Note));
			this.AddCommentTagHandler("overloads", new EventHandler<CommentMatchEventArgs>(this.IgnoreComment));
			this.AddCommentTagHandler("para", new EventHandler<CommentMatchEventArgs>(this.Para));
			this.AddCommentTagHandler("param", new EventHandler<CommentMatchEventArgs>(this.Param));
			this.AddCommentTagHandler("paramref", new EventHandler<CommentMatchEventArgs>(this.Paramref));
			this.AddCommentTagHandler("permission", new EventHandler<CommentMatchEventArgs>(this.Permission));
			this.AddCommentTagHandler("preliminary", new EventHandler<CommentMatchEventArgs>(this.Preliminary));
			this.AddCommentTagHandler("remarks", new EventHandler<CommentMatchEventArgs>(this.Remarks));
			this.AddCommentTagHandler("returns", new EventHandler<CommentMatchEventArgs>(this.Returns));
			this.AddCommentTagHandler("see", new EventHandler<CommentMatchEventArgs>(this.See));
			this.AddCommentTagHandler("seealso", new EventHandler<CommentMatchEventArgs>(this.SeeAlso));
			this.AddCommentTagHandler("summary", new EventHandler<CommentMatchEventArgs>(this.Summary));
			this.AddCommentTagHandler("threadsafety", new EventHandler<CommentMatchEventArgs>(this.ThreadSafety));
			this.AddCommentTagHandler("typeparam", new EventHandler<CommentMatchEventArgs>(this.TypeParam));
			this.AddCommentTagHandler("typeparamref", new EventHandler<CommentMatchEventArgs>(this.TypeParamref));
			this.AddCommentTagHandler("value", new EventHandler<CommentMatchEventArgs>(this.Value));
		}

		#endregion

		#region Methods

		#region Tags

		/// <summary>
		/// Matches the 'c' tag.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void C(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<span class=\"code\">");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</span>");
		}

		/// <summary>
		/// Matches and processes a 'code' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Code(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<div class=\"code\"><pre>");
			// Sandcastle doesn't support the lang attribute, the escaped attribute, or converting tabs to spaces
			this.Writer.Write(HttpUtility.HtmlEncode(e.Element.InnerText));
			this.Writer.Write("</pre></div>");
		}

		/// <summary>
		/// Matches and processes an 'exception' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Exception(object sender, CommentMatchEventArgs e)
		{
			this.StandardTableRow(e.Element);
		}

		/// <summary>
		/// Passes through HTML comment tags.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void HtmlPassThrough(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<");
			this.Writer.Write(e.Element.Name);
			TextProcessor.AttributePassThrough(this.Writer, e.Element);
			if (e.Element.InnerXml == "")
			{
				// If the tag is empty (like "<br />") just close it
				// Otherwise we get odd things like "<br><br>"
				this.Writer.Write("/>");
			}
			else
			{
				this.Writer.Write(">");
				this.ApplyTemplates(e.Element);
				this.Writer.Write("</" + e.Element.Name + ">");
			}
		}

		/// <summary>
		/// Matches and processes an 'include' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Include(object sender, CommentMatchEventArgs e)
		{
			// If includes haven't been processed out, put a placeholder
			this.Writer.Write("<p><i><b>[Insert documentation here: file = ");
			this.Writer.Write(Evaluator.ValueOf(e.Element, "@file"));
			this.Writer.Write(", path = ");
			this.Writer.Write(Evaluator.ValueOf(e.Element, "@path"));
			this.Writer.Write("]</b></i></p>");
		}

		/// <summary>
		/// Matches and processes a 'list' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void List(object sender, CommentMatchEventArgs e)
		{
			switch (Evaluator.ValueOf(e.Element, "@type"))
			{
				case "table":
					this.Writer.Write("<table class=\"authoredTable\">");

					foreach (XmlNode listHeader in e.Element.SelectNodes("listheader"))
					{
						this.Writer.Write("<tr>");
						foreach (XmlNode column in listHeader.ChildNodes)
						{
							if (!(column is XmlElement))
							{
								continue;
							}
							this.Writer.Write("<th>");
							this.ApplyTemplates(column.ChildNodes);
							this.Writer.Write("</th>");
						}
						this.Writer.Write("</tr>");
					}

					foreach (XmlNode item in e.Element.SelectNodes("item"))
					{
						this.Writer.Write("<tr>");
						foreach (XmlNode column in item.ChildNodes)
						{
							if (!(column is XmlElement))
							{
								continue;
							}
							this.Writer.Write("<td>");
							this.ApplyTemplates(column.ChildNodes);
							this.Writer.Write("<br/></td>");
						}
						this.Writer.Write("</tr>");
					}

					this.Writer.Write("</table>");
					break;

				case "bullet":
					this.Writer.Write("<ul>");
					foreach (XmlNode item in e.Element.SelectNodes("item"))
					{
						this.Writer.Write("<li>");
						foreach (XmlNode itemChild in item.ChildNodes)
						{
							this.ApplyTemplates(itemChild);
						}
						this.Writer.Write("</li>");
					}
					this.Writer.Write("</ul>");
					break;

				case "number":
					this.Writer.Write("<ol>");
					foreach (XmlNode item in e.Element.SelectNodes("item"))
					{
						this.Writer.Write("<li>");
						foreach (XmlNode itemChild in item.ChildNodes)
						{
							this.ApplyTemplates(itemChild);
						}
						this.Writer.Write("</li>");
					}
					this.Writer.Write("</ol>");
					break;
				default:
					this.Writer.Write(e.Element.InnerText);
					break;
			}
		}

		/// <summary>
		/// Matches and processes the root 'member' element in a document.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Member(object sender, CommentMatchEventArgs e)
		{
			// Note that Sandcastle doesn't process out redundant "see" links

			// Stick the banner at the top
			this.Banner();

			// Open the "main" (content) div.
			this.Writer.Write("<div id=\"main\"><div id=\"header\">This is experimental documentation.</div>");

			// Errors (at top level)
			this.ApplyTemplates(CommentParser.GetChildErrorNodes(e.Element));

			// Preliminary
			this.ApplyTemplates(e.Element, "preliminary");

			// Summary
			this.ApplyTemplates(e.Element, "summary");

			// Syntax
			this.Syntax();

			// Type Parameters
			if (this.Options.RecognizedTags.Contains("typeparam") && e.Element.GetElementsByTagName("typeparam").Count != 0)
			{
				this.SectionOpen("Generic Template Parameters");
				this.Writer.Write("<dl>");
				this.ApplyTemplates(e.Element, "typeparam");
				this.Writer.Write("</dl>");
				this.SectionClose();
			}

			// Parameters
			if (this.Options.RecognizedTags.Contains("param") && e.Element.GetElementsByTagName("param").Count != 0)
			{
				this.SectionOpen("Parameters");
				this.Writer.Write("<dl>");
				this.ApplyTemplates(e.Element, "param");
				this.Writer.Write("</dl>");
				this.SectionClose();
			}

			// Value
			this.ApplyTemplates(e.Element, "value");

			// Returns
			this.ApplyTemplates(e.Element, "returns");

			// Members (Applies to type declarations including enum values)
			this.Members();

			// Remarks
			this.ApplyTemplates(e.Element, "remarks");

			// Threadsafety
			this.ApplyTemplates(e.Element, "threadsafety");

			// Example
			if (this.Options.RecognizedTags.Contains("example") && e.Element.GetElementsByTagName("example").Count != 0)
			{
				this.SectionOpen("Examples");
				this.ApplyTemplates(e.Element, "example");
				this.SectionClose();
			}

			// Permissions
			if (this.Options.RecognizedTags.Contains("permission") && e.Element.GetElementsByTagName("permission").Count != 0)
			{
				this.SectionOpen("Permissions");
				this.Writer.Write("<table class=\"permissions\"><tr><th class=\"permissionNameColumn\">Permission</th><th class=\"permissionDescriptionColumn\">Description</th></tr>");
				this.ApplyTemplates(e.Element, "permission");
				this.Writer.Write("</table>");
				this.SectionClose();
			}

			// Exceptions
			if (this.Options.RecognizedTags.Contains("exception") && e.Element.GetElementsByTagName("exception").Count != 0)
			{
				this.SectionOpen("Exceptions");
				this.Writer.Write("<table class=\"exceptions\"><tr><th class=\"exceptionNameColumn\">Exception</th><th class=\"exceptionConditionColumn\">Condition</th></tr>");
				this.ApplyTemplates(e.Element, "exception");
				this.Writer.Write("</table>");
				this.SectionClose();
			}

			// Usually classes would display "Inheritance Hierarchy" information here, like:
			// Object
			// +-SomeClass
			//   +-SomeDerivedClass
			// But that doesn't really add anything to a preview, so we'll omit it.

			// SeeAlso
			if (this.Options.RecognizedTags.Contains("seealso") && e.Element.GetElementsByTagName("seealso").Count != 0)
			{
				this.SectionOpen("See Also");
				this.ApplyTemplates(e.Element, "seealso");
				this.SectionClose();
			}

			// Usually "Assembly" information will be shown here, like:
			// Assembly: Foo (Module: Bar)
			// But that doesn't really add anything to a preview, so we'll omit it.

			// Close the "main" div.
			this.Writer.Write("</div>");
		}

		/// <summary>
		/// Matches and processes a 'note' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Note(object sender, CommentMatchEventArgs e)
		{
			// TODO: When images/assets are available, put the note image in.
			// <div class="alert"><img src="../icons/alert_note.gif" /> <b>Note:</b>

			// Sandcastle doesn't support note "types" (inheritinfo, etc.). It's all just "note."
			this.Writer.Write("<div class=\"alert\"> <b>Note:</b>");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</div>");
		}

		/// <summary>
		/// Matches and processes a 'para' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Para(object sender, CommentMatchEventArgs e)
		{
			// Sandcastle doesn't support passing through style or align attributes.
			// Sandcastle doesn't support the lang attribute.
			this.Writer.Write("<p>");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</p>");
		}

		/// <summary>
		/// Matches and processes a 'param' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Param(object sender, CommentMatchEventArgs e)
		{
			string paramName = Evaluator.ValueOf(e.Element, "@name");
			string paramType = this.ParamType(paramName);
			this.Writer.Write("<dt><span class=\"parameter\">");
			this.Writer.Write(paramName);
			if (paramType != null)
			{
				this.Writer.Write(" (<a href=\"#\">");
				this.Writer.Write(paramType);
				this.Writer.Write("</a>)");
			}
			this.Writer.Write("</span>");
			this.Writer.Write("</dt>");
			this.Writer.Write("<dd>");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</dd>");
		}

		/// <summary>
		/// Matches and processes a 'paramref' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Paramref(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<span class=\"parameter\">");
			this.Writer.Write(Evaluator.ValueOf(e.Element, "@name"));
			this.Writer.Write("</span>");
		}

		/// <summary>
		/// Matches and processes a 'permission' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Permission(object sender, CommentMatchEventArgs e)
		{
			this.StandardTableRow(e.Element);
		}

		/// <summary>
		/// Matches and processes a 'preliminary' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Preliminary(object sender, CommentMatchEventArgs e)
		{
			// December 2006 CTP does not pay attention to specified text for the preliminary message.
			this.Writer.Write("<div class=\"preliminary\">This API is preliminary and subject to change.</div>");
		}

		/// <summary>
		/// Matches and processes a 'remarks' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Remarks(object sender, CommentMatchEventArgs e)
		{
			this.SectionOpen("Remarks");
			this.ApplyTemplates(e.Element);
			this.SectionClose();
		}

		/// <summary>
		/// Matches and processes a 'returns' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Returns(object sender, CommentMatchEventArgs e)
		{
			this.SectionOpen("Return Value");
			this.ApplyTemplates(e.Element);
			this.SectionClose();
		}

		/// <summary>
		/// Matches and processes a 'see' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void See(object sender, CommentMatchEventArgs e)
		{
			if (Evaluator.Test(e.Element, "@langword"))
			{
				string langword = Evaluator.ValueOf(e.Element, "@langword");
				this.Writer.Write("<span class=\"keyword\">");
				switch (langword)
				{
					case "null":
					case "Nothing":
					case "nullptr":
						this.Writer.Write("<span class=\"cs\">null</span><span class=\"vb\">Nothing</span><span class=\"cpp\">nullptr</span>");
						break;
					case "static":
					case "Shared":
						this.Writer.Write("<span class=\"cs\">static</span><span class=\"vb\">Shared</span><span class=\"cpp\">static</span>");
						break;
					case "virtual":
					case "Overridable":
						this.Writer.Write("<span class=\"cs\">virtual</span><span class=\"vb\">Overridable</span><span class=\"cpp\">virtual</span>");
						break;
					case "True":
					case "true":
						this.Writer.Write("<span class=\"cs\">true</span><span class=\"vb\">True</span><span class=\"cpp\">true</span>");
						break;
					case "false":
					case "False":
						this.Writer.Write("<span class=\"cs\">false</span><span class=\"vb\">False</span><span class=\"cpp\">false</span>");
						break;
					case "abstract":
					case "MustInherit":
						this.Writer.Write("<span class=\"cs\">abstract</span><span class=\"vb\">MustInherit</span><span class=\"cpp\">abstract</span>");
						break;
					default:
						this.Writer.Write(langword);
						break;
				}
				this.Writer.Write("</span>");
			}
			else if (Evaluator.Test(e.Element, "@cref"))
			{
				string cref = Evaluator.ValueOf(e.Element, "@cref");
				string text = e.Element.InnerText;
				if (!String.IsNullOrEmpty(text))
				{
					text = HttpUtility.HtmlEncode(text);
				}
				else
				{
					text = MemberKey.GetName(cref);
					// This handles the type parameters in the member name.
					text = text.Replace("{", "<span class=\"cs\">&lt;</span><span class=\"vb\">(Of </span><span class=\"cpp\">&lt;</span>");
					text = text.Replace("}", "<span class=\"cs\">&gt;</span><span class=\"vb\">)</span><span class=\"cpp\">&gt;</span>");
				}
				this.Writer.Write("<a href=\"urn:member:");
				this.Writer.Write(cref);
				this.Writer.Write("\">");
				this.Writer.Write(text);
				this.Writer.Write("</a>");
			}
			else
			{
				this.Writer.Write(HttpUtility.HtmlEncode(e.Element.InnerText));
			}
		}

		/// <summary>
		/// Matches and processes a 'seealso' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void SeeAlso(object sender, CommentMatchEventArgs e)
		{
			string linkDestination = null;
			string text = e.Element.InnerText;
			if (Evaluator.Test(e.Element, "@cref"))
			{
				linkDestination = "urn:member:" + Evaluator.ValueOf(e.Element, "@cref");
				if (text != "")
				{
					text = HttpUtility.HtmlEncode(text);
				}
				else
				{
					text = MemberKey.GetName(Evaluator.ValueOf(e.Element, "@cref"));
					// This handles the type parameters in the member name.
					text = text.Replace("{", "<span class=\"cs\">&lt;</span><span class=\"vb\">(Of </span><span class=\"cpp\">&lt;</span>");
					text = text.Replace("}", "<span class=\"cs\">&gt;</span><span class=\"vb\">)</span><span class=\"cpp\">&gt;</span>");
				}
			}

			if (linkDestination != null)
			{
				this.Writer.Write("<a href=\"");
				this.Writer.Write(linkDestination);
				this.Writer.Write("\">");
			}
			else
			{
				this.Writer.Write("<span class=\"nolink\">");
			}

			this.Writer.Write(text);

			if (linkDestination != null)
			{
				this.Writer.Write("</a>");
			}
			else
			{
				this.Writer.Write("</span>");
			}

			this.Writer.Write("<br />");
		}

		/// <summary>
		/// Matches and processes a 'summary' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Summary(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<div class=\"summary\">");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</div>");
		}

		/// <summary>
		/// Matches and processes a 'remarks' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void ThreadSafety(object sender, CommentMatchEventArgs e)
		{
			this.SectionOpen("Thread Safety");

			bool staticSafe = false;
			bool instanceSafe = false;
			Boolean.TryParse(Evaluator.ValueOf(e.Element, "@static"), out staticSafe);
			Boolean.TryParse(Evaluator.ValueOf(e.Element, "@instance"), out instanceSafe);

			this.Writer.Write("Static members of this type are {0}safe for multi-threaded operations. ", staticSafe ? "" : "not ");
			this.Writer.Write("Instance members of this type are {0}safe for multi-threaded operations. ", instanceSafe ? "" : "not ");

			this.SectionClose();
		}

		/// <summary>
		/// Matches and processes a 'typeparam' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void TypeParam(object sender, CommentMatchEventArgs e)
		{
			string paramName = Evaluator.ValueOf(e.Element, "@name");
			this.Writer.Write("<dt><span class=\"parameter\">");
			this.Writer.Write(paramName);
			this.Writer.Write("</span>");
			this.Writer.Write("</dt>");
			this.Writer.Write("<dd>");
			this.ApplyTemplates(e.Element);
			this.Writer.Write("</dd>");
		}

		/// <summary>
		/// Matches and processes a 'typeparamref' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void TypeParamref(object sender, CommentMatchEventArgs e)
		{
			this.Writer.Write("<span class=\"typeparameter\">");
			this.Writer.Write(Evaluator.ValueOf(e.Element, "@name"));
			this.Writer.Write("</span>");
		}

		/// <summary>
		/// Matches and processes a 'value' element.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CR_Documentor.Transformation.CommentMatchEventArgs"/> instance containing the event data.</param>
		private void Value(object sender, CommentMatchEventArgs e)
		{
			this.SectionOpen("Value");
			this.ApplyTemplates(e.Element);
			this.SectionClose();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Produces the banner at the top of the preview
		/// </summary>
		private void Banner()
		{
			this.Writer.Write("<script type=\"text/javascript\">registerEventHandler(window, 'load', function() { var ss = new SplitScreen('control', 'main'); });</script>");
			this.Writer.Write("<div id=\"control\">");
			this.Writer.Write("<span class=\"productTitle\">Reference Library</span><br />");

			// Output the member description/title
			this.Writer.Write("<span class=\"topicTitle\">");
			SP.AccessSpecifiedElement el = this.CodeTargetToRender;
			if (el == null)
			{
				// The element can't be documented; put an empty title
				this.Writer.Write("&nbsp;");
			}
			else
			{
				// The first part of the header is the name of the element
				if (el is SP.Method)
				{
					this.Writer.Write(Lookup.MethodName((SP.Method)el));
				}
				else
				{
					this.Writer.Write(el.Name);
				}
				this.Writer.Write(" ");

				// The second part of the header is the type of element
				this.Writer.Write(Lookup.ElementTypeDescription(el));
			}
			this.Writer.Write("</span><br />"); // topicTitle

			// Output the 'toolbar'
			this.Writer.Write("<div id=\"toolbar\">");
			this.Writer.Write("<span id=\"chickenFeet\">");
			if (el == null)
			{
				this.Writer.Write("&nbsp;");
			}
			else
			{
				this.Writer.Write("<a href=\"#\">Namespaces</a>");
				SP.Namespace memberNamespace = el.GetNamespace();
				if (memberNamespace != null)
				{
					this.Writer.Write(" &#x25ba; <a href=\"#\">");
					this.Writer.Write(memberNamespace.FullName);
					this.Writer.Write("</a> ");
				}
				this.Writer.Write("&#x25ba; ");
				if (el is SP.Member)
				{
					// Write the parent's name before the item name
					this.Writer.Write("<a href=\"#\">");
					this.Writer.Write(el.Parent.Name);
					this.Writer.Write("</a> &#x25ba; ");
				}
				this.Writer.Write("<span class=\"nolink\">");
				this.Writer.Write(el.Name);
				this.Writer.Write("</span>"); // nolink
			}
			this.Writer.Write("</span>"); // chickenFeet
			this.Writer.Write("<span id=\"languageFilter\">");
			this.Writer.Write("<select id=\"languageSelector\">");
			if (el == null)
			{
				this.Writer.Write("<option value=\"x\">--</option>");
			}
			else
			{
				string languageOption = "x";
				switch (el.Document.Language)
				{
					case Language.Basic:
						languageOption = "VisualBasic vb";
						break;
					case Language.C:
						languageOption = "ManagedCPlusPlus cpp";
						break;
					case Language.CSharp:
						languageOption = "CSharp cs";
						break;
				}
				this.Writer.Write("<option value=\"");
				this.Writer.Write(languageOption);
				this.Writer.Write("\">");
				this.Writer.Write(HttpUtility.HtmlEncode(Lookup.LanguageName[el.Document.Language]));
				this.Writer.Write("</option>");
			}
			this.Writer.Write("</select>");
			this.Writer.Write("</span>"); // languageFilter
			this.Writer.Write("</div>"); // toolbar
			this.Writer.Write("</div>"); // control
		}

		/// <summary>
		/// Writes out the "Members" section for type declarations.
		/// </summary>
		private void Members()
		{
			SP.Class targetClass = this.CodeTarget as SP.Class;
			SP.Enumeration targetEnum = this.CodeTarget as SP.Enumeration;
			if ((targetClass == null && targetEnum == null) || this.CodeTarget.NodeCount < 1)
			{
				return;
			}
			this.SectionOpen("Members");
			if (targetClass != null)
			{
				this.Writer.Write("<table class=\"filter\"><tr class=\"tabs\" id=\"memberTabs\">");
				this.Writer.Write("<td class=\"tab\" value=\"all\">All Members</td><td class=\"tab\" value=\"constructor\">Constructors</td><td class=\"tab\" value=\"method\">Methods</td><td class=\"tab\" value=\"property\">Properties</td><td class=\"tab\" value=\"field\">Fields</td><td class=\"tab\" value=\"event\">Events</td>");
				this.Writer.Write("</tr><tr>");
				this.Writer.Write("<td class=\"line\" colspan=\"2\"><label for=\"public\"><input id=\"public\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Public</label><br /><label for=\"protected\"><input id=\"protected\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Protected</label></td>");
				this.Writer.Write("<td class=\"line\" colspan=\"2\"><label for=\"instance\"><input id=\"instance\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Instance</label><br /><label for=\"static\"><input id=\"static\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Static</label></td>");
				this.Writer.Write("<td class=\"line\" colspan=\"2\"><label for=\"declared\"><input id=\"declared\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Declared</label><br /><label for=\"inherited\"><input id=\"inherited\" type=\"checkbox\" checked=\"true\" disabled=\"true\" />Inherited</label></td>");
				this.Writer.Write("</tr></table>");
			}
			this.Writer.Write("<table class=\"members\" id=\"memberList\"><tr>");
			if (targetClass != null)
			{
				this.Writer.Write("<th class=\"iconColumn\">Icon</th>");
			}
			this.Writer.Write("<th class=\"nameColumn\">Member</th><th class=\"descriptionColumn\">Description</th></tr>");
			if (targetEnum != null)
			{
				// Write out the enumeration members
				foreach (object node in targetEnum.Nodes)
				{
					SP.EnumElement element = node as SP.EnumElement;
					if (element == null)
					{
						continue;
					}
					this.Writer.Write("<tr><td><b><span class=\"selflink\">");
					this.Writer.Write(element.Name);
					this.Writer.Write("</span></b></td><td>");
					this.RenderElementSummary(element);
					this.Writer.Write("<br /></td></tr>");
				}
			}
			else if (targetClass != null)
			{
				foreach (SP.LanguageElement classMember in targetClass.AllMembers)
				{
					// TODO: When images/assets are available, put the member icon in.
					this.Writer.Write("<tr><td>&nbsp;</td><td><a href=\"#\">");
					this.Writer.Write(HttpUtility.HtmlEncode(classMember.Name));
					this.Writer.Write("</a></td><td>");
					this.RenderElementSummary(classMember);
					this.Writer.Write("</td></tr>");
				}
			}
			this.Writer.Write("</table>");
			this.SectionClose();
		}

		/// <summary>
		/// Retrieves the type name for a parameter on the current element being documented.
		/// </summary>
		/// <param name="paramName">The name of the parameter to look up the type for.</param>
		/// <returns>
		/// A <see cref="System.String"/> containing the parameter's type name,
		/// or <see langword="null" /> if not found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// If <paramref name="paramName" /> is <see langword="null" /> or
		/// <see cref="System.String.Empty"/>, this method returns <see langword="null" />.
		/// </para>
		/// <para>
		/// If the current code target (<see cref="CR_Documentor.Transformation.TransformEngine.CodeTargetToRender"/>)
		/// is not a <see cref="DevExpress.CodeRush.StructuralParser.MemberWithParameters"/>
		/// or if the code target has no parameters, this method returns <see langword="null" />.
		/// </para>
		/// </remarks>
		private string ParamType(string paramName)
		{
			if (paramName == null || paramName.Length == 0)
			{
				return null;
			}
			SP.MemberWithParameters mwpElement = this.CodeTargetToRender as SP.MemberWithParameters;
			if (mwpElement == null || mwpElement.ParameterCount == 0)
			{
				return null;
			}
			SP.LanguageElementCollection parameters = mwpElement.Parameters;
			foreach (SP.LanguageElement parameter in parameters)
			{
				if (parameter.Name == paramName)
				{
					return System.Web.HttpUtility.HtmlEncode(((SP.Param)parameter).ParamType);
				}
			}
			return null;
		}

		/// <summary>
		/// Opens a standard section block with a title.
		/// </summary>
		/// <param name="sectionTitle">The title of the section block.</param>
		private void SectionOpen(string sectionTitle)
		{
			this.Writer.Write("<div class=\"section\"><div class=\"sectionTitle\">");
			this.Writer.Write(sectionTitle);
			this.Writer.Write("</div><div class=\"sectionContent\">");
		}

		/// <summary>
		/// Closes a standard section block.
		/// </summary>
		private void SectionClose()
		{
			this.Writer.Write("</div></div>");
		}

		/// <summary>
		/// Writes a standard table row for an exception, event, or other standard table.
		/// </summary>
		/// <param name="element">The element containing the information to write.</param>
		private void StandardTableRow(XmlElement element)
		{
			this.Writer.Write("<tr><td><a href=\"urn:member:");
			this.Writer.Write(Evaluator.ValueOf(element, "@cref"));
			this.Writer.Write("\">");
			this.Writer.Write(MemberKey.GetName(Evaluator.ValueOf(element, "@cref")));
			this.Writer.Write("</a></td><td>");
			this.ApplyTemplates(element);
			this.Writer.Write("</td></tr>");
		}

		/// <summary>
		/// Produces the object signature preview.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the current code target (<see cref="CR_Documentor.Transformation.TransformEngine.CodeTargetToRender"/>)
		/// is not an <see cref="DevExpress.CodeRush.StructuralParser.AccessSpecifiedElement"/>,
		/// this method exits.
		/// </para>
		/// <para>
		/// Otherwise, the member syntax signature cache is checked.  If the cache is empty,
		/// the member syntax signature is generated.  If it's not, generation is skipped
		/// as the currently active member has already been "rendered."  The member syntax
		/// signature is then written to the output document.
		/// </para>
		/// </remarks>
		private void Syntax()
		{
			// If we don't have a target or if the target can't be documented, we
			// Everything that has a signature has an access specifier; don't try
			// to render anything that doesn't
			SP.AccessSpecifiedElement asElement = this.CodeTargetToRender;
			if (asElement == null)
			{
				return;
			}

			if (this.MemberSyntax == null)
			{
				// Refresh the member syntax signature cache if it's empty
				System.IO.StringWriter syntaxWriter = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
				SyntaxGenerator generator = new SyntaxGenerator(asElement, syntaxWriter);
				generator.GenerateSyntaxPreview();
				this.MemberSyntax = syntaxWriter.ToString();
			}
			this.SectionOpen("Declaration Syntax");
			this.Writer.Write(this.MemberSyntax);
			this.SectionClose();
		}

		#endregion

		#endregion

		#endregion

	}
}
