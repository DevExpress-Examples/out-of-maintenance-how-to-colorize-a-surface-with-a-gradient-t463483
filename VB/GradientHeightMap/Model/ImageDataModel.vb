Imports System.Windows.Media

Namespace GradientHeightMap
	Public Class ImageData
		Public Property XArguments() As DoubleCollection
		Public Property YArguments() As DoubleCollection
		Public Property Values() As DoubleCollection
		Public Sub New(ByVal xArguments As DoubleCollection, ByVal yArguments As DoubleCollection, ByVal values As DoubleCollection)
			Me.XArguments = xArguments
			Me.YArguments = yArguments
			Me.Values = values
		End Sub
	End Class
End Namespace