import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.text.NumberFormat;
import java.util.*;

import com.latencybusters.umds.*;
import com.latencybusters.umds.UMDS.*;

// See http://kb.29west.com/index.php?View=entry&EntryID=8
import org.openmdx.uses.gnu.getopt.*;

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

class umdspersistentreceive extends UMDSServerConnection {

	private static int stat_secs = 0;
	private static long end_msgs = 0;

	private static final String appl_name = "umdspersistentreceive";
	private static final String purpose = "Purpose: Receive messages on a single topic.";
	private static final String usage = "Usage: umdspersistentreceive [options] -S address[:port] topic\n"
        		+ "  -S address[:port] = Server address/name and optionally port\n"
			+ "                    A comma separated list of multiple servers may be provided\n"
			+ "Available options:\n"
			+ "  -A Suppress sending the application name to the server on login\n "
			+ "  -c filename = read config file filename\n"
			+ "  -h = help\n"
			+ "  -n Set Starting seq number to Starting\n"
			+ "  -M num_msgs = End after num_msgs received\n"
			+ "  -N num_topics = Number of topics (receivers)\n"
			+ "  -s num_secs = print statistics every num_secs along with bandwidth\n"
			+ "  -S address:port = Server address and port\n"
			+ "  -U username = set the user name and prompt for password\n"
			+ "  -T tls = use encrypted communication\n"
			+ "  -t truststore = truststore file path\n"
			+ "  -p truststore-password = truststore password\n"
			+ "  -v = be verbose about each message\n"
			+ "  -W = Wildcard topic";

	int numtopics = 1;
	boolean verbose = false;
	String conffname = null;
	long stats_ms = 1000;
	int startSeqOffset = 0;
	String server_list = null;
	String serverip = "localhost";
	String serverport = "1234";
	String topic = null;
	String username = null;
	String password = null;
	boolean useTls = false;
	String trustStoreFile = null;
	String trustStorePassword = null;
	boolean wildcard = false;
	boolean auth_failed = false;
	int createCount = 0;
	int closeCount = 0;
	boolean help = false;
	boolean sendAppName = true;

	private void process_cmdline(String[] args) throws Exception {
		int c = -1;

		Getopt gopt = new Getopt(appl_name, args, "+c:n:M:N:P:L:S:s:U:t:p:TAhvW");
		while ((c = gopt.getopt()) != -1) {
			switch (c) {
				case 'A' :
					sendAppName = false;
					break;

			case 'c':
				conffname = gopt.getOptarg();
				break;

			case 'h':
				help = true;
				throw new Exception("Help:");

			case 'n':
				try {
					startSeqOffset = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid start Seq Offset:"
							+ gopt.getOptarg());
				}
				break;

			case 'M':
				try {
					end_msgs = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid number of messages:"
							+ gopt.getOptarg());
				}
				break;

			case 'N':
				try {
					numtopics = Integer.parseInt(gopt.getOptarg());
				} catch (Exception e) {
					throw new Exception("Invalid number of receivers:"
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

			case 'W':
				wildcard = true;
				break;

			default:
				throw new Exception(
						"Error occurred processing command line options");
			}
		}
		if(gopt.getOptind() >= args.length)
			throw new Exception("Missing topic");

		topic = args[gopt.getOptind()];
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
	 * Method to read a file of lines in "x = y" format
	 * containing configuration data, and setting the connections
	 * configuration
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
			if (listTokens.countTokens() == 0) return ret;
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
			getUMDS().log(LOG_LEVEL.INFO,"umdspersistentreceive Event: General loss occurred prior to sequence number " + msg.seqnum );
			break;
		case UMDSMessage.MSG_TYPE.DISCONNECT:
			getUMDS().log(LOG_LEVEL.INFO,"umdspersistentreceive Event: " + msg);
			break;
		case UMDSMessage.MSG_TYPE.LOGIN_DENIED:
			getUMDS().log(LOG_LEVEL.INFO,"umdspersistentreceive Event: " + msg);
			auth_failed = true;
			break;

		case UMDSMessage.MSG_TYPE.SERVER_STOPPED:
			getUMDS().log(LOG_LEVEL.INFO,"umdspersistentreceive Event: " + msg);
			auth_failed = true;
			break;

		case UMDSMessage.MSG_TYPE.SOURCE_CREATE :
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Source Create completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.RECEIVER_CREATE :
			createCount++;
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Receiver create completed ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.SOURCE_DELETE :
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Source delete Completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.RECEIVER_DELETE :
			closeCount++;
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Receiver delete Completed. ID " + msg.srcidx + " Status " + msg.status + "" + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.CONNECT :
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Client is (re)connected to server. Status " + msg.status + " " + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.LOGIN_COMPLETE :
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Client is Logged in to server. Status " + msg.status + " " + msg.status_str );
			break;

		case UMDSMessage.MSG_TYPE.LOGOUT_COMPLETE :
			getUMDS().log(LOG_LEVEL.INFO, "umdspersistentreceive Event: Client has Logged out of server. Status " + msg.status + " " + msg.status_str );
			break;

		default:
			getUMDS().log(LOG_LEVEL.INFO,"umdspersistentreceive Event: Unknown server connection event received " + msg);
			break;
		}
	}

	private umdspersistentreceive(String[] args) {
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
			if(!help)
				System.err.println(e);
			System.err.println(UMDS.version());
			System.err.println(purpose);
			System.err.println(usage);
			System.exit(1);
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
			if (useTls) {
				svrconn.setProperty("use-tls", "1");
			}
			if (trustStoreFile != null) {
				svrconn.setProperty("truststore", trustStoreFile);
			}
			if (trustStorePassword != null) {
				svrconn.setProperty("truststore-password", trustStorePassword);
			}
		} catch (UMDSException ex) {
			System.err.println("Error setting UMDS configuration: "
					+ ex.toString());
			System.exit(1);
		}

		/*
		 * Now start the connection to a server. 
		 * If server-reconnect is enabled it will return asynchronously.
		 * If server-reconnect is disabled, start() will return
		 * when the connection is authenticated
		 */
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
			try {
				svrconn.close();
			} catch (UMDSException ex) {
				System.err.println("Error closing connection: " + ex.toString());
			}
			return;
		}
		
		/* Create an array of receivers based on -N if it was used. */
		AppReceiver rcv[] = new AppReceiver[numtopics];

		Creates = numtopics;
		createCount = 0;
		try {
			for (int n = 0; n < numtopics; n++) {
				String fulltopic;

				/* Create a topic string. Append .N to the name if -N was used */
				if (numtopics == 1)
					fulltopic = topic;
				else
					fulltopic = topic + "." + n;

				/*
				 * Create the receiver object for this topic AppReceiver derives
				 * from UMDSReceiver which on creation registers with the server
				 * synchronously.
				 */
				if (startSeqOffset > 0) {
                    rcv[n] = new AppReceiver(svrconn, fulltopic, n, wildcard,
                            new UMDSRecoverySequenceNumberCallbackimpl(startSeqOffset), (Object) startSeqOffset);
                } else {
                    rcv[n] = new AppReceiver(svrconn, fulltopic, n, wildcard, null, null);
                }

				/* Copy the verbose flag from the command line */
				rcv[n].verbose = verbose;
			}
		} catch (UMDSException e) {
			System.out.println("Failed to create receiver object:" + e);
			System.exit(5);
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
		while (auth_failed == false) {
			/* Dump stats for each receiver object and reset the timing */
			for (int n = 0; n < numtopics; n++) {
				if(stats_ms > 0)
					rcv[n].print_bw();
				rcv[n].newInterval();
			}

			/* Check the server connection hasn't encountered any
			 * errors and exit if it has. This might come from
			 * a receiver too.
			 */
			if(svrconn.getError() != UMDS.ERRCODE.NO_ERROR) {
				System.err.println("Server Connection Error detected:" + svrconn.getErrorStr());
				break;
			}
			/* Wait for the statistics interval */
			try {
				Thread.sleep(stats_ms > 0 ? stats_ms : 1000);
			} catch (InterruptedException e) {
			}

			/* Count all the received messages on all the receivers */
			int total_msg_count = 0;
			for (int n = 0; n < numtopics; n++) {
				total_msg_count += rcv[n].total_msg_count;
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
		closeCount = 0;
		if (auth_failed == false) {
			try {
				for (int n = 0; n < numtopics; n++) {
					if ( rcv[n] != null ) {
						rcv[n].close();
						Closes++;
					}
				}

				while ( closeCount < Closes ) {
					if ( verbose ) {
						System.err.println("Receivers: " + Closes + "  Closed: " + closeCount );
					}
					try {
						Thread.sleep( 100 );
					} catch (InterruptedException e) {
					}
				}
				
				if ( verbose ) {
					System.err.println("All receivers closed: Receivers: " + Closes + "  Closed: " + closeCount );
				}

				System.err.println("Closing Connection " );
				svrconn.close();
			} catch (UMDSException ex) {
				System.err.println("Error closing receiver: " + ex.toString());
			}
		} else
			System.err.println("Exiting - no longer authenticated.");
	}

	public void print_summary(AppReceiver[] rcv) {
		double total_time_sec, mps, bps;
		long total_byte_count = 0;
		long created_time = 0;
		long data_end_time = 0;
		long total_msg_count = 0;

		for (int n = 0; n < numtopics; n++) {
			total_byte_count += rcv[n].total_byte_count;
			total_msg_count += rcv[n].total_msg_count;
			if (created_time == 0 || rcv[n].created_time < created_time)
				created_time = rcv[n].created_time;
			if (rcv[n].data_end_time > data_end_time)
				data_end_time = rcv[n].data_end_time;
		}

		total_time_sec = 0.0;
		mps = 0.0;
		bps = 0.0;

		long bits_received = total_byte_count * 8;
		long total_time = data_end_time - created_time;

		NumberFormat nf = NumberFormat.getInstance();
		nf.setMaximumFractionDigits(3);

		total_time_sec = total_time / 1000.0;

		if (total_time_sec > 0) {
			mps = total_msg_count / total_time_sec;
			bps = bits_received / total_time_sec;
		}

		System.out.println("\nTotal time         : "
				+ nf.format(total_time_sec) + "  sec");
		System.out.println("Messages received  : " + total_msg_count);
		System.out.println("Bytes received     : " + total_byte_count);
		System.out.println("Avg. throughput    : " + nf.format(mps / 1000.0)
				+ " Kmsgs/sec, " + nf.format(bps / 1000000.0) + " Mbps\n\n");

	}

	class UMDSRecoverySequenceNumberCallbackimpl implements UMDSReceiverRecoveryInfoCallback {
		private int _seqnum_offset = 0;

		public UMDSRecoverySequenceNumberCallbackimpl(int seqnum_offset) {
			_seqnum_offset = seqnum_offset;
		}

		@Override
		public int setReceiverRecoverySequenceNumber(Object cbArg, UMDSReceiverRecoveryInfo cbInfo) {
			int newStartSqn = cbInfo.getLowSequenceNumber() + _seqnum_offset;
			System.out.println("setReceiverRecoverySequenceNumber : " + cbInfo.toString()
					+ " newStartSeqNum : " + newStartSqn + " Topic : " + cbInfo.getTopic());

			cbInfo.setLowSequenceNumber(newStartSqn);
			return 0;
		}
	}
	/**
	 * The AppReceiver class decends from the UMDSReceiver. It implements the
	 * application callbacks for a receiver object.
	 *
	 * Applications should overload onMessage() and onEvent()
	 */
	public static class AppReceiver extends UMDSPersistentReceiver {

		/**
		 * The constructor for the application receiver class
		 *
		 * @param sconn
		 *            The server connection for this receiver object
		 * @param topic
		 *            The topic this receiver is listening to
		 * @param recvnum
		 *            The number of this receiver in umdsreceive
		 *            (this is not passed to the UMDS Client API)
		 * @throws UMDSException
		 *             (From UMDSReceiver constructor)
		 * @see UMDSReceiver
		 */
		public AppReceiver(UMDSServerConnection sconn, String topic, int recvnum, boolean wildcard,
                           UMDSReceiverRecoveryInfoCallback sqnNumCb, Object cbArg)
				throws UMDSException {
			super(sconn, topic, wildcard, sqnNumCb, cbArg);
			rcvnum = "" + recvnum;
		}


		// The string version of this receivers ID in umdspersistentreceive
		String rcvnum;

		/**
		 * The onMessage method is called by the UMDS client code. Application
		 * logic should be added within this method for processing each message.
		 *
		 * This message is called from the UMDS client thread processing the
		 * socket. Blocking or complex code in this function can prevent the
		 * socket from being processed. Ensure that application logic is kept to
		 * a minimum, and hands off the received message to an application
		 * thread.
		 */
		public void onMessage(UMDSMessage msg) {
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
			if (verbose) {
				System.out.println(rcvnum
						+ ": Yahoo! Received Data Message of Length "
						+ msg.length() + " Sequence Number " + msg.seqnum
						+ " source " + msg.srcidx
						+ " topic " + msg.topic
                        + " src_session_id " + msg.source_session_id
						+ (msg.recovered ? " -RX-" : "")
						+ " app meta data: " + msg.appmeta_data);

				// Find the first 0 byte and create a string with the text
				if (msg.appdata.length > 0) {
					int zero;
					for (zero = 0; zero < msg.appdata.length
							&& msg.appdata[zero] != 0; zero++)
						;
					String appstr = new String(msg.appdata, 0, zero);
					System.out.println(appstr);
				}
			}
			/*
			*/
		}

		/**
		 * The onEvent method is called by the UMDS client code. Application
		 * code should be added to handle various events. These events are
		 * typically errors.
		 */
		public void onEvent(UMDSMessage msg) {
			switch (msg.type) {
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

		/** Initial time */
		public long created_time = 0;

		/** Start MS of interval */
		public long data_start_time = 0;

		/** End MS of interval */
		public long data_end_time = 0;

		/** Number of messages received in interval */
		public int interval_msg_count = 0;

		/** Number of recovered messages received */
		public int total_rxmsg_count = 0;

		/** Absolute total number of messages */
		public long total_msg_count = 0;

		/** Number of bytes received in interval */
		public long interval_byte_count = 0;

		/** Absolute total number of bytes */
		public long total_byte_count = 0;

		/** Verbose command line flag */
		public boolean verbose = false;

		public void newInterval() {
			data_start_time = data_end_time;
			if (created_time == 0 || total_msg_count == 0)
				created_time = data_start_time;
			data_end_time = System.currentTimeMillis();
			total_msg_count += interval_msg_count;
			total_byte_count += interval_byte_count;
			interval_msg_count = 0;
			interval_byte_count = 0;
		}

		static NumberFormat nf = null;
		static NumberFormat nfsec = null;

		public void print_bw() {
			if (data_start_time == 0 || data_end_time == data_start_time)
				return;

			int msgs = interval_msg_count;
			long msec = data_end_time - data_start_time;
			long bytes = interval_byte_count;

			double sec;
			double mps = 0.0, bps = 0.0;
			double kscale = 1000.0, mscale = 1000000.0;
			char mgscale = 'K', bscale = 'K';

			sec = msec / 1000.;
			mps = ((double) msgs) / sec;
			bps = ((double) bytes * 8) / sec;
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
			if (nf == null) {
				nf = NumberFormat.getInstance();
				nf.setMaximumFractionDigits(3);
				nf.setMinimumFractionDigits(3);
				nf.setMinimumIntegerDigits(3);
			}
			if (nfsec == null) {
				nfsec = NumberFormat.getInstance();
				nfsec.setMinimumFractionDigits(3);
			}
			System.err.println(rcvnum + ":" + nfsec.format(sec) + " secs. "
					+ nf.format(mps) + " " + mgscale + "msgs/sec. "
					+ nf.format(bps) + " " + bscale + "bps" + " "
					+ msgs + " msgs " + (total_msg_count + msgs) + " total msgs "
					+ total_rxmsg_count + " rx msgs");
		}
	}

	public static void main(String[] args) {
		new umdspersistentreceive(args);
	}
}
