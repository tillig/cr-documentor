using System;

namespace CR_Documentor.Options
{
	/// <summary>
	/// Indicates how included documentation should be processed.
	/// </summary>
	public enum IncludeProcessing
	{
		/// <summary>
		/// Don't process includes.
		/// </summary>
		None,

		/// <summary>
		/// Process only absolute file paths for includes.
		/// </summary>
		Absolute,

		/// <summary>
		/// Process absolute and relative file paths for includes.
		/// </summary>
		Relative
	}

	// TODO: Update tag compatibility to be "Microsoft1_1," "Microsoft2_0," "NDoc1_3" (?)
	// TODO: Should I even bother with tag compatibility?  Is it even used?  Maybe it should be entirely tied to the rendering engine?
	/// <summary>
	/// The possible tag compatibility levels.
	/// </summary>
	public enum TagCompatibilityLevel
	{
		/// <summary>
		/// Microsoft Strict - only recognize the tags outlined in MS documentation.
		/// </summary>
		MicrosoftStrict,

		/// <summary>
		/// NDoc 1.3 - recognize MS and extended tags available in NDoc 1.3.
		/// </summary>
		NDoc1_3,

		/// <summary>
		/// Sandcastle - recognize MS and extended tags available in Sandcastle.
		/// </summary>
		Sandcastle
	}


	/// <summary>
	/// The possible ways to handle unrecognized tags.
	/// </summary>
	public enum UnrecognizedTagHandlingMethod
	{
		/// <summary>
		/// Hide the tag and its contents.
		/// </summary>
		HideTagAndContents,

		/// <summary>
		/// Strip the tag, but show the contents.
		/// </summary>
		StripTagShowContents,

		/// <summary>
		/// Highlight the tag and its contents.
		/// </summary>
		HighlightTagAndContents,

		/// <summary>
		/// Render the contents anyway.
		/// </summary>
		RenderContents
	}
}
