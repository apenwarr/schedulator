/*
 * Versaplex:
 *   Copyright (C)2007-2008 Versabanq Innovations Inc. and contributors.
 *       See the included file named LICENSE for license information.
 */
using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Timers;
using Wv;
using Wv.Extensions;


namespace ServTest
{
    public static class SillyHelper
    {
	public static void print(this Stream stream, string s)
	{
	    byte[] utf8 = s.ToUTF8();
	    stream.Write(utf8, 0, utf8.Length);
	    stream.Flush();
	}
    }

    [RunInstaller(true)]
    public class ProjectInstaller: Installer
    {
	ServiceProcessInstaller spi;
	ServiceInstaller si;

	public ProjectInstaller()
	{
	    si = new ServiceInstaller();
	    si.ServiceName = "ServTest";
	    si.Description = "Description++";
	    si.StartType = ServiceStartMode.Automatic;
	    
	    spi = new ServiceProcessInstaller();
	    spi.Account = ServiceAccount.LocalSystem;
	    spi.Password = null;
	    spi.Username = null;
	    
	    Installers.Add(si);
	    Installers.Add(spi);
	}
    }

    public class ServTest: ServiceBase
    {
	FileStream f;
	Timer t;
	int i = 0;
	
	public ServTest()
	{
	    f = File.Open("/tmp/test.txt",
			  FileMode.Append,
			  FileAccess.Write, 
			  FileShare.ReadWrite | FileShare.Delete);
	    t = new Timer();
	    t.Interval = 5000;
	    t.Elapsed += new ElapsedEventHandler(t_Elapsed);

	    ServiceName = "ServTest";
	}
	
	static void Main(string[] args)
	{
	    ServTest o = new ServTest();
	    o.f.print(String.Format("args: ({0}) ({1}) {2}\n",
				    Environment.UserName,
				    Environment.UserInteractive,
				    args.Length));
	    
	    if (Environment.UserInteractive 
		   || (args.Length > 0 && args[0] == "-f"))
	    {
		Console.WriteLine("Running in foreground!");
		o.OnStart(args);
		while (true)
		    wv.sleep(1000);
	    }
	    else
	    {
		Console.WriteLine("Running in background!");
		ServiceBase.Run(o);
	    }
	}

	protected override void OnStart(string[] args)
	{
	    f.print("Service started.\n");
	    t.Enabled = true;
	}

	protected override void OnStop()
	{
	    t.Enabled = false;
	    f.print("Service stopped.\n");
	}
	
	private void t_Elapsed(object sender, ElapsedEventArgs e)
	{
	    f.print("Timer elapsed! " + (++i).ToString() + "\n");
	}
    }
}
