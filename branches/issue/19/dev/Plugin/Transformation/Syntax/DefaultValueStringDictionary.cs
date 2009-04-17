using System;
using System.Collections.Specialized;

namespace CR_Documentor.Transformation.Syntax
{
	/// <summary>
	/// A <see cref="System.Collections.Specialized.StringDictionary"/> that provides
	/// a default value that will be returned if the key isn't found.  Used in
	/// providing lookups for language-specific items.
	/// </summary>
	public class DefaultValueStringDictionary : StringDictionary
	{
		/// <summary>
		/// Internal storage for the
		/// <see cref="CR_Documentor.Transformation.Syntax.DefaultValueStringDictionary.DefaultValue" />
		/// property.
		/// </summary>
		/// <seealso cref="CR_Documentor.Transformation.Syntax.DefaultValueStringDictionary" />
        private string _defaultValue = "";
		
		/// <summary>
		/// Gets or sets the default return value for the dictionary.
		/// </summary>
		/// <value>
		/// A <see cref="System.String"/> that will be returned if the dictionary
		/// is queried for a key that doesn't exist.  Defaults to <see cref="System.String.Empty"/>.
		/// </value>
		public virtual string DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="System.String"/> with the specified key.
		/// </summary>
		/// <value>
		/// The <see cref="System.String"/> value that corresponds to the specified
		/// key, or <see cref="CR_Documentor.Transformation.Syntax.DefaultValueStringDictionary.DefaultValue"/>
		/// if the key is not found.
		/// </value>
		public override string this[string key]
		{
			get
			{
				if (!this.ContainsKey(key))
				{
					return this.DefaultValue;
				}
				return base[key];
			}
			set
			{
				base[key] = value;
			}
		}
	}
}