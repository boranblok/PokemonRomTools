namespace EndianSwapper
{
    partial class Form1
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
            this.lblPointer = new System.Windows.Forms.Label();
            this.txtPointer = new System.Windows.Forms.TextBox();
            this.lblReference = new System.Windows.Forms.Label();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.btnGenerateREF = new System.Windows.Forms.Button();
            this.btnGeneratePointer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPointer
            // 
            this.lblPointer.AutoSize = true;
            this.lblPointer.Location = new System.Drawing.Point(13, 13);
            this.lblPointer.Name = "lblPointer";
            this.lblPointer.Size = new System.Drawing.Size(29, 13);
            this.lblPointer.TabIndex = 0;
            this.lblPointer.Text = "PTR";
            // 
            // txtPointer
            // 
            this.txtPointer.Location = new System.Drawing.Point(54, 10);
            this.txtPointer.Name = "txtPointer";
            this.txtPointer.Size = new System.Drawing.Size(100, 20);
            this.txtPointer.TabIndex = 1;
            // 
            // lblReference
            // 
            this.lblReference.AutoSize = true;
            this.lblReference.Location = new System.Drawing.Point(13, 39);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(28, 13);
            this.lblReference.TabIndex = 2;
            this.lblReference.Text = "REF";
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(54, 36);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(100, 20);
            this.txtReference.TabIndex = 3;
            // 
            // btnGenerateREF
            // 
            this.btnGenerateREF.Location = new System.Drawing.Point(160, 8);
            this.btnGenerateREF.Name = "btnGenerateREF";
            this.btnGenerateREF.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateREF.TabIndex = 4;
            this.btnGenerateREF.Text = "Swap";
            this.btnGenerateREF.UseVisualStyleBackColor = true;
            this.btnGenerateREF.Click += new System.EventHandler(this.btnGenerateREF_Click);
            // 
            // btnGeneratePointer
            // 
            this.btnGeneratePointer.Location = new System.Drawing.Point(160, 34);
            this.btnGeneratePointer.Name = "btnGeneratePointer";
            this.btnGeneratePointer.Size = new System.Drawing.Size(75, 23);
            this.btnGeneratePointer.TabIndex = 4;
            this.btnGeneratePointer.Text = "Swap";
            this.btnGeneratePointer.UseVisualStyleBackColor = true;
            this.btnGeneratePointer.Click += new System.EventHandler(this.btnGeneratePointer_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 76);
            this.Controls.Add(this.btnGeneratePointer);
            this.Controls.Add(this.btnGenerateREF);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.lblReference);
            this.Controls.Add(this.txtPointer);
            this.Controls.Add(this.lblPointer);
            this.Name = "Form1";
            this.Text = "EndianSwapper";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPointer;
        private System.Windows.Forms.TextBox txtPointer;
        private System.Windows.Forms.Label lblReference;
        private System.Windows.Forms.TextBox txtReference;
        private System.Windows.Forms.Button btnGenerateREF;
        private System.Windows.Forms.Button btnGeneratePointer;
    }
}

