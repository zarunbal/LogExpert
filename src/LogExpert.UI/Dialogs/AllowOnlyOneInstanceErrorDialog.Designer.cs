namespace LogExpert.UI.Dialogs;

partial class AllowOnlyOneInstanceErrorDialog
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
    private void InitializeComponent ()
    {
        checkBoxIgnoreMessage = new CheckBox();
        buttonOk = new Button();
        labelErrorText = new Label();
        SuspendLayout();
        // 
        // checkBoxIgnoreMessage
        // 
        checkBoxIgnoreMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        checkBoxIgnoreMessage.AutoSize = true;
        checkBoxIgnoreMessage.Location = new Point(9, 72);
        checkBoxIgnoreMessage.Margin = new Padding(2);
        checkBoxIgnoreMessage.Name = "checkBoxIgnoreMessage";
        checkBoxIgnoreMessage.Size = new Size(186, 19);
        checkBoxIgnoreMessage.TabIndex = 0;
        checkBoxIgnoreMessage.Text = "Show this message only once?";
        checkBoxIgnoreMessage.UseVisualStyleBackColor = true;
        // 
        // buttonOk
        // 
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        buttonOk.DialogResult = DialogResult.OK;
        buttonOk.Location = new Point(209, 71);
        buttonOk.Margin = new Padding(2);
        buttonOk.Name = "buttonOk";
        buttonOk.Size = new Size(104, 23);
        buttonOk.TabIndex = 1;
        buttonOk.Text = "Ok";
        buttonOk.UseVisualStyleBackColor = true;
        buttonOk.Click += OnButtonOkClick;
        // 
        // labelErrorText
        // 
        labelErrorText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        labelErrorText.AutoEllipsis = true;
        labelErrorText.Location = new Point(9, 8);
        labelErrorText.Margin = new Padding(2, 0, 2, 0);
        labelErrorText.Name = "labelErrorText";
        labelErrorText.Size = new Size(303, 43);
        labelErrorText.TabIndex = 2;
        labelErrorText.Text = "Only one instance allowed, uncheck \"View Settings => Allow only 1 Instances\" to start multiple instances!";
        // 
        // AllowOnlyOneInstanceErrorDialog
        // 
        ClientSize = new Size(323, 102);
        Controls.Add(labelErrorText);
        Controls.Add(buttonOk);
        Controls.Add(checkBoxIgnoreMessage);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Margin = new Padding(2);
        MaximizeBox = false;
        Name = "AllowOnlyOneInstanceErrorDialog";
        SizeGripStyle = SizeGripStyle.Hide;
        Text = "Allow only one instance error dialog";
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxIgnoreMessage;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Label labelErrorText;
}