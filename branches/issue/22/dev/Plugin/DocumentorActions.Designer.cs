namespace CR_Documentor
{
	partial class DocumentorActions
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentorActions"/> class.
		/// </summary>
		public DocumentorActions()
		{
			// Required for Windows.Forms Class Composition Designer support
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentorActions));
			this.collapseXmlDocComments = new DevExpress.CodeRush.Core.Action(this.components);
			this.expandXmlDocComments = new DevExpress.CodeRush.Core.Action(this.components);
			((System.ComponentModel.ISupportInitialize)(this.collapseXmlDocComments)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.expandXmlDocComments)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// collapseXmlDocComments
			// 
			this.collapseXmlDocComments.ActionName = "Collapse XML Doc Comments";
			this.collapseXmlDocComments.ButtonText = "Collapse XML Doc Comments";
			this.collapseXmlDocComments.CommonMenu = DevExpress.CodeRush.Menus.VsCommonBar.None;
			this.collapseXmlDocComments.Description = "Collapses the XML documentation comment blocks in a source file.";
			this.collapseXmlDocComments.Image = ((System.Drawing.Bitmap)(resources.GetObject("collapseXmlDocComments.Image")));
			this.collapseXmlDocComments.ImageBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(254)))), ((int)(((byte)(0)))));
			this.collapseXmlDocComments.Execute += new DevExpress.CodeRush.Core.CommandExecuteEventHandler(this.collapseXmlDocComments_Execute);
			this.collapseXmlDocComments.QueryStatus += new DevExpress.CodeRush.Core.QueryStatusEventHandler(this.toggleXmlDocComments_QueryStatus);
			// 
			// expandXmlDocComments
			// 
			this.expandXmlDocComments.ActionName = "Expand XML Doc Comments";
			this.expandXmlDocComments.ButtonText = "Expand XML Doc Comments";
			this.expandXmlDocComments.CommonMenu = DevExpress.CodeRush.Menus.VsCommonBar.None;
			this.expandXmlDocComments.Description = "Expands the XML documentation comment blocks in a source file.";
			this.expandXmlDocComments.Image = ((System.Drawing.Bitmap)(resources.GetObject("expandXmlDocComments.Image")));
			this.expandXmlDocComments.ImageBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(254)))), ((int)(((byte)(0)))));
			this.expandXmlDocComments.Execute += new DevExpress.CodeRush.Core.CommandExecuteEventHandler(this.expandXmlDocComments_Execute);
			((System.ComponentModel.ISupportInitialize)(this.collapseXmlDocComments)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.expandXmlDocComments)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

		}

		#endregion

		private DevExpress.CodeRush.Core.Action collapseXmlDocComments;
		private DevExpress.CodeRush.Core.Action expandXmlDocComments;
	}
}