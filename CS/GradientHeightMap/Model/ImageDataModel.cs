using System.Windows.Media;

namespace GradientHeightMap {
    public class ImageData {
        public DoubleCollection XArguments { get; set; }
        public DoubleCollection YArguments { get; set; }
        public DoubleCollection Values { get; set; }
        public ImageData(DoubleCollection xArguments, DoubleCollection yArguments, DoubleCollection values) {
            this.XArguments = xArguments;
            this.YArguments = yArguments;
            this.Values = values;
        }
    }
}