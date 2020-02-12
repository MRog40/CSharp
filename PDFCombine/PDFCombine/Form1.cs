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
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDFCombine
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			this.listBox1.AllowDrop = true;
		}

		// Select Files
		private void button2_Click_1(object sender, EventArgs e)
		{
			textBox1.AppendLine("> Select Files");

			// Select the PDFs to combine
			OpenFileDialog files_selected = new OpenFileDialog
			{
				Filter = "pdf files(*.pdf)| *.pdf",
				FilterIndex = 1,
				Multiselect = true
			};
			files_selected.ShowDialog();

			// Sort the file names
			Array.Sort(files_selected.FileNames);

			// List filenames in listbox to allow user to rearrange them
			foreach (string pdf in files_selected.FileNames)
			{
				listBox1.Items.Add(Path.GetFileName(pdf)
					+ new String('\t', 20) + Path.GetDirectoryName(pdf));
			}

			textBox1.AppendLine("  > Arrange the files");
		}

		private void listBox1_MouseDown(object sender, MouseEventArgs e)
		{
			if (this.listBox1.SelectedItem == null) return;
			this.listBox1.DoDragDrop(this.listBox1.SelectedItem, DragDropEffects.Move);
		}

		private void listBox1_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void listBox1_DragDrop(object sender, DragEventArgs e)
		{
			Point point = listBox1.PointToClient(new Point(e.X, e.Y));
			int index = this.listBox1.IndexFromPoint(point);
			if (index < 0) index = this.listBox1.Items.Count - 1;
			try
			{
				object data = listBox1.SelectedItem;
				this.listBox1.Items.Remove(data);
				this.listBox1.Items.Insert(index, data);
			}
			catch (Exception er)
			{
				textBox1.AppendLine(" !> DragDrop Error");
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			// Combine PDFs
			List<string> ordered_files = new List<string>(listBox1.Items.Count);

			for (int k = 0; k < listBox1.Items.Count; k++)
			{
				ordered_files.Add(listBox1.GetItemValue(listBox1.Items[k]).ToString());
				ordered_files[k] = WinFormsExtensions.back2dir(ordered_files[k]);
			}

			WinFormsExtensions.CombinePDFs(ordered_files.ToArray(), textBox1, listBox1.Items.Count);
			listBox1.Items.Clear();
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
		public static void CombinePDFs(string[] files_selected, TextBox textBox1, int length)
		{
			using (PdfDocument targetDoc = new PdfDocument())
			{
				foreach (string pdf in files_selected)
				{
					try
					{
						using (PdfDocument pdfDoc = PdfReader.Open(pdf, PdfDocumentOpenMode.Import))
						{
							for (int i = 0; i < pdfDoc.PageCount; i++)
							{
								targetDoc.AddPage(pdfDoc.Pages[i]);
							}
						}
						textBox1.AppendLine("    > Combined: " + Path.GetFileName(pdf));
					}
					catch (Exception e)
					{
						textBox1.AppendLine(" !> Error: At least one file is encrypted");
					}
				}
				// Prompt user to save combined PDF
				SaveFileDialog SaveFileDialog1 = new SaveFileDialog
				{
					Filter = "pdf files(*.pdf)| *.pdf",
					FilterIndex = 1,
					FileName = "CombinedPDF",
					DefaultExt = "pdf",
					InitialDirectory = @"C:\",
					Title = "Save Combined PDF"
				};
				SaveFileDialog1.ShowDialog();
				try
				{
					targetDoc.Save(SaveFileDialog1.FileName);
					textBox1.AppendLine("  > Files Combined");
				}
				catch (Exception e)
				{
					textBox1.AppendLine(" !> Save Error");
				}
			}
		}
		// Remove the 20 spaces and arrange the dir to a usable form
		public static string back2dir(string file_bad)
		{
			string limiter = new String('\t', 20);
			// listBox1.Items.Add(Path.GetFileNameWithoutExtension(pdf)
			// 		+ new String('\t', 20) + Path.GetDirectoryName(pdf));
			string file_good = file_bad.Split(new string[] { limiter }, StringSplitOptions.None)[1] +
				"\\" + file_bad.Split(new string[] { limiter }, StringSplitOptions.None)[0];
			return file_good;
		}
	}

	public static class ListControlExtensions
	{
		public static object GetItemValue(this ListControl list, object item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (string.IsNullOrEmpty(list.ValueMember))
				return item;

			var property = TypeDescriptor.GetProperties(item)[list.ValueMember];
			if (property == null)
				throw new ArgumentException(
					string.Format("item doesn't contain '{0}' property or column.",
					list.ValueMember));
			return property.GetValue(item);
		}
	}
}
