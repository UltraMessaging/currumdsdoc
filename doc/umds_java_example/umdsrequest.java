import com.latencybusters.umds.*;
import com.latencybusters.umds.UMDS.*;

import java.io.*;
import java.util.*;
import java.text.NumberFormat;

// See http://kb.29west.com/index.php?View=entry&EntryID=8
import org.openmdx.uses.gnu.getopt.*;

/*
 (C) Copyright 2005,2024 Informatica Inc.  Permission is granted to licensees to use
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

class appRequest extends UMDSSource {

	long Requests;
	long RequestsCanceled;
	long Responses;
	boolean verbose = false;

	public appRequest(UMDSServerConnection sconn, String topic, String[] options) throws UMDSException {
		super(sconn,topic,options);
		
		Requests = 0;
		RequestsCanceled = 0;
		Responses = 0;
	}

	public void request( int request_id, byte[] request_data )  throws UMDSException {
		Requests++;
		super.request( request_id, request_data );
	}

	/**
	 * The onEvent method is called by the UMDS client code. Application
	 * code should be added to handle various events. These events are
	 * typically errors.
	 */
	public void onEvent(UMDSMessage msg) {
		super.onEvent( msg);

		if (  UMDSMessage.MSG_TYPE.REQUEST_CANCELED == msg.type ) {
			if ( verbose ) {
				log(LOG_LEVEL.INFO, "Request <"  + msg.requestID + "> Timedout.");
			}
			RequestsCanceled++;
		}
	}

	/**
	 * The onResponse method is called by the UMDS client code. Application
	 * code should be added to handle various message types. 
	 */
	public void onResponse(UMDSMessage msg) {
		if ( verbose ) {
			System.out.println( "onResponse called: type < " + msg.type + " >" );
		}
		switch ( msg.type ) {
			case UMDSMessage.MSG_TYPE.RESPONSE :
				if ( verbose ) {
					try {
/*
						// This code block will display the verifiable content in the request from the UMDS response app. 
						// The UM lbmres app does not provide this text.
						//

						String res_temp = new String( msg.appdata,"UTF-8");
						int breakAt = res_temp.indexOf( ':' );
						String res = res_temp.substring( 0, breakAt );
						System.out.println( "Received Response to request <" + msg.requestID + "> len [" + msg.appdata.length + "] RES Data <" + res + ">" );
*/
						String res_temp = new String( msg.appdata,"UTF-8");
						System.out.println( "Received Response to request <" + msg.requestID + "> len [" + msg.appdata.length + "]" );
					} catch( UnsupportedEncodingException e ) {
						
					}
				}
				Responses++;
				break;
		}
	}

	public long getRequests() { return( Requests ); }
	public long getRequestsCanceled() { return( RequestsCanceled ); }
	public long getResponses() { return( Responses ); }
	public void setVerbose( boolean verbose ) { this.verbose = verbose; }

}	// appRequest

class umdsrequest extends UMDSServerConnection {
	private static final String appl_name = "umdsrequest";
	private static final String purpose = "Purpose: Send requests (and messages) on a single topic.";
	private static final String usage = "Usage: umdsrequest [options] -S address[:port] topic\n"
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

	int num_topics = 1;
	int msgs = 1000000;
	boolean verbose = false;
	String conffname = null;
	int msglen = 25;
	int reqlen = 25;
	long stats_ms = 1000;
	int pause = 0;
	String server_list = null;
	String serverip = "localhost";
	String serverport = "1234";
	int linger = 0;
	String topic = null;
	String username = null;
	String password = null;
	boolean auth_failed = false;
	boolean help = false;
	boolean connsend = false;
	int createCount = 0;
	int closeCount = 0;
	appRequest[] srcs = null;
	boolean sendAppName = true;

	private void process_cmdline(String[] args) throws Exception {
		int c = -1;

		Getopt gopt = new Getopt(appl_name, args, "+c:AhIl:M:N:P:L:r:S:s:U:v");
		while ((c = gopt.getopt()) != -1) {
			switch (c) {
			case 'A' :
				sendAppName = false;
				break;

			case 'c':
				conffname = gopt.getOptarg();
				break;

			case 'I':
				connsend = true;
				break;

			case 'h':
				help = true;
				throw new Exception("Help:");

			case 'l':
				try {
					msglen = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid message length:" + gopt.getOptarg());
				}
				break;

			case 'r':
				try {
					reqlen = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid request length:" + gopt.getOptarg());
				}
				break;

			case 'L':
				try {
					linger = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid Linger time:" + gopt.getOptarg());
				}
				break;

			case 'M':
				try {
					msgs = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid number of messages:" + gopt.getOptarg());
				}
				break;

			case 'N':
				try {
					num_topics = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid number of topics:" + gopt.getOptarg());
				}
				break;

			case 'P':
				try {
					pause = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid pause time:" + gopt.getOptarg());
				}
				break;

			case 's':
				try {
					stats_ms = Long.parseLong(gopt.getOptarg()) * 1000;
				} catch (Exception e) {
					throw new Exception(
							"Invalid number of seconds for statistics:" + gopt.getOptarg());
				}
				break;

			case 'S':
				server_list = gopt.getOptarg();
				break;

			case 'U':
				username = gopt.getOptarg();
				break;

			case 'v':
				verbose = true;
				break;

			default:
				throw new Exception( "Error occurred processing command line options");
			}
		}
		if (gopt.getOptind() >= args.length)
			throw new Exception("Missing topic");

		topic = args[gopt.getOptind()];
		// Validate Mandatory options
		if (server_list == null)
			throw new Exception("Must supply server list (-S)");

	}

	/*
	 * Java doesn't support turning off echo so the password will be displayed.
	 */
	private void get_password() {
		System.out.println("Enter password");
		BufferedReader linein = new BufferedReader(new InputStreamReader(
				System.in));
		try {
			password = linein.readLine();
		} catch (IOException e) {
			System.err.println("Error reading password");
			e.printStackTrace();
		}
	}

	/*
	 * Method to read a file of lines in "x = y" format containing configuration
	 * data, and setting the connections configuration
	 */
	boolean read_config(UMDSServerConnection sconn, String filename) {
		boolean ret = true;

		BufferedReader linein;
		try {
			linein = new BufferedReader(new InputStreamReader(
					new FileInputStream(filename)));
		} catch (FileNotFoundException e) {
			System.err.println("File " + filename + " not found.");
			return false;
		}
		do {
			String line;

			try {
				line = linein.readLine();
			} catch (IOException e) {
				System.err.println("Error reading config file " + filename);
				return false;
			}
			if (line == null)
				break;
			if (line.startsWith("#"))
				continue;

			StringTokenizer listTokens;

			listTokens = new StringTokenizer(line, "=");
			if (listTokens.countTokens() == 0)
				return ret;
			if (listTokens.countTokens() != 2) {
				System.err.println("Malformed config line " + line);
				ret = false; // Allow file to be read to show all errors
				continue;
			}

			String[] newprop = new String[2];

			for (int t = 0; t < newprop.length; t++) {
				newprop[t] = listTokens.nextToken().trim();
			}
			try {
				sconn.setProperty(newprop);
				System.err.println("Setting server connection property "
						+ newprop[0] + " to " + newprop[1]);
			} catch (UMDSException e) {
				System.err.println("Failed to set server connection property "
						+ newprop[0] + " to " + newprop[1]);
				ret = false;
			}
		} while (true);
		return ret;
	}

	/**
	 * Event Notification function. This function is called when non-data events
	 * occur, E.G. Disconnection reported by the server.
	 * 
	 * @param msg
	 *            The message containing the event
	 */
	public void onEvent(UMDSMessage msg) {
		switch (msg.type) {
		case UMDSMessage.MSG_TYPE.LOSS:
			/*
			 * Received when the server detects general MIM loss.
			 */
			getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: General loss occurred prior to sequence number " + msg.seqnum);
			break;

		case UMDSMessage.MSG_TYPE.DISCONNECT:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: " + msg);
			break;

		case UMDSMessage.MSG_TYPE.LOGIN_DENIED:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: " + msg);
			auth_failed = true;
			break;

		case UMDSMessage.MSG_TYPE.SERVER_STOPPED:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: " + msg);
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
			getUMDS().log(LOG_LEVEL.INFO, "UMDSrequest Event: Unknown server connection event received " + msg);
			break;
		}
	}

	private umdsrequest(String[] args) {
		super("");
		int Creates = 0;
		int	Closes = 0;

		/* Process the command line arguments */
		try {
			process_cmdline(args);
			if ( sendAppName ) {
				setProperty("appl-name", appl_name );
			}
		} catch (Exception e) {
			if (!help) {
				System.err.println(e);
			}
			System.err.println(UMDS.version());
			System.err.println(purpose);
			System.err.println(usage);
			System.exit(1);
		}

		/* If a user name was provided, get the password */
		if (username != null)
			get_password();

		UMDSServerConnection svrconn = this;

		/*
		 * If an application config file is provided, read it and set the server
		 * connection properties.
		 */
		if (conffname != null) {
			if (read_config(svrconn, conffname) == false)
				System.exit(1);
		}

		/* Set the list of servers, user name and password in the connection */
		try {
			svrconn.setProperty("server-list", server_list);
			if (username != null) {
				svrconn.setProperty("user", username);
				svrconn.setProperty("password", password);
			}

			// Set the linger value to set a timeout on draining data when
			// closing
			if (linger > 0)
				svrconn.setProperty("client-linger", "" + (linger * 1000));

		} catch (UMDSException ex) {
			System.err.println("Error setting UMDS configuration: "
					+ ex.toString());
			System.exit(1);
		}

		/*
		 * Now start the connection to a server. If server-reconnect is enabled
		 * it will return asynchronously. If server-reconnect is disabled,
		 * start() will return when the connection is authenticated
		 */
		String ServerReconnect;
		try {
			ServerReconnect = svrconn.getProperty("server-reconnect" );
		} catch( UMDSException e ) {
			ServerReconnect = "1";
		}
		// System.err.println("Property: server-reconnect" + ServerReconnect );
		try {
			svrconn.start();
		} catch (UMDSAuthenticationException e) {
			System.err
					.println("Authentication failed - check user name/password requirements of the server");
			System.exit(1);
		} catch (UMDSException e) {
			System.err.println("Failed to create the server connection:" + e);
			System.exit(1);
		}

		while(!svrconn.isAuthenticated() && auth_failed == false) {
			try {
				Thread.sleep(100);
			} catch(InterruptedException e) { }
		}

		if(auth_failed) {
			System.exit(1);
		}

		/* Now the process of sending data begins... */
		if ( 0 == msglen ) {
			System.out.print("Sending NO messages: Only " + msgs + " requests");
		} else {
			System.out.print("Sending " + msgs + " messages and requests of size " + msglen
					+ " bytes to topic [" + topic + "]");
		}

		if (pause == 0) {
			System.out.println(" as fast as the system can");
		} else {
			System.out.println(" pausing " + pause + "ms");
		}

		/*
		 * Create a payload with the string "UMDS" and set every K to contain
		 * and offset for payload verification
		 */
		byte[] message = null;
		byte[] dummymsg = "UMDS".getBytes();
		if ( 0 != msglen ) {
			message = new byte[msglen];
			System.arraycopy(dummymsg, 0, message, 0,
					dummymsg.length < message.length ? dummymsg.length
							: message.length);

			for (int k = 1; (k * 1024) < message.length && k < 256; k++)
				message[k * 1024] = (byte) k;

		}

		byte[] dummy_req = "UMDS REQ".getBytes();
		byte[] request_data = new byte[ ( reqlen < dummy_req.length ) ? dummy_req.length : reqlen ];
		System.arraycopy( dummy_req, 0, 
						  request_data, 0,
						  (dummy_req.length < request_data.length) ? dummy_req.length : request_data.length );
		for (int k = 1; (k * 1024) < request_data.length && k < 256; k++) {
			request_data[k * 1024] = (byte) k;
		}


		/*
		 * A simple loop to send messages is used. This warning indicates a long
		 * pause interval was provided relative to the stats interval.
		 */
		if ((pause > stats_ms) && (stats_ms > 0)) {
			System.out.println("Warning - Pause rate " + pause
					+ " exceeds stats interval : " + stats_ms
					+ " (will see intervals of 0 sends)");
		}

		/*
		 * Create an array of topic names (and sources) to send on. If -N was
		 * provided, each topic will be used in round robin fashion.
		 */
		String[] topicnames = new String[num_topics];
		srcs = new appRequest[num_topics];
		try {
			if (num_topics > 1) {
				for (int n = 0; n < num_topics; n++) {
					topicnames[n] = topic + "." + n;
					if (connsend == false) {
						srcs[n] = new appRequest(svrconn, topicnames[n], null);
						srcs[n].setVerbose( verbose );
					} else {
						srcs[n] = null;
					}
				}
			} else {
				topicnames[0] = topic;
				if (connsend == false) {
					srcs[0] = new appRequest(svrconn, topicnames[0], null);
					srcs[0].setVerbose( verbose );
				} else {
					srcs[0] = null;
				}
			}
		} catch (UMDSException e) {
			System.err.println("Failed to create the source:" + e);
			System.exit(1);
		}

		/* Data used in sending */
		int req_counter = 0;
		boolean req_sent;
		long seq_counter = 0;
		boolean seq_sent;
		long msg_counter = 0;
		long seq_end = msg_counter + msgs;
		long last_msgs_sent = 0;
		long last_bytes_sent = 0;
		long bytes_sent = 0;
		long start_time = System.currentTimeMillis();
		long end_time = 0, last_time = start_time;

		/*
		 * A simple loop is run to send messages and dump stats. Calculate the
		 * time to wait for each iteration and use ms times decrementing each
		 * iteration to trigger sending of messages and dumping of stats.
		 */
		long pause_togo = pause;
		long stats_togo = stats_ms;
		long iter_time;

		if (pause > 0) {
			if (pause == stats_ms)
				iter_time = pause;
			else {
				if (pause < stats_ms)
					iter_time = pause % (stats_ms - pause);
				else {
					if (pause > stats_ms)
						iter_time = stats_ms;
					else
						iter_time = stats_ms / (stats_ms / (pause - stats_ms));
				}
			}
		} else
			iter_time = 0;

		System.out.println("stats_ms " + stats_ms + " pause " + pause
				+ " iteration " + iter_time);

		/* Start sending messages! */
		int topic_idx = 0;
		long procd = System.currentTimeMillis();
		while (msg_counter < seq_end && auth_failed == false) {
			req_sent = false;
			seq_sent = false;
			if (stats_togo <= 0 || iter_time == 0) {
				end_time = System.currentTimeMillis();
				if ((last_time > 0) && (end_time - last_time >= stats_ms)) {
					/* New stats interval */
					double secs = (end_time - last_time) / 1000.;
					print_bw(secs, (int) last_msgs_sent, last_bytes_sent, seq_counter);
					last_time = end_time;
					last_msgs_sent = 0;
					last_bytes_sent = 0;
				}
				stats_togo = stats_ms;
			}

			if ((pause_togo <= 0 || iter_time == 0) && 
					svrconn.isAuthenticated()) {
				pause_togo = pause;
				/* Send a message! */
				if ( 0 != msglen ) {
					try {
						if ( srcs[topic_idx].canSend() ) {
							if ( verbose ) {
								System.out.println("Send: topic " + topic_idx + " msg: " + seq_counter);
							}
							srcs[topic_idx].send(message);
	
							seq_counter++;
							last_msgs_sent++;
							bytes_sent += msglen;
							last_bytes_sent += msglen;
							seq_sent = true;
						}
					} catch (UMDSAuthenticationException ex) {
						/* This can occur while auto-reconnecting */
						try {
							// Give the connection time to reestablish
							Thread.sleep(10);
						} catch (InterruptedException e) {
						}
					} catch (UMDSBadStateException ex) {
						try {
							System.out.println("Error sending message: Bad State:" + ex.toString() + " seq " + seq_counter);
							// Give the source time to establish
							Thread.sleep(10);
						} catch (InterruptedException e) {
						}
					} catch (UMDSDisconnectException ex) {
						try {
							// Give the connection time to reestablish
							Thread.sleep(10);
						} catch (InterruptedException e) {
						}
	
					} catch (UMDSException ex) {
						System.err.println("Error sending message: "
								+ ex.toString() + " seq " + seq_counter);
						break;
					}
				}

				// Now send some requests
				try {
					if ( srcs[topic_idx].canSend() ) {
						if ( verbose ) {
							System.out.println("Req:  topic " + topic_idx + " reqid " + req_counter + " msgc " + msg_counter );
						}

						String temp = "UMDS REQ " + req_counter + " :";
						dummy_req = temp.getBytes();
						request_data = new byte[ ( reqlen < dummy_req.length ) ? dummy_req.length : reqlen ];
						System.arraycopy( dummy_req, 0, 
										  request_data, 0,
										 (dummy_req.length < request_data.length) ? dummy_req.length : request_data.length );
						// Try pretty hard not to step on the tracking string above 
						for (int k = 1; (k * 1024) < request_data.length && k < 256; k++) {
							request_data[k * 1024] = (byte) k;
						}

						srcs[topic_idx].request( req_counter, request_data );
						req_counter++;
						req_sent = true;
					} else {
						try {
							// Give the connection time to settle down
							Thread.sleep(10);
						} catch (InterruptedException e) {
						} // Interupted exception
					}
				} catch (UMDSAuthenticationException ex) {
					/* This can occur while auto-reconnecting */
					try {
						// Give the connection time to reestablish
						Thread.sleep(10);
					} catch (InterruptedException e) {
					} // Interupted exception
				} catch (UMDSBadStateException ex) {
					System.out.println("Error sending message: Bad State:" + ex.toString() + " REQ " + req_counter);
					try {
						// Give the source time to establish
						Thread.sleep(10);
					} catch (InterruptedException e) {
					} // Interupted exception
				} catch (UMDSDisconnectException ex) {
					try {
						// Give the connection time to reestablish
						Thread.sleep(10);
					} catch (InterruptedException e) {
					} // Interupted exception
	
				} catch (UMDSException ex) {
					System.err.println("Error sending request: "
							+ ex.toString() + " req " + req_counter);
					break;
				}

				if (num_topics > 1) {
					topic_idx++;
					if (topic_idx == topicnames.length)
						topic_idx = 0;
				}

				// Can only send messages from within this if
				if ( req_sent || seq_sent ) {
					msg_counter++;
				}
			}
			if (!svrconn.isAuthenticated()) {
				// Defensive sleep while waiting for reconnection
				if ( ServerReconnect.equals( "1" ) ) {
					try {
						Thread.sleep(500);
					} catch (InterruptedException e) {
					}
				}
				else {
					auth_failed = true;
				}
			}
			/* If the user is trying to send slowly, take a break */
			if (iter_time > 0) {
				try {
					Thread.sleep(iter_time);
				} catch (InterruptedException e) {
				}
			}

			long newprocd = System.currentTimeMillis();
			long procdiff = (newprocd - procd);
			if (pause > 0)
				pause_togo -= procdiff;
			if (stats_ms > 0)
				stats_togo -= procdiff;
			procd = newprocd;
		}	// send loop

// This block of code adds a variable length delay to closing the src(s) to allow
// the responder time to respond; by waiting until all the requests have timedout. 
		boolean fOKtoClose = false;
		int		CloseCount = 0;
		if ( true == auth_failed ) {
			fOKtoClose = true;
		}
		while( !fOKtoClose ) {
			long	ReqCanceled = 0;
			for (int n = 0; n < srcs.length; n++) {
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
					Thread.sleep( 100 );
					CloseCount++;
				} catch (InterruptedException e) {
				}

				if ( 100 < CloseCount ) {		// This gives us a maximum wait of 10 seconds; the default request timeout.
					fOKtoClose = true;
					if ( verbose ) {
						getUMDS().log( 0, "Too much waiting" );
					}
				}
			}
		}

		if ( auth_failed == false ) {      // Still have to be connected to delete resources
			Closes = 0;
			for (int n = 0; n < srcs.length; n++) {
				if ( null != srcs[n] )
				{
					try {
						srcs[ n ].close();
						// srcs[ n ] = null;	Don't null, need the object for stats
						Closes++;
					}
					catch( UMDSException ex ) {
						System.err.println("Error closing source[" + n + "]: " + ex.toString() );
					}
				}
			}
	
			while( closeCount < Closes ) {
				if ( verbose ) {
					System.err.println("Sources: " + Closes + "  Closed: " + closeCount );
				}
				try {
					Thread.sleep( 100 );
					CloseCount++;
				} catch (InterruptedException e) {
				}
			}
			if ( verbose ) {
				System.err.println("All sources closed: Sources: " + Closes + "  Closed: " + closeCount );
			}
	
			/* Close the connection, we are done */
			try {
				svrconn.close();
			} catch (UMDSException ex) {
				System.err.println("Error closing server connection: " + ex.toString());
			}
		}

		/* Finished sending, Dump the summary statistics */
		end_time = System.currentTimeMillis();
		double secs = (end_time - start_time) / 1000.;
		System.out.println("Sent " + seq_counter + " messages of size " + msglen + " bytes in " + secs + " seconds.");
		System.out.println("Sent " + req_counter + " requests of size " + reqlen + " bytes in " + secs + " seconds.");
		print_bw(secs, (int) seq_counter, bytes_sent, seq_counter);

	}

	private void print_bw(	double sec,
							int msgs,
							long bytes,
							long total_msgs) {
		double mps = 0;
		double bps = 0;
		double kscale = 1000;
		double mscale = 1000000;
		char mgscale = 'K';
		char bscale = 'K';
		long	Requests = 0;
		long	RequestsCanceled = 0;
		long	Responses = 0;

		if (sec == 0.0)
			return; // Avoid divide by 0

		mps = msgs / sec;
		bps = bytes * 8 / sec;
		if (mps <= mscale) {
			mgscale = 'K';
			mps /= kscale;
		} else {
			mgscale = 'M';
			mps /= mscale;
		}
		if (bps <= mscale) {
			bscale = 'K';
			bps /= kscale;
		} else {
			bscale = 'M';
			bps /= mscale;
		}
		for (int n = 0; n < srcs.length; n++) {
			if ( null != srcs[n] ) {
				Requests += srcs[ n ].getRequests();
				RequestsCanceled += srcs[ n ].getRequestsCanceled();
				Responses += srcs[ n ].getResponses();
			}
		}

		NumberFormat nf = NumberFormat.getInstance();
		nf.setMaximumFractionDigits(3);
		System.out.println(sec + " secs. " + nf.format(mps) + " " + mgscale
				+ "msgs/sec. " + nf.format(bps) + " " + bscale + "bps "
				+ total_msgs + " msgs -- REQ <" + Requests + "> Canceled <" + RequestsCanceled + "> RES <" + Responses + ">");
	}

	public static void main(String[] args) {
		new umdsrequest(args);
		System.out.println( "Exiting");
	}
}
