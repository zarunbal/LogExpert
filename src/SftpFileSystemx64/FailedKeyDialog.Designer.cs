namespace SftpFileSystem
{
  partial class FailedKeyDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnUsePasswordAuthentication = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRetry = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Opening file via SSH key failed!";
            // 
            // btnUsePasswordAuthentication
            // 
            this.btnUsePasswordAuthentication.Location = new System.Drawing.Point(108, 72);
            this.btnUsePasswordAuthentication.Name = "btnUsePasswordAuthentication";
            this.btnUsePasswordAuthentication.Size = new System.Drawing.Size(123, 23);
            this.btnUsePasswordAuthentication.TabIndex = 2;
            this.btnUsePasswordAuthentication.Text = "Use password auth";
            this.btnUsePasswordAuthentication.UseVisualStyleBackColor = true;
            this.btnUsePasswordAuthentication.Click += new System.EventHandler(this.OnBtnUsePasswordAuthenticationClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(251, 72);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.OnBtnCancelClick);
            // 
            // btnRetry
            // 
            this.btnRetry.Location = new System.Drawing.Point(15, 72);
            this.btnRetry.Name = "btnRetry";
            this.btnRetry.Size = new System.Drawing.Size(75, 23);
            this.btnRetry.TabIndex = 1;
            this.btnRetry.Text = "Retry";
            this.btnRetry.UseVisualStyleBackColor = true;
            this.btnRetry.Click += new System.EventHandler(this.OnBtnRetryClick);
            // 
            // FailedKeyDialog
            // 
            this.AcceptButton = this.btnRetry;
            this.ClientSize = new System.Drawing.Size(338, 115);
            this.ControlBox = false;
            this.Controls.Add(this.btnRetry);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUsePasswordAuthentication);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FailedKeyDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LogExpert SFTP Plugin";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnUsePasswordAuthentication;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnRetry;
  }
}