Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Drawing

Public Class UC_Reporting_Analytics

    Private connectionString As String = "Server=localhost\SQLEXPRESS;Database=POS;Integrated Security=True;TrustServerCertificate=True;"

    Private TotalSalesChart As Chart
    Private OrderCountChart As Chart
    Private AvgCheckSizeChart As Chart
    Private TopSellingItemChart As Chart

    Private Const AvgCheckSizeTarget As Decimal = 35D

    Private WithEvents autoRefreshTimer As New Timer()

    'DATA CLASSES
    Public Class SalesDataPoint
        Public Property DateValue As DateTime
        Public Property SalesAmount As Decimal
        Public Property OrderCount As Integer
        Public Property AvgCheckSize As Decimal
        Public Property TargetValue As Decimal

        Public Sub New(dateValue As DateTime, salesAmount As Decimal, orderCount As Integer, avgCheckSize As Decimal, targetValue As Decimal)
            Me.DateValue = dateValue
            Me.SalesAmount = salesAmount
            Me.OrderCount = orderCount
            Me.AvgCheckSize = avgCheckSize
            Me.TargetValue = targetValue
        End Sub
    End Class

    Public Class TopItemSale
        Public Property ItemName As String
        Public Property ItemsSold As Integer

        Public Sub New(itemName As String, itemsSold As Integer)
            Me.ItemName = itemName
            Me.ItemsSold = itemsSold
        End Sub
    End Class


    Public Sub New()
        InitializeComponent()
        InitializeChartControls()
        InitializeFilterComboBox()

        autoRefreshTimer.Interval = 30000
        autoRefreshTimer.Start()
        ApplyFilter()
    End Sub

    ' FILTER COMBOBOX
    Private Sub InitializeFilterComboBox()
        FilterComboBox.Items.Clear()
        FilterComboBox.Items.AddRange({"Current Day", "Last 7 Days", "This Month", "This Year"})
        FilterComboBox.SelectedIndex = 0
        AddHandler FilterComboBox.SelectedIndexChanged, AddressOf FilterComboBox_SelectedIndexChanged
    End Sub

    Private Sub FilterComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        Dim startDate As DateTime
        Dim endDate As DateTime = DateTime.Now.Date
        Dim titlePrefix As String = ""

        Select Case FilterComboBox.SelectedItem?.ToString()
            Case "Current Day"
                startDate = DateTime.Today
                endDate = DateTime.Today.AddDays(1)

            Case "Last 7 Days"
                startDate = endDate.AddDays(-6)
                titlePrefix = "Last 7 Days"

            Case "This Month"
                startDate = New Date(endDate.Year, endDate.Month, 1)
                titlePrefix = "This Month"

            Case "This Year"
                startDate = New Date(endDate.Year, 1, 1)
                titlePrefix = "This Year"

            Case Else
                startDate = endDate.AddDays(-6)
                titlePrefix = "Last 7 Days"
        End Select

        LoadAnalyticsData(startDate, endDate, titlePrefix)
    End Sub

    '   LOADING ANALYTICS
    Private Sub LoadAnalyticsData(startDate As DateTime, endDate As DateTime, titlePrefix As String)

        Dim timeSeriesData = GetTimeSeriesData(startDate, endDate)
        Dim totalEarnings As Decimal = GetTotalEarnings(startDate, endDate)
        Dim itemData = GetTopItemData(startDate, endDate)

        Dim totalOrders As Integer = timeSeriesData.Sum(Function(x) x.OrderCount)
        Dim avgCheckSize As Decimal = If(totalOrders > 0, totalEarnings / totalOrders, 0D)

        lblTotalSalesValue.Text = totalEarnings.ToString("C2")
        lblOrderCountValue.Text = totalOrders.ToString()
        lblAvgCheckSizeValue.Text = avgCheckSize.ToString("C2")

        ' Total Sales
        TotalSalesChart.DataSource = timeSeriesData
        TotalSalesChart.Series(0).XValueMember = "DateValue"
        TotalSalesChart.Series(0).YValueMembers = "SalesAmount"
        TotalSalesChart.DataBind()
        TotalSalesChart.Titles(0).Text = $"Total Sales - {titlePrefix}"

        ' Order Count 
        OrderCountChart.DataSource = timeSeriesData
        OrderCountChart.Series(0).XValueMember = "DateValue"
        OrderCountChart.Series(0).YValueMembers = "OrderCount"
        OrderCountChart.DataBind()
        OrderCountChart.Titles(0).Text = $"Order Count - {titlePrefix}"

        ' Avg Check Size
        AvgCheckSizeChart.Titles(0).Text = "Average Check Size"
        ApplyAvgCheckSizeConditionalColors(timeSeriesData)

        ' Top Selling Items
        TopSellingItemChart.DataSource = itemData
        TopSellingItemChart.Series(0).XValueMember = "ItemName"
        TopSellingItemChart.Series(0).YValueMembers = "ItemsSold"
        TopSellingItemChart.DataBind()
        TopSellingItemChart.Titles(0).Text = $"Top Selling Items - {titlePrefix}"

    End Sub

    ' SQL HELPERS
    Private Function DayStart(d As DateTime) As DateTime
        Return d.Date
    End Function

    Private Function DayEnd(d As DateTime) As DateTime
        Return d.Date.AddDays(1)
    End Function

    Private Function GetTotalEarnings(startDate As DateTime, endDate As DateTime) As Decimal
        Using conn As New SqlConnection(connectionString)
            conn.Open()

            Dim query As String = "
                SELECT SUM(OI.Subtotal)
                FROM Orders O
                INNER JOIN OrderItems OI ON O.OrderID = OI.OrderID
                WHERE O.CreatedAt >= @Start AND O.CreatedAt < @End
                AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')
            "

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Start", DayStart(startDate))
                cmd.Parameters.AddWithValue("@End", DayEnd(endDate))

                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                    Return Convert.ToDecimal(result)
                End If
            End Using
        End Using

        Return 0D
    End Function


    Private Function GetTimeSeriesData(startDate As DateTime, endDate As DateTime) As List(Of SalesDataPoint)
        Dim result As New List(Of SalesDataPoint)

        Using conn As New SqlConnection(connectionString)
            conn.Open()

            Dim query As String = "
                SELECT CAST(O.CreatedAt AS DATE) AS SalesDate,
                       SUM(OI.Subtotal) AS TotalSales,
                       COUNT(DISTINCT O.OrderID) AS OrderCount
                FROM Orders O
                INNER JOIN OrderItems OI ON O.OrderID = OI.OrderID
                WHERE O.CreatedAt >= @Start AND O.CreatedAt < @End
                AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')
                GROUP BY CAST(O.CreatedAt AS DATE)
                ORDER BY SalesDate
            "

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Start", DayStart(startDate))
                cmd.Parameters.AddWithValue("@End", DayEnd(endDate))

                Using r = cmd.ExecuteReader()
                    While r.Read()
                        Dim sales = If(IsDBNull(r("TotalSales")), 0D, Convert.ToDecimal(r("TotalSales")))
                        Dim orders = If(IsDBNull(r("OrderCount")), 0, Convert.ToInt32(r("OrderCount")))
                        Dim avgSize As Decimal = If(orders > 0, Decimal.Round(sales / orders, 2), 0D)

                        result.Add(New SalesDataPoint(
                            Convert.ToDateTime(r("SalesDate")),
                            sales,
                            orders,
                            avgSize,
                            AvgCheckSizeTarget
                        ))
                    End While
                End Using
            End Using
        End Using


        Dim filled As New List(Of SalesDataPoint)
        Dim currentDate = startDate

        While currentDate <= endDate
            Dim found = result.FirstOrDefault(Function(x) x.DateValue.Date = currentDate.Date)

            If found IsNot Nothing Then
                filled.Add(found)
            Else
                filled.Add(New SalesDataPoint(currentDate, 0, 0, 0, AvgCheckSizeTarget))
            End If

            currentDate = currentDate.AddDays(1)
        End While

        Return filled
    End Function


    Private Function GetTopItemData(startDate As DateTime, endDate As DateTime) As List(Of TopItemSale)
        Dim result As New List(Of TopItemSale)

        Using conn As New SqlConnection(connectionString)
            conn.Open()

            Dim query As String = "
                SELECT P.ProductName, SUM(OI.Quantity) AS ItemsSold
                FROM OrderItems OI
                INNER JOIN Orders O ON OI.OrderID = O.OrderID
                INNER JOIN Products P ON OI.ProductID = P.ProductID
                WHERE O.CreatedAt >= @Start AND O.CreatedAt < @End
                AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')
                GROUP BY P.ProductName
                ORDER BY ItemsSold DESC
            "

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Start", DayStart(startDate))
                cmd.Parameters.AddWithValue("@End", DayEnd(endDate))

                Using r = cmd.ExecuteReader()
                    While r.Read()
                        result.Add(New TopItemSale(
                            r("ProductName").ToString(),
                            Convert.ToInt32(r("ItemsSold"))
                        ))
                    End While
                End Using
            End Using
        End Using

        Return result
    End Function

    ' AVG CHECK SIZE COLORS
    Private Sub ApplyAvgCheckSizeConditionalColors(data As List(Of SalesDataPoint))
        Dim actualSeries = AvgCheckSizeChart.Series("Average Check Size")
        Dim targetSeries = AvgCheckSizeChart.Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2"))

        actualSeries.Points.Clear()
        targetSeries.Points.Clear()

        For Each dp In data
            Dim p As New DataPoint(dp.DateValue.ToOADate(), dp.AvgCheckSize)

            If dp.AvgCheckSize >= AvgCheckSizeTarget Then
                p.Color = Color.Green
            Else
                p.Color = Color.OrangeRed
            End If

            actualSeries.Points.Add(p)
            targetSeries.Points.AddXY(dp.DateValue.ToOADate(), dp.TargetValue)
        Next
    End Sub

    ' CHART INITIALIZATION
    Private Sub InitializeChartControls()


        TotalSalesChart = New Chart() With {.Dock = DockStyle.Fill}
        Dim totalArea As New ChartArea("TotalSalesArea")
        TotalSalesChart.ChartAreas.Add(totalArea)

        TotalSalesChart.Series.Add(New Series("Total Sales") With {
            .ChartType = SeriesChartType.Line,
            .XValueType = ChartValueType.Date,
            .ToolTip = "Date: #VALX{dd MMM yyyy}" & vbCrLf &
                       "Sales: ₱#VALY{N2}"
        })

        TotalSalesChart.Titles.Add("Total Sales")
        Panel1.Controls.Add(TotalSalesChart)



        OrderCountChart = New Chart() With {.Dock = DockStyle.Fill}
        Dim orderArea As New ChartArea("OrderCountArea")
        OrderCountChart.ChartAreas.Add(orderArea)

        OrderCountChart.Series.Add(New Series("Order Count") With {
            .ChartType = SeriesChartType.Line,
            .XValueType = ChartValueType.Date,
            .ToolTip = "Date: #VALX{dd MMM yyyy}" & vbCrLf &
                       "Orders: #VALY"
        })

        OrderCountChart.Titles.Add("Order Count")
        Panel2.Controls.Add(OrderCountChart)



        AvgCheckSizeChart = New Chart() With {.Dock = DockStyle.Fill}
        Dim avgArea As New ChartArea("AvgCheckArea")
        AvgCheckSizeChart.ChartAreas.Add(avgArea)

        AvgCheckSizeChart.Series.Add(New Series("Average Check Size") With {
            .ChartType = SeriesChartType.Line,
            .XValueType = ChartValueType.Date,
            .ToolTip = "Date: #VALX{dd MMM yyyy}" & vbCrLf &
                       "Avg Check: ₱#VALY{N2}"
        })

        AvgCheckSizeChart.Series.Add(New Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2")) With {
            .ChartType = SeriesChartType.Line,
            .BorderDashStyle = ChartDashStyle.Dash,
            .XValueType = ChartValueType.Date,
            .ToolTip = "Target: ₱#VALY{N2}"
        })

        AvgCheckSizeChart.Titles.Add("Average Check Size")
        Panel3.Controls.Add(AvgCheckSizeChart)



        TopSellingItemChart = New Chart() With {.Dock = DockStyle.Fill}
        Dim topArea As New ChartArea("TopItemArea")
        TopSellingItemChart.ChartAreas.Add(topArea)

        TopSellingItemChart.Series.Add(New Series("Top Items") With {
            .ChartType = SeriesChartType.Column,
            .ToolTip = "Item: #VALX" & vbCrLf &
                       "Sold: #VALY"
        })

        TopSellingItemChart.Titles.Add("Top Selling Items")
        Panel4.Controls.Add(TopSellingItemChart)

    End Sub

    ' AUTO-REFRESH
    Private Sub autoRefreshTimer_Tick(sender As Object, e As EventArgs) Handles autoRefreshTimer.Tick
        Try
            ApplyFilter()
        Catch ex As Exception
            Debug.WriteLine("Auto-refresh failed: " & ex.Message)
        End Try
    End Sub

End Class
