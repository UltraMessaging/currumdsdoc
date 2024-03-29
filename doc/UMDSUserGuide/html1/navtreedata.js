var NAVTREE =
[
  [ "UMDS User Guide", "index.html", [
    [ "Introduction", "index.html#firstsect", [
      [ "UMDS Overview", "index.html#umdsoverview", null ],
      [ "UMDS Architecture", "index.html#umdsarchitecture", null ]
    ] ],
    [ "UMDS Client", "index.html#umdsclient", [
      [ "UMDS API", "index.html#umdsapi", null ],
      [ "Server Connection", "index.html#serverconnection", [
        [ "UMDS Server List", "index.html#umdsserverlist", null ],
        [ "Connecting to Multiple Servers", "index.html#connectingtomultipleservers", null ],
        [ "Client Configuration Properties", "index.html#clientconfigurationproperties", null ],
        [ "Authenticating Applications and Users", "index.html#authenticatingapplicationsandusers", null ],
        [ "Assigning Different Client Settings to Your Application", "index.html#assigningdifferentclientsettingstoyourapplication", null ],
        [ "Application Name", "index.html#applicationname", null ]
      ] ],
      [ "Receiving", "index.html#receiving", null ],
      [ "Sending", "index.html#sending", null ],
      [ "Request and Response Capability", "index.html#requestandresponsecapability", null ],
      [ "Using UMDS Late Join", "index.html#usingumdslatejoin", [
        [ "UMDS Late Join Differences", "index.html#umdslatejoindifferences", null ],
        [ "Late Join UMDS Sources", "index.html#latejoinumdssources", null ]
      ] ],
      [ "Using UMDS Persistence", "index.html#usingumdspersistence", [
        [ "UMDS Persistence uses Session IDs", "index.html#umdspersistenceusessessionids", null ],
        [ "Configuring UMDS Server for Persistence", "index.html#configuringumdsserverforpersistence", null ],
        [ "Transient Receivers", "index.html#transientreceivers", null ],
        [ "Persistence and Server Failover", "index.html#persistenceandserverfailover", null ],
        [ "UMDS Persistence Differences", "index.html#umdspersistencedifferences", null ]
      ] ],
      [ "Using UMDS Client Encryption", "index.html#usingumdsclientencryption", [
        [ "UMDS TLS Authentication", "index.html#umdstlsauthentication", null ],
        [ "Configuring Encryption on Client", "index.html#configuringencryptiononclient", null ],
        [ "Configuring Encryption on Server", "index.html#configuringencryptiononserver", null ]
      ] ],
      [ "Client Compression", "index.html#clientcompression", null ],
      [ "Log Handling", "index.html#loghandling", [
        [ "Size-Based Log Rolling", "index.html#sizebasedlogrolling", null ],
        [ "Time-Based Log Rolling", "index.html#timebasedlogrolling", null ],
        [ "Combined Log Rolling", "index.html#combinedlogrolling", null ]
      ] ]
    ] ],
    [ "UMDS Example Client Applications", "index.html#umdsexampleclientapplications", [
      [ "Java Example Applications", "index.html#javaexampleapplications", [
        [ "umdsreceive.java", "index.html#umdsreceivejava", null ],
        [ "umdssend.java", "index.html#umdssendjava", null ],
        [ "umdsresponse.java", "index.html#umdsresponsejava", null ],
        [ "umdsrequest.java", "index.html#umdsrequestjava", null ],
        [ "umdspersistentreceive.java", "index.html#umdspersistentreceive", null ]
      ] ],
      [ ".NET Example Applications", "index.html#netexampleapplications", [
        [ "umdssend.cs", "index.html#umdssendcs", null ],
        [ "umdsreceive.cs", "index.html#umdsreceivecs", null ],
        [ "umdsresponse.cs", "index.html#umdsresponsecs", null ],
        [ "umdsrequest.cs", "index.html#umdsrequestcs", null ]
      ] ]
    ] ],
    [ "UMDS Server", "index.html#umdsserver", [
      [ "User Authentication", "index.html#userauthentication", null ],
      [ "Client Application Parameters", "index.html#clientapplicationparameters", null ],
      [ "Keep Alive Timers During Idle Periods", "index.html#keepalivetimersduringidleperiods", null ],
      [ "Message Queues", "index.html#messagequeue", [
        [ "Per-Topic Message Queues", "index.html#pertopicmessagequeues", null ],
        [ "Configuring Message Queue Size", "index.html#configuringmessagequeuesize", null ],
        [ "Approximating Per-Queue Memory Use", "index.html#approximatingperqueuememoryuse", null ],
        [ "Approximating the Number of Messages Per Queue", "index.html#approximatingthenumberofmessagesperqueue", null ],
        [ "Calculating Optimal Queue Size Limits", "index.html#calculatingoptimalqueuesizelimits", null ]
      ] ],
      [ "Worker Configuration Guidelines", "index.html#workerconfigurationguidelines", [
        [ "Increasing Number of UMDS Workers", "index.html#increasingnumberofumdsworkers", null ],
        [ "Workers CPU Cores and Performance", "index.html#workerscpucoresandperformance", null ],
        [ "Workers Versus Client Load", "index.html#workersversusclientload", null ]
      ] ]
    ] ],
    [ "Umdsd Man Page", "index.html#umdsdmanpage", null ],
    [ "Daemon Statistics", "index.html#daemonstatistics", [
      [ "Daemon Statistics Structures", "index.html#daemonstatisticsstructures", null ],
      [ "Daemon Statistics Binary Data", "index.html#daemonstatisticsbinarydata", null ],
      [ "Daemon Statistics Versioning", "index.html#daemonstatisticsversioning", null ],
      [ "Daemon Statistics Requests", "index.html#daemonstatisticsrequests", null ],
      [ "UMDS Daemon Statistics Structures", "index.html#umdsdaemonstatisticsstructures", null ],
      [ "UMDS Daemon Statistics Byte Swapping", "index.html#umdsdaemonstatisticsbyteswapping", null ],
      [ "UMDS Daemon Statistics String Buffers", "index.html#umdsdaemonstatisticsstringbuffers", null ],
      [ "UMDS Daemon Statistics Configuration", "index.html#umdsdaemonstatisticsconfiguration", null ],
      [ "UMDS Daemon Statistics Requests", "index.html#umdsdaemonstatisticsrequests", null ],
      [ "UMDS Daemon Statistics Example Files", "index.html#umdsdaemonstatisticsexamplefiles", null ]
    ] ],
    [ "UMDS Web Monitor", "index.html#umdswebmonitor", [
      [ "Main Menu", "index.html#mainmenu", null ],
      [ "List Current Connections", "index.html#listcurrentconnections", null ],
      [ "Client Details", "index.html#connectiondetails", null ],
      [ "Current Server Configuration File", "index.html#currentserverconfigurationfile", null ],
      [ "Dump Current Memory Allocation", "index.html#dumpcurrentmemoryallocation", null ],
      [ "Quit Server", "index.html#quitserver", null ]
    ] ],
    [ "UMDS Server Configuration", "index.html#umdsserverconfiguration", [
      [ "UMDS Server Configuration File", "index.html#umdsdconfigurationfile", [
        [ "UMDS Element \"<umds-daemon>\"", "index.html#umdsxmlumdsdaemon", null ],
        [ "UMDS Element \"<daemon>\"", "index.html#umdsxmldaemon", null ],
        [ "UMDS Element \"<tls>\"", "index.html#umdsxmltls", null ],
        [ "UMDS Element \"<cipher-suites>\"", "index.html#umdsxmlciphersuites", null ],
        [ "UMDS Element \"<trusted-certificates>\"", "index.html#umdsxmltrustedcertificates", null ],
        [ "UMDS Element \"<certificate-key-password>\"", "index.html#umdsxmlcertificatekeypassword", null ],
        [ "UMDS Element \"<certificate-key>\"", "index.html#umdsxmlcertificatekey", null ],
        [ "UMDS Element \"<certificate>\"", "index.html#umdsxmlcertificate", null ],
        [ "UMDS Element \"<topics>\"", "index.html#umdsxmltopics", null ],
        [ "UMDS Element \"<topic>\"", "index.html#umdsxmltopic", null ],
        [ "UMDS Element \"<umds-attributes>\"", "index.html#umdsxmlumdsattributes", null ],
        [ "UMDS Element \"<option>\"", "index.html#umdsxmloption", null ],
        [ "UMDS Element \"<monitor>\"", "index.html#umdsxmlmonitor", null ],
        [ "UMDS Element \"<application-id>\"", "index.html#umdsxmlapplicationid", null ],
        [ "UMDS Element \"<format>\"", "index.html#umdsxmlformat", null ],
        [ "UMDS Element \"<transport>\"", "index.html#umdsxmltransport", null ],
        [ "UMDS Element \"<daemon-monitor>\"", "index.html#umdsxmldaemonmonitor", null ],
        [ "UMDS Element \"<lbm-config>\"", "index.html#umdsxmllbmconfig", null ],
        [ "UMDS Element \"<remote-config-changes-request>\"", "index.html#umdsxmlremoteconfigchangesrequest", null ],
        [ "UMDS Element \"<remote-snapshot-request>\"", "index.html#umdsxmlremotesnapshotrequest", null ],
        [ "UMDS Element \"<publishing-interval>\"", "index.html#umdsxmlpublishinginterval", null ],
        [ "UMDS Element \"<group>\"", "index.html#umdsxmlgroup", null ],
        [ "UMDS Element \"<web-monitor>\"", "index.html#umdsxmlwebmonitor", null ],
        [ "UMDS Element \"<authentication>\"", "index.html#umdsxmlauthentication", null ],
        [ "UMDS Element \"<basic>\"", "index.html#umdsxmlbasic", null ],
        [ "UMDS Element \"<none>\"", "index.html#umdsxmlnone", null ],
        [ "UMDS Element \"<permissions>\"", "index.html#umdsxmlpermissions", null ],
        [ "UMDS Element \"<can-reqresp>\"", "index.html#umdsxmlcanreqresp", null ],
        [ "UMDS Element \"<can-stream>\"", "index.html#umdsxmlcanstream", null ],
        [ "UMDS Element \"<can-send>\"", "index.html#umdsxmlcansend", null ],
        [ "UMDS Element \"<client>\"", "index.html#umdsxmlclient", null ],
        [ "UMDS Element \"<compression>\"", "index.html#umdsxmlcompression", null ],
        [ "UMDS Element \"<server-reconnect>\"", "index.html#umdsxmlserverreconnect", null ],
        [ "UMDS Element \"<client-nodelay>\"", "index.html#umdsxmlclientnodelay", null ],
        [ "UMDS Element \"<client-sndbuf>\"", "index.html#umdsxmlclientsndbuf", null ],
        [ "UMDS Element \"<client-rcvbuf>\"", "index.html#umdsxmlclientrcvbuf", null ],
        [ "UMDS Element \"<server-nodelay>\"", "index.html#umdsxmlservernodelay", null ],
        [ "UMDS Element \"<server-sndbuf>\"", "index.html#umdsxmlserversndbuf", null ],
        [ "UMDS Element \"<server-rcvbuf>\"", "index.html#umdsxmlserverrcvbuf", null ],
        [ "UMDS Element \"<server-ka-threshold>\"", "index.html#umdsxmlserverkathreshold", null ],
        [ "UMDS Element \"<client-ka-interval>\"", "index.html#umdsxmlclientkainterval", null ],
        [ "UMDS Element \"<client-ka-threshold>\"", "index.html#umdsxmlclientkathreshold", null ],
        [ "UMDS Element \"<server-ka-interval>\"", "index.html#umdsxmlserverkainterval", null ],
        [ "UMDS Element \"<server-list>\"", "index.html#umdsxmlserverlist", null ],
        [ "UMDS Element \"<server>\"", "index.html#umdsxmlserver", null ],
        [ "UMDS Element \"<lbm-license-file>\"", "index.html#umdsxmllbmlicensefile", null ],
        [ "UMDS Element \"<pidfile>\"", "index.html#umdsxmlpidfile", null ],
        [ "UMDS Element \"<gid>\"", "index.html#umdsxmlgid", null ],
        [ "UMDS Element \"<uid>\"", "index.html#umdsxmluid", null ],
        [ "UMDS Element \"<log>\"", "index.html#umdsxmllog", null ],
        [ "UMDS Receiver Topic Options", "index.html#umdsreceivertopicoptions", null ],
        [ "UMDS Source Topic Options", "index.html#umdssourcetopicoptions", null ]
      ] ],
      [ "UM License File", "index.html#umlicensefile", null ],
      [ "UM Configuration File", "index.html#umconfigurationfile", null ],
      [ "Basic Authentication File", "index.html#basicauthenticationfile", [
        [ "UMDS application Element", "index.html#umdsxmlapplication", null ],
        [ "UMDS user Element", "index.html#umdsuserelement", null ]
      ] ],
      [ "UMDS Configuration DTD", "index.html#umdsconfigurationdtd", null ],
      [ "Example UMDS Configuration Files", "index.html#exampleumdsconfigurationfiles", [
        [ "Minimum Configuration File", "index.html#minimumconfigurationfile", null ],
        [ "Typical Configuration File", "index.html#typicalconfigurationfile", null ],
        [ "Complete Configuration File", "index.html#completeconfigurationfile", null ],
        [ "Sample UM Configuration File", "index.html#sampleumconfigurationfile", null ],
        [ "Sample Authentication File", "index.html#sampleauthenticationfile", null ]
      ] ]
    ] ],
    [ "UMDS Log Messages", "index.html#umdslogmessages", null ]
  ] ]
];

var NAVTREEINDEX =
[
"index.html"
];

var SYNCONMSG = 'click to disable panel synchronisation';
var SYNCOFFMSG = 'click to enable panel synchronisation';