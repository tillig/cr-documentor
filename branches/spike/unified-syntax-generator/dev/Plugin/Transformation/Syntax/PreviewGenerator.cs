﻿using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.CodeRush.StructuralParser;
using System.IO;
using System.Web.UI;
using System.Web;
using CR_Documentor.Properties;

namespace CR_Documentor.Transformation.Syntax
{
	/// <summary>
	/// Syntax preview generator. Converts a langauge element into an HTML snippet
	/// that can be inserted into a documentation preview.
	/// </summary>
	/// <remarks>
	/// <para>
	/// CSS classes for common syntax generator:
	/// </para>
	/// <para>
	/// <c>.code</c> - wrapper around code blocks
	/// </para>
	/// <para>
	/// Language constructs:
	/// </para>
	/// <list type="bullet">
	/// <item><term>.attribute</term></item>
	/// <item><term>.attributes</term></item>
	/// <item><term>.comment</term></item>
	/// <item><term>.identifier</term></item>
	/// <item><term>.keyword</term></item>
	/// <item><term>.literal</term></item>
	/// <item><term>.member</term></item>
	/// <item><term>.parameter</term></item>
	/// <item><term>.parameters</term></item>
	/// <item><term>.typeparameter</term></item>
	/// <item><term>.typeparameters</term></item>
	/// </list>
	/// <para>
	/// There will be line continuations and line breaks in places. That may
	/// not quite be identical to how the finished product actually renders
	/// but there's not much we can do about that since we can't get CSS
	/// to add the special line-continuation characters in VB.
	/// </para>
	/// <para>
	/// Since we're not generating the preview for every language all at once,
	/// there is no need for CSS classes that are language-specific.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// For this method:
	/// </para>
	/// <code lang="C#">
	/// [SomeAttribute]
	/// [OtherAttribute(1, b=2)]
	/// public abstract string MyMethod&lt;T&gt;(out string a, T b)
	/// </code>
	/// <para>
	/// The corresponding HTML for a C# preview would be:
	/// </para>
	/// <code>
	/// &lt;div class=&quot;code&quot;&gt;
	///   &lt;div class=&quot;attributes&quot;&gt;
	///     &lt;div class=&quot;attribute&quot;&gt;[SomeAttribute]&lt;/div&gt;
	///     &lt;div class=&quot;attribute&quot;&gt;[OtherAttribute&lt;div class=&quot;parameters&quot;&gt;(&lt;div class=&quot;literal&quot;&gt;1&lt;/div&gt;, &lt;div class=&quot;parameter&quot;&gt;b&lt;/div&gt;=&lt;div class=&quot;literal&quot;&gt;2&lt;/div&gt;)&lt;/div&gt;]&lt;/div&gt;
	///   &lt;/div&gt;
	///   &lt;div class=&quot;member&quot;&gt;
	///     &lt;span class=&quot;keyword&quot;&gt;public&lt;span&gt;
	///     &lt;span class=&quot;keyword&quot;&gt;abstract&lt;/span&gt;
	///     &lt;span class=&quot;keyword&quot;&gt;&lt;a href=&quot;#&quot;&gt;string&lt;/a&gt;&lt;/span&gt;
	///     &lt;span class=&quot;identifier&quot;&gt;MyMethod&lt;/span&gt;&lt;div class=&quot;typeparameters&quot;&gt;&amp;lt;&lt;div class=&quot;typeparameter&quot;&gt;T&lt;/div&gt;&amp;gt;&lt;/div&gt;&lt;div class=&quot;parameters&quot;&gt;(&lt;br /&gt;
	///     &lt;div class=&quot;keyword&quot;&gt;out&lt;/div&gt; &lt;div class=&quot;keyword&quot;&gt;&lt;a href=&quot;#&quot;&gt;string&lt;/a&gt;&lt;/div&gt; &lt;div class=&quot;parameter&quot;&gt;a&lt;/div&gt;,&lt;br /&gt;
	///     &lt;div class=&quot;keyword&quot;&gt;T&lt;/div&gt; &lt;div class=&quot;parameter&quot;&gt;b&lt;/div&gt;&lt;br /&gt;
	///     )&lt;/div&gt;
	///   &lt;/div&gt;
	/// &lt;/div&gt;
	/// </code>
	/// </example>
	public class PreviewGenerator
	{
		/* TODO: After all is said and done, encode this sample and update it in the XML doc above.
		 * <div class="code">
		 *   <div class="attributes">
		 *     <div class="attribute">[SomeAttribute]</div>
		 *     <div class="attribute">[OtherAttribute<div class="parameters">(<span class="parameter">1</span>, <span class="parameter">b</span>=<span class="literal">2</span>)</div>]</div>
		 *   </div>
		 *   <div class="member">
		 *     <span class="keyword">public<span>
		 *     <span class="keyword">abstract</span>
		 *     <span class="keyword"><a href="#">string</a></span>
		 *     <span class="identifier">MyMethod</span><div class="typeparameters">&lt;<span class="typeparameter">T</span>&gt;</div><div class="parameters">(<br />
		 *     <span class="keyword">out</span> <span class="keyword"><a href="#">string</a></span> <span class="parameter">a</span>,<br />
		 *     <span class="keyword"><a href="#">string</a></span> <span class="parameter">b</span><br />
		 *     )</div>
		 *   </div>
		 * </div>
		 */

		/// <summary>
		/// Gets the element being documented.
		/// </summary>
		/// <value>
		/// The <see cref="DevExpress.CodeRush.StructuralParser.AccessSpecifiedElement"/>
		/// for which a preview will be generated.
		/// </value>
		public AccessSpecifiedElement Element { get; private set; }

		/// <summary>
		/// Gets a flag indicating if there are generic type parameters
		/// to render to the preview.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if <see cref="CR_Documentor.Transformation.Syntax.PreviewGenerator.Element"/>
		/// has generic type parameters to render; <see langword="false" /> if not.
		/// </value>
		protected bool ElementHasGenericParameters
		{
			get
			{
				if (!this.Element.IsGeneric || this.Element.GenericModifier.TypeParameters.Count == 0)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the preview language.
		/// </summary>
		/// <value>
		/// The <see cref="CR_Documentor.Transformation.Syntax.SupportedLanguageId"/>
		/// for which a preview will be generated.
		/// </value>
		public SupportedLanguageId Language { get; private set; }

		/// <summary>
		/// Gets a value indicating if type parameter constraints get shown
		/// inline with the parameters.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if type parameters get shown inline
		/// like in VB; <see langword="false" /> if they appear after the member
		/// signature like in C#.
		/// </value>
		protected virtual bool ShowTypeParameterConstraintsInline
		{
			get
			{
				return this.Language == SupportedLanguageId.Basic;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CR_Documentor.Transformation.Syntax.PreviewGenerator" /> class.
		/// </summary>
		/// <param name="element">
		/// The element for which the syntax preview should be generated.
		/// </param>
		/// <param name="language">
		/// The programming language in which the preview should be rendered.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="element" /> is <see langword="null" />.
		/// </exception>
		public PreviewGenerator(AccessSpecifiedElement element, SupportedLanguageId language)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			this.Element = element;
			this.Language = language;
		}

		/// <summary>
		/// Writes attribute information.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		protected virtual void Attributes(HtmlTextWriter writer)
		{
			if (this.Element.AttributeCount < 1)
			{
				return;
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Attributes);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			var attributes = this.Element.Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Attribute);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						writer.Write("&lt;");
						break;
					default:
						writer.Write("[");
						break;
				}
				var attribute = (DevExpress.CodeRush.StructuralParser.Attribute)attributes[i];
				writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				writer.Write(attribute.Name);
				writer.RenderEndTag();
				//if (attribute.ArgumentCount > 0)
				//{
				//    this.AttributeArguments(attribute.Arguments);
				//}
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						writer.Write("&gt; _");
						break;
					default:
						writer.Write("]");
						break;
				}
				writer.RenderEndTag();
			}
			writer.RenderEndTag();
		}

		/// <summary>
		/// Writes the preview for a class.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		protected virtual void Class(HtmlTextWriter writer)
		{
			if (this.Language == SupportedLanguageId.CSharp && this.Element.IsNew)
			{
				this.WriteSpan(writer, PreviewCss.Keyword, "new");
			}
			this.WriteSpan(writer, PreviewCss.Keyword, Lookup.Visibility(this.Language, this.Element.Visibility));
			if (this.Language == SupportedLanguageId.Basic && this.Element.IsNew)
			{
				this.WriteSpan(writer, PreviewCss.Keyword, "Shadows");
			}
			if (this.Element.IsStatic)
			{
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						break;
					default:
						this.WriteSpan(writer, PreviewCss.Keyword, "static");
						break;
				}
			}
			else if (this.Element.IsAbstract)
			{
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						this.WriteSpan(writer, PreviewCss.Keyword, "MustInherit");
						break;
					default:
						this.WriteSpan(writer, PreviewCss.Keyword, "abstract");
						break;
				}
			}
			else if (this.Element.IsSealed)
			{
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						this.WriteSpan(writer, PreviewCss.Keyword, "NotInheritable");
						break;
					default:
						this.WriteSpan(writer, PreviewCss.Keyword, "sealed");
						break;
				}
			}
			this.WriteSpan(writer, PreviewCss.Keyword, Lookup.ElementType(this.Language, this.Element));
			this.WriteSpan(writer, PreviewCss.Identifier, this.Element.Name, null, "");
			this.TypeParameters(writer);
			this.TypeParameterConstraintsPostSignature(writer);
		}

		/// <summary>
		/// Writes the preview for an enumeration.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		protected virtual void Enumeration(HtmlTextWriter writer)
		{
			this.WriteSpan(writer, PreviewCss.Keyword, Lookup.Visibility(this.Language, this.Element.Visibility));
			this.WriteSpan(writer, PreviewCss.Keyword, Lookup.ElementType(this.Language, this.Element));
			this.WriteSpan(writer, PreviewCss.Identifier, this.Element.Name, null, "");
			string underlyingType = ((Enumeration)this.Element).UnderlyingType;
			if (!String.IsNullOrEmpty(underlyingType))
			{
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						this.WriteSpan(writer, PreviewCss.Keyword, "As", " ", " ");
						break;
					default:
						writer.Write(" : ");
						break;
				}
				this.WriteSpan(writer, PreviewCss.Keyword, underlyingType, null, "");
			}
		}

		/// <summary>
		/// Generates the HTML syntax preview for an element.
		/// </summary>
		public virtual string Generate()
		{
			using (StringWriter baseWriter = new StringWriter())
			using (XhtmlTextWriter writer = new XhtmlTextWriter(baseWriter, ""))
			{
				string cssClass = PreviewCss.Code;
				switch (this.Language)
				{
					case SupportedLanguageId.Basic:
						cssClass += " " + PreviewCss.Language_Basic;
						break;
					case SupportedLanguageId.CSharp:
						cssClass += " " + PreviewCss.Language_CSharp;
						break;
				}
				writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				if (this.Language == SupportedLanguageId.None)
				{
					writer.Write(HttpUtility.HtmlEncode(Resources.PreviewGenerator_LanguageNotSupported));
				}
				else
				{
					this.Attributes(writer);
					writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Member);
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
					if (this.Element is Enumeration)
					{
						this.Enumeration(writer);
					}
					else if (this.Element is Class)
					{
						this.Class(writer);
					}
					writer.RenderEndTag();
				}
				writer.RenderEndTag();
				writer.Flush();
				return baseWriter.ToString();
			}
		}

		/// <summary>
		/// Writes the info for all type parameter constraints to the object signature.
		/// Used when the constraints are not written inline during the type parameter
		/// rendering.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		protected virtual void TypeParameterConstraintsPostSignature(HtmlTextWriter writer)
		{
			if (this.ShowTypeParameterConstraintsInline || !this.ElementHasGenericParameters)
			{
				return;
			}
			var typeParams = this.Element.GenericModifier.TypeParameters;
			int parameterCount = typeParams.Count;
			bool constraintsExist = false;
			for (int i = 0; i < parameterCount; i++)
			{
				var parameter = typeParams[i];
				var constraints = parameter.Constraints;
				var constraintCount = constraints.Count;
				if (constraintCount < 1)
				{
					continue;
				}
				if (!constraintsExist)
				{
					// Write the block for the constraints when the first one is encountered
					writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Constraints);
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
					constraintsExist = true;
				}
				writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Constraint);
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				this.WriteSpan(writer, PreviewCss.Keyword, "where");
				this.WriteSpan(writer, PreviewCss.TypeParameter, parameter.Name);
				writer.Write(": ");
				for (int j = 0; j < constraintCount; j++)
				{
					this.TypeParameterConstraintValue(writer, constraints[j]);
					if (j + 1 < constraintCount)
					{
						writer.Write(", ");
					}
				}
				writer.RenderEndTag();
			}
			if (constraintsExist)
			{
				// Close the constraint block if it was opened.
				writer.RenderEndTag();
			}
		}

		/// <summary>
		/// Writes the info for an individual type parameter constraint to the object signature
		/// for inline constraints.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		/// <param name="parameter">
		/// The type parameter constraint for which the information is being written.
		/// </param>
		protected virtual void TypeParameterConstraintsInline(HtmlTextWriter writer, TypeParameter parameter)
		{
			if (!this.ShowTypeParameterConstraintsInline)
			{
				return;
			}
			var constraints = parameter.Constraints;
			int constraintCount = constraints.Count;
			if (constraintCount == 0)
			{
				return;
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Constraints);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			this.WriteSpan(writer, PreviewCss.Keyword, "As", " ", " ");
			if (constraintCount > 1)
			{
				writer.Write("{");
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.Constraint);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			for (int j = 0; j < constraintCount; j++)
			{
				this.TypeParameterConstraintValue(writer, constraints[j]);
				if (j + 1 < constraintCount)
				{
					writer.Write(", ");
				}
			}
			writer.RenderEndTag();
			if (constraintCount > 1)
			{
				writer.Write("}");
			}
			writer.RenderEndTag();
		}

		private void TypeParameterConstraintValue(HtmlTextWriter writer, TypeParameterConstraint constraint)
		{
			if (constraint is NamedTypeParameterConstraint)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				// TODO: Consider rendering the complete signature of the type in the constraint.
				// While NamedTypeParameterConstraint.TypeReference gives you a
				// reference to a Type in a constraint, if that type is generic
				// we'd have to recursively process it and render that, too. For
				// now, if there's a constraint like...
				// where T : IList<Something>
				// it will render as...
				// where T : IList
				// because rendering the full type in a language-specific fashion is non-trivial.
				writer.Write(HttpUtility.HtmlEncode(((NamedTypeParameterConstraint)constraint).TypeReference.ToString()));
				writer.RenderEndTag();
			}
			else
			{
				this.WriteSpan(writer, PreviewCss.Keyword, constraint.Name, "", "");
			}
		}

		/// <summary>
		/// Writes the type parameter information to the object signature.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="System.Web.UI.HtmlTextWriter"/> to which the preview
		/// is being written.
		/// </param>
		protected virtual void TypeParameters(HtmlTextWriter writer)
		{
			if (!this.ElementHasGenericParameters)
			{
				return;
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, PreviewCss.TypeParameters);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			switch (this.Language)
			{
				case SupportedLanguageId.Basic:
					writer.Write("(");
					this.WriteSpan(writer, PreviewCss.Keyword, "Of");
					break;
				default:
					writer.Write("&lt;");
					break;
			}

			var typeParams = this.Element.GenericModifier.TypeParameters;
			int parameterCount = typeParams.Count;
			for (int i = 0; i < parameterCount; i++)
			{
				var parameter = typeParams[i];
				this.WriteSpan(writer, PreviewCss.TypeParameter, parameter.Name, "", "");
				if (parameter.Constraints.Count > 0)
				{
					this.TypeParameterConstraintsInline(writer, parameter);
				}
				if (i + 1 < parameterCount)
				{
					writer.Write(", ");
				}
			}

			switch (this.Language)
			{
				case SupportedLanguageId.Basic:
					writer.Write(")");
					break;
				default:
					writer.Write("&gt;");
					break;
			}
			writer.RenderEndTag();
		}

		private void WriteDiv(HtmlTextWriter writer, string cssClass, string contents)
		{
			this.WriteDiv(writer, cssClass, contents, null, null);
		}

		private void WriteDiv(HtmlTextWriter writer, string cssClass, string contents, string before, string after)
		{
			this.WriteTag(writer, HtmlTextWriterTag.Div, cssClass, contents, before, after);
		}

		private void WriteSpan(HtmlTextWriter writer, string cssClass, string contents)
		{
			this.WriteSpan(writer, cssClass, contents, null, null);
		}

		private void WriteSpan(HtmlTextWriter writer, string cssClass, string contents, string before, string after)
		{
			this.WriteTag(writer, HtmlTextWriterTag.Span, cssClass, contents, before, after);
		}

		private void WriteTag(HtmlTextWriter writer, HtmlTextWriterTag tagKey, string cssClass, string contents, string before, string after)
		{
			if (string.IsNullOrEmpty(contents))
			{
				return;
			}
			if (before != null)
			{
				writer.Write(before);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
			writer.RenderBeginTag(tagKey);
			writer.Write(HttpUtility.HtmlEncode(contents));
			writer.RenderEndTag();
			if (after != null)
			{
				writer.Write(before);
			}
			else
			{
				writer.Write(" ");
			}
		}
	}
}
