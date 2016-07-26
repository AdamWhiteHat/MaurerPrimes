namespace MaurerWinform
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
			this.btnSearch = new System.Windows.Forms.Button();
			this.tbOutput = new System.Windows.Forms.TextBox();
			this.tbNumberOfBits = new System.Windows.Forms.TextBox();
			this.btnPrimalityTest = new System.Windows.Forms.Button();
			this.btnMultiply = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(106, 2);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(115, 22);
			this.btnSearch.TabIndex = 0;
			this.btnSearch.Text = "Search for primes...";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// tbOutput
			// 
			this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbOutput.Location = new System.Drawing.Point(4, 27);
			this.tbOutput.Multiline = true;
			this.tbOutput.Name = "tbOutput";
			this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbOutput.Size = new System.Drawing.Size(844, 405);
			this.tbOutput.TabIndex = 1;
			this.tbOutput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbOutput_KeyUp);
			// 
			// tbNumberOfBits
			// 
			this.tbNumberOfBits.Location = new System.Drawing.Point(4, 3);
			this.tbNumberOfBits.Name = "tbNumberOfBits";
			this.tbNumberOfBits.Size = new System.Drawing.Size(100, 20);
			this.tbNumberOfBits.TabIndex = 2;
			this.tbNumberOfBits.Text = "2048";
			this.tbNumberOfBits.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// btnPrimalityTest
			// 
			this.btnPrimalityTest.Location = new System.Drawing.Point(567, 1);
			this.btnPrimalityTest.Name = "btnPrimalityTest";
			this.btnPrimalityTest.Size = new System.Drawing.Size(115, 22);
			this.btnPrimalityTest.TabIndex = 3;
			this.btnPrimalityTest.Text = "Primality Test...";
			this.btnPrimalityTest.UseVisualStyleBackColor = true;
			this.btnPrimalityTest.Click += new System.EventHandler(this.btnPrimalityTest_Click);
			// 
			// btnMultiply
			// 
			this.btnMultiply.Location = new System.Drawing.Point(683, 1);
			this.btnMultiply.Name = "btnMultiply";
			this.btnMultiply.Size = new System.Drawing.Size(165, 22);
			this.btnMultiply.TabIndex = 4;
			this.btnMultiply.Text = "Multiply two large numbers";
			this.btnMultiply.UseVisualStyleBackColor = true;
			this.btnMultiply.Click += new System.EventHandler(this.btnMultiply_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(852, 436);
			this.Controls.Add(this.btnMultiply);
			this.Controls.Add(this.btnPrimalityTest);
			this.Controls.Add(this.tbNumberOfBits);
			this.Controls.Add(this.tbOutput);
			this.Controls.Add(this.btnSearch);
			this.Name = "MainForm";
			this.Text = "Search for primes";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.TextBox tbOutput;
		private System.Windows.Forms.TextBox tbNumberOfBits;
		private System.Windows.Forms.Button btnPrimalityTest;
		private System.Windows.Forms.Button btnMultiply;
	}
}