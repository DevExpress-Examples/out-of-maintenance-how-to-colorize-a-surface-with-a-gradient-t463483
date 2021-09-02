Imports DevExpress.Utils
Imports System
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Resources
Imports System.Windows.Threading

Namespace GradientHeightMap
	Public Class HeightMapViewModel
		Public Shared ReadOnly Property HeightMapUri() As Uri
			Get
				Return AssemblyHelper.GetResourceUri(GetType(HeightMapViewModel).Assembly, "Data/Heightmap.jpg")
			End Get
		End Property
		Public Property ImageData() As ImageData
		Public Sub New()
			GenerateMap(HeightMapUri)
		End Sub
		Private Sub GenerateMap(ByVal uri As Uri)
			Dim imageDataLoader As New ImageDataLoader(uri)
			Dim pixels(,) As PixelColor = imageDataLoader.GetPixels()
			Dim countX As Integer = pixels.GetLength(0)
			Dim countZ As Integer = pixels.GetLength(1)
			Dim startX As Integer = 0
			Dim startZ As Integer = 0
			Dim gridStep As Integer = 100
			Dim minY As Double = -300
			Dim maxY As Double = 3000
'INSTANT VB NOTE: The variable imageData was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim imageData_Conflict As New ImageData(New DoubleCollection(countX), New DoubleCollection(countZ), New DoubleCollection(countX * countZ))
			Dim fullZ As Boolean = False
			For i As Integer = 0 To countX - 1
				imageData_Conflict.XArguments.Add(startX + i * gridStep)
				For j As Integer = 0 To countZ - 1
					If Not fullZ Then
						imageData_Conflict.YArguments.Add(startZ + j * gridStep)
					End If
					Dim value As Double = GetValue(pixels(i, j), minY, maxY)
					imageData_Conflict.Values.Add(value)
				Next j
				fullZ = True
			Next i
			ImageData = imageData_Conflict
		End Sub
		Private Function GetValue(ByVal color As PixelColor, ByVal min As Double, ByVal max As Double) As Double
			Dim normalizedValue As Double = CDbl(color.Gray) / 255.0
			Return min + normalizedValue * (max - min)
		End Function
	End Class
	Public Class ImageDataLoader
		Private ReadOnly streamResourceInfo As StreamResourceInfo
		Public Sub New(ByVal uri As Uri)
			Me.streamResourceInfo = Application.GetResourceStream(uri)
		End Sub
		Private Function GetPixels(ByVal source As BitmapSource) As PixelColor(,)
			If source.Format <> PixelFormats.Bgra32 Then
				source = New FormatConvertedBitmap(source, PixelFormats.Bgra32, Nothing, 0)
			End If
			Dim result(source.PixelWidth - 1, source.PixelHeight - 1) As PixelColor
			Dim stride As Integer = CInt(source.PixelWidth) * (source.Format.BitsPerPixel \ 8)
			CopyPixels(source, result, stride, 0)
			Return result
		End Function
		Private Sub CopyPixels(ByVal source As BitmapSource, ByVal pixels(,) As PixelColor, ByVal stride As Integer, ByVal offset As Integer)
			Dim height = source.PixelHeight
			Dim width = source.PixelWidth
			Dim pixelBytes = New Byte((height * width * 4) - 1){}
			source.CopyPixels(pixelBytes, stride, 0)
			Dim y0 As Integer = offset \ width
			Dim x0 As Integer = offset - width * y0
			For y As Integer = 0 To height - 1
				For x As Integer = 0 To width - 1
					pixels(x + x0, y + y0) = New PixelColor With {
						.Blue = pixelBytes((y * width + x) * 4 + 0),
						.Green = pixelBytes((y * width + x) * 4 + 1),
						.Red = pixelBytes((y * width + x) * 4 + 2),
						.Alpha = pixelBytes((y * width + x) * 4 + 3)
					}
				Next x
			Next y
		End Sub
		Private Sub DoEvents()
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, New Action(Sub()
			End Sub))
		End Sub
		Public Function GetPixels() As PixelColor(,)
			Dim pixels(-1, -1) As PixelColor
			Try
				Dim source As New BitmapImage()
				source.BeginInit()
				source.StreamSource = Me.streamResourceInfo.Stream
				source.EndInit()
				Do While source.IsDownloading
					DoEvents()
				Loop
				pixels = GetPixels(source)
			Catch
			End Try
			Return pixels
		End Function
	End Class
	Public Structure PixelColor
		Public Blue As Byte
		Public Green As Byte
		Public Red As Byte
		Public Alpha As Byte
		Public ReadOnly Property Gray() As Byte
			Get
				Return CByte((CInt(Blue) + CInt(Green) + CInt(Red)) \ 3)
			End Get
		End Property
	End Structure
End Namespace