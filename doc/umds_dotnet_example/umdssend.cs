using System;
using com.latencybusters.umds;
using UMDSException = com.latencybusters.umds.UMDS.UMDSException;
using UMDSDisconnectException = com.latencybusters.umds.UMDS.UMDSDisconnectException;
using UMDSBadStateException = com.latencybusters.umds.UMDS.UMDSBadStateException;
using UMDSAuthenticationException = com.latencybusters.umds.UMDS.UMDSAuthenticationException;
using LOG_LEVEL = com.latencybusters.umds.UMDS.LOG_LEVEL;

/*
Copyright (c) 2005-2019 Informatica Corporation  Permission is granted to licensees to use
or alter this software for any purpose, including commercial applications,
according to the terms laid out in the Software License Agreement.

This source code example is provided by Informatica for educational
and evaluation purposes only.

THE SOFTWARE IS PROVIDED "AS IS" AND INFORMATICA DISCLAIMS ALL WARRANTIES
EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION, ANY IMPLIED WARRANTIES OF
NON-INFRINGEMENT, MERCHANTABILITY OR FITNESS FOR A PARTICULAR
PURPOSE.  INFORMATICA DOES NOT WARRANT THAT USE OF THE SOFTWARE WILL BE
UNINTERRUPTED OR ERROR-FREE.  INFORMATICA SHALL NOT, UNDER ANY CIRCUMSTANCES, BE
LIABLE TO LICENSEE FOR LOST PROFITS, CONSEQUENTIAL, INCIDENTAL, SPECIAL OR
INDIRECT DAMAGES ARISING OUT OF OR RELATED TO THIS AGREEMENT OR THE
TRANSACTIONS CONTEMPLATED HEREUNDER, EVEN IF INFORMATICA HAS BEEN APPRISED OF
THE LIKELIHOOD OF SUCH DAMAGES.
*/

class umdssend:UMDSServerConnection
{
	private const System.String appl_name = "umdssend";
	private const System.String purpose = "Purpose: Send messages on a single topic.";
	private const System.String usage = "Usage: umdssend [options] -S address[:port] topic\n" + 
	"  -S address[:port] = Server address/name and optionally port\n" + 
	"                    A comma separated list of multiple servers may be provided\n" + 
	"Available options:\n" + 
	"  -A Suppress sending the application name to the server on login\n " +
	"  -c filename = read config parameters from filename\n" + 
	"  -I Immediate Mode\n" + 
	"  -h = help\n" + 
	"  -l len = send messages of len bytes\n" + 
	"  -L linger = Allow traffic to drain for up to linger seconds\n" + 
	"              before closing the connection\n" + 
	"  -M msgs = send msgs number of messages\n" + 
	"  -N num_topics = Number of topics to send on\n" + 
	"  -P msec = pause after each send msec milliseconds\n" + 
	"  -s num_secs = Print statistics every num_secs\n" + 
	"  -U username = set the user name and prompt for password\n" +
	"  -v = be verbose in reporting to the console\n" ;
	
	internal int num_topics = 1;
	internal int msgs = 1000000;
	internal bool verbose = false;
	internal System.String conffname = null;
	internal int msglen = 25;
	internal int stats_ms = 1000;
	internal int pause = 0;
	internal System.String server_list = null;
	internal System.String serverip = "localhost";
	internal System.String serverport = "1234";
	internal int linger = 0;
	internal System.String topic = null;
	internal System.String username = null;
	internal System.String password = null;
	internal bool auth_failed = false;
	internal bool help = false;
	internal bool connsend = false;
	internal int createCount = 0;
	internal int closeCount = 0;
	internal bool loggedOut = false;
	internal bool sendAppName = true;
	
	private void  process_cmdline(System.String[] args)
	{
		int argnum = 0;
		
		if(args.Length == 0)
			throw new System.Exception("Need a server list and a topic");

		while (argnum <= args.Length - 1)
		{
			Char [] ca = new Char[1];
			args[argnum].CopyTo(0, ca, 0, 1);
			if(ca[0] != '-') break;
			args[argnum++].CopyTo(1, ca, 0, 1);
			switch (ca[0])
			{
				case 'A':
				case 'h':
				case 'I':
				case 'v':
					/* These cases don't have args to validate */
					System.Console.Out.WriteLine( "Parameter: " + ca[0] );
		 			argnum--;
					break;
				default:
					System.Console.Out.WriteLine( "Parameter: " + ca[0] + "  Arg: " + ((argnum < args.Length) ? args[argnum] : null ) );

					if(argnum + 1 >= args.Length)
						throw new System.Exception("Missing option value or topic");
					break;
			}
			switch (ca[0])
			{
				
				case 'A': 
					sendAppName = false;
					break;
			
				
				case 'c': 
					conffname = args[argnum];
					break;
				
				
				case 'I': 
					connsend = true;
					break;;
				
				
				case 'h': 
					help = true;
					throw new System.Exception("Help:");
				
				
				case 'l': 
					try
					{
						msglen = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid message length:" + args[argnum]);
					}
					break;
				
				
				case 'L': 
					try
					{
						linger = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid Linger time:" + args[argnum]);
					}
					break;
				
				
				case 'M': 
					try
					{
						msgs = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid number of messages:" + args[argnum]);
					}
					break;
				
				
				case 'N': 
					try
					{
						num_topics = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid number of topics:" + args[argnum]);
					}
					break;
				
				case 'P': 
					try
					{
						pause = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid pause time:" + args[argnum]);
					}
					break;
				
				
				case 's': 
					try
					{
						stats_ms = Convert.ToInt32(args[argnum]) * 1000;
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid number of seconds for statistics:" + args[argnum]);
					}
					break;
				
				
				case 'S': 
					server_list = args[argnum];
					break;

				case 'U': 
					username = args[argnum];
					break;

				case 'v': 
					verbose = true;
					break;
			}
 			argnum++;
		}
		if (argnum >= args.Length)
			throw new System.Exception("Missing topic");
		
		topic = args[argnum];
		// Validate Mandatory options
		if (server_list == null)
			throw new System.Exception("Must supply server list (-S)");
	}
	
	/*
	* .NET doesn't support turning off echo so the password will be displayed.
	*/
	private void  get_password()
	{
		System.Console.Out.WriteLine("Enter password");
		System.IO.StreamReader linein = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).CurrentEncoding);
		try
		{
			password = linein.ReadLine();
		}
		catch (System.IO.IOException e)
		{
			System.Console.Error.WriteLine("Error reading password:" + e);
		}
	}
	
	/*
	* Method to read a file of lines in "x = y" format containing configuration
	* data, and setting the connections configuration
	*/
	internal virtual bool read_config(UMDSServerConnection sconn, System.String filename)
	{
		bool ret = true;
		
		System.IO.StreamReader linein;
		try
		{
			linein = new System.IO.StreamReader(new System.IO.StreamReader(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read), System.Text.Encoding.Default).CurrentEncoding);
		}
		catch (System.IO.FileNotFoundException)
		{
			System.Console.Error.WriteLine("File " + filename + " not found.");
			return false;
		}
		do 
		{
			System.String line;
			
			try
			{
				line = linein.ReadLine();
			}
			catch (System.IO.IOException)
			{
				System.Console.Error.WriteLine("Error reading config file " + filename);
				return false;
			}
			if (line == null)
				break;
			if (line.StartsWith("#"))
				continue;
			
			int eq = line.IndexOf('=');
			if(eq == -1)
				return ret;
			
			System.String[] newprop = new System.String[2];
			
			newprop[0] = line.Substring(0,eq).Trim();
			newprop[1] = line.Substring(eq + 1, line.Length - eq - 1).Trim();

			try
			{
				sconn.setProperty(newprop);
			}
			catch (UMDSException)
			{
				System.Console.Error.WriteLine("Failed to set server connection property " + newprop[0] + " to " + newprop[1]);
				ret = false;
			}
		}
		while (true);
		return ret;
	}
	
	/// <summary> Event Notification function. This function is called when non-data events
	/// occur, E.G. Disconnection reported by the server.
	/// 
	/// </summary>
	/// <param name="msg">The message containing the event
	/// </param>
	public override void  onEvent(UMDSMessage msg)
	{
		switch (msg.type)
		{
			case UMDSMessage.MSG_TYPE.LOSS: 
				/*
				* Received when the server detects general MIM loss.
				*/
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: General loss occurred prior to sequence number " + msg.seqnum);
				break;

			case UMDSMessage.MSG_TYPE.DISCONNECT: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event:" + msg);
				break;
			
			case UMDSMessage.MSG_TYPE.LOGIN_DENIED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event:" + msg);
				auth_failed = true;
				break;

			case UMDSMessage.MSG_TYPE.SERVER_STOPPED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event:" + msg);
				auth_failed = true;
				break;
			
			case UMDSMessage.MSG_TYPE.SOURCE_CREATE :
				createCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Source Create completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_CREATE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Receiver create completed ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.SOURCE_DELETE :
				closeCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Source delete Completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_DELETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Receiver delete Completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.CONNECT :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Client is (re)connected to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGIN_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Client is Logged in to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGOUT_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Client has Logged out of server. Status " + msg.status + " " + msg.status_str );
				break;
			
			
			default: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: Unknown server connection event received " + msg);
				break;
			
		}
	}
	
	private umdssend(System.String[] args):base("")
	{
		int Creates = 0;
		int	Closes = 0;

		/* Process the command line arguments */
		try
		{
			process_cmdline(args);
			if ( sendAppName ) {
				setProperty("appl-name", appl_name );
			}
		}
		catch (System.Exception e)
		{
			if (!help)
			{
				System.Console.Error.WriteLine("Error:" + e.Message);
			}
			System.Console.Error.WriteLine(UMDS.version());
			System.Console.Error.WriteLine(purpose);
			System.Console.Error.WriteLine(usage);
			System.Environment.Exit(1);
		}
		
		/* If a user name was provided, get the password */
		if (username != null)
			get_password();
		
		UMDSServerConnection svrconn = this;
		
		/*
		* If an application config file is provided, read it and set the server
		* connection properties.
		*/
		if (conffname != null)
		{
			if (read_config(svrconn, conffname) == false)
				System.Environment.Exit(1);
		}
		
		/* Set the list of servers, user name and password in the connection */
		try
		{
			svrconn.setProperty("server-list", server_list);
			if (username != null)
			{
				svrconn.setProperty("user", username);
				svrconn.setProperty("password", password);
			}
			
			// Set the linger value to set a timeout on draining data when
			// closing
			if (linger > 0)
				svrconn.setProperty("client-linger", "" + (linger * 1000));
		}
		catch (UMDSException ex)
		{
			System.Console.Error.WriteLine("Error setting UMDS configuration: " + ex.ToString());
			System.Environment.Exit(1);
		}
		
		/*
		* Now start the connection to a server. If server-reconnect is enabled
		* it will return asynchronously. If server-reconnect is disabled,
		* start() will return when the connection is authenticated
		*/
		bool ServerReconnect;
		try {
			ServerReconnect = (svrconn.getProperty("server-reconnect" ).CompareTo("1") == 0) ? true : false;
		} catch( UMDSException e ) {
			ServerReconnect = true;
		}
		try
		{
			svrconn.start();
		}
		catch (UMDSAuthenticationException)
		{
			System.Console.Error.WriteLine("Authentication failed - check user name/password requirements of the server");
			System.Environment.Exit(1);
		}
		catch (UMDSException e)
		{
			System.Console.Error.WriteLine("Failed to create the server connection:" + e);
			System.Environment.Exit(1);
		}
		
		while (!svrconn.Authenticated && auth_failed == false)
		{
			System.Threading.Thread.Sleep( 100 );
		}
		
		if (auth_failed)
		{
			System.Environment.Exit(1);
		}
		
		/* Now the process of sending data begins... */
		System.Console.Out.Write("Sending " + msgs + " messages of size " + msglen + " bytes to topic [" + topic + "]");
		
		if (pause == 0)
		{
			System.Console.Out.WriteLine(" as fast as the system can");
		}
		else
		{
			System.Console.Out.WriteLine(" pausing " + pause + "ms");
		}
		
		/*
		* Create a payload with the string "UMDS" and set every K to contain
		* and offset for payload verification
		*/
		byte[] message = new byte[msglen];
		byte[] dummymsg = System.Text.UTF8Encoding.UTF8.GetBytes("UMDS");
		Array.Copy(dummymsg, 0, message, 0, dummymsg.Length < message.Length?dummymsg.Length:message.Length);
		
		for (int k = 1; (k * 1024) < message.Length && k < 256; k++)
			message[k * 1024] = (byte) k;
		
		/*
		* A simple loop to send messages is used. This warning indicates a long
		* pause interval was provided relative to the stats interval.
		*/
		if ((pause > stats_ms) && (stats_ms > 0))
		{
			System.Console.Out.WriteLine("Warning - Pause rate " + pause + " exceeds stats interval : " + stats_ms + " (will see intervals of 0 sends)");
		}
		
		/*
		* Create an array of topic names (and sources) to send on. If -N was
		* provided, each topic will be used in round robin fashion.
		*/
		System.String[] topicnames = new System.String[num_topics];
		UMDSSource[] srcs = new UMDSSource[num_topics];
		Creates = num_topics;
		createCount = 0;
		try
		{
			if (num_topics > 1)
			{
				for (int n = 0; n < num_topics; n++)
				{
					topicnames[n] = topic + "." + n;
					if (connsend == false)
						srcs[n] = new UMDSSource(svrconn, topicnames[n], null);
					else
						srcs[n] = null;
				}
			}
			else
			{
				topicnames[0] = topic;
				if (connsend == false)
					srcs[0] = new UMDSSource(svrconn, topicnames[0], null);
				else
					srcs[0] = null;
			}
		}
		catch (UMDSException e)
		{
			System.Console.Error.WriteLine("Failed to create the source:" + e);
			System.Environment.Exit(1);
		}
		
		if ( false ) {
			while( createCount < Creates ) {
				System.Threading.Thread.Sleep( 10 );
				System.Console.Error.WriteLine("Sources Created: " + createCount );
			}
			System.Console.Error.WriteLine("Sources Created: " + createCount );
		}

		
		/* Data used in sending */
		long seq_counter = 0;
		long seq_end = seq_counter + msgs;
		long last_msgs_sent = 0;
		long last_bytes_sent = 0;
		long bytes_sent = 0;
		long start_time = UMDS.TimeAsMillis();
		long end_time = 0, last_time = start_time;
		long elapsed_time = 0;
		
		/*
		* A simple loop is run to send messages and dump stats. Calculate the
		* time to wait for each iteration and use ms times decrementing each
		* iteration to trigger sending of messages and dumping of stats.
		*/
		long pause_togo = pause;
		long stats_togo = stats_ms;
		int iter_time;
		
		if (pause > 0)
		{
			if (pause == stats_ms)
				iter_time = pause;
			else
			{
				if (pause < stats_ms)
					iter_time = pause % (stats_ms - pause);
				else
				{
					if (pause > stats_ms)
						iter_time = stats_ms;
					else
						iter_time = stats_ms / (stats_ms / (pause - stats_ms));
				}
			}
		}
		else
			iter_time = 0;
		
		System.Console.Out.WriteLine("stats_ms " + stats_ms + " pause " + pause + " iteration " + iter_time);
		
		/* Start sending messages! */
		int topic_idx = 0;
		long procd = UMDS.TimeAsMillis();
		while (seq_counter < seq_end && auth_failed == false)
		{
			if (stats_togo <= 0 || iter_time == 0)
			{
				end_time = UMDS.TimeAsMillis();

				elapsed_time = end_time - last_time;
				if (elapsed_time >= stats_ms)
				{
					/* New stats interval */
					double secs = (elapsed_time) / 1000.0;
					print_bw(secs, (int) last_msgs_sent, last_bytes_sent, seq_counter);
					last_time = end_time;
					last_msgs_sent = 0;
					last_bytes_sent = 0;
					elapsed_time = 0;
				}
				stats_togo = stats_ms;
			}

			bool sent = false;
			if ((pause_togo <= 0 || iter_time == 0) && svrconn.Authenticated)
			{
				pause_togo = pause;
				/* Send a message! */
				try
				{
					sent = false;
					if (connsend) {
						svrconn.send(topicnames[topic_idx], message);
						sent = true;
					} else {
						if ( srcs[topic_idx].canSend() ) { 
							srcs[topic_idx].send(message);
							sent = true;
						}
					}
					if ( sent ) {
						seq_counter++;
						last_msgs_sent++;
						bytes_sent += msglen;
						last_bytes_sent += msglen;
						if (num_topics > 1)
						{
							topic_idx++;
							if (topic_idx == topicnames.Length)
								topic_idx = 0;
						}
					}
				}
				catch (UMDSAuthenticationException)
				{
					/* This can occur while auto-reconnecting */
					// Give the connection time to reestablish.
					System.Threading.Thread.Sleep( 10 );
					}
				catch (UMDSBadStateException ex)
					{
					System.Console.Error.WriteLine("Error sending message: Bad State:" + ex.ToString() + " seq " + seq_counter);
					// Give the source time to establish.
					System.Threading.Thread.Sleep( 10 );
					}
				catch (UMDSDisconnectException)
					{
					// Give the connection time to reestablish.
					System.Threading.Thread.Sleep( 10 );
				}
				catch (UMDSException ex)
				{
					System.Console.Error.WriteLine("Error sending message: " + ex.ToString() + " seq " + seq_counter);
					break;
				}
			}
			if (!svrconn.Authenticated)
			{
				// Defensive sleep while waiting for reconnection
				if ( ServerReconnect ) {
					System.Threading.Thread.Sleep( 500 );
				} else {
					auth_failed = true;
				}
			}
			/* If the user is trying to send slowly, take a break */
			if (iter_time > 0)
			{
					System.Threading.Thread.Sleep( iter_time );
			}
			
			long newprocd = UMDS.TimeAsMillis();
			long procdiff = (newprocd - procd);
			if ( procdiff < 0 ) procdiff = -procdiff;			// When this is negative (we crossed a second boundary); make it positive.
			if (pause > 0)
				pause_togo -= procdiff;
			if (stats_ms > 0)
				stats_togo -= procdiff;
			procd = newprocd;
		}
		
		if ( true != auth_failed ) {
			Closes = 0;
			for (int n = 0; n < srcs.Length; n++)
			{
				if (null != srcs[n])
				{
					try
					{
						srcs[n].close();
						srcs[n] = null;
						Closes++;
					}
					catch (UMDSException ex)
					{
						System.Console.Error.WriteLine("Error closing source[" + n + "]: " + ex.ToString());
					}
				}
			}
			
			while( closeCount < Closes ) {
				if ( verbose ) {
					System.Console.Error.WriteLine("Sources: " + Closes + "  Closed: " + closeCount );
				}
				System.Threading.Thread.Sleep( 100 );
			}
			if ( verbose ) {
				System.Console.Error.WriteLine("All sources closed: Sources: " + Closes + "  Closed: " + closeCount );
			}
	
			/* Close the connection, we are done */
			try
			{
				System.Console.Error.WriteLine("Closing Connection " );
				end_time = UMDS.TimeAsMillis();	// Have to get end_time here; close invalidates UMDS
				svrconn.close();
			}
			catch (UMDSException ex)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.Console.Error.WriteLine("Error closing server connection: " + ex.ToString());
			}
			System.Console.Error.WriteLine( "Connection Closed" );
		}
		
		/* Finished sending, Dump the summary statistics */
		double secs2 = (end_time - start_time) / 1000.0;
		System.Console.Out.WriteLine("Sent " + seq_counter + " messages of size " + msglen + " bytes in " + secs2 + " seconds.");
		print_bw(secs2, (int) seq_counter, bytes_sent, seq_counter);

		System.Console.Out.WriteLine( "Done" );
	}
	
	private static void  print_bw(double sec, int msgs, long bytes, long total_msgs)
	{
		double mps = 0;
		double bps = 0;
		double kscale = 1000;
		double mscale = 1000000;
		char mgscale = 'K';
		char bscale = 'K';
		
		if (sec == 0.0)
			return ; // Avoid divide by 0
		
		mps = msgs / sec;
		bps = bytes * 8 / sec;
		if (mps <= mscale)
		{
			mgscale = 'K';
			mps /= kscale;
		}
		else
		{
			mgscale = 'M';
			mps /= mscale;
		}
		if (bps <= mscale)
		{
			bscale = 'K';
			bps /= kscale;
		}
		else
		{
			bscale = 'M';
			bps /= mscale;
		}
		TextNumberFormat nf = new TextNumberFormat(3,-1,-1,-1,3);
		System.Console.Out.WriteLine(sec + " secs. " + nf.FormatDouble(mps) + " " + mgscale + "msgs/sec. " + nf.FormatDouble(bps) + " " + bscale + "bps " + total_msgs + " msgs");
	}
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		new umdssend(args);
	}

	public class TextNumberFormat
	{
		private System.Globalization.NumberFormatInfo numberFormat;
		private int maxIntDigits;
		private int minIntDigits;
		private int maxFractionDigits;
		private int minFractionDigits;

		public TextNumberFormat(int digits,int minint,int maxint,int minfrac,int maxfrac)
		{
			numberFormat = System.Globalization.NumberFormatInfo.CurrentInfo;
			minIntDigits = minint < 0 ? 1 : minint;
			maxIntDigits = maxint < 0 ? 127 : maxint;
			minFractionDigits = minfrac < 0 ? 0 : minfrac;
			maxFractionDigits = maxfrac < 0 ? 3 : maxfrac;
		}

		public System.String FormatDouble(double number)
		{
			int counter = 0;
			double temp = System.Math.Abs(number);
			while ( (temp % 1) > 0 )
			{
				temp *= 10;
				counter++;
			}
			int numdigs = (counter < minFractionDigits) ? minFractionDigits : (( counter < maxFractionDigits ) ? counter : maxFractionDigits); 

			String fmt = number.ToString("n" + numdigs, numberFormat);
			String decimals = "";
			String fraction = "";

			int i = fmt.IndexOf(numberFormat.NumberDecimalSeparator);
			if (i > 0)
			{
				fraction = fmt.Substring(i);
				decimals = fmt.Substring(0,i).Replace(numberFormat.NumberGroupSeparator,"");
			}
			else decimals = fmt.Replace(numberFormat.NumberGroupSeparator,"");
			decimals = decimals.PadLeft(minIntDigits,'0');

			if ((i = decimals.Length - maxIntDigits) > 0) decimals = decimals.Remove(0,i);

			for (i = decimals.Length;i > 3;i -= 3)
			{
				decimals = decimals.Insert(i - 3,numberFormat.NumberGroupSeparator);
			}

			decimals = decimals + fraction;
			if (decimals.Length == 0) return "0";
			else return decimals;
		}
	}
}