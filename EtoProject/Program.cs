using System;

namespace EtoProject
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			new Eto.Forms.Application ().Run (new MyForm ());
		}
	}
}
