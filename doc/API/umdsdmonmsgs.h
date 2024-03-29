/** \file umdsdmonmsgs.h
	\brief Ultra Messaging Desktop Services message definitions
	for UMDS Server Daemon Statistics.
	For general information on Daemon Statistics, see \ref daemonstatistics.

  All of the documentation and software included in this and any
  other Informatica Inc. Ultra Messaging Releases
  Copyright (C) Informatica Inc. All rights reserved.
  
  Redistribution and use in source and binary forms, with or without
  modification, are permitted only as covered by the terms of a
  valid software license agreement with Informatica Inc.

  (C) Copyright 2004,2024 Informatica Inc. All Rights Reserved.

  THE SOFTWARE IS PROVIDED "AS IS" AND INFORMATICA DISCLAIMS ALL WARRANTIES 
  EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION, ANY IMPLIED WARRANTIES OF 
  NON-INFRINGEMENT, MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE.
  INFORMATICA DOES NOT WARRANT THAT USE OF THE SOFTWARE WILL BE UNINTERRUPTED
  OR ERROR-FREE.  INFORMATICA SHALL NOT, UNDER ANY CIRCUMSTANCES, BE 
  LIABLE TO LICENSEE FOR LOST PROFITS, CONSEQUENTIAL, INCIDENTAL, SPECIAL OR 
  INDIRECT DAMAGES ARISING OUT OF OR RELATED TO THIS AGREEMENT OR THE 
  TRANSACTIONS CONTEMPLATED HEREUNDER, EVEN IF INFORMATICA HAS BEEN APPRISED OF 
  THE LIKELIHOOD OF SUCH DAMAGES.
*/

/*! \brief For internal use only. */
#define UMDS_DSTATTYPE_INVALID		0
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	UMDS Configuration message, of type:
*	\ref umdsd_dstat_config_msg_stct.
*/
#define UMDS_DSTATTYPE_CFG			1
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Memory Allocation Statistics message, of type:
*	\ref umdsd_dstat_mallinfo_msg_stct.
*/
#define UMDS_DSTATTYPE_MALLINFO		2
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Connection Summary message, of type:
*	\ref umdsd_dstat_connection_summary_msg_stct.
*/
#define UMDS_DSTATTYPE_CONNSUMMARY	3
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Client Permissions message, of type:
*	\ref umdsd_dstat_connection_permission_msg_stct.
*/
#define UMDS_DSTATTYPE_CLIENTPERMS	4
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Client Attributes message, of type:
*	\ref umdsd_dstat_connection_attribute_msg_stct.
*/
#define UMDS_DSTATTYPE_CLIENTATTRS	5
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Topic Data message, of type:
*	\ref umdsd_dstat_connection_pertopic_msg_stct.
*/
#define UMDS_DSTATTYPE_PERTOPIC		6
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Topic Total message, of type:
*	\ref umdsd_dstat_connection_totaltopic_msg_stct.
*/
#define UMDS_DSTATTYPE_TOPICTOTALS	7
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Source Information message, of type:
*	\ref umdsd_dstat_connection_source_msg_stct.
*/
#define UMDS_DSTATTYPE_SOURCE		8
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Receiver Information message, of type:
*	\ref umdsd_dstat_connection_receiver_msg_stct.
*/
#define UMDS_DSTATTYPE_RECEIVER		9
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Smartheap Information message, of type:
*	\ref umdsd_dstat_smartheap_msg_stct.
*/
#define UMDS_DSTATTYPE_SMARTHEAP	10
/*! \brief Value for umdsd_dstat_msg_hdr_t_stct::type for a
*	Worker Information message, of type:
*	\ref umdsd_dstat_worker_msg_stct.
*/
#define UMDS_DSTATTYPE_WORKER	    11


/*! \brief Value of umdsd_dstat_msg_hdr_t_stct::version indicating
*	the version of the monitoring daemon.  See \ref daemonstatisticsversioning
*	for general information on versioning of these structures.
*/
#define LBM_UMDSD_DMON_VERSION 0

/*! \brief Common message header structure included at the start of all
*	messages.
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_msg_hdr_t_stct {
	/*! \brief "Magic" value set by sender to indicate to the receiver
	*	whether byte swapping is needed.
	*	Possible values: \ref LBM_UMDS_DMON_MAGIC,
	*	\ref LBM_UMDS_DMON_ANTIMAGIC.
	*/
	lbm_uint16_t magic;
	/*! \brief Message type set by sender to indicate which kind of message
	*	this is.
	*	Possible values: one of the UMDSD_DSTATTYPE_* constants
	*	( \ref UMDS_DSTATTYPE_CFG,
	*	\ref UMDS_DSTATTYPE_MALLINFO, etc. )
	*/
	lbm_uint16_t type;
	/*! \brief Version of the message definition.  See
	*	\ref daemonstatisticsversioning for general information on
	*	versioning of these structures.
	*/
	lbm_uint16_t version;
	/*! \brief Total length of the message, including this header.
	*	Note that some message types do not have fixed lengths.
	*/
	lbm_uint16_t length;
	/*! \brief Approximate timestamp when the message was sent.
	*	Represents UTC wall clock time from the sending host's perspective.
	*	Value is "POSIX Time" (seconds since 1-Jan-1970, UTC).
	*/
	lbm_uint32_t tv_sec;
	/*! \brief Count of microseconds to be added to "tv_sec" to increase
	*	the precision of the timestamp.  However, the accuracy of the
	*	timestamp is not guaranteed to be at the microsecond level.
	*/
	lbm_uint32_t tv_usec;
	/*! \brief The worker number or index associated with this message.
	*
	*	To maintain consistency with the Web Monitor, this field is presented
	*	differently under different circumstances, depending on the next
	*	field, `"connId"`.
	*
	*	If `"connId"` is valid (not \ref UMDS_DSTAT_CONN_NA), then `"workerId"`
	*	is zero-based `0..num_workers-1`, to correspond with the web monitor's
	*	<i>worker index</i>, part of the web monitor's "x.y" connection ID
	*	(where "x" is the <i>worker index</i>, and "y" is the <i>connection
	*	index</i>).
	*
	*	If `"connId"` is not valid (set to \ref UMDS_DSTAT_CONN_NA), then
	*	`"workerId"` is one-based `1..num_workers`, corresponding to the web
	*	mon's <i>worker number</i>.
	*
	*	There are some statistics message (e.g. memory usage) which are not
	*	specific to any worker or connection, in which case '"workerId"' is
	*	set to \ref UMDS_DSTAT_WORKER_NA.
	*/
	lbm_uint32_t workerId;
	/*! \brief The client connection index associated with this message.
	*
	*	Set to \ref UMDS_DSTAT_CONN_NA when the message is not
	*	associated with a specific client connection.
	*
	*	Note that the connection index is zero-based `0..num_connections-1`.
	*	The index is by worker.  I.e. each worker's connections are numbered
	*	from 0 to the number of connections which that worker is handling.
	*/
	lbm_uint32_t connId;
} umdsd_dstat_msg_hdr_t;

/*! \brief Value of \ref umdsd_dstat_msg_hdr_t_stct::magic indicating that
*	the message does NOT need byte swapping.
*/
#define LBM_UMDS_DMON_MAGIC 0x4542
/*! \brief Value of \ref umdsd_dstat_msg_hdr_t_stct::magic indicating that
*	the message DOES need byte swapping.
*/
#define LBM_UMDS_DMON_ANTIMAGIC 0x4245


/***************************************************************************************************************************************************************/

/*! \brief Information about a UMDS server worker.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_WORKER )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_worker_record_stct {
	/*! \brief The worker number `1..num_workers` associated with
	*	this message. */
	lbm_uint32_t workerId;
	/*! \brief The number of active client connections being managed
	*	by this worker.
	*/
	lbm_uint32_t num_connections;
} umdsd_dstat_worker_record_t;

/*! \brief Message containing information about a UMDS server worker.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_WORKER )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_worker_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Information about a UMDS server worker.  */
	umdsd_dstat_worker_record_t record;
} umdsd_dstat_worker_msg_t;

/*! \brief value for \ref umdsd_dstat_msg_hdr_t_stct::workerId when the
*	message is not associated with a specific worker.
*/
#define UMDS_DSTAT_WORKER_NA -1
/*! \brief value for \ref umdsd_dstat_msg_hdr_t_stct::connId when the
*	message is not associated with a specific client connection.
*/
#define UMDS_DSTAT_CONN_NA -1

#if (LBM_UMDSD_DMON_VERSION == 0)
#define UMDS_DSTAT_CFG_EL_NAME_SZ	32			// = CFG_EL_NAME_SZ
#define UMDS_DSTAT_LBM_MAX_TOPIC_NAME_LEN 256	// = LBM_MAX_TOPIC_NAME_LEN
/*! \brief The attribute names are hardcoded in an array of strings.
*	No upper limit is defined on how large the attribute name can actually be.
*	However, should the name go beyond UMDS_DSTAT_MAX_ATTR_NAME_SZ,
*	the name will be truncated.
*/
#define	UMDS_DSTAT_MAX_ATTR_NAME_SZ 32
#define	UMDS_DSTAT_NUM_CLIENT_ATTRS 13			// = UMDS_NUM_CLIENT_ATTRS
#define	UMDS_DSTAT_NUM_CLIENT_PERMS 3			// = UMDS_NUM_CLIENT_PERMS
#define UMDS_DSTAT_DAEMON_INFO_STRLEN 256		// = DAEMON_INFO_STRLEN
#endif

/***************************************/

/*! \brief Connection Summary information.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CONNSUMMARY )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_summary_record_stct {
	/*! \brief Number of data messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			data_msgs_rcvd;
	/*! \brief Number of request messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			req_msgs_rcvd;			
	/*! \brief Number of response messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			resp_msgs_rcvd;		
	/*! \brief Number of control  messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			control_msgs_rcvd;
	/*! \brief Number of all types of messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			total_msgs_rcvd;
	/*! \brief Bytes of data messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			data_bytes_rcvd;		
	/*! \brief Bytes of request messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			req_bytes_rcvd;		
	/*! \brief Bytes of response messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			resp_bytes_rcvd;
	/*! \brief Bytes of control  messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			control_bytes_rcvd;	
	/*! \brief Bytes of all types of messages that the UMDS Server has received
	*	from the UMDS Client applications.
	*/
	lbm_uint64_t			total_bytes_rcvd;
	/*! \brief Number of data messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			data_msgs_sent;
	/*! \brief Number of request messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			req_msgs_sent;			
	/*! \brief Number of response messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			resp_msgs_sent;			
	/*! \brief Number of control  messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			control_msgs_sent;
	/*! \brief Number of all types of messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			total_msgs_sent;
	/*! \brief Bytes of data messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			data_bytes_sent;		
	/*! \brief Bytes of request messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			req_bytes_sent;		
	/*! \brief Bytes of response messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			resp_bytes_sent;
	/*! \brief Bytes of control  messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			control_bytes_sent;	
	/*! \brief Bytes of all types of messages that the UMDS Server has sent
	*	to the UMDS Client applications.
	*/
	lbm_uint64_t			total_bytes_sent;
	/*! \brief Messages dropped by the UMDS server because the message queue
	*	has reached the limit set by the `msg-age-limit` attribute of the
	*	\ref umdsxmlserver.
	*/
	lbm_uint64_t			user_msgs_tossed_for_age;
	/*! \brief Messages dropped by the UMDS server because the message queue
	*	has reached the limit set by the `msg-q-size-limit` attribute of the
	*	\ref umdsxmlserver.
	*/
	lbm_uint64_t			user_msgs_tossed_for_size;	
	/*! \brief Messages never enqueued on the client queue by the UMDS Server.
	*	Transport level loss can happen between the UMDS Server and external
	*	Ultra Messaging sources, or between sources and receivers internal
	*	to the UMDS Server.
	*/
	lbm_uint64_t			msgs_lost;
	/*! \brief This is the connection ID, and is the same as the header field
	*	\ref umdsd_dstat_msg_hdr_t_stct::connId.
	*	It is replicated here for convenience. */
	lbm_uint32_t			id;
	/*! \brief Name of the user logged in for this connection,
	*	as sent by the client. If the UMDS Client does not supply a user name,
	*	this item is blank. You specify authenticated users in the Basic
	*	Authentication File.
	*/
	char					user_name[ UMDS_DSTAT_CFG_EL_NAME_SZ + 1 ];
	/*! \brief Name of the client application connected to the server,
	*	as sent by the client. You can specify an application name in the
	*	Basic Authentication File or from within the application.
	*/
	char					appl_name[ UMDS_DSTAT_CFG_EL_NAME_SZ + 1 ];
	/*! \brief IP address of the host where the UMDS Client application is
	*	running.
	*/
	char					net_ip[ 20 ];

	/*! \brief Message bytes dropped by the UMDS server because the total of all
	 * message queues reached the limit set by the `total-q-size-limit` attribute
	 * of the \ref umdsxmlserver.
	 */
	lbm_uint64_t				user_bytes_tossed_for_total_size;
	/*! \brief Messages dropped by the UMDS server because the message queue
	*	has reached the limit set by the `msg-q-size-limit` attribute of the
	*	\ref umdsxmlserver.
	*/
	lbm_uint64_t				user_msgs_tossed_for_total_size;
} umdsd_dstat_connection_summary_record_t;

/*! \brief Message containing Connection Summary information.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CONNSUMMARY )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_summary_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Connection Summary information. */
	umdsd_dstat_connection_summary_record_t record;
} umdsd_dstat_connection_summary_msg_t;


/***************************************/

/*! \brief Topic-specific information for a connection.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_PERTOPIC )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_pertopic_record_stct {
	/*! \brief Number of data messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	data_msgs_ever_enq;
	/*! \brief Number of loss events that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	loss_msgs_ever_enq;
	/*! \brief Number of request messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	req_msgs_ever_enq;
	/*! \brief Number of response messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	resp_msgs_ever_enq;
	/*! \brief Number of control messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	control_msgs_ever_enq;
	/*! \brief Number of messages of all types that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	total_msgs_ever_enq;
	/*! \brief Bytes of data messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	data_bytes_ever_enq;
	/*! \brief Bytes of request messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	req_bytes_ever_enq;
	/*! \brief Bytes of response messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	resp_bytes_ever_enq;
	/*! \brief Bytes of control messages that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	control_bytes_ever_enq;
	/*! \brief Bytes of messages of all types that the UMDS server has enqueued
	* for this topic since the queue was created or reset.
	*/
	lbm_uint64_t	total_bytes_ever_enq;
	/*! \brief Number of data messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	data_msgs_cur_enq;
	/*! \brief Number of loss events that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	loss_msgs_cur_enq;
	/*! \brief Number of request messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	req_msgs_cur_enq;
	/*! \brief Number of response messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	resp_msgs_cur_enq;
	/*! \brief Number of control messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	control_msgs_cur_enq;
	/*! \brief Number of messages of all types that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	total_msgs_cur_enq;
	/*! \brief Bytes of data messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	data_bytes_cur_enq;
	/*! \brief Bytes of request messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	req_bytes_cur_enq;
	/*! \brief Bytes of response messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	resp_bytes_cur_enq;
	/*! \brief Bytes of control messages that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	control_bytes_cur_enq;
	/*! \brief Bytes of messages of all types that are currently enqueued
	* for this topic.
	*/
	lbm_uint64_t	total_bytes_cur_enq;
	/*! \brief Messages dropped by the UMDS server for this topic because
	*	the message queue has reached the limit set by the `msg-age-limit`
	*	attribute of the \ref umdsxmlserver.
	*/
	lbm_uint64_t	user_msgs_tossed_for_age;
	/*! \brief Messages dropped by the UMDS server for this topic because
	*	the message queue has reached the limit set by the `msg-q-size-limit`
	*	attribute of the \ref umdsxmlserver.
	*/
	lbm_uint64_t	user_msgs_tossed_for_size;
	/*! \brief Messages never enqueued on the client queue for this topic
	*	by the UMDS Server. Transport level loss can happen between the
	*	UMDS Server and external Ultra Messaging sources, or between
	*	sources and receivers internal to the UMDS Server.
	*/
	lbm_uint64_t	msgs_lost;
	/*! \brief The queue size limit for this message queue. */
	lbm_uint32_t	quota;
	/*! \brief For per-topic message queues, this is the topic name.
	*	`"Default"` is the non-topic-specific default message queue.
	*/
	char			topic_name[ UMDS_DSTAT_LBM_MAX_TOPIC_NAME_LEN ];
} umdsd_dstat_connection_pertopic_record_t;

/*! \brief Message containing topic-specific information for a connection.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_PERTOPIC )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_pertopic_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Topic-specific information. */
	umdsd_dstat_connection_pertopic_record_t record;
} umdsd_dstat_connection_pertopic_msg_t;


/***************************************/


/*! \brief Totals across all topics for a connection.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_TOPICTOTALS )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_totaltopic_record_stct {
	/*! \brief Number of data messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_data_msgs_ever_enq;
	/*! \brief Number of loss events that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_loss_msgs_ever_enq;
	/*! \brief Number of request messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_req_msgs_ever_enq;
	/*! \brief Number of response messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_resp_msgs_ever_enq;
	/*! \brief Number of control messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_control_msgs_ever_enq;
	/*! \brief Number of messages of all types that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            total_msgs_ever_enq;
	/*! \brief Bytes of data messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_data_bytes_ever_enq;
	/*! \brief Bytes of request messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_req_bytes_ever_enq;
	/*! \brief Bytes of response messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_resp_bytes_ever_enq;
	/*! \brief Bytes of control messages that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            tot_control_bytes_ever_enq;
	/*! \brief Bytes of messages of all types that the UMDS server has enqueued
	* across all topics since the queue was created or reset.
	*/
	lbm_uint64_t            total_bytes_ever_enq;
	/*! \brief Number of data messages that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            tot_data_msgs_cur_enq;
	/*! \brief Number of loss events that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            tot_loss_msgs_cur_enq;
	/*! \brief Number of request messages that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            tot_req_msgs_cur_enq;
	/*! \brief Number of response messages that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            tot_resp_msgs_cur_enq;
	/*! \brief Number of control messages that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            tot_control_msgs_cur_enq;
	/*! \brief Number of messages of all types that are currently enqueued
	* across all topisc.
	*/
	lbm_uint64_t            total_msgs_cur_enq;
	/*! \brief Messages dropped by the UMDS server across all topics because
	*	the message queue has reached the limit set by the `msg-age-limit`
	*	attribute of the \ref umdsxmlserver.
	*/
	lbm_uint64_t            total_user_msgs_tossed_for_age;
	/*! \brief Messages dropped by the UMDS server across all topics because
	*	the message queue has reached the limit set by the `msg-q-size-limit`
	*	attribute of the \ref umdsxmlserver.
	*/
	lbm_uint64_t            total_user_msgs_tossed_for_size;
	/*! \brief Messages never enqueued on the client queue across all topics.
	*	Transport level loss can happen between the
	*	UMDS Server and external Ultra Messaging sources, or between
	*	sources and receivers internal to the UMDS Server.
	*/
	lbm_uint64_t            total_msgs_lost;
} umdsd_dstat_connection_totaltopic_record_t;

/*! \brief Message containing totals across all topics for a connection.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_TOPICTOTALS )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_totaltopic_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Totals related to topics. */
	umdsd_dstat_connection_totaltopic_record_t record;
} umdsd_dstat_connection_totaltopic_msg_t;

/***************************************/

/*! \brief Information about a client's Receivers.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_RECEIVER )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_receiver_record_stct {
	/*! \brief Topic index for receiver. */
	lbm_uint32_t			topicQ_id;
	/*! \brief Topic string. */
	char					topic[ UMDS_DSTAT_LBM_MAX_TOPIC_NAME_LEN ];
} umdsd_dstat_connection_receiver_record_t;

/*! \brief Message containing information about a client's Receivers.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_RECEIVER )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_receiver_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Information about Receivers. */
	umdsd_dstat_connection_receiver_record_t record;
} umdsd_dstat_connection_receiver_msg_t;

/***************************************/

/*! \brief Information about a client's Sources.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_SOURCE )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_source_record_stct {	
	/*! \brief Topic index for source. */
	lbm_uint32_t			topicQ_id;
	/*! \brief Topic string. */
	char					topic[ UMDS_DSTAT_LBM_MAX_TOPIC_NAME_LEN ];
} umdsd_dstat_connection_source_record_t;

/*! \brief Message containing information about a client's Sources.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_SOURCE )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_source_msg_stct {	
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Information about Sources. */
	umdsd_dstat_connection_source_record_t record;
} umdsd_dstat_connection_source_msg_t;

/***************************************/


/*! \brief Client attributes.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CLIENTATTRS )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_attribute_record_stct {
	/*! \brief Value for this attribute. */
	lbm_uint64_t		val;
	/*! \brief Name of attriute set for this connection. */
	char				attribute_name[UMDS_DSTAT_MAX_ATTR_NAME_SZ];
} umdsd_dstat_connection_attribute_record_t;

/*! \brief Message containing client attributes.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CLIENTATTRS )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_connection_attribute_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Client attributes. */
	umdsd_dstat_connection_attribute_record_t attr[UMDS_DSTAT_NUM_CLIENT_ATTRS];
} umdsd_dstat_connection_attribute_msg_t;

/***************************************/

/*! \brief Client permission information.
*	<br>Note: the client permission feature is deprecated.
*	The messages are published for backwards compatibility.
*/
typedef struct umdsd_dstat_connection_permission_record_stct {
	/*! \brief Value for this permission. */
	lbm_uint64_t		val;
	/*! \brief Name of permission set for this connection. */
	char				permission_name[UMDS_DSTAT_DAEMON_INFO_STRLEN ];
} umdsd_dstat_connection_permission_record_t;

/*! \brief Message containing client permission information.
*	<br>Note: the client permission feature is deprecated.
*	The messages are published for backwards compatibility.
*/
typedef struct umdsd_dstat_connection_permission_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Client permission information. */
	umdsd_dstat_connection_permission_record_t perms[UMDS_DSTAT_NUM_CLIENT_PERMS];
} umdsd_dstat_connection_permission_msg_t;


/***************************************/
/*! \brief Memory allocation statistics.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_MALLINFO )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_mallinfo_record_stct {
	/*! \brief Non-mmapped space allocated (bytes). */
	lbm_uint32_t arena;
	/*! \brief Number of free chunks. */
	lbm_uint32_t ordblks;
	/*! \brief Number of mmapped regions. */
	lbm_uint32_t hblks;
	/*! \brief Space allocated in mmapped regions (bytes). */
	lbm_uint32_t hblkhd;
	/*! \brief Total allocated space (bytes). */
	lbm_uint32_t uordblks;
	/*! \brief Total free space (bytes). */
	lbm_uint32_t fordblks;
}umdsd_dstat_mallinfo_record_t;

/*! \brief Message containing memory allocation statistics.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_MALLINFO )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_mallinfo_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Memory allocation statistics. */
	umdsd_dstat_mallinfo_record_t record;
} umdsd_dstat_mallinfo_msg_t;


/***************************************/
/*! \brief Information about SmartHeap.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_SMARTHEAP )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_smartheap_record_stct {
	/*! \brief Active allocation count (bytes) as reported by SmartHeap's
	*	MemPoolCount() function.
	*/
	lbm_uint64_t poolcount;
	/*! \brief Small block size (bytes) as reported by SmartHeap's
	*	MemPoolInfo() function.
	*/
	lbm_uint64_t smallblocksize;
	/*! \brief Page size (bytes) as reported by SmartHeap's
	*	MemPoolInfo() function.
	*/
	lbm_uint64_t pagesize;
	/*! \brief Memory usage (bytes) as reported by SmartHeap's MemPoolSize()
	*	function.
	*/
	lbm_uint64_t poolsize;
	/*! \brief SmartHeap major version number. */
	lbm_uint32_t maj_ver;
	/*! \brief SmartHeap minor version number. */
	lbm_uint32_t min_ver;
	/*! \brief SmartHeap update version number. */
	lbm_uint32_t upd_ver;
}umdsd_dstat_smartheap_record_t;

/*! \brief Message containing information about SmartHeap.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_SMARTHEAP )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_smartheap_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief Information about SmartHeap. */
	umdsd_dstat_smartheap_record_t record;
} umdsd_dstat_smartheap_msg_t;


/***************************************/

/*! \brief UMDS Server configuration information.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CFG )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_config_record_stct {
	/*! \brief Variable-length string, NOT null-terminated, containing
	*	the configuration.
	*	(Use the message length to determine the length of the string data.)
	*/
	char data;
} umdsd_dstat_config_record_t;

/*! \brief Message containing UMDS Server configuation information.
*	<br>( \ref umdsd_dstat_msg_hdr_t_stct::type == \ref UMDS_DSTATTYPE_CFG )
*
*	Except where indicated, all fields of type `lbm_uintXX_t` should be
*	byte-swapped if `hdr.magic` is equal to \ref LBM_UMDS_DMON_ANTIMAGIC.
*/
typedef struct umdsd_dstat_config_msg_stct {
	/*! \brief Message header identifying the message type and other general
	*	information common for all messages.
	*/
	umdsd_dstat_msg_hdr_t hdr;
	/*! \brief UMDS Server configuration information. */
	umdsd_dstat_config_record_t record;
} umdsd_dstat_config_msg_t;

/***************************************/
