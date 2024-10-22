/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class TuioDemo : Form, TuioListener
{
	private TuioClient client;
	private Dictionary<long, TuioObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;
	private TcpListener listener;
	private Thread listenerThread;


	public static int width, height;
	private int window_width = 640;
	private int window_height = 480;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;

	private bool fullscreen;
	private bool verbose;

	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

	List<string> Tshirts = new List<string>() { "blackTshirt.PNG", "whiteTshirt.PNG", "blueTshirt.PNG", "greenTshirt.PNG", "redTshirt.PNG" }; // ID = 0
	List<string> LongSleeveShirts = new List<string>() { "blackLongSleeve.PNG", "whiteLongSleeve.PNG" }; // ID = 1
	List<string> Hoodies = new List<string>() { "blueHoodie.PNG", "yellowHoodie.PNG", "greenHoodie.PNG" }; // ID = 2
	List<string> Pants = new List<string>() { "biegePants.PNG", "brownPants.PNG" }; // ID = 3
	private int currentTshirtIndex = 0; // For T-shirts

	private int currentLongSleeveIndex = 0; // For Long Sleeves

	private int currentHoodieIndex = 0; // For Hoodies

	private int currentPantsIndex = 0; // For Pants
	public TuioDemo(int port)
	{

		verbose = false;
		fullscreen = false;
		width = window_width;
		height = window_height;
		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";

		this.Closing += new CancelEventHandler(Form_Closing);
		this.KeyDown += new KeyEventHandler(Form_KeyDown);

		this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long, TuioObject>(128);
		cursorList = new Dictionary<long, TuioCursor>(128);
		blobList = new Dictionary<long, TuioBlob>(128);

		client = new TuioClient(port);
		client.addTuioListener(this);
		StartListeningForCommands(12347);
		client.connect();
	}
	private void StartListeningForCommands(int port)

	{

		listener = new TcpListener(IPAddress.Any, port);

		listener.Start();

		listenerThread = new Thread(HandleIncomingCommands);

		listenerThread.Start();

	}
	private void HandleIncomingCommands()

	{

		while (true)

		{

			using (TcpClient client = listener.AcceptTcpClient())

			{

				using (NetworkStream stream = client.GetStream())

				{

					byte[] buffer = new byte[1024];

					int bytesRead = stream.Read(buffer, 0, buffer.Length);

					string command = Encoding.ASCII.GetString(buffer, 0, bytesRead);

					ProcessCommand(command);

				}

			}

		}

	}
	private void ProcessCommand(string command)

	{

		if (command.Trim() == "NEXT")

		{

			ChangeToNextImage();
		}

	}
	private void ChangeToNextImage()

	{

		Console.WriteLine("Changing to next image in TUIO demo...");


		

		foreach (TuioObject tobj in objectList.Values)

		{

			switch (tobj.SymbolID)

			{

				case 0: // T-shirts

					currentTshirtIndex = (currentTshirtIndex + 1) % Tshirts.Count;

					UpdateDisplayedImage(Tshirts[currentTshirtIndex]);

					break;

				case 1: // Long Sleeve

					currentLongSleeveIndex = (currentLongSleeveIndex + 1) % LongSleeveShirts.Count;

					UpdateDisplayedImage(LongSleeveShirts[currentLongSleeveIndex]);

					break;

				case 2: // Hoodies

					currentHoodieIndex = (currentHoodieIndex + 1) % Hoodies.Count;

					UpdateDisplayedImage(Hoodies[currentHoodieIndex]);

					break;

				case 3: // Pants

					currentPantsIndex = (currentPantsIndex + 1) % Pants.Count;

					UpdateDisplayedImage(Pants[currentPantsIndex]);

					break;

				default:

					Console.WriteLine("Unknown SymbolID: " + tobj.SymbolID);

					break;

			}

		}


		

		Invalidate(); 

	}

	private string currentImagePath;

	private void UpdateDisplayedImage(string img_path)

	{

		try

		{

			

			if (!File.Exists(img_path))

			{

				Console.WriteLine("File not found: " + img_path);

				return; 

			}


			

			using (Bitmap img = new Bitmap(img_path))

			{

				

				Console.WriteLine("Image loaded successfully: " + img_path);

				

			}

		}

		catch (OutOfMemoryException ex)

		{

			Console.WriteLine("Error loading image: Out of memory. " + ex.Message);

			

		}

		catch (Exception ex)

		{

			Console.WriteLine("Error loading image: " + ex.Message);


		}
		Invalidate(); 

	}



	private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{

		if (e.KeyData == Keys.F1)
		{
			if (fullscreen == false)
			{

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
				this.Left = 0;
				this.Top = 0;
				this.Width = screen_width;
				this.Height = screen_height;

				fullscreen = true;
			}
			else
			{

				width = window_width;
				height = window_height;

				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.Left = window_left;
				this.Top = window_top;
				this.Width = window_width;
				this.Height = window_height;

				fullscreen = false;
			}
		}
		else if (e.KeyData == Keys.Escape)
		{
			this.Close();

		}
		else if (e.KeyData == Keys.V)
		{
			verbose = !verbose;
		}

	}

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Add(o.SessionID, o);
		}
		if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
	}

	public void updateTuioObject(TuioObject o)
	{

		if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
	}

	public void removeTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
	}

	public void addTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Add(c.SessionID, c);
		}
		if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
	}

	public void updateTuioCursor(TuioCursor c)
	{
		if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
	}

	public void addTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Add(b.SessionID, b);
		}
		if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
	}

	public void updateTuioBlob(TuioBlob b)
	{

		if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
	}

	public void refresh(TuioTime frameTime)
	{
		Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
		try
		{
			Bitmap backgroundImage = new Bitmap("abdul.jpg"); 
			g.DrawImage(backgroundImage, new Rectangle(0, 0, width, height)); 
		}
		catch (Exception ex)
		{
			// In case the image can't be loaded, fallback to solid color
			Console.WriteLine("Error loading background image: " + ex.Message);
			g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));  // Default background color
		}

		// draw the cursor path
		if (cursorList.Count > 0)
		{
			lock (cursorList)
			{
				foreach (TuioCursor tcur in cursorList.Values)
				{
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++)
					{
						TuioPoint next_point = path[i];
						g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
						current_point = next_point;
					}
					g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
					g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
				}
			}
		}


		// draw the objects
		if (objectList.Count > 0)

		{

			lock (objectList)

			{

				foreach (TuioObject tobj in objectList.Values)

				{

					int ox = tobj.getScreenX(width);

					int oy = tobj.getScreenY(height);

					int size = height / 8;


					string img_path = GetImagePath(tobj.SymbolID); 


					try

					{

						using (Bitmap img = new Bitmap(img_path)) 

						{

							int newWidth = img.Width / 6;

							int newHeight = img.Height / 10;
							if (newWidth > 0 && newHeight > 0)

							{

								g.TranslateTransform(ox, oy);

								g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));

								g.TranslateTransform(-ox, -oy);


								g.DrawImage(img, ox, oy, newWidth, newHeight);


								g.TranslateTransform(ox, oy);

								g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));

								g.TranslateTransform(-ox, -oy);

							}

							else

							{

								Console.WriteLine("Calculated dimensions are invalid: Width = " + newWidth + ", Height = " + newHeight);

							}

						}

					}

					catch (OutOfMemoryException ex)

					{

						Console.WriteLine("Error loading image: Out of memory. " + ex.Message);

					}

					catch (Exception ex)

					{

						Console.WriteLine("Error loading image: " + ex.Message);

					}

				}

			}

		}

		// draw the blobs
		if (blobList.Count > 0)
		{
			lock (blobList)
			{
				foreach (TuioBlob tblb in blobList.Values)
				{
					int bx = tblb.getScreenX(width);
					int by = tblb.getScreenY(height);
					float bw = tblb.Width * width;
					float bh = tblb.Height * height;

					g.TranslateTransform(bx, by);
					g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-bx, -by);

					g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

					g.TranslateTransform(bx, by);
					g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
					g.TranslateTransform(-bx, -by);

					g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
				}
			}
		}
	}
	private string GetImagePath(int symbolID)

	{

		switch (symbolID)

		{

			case 0: return Tshirts[currentTshirtIndex];

			case 1: return LongSleeveShirts[currentLongSleeveIndex];

			case 2: return Hoodies[currentHoodieIndex];

			case 3: return Pants[currentPantsIndex];

			default: return Tshirts[currentTshirtIndex];

		}
	}
		public static void Main(String[] argv)
	{
		int port = 0;
		switch (argv.Length)
		{
			case 1:
				port = int.Parse(argv[0], null);
				if (port == 0) goto default;
				break;
			case 0:
				port = 3333;
				break;
			default:
				Console.WriteLine("usage: mono TuioDemo [port]");
				System.Environment.Exit(0);
				break;
		}

		TuioDemo app = new TuioDemo(port);
		Application.Run(app);
	}

}

