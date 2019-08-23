using System;
using com.latencybusters.umds;
using UMDSException = com.latencybusters.umds.UMDS.UMDSException;
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

class umdsreceive:UMDSServerConnection
{
	
	private static long end_msgs = 0;
	
	private const System.String appl_name = "umdsreceive";
	private const System.String purpose = "Purpose: Receive messages on a single topic.";
	private const System.String usage = "Usage: umdsreceive [options] -S address[:port] topic\n" +
			"  -S address[:port] = Server address/name and optionally port\n" + 
			"                    A comma separated list of multiple servers may be provided\n" + 
			"Available options:\n" + 
			"  -A Suppress sending the application name to the server on login\n " +
			"  -c filename = read config file filename\n" + 
			"  -h = help\n" + 
			"  -M num_msgs = End after num_msgs received\n" + 
			"  -N num_topics = Number of topics (receivers)\n" + 
			"  -s num_secs = print statistics every num_secs along with bandwidth\n" + 
			"  -S address:port = Server address and port\n" + 
			"  -U username = set the user name and prompt for password\n" + 
			"  -v = be verbose about each message\n" + 
			"  -W = Wildcard topic";
	
	internal int numtopics = 1;
	internal bool verbose = false;
	internal System.String conffname = null;
	internal int stats_ms = 1000;
	internal System.String server_list = null;
	internal System.String serverip = "localhost";
	internal System.String serverport = "1234";
	internal System.String topic = null;
	internal System.String username = null;
	internal System.String password = null;
	internal bool wildcard = false;
	internal bool auth_failed = false;
	internal int createCount = 0;
	internal int closeCount = 0;
	internal bool help = false;
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
				case 'v':
				case 'W':
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
				
				
				case 'h': 
					help = true;
					throw new System.Exception("Help:");
				
				
				case 'M': 
					try
					{
						end_msgs = Convert.ToInt64(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid number of messages:" + args[argnum]);
					}
					break;
				
				
				case 'N': 
					try
					{
						numtopics = Convert.ToInt32(args[argnum]);
					}
					catch (System.Exception)
					{
						throw new System.Exception("Invalid number of receivers:" + args[argnum]);
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
				
				
				case 'W': 
					wildcard = true;
					break;
			}
 			argnum++;
		}
		if (argnum >= args.Length)
			throw new System.Exception("Missing topic");
		
		topic = args[argnum];
		if (server_list == null)
			throw new System.Exception("Must supply server list (-S)");
	}
	
	/*
! 	* .NET doesn't support turning off echo so the password will be displayed.
	*/
	private void  get_password()
	{
		System.Console.Out.WriteLine("Enter password");
		System.IO.StreamReader linein = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).CurrentEncoding);
		try
		{
			password = linein.ReadLine();
		}
		catch (System.IO.IOException)
		{
			System.Console.Error.WriteLine("Error reading password");
		}
	}
	
	/*
	* Method to read a file of lines in "x = y" format
	* containing configuration data, and setting the connections
	* configuration
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
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: General loss occurred prior to sequence number " + msg.seqnum);
				break;
			
			case UMDSMessage.MSG_TYPE.DISCONNECT: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: " + msg);
				break;
			
			case UMDSMessage.MSG_TYPE.LOGIN_DENIED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: " + msg);
				auth_failed = true;
				break;
			
			case UMDSMessage.MSG_TYPE.SERVER_STOPPED: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: " + msg);
				auth_failed = true;
				break;
			
			case UMDSMessage.MSG_TYPE.SOURCE_CREATE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Source Create completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_CREATE :
				createCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Receiver create completed ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.SOURCE_DELETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Source delete Completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.RECEIVER_DELETE :
				closeCount++;
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Receiver delete Completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.CONNECT :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Client is (re)connected to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGIN_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Client is Logged in to server. Status " + msg.status + " " + msg.status_str );
				break;
	
			case UMDSMessage.MSG_TYPE.LOGOUT_COMPLETE :
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Client has Logged out of server. Status " + msg.status + " " + msg.status_str );
				break;

			default: 
				getUMDS().log(LOG_LEVEL.INFO, "UMDSreceive Event: Unknown server connection event received " + msg);
				break;
		}
	}
	
	private umdsreceive(System.String[] args):base("")
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
		
		/*
		* Create the server connection object for this application. The
		* application name is registered on the server when connected. The
		* server is not connected until start() is called.
		*/
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
		}
		catch (UMDSException ex)
		{
			System.Console.Error.WriteLine("Error setting UMDS configuration: " + ex.ToString());
			System.Environment.Exit(1);
		}
		
		/*
		* Now start the connection to a server. 
		* If server-reconnect is enabled it will return asynchronously.
		* If server-reconnect is disabled, start() will return
		* when the connection is authenticated
		*/
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
			try
			{
				svrconn.close();
			}
			catch (UMDSException ex)
			{
				System.Console.Error.WriteLine("Error closing connection: " + ex.ToString());
			}
			return ;
		}
		
		/* Create an array of receivers based on -N if it was used. */
		AppReceiver[] rcv = new AppReceiver[numtopics];
		
		Creates = numtopics;
		createCount = 0;
		try
		{
			for (int n = 0; n < numtopics; n++)
			{
				System.String fulltopic;
				
				/* Create a topic string. Append .N to the name if -N was used */
				if (numtopics == 1)
					fulltopic = topic;
				else
					fulltopic = topic + "." + n;
				
				/*
				* Create the receiver object for this topic AppReceiver derives
				* from UMDSReceiver which on creation registers with the server.
				*/
				rcv[n] = new AppReceiver(svrconn, fulltopic, n, wildcard);
				
				/* Copy the verbose flag from the command line */
				rcv[n].verbose = verbose;
			}
		}
		catch (UMDSException e)
		{
			System.Console.Out.WriteLine("Failed to create receiver object:" + e);
			System.Environment.Exit(5);
		}
		
		/* Loop over all the receivers and start timing */
		for (int n = 0; n < numtopics; n++) {
			rcv[n].newInterval();
		}

		/*
		* Now start sending data until the number of requested messages have
		* been received or the server is no longer authenticated. (Most likely
		* disconnected)
		*/
		System.Console.Out.WriteLine("Printing Statistics every:" + stats_ms + " ms " );
		while (auth_failed == false)
		{
			/* Dump stats for each receiver object and reset the timing */
			for (int n = 0; n < numtopics; n++)
			{
				if (stats_ms > 0) {
					rcv[n].print_bw();
				}
				rcv[n].newInterval();
			}
			
			/* Check the server connection hasn't encountered any
			* errors and exit if it has. This might come from
			* a receiver too.
			*/
			if (svrconn.Error != UMDS.ERRCODE.NO_ERROR)
			{
				System.Console.Error.WriteLine("Server Connection Error detected:" + svrconn.ErrorStr);
				break;
			}

			/* Wait for the statistics interval */
			try
			{
				// Sleep the interval or 1 second.
				System.Threading.Thread.Sleep( ((stats_ms > 0) ? stats_ms : 1000 ) );
			}
			catch (System.Threading.ThreadInterruptedException e)
			{
			}
			
			/* Count all the received messages on all the receivers */
			int total_msg_count = 0;
			for (int n = 0; n < numtopics; n++) {
				total_msg_count = (int) (total_msg_count + rcv[n].total_msg_count);
			}
			
			/* If we received enough messages, end */
			if (end_msgs > 0 && total_msg_count >= end_msgs) {
				break;
			}
		}
		
		/* Summarise the results and output */
		print_summary(rcv);
		
		/* Close all the receivers and the connection, we are done */
		Closes = 0;
		if (auth_failed == false)
		{
			try
			{
				for (int n = 0; n < numtopics; n++) {
					if ( rcv[ n ] != null ) {
						rcv[n].close();
						Closes++;
					}
				}
				
				while( closeCount < Closes ) {
					if ( verbose ) {
						System.Console.Error.WriteLine("Receivers: " + Closes + "  Closed: " + closeCount );
					}
					System.Threading.Thread.Sleep( 100 );
				}
				if ( verbose ) {
					System.Console.Error.WriteLine("All receivers closed: Receivers: " + Closes + "  Closed: " + closeCount );
				}

				System.Console.Error.WriteLine("Closing Connection " );
				svrconn.close();
			}
			catch (UMDSException ex)
			{
				System.Console.Error.WriteLine("Error closing receiver: " + ex.ToString());
			}
		}
		else {
			System.Console.Error.WriteLine("Exiting - no longer authenticated.");
		}

		System.Console.Error.WriteLine( "Connection Closed" );

		System.Console.Error.WriteLine( "Done" );
	}
	
	public virtual void  print_summary(AppReceiver[] rcv)
	{
		double total_time_sec, mps, bps;
		long total_byte_count = 0;
		System.DateTime created_time = System.DateTime.Now;
		System.DateTime data_end_time = created_time;
		long total_msg_count = 0;
		
		for (int n = 0; n < numtopics; n++)
		{
			total_byte_count += rcv[n].total_byte_count;
			total_msg_count += rcv[n].total_msg_count;
			if ( rcv[n].created_time < created_time )
				created_time = rcv[n].created_time;
			if ( rcv[n].data_end_time > data_end_time )
				data_end_time = rcv[n].data_end_time;
		}
		
		total_time_sec = 0.0;
		mps = 0.0;
		bps = 0.0;
		
		long bits_received = total_byte_count * 8;
		System.TimeSpan elapsed = (data_end_time - created_time);
		long total_time = elapsed.Milliseconds +
					( elapsed.Seconds * 1000 ) +				// millis per second
					( elapsed.Minutes * 60 * 1000 ) +			// seconds per minute ...
					( elapsed.Hours * 60 * 60 * 1000 ) +		// minutes per hour
					( elapsed.Days * 24 * 60 * 60 * 1000 );		// Hours per day
		
		TextNumberFormat nf = new TextNumberFormat(3,-1,-1,-1,3);
		
		total_time_sec = total_time / 1000.0;
		
		if (total_time_sec > 0)
		{
			mps = total_msg_count / total_time_sec;
			bps = bits_received / total_time_sec;
		}
		
		System.Console.Out.WriteLine("\nTotal time         : " + nf.FormatDouble(total_time_sec) + "  sec");
		System.Console.Out.WriteLine("Messages received  : " + total_msg_count);
		System.Console.Out.WriteLine("Bytes received     : " + total_byte_count);
		System.Console.Out.WriteLine("Avg. throughput    : " + nf.FormatDouble(mps / 1000.0) + " Kmsgs/sec, " + nf.FormatDouble(bps / 1000000.0) + " Mbps\n\n");
	}
	
	/// <summary> The AppReceiver class decends from the UMDSReceiver. It implements the
	/// application callbacks for a receiver object.
	/// 
	/// Applications should overload onMessage() and onEvent()
	/// </summary>
	public class AppReceiver:UMDSReceiver
	{
		
		/// <summary> The constructor for the application receiver class
		/// 
		/// </summary>
		/// <param name="sconn">The server connection for this receiver object
		/// </param>
		/// <param name="topic">The topic this receiver is listening to
		/// </param>
		/// <param name="recvnum">The number of this receiver in umdsreceive
		/// (this is not passed to the UMDS Client API)
		/// </param>
		/// <throws>  UMDSException </throws>
		/// <summary>             (From UMDSReceiver constructor)
		/// </summary>
		/// <seealso cref="UMDSReceiver">
		/// </seealso>
		public AppReceiver(UMDSServerConnection sconn, System.String topic, int recvnum, bool wildcard):base(sconn, topic, wildcard)
		{
			rcvnum = "" + recvnum;
		}
		
		// The string version of this receivers ID in umdsreceive
		internal System.String rcvnum;
		
		/// <summary> The onMessage method is called by the UMDS client code. Application
		/// logic should be added within this method for processing each message.
		/// 
		/// This message is called from the UMDS client thread processing the
		/// socket. Blocking or complex code in this function can prevent the
		/// socket from being processed. Ensure that application logic is kept to
		/// a minimum, and hands off the received message to an application
		/// thread.
		/// </summary>
		public override void  onMessage(UMDSMessage msg)
		{
			/* Count this message and the number of bytes received */
			interval_msg_count++;
			if (msg.appdata != null)
				interval_byte_count += msg.length();
			if (msg.recovered)
				total_rxmsg_count++;
			
			/*
			* If the verbose option was used, dump data about the message.
			* Assume the message begins with text
			*/
			if (verbose)
			{
				System.Console.Out.WriteLine(rcvnum + ": Yahoo! Received Data Message of Length " + msg.length() + " Sequence Number " + msg.seqnum + " source " + msg.srcidx + " topic " + msg.topic + (msg.recovered?" -RX-":""));
				
				// Find the first 0 byte and create a string with the text
				if (msg.appdata.Length > 0)
				{
					int zero;
					for (zero = 0; zero < msg.appdata.Length && msg.appdata[zero] != 0; zero++)
						;
					char [] appbs = new char[zero];
					for (zero = 0; zero < msg.appdata.Length && msg.appdata[zero] != 0; zero++)
						appbs[zero] = (char) msg.appdata[zero];
					System.String appstr = new System.String(appbs);
					System.Console.Out.WriteLine(appstr);
				}
			}
		}
		
		/// <summary> The onEvent method is called by the UMDS client code. Application
		/// code should be added to handle various events. These events are
		/// typically errors.
		/// </summary>
		public override void  onEvent(UMDSMessage msg)
		{

			switch (msg.type)
			{
			case UMDSMessage.MSG_TYPE.LOSS:
				/*
				 * Received when the server detects loss on this topic.
				 */
				log(LOG_LEVEL.INFO, "AppReceiver.onEvent(): Receiver message loss occurred prior to sequence number " + msg.seqnum);
				break;

			case UMDSMessage.MSG_TYPE.DISCONNECT:
				/*
				 * Received when the receiver becomes disconnected from the server
				 */
				log(LOG_LEVEL.INFO, "AppReceiver.onEvent(): Receiver is disconnected from the Server" );
				break;

			case UMDSMessage.MSG_TYPE.SERVER_DENIED:
				/*
				 * Received if the server denied creation of the object. The
				 * UMDS client code will unregister this object with the server
				 * upon returning. Applications will need to create a new
				 * receiver object to continue.
				 */
				log(LOG_LEVEL.INFO, "AppReceiver.onEvent(): Server denied receiver operation");
				break;

			case UMDSMessage.MSG_TYPE.RECEIVER_CREATE :
				/*
				 * Received after the server Acknowledges the creation of this receiver.
				 * Also received upon reconnection to the server.
				 */
				if ( UMDSMessage.MSG_STATUS.CLOSE_PENDING == msg.status ) {
					log(LOG_LEVEL.INFO, "AppReceiver Receiver create with close pending: ID " + msg.srcidx + " Status " + msg.status + " : " + msg.status_str );
				} else {
					log(LOG_LEVEL.INFO, "AppReceiver Receiver create completed ID " + msg.srcidx + " Status " + msg.status + " : " + msg.status_str );
				}
				break;


			case UMDSMessage.MSG_TYPE.RECEIVER_DELETE :
				/*
				 * Received after the server Acknowledges the deletion of this receiver. After
				 * receiving this event, no further messages will be received on this topic.
				 * (Messages may still be received after the call to close() returns, while 
				 * the server is still processing the close / delete request.)
				 */
				log(LOG_LEVEL.INFO, "AppReceiver.onEvent(): Receiver has been deleted");
				break;

			default:
				/*
				 * An unknown event. The application developer needs to
				 * determine what the correct behaviour is with respect to their
				 * application in this case.
				 */
				log(LOG_LEVEL.INFO, "AppReceiver.onEvent(): Unknown event received " + msg.type);
				break;
			}
		}
		
		/// <summary>Initial time </summary>
		public System.DateTime created_time = System.DateTime.Now;
		
		/// <summary>Start MS of interval </summary>
		public System.DateTime data_start_time = System.DateTime.Now;
		
		/// <summary>End MS of interval </summary>
		public System.DateTime data_end_time = System.DateTime.Now;
		
		/// <summary>Number of messages received in interval </summary>
		public int interval_msg_count = 0;
		
		/// <summary>Number of recovered messages received </summary>
		public int total_rxmsg_count = 0;
		
		/// <summary>Absolute total number of messages </summary>
		public long total_msg_count = 0;
		
		/// <summary>Number of bytes received in interval </summary>
		public long interval_byte_count = 0;
		
		/// <summary>Absolute total number of bytes </summary>
		public long total_byte_count = 0;
		
		/// <summary>Verbose command line flag </summary>
		public bool verbose = false;
		
		public virtual void  newInterval()
		{
			data_start_time = data_end_time;
			data_end_time = System.DateTime.Now;
			total_msg_count += interval_msg_count;
			total_byte_count += interval_byte_count;
			interval_msg_count = 0;
			interval_byte_count = 0;
		}
		
		internal static TextNumberFormat nf = null;
		internal static TextNumberFormat nfsec = null;
		
		public virtual void  print_bw()
		{
			if (data_end_time == data_start_time)
				return ;
			
			int msgs = interval_msg_count;
			System.TimeSpan elapsed = (data_end_time - data_start_time);
			int msec = elapsed.Milliseconds +
					( elapsed.Seconds * 1000 ) +				// millis per second
					( elapsed.Minutes * 60 * 1000 ) +			// seconds per minute ...
					( elapsed.Hours * 60 * 60 * 1000 ) +		// minutes per hour
					( elapsed.Days * 24 * 60 * 60 * 1000 );		// Hours per day
			if (msec < 0) msec = -msec;
			long bytes = interval_byte_count;
			
			double sec;
			double mps = 0.0, bps = 0.0;
			double kscale = 1000.0, mscale = 1000000.0;
			char mgscale = 'K', bscale = 'K';
			
			sec = msec / 1000.0;
			mps = ((double) msgs) / sec;
			bps = ((double) bytes * 8) / sec;
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
			if (nf == null)
			{
				nf = new TextNumberFormat(3,3,-1,3,-1);
			}
			if (nfsec == null)
			{
				nfsec = new TextNumberFormat(3,-1,-1,3,-1);
			}
			System.Console.Error.WriteLine(rcvnum + ":" + nfsec.FormatDouble(sec) + " secs. " + nf.FormatDouble(mps) + " " + mgscale + "msgs/sec. " + nf.FormatDouble(bps) + " " + bscale + "bps" + " " + msgs + " msgs " + (total_msg_count + msgs) + " total msgs " + total_rxmsg_count + " rx msgs");
			
		}
	}
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		new umdsreceive(args);
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
