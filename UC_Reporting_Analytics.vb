Imports System.Windows.Forms.DataVisualization.Charting
' Note: System.Collections.Generic is implicitly used via fully qualified names (System.Collections.Generic.List)

Public Class UC_Reporting_Analytics

    ' Chart Control Declarations
    Private TotalSalesChart As Chart
    Private OrderCountChart As Chart
    Private AvgCheckSizeChart As Chart
    Private TopSellingItemChart As Chart

    ' Constant for the Target Line
    Private Const AvgCheckSizeTarget As Decimal = 35D

    ' Class for Time-Series Data (Panels 1, 2, 3)
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

    ' Class for Categorical Data (Panel 4)
    Public Class TopItemSale
        Public Property ItemName As String
        Public Property itemsSold As Integer

        Public Sub New(itemName As String, itemsSold As Integer)
            Me.ItemName = itemName
            Me.itemsSold = itemsSold
        End Sub
    End Class

    Public Sub New()
        InitializeComponent()

        ' 1. Initialize chart controls (done only once)
        InitializeChartControls()

        ' 2. Set default date and load initial data
        Dim defaultEndDate As DateTime = DateTime.Now.Date
        Dim defaultStartDate As DateTime = defaultEndDate.AddDays(-6)

        LoadAnalyticsData(defaultStartDate, defaultEndDate)
    End Sub

    ' Call all structure setup methods once
    Private Sub InitializeChartControls()
        InitializeTotalSalesChartStructure()
        InitializeOrderCountChartStructure()
        InitializeAvgCheckSizeChartStructure()
        InitializeTopSellingItemChartStructure()
    End Sub


    Private Sub LoadAnalyticsData(startDate As DateTime, endDate As DateTime)

        ' Format date range for dynamic titles
        Dim dateRangeTitle As String = $" ({startDate.ToString("yyyy-MM-dd")} to {endDate.ToString("yyyy-MM-dd")})"

        ' 1. Retrieve data based on the date range
        Dim timeSeriesData As System.Collections.Generic.List(Of SalesDataPoint) = GetSampleTimeSeriesData(startDate, endDate)
        Dim itemData As System.Collections.Generic.List(Of TopItemSale) = GetSampleTopItemData()

        ' 2. Bind data to charts and update titles

        ' Panel 1: Total Sales
        TotalSalesChart.DataSource = timeSeriesData
        TotalSalesChart.DataBind()
        TotalSalesChart.Titles(0).Text = "Total Sales" & dateRangeTitle

        ' Panel 2: Order Count
        OrderCountChart.DataSource = timeSeriesData
        OrderCountChart.DataBind()
        OrderCountChart.Titles(0).Text = "Order Count" & dateRangeTitle

        ' Panel 3: Average Check Size (Uses custom binding for conditional colors)
        AvgCheckSizeChart.Titles(0).Text = "Average Check Size" & dateRangeTitle
        ApplyAvgCheckSizeConditionalColors(timeSeriesData)

        ' Panel 4: Top Selling Item
        TopSellingItemChart.DataSource = itemData
        TopSellingItemChart.DataBind()
        TopSellingItemChart.Titles(0).Text = "Top Selling Item" & dateRangeTitle
    End Sub


    Private Sub InitializeTotalSalesChartStructure()
        TotalSalesChart = New Chart()
        Panel1.Controls.Add(TotalSalesChart)
        TotalSalesChart.Dock = DockStyle.Fill

        Dim chartArea1 As New ChartArea("ChartArea1")
        TotalSalesChart.ChartAreas.Add(chartArea1)

        chartArea1.AxisX.Title = "Date/Period"
        chartArea1.AxisY.Title = "Total Sales (₱)"
        chartArea1.AxisY.Minimum = 0
        chartArea1.AxisX.LabelStyle.Format = "MM/dd"

        Dim areaSeries As New Series("Total Sales")
        areaSeries.ChartType = SeriesChartType.Area
        areaSeries.ChartArea = "ChartArea1"
        areaSeries.XValueMember = "DateValue"
        areaSeries.YValueMembers = "SalesAmount"
        areaSeries.ToolTip = "Date: #VALX{MM/dd} \nSales: #VALY{C0}"

        ' VISUAL POLISH: Area Chart Gradient
        areaSeries.Color = Drawing.Color.FromArgb(128, Drawing.Color.SteelBlue)
        areaSeries.BackSecondaryColor = Drawing.Color.FromArgb(0, Drawing.Color.SteelBlue)
        areaSeries.BackGradientStyle = GradientStyle.TopBottom
        areaSeries.BorderColor = Drawing.Color.DarkBlue
        areaSeries.BorderWidth = 2

        TotalSalesChart.Series.Add(areaSeries)
        TotalSalesChart.Titles.Add("") ' Initial empty title
    End Sub

    Private Sub InitializeOrderCountChartStructure()
        OrderCountChart = New Chart()
        Panel2.Controls.Add(OrderCountChart)
        OrderCountChart.Dock = DockStyle.Fill

        Dim chartArea2 As New ChartArea("ChartArea2")
        OrderCountChart.ChartAreas.Add(chartArea2)

        chartArea2.AxisX.Title = "Date/Period"
        chartArea2.AxisY.Title = "Order Count"
        chartArea2.AxisY.Minimum = 0
        chartArea2.AxisX.LabelStyle.Format = "MM/dd"

        Dim lineSeries As New Series("Order Count")
        lineSeries.ChartType = SeriesChartType.Line
        lineSeries.BorderWidth = 3
        lineSeries.Color = Drawing.Color.ForestGreen
        lineSeries.ChartArea = "ChartArea2"
        lineSeries.XValueMember = "DateValue"
        lineSeries.YValueMembers = "OrderCount"

        ' ADDED TOOLTIP
        lineSeries.ToolTip = "Date: #VALX{MM/dd} \nOrders: #VALY{N0}"

        OrderCountChart.Series.Add(lineSeries)
        OrderCountChart.Titles.Add("") ' Initial empty title
    End Sub

    Private Sub InitializeAvgCheckSizeChartStructure()
        AvgCheckSizeChart = New Chart()
        Panel3.Controls.Add(AvgCheckSizeChart)
        AvgCheckSizeChart.Dock = DockStyle.Fill

        Dim chartArea3 As New ChartArea("ChartArea3")
        AvgCheckSizeChart.ChartAreas.Add(chartArea3)

        chartArea3.AxisX.Title = "Date/Period"
        chartArea3.AxisY.Title = "Average Check Size (₱)"
        chartArea3.AxisY.Minimum = 20D

        ' FIX FOR DATE FORMATTING
        chartArea3.AxisX.LabelStyle.Format = "MM/dd"
        chartArea3.AxisX.LabelStyle.IsStaggered = True
        chartArea3.AxisX.IntervalType = DateTimeIntervalType.Days
        chartArea3.AxisX.MajorGrid.LineColor = Drawing.Color.LightGray

        ' Actual Average Line 
        Dim actualSeries As New Series("Average Check Size")
        actualSeries.ChartType = SeriesChartType.Line
        actualSeries.BorderWidth = 3
        actualSeries.Color = Drawing.Color.Gray
        actualSeries.ChartArea = "ChartArea3"
        actualSeries.XValueType = ChartValueType.DateTime

        ' ADDED TOOLTIP (Uses {C2} for currency format with 2 decimal places)
        actualSeries.ToolTip = "Date: #VALX{MM/dd} \nAvg Check: #VALY{C2}"

        AvgCheckSizeChart.Series.Add(actualSeries)


        Dim targetSeries As New Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2"))
        targetSeries.ChartType = SeriesChartType.Line
        targetSeries.BorderWidth = 2
        targetSeries.Color = Drawing.Color.Red
        targetSeries.BorderDashStyle = ChartDashStyle.Dash
        targetSeries.ChartArea = "ChartArea3"
        targetSeries.XValueType = ChartValueType.DateTime

        AvgCheckSizeChart.Series.Add(targetSeries)
        AvgCheckSizeChart.Titles.Add("") ' Initial empty title
    End Sub

    Private Sub InitializeTopSellingItemChartStructure()
        TopSellingItemChart = New Chart()
        Panel4.Controls.Add(TopSellingItemChart)
        TopSellingItemChart.Dock = DockStyle.Fill

        Dim chartArea4 As New ChartArea("ChartArea4")
        TopSellingItemChart.ChartAreas.Add(chartArea4)

        chartArea4.AxisX.Title = "Item Name"
        chartArea4.AxisY.Title = "Items Sold"
        chartArea4.AxisY.Minimum = 0
        chartArea4.AxisX.LabelStyle.IsStaggered = True

        Dim columnSeries As New Series("Items Sold")
        columnSeries.ChartType = SeriesChartType.Column
        columnSeries.ChartArea = "ChartArea4"
        columnSeries.XValueMember = "ItemName"
        columnSeries.YValueMembers = "ItemsSold"
        columnSeries.IsValueShownAsLabel = True

        ' MOUSEOVER TOOLTIP FEATURE
        columnSeries.ToolTip = "#VALX - #VALY Items sold"

        ' VISUAL POLISH: Bar Chart Colors
        columnSeries.Color = Drawing.Color.DarkOrange
        columnSeries.LabelForeColor = Drawing.Color.Black

        TopSellingItemChart.Series.Add(columnSeries)
        TopSellingItemChart.Titles.Add("") ' Initial empty title
    End Sub

    Private Sub ApplyAvgCheckSizeConditionalColors(data As System.Collections.Generic.List(Of SalesDataPoint))
        Dim actualSeries As Series = AvgCheckSizeChart.Series("Average Check Size")
        Dim targetSeries As Series = AvgCheckSizeChart.Series("Target: ₱" & AvgCheckSizeTarget.ToString("N2"))


        ' CRITICAL FIX: Clear both series before re-adding points
        actualSeries.Points.Clear()
        targetSeries.Points.Clear()

        ' 1. Manually add points for the actual series to apply conditional colors
        For Each dp As SalesDataPoint In data
            Dim p As DataPoint = New DataPoint()
            p.XValue = dp.DateValue.ToOADate()
            p.YValues(0) = dp.AvgCheckSize

            If dp.AvgCheckSize >= AvgCheckSizeTarget Then
                p.Color = Drawing.Color.Green
                p.MarkerStyle = MarkerStyle.Circle
                p.MarkerColor = Drawing.Color.Green
            Else
                p.Color = Drawing.Color.OrangeRed
                p.MarkerStyle = MarkerStyle.Cross
                p.MarkerColor = Drawing.Color.OrangeRed
            End If
            actualSeries.Points.Add(p)
        Next

        ' 2. Manually re-add points for the Target series
        For Each dp As SalesDataPoint In data
            targetSeries.Points.AddXY(dp.DateValue.ToOADate(), dp.TargetValue)
        Next

        AvgCheckSizeChart.Invalidate()
    End Sub

    Private Sub ApplyFilterButton_Click(sender As Object, e As EventArgs) Handles ApplyFilterButton.Click
        Dim endDate As DateTime = DateFilterPicker.Value.Date
        Dim startDate As DateTime = endDate.AddDays(-6)

        LoadAnalyticsData(startDate, endDate)
    End Sub


    Private Function GetSampleTimeSeriesData(startDate As DateTime, endDate As DateTime) As System.Collections.Generic.List(Of SalesDataPoint)
        ' *** Replace this logic with your actual database query ***

        Dim data As New System.Collections.Generic.List(Of SalesDataPoint)
        Dim currentDate As DateTime = startDate
        Dim days As Integer = 0

        Do While currentDate <= endDate
            ' Dummy data simulation
            Dim sales As Decimal = 1500D + (100 * days) + Rnd() * 500
            Dim orders As Integer = 15 + days * 2 + CInt(Rnd() * 5)
            Dim avgSize As Decimal = sales / orders

            data.Add(New SalesDataPoint(currentDate, sales, orders, avgSize, AvgCheckSizeTarget))

            currentDate = currentDate.AddDays(1)
            days += 1
        Loop

        Return data
    End Function

    Private Function GetSampleTopItemData() As System.Collections.Generic.List(Of TopItemSale)
        ' *** Replace this logic with your actual database query for top items ***

        Dim data As New System.Collections.Generic.List(Of TopItemSale)
        data.Add(New TopItemSale("Soda", 710))
        data.Add(New TopItemSale("Fries", 620))
        data.Add(New TopItemSale("Milkshake", 550))
        data.Add(New TopItemSale("Classic Burger", 450))
        data.Add(New TopItemSale("Deluxe Salad", 180))

        Return data.OrderByDescending(Function(i) i.ItemsSold).ToList()
    End Function



    Private Sub TableLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles TableLayoutPanel1.Paint
    End Sub

    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint
    End Sub

    Private Sub Panel2_Paint(sender As Object, e As PaintEventArgs) Handles Panel2.Paint
    End Sub

    Private Sub Panel3_Paint(sender As Object, e As PaintEventArgs) Handles Panel3.Paint
    End Sub

    Private Sub Panel4_Paint(sender As Object, e As EventArgs) Handles Panel4.Paint
    End Sub

End Class