using System;
using System.Text;
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

class appRequest:UMDSSource
{
	
	internal long Requests;
	internal long RequestsCanceled;
	internal long Responses;
	internal bool verbose = false;
	
	public appRequest(UMDSServerConnection sconn, System.String topic, System.String[] options):base(sconn, topic, options)
	{
		
		Requests = 0;
		RequestsCanceled = 0;
		Responses = 0;
	}
	
	public override void  request( int request_id, byte[] request_data)
	{
		Requests++;
		base.request( request_id, request_data);
	}
	
	/// <summary> The onEvent method is called by the UMDS client code. Application
	/// code should be added to handle various events. These events are
	/// typically errors.
	/// </summary>
	public override void  onEvent( UMDSMessage msg )
	{
		if ( UMDSMessage.MSG_TYPE.REQUEST_CANCELED == msg.type ) {
			if ( verbose ) {
				log(LOG_LEVEL.INFO, "Request <" + msg.requestID + "> Timedout.");
			}
			RequestsCanceled++;
		}
	}
	
	/// <summary> The onResponse method is called by the UMDS client code. Application
	/// code should be added to handle various events.
	/// </summary>
	public override void  onResponse(UMDSMessage msg)
	{
		if ( verbose ) {
			System.Console.Out.WriteLine( "onResponse called: type < " + msg.type + " >" );
		}
		switch ( msg.type )
		{
			case UMDSMessage.MSG_TYPE.RESPONSE :
				if ( verbose ) {
					try {
/*
						// This code block will display the verifiable content in the request from the UMDS response app. 
						// The UM lbmres app does not provide this text.
						//

						System.String res = Encoding.UTF8.GetString( msg.appdata );
						System.Console.Out.WriteLine( "Received Response to request <" + msg.requestID + "> len [" + msg.appdata.Length + "] RES Data <" + res + ">" );
*/
						System.Console.Out.WriteLine( "Received Response to request <" + msg.requestID + "> len [" + msg.appdata.Length + "]" );
					} catch( System.Exception e ) {
						
					}
				}
			Responses++;
			break;
		}
	}
	
	public virtual long getRequests()
	{
		return (Requests);
	}
	public virtual long getRequestsCanceled()
	{
		return (RequestsCanceled);
	}
	public virtual long getResponses()
	{
		return (Responses);
	}
	public void setVerbose( bool verbose ) { this.verbose = verbose; }
} // appRequest

class umdsrequest:UMDSServerConnection
{
	private const System.String appl_name = "umdsrequest";
	private const System.String purpose = "Purpose: Send requests (and messages) on a single topic.";
	private const System.String usage = "Usage: umdsrequest [options] -S address[:port] topic\n" 
			+ "  -S address[:port] = Server address/name and optionally port\n"
			+ "                    A comma separated list of multiple servers may be provided\n"
			+ "Available options:\n"
			+ "  -A Suppress sending the application name to the server on login\n "
			+ "  -c filename = read config parameters from filename\n"
			+ "  -I Immediate Mode\n"
			+ "  -h = help\n"
			+ "  -l len = send messages of len bytes\n"
			+ "  -L linger = Allow traffic to drain for up to linger seconds\n"
			+ "              before closing the connection\n"
			+ "  -M msgs = send msgs number of messages\n"
			+ "  -N num_topics = Number of topics to send on\n"
			+ "  -P msec = pause after each send msec milliseconds\n"
			+ "  -r len = send requests of len bytes\n"
			+ "  -s num_secs = Print statistics every num_secs\n"
			+ "  -U username = set the user name and prompt for password\n"
			+ "  -v = be verbose in reporting to the console\n";
	
	internal int num_topics = 1;
	internal int msgs = 1000000;
	internal bool verbose = false;
	internal System.String conffname = null;
	internal int msglen = 25;
	internal int reqlen = 25;
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
	internal appRequest[] srcs = null;
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
					break;
				
				
				case 'h': 
					help = true;
					throw new System.Exception("Help:");
				
				
				case 'l': 
					try
					{
						msglen = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid message length:" + args[argnum]);
					}
					break;
				
				
				case 'L': 
					try
					{
						linger = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid Linger time:" + args[argnum]);
					}
					break;
				
				
				case 'M': 
					try
					{
						msgs = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid number of messages:" + args[argnum]);
					}
					break;
				
				
				case 'N': 
					try
					{
						num_topics = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid number of topics:" + args[argnum]);
					}
					break;
				
				case 'P': 
					try
					{
						pause = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid pause time:" + args[argnum]);
					}
					break;
				
				case 'r': 
					try
					{
						reqlen = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception e)
					{
						throw new System.Exception("Invalid request length:" + args[argnum]);
					}
					break;
				
				
				case 's': 
					try
					{
						stats_ms = Convert.ToInt32(args[argnum]) * 1000;
					}
					catch (System.Exception e)
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
				
				
				default: 
					throw new System.Exception("Error occurred processing command line option: " + ca[0] );
				
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
	* Java doesn't support turning off echo so the password will be displayed.
	*/
	private void  get_password()
	{
		System.Console.Out.WriteLine("Enter password");
		//UPGRADE_TODO: The differences in the expected value  of parameters for constructor 'java.io.BufferedReader.BufferedReader'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
		//UPGRADE_WARNING: At least one expression was used more than once in the target code. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1181'"
		System.IO.StreamReader linein = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).CurrentEncoding);
		try
		{
			password = linein.ReadLine();
		}
		catch (System.IO.IOException e)
		{
			System.Console.Error.WriteLine("Error reading password");
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
			//UPGRADE_TODO: The differences in the expected value  of parameters for constructor 'java.io.BufferedReader.BufferedReader'  may cause compilation errors.  "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1092'"
			//UPGRADE_WARNING: At least one expression was used more than once in the target code. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1181'"
			//UPGRADE_TODO: Constructor 'java.io.FileInputStream.FileInputStream' was converted to 'System.IO.FileStream.FileStream' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioFileInputStreamFileInputStream_javalangString'"
			linein = new System.IO.StreamReader(new System.IO.StreamReader(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read), System.Text.Encoding.Default).CurrentEncoding);
		}
		catch (System.IO.FileNotFoundException e)
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
			catch (System.IO.IOException e)
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
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: General loss occurred prior to sequence number " + msg.seqnum);
				break;
			
			case UMDSMessage.MSG_TYPE.DISCONNECT: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event:" + msg);
				break;
			
			case UMDSMessage.MSG_TYPE.LOGIN_DENIED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event:" + msg);
				auth_failed = true;
				break;
			
			case UMDSMessage.MSG_TYPE.SERVER_STOPPED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event:" + msg);
				auth_failed = true;
				break;
			
			case UMDSMessage.MSG_TYPE.SOURCE_CREATE :
				createCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Source Create completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_CREATE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Receiver create completed ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.SOURCE_DELETE :
				closeCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Source delete Completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_DELETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Receiver delete Completed. ID " + msg.srcidx + " Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.CONNECT :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Client is (re)connected to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGIN_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Client is Logged in to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGOUT_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Client has Logged out of server. Status " + msg.status + " " + msg.status_str );
				break;


			default: 
				getUMDS().log(LOG_LEVEL.INFO, "Unknown server connection event received " + msg);
				break;
			
		}
	}
	
	private umdsrequest(System.String[] args):base("")
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
				//UPGRADE_TODO: Method 'java.io.PrintStream.println' was converted to 'System.Console.Error.WriteLine' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javaioPrintStreamprintln_javalangObject'"
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
				System.Console.Error.WriteLine(e);
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
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			System.Console.Error.WriteLine("Error setting UMDS configuration: " + ex.ToString());
			System.Environment.Exit(1);
		}
		
		/*
		* Now start the connection to a server. If server-reconnect is enabled
		* it will return asynchronously. If server-reconnect is disabled,
		* start() will return when the connection is authenticated
		*/
		System.String ServerReconnect;
		try
		{
			ServerReconnect = svrconn.getProperty("server-reconnect");
		}
		catch (UMDSException e)
		{
			ServerReconnect = "1";
		}

		System.Console.Error.WriteLine("Property: server-reconnect" + ServerReconnect );
		try
		{
			svrconn.start();
		}
		catch (UMDSAuthenticationException e)
		{
			System.Console.Error.WriteLine("Authentication failed - check user name/password requirements of the server");
			System.Environment.Exit(1);
		}
		catch (UMDSException e)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
			System.Console.Error.WriteLine("Failed to create the server connection:" + e);
			System.Environment.Exit(1);
		}
		
		while (!svrconn.Authenticated && auth_failed == false)
		{
			System.Console.Out.WriteLine("Not Authorized" );
			try
			{
				//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
				System.Threading.Thread.Sleep( 100 );
			}
			catch (System.Threading.ThreadInterruptedException e)
			{
			}
		}
		
		if (auth_failed)
		{
			System.Environment.Exit(1);
		}
		
		/* Now the process of sending data begins... */
		if ( 0 == msglen ) {
			System.Console.Out.Write("Sending NO messages: Only " + msgs + " requests" );
		} else {
			System.Console.Out.Write("Sending " + msgs + " messages of size " + msglen + " bytes to topic [" + topic + "]");
		}
		
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
		byte[] message = null;
		byte[] dummymsg = System.Text.UTF8Encoding.UTF8.GetBytes("UMDS");
		if ( 0 != msglen ) {
			message = new byte[msglen];
			Array.Copy(dummymsg, 0, message, 0, dummymsg.Length < message.Length?dummymsg.Length:message.Length);

		
			for (int k = 1; (k * 1024) < message.Length && k < 256; k++)
				message[k * 1024] = (byte) k;
		}
		
		byte[] dummy_req = System.Text.UTF8Encoding.UTF8.GetBytes("UMDS REQ");
		byte[] request_data = new byte[ (reqlen < dummy_req.Length) ? dummy_req.Length : reqlen ];
		Array.Copy(dummy_req, 0, request_data, 0, (dummy_req.Length < request_data.Length)?dummy_req.Length:request_data.Length);
		
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
		srcs = new appRequest[num_topics];
		Creates = num_topics;
		createCount = 0;
		try
		{
			if (num_topics > 1)
			{
				for (int n = 0; n < num_topics; n++)
				{
					topicnames[n] = topic + "." + n;
					if (connsend == false) {
						srcs[n] = new appRequest(svrconn, topicnames[n], null);
						srcs[n].setVerbose( verbose );
					} else {
						srcs[n] = null;
					}
				}
			}
			else
			{
				topicnames[0] = topic;
				if (connsend == false) {
					srcs[0] = new appRequest(svrconn, topicnames[0], null);
					srcs[0].setVerbose( verbose );
				} else {
					srcs[0] = null;
				}
			}
		}
		catch (UMDSException e)
		{
			//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
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
		int req_counter = 0;
		bool	req_sent;
		long seq_counter = 0;
		bool	seq_sent;
		long	msg_counter = 0;
		long seq_end = msg_counter + msgs;
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
		while (msg_counter < seq_end && auth_failed == false)
		{
			req_sent = false;
			seq_sent = false;
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

			if ((pause_togo <= 0 || iter_time == 0) && 
				svrconn.Authenticated)
			{
				pause_togo = pause;
				/* Send a message! */
				if ( 0 != msglen ) {
					try
					{
						if ( srcs[ topic_idx ].canSend() ) {
							if (verbose)
							{
								System.Console.Out.WriteLine("Send: topic " + topic_idx + " msg: " + seq_counter );
							}
							srcs[topic_idx].send(message);
		
							seq_counter++;
							last_msgs_sent++;
							bytes_sent += msglen;
							last_bytes_sent += msglen;
							seq_sent = true;
						}
					}
					catch (UMDSAuthenticationException ex)
					{
						/* This can occur while auto-reconnecting */
						try
						{
							// Give the connection time to reestablish
							//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
							System.Threading.Thread.Sleep( 100 );
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
						}
					}
					catch (UMDSBadStateException ex)
					{
						System.Console.Error.WriteLine("Error sending message: Bad State:" + ex.ToString() + " seq " + seq_counter);
						try
						{
							// Give the source time to establish
							//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
							System.Threading.Thread.Sleep( 100 );
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
						}
					}
					catch (UMDSDisconnectException ex)
					{
						try
						{
							// Give the connection time to reestablish
							//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
							System.Threading.Thread.Sleep( 100 );
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
						}
					}
					catch (UMDSException ex)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
						System.Console.Error.WriteLine("Error sending message: " + ex.ToString() + " seq " + seq_counter);
						break;
					}
				}

				// Now send some requests
				try
				{
					if ( srcs[ topic_idx ].canSend()  )
					{
						if (verbose)
						{
							System.Console.Out.WriteLine("Req:  topic " + topic_idx + " reqid " + req_counter + " msgc " + msg_counter );
						}

						System.String temp = "UMDS REQ " + req_counter + " :";
						dummy_req = System.Text.UTF8Encoding.UTF8.GetBytes( temp );
						request_data = new byte[ (reqlen < dummy_req.Length) ? dummy_req.Length : reqlen ];
						Array.Copy( dummy_req, 0, 
								request_data, 0,
										 (dummy_req.Length < request_data.Length) ? dummy_req.Length : request_data.Length );

						srcs[topic_idx].request(  req_counter, request_data);
						req_counter++;
						req_sent = true;
					} else {
						try {
							// Give the connection time to settle down
							// Comes from running the server under valgrind.
							System.Threading.Thread.Sleep( 100 );
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
						}
					}
				}
				catch (UMDSAuthenticationException ex)
				{
					/* This can occur while auto-reconnecting */
					try
					{
						// Give the connection time to reestablish
						//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
						System.Threading.Thread.Sleep( 100 );
					}
					catch (System.Threading.ThreadInterruptedException e)
					{
					}
				}
				catch (UMDSBadStateException ex)
				{
					System.Console.Error.WriteLine("Error sending message: Bad State:" + ex.ToString() + " REQ " + req_counter);
					try
					{
						// Give the source time to establish
						//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
						System.Threading.Thread.Sleep( 100 );
					}
					catch (System.Threading.ThreadInterruptedException e)
					{
					}
				}
				catch (UMDSDisconnectException ex)
				{
					try
					{
						// Give the connection time to reestablish
						//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
						System.Threading.Thread.Sleep( 100 );
					}
					catch (System.Threading.ThreadInterruptedException e)
					{
					}
				}
				catch (UMDSException ex)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
					System.Console.Error.WriteLine("Error sending request: " + ex.ToString() + " req " + req_counter);
					break;
				}

				if (num_topics > 1)
				{
					topic_idx++;
					if (topic_idx == topicnames.Length)
						topic_idx = 0;
				}

				// Can only send messages from within this if
				if ( req_sent || seq_sent ) {
					msg_counter++;
				}
			}

			if (!svrconn.Authenticated)
			{
				// Defensive sleep while waiting for reconnection
				if (ServerReconnect.Equals("1"))
				{
					try
					{
						//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
						System.Threading.Thread.Sleep( 500 );
					}
					catch (System.Threading.ThreadInterruptedException e)
					{
					}
				}
				else
				{
					auth_failed = true;
				}
			}
			/* If the user is trying to send slowly, take a break */
			if (iter_time > 0)
			{
				try
				{
					//UPGRADE_TODO: Method 'java.lang.Thread.sleep' was converted to 'System.Threading.Thread.Sleep' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javalangThreadsleep_long'"
					System.Threading.Thread.Sleep( iter_time );
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
				}
			}
			
			long newprocd = UMDS.TimeAsMillis();
			long procdiff = (newprocd - procd);
			if (pause > 0)
				pause_togo -= procdiff;
			if (stats_ms > 0)
				stats_togo -= procdiff;
			procd = newprocd;
		}	// send loop

		// This block of code adds a variable length delay to closing the src(s) to allow
		// the responder time to respond; by waiting until all the requests have timedout. 
		bool fOKtoClose = false;
		int		CloseCount = 0;
		if ( true == fOKtoClose ) {	
			fOKtoClose = true;
		}
		while( !fOKtoClose ) {
			long	ReqCanceled = 0;
			for (int n = 0; n < srcs.Length; n++) {
				ReqCanceled += srcs[ n ].getRequestsCanceled();
			}

			if ( (msgs == ReqCanceled) ) {	// Wait until all requests timeout.
				fOKtoClose = true;
				if ( verbose ) {
					getUMDS().log( 0, "OK to Close" );	// log also places the message inthe debug log: BONUS!
				}
			} else {
				fOKtoClose = false;
				if ( verbose ) {
					getUMDS().log( 0, "NOT OK to Close srcs: m: " + msgs + " / C: " + ReqCanceled );
				}
			}

			// Wait for the last request to time out 
			if (!fOKtoClose ) {
				try {
					System.Threading.Thread.Sleep( 100 );
					CloseCount++;
				} catch (System.Threading.ThreadInterruptedException e) {
				}

				if ( 100 < CloseCount ) {		// This gives us a maximum wait of 10 seconds; the default request timeout.
					fOKtoClose = true;
					if ( verbose ) {
						getUMDS().log( 0, "Too much waiting" );
					}
				}
			}
		}

		if ( auth_failed == false ) {			// Have to sill be connected to delet resources
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
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.toString' may return a different value. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1043'"
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
		System.Console.Out.WriteLine("Sent " + req_counter + " requests of size " + reqlen + " bytes in " + secs2 + " seconds.");
		print_bw(secs2, (int) seq_counter, bytes_sent, seq_counter);

		System.Console.Out.WriteLine( "Done" );
	}
	
	private void  print_bw(double sec, int msgs, long bytes, long total_msgs)
	{
		double mps = 0;
		double bps = 0;
		double kscale = 1000;
		double mscale = 1000000;
		char mgscale = 'K';
		char bscale = 'K';
		long Requests = 0;
		long RequestsCanceled = 0;
		long Responses = 0;
		
		if (sec == 0.0) {
			return ; // Avoid divide by 0
		}
		
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
		for (int n = 0; n < srcs.Length; n++) {
			if ( null != srcs[n] ) {
				Requests += srcs[ n ].getRequests();
				RequestsCanceled += srcs[ n ].getRequestsCanceled();
				Responses += srcs[ n ].getResponses();
			}
		}

		TextNumberFormat nf = new TextNumberFormat(3,-1,-1,-1,3);
		System.Console.Out.WriteLine(sec + " secs. " + nf.FormatDouble(mps) + " " +
					 mgscale + "msgs/sec. " + nf.FormatDouble(bps) + " " + 
					 bscale + "bps " + 
					 total_msgs + " msgs -- REQ <" + Requests + "> Canceled <" + RequestsCanceled + "> RES <" + Responses + ">");
	}
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		new umdsrequest(args);
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