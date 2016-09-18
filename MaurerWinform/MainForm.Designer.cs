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
			this.tbInput = new System.Windows.Forms.TextBox();
			this.btnPrimalityTest = new System.Windows.Forms.Button();
			this.btnMultiply = new System.Windows.Forms.Button();
			this.btnTrialDivision = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tbSearchDepth = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnJacobi = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearch.Location = new System.Drawing.Point(763, 2);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(85, 22);
			this.btnSearch.TabIndex = 0;
			this.btnSearch.Text = "Find primes...";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// tbOutput
			// 
			this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbOutput.Location = new System.Drawing.Point(4, 116);
			this.tbOutput.Multiline = true;
			this.tbOutput.Name = "tbOutput";
			this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbOutput.Size = new System.Drawing.Size(844, 335);
			this.tbOutput.TabIndex = 1;
			this.tbOutput.WordWrap = false;
			this.tbOutput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbOutput_KeyUp);
			// 
			// tbInput
			// 
			this.tbInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbInput.Location = new System.Drawing.Point(4, 22);
			this.tbInput.Multiline = true;
			this.tbInput.Name = "tbInput";
			this.tbInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbInput.Size = new System.Drawing.Size(753, 68);
			this.tbInput.TabIndex = 2;
			this.tbInput.Text = "2048";
			// 
			// btnPrimalityTest
			// 
			this.btnPrimalityTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPrimalityTest.Location = new System.Drawing.Point(763, 46);
			this.btnPrimalityTest.Name = "btnPrimalityTest";
			this.btnPrimalityTest.Size = new System.Drawing.Size(85, 22);
			this.btnPrimalityTest.TabIndex = 3;
			this.btnPrimalityTest.Text = "Test Primality";
			this.btnPrimalityTest.UseVisualStyleBackColor = true;
			this.btnPrimalityTest.Click += new System.EventHandler(this.btnPrimalityTest_Click);
			// 
			// btnMultiply
			// 
			this.btnMultiply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnMultiply.Location = new System.Drawing.Point(763, 24);
			this.btnMultiply.Name = "btnMultiply";
			this.btnMultiply.Size = new System.Drawing.Size(85, 22);
			this.btnMultiply.TabIndex = 4;
			this.btnMultiply.Text = "Multiply";
			this.btnMultiply.UseVisualStyleBackColor = true;
			this.btnMultiply.Click += new System.EventHandler(this.btnMultiply_Click);
			// 
			// btnTrialDivision
			// 
			this.btnTrialDivision.Location = new System.Drawing.Point(0, 0);
			this.btnTrialDivision.Name = "btnTrialDivision";
			this.btnTrialDivision.Size = new System.Drawing.Size(75, 23);
			this.btnTrialDivision.TabIndex = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(1, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Input:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(1, 100);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Output:";
			// 
			// tbSearchDepth
			// 
			this.tbSearchDepth.Location = new System.Drawing.Point(716, 93);
			this.tbSearchDepth.Name = "tbSearchDepth";
			this.tbSearchDepth.Size = new System.Drawing.Size(41, 20);
			this.tbSearchDepth.TabIndex = 8;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Bold);
			this.label3.Location = new System.Drawing.Point(533, 97);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(181, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Miller-Rabin Composite Tests:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnJacobi
			// 
			this.btnJacobi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnJacobi.Location = new System.Drawing.Point(763, 68);
			this.btnJacobi.Name = "btnJacobi";
			this.btnJacobi.Size = new System.Drawing.Size(85, 22);
			this.btnJacobi.TabIndex = 11;
			this.btnJacobi.Text = "Jacobi Symbol";
			this.btnJacobi.UseVisualStyleBackColor = true;
			this.btnJacobi.Click += new System.EventHandler(this.btnJacobi_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(852, 455);
			this.Controls.Add(this.btnJacobi);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbSearchDepth);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnTrialDivision);
			this.Controls.Add(this.btnMultiply);
			this.Controls.Add(this.btnPrimalityTest);
			this.Controls.Add(this.tbOutput);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.tbInput);
			this.MinimumSize = new System.Drawing.Size(585, 230);
			this.Name = "MainForm";
			this.Text = "Search for primes";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.TextBox tbOutput;
		private System.Windows.Forms.TextBox tbInput;
		private System.Windows.Forms.Button btnPrimalityTest;
		private System.Windows.Forms.Button btnMultiply;
		private System.Windows.Forms.Button btnTrialDivision;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbSearchDepth;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnJacobi;
	}
}