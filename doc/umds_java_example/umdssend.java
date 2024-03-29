import com.latencybusters.umds.*;
import com.latencybusters.umds.UMDS.*;

import java.io.*;
import java.util.*;
import java.text.NumberFormat;

// See https://communities.informatica.com/infakb/faq/5/Pages/80008.aspx
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

class umdssend extends UMDSServerConnection {
	private static final String appl_name = "umdssend";
	private static final String purpose = "Purpose: Send messages on a single topic.";
	private static final String usage = "Usage: umdssend [options] -S address[:port] topic\n"
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
			+ "  -s num_secs = Print statistics every num_secs\n"
			+ "  -U username = set the user name and prompt for password\n"
			+ "  -T tls = use encrypted communcation\n"
			+ "  -t truststore = truststore file path\n"
			+ "  -p truststore-password = truststore password\n"
			+ "  -v be verbose\n" ;

	int num_topics = 1;
	int msgs = 1000000;
	boolean verbose = false;
	String conffname = null;
	int msglen = 25;
	long stats_ms = 1000;
	int pause = 0;
	String server_list = null;
	String serverip = "localhost";
	String serverport = "1234";
	int linger = 0;
	String topic = null;
	String username = null;
	String password = null;
	boolean useTls = false;
	String trustStoreFile = null;
	String trustStorePassword = null;
	boolean auth_failed = false;
	boolean help = false;
	boolean connsend = false;
	int createCount = 0;
	int closeCount = 0;
	boolean loggedOut = false;
	boolean sendAppName = true;


	private void process_cmdline(String[] args) throws Exception {
		int c = -1;

		Getopt gopt = new Getopt(appl_name, args, "+c:AhIvl:M:N:P:L:S:s:U:t:p:T");
		while ((c = gopt.getopt()) != -1) {
			System.err.println("Parameter: " + (char)c + "\tArgument: " + gopt.getOptarg() );
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
						throw new Exception("Invalid message length:"
								+ gopt.getOptarg());
					}
					break;

				case 'L':
					try {
						linger = Integer.parseInt(gopt.getOptarg());
					} catch (Exception e) {
						throw new Exception("Invalid Linger time:"
								+ gopt.getOptarg());
					}
					break;

				case 'M':
					try {
						msgs = Integer.parseInt(gopt.getOptarg());
					} catch (Exception e) {
						throw new Exception("Invalid number of messages:"
								+ gopt.getOptarg());
					}
					break;

				case 'N':
					try {
						num_topics = Integer.parseInt(gopt.getOptarg());
					} catch (Exception e) {
						throw new Exception("Invalid number of topics:"
								+ gopt.getOptarg());
					}
					break;
				case 'P':
					try {
						pause = Integer.parseInt(gopt.getOptarg());
					} catch (Exception e) {
						throw new Exception("Invalid pause time:"
								+ gopt.getOptarg());
					}
					break;

				case 's':
					try {
						stats_ms = Long.parseLong(gopt.getOptarg()) * 1000;
					} catch (Exception e) {
						throw new Exception(
								"Invalid number of seconds for statistics:"
										+ gopt.getOptarg());
					}
					break;

				case 'S':
					server_list = gopt.getOptarg();
					break;

				case 'U':
					username = gopt.getOptarg();
					break;

				case 'T':
					useTls = true;
					break;

				case 't':
					trustStoreFile = gopt.getOptarg();
					break;

				case 'p':
					trustStorePassword = gopt.getOptarg();
					break;

				case 'v':
					verbose = true;
					break;

				default:
					throw new Exception(
							"Error occurred processing command line options");
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
			getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: General loss occurred prior to sequence number " + msg.seqnum);
			break;

		case UMDSMessage.MSG_TYPE.DISCONNECT:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: " + msg);
			break;

		case UMDSMessage.MSG_TYPE.LOGIN_DENIED:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: " + msg);
			auth_failed = true;
			break;

		case UMDSMessage.MSG_TYPE.SERVER_STOPPED:
			getUMDS().log(LOG_LEVEL.INFO, "UMDSsend Event: " + msg);
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

	private umdssend(String[] args) {
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
			if (!help)
				System.err.println(e);
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
				System.err.println("password: <" + password +">");
				svrconn.setProperty("password", password);
			}

			if (useTls) {
				svrconn.setProperty("use-tls", "1");
			}
			if (trustStoreFile != null) {
				svrconn.setProperty("truststore", trustStoreFile);
			}
			if (trustStorePassword != null) {
				svrconn.setProperty("truststore-password", trustStorePassword);
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
			System.err.println("Failed to authenticate with server.");
			System.exit(1);
		}

		/* Now the process of sending data begins... */
		System.out.print("Sending " + msgs + " messages of size " + msglen
				+ " bytes to topic [" + topic + "]");

		if (pause == 0) {
			System.out.println(" as fast as the system can");
		} else {
			System.out.println(" pausing " + pause + "ms");
		}

		/*
		 * Create a payload with the string "UMDS" and set every K to contain
		 * and offset for payload verification
		 */
		byte[] message = new byte[msglen];
		byte[] dummymsg = "UMDSSEND.JAVA-TEST-APP-TEST-MSG".getBytes();
		System.arraycopy(dummymsg, 0, message, 0,
				dummymsg.length < message.length ? dummymsg.length
						: message.length);

		for (int k = 1; (k * 1024) < message.length && k < 256; k++)
			message[k * 1024] = (byte) k;

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
		UMDSSource[] srcs = new UMDSSource[num_topics];
		Creates = num_topics;
		createCount = 0;
		try {
			if (num_topics > 1) {
				for (int n = 0; n < num_topics; n++) {
					topicnames[n] = topic + "." + n;
					if (connsend == false)
						srcs[n] = new UMDSSource(svrconn, topicnames[n], null);
					else
						srcs[n] = null;
				}
			} else {
				topicnames[0] = topic;
				if (connsend == false)
					srcs[0] = new UMDSSource(svrconn, topicnames[0], null);
				else
					srcs[0] = null;
			}
		} catch (UMDSException e) {
			System.err.println("Failed to create the source:" + e);
			System.exit(1);
		}
		
		if ( false ) {
		while( createCount < Creates ) {
			try {
				Thread.sleep( 20 );
				System.err.println("Sources Created: " + createCount );
			} catch (InterruptedException e) {
			}
		}
		System.err.println("Sources Created: " + createCount );
		}
		

		/* Data used in sending */
		long seq_counter = 0;
		long seq_end = seq_counter + msgs;
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
		while (seq_counter < seq_end && auth_failed == false) {
			if (stats_togo <= 0 || iter_time == 0) {
				end_time = System.currentTimeMillis();
				if ((last_time > 0) && (end_time - last_time >= stats_ms)) {
					/* New stats interval */
					double secs = (end_time - last_time) / 1000.;
					print_bw(secs, (int) last_msgs_sent, last_bytes_sent, seq_counter, seq_end );
					last_time = end_time;
					last_msgs_sent = 0;
					last_bytes_sent = 0;
				}
				stats_togo = stats_ms;
			}

			boolean sent = false;
			if ((pause_togo <= 0 || iter_time == 0)
					&& svrconn.isAuthenticated()) {
				pause_togo = pause;
				/* Send a message! */
				try {
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
						if (num_topics > 1) {
							topic_idx++;
							if (topic_idx == topicnames.length)
								topic_idx = 0;
						}
					}
				} catch (UMDSAuthenticationException ex) {
					/* This can occur while auto-reconnecting */
					try {
						// Give the connection time to reestablish
						System.out.println("Error sending message: auth error:" + ex.toString());
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
					System.out.println("Error sending message: disconnect:" + ex.toString());
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
		}

		// If we aren't connected, we can't tell the server to delete sources
		if ( true != auth_failed ) {
			try {
				System.err.println("Lingering for a second before closing source.");
				Thread.sleep( 5000 );
			} catch (InterruptedException e) {
			}
			Closes = 0;
			for (int n = 0; n < srcs.length; n++) {
				if ( null != srcs[n] )
				{
					try {
					srcs[ n ].close();
					srcs[ n ] = null;
					Closes++;
					}
					catch( UMDSException ex ) {
						System.err.println("Error closing source[" + n + "]: " + ex.toString() );
					}
				}
			}
			
			while ( closeCount < Closes ) {
				if ( verbose ) {
					System.err.println("Sources: " + Closes + "  Closed: " + closeCount );
				}
				try {
					Thread.sleep( 100 );
				} catch (InterruptedException e) {
				}
	
			}
			
			if ( verbose ) {
				System.err.println("All sources closed: Sources: " + Closes + "  Closed: " + closeCount );
			}
			/* Close the connection, we are done */
			try {
				System.err.println("Closing Connection " );
				svrconn.close();
			} catch (UMDSException ex) {
				System.err.println("Error closing server connection: " + ex.toString());
			}
			System.err.println( "Connection Closed" );
		}
		

		/* Finished sending, Dump the summary statistics */
		end_time = System.currentTimeMillis();
		double secs = (end_time - start_time) / 1000.;
		System.out.println("Sent " + seq_counter + " messages of size "
				+ msglen + " bytes in " + secs + " seconds.");
		print_bw(secs, (int) seq_counter, bytes_sent, seq_counter, seq_end );

		System.err.println("Done" );
	}

	private static void print_bw(double sec, int msgs, long bytes, long total_msgs, long end_seq ) {
		double mps = 0;
		double bps = 0;
		double kscale = 1000;
		double mscale = 1000000;
		char mgscale = 'K';
		char bscale = 'K';

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
		NumberFormat nf = NumberFormat.getInstance();
		nf.setMaximumFractionDigits(3);
		System.out.println(sec + " secs. " + nf.format(mps) + " " + mgscale
				+ "msgs/sec. " + nf.format(bps) + " " + bscale + "bps "
				+ total_msgs + " msgs ");
	}

	public static void main(String[] args) {
		new umdssend(args);
	}
}
