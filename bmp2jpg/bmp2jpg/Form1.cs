using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace bmp2jpg
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		// Select Files
		private void button2_Click(object sender, EventArgs e)
		{
			textBox1.AppendLine("> File Conversion");

			// Select the files to convert
			OpenFileDialog files_selected = new OpenFileDialog
			{
				Filter = "bmp files(*.bmp)| *.bmp",
				FilterIndex = 1,
				Multiselect = true
			};
			files_selected.ShowDialog();

			foreach (string file_name in files_selected.FileNames)
			{
				textBox1.AppendLine(" > Converted " + file_name.Split('\\')[file_name.Split('\\').Length - 1]);

				// Load BMP and save as JPEG
				Bitmap img = new Bitmap(file_name);

				string new_file_name = file_name.Replace(Path.GetExtension(file_name), ".jpg");

				// Get an ImageCodecInfo object that represents the JPEG codec.
				ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
				System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
				EncoderParameters myEncoderParameters = new EncoderParameters(1);

				// Save the bitmap as a JPEG file with quality level 95.
				EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 95L);
				myEncoderParameters.Param[0] = myEncoderParameter;

				try
				{
					//Save the image
					img.Save(new_file_name, myImageCodecInfo, myEncoderParameters);
					img.Dispose();
					File.Delete(file_name);
				}
				catch (Exception err) { textBox1.AppendLine("  > Image Save Error: " + err.Message); }
			}
		}

		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			int j;
			ImageCodecInfo[] encoders;
			encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

	}

	public static class WinFormsExtensions
	{
		// Add line to multiline textbox
		public static void AppendLine(this TextBox source, string value)
		{
			if (source.Text.Length == 0)
				source.Text = value;
			else
				source.AppendText("\r\n" + value);
		}

		// Limit the number of lines of a multiline textbox
		public static void LimitLines(this TextBox source, int value)
		{
			while (source.Lines.Count() > value)
			{
				var lines = source.Lines;
				var newLines = lines.Skip(1);
				source.Lines = newLines.ToArray();
			}
		}
	}
}
