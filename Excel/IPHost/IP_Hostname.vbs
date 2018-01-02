Function HostName(x As String) As String

Set dict = CreateObject("Scripting.Dictionary")

Dim rng1 As Range
Dim rng2 As Range
Set rng1 = Worksheets("IP").Range("A1:B254")
For Each rng2 In rng1.Rows
    If Not dict.Exists(Trim(rng2.Columns(1))) Then
        dict.Add Trim(rng2.Columns(1)), Trim(rng2.Columns(2))
    End If
Next

    If Not dict.Exists(x) Then
        HostName = x
        Else
        HostName = dict.Item(x)
    End If
    
End Function



Sub Hello()
    

Dim rng1 As Range
Dim rng2 As Range
Set rng1 = Worksheets("IP").Range("A1:B2")

MsgBox "Fixed:" + HostName("168.95.1.1")

For Each rng2 In rng1.Rows
    MsgBox "AA:" + Trim(rng2.Columns(2)) + " =" + HostName(Trim(rng2.Columns(2)))
    'MsgBox rng2.Columns(2)
Next
    
End Sub

