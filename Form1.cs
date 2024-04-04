using OpenCvSharp;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using System.IO;
using Point = OpenCvSharp.Point;

namespace imgpros
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif; *.bmp)|*.jpg; *.jpeg; *.png; *.gif; *.bmp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Dispose of the existing image to free up resources
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                    }

                    try
                    {
                        string filePath = openFileDialog.FileName;
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox1.Image = Image.FromFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image loaded in the PictureBox.");
                return;
            }

            // Directly convert the Image from pictureBox1 to a Bitmap, if it's not already one.
            Bitmap bitmap = pictureBox1.Image as Bitmap ?? new Bitmap(pictureBox1.Image);

            using (Mat src = BitmapConverter.ToMat(bitmap))
            {
                // Convert the image to grayscale
                using (Mat gray = new Mat())
                {
                    Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                    // Apply thresholding to obtain a binary image
                    using (Mat binary = new Mat())
                    {
                        Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Binary);

                        pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

                        // Display the binary image in pictureBox2
                        // Directly convert and assign the binary Mat to pictureBox2 without intermediate Bitmap
                        pictureBox2.Image?.Dispose(); // Dispose previous image if exists
                        pictureBox2.Image = BitmapConverter.ToBitmap(binary);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if the PictureBox contains an image
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return; // Exit early if no image
            }

            // Directly use pictureBox2's image as a Bitmap if possible
            Bitmap bitmap = pictureBox2.Image as Bitmap;
            if (bitmap == null)
            {
                MessageBox.Show("Error: The image cannot be processed.");
                return;
            }

            // Use 'using' to ensure resources are released properly
            using (Mat src = BitmapConverter.ToMat(bitmap))
            {
                // Generate a unique filename based on timestamp and a GUID
                string filename = $"image_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.png";
                string directoryPath = @"C:\Users\ykoma\source\repos\imgpros\img\"; // Consider using a more dynamic path

                // Ensure the directory exists
                Directory.CreateDirectory(directoryPath); // No effect if already exists

                string filePath = Path.Combine(directoryPath, filename);

                // Save the image
                Cv2.ImWrite(filePath, src);
            }

            MessageBox.Show("Image saved successfully.");
        }


        //private void button4_Click(object sender, EventArgs e)
        //{

        //    // Check if the PictureBox contains an image
        //    if (pictureBox2.Image != null)
        //    {

        //        Image image = pictureBox2.Image;

        //        // Convert the Image object to a Bitmap object
        //        Bitmap bitmap = new Bitmap(image);

        //        // Convert the Bitmap object to a Mat object
        //        Mat src = BitmapConverter.ToMat(bitmap);


        //        pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

        //        // Display the selected image
        //        // Convert the binary Mat object to a Bitmap object
        //        Bitmap binaryBitmap = BitmapConverter.ToBitmap(src);

        //        // Display the binary image in pictureBox1
        //        pictureBox1.Image = binaryBitmap;
        //        pictureBox2.Image = null;


        //    }
        //    else
        //    {
        //        MessageBox.Show("No image found in PictureBox.");
        //    }
        //}


        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            // Convert pictureBox2's Image to a Bitmap (if it's not already a Bitmap)
            Bitmap sourceBitmap = pictureBox2.Image as Bitmap;

            // Assuming you want to ensure the image is in BGR format before transferring
            using (Mat srcMat = BitmapConverter.ToMat(sourceBitmap))
            {
                Mat processedMat = new Mat();

                // Check if the source image is grayscale; convert it to BGR if it is.
                if (srcMat.Channels() == 1)
                {
                    Cv2.CvtColor(srcMat, processedMat, ColorConversionCodes.GRAY2BGR);
                }
                else
                {
                    processedMat = srcMat.Clone();
                }
                // Convert the processed Mat back to a Bitmap to display in pictureBox1
                Bitmap processedBitmap = BitmapConverter.ToBitmap(processedMat);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Image = processedBitmap;
            }

            // Clear the image in pictureBox2
            pictureBox2.Image = null;
        }


        //button5_Click
        private void button5_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            var originalImage = BitmapConverter.ToMat((Bitmap)pictureBox1.Image);
            var resizedImage = ResizeImage(originalImage, 0.25); // Resize to 25% of the original size for efficiency
            var binaryImage = ConvertToBinary(resizedImage);
            var contours = FindContours(binaryImage);

            foreach (var contour in contours)
            {
                // Assuming the CalculateContourCenter and other preparation steps are correct
                var center = CalculateContourCenter(contour);
                if (center == null) continue; // Skip if center can't be calculated

                //EnhanceContourDisplay(contour, resizedImage, center.Value);
                DrawContourLines(contour, resizedImage);
            }

            DisplayResult(resizedImage,"result");
        }



        private void DisplayResult(Mat image,String text)
        {
            Cv2.ImShow(text, image);
            pictureBox2.Image = BitmapConverter.ToBitmap(image);
        }



        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            Mat originalImage = BitmapConverter.ToMat((Bitmap)pictureBox1.Image);
            Mat resizedImage = ResizeImage(originalImage, 0.25); // Resize for efficiency
            Mat binaryImage = ConvertToBinary(resizedImage);
            Point[][] contours = FindContours(binaryImage);

            Mat resultImage = resizedImage.Clone();

            foreach (var contour in contours)
            {
                // Assuming the CalculateContourCenter and other preparation steps are correct
                var center = CalculateContourCenter(contour);
                if (center == null) continue; // Skip if center can't be calculated
                ContourAndCinterPointDisplay(contour, resizedImage, center.Value);
            }

            DisplayResult(resizedImage,"draw center point");
        }



        private void button7_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            Bitmap sourceBitmap = pictureBox1.Image as Bitmap ?? new Bitmap(pictureBox1.Image);
            using (Mat originalImage = BitmapConverter.ToMat(sourceBitmap))
            {
                Mat resizedImage = ResizeImage(originalImage, 0.25); // Assuming ResizeImage and ConvertToBinary are defined elsewhere
                Mat binaryImage = ConvertToBinary(resizedImage);
                Point[][] contours = FindContours(binaryImage);

                Mat resultImage = resizedImage.Clone();

                foreach (var contour in contours)
                {
                    var boundingRect = Cv2.BoundingRect(contour);
                    Cv2.Rectangle(resultImage, boundingRect, Scalar.Green, 2);

                    RotatedRect rotatedRect = Cv2.MinAreaRect(contour);
                    Point2f[] vertices = Cv2.BoxPoints(rotatedRect);
                    Point[] points = Array.ConvertAll(vertices, point => new Point((int)point.X, (int)point.Y));
                    Cv2.Polylines(resultImage, new Point[][] { points }, isClosed: true, color: Scalar.Red, thickness: 2);

                    Point midpoint = CalculateMidpoint(points[0], points[1]);
                    double angleWithHorizontalDegrees = CalculateAngleDegrees(points[0], points[1]);

                    // Draw angle lines
                    DrawAngleLines(resultImage, midpoint, angleWithHorizontalDegrees);

                    Cv2.PutText(resultImage, $"Angle: {angleWithHorizontalDegrees:F2}°", new Point(10, 120), HersheyFonts.HersheyPlain, 1, Scalar.White, 2);
                }

                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.Image?.Dispose(); // Dispose previous image if exists
                DisplayResult(resultImage, "Draw center point, minimum bounding box, and angle lines");

                //pictureBox2.Image = BitmapConverter.ToBitmap(resultImage);
            }
        }


        private void button9_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            Mat originalImage = BitmapConverter.ToMat((Bitmap)pictureBox1.Image);
            Mat resizedImage = originalImage.Resize(new OpenCvSharp.Size(originalImage.Width / 4, originalImage.Height / 4));
            Mat src = resizedImage.Clone();

            // Convert to grayscale and blur to reduce noise
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);

            // Edge detection
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 50, 150);

            // Find contours
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            int largestContourIndex = Array.FindIndex(contours, c => Cv2.ContourArea(c) == contours.Max(x => Cv2.ContourArea(x)));
            RotatedRect minAreaRect = Cv2.MinAreaRect(contours[largestContourIndex]);
            Point2f[] rectPoints = Cv2.BoxPoints(minAreaRect);
            Point[] points = Array.ConvertAll(rectPoints, point => new Point((int)point.X, (int)point.Y));

            // Draw the minimum area bounding box
            Cv2.Polylines(src, new[] { points }, true, new Scalar(0, 255, 0), 1);

            double pixelPerMetric = src.Width / 12.0; // Assuming 24cm is the known width
            double objectWidth = minAreaRect.Size.Width / pixelPerMetric;
            double objectHeight = minAreaRect.Size.Height / pixelPerMetric;

            // Put the size on the image
           // Cv2.PutText(src, $"W: {objectWidth:F1}cm, H: {objectHeight:F1}cm", new Point(20, 150), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);

            double angle = minAreaRect.Angle;
            if (minAreaRect.Size.Width < minAreaRect.Size.Height)
            {
                angle += 90;
            }
            Cv2.PutText(src, $"Angle: {angle:F2}°", new Point(10, 120), HersheyFonts.HersheyPlain, 1, Scalar.White, 2);
            // Calculate and display the angle

            // Draw lines to represent the full angle visually
            Point center = new Point((int)minAreaRect.Center.X, (int)minAreaRect.Center.Y);
            double radians = angle * Math.PI / 180.0;
            // Extend in both directions from the center
            Point lineEnd1 = new Point(center.X + (int)(100 * Math.Cos(radians)), center.Y + (int)(100 * Math.Sin(radians)));
            Point lineEnd2 = new Point(center.X - (int)(100 * Math.Cos(radians)), center.Y - (int)(100 * Math.Sin(radians)));
            Cv2.Line(src, center, lineEnd1, Scalar.BlueViolet, 2);
            Cv2.Line(src, center, lineEnd2, Scalar.BlueViolet, 2);

            DisplayResult(src, "Object Angle");

            // Show the image
            //Cv2.ImShow("Object Size and Full Angle", src);

        }

        private void DrawAngleLines(Mat image, Point midpoint, double angleDegrees)
        {
            double angleRadians = angleDegrees * (Math.PI / 180.0);
            int lineLength = 100;
            Point lineEnd1 = new Point(
                midpoint.X + (int)(lineLength * Math.Cos(angleRadians)),
                midpoint.Y + (int)(lineLength * Math.Sin(angleRadians)));
            Point lineEnd2 = new Point(
                midpoint.X - (int)(lineLength * Math.Cos(angleRadians)),
                midpoint.Y - (int)(lineLength * Math.Sin(angleRadians)));

            Cv2.Line(image, midpoint, lineEnd1, Scalar.Yellow, 1);
            Cv2.Line(image, midpoint, lineEnd2, Scalar.Yellow, 1);
        }

        private Point CalculateMidpoint(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
        }

        private double CalculateAngleDegrees(Point point1, Point point2)
        {
            double deltaY = point2.Y - point1.Y;
            double deltaX = point2.X - point1.X;
            return Math.Atan2(deltaY, deltaX) * (180.0 / Math.PI);
        }

        private void ContourAndCinterPointDisplay(Point[] contour, Mat image, Point center)
        {
            DrawCenter(center, image);
            DrawBoundingBox(contour, image);
            DrawContourLines(contour, image);

            // Drawing the center point coordinates and put the text
            string centerText = $"({center.X}, {center.Y})";
            Cv2.PutText(image, centerText, new Point(center.X + 70, center.Y + 50), HersheyFonts.HersheySimplex, 0.5, Scalar.White, 1, LineTypes.AntiAlias);
        }

        private Mat ResizeImage(Mat image, double scale)
        {
            Mat resizedImage = new Mat();
            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size((int)(image.Width * scale), (int)(image.Height * scale)));
            return resizedImage;
        }

        private Mat ConvertToBinary(Mat image)
        {
            Mat binary = new Mat();
            Cv2.CvtColor(image, binary, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(binary, binary, 128, 255, ThresholdTypes.Binary);
            return binary;
        }

        private Point[][] FindContours(Mat binaryImage)
        {
            Cv2.FindContours(binaryImage, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            return contours;
        }

        private Point? CalculateContourCenter(Point[] contour)
        {
            Moments moments = Cv2.Moments(contour);
            if (Math.Abs(moments.M00) < double.Epsilon) return null;
            int centerX = (int)(moments.M10 / moments.M00);
            int centerY = (int)(moments.M01 / moments.M00);
            return new Point(centerX, centerY);
        }





        private void DrawCenter(Point center, Mat image)
        {
            Cv2.Circle(image, center, 4, Scalar.Red, -1);
        }

        private void DrawBoundingBox(Point[] contour, Mat image)
        {
            var boundingRect = Cv2.BoundingRect(contour);
            Cv2.Rectangle(image, boundingRect, Scalar.Green, 2);
        }

        private void DrawContourLines(Point[] contour, Mat image)
        {
            Scalar greenColor = new Scalar(0, 255, 0); // Green

            // Draw contour lines
            for (int i = 0; i < contour.Length; i++)
            {
                Point startPoint = contour[i];
                Point endPoint = contour[(i + 1) % contour.Length];

                // Draw contour line
                Cv2.Line(image, startPoint, endPoint, greenColor, 2);
            }

        }



        private void ContourAndCenterPointDisplay(Point[] contour, Mat image, Point center)
        {
            // Draw the contour
            Cv2.DrawContours(image, new Point[][] { contour }, -1, Scalar.Green, 2);

            // Mark the center point
            Cv2.Circle(image, center, 5, Scalar.Red, -1);
        }



        private void button10_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image found in PictureBox.");
                return;
            }

            Mat originalImage = BitmapConverter.ToMat((Bitmap)pictureBox1.Image);
            Mat resizedImage = ResizeImage(originalImage, 0.25); // Resize for efficiency
            Mat binaryImage = ConvertToBinary(resizedImage);
            Point[][] contours = FindContours(binaryImage);

            Mat resultImage = resizedImage.Clone();

            foreach (var contour in contours)
            {
                var center = CalculateContourCenter(contour);
                if (center == null) continue; // Skip if center can't be calculated

                ContourAndCenterPointDisplay(contour, resultImage, center.Value);

                DrawLineAcrossThickestPart(contour, resultImage);
            }

            DisplayResult(resultImage, "Draw center point and line across the thickest part");
            //pictureBox2.Image = BitmapConverter.ToBitmap(resultImage);
        }

        private void DrawLineAcrossThickestPart(Point[] contour, Mat image)
        {
            // Calculate the moments of the contour
            Moments moments = Cv2.Moments(contour);

            // The center or centroid of the contour
            Point2f center = new Point2f((float)(moments.M10 / moments.M00), (float)(moments.M01 / moments.M00));

            // Calculate the angle of the major axis
            double angle = 0.5 * Math.Atan2(2 * moments.Mu11, moments.Mu20 - moments.Mu02);

            // Determine the direction vector perpendicular to the major axis
            Point2f direction = new Point2f((float)Math.Sin(angle), (float)-Math.Cos(angle));

            // Find farthest points along the direction vector
            Point farthestPositive = (Point)center;
            Point farthestNegative = (Point)center;
            double maxDistPositive = 0;
            double maxDistNegative = 0;

            foreach (Point pt in contour)
            {
                // Project the point onto the direction vector
                double projection = (pt.X - center.X) * direction.X + (pt.Y - center.Y) * direction.Y;

                // Update the farthest points in both directions
                if (projection > maxDistPositive)
                {
                    maxDistPositive = projection;
                    farthestPositive = pt;
                }
                else if (projection < maxDistNegative)
                {
                    maxDistNegative = projection;
                    farthestNegative = pt;
                }
            }



            // Create a line perpendicular to the major axis
            Point2f endPoint1 = new Point2f(
                center.X + 1000 * (float)Math.Sin(angle), // Arbitrarily large number to ensure the line goes beyond the contour
                center.Y - 1000 * (float)Math.Cos(angle)
            );
            Point2f endPoint2 = new Point2f(
                center.X - 1000 * (float)Math.Sin(angle), // Arbitrarily large number to ensure the line goes beyond the contour
                center.Y + 1000 * (float)Math.Cos(angle)
            );


            SplitImageAlongLine(image, endPoint1, endPoint2);
            // Draw the line between the farthest points on the contour
            Cv2.Line(image, farthestPositive, farthestNegative, new Scalar(0, 255, 255), 2); // Yellow line
        }

        private void SplitImageAlongLine(Mat image, Point2f point1, Point2f point2)
        {
            // Create a mask the same size as the image, initialized to zeros (black)
            Mat mask = Mat.Zeros(image.Size(), MatType.CV_8UC1);

            // Determine the thickness for the line used to split the image
            int lineThickness = 5; // Thickness to "delete" from the split

            // Draw a white line on the mask
            Cv2.Line(mask, (Point)point1, (Point)point2, new Scalar(255), lineThickness);

            // Determine a seed point based on the line's orientation
            Point seedPoint1 = new Point((int)point1.X - lineThickness, (int)point1.Y);
            Point seedPoint2 = new Point((int)point2.X + lineThickness, (int)point2.Y);

            // Clamp the seed points to be within the image bounds
            seedPoint1 = EnsurePointWithinBounds(seedPoint1, image.Size());
            seedPoint2 = EnsurePointWithinBounds(seedPoint2, image.Size());

            // Create two masks for each half by flood-filling on either side of the line
            Mat maskAbove = new Mat();
            Mat maskBelow = new Mat();
            mask.CopyTo(maskAbove);
            mask.CopyTo(maskBelow);

            Cv2.FloodFill(maskAbove, seedPoint1, new Scalar(255));
            Cv2.FloodFill(maskBelow, seedPoint2, new Scalar(255));

            // Invert the masks to get the correct halves
            Cv2.BitwiseNot(maskAbove, maskAbove);
            Cv2.BitwiseNot(maskBelow, maskBelow);

            // Create an erosion kernel
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(2 * lineThickness + 1, 2 * lineThickness + 1), new Point(lineThickness, lineThickness));

            // Erode the masks to "delete" 5 pixels from the split edges
            Cv2.Erode(maskAbove, maskAbove, kernel);
            Cv2.Erode(maskBelow, maskBelow, kernel);

            // Use the eroded masks to create the two halves of the image
            Mat firstHalf = new Mat();
            Mat secondHalf = new Mat();
            image.CopyTo(firstHalf, maskAbove);
            image.CopyTo(secondHalf, maskBelow);

            // Display the two halves
            Cv2.ImShow("First Half - 5px", firstHalf);
            Cv2.ImShow("Second Half - 5px", secondHalf);
        }

        private Point EnsurePointWithinBounds(Point point, OpenCvSharp.Size size)
        {
            int x = Math.Max(point.X, 0);
            x = Math.Min(x, size.Width - 1);
            int y = Math.Max(point.Y, 0);
            y = Math.Min(y, size.Height - 1);
            return new Point(x, y);
        }


    }
}


