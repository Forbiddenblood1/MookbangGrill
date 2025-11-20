<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UC_Reporting_Analytics
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        TableLayoutPanel1 = New TableLayoutPanel()
        Panel1 = New Panel()
        Panel2 = New Panel()
        Panel3 = New Panel()
        Panel4 = New Panel()
        FilterComboBox = New ComboBox()
        TableLayoutPanel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.BackColor = SystemColors.ControlDarkDark
        TableLayoutPanel1.ColumnCount = 2
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 49.8046875F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.1953125F))
        TableLayoutPanel1.Controls.Add(Panel1, 0, 0)
        TableLayoutPanel1.Controls.Add(Panel2, 1, 0)
        TableLayoutPanel1.Controls.Add(Panel3, 0, 1)
        TableLayoutPanel1.Controls.Add(Panel4, 1, 1)
        TableLayoutPanel1.Location = New Point(28, 88)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Size = New Size(978, 619)
        TableLayoutPanel1.TabIndex = 0
        ' 
        ' Panel1
        ' 
        Panel1.Dock = DockStyle.Fill
        Panel1.Location = New Point(3, 3)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(481, 303)
        Panel1.TabIndex = 0
        ' 
        ' Panel2
        ' 
        Panel2.Dock = DockStyle.Fill
        Panel2.Location = New Point(490, 3)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(485, 303)
        Panel2.TabIndex = 1
        ' 
        ' Panel3
        ' 
        Panel3.Dock = DockStyle.Fill
        Panel3.Location = New Point(3, 312)
        Panel3.Name = "Panel3"
        Panel3.Size = New Size(481, 304)
        Panel3.TabIndex = 2
        ' 
        ' Panel4
        ' 
        Panel4.Dock = DockStyle.Fill
        Panel4.Location = New Point(490, 312)
        Panel4.Name = "Panel4"
        Panel4.Size = New Size(485, 304)
        Panel4.TabIndex = 3
        ' 
        ' FilterComboBox
        ' 
        FilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        FilterComboBox.FormattingEnabled = True
        FilterComboBox.Items.AddRange(New Object() {"Current", "Last 7 Days", "This Month", "This Year"})
        FilterComboBox.Location = New Point(816, 30)
        FilterComboBox.Name = "FilterComboBox"
        FilterComboBox.Size = New Size(190, 23)
        FilterComboBox.TabIndex = 3
        ' 
        ' UC_Reporting_Analytics
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(FilterComboBox)
        Controls.Add(TableLayoutPanel1)
        Name = "UC_Reporting_Analytics"
        Size = New Size(1024, 768)
        TableLayoutPanel1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Panel3 As Panel
    Friend WithEvents Panel4 As Panel
    Friend WithEvents FilterComboBox As ComboBox

End Class
