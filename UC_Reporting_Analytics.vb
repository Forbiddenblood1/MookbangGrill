Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Data.SqlClient

Public Class UC_Reporting_Analytics

    Private connectionString As String = "Server=localhost\SQLEXPRESS;Database=POS;Integrated Security=True;TrustServerCertificate=True;"

    Private TotalSalesChart As Chart
    Private OrderCountChart As Chart
    Private AvgCheckSizeChart As Chart
    Private TopSellingItemChart As Chart

    Private Const AvgCheckSizeTarget As Decimal = 35D

    ' ===========================
    '   DATA CLASSES
    ' ===========================
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
        ApplyFilter()
    End Sub

    ' ==========================================
    '   FILTER COMBOBOX
    ' ==========================================
    Private Sub InitializeFilterComboBox()
        FilterComboBox.Items.Clear()
        FilterComboBox.Items.AddRange({"Current Day", "Last 7 Days", "This Month", "This Year"})
        FilterComboBox.SelectedIndex = 1
        AddHandler FilterComboBox.SelectedIndexChanged, AddressOf FilterComboBox_SelectedIndexChanged
    End Sub

    Private Sub FilterComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        Dim startDate As DateTime
        Dim endDate As DateTime = DateTime.Now.Date
        Dim titlePrefix As String = ""

        Select Case FilterComboBox.SelectedItem.ToString()
            Case "Current Day"
                startDate = endDate
                titlePrefix = "Today"
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

    ' ==========================================
    '   LOAD ANALYTICS
    ' ==========================================
    Private Sub LoadAnalyticsData(startDate As DateTime, endDate As DateTime, titlePrefix As String)
        Dim timeSeriesData = GetTimeSeriesData(startDate, endDate)
        Dim totalEarnings As Decimal = GetTotalEarnings(startDate, endDate)
        Dim itemData = GetTopItemData(startDate, endDate)

        ' Total Sales
        TotalSalesChart.DataSource = timeSeriesData
        TotalSalesChart.Series(0).XValueMember = "DateValue"
        TotalSalesChart.Series(0).YValueMembers = "SalesAmount"
        TotalSalesChart.DataBind()
        TotalSalesChart.Titles(0).Text = $"Total Sales - ₱{totalEarnings:N2}"

        ' Order Count
        OrderCountChart.DataSource = timeSeriesData
        OrderCountChart.Series(0).XValueMember = "DateValue"
        OrderCountChart.Series(0).YValueMembers = "OrderCount"
        OrderCountChart.DataBind()
        OrderCountChart.Titles(0).Text = $"Order Count - {titlePrefix}"

        ' Avg Check Size
        AvgCheckSizeChart.Titles(0).Text = $"Average Check Size - ₱{totalEarnings:N2}"
        ApplyAvgCheckSizeConditionalColors(timeSeriesData)

        ' Top Selling Items (FILTER-BASED)
        TopSellingItemChart.DataSource = itemData
        TopSellingItemChart.Series(0).XValueMember = "ItemName"
        TopSellingItemChart.Series(0).YValueMembers = "ItemsSold"
        TopSellingItemChart.DataBind()
        TopSellingItemChart.Titles(0).Text = $"Top Selling Items - {titlePrefix}"
    End Sub

    ' ==========================================
    '   GET TOTAL EARNINGS
    ' ==========================================
    Private Function GetTotalEarnings(startDate As DateTime, endDate As DateTime) As Decimal
        Using conn As New SqlConnection(connectionString)
            conn.Open()
            Dim query As String = "
                SELECT SUM(OI.Subtotal)
                FROM Orders O
                INNER JOIN OrderItems OI ON O.OrderID = OI.OrderID
                WHERE O.CreatedAt BETWEEN @Start AND @End
                  AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')"

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Start", startDate)
                cmd.Parameters.AddWithValue("@End", endDate.AddDays(1).AddSeconds(-1))

                Dim result = cmd.ExecuteScalar()
                If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                    Return Convert.ToDecimal(result)
                End If
            End Using
        End Using

        Return 0D
    End Function

    ' ==========================================
    '   GET TIME SERIES DATA
    ' ==========================================
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
                WHERE O.CreatedAt BETWEEN @StartDate AND @EndDate
                  AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')
                GROUP BY CAST(O.CreatedAt AS DATE)
                ORDER BY SalesDate"

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@StartDate", startDate)
                cmd.Parameters.AddWithValue("@EndDate", endDate.AddDays(1).AddSeconds(-1))

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

        ' Fill missing dates
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

    ' ==========================================
    '   GET TOP SELLING ITEMS (FILTER-BASED)
    ' ==========================================
    Private Function GetTopItemData(startDate As DateTime, endDate As DateTime) As List(Of TopItemSale)
        Dim result As New List(Of TopItemSale)

        Using conn As New SqlConnection(connectionString)
            conn.Open()
            Dim query As String = "
                SELECT P.ProductName, SUM(OI.Quantity) AS ItemsSold
                FROM OrderItems OI
                INNER JOIN Orders O ON OI.OrderID = O.OrderID
                INNER JOIN Products P ON OI.ProductID = P.ProductID
                WHERE O.CreatedAt BETWEEN @Start AND @End
                  AND (O.OrderStatus='Completed' OR O.OrderStatus='Paid')
                GROUP BY P.ProductName
                ORDER BY ItemsSold DESC"

            Using cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Start", startDate)
                cmd.Parameters.AddWithValue("@End", endDate.AddDays(1).AddSeconds(-1))

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

    ' ==========================================
    '   DRAW AVG CHECK SIZE
    ' ==========================================
    Private Sub ApplyAvgCheckSizeConditionalColors(data As List(Of SalesDataPoint))
        Dim actualSeries = AvgCheckSizeChart.Series("Average Check Size")
        Dim targetSeries = AvgCheckSizeChart.Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2"))

        actualSeries.Points.Clear()
        targetSeries.Points.Clear()

        For Each dp In data
            Dim p As New DataPoint(dp.DateValue.ToOADate(), dp.AvgCheckSize)

            If dp.AvgCheckSize >= AvgCheckSizeTarget Then
                p.Color = Color.Green
                p.MarkerStyle = MarkerStyle.Circle
            Else
                p.Color = Color.OrangeRed
                p.MarkerStyle = MarkerStyle.Cross
            End If

            actualSeries.Points.Add(p)
            targetSeries.Points.AddXY(dp.DateValue.ToOADate(), dp.TargetValue)
        Next

        AvgCheckSizeChart.Invalidate()
    End Sub

    ' ==========================================
    '   INITIALIZE CHART CONTROLS
    ' ==========================================
    Private Sub InitializeChartControls()
        InitializeTotalSalesChartStructure()
        InitializeOrderCountChartStructure()
        InitializeAvgCheckSizeChartStructure()
        InitializeTopSellingItemChartStructure()
    End Sub

    ' ==== Total Sales Chart ====
    Private Sub InitializeTotalSalesChartStructure()
        TotalSalesChart = New Chart()
        Panel1.Controls.Add(TotalSalesChart)
        TotalSalesChart.Dock = DockStyle.Fill

        Dim chartArea As New ChartArea("ChartArea1")
        TotalSalesChart.ChartAreas.Add(chartArea)

        Dim series As New Series("Total Sales")
        series.ChartType = SeriesChartType.Area
        series.XValueType = ChartValueType.DateTime
        series.Color = Color.FromArgb(128, Color.SteelBlue)
        series.BorderWidth = 2

        TotalSalesChart.Series.Add(series)
        TotalSalesChart.Titles.Add("")
    End Sub

    ' ==== Order Count ====
    Private Sub InitializeOrderCountChartStructure()
        OrderCountChart = New Chart()
        Panel2.Controls.Add(OrderCountChart)
        OrderCountChart.Dock = DockStyle.Fill

        Dim chartArea As New ChartArea("ChartArea2")
        OrderCountChart.ChartAreas.Add(chartArea)

        Dim series As New Series("Order Count")
        series.ChartType = SeriesChartType.Line
        series.BorderWidth = 3
        series.Color = Color.ForestGreen
        OrderCountChart.Series.Add(series)

        OrderCountChart.Titles.Add("")
    End Sub

    ' ==== Avg Check Size ====
    Private Sub InitializeAvgCheckSizeChartStructure()
        AvgCheckSizeChart = New Chart()
        Panel3.Controls.Add(AvgCheckSizeChart)
        AvgCheckSizeChart.Dock = DockStyle.Fill

        Dim chartArea As New ChartArea("ChartArea3")
        AvgCheckSizeChart.ChartAreas.Add(chartArea)

        Dim actualSeries As New Series("Average Check Size")
        actualSeries.ChartType = SeriesChartType.Line
        actualSeries.BorderWidth = 3
        actualSeries.Color = Color.Gray

        Dim targetSeries As New Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2"))
        targetSeries.ChartType = SeriesChartType.Line
        targetSeries.BorderDashStyle = ChartDashStyle.Dash
        targetSeries.Color = Color.Red

        AvgCheckSizeChart.Series.Add(actualSeries)
        AvgCheckSizeChart.Series.Add(targetSeries)
        AvgCheckSizeChart.Titles.Add("")
    End Sub

    ' ==== Top Selling Items ====
    Private Sub InitializeTopSellingItemChartStructure()
        TopSellingItemChart = New Chart()
        Panel4.Controls.Add(TopSellingItemChart)
        TopSellingItemChart.Dock = DockStyle.Fill

        Dim chartArea As New ChartArea("ChartArea4")
        TopSellingItemChart.ChartAreas.Add(chartArea)

        Dim series As New Series("Items Sold")
        series.ChartType = SeriesChartType.Column
        series.IsValueShownAsLabel = True

        TopSellingItemChart.Series.Add(series)
        TopSellingItemChart.Titles.Add("")
    End Sub

    Private Sub FilterComboBox_SelectedIndexChanged_1(sender As Object, e As EventArgs) Handles FilterComboBox.SelectedIndexChanged

    End Sub

    Private Sub FilterLabel_Click(sender As Object, e As EventArgs)

    End Sub
End Class
