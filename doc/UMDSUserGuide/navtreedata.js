var NAVTREE =
[
  [ "UMDS User Guide", "index.html", [
    [ "Introduction", "index.html", [
      [ "UMDS Overview", "index.html#umdsoverview", null ],
      [ "UMDS Architecture", "index.html#umdsarchitecture", null ]
    ] ],
    [ "UMDS Client", "umdsclient.html", [
      [ "UMDS API", "umdsclient.html#umdsapi", null ],
      [ "Server Connection", "umdsclient.html#serverconnection", [
        [ "UMDS Server List", "umdsclient.html#umdsserverlist", null ],
        [ "Connecting to Multiple Servers", "umdsclient.html#connectingtomultipleservers", null ],
        [ "Client Configuration Properties", "umdsclient.html#clientconfigurationproperties", null ],
        [ "Authenticating Applications and Users", "umdsclient.html#authenticatingapplicationsandusers", null ],
        [ "Assigning Different Client Settings to Your Application", "umdsclient.html#assigningdifferentclientsettingstoyourapplication", null ],
        [ "Application Name", "umdsclient.html#applicationname", null ]
      ] ],
      [ "Receiving", "umdsclient.html#receiving", null ],
      [ "Sending", "umdsclient.html#sending", null ],
      [ "Request and Response Capability", "umdsclient.html#requestandresponsecapability", null ],
      [ "Using UMDS Late Join", "umdsclient.html#usingumdslatejoin", [
        [ "UMDS Late Join Differences", "umdsclient.html#umdslatejoindifferences", null ],
        [ "Late Join UMDS Sources", "umdsclient.html#latejoinumdssources", null ]
      ] ],
      [ "Using UMDS Persistence", "umdsclient.html#usingumdspersistence", [
        [ "UMDS Persistence uses Session IDs", "umdsclient.html#umdspersistenceusessessionids", null ],
        [ "Configuring UMDS Server for Persistence", "umdsclient.html#configuringumdsserverforpersistence", null ],
        [ "Transient Receivers", "umdsclient.html#transientreceivers", null ],
        [ "Persistence and Server Failover", "umdsclient.html#persistenceandserverfailover", null ],
        [ "UMDS Persistence Differences", "umdsclient.html#umdspersistencedifferences", null ]
      ] ],
      [ "Using UMDS Client Encryption", "umdsclient.html#usingumdsclientencryption", [
        [ "UMDS TLS Authentication", "umdsclient.html#umdstlsauthentication", null ],
        [ "Configuring Encryption on Client", "umdsclient.html#configuringencryptiononclient", null ],
        [ "Configuring Encryption on Server", "umdsclient.html#configuringencryptiononserver", null ]
      ] ],
      [ "Client Compression", "umdsclient.html#clientcompression", null ],
      [ "Log Handling", "umdsclient.html#loghandling", [
        [ "Size-Based Log Rolling", "umdsclient.html#sizebasedlogrolling", null ],
        [ "Time-Based Log Rolling", "umdsclient.html#timebasedlogrolling", null ],
        [ "Combined Log Rolling", "umdsclient.html#combinedlogrolling", null ]
      ] ]
    ] ],
    [ "UMDS Example Client Applications", "umdsexampleclientapplications.html", [
      [ "Java Example Applications", "umdsexampleclientapplications.html#javaexampleapplications", [
        [ "umdsreceive.java", "umdsexampleclientapplications.html#umdsreceivejava", null ],
        [ "umdssend.java", "umdsexampleclientapplications.html#umdssendjava", null ],
        [ "umdsresponse.java", "umdsexampleclientapplications.html#umdsresponsejava", null ],
        [ "umdsrequest.java", "umdsexampleclientapplications.html#umdsrequestjava", null ],
        [ "umdspersistentreceive.java", "umdsexampleclientapplications.html#umdspersistentreceive", null ]
      ] ],
      [ ".NET Example Applications", "umdsexampleclientapplications.html#netexampleapplications", [
        [ "umdssend.cs", "umdsexampleclientapplications.html#umdssendcs", null ],
        [ "umdsreceive.cs", "umdsexampleclientapplications.html#umdsreceivecs", null ],
        [ "umdsresponse.cs", "umdsexampleclientapplications.html#umdsresponsecs", null ],
        [ "umdsrequest.cs", "umdsexampleclientapplications.html#umdsrequestcs", null ]
      ] ]
    ] ],
    [ "UMDS Server", "umdsserver.html", [
      [ "User Authentication", "umdsserver.html#userauthentication", null ],
      [ "Client Application Parameters", "umdsserver.html#clientapplicationparameters", null ],
      [ "Keep Alive Timers During Idle Periods", "umdsserver.html#keepalivetimersduringidleperiods", null ],
      [ "Message Queues", "umdsserver.html#messagequeue", [
        [ "Per-Topic Message Queues", "umdsserver.html#pertopicmessagequeues", null ],
        [ "Configuring Message Queue Size", "umdsserver.html#configuringmessagequeuesize", null ],
        [ "Approximating Per-Queue Memory Use", "umdsserver.html#approximatingperqueuememoryuse", null ],
        [ "Approximating the Number of Messages Per Queue", "umdsserver.html#approximatingthenumberofmessagesperqueue", null ],
        [ "Calculating Optimal Queue Size Limits", "umdsserver.html#calculatingoptimalqueuesizelimits", null ]
      ] ],
      [ "Worker Configuration Guidelines", "umdsserver.html#workerconfigurationguidelines", [
        [ "Increasing Number of UMDS Workers", "umdsserver.html#increasingnumberofumdsworkers", null ],
        [ "Workers CPU Cores and Performance", "umdsserver.html#workerscpucoresandperformance", null ],
        [ "Workers Versus Client Load", "umdsserver.html#workersversusclientload", null ]
      ] ]
    ] ],
    [ "Umdsd Man Page", "umdsdmanpage.html", null ],
    [ "Daemon Statistics", "daemonstatistics.html", [
      [ "Daemon Statistics Structures", "daemonstatistics.html#daemonstatisticsstructures", null ],
      [ "Daemon Statistics Binary Data", "daemonstatistics.html#daemonstatisticsbinarydata", null ],
      [ "Daemon Statistics Versioning", "daemonstatistics.html#daemonstatisticsversioning", null ],
      [ "Daemon Statistics Requests", "daemonstatistics.html#daemonstatisticsrequests", null ],
      [ "UMDS Daemon Statistics Structures", "daemonstatistics.html#umdsdaemonstatisticsstructures", null ],
      [ "UMDS Daemon Statistics Byte Swapping", "daemonstatistics.html#umdsdaemonstatisticsbyteswapping", null ],
      [ "UMDS Daemon Statistics String Buffers", "daemonstatistics.html#umdsdaemonstatisticsstringbuffers", null ],
      [ "UMDS Daemon Statistics Configuration", "daemonstatistics.html#umdsdaemonstatisticsconfiguration", null ],
      [ "UMDS Daemon Statistics Requests", "daemonstatistics.html#umdsdaemonstatisticsrequests", null ],
      [ "UMDS Daemon Statistics Example Files", "daemonstatistics.html#umdsdaemonstatisticsexamplefiles", null ]
    ] ],
    [ "UMDS Web Monitor", "umdswebmonitor.html", [
      [ "Main Menu", "umdswebmonitor.html#mainmenu", null ],
      [ "List Current Connections", "umdswebmonitor.html#listcurrentconnections", null ],
      [ "Client Details", "umdswebmonitor.html#connectiondetails", null ],
      [ "Current Server Configuration File", "umdswebmonitor.html#currentserverconfigurationfile", null ],
      [ "Dump Current Memory Allocation", "umdswebmonitor.html#dumpcurrentmemoryallocation", null ],
      [ "Quit Server", "umdswebmonitor.html#quitserver", null ]
    ] ],
    [ "UMDS Server Configuration", "umdsserverconfiguration.html", [
      [ "UMDS Server Configuration File", "umdsserverconfiguration.html#umdsdconfigurationfile", [
        [ "UMDS Element \"<umds-daemon>\"", "umdsserverconfiguration.html#umdsxmlumdsdaemon", null ],
        [ "UMDS Element \"<daemon>\"", "umdsserverconfiguration.html#umdsxmldaemon", null ],
        [ "UMDS Element \"<tls>\"", "umdsserverconfiguration.html#umdsxmltls", null ],
        [ "UMDS Element \"<cipher-suites>\"", "umdsserverconfiguration.html#umdsxmlciphersuites", null ],
        [ "UMDS Element \"<trusted-certificates>\"", "umdsserverconfiguration.html#umdsxmltrustedcertificates", null ],
        [ "UMDS Element \"<certificate-key-password>\"", "umdsserverconfiguration.html#umdsxmlcertificatekeypassword", null ],
        [ "UMDS Element \"<certificate-key>\"", "umdsserverconfiguration.html#umdsxmlcertificatekey", null ],
        [ "UMDS Element \"<certificate>\"", "umdsserverconfiguration.html#umdsxmlcertificate", null ],
        [ "UMDS Element \"<topics>\"", "umdsserverconfiguration.html#umdsxmltopics", null ],
        [ "UMDS Element \"<topic>\"", "umdsserverconfiguration.html#umdsxmltopic", null ],
        [ "UMDS Element \"<umds-attributes>\"", "umdsserverconfiguration.html#umdsxmlumdsattributes", null ],
        [ "UMDS Element \"<option>\"", "umdsserverconfiguration.html#umdsxmloption", null ],
        [ "UMDS Element \"<monitor>\"", "umdsserverconfiguration.html#umdsxmlmonitor", null ],
        [ "UMDS Element \"<application-id>\"", "umdsserverconfiguration.html#umdsxmlapplicationid", null ],
        [ "UMDS Element \"<format>\"", "umdsserverconfiguration.html#umdsxmlformat", null ],
        [ "UMDS Element \"<transport>\"", "umdsserverconfiguration.html#umdsxmltransport", null ],
        [ "UMDS Element \"<daemon-monitor>\"", "umdsserverconfiguration.html#umdsxmldaemonmonitor", null ],
        [ "UMDS Element \"<lbm-config>\"", "umdsserverconfiguration.html#umdsxmllbmconfig", null ],
        [ "UMDS Element \"<remote-config-changes-request>\"", "umdsserverconfiguration.html#umdsxmlremoteconfigchangesrequest", null ],
        [ "UMDS Element \"<remote-snapshot-request>\"", "umdsserverconfiguration.html#umdsxmlremotesnapshotrequest", null ],
        [ "UMDS Element \"<publishing-interval>\"", "umdsserverconfiguration.html#umdsxmlpublishinginterval", null ],
        [ "UMDS Element \"<group>\"", "umdsserverconfiguration.html#umdsxmlgroup", null ],
        [ "UMDS Element \"<web-monitor>\"", "umdsserverconfiguration.html#umdsxmlwebmonitor", null ],
        [ "UMDS Element \"<authentication>\"", "umdsserverconfiguration.html#umdsxmlauthentication", null ],
        [ "UMDS Element \"<basic>\"", "umdsserverconfiguration.html#umdsxmlbasic", null ],
        [ "UMDS Element \"<none>\"", "umdsserverconfiguration.html#umdsxmlnone", null ],
        [ "UMDS Element \"<permissions>\"", "umdsserverconfiguration.html#umdsxmlpermissions", null ],
        [ "UMDS Element \"<can-reqresp>\"", "umdsserverconfiguration.html#umdsxmlcanreqresp", null ],
        [ "UMDS Element \"<can-stream>\"", "umdsserverconfiguration.html#umdsxmlcanstream", null ],
        [ "UMDS Element \"<can-send>\"", "umdsserverconfiguration.html#umdsxmlcansend", null ],
        [ "UMDS Element \"<client>\"", "umdsserverconfiguration.html#umdsxmlclient", null ],
        [ "UMDS Element \"<compression>\"", "umdsserverconfiguration.html#umdsxmlcompression", null ],
        [ "UMDS Element \"<server-reconnect>\"", "umdsserverconfiguration.html#umdsxmlserverreconnect", null ],
        [ "UMDS Element \"<client-nodelay>\"", "umdsserverconfiguration.html#umdsxmlclientnodelay", null ],
        [ "UMDS Element \"<client-sndbuf>\"", "umdsserverconfiguration.html#umdsxmlclientsndbuf", null ],
        [ "UMDS Element \"<client-rcvbuf>\"", "umdsserverconfiguration.html#umdsxmlclientrcvbuf", null ],
        [ "UMDS Element \"<server-nodelay>\"", "umdsserverconfiguration.html#umdsxmlservernodelay", null ],
        [ "UMDS Element \"<server-sndbuf>\"", "umdsserverconfiguration.html#umdsxmlserversndbuf", null ],
        [ "UMDS Element \"<server-rcvbuf>\"", "umdsserverconfiguration.html#umdsxmlserverrcvbuf", null ],
        [ "UMDS Element \"<server-ka-threshold>\"", "umdsserverconfiguration.html#umdsxmlserverkathreshold", null ],
        [ "UMDS Element \"<client-ka-interval>\"", "umdsserverconfiguration.html#umdsxmlclientkainterval", null ],
        [ "UMDS Element \"<client-ka-threshold>\"", "umdsserverconfiguration.html#umdsxmlclientkathreshold", null ],
        [ "UMDS Element \"<server-ka-interval>\"", "umdsserverconfiguration.html#umdsxmlserverkainterval", null ],
        [ "UMDS Element \"<server-list>\"", "umdsserverconfiguration.html#umdsxmlserverlist", null ],
        [ "UMDS Element \"<server>\"", "umdsserverconfiguration.html#umdsxmlserver", null ],
        [ "UMDS Element \"<lbm-license-file>\"", "umdsserverconfiguration.html#umdsxmllbmlicensefile", null ],
        [ "UMDS Element \"<pidfile>\"", "umdsserverconfiguration.html#umdsxmlpidfile", null ],
        [ "UMDS Element \"<gid>\"", "umdsserverconfiguration.html#umdsxmlgid", null ],
        [ "UMDS Element \"<uid>\"", "umdsserverconfiguration.html#umdsxmluid", null ],
        [ "UMDS Element \"<log>\"", "umdsserverconfiguration.html#umdsxmllog", null ],
        [ "UMDS Receiver Topic Options", "umdsserverconfiguration.html#umdsreceivertopicoptions", null ],
        [ "UMDS Source Topic Options", "umdsserverconfiguration.html#umdssourcetopicoptions", null ]
      ] ],
      [ "UM License File", "umdsserverconfiguration.html#umlicensefile", null ],
      [ "UM Configuration File", "umdsserverconfiguration.html#umconfigurationfile", null ],
      [ "Basic Authentication File", "umdsserverconfiguration.html#basicauthenticationfile", [
        [ "UMDS application Element", "umdsserverconfiguration.html#umdsxmlapplication", null ],
        [ "UMDS user Element", "umdsserverconfiguration.html#umdsuserelement", null ]
      ] ],
      [ "UMDS Configuration DTD", "umdsserverconfiguration.html#umdsconfigurationdtd", null ],
      [ "Example UMDS Configuration Files", "umdsserverconfiguration.html#exampleumdsconfigurationfiles", [
        [ "Minimum Configuration File", "umdsserverconfiguration.html#minimumconfigurationfile", null ],
        [ "Typical Configuration File", "umdsserverconfiguration.html#typicalconfigurationfile", null ],
        [ "Complete Configuration File", "umdsserverconfiguration.html#completeconfigurationfile", null ],
        [ "Sample UM Configuration File", "umdsserverconfiguration.html#sampleumconfigurationfile", null ],
        [ "Sample Authentication File", "umdsserverconfiguration.html#sampleauthenticationfile", null ]
      ] ]
    ] ],
    [ "UMDS Log Messages", "umdslogmessages.html", null ]
  ] ]
];

var NAVTREEINDEX =
[
"daemonstatistics.html"
];

var SYNCONMSG = 'click to disable panel synchronisation';
var SYNCOFFMSG = 'click to enable panel synchronisation';