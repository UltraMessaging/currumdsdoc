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

#ifdef __VOS__
#define _POSIX_C_SOURCE 200112L
#include <sys/time.h>
#endif
#if defined(__TANDEM) && defined(HAVE_TANDEM_SPT)
#include <ktdmtyp.h>
#include <spthread.h>
#endif

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <time.h>
#ifdef _WIN32
#include <winsock2.h>
#include <sys/timeb.h>
#define strcasecmp stricmp
#else
#include <unistd.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <signal.h>
#include <sys/time.h>
#if defined(__TANDEM)
#include <strings.h>
#endif
#endif
#include "lbm/lbm.h"
#include "replgetopt.h"
#include "umdsdmonmsgs.h"

#if defined(_WIN32)
#   define SLEEP_SEC(x) Sleep(x*1000)
#   define SLEEP_MSEC(x) Sleep(x)
#else
#   define SLEEP_SEC(x) sleep(x)
#   define SLEEP_MSEC(x) \
		do{ \
			if ((x) >= 1000){ \
				sleep((x) / 1000); \
				usleep((x) % 1000 * 1000); \
						} \
						else{ \
				usleep((x)*1000); \
						} \
				}while (0)
#endif /* _WIN32 */


lbm_uint16_t byte_swap16(lbm_uint16_t n16) {
	lbm_uint16_t h16 = 0;
	int i = 0;
	for (; i<2; ++i) {
		h16 <<= 8;
		h16 |= (n16 & 0xff);
		n16 >>= 8;
	}
	return h16;
}

lbm_uint32_t byte_swap32(lbm_uint32_t n32) {
	lbm_uint32_t h32 = 0;
	int i = 0;
	for (; i<4; ++i) {
		h32 <<= 8;
		h32 |= (n32 & 0xff);
		n32 >>= 8;
	}
	return h32;
}

lbm_uint64_t byte_swap64(lbm_uint64_t n64) {
	lbm_uint64_t h64 = 0;
	int i = 0;
	for (; i<8; ++i) {
		h64 <<= 8;
		h64 |= (n64 & 0xff);
		n64 >>= 8;
	}
	return h64;
}
#define COND_SWAP16(_bs,_n) ((_bs)? byte_swap16(_n) : _n)
#define COND_SWAP32(_bs,_n) ((_bs)? byte_swap32(_n) : _n)
#define COND_SWAP64(_bs,_n) ((_bs)? byte_swap64(_n) : _n)

/* Lines starting with double quote are extracted for UM documentation. */

const char purpose[] = "Purpose: "
"application that receives UMDS daemon messages on the specified publishing topic."
;

const char usage[] =
"Usage: umdsddmon [-Ehv] [-c filename] publishing_topic\n"
"Available options:\n"
"  -c, --config=FILE    Use LBM configuration file FILE.\n"
"                       Multiple config files are allowed.\n"
"                       Example:  '-c file1.cfg -c file2.cfg'\n"
"  -E, --exit           exit when source stops sending\n"
"  -h, --help           display this help and exit\n"
"  -v, --verbose        be verbose about incoming messages (-v -v = be even more verbose)\n"
;

const char * OptionString = "Ac:Ehs:v";
const struct option OptionTable[] = {
	{ "config", required_argument, NULL, 'c' },
	{ "exit", no_argument, NULL, 'E' },
	{ "help", no_argument, NULL, 'h' },
	{ "verbose", no_argument, NULL, 'v' },
	{ NULL, 0, NULL, 0 }
};

struct Options {
	int end_on_end;  /* Flag to end program when source stops sending */
	int verbose;     /* Flag to control program verbosity */
	char *topic;     /* The topic on which to receive messages */
} options;

int close_recv = 0;
int opmode;							/* operational mode of LBM: sequential or embedded */
lbm_context_t *ctx;					/* ptr to context object */
int verbose = 0;

size_t format_timeval(struct timeval *tv, char *buf, size_t sz)
{
	size_t written = -1;
	time_t tt = tv->tv_sec;
	struct tm *gm = gmtime(&tt);
	if (gm) {
		written = (size_t)strftime(buf, sz, "%Y-%m-%d %H:%M:%S GMT", gm);
	}
	return written;
}

/*
* For the elapsed time, calculate and print the msgs/sec, bits/sec, and
* loss stats
*/
void print_bw(FILE *fp, struct timeval *tv, unsigned int msgs, unsigned int bytes, int unrec, lbm_ulong_t lost, int rx_msgs, int otr_msgs)
{
	char scale[] = { '\0', 'K', 'M', 'G' };
	int msg_scale_index = 0, bit_scale_index = 0;
	double sec = 0.0, mps = 0.0, bps = 0.0;
	double kscale = 1000.0;

	if (tv->tv_sec == 0 && tv->tv_usec == 0) return;/* avoid div by 0 */
	sec = (double)tv->tv_sec + (double)tv->tv_usec / 1000000.0;
	mps = (double)msgs / sec;
	bps = (double)bytes * 8 / sec;

	while (mps >= kscale) {
		mps /= kscale;
		msg_scale_index++;
	}

	while (bps >= kscale) {
		bps /= kscale;
		bit_scale_index++;
	}

	if ((rx_msgs != 0) || (otr_msgs != 0))
		fprintf(fp, "%-6.4g secs.  %-5.4g %cmsgs/sec.  %-5.4g %cbps [RX: %d][OTR: %d]",
		sec, mps, scale[msg_scale_index], bps, scale[bit_scale_index], rx_msgs, otr_msgs);
	else
		fprintf(fp, "%-6.4g secs.  %-5.4g %cmsgs/sec.  %-5.4g %cbps",
		sec, mps, scale[msg_scale_index], bps, scale[bit_scale_index]);
	fprintf(fp, "\n");
	fflush(fp);
}

/* Utility to print the contents of a buffer in hex/ASCII format */
void dump(const char *buffer, int size)
{
	int i, j;
	unsigned char c;
	char textver[20];

	for (i = 0; i<(size >> 4); i++) {
		for (j = 0; j<16; j++) {
			c = buffer[(i << 4) + j];
			printf("%02x ", c);
			textver[j] = ((c<0x20) || (c>0x7e)) ? '.' : c;
		}
		textver[j] = 0;
		printf("\t%s\n", textver);
	}
	for (i = 0; i<size % 16; i++) {
		c = buffer[size - size % 16 + i];
		printf("%02x ", c);
		textver[i] = ((c<0x20) || (c>0x7e)) ? '.' : c;
	}
	for (i = size % 16; i<16; i++) {
		printf("   ");
		textver[i] = ' ';
	}
	textver[i] = 0;
	printf("\t%s\n", textver);
}

/* Logging handler passed into lbm_log() */
int lbm_log_msg(int level, const char *message, void *clientd)
{
	printf("LOG Level %d: %s\n", level, message);
	return 0;
}

void umdsd_dmon_msg_handler(const char *buffer, int size)
{
	int msg_swap;				/* 1 means byte swap message */
	lbm_uint16_t msg_type;		/* swabbed message type */
	lbm_uint16_t msg_length;	/* swabbed message length */
	lbm_uint16_t msg_version;	/* swabbed message version */
	lbm_uint32_t msg_tv_sec;	/* swabbed message timeval seconds */
	lbm_uint32_t msg_tv_usec;	/* swabbed message timeval microseconds */
	lbm_uint32_t msg_workerId;	/* swabbed message workerID */
	lbm_uint32_t msg_connId;	/* swabbed message connection ID */
	char time_buff_sent[100];
	char time_buff_rcvd[100];
	struct timeval sent_tv;
	time_t now = time(0);

	umdsd_dstat_msg_hdr_t *msg_hdr;
	char *aligned_msg_buffer = malloc(size + 16);
	memset(aligned_msg_buffer, 0, (size + 16));
	memcpy(aligned_msg_buffer, buffer, size);
	msg_hdr = (umdsd_dstat_msg_hdr_t *)aligned_msg_buffer;

	strftime(time_buff_rcvd, 100, "%Y-%m-%d %H:%M:%S", localtime(&now));
	printf("\n%s Received ", time_buff_rcvd);
	if (size < sizeof(umdsd_dstat_msg_hdr_t)) {
		printf("undersized message: %d\n!", size);
		return;
	}
	if (msg_hdr->magic != LBM_UMDS_DMON_MAGIC && msg_hdr->magic != LBM_UMDS_DMON_ANTIMAGIC) {
		printf("message with bad magic: 0x%x\n!", msg_hdr->magic);
		return;
	}
	msg_swap = (msg_hdr->magic != LBM_UMDS_DMON_MAGIC);
	msg_type = COND_SWAP16(msg_swap, msg_hdr->type);
	msg_length = COND_SWAP16(msg_swap, msg_hdr->length);
	msg_version = COND_SWAP16(msg_swap, msg_hdr->version);
	msg_tv_sec = COND_SWAP32(msg_swap, msg_hdr->tv_sec);
	msg_tv_usec = COND_SWAP32(msg_swap, msg_hdr->tv_usec);
	msg_workerId = COND_SWAP32(msg_swap, msg_hdr->workerId);
	msg_connId = COND_SWAP32(msg_swap, msg_hdr->connId);
	sent_tv.tv_sec = msg_tv_sec;
	sent_tv.tv_usec = msg_tv_usec;
	if (format_timeval(&sent_tv, time_buff_sent, sizeof(time_buff_sent)) <= 0) {
		strcpy(time_buff_sent, "unknown");
	}

	switch (msg_type) {
	case UMDS_DSTATTYPE_MALLINFO:
	{
		umdsd_dstat_mallinfo_msg_t * msg;
		msg = (umdsd_dstat_mallinfo_msg_t *) aligned_msg_buffer;
		umdsd_dstat_mallinfo_record_t * record;
		record = &msg->record;
		if (msg_length < sizeof(umdsd_dstat_mallinfo_msg_t)) {
			printf("undersized umdsd_DSTATTYPE_MALLINFO message: %d\n", size);
			return;
		}
		printf("\n==============Malloc Info (Version: %d)==============\n%s Sent\n", msg_version, time_buff_sent);
		printf("         arena: %d\n", COND_SWAP32(msg_swap, record->arena));
		printf("       ordblks: %d\n", COND_SWAP32(msg_swap, record->ordblks));
		printf("         hblks: %d\n", COND_SWAP32(msg_swap, record->hblks));
		printf("        hblkhd: %d\n", COND_SWAP32(msg_swap, record->hblkhd));
		printf("      uordblks: %d\n", COND_SWAP32(msg_swap, record->uordblks));
		printf("      fordblks: %d\n\n", COND_SWAP32(msg_swap, record->fordblks));
		break;
	}

	case UMDS_DSTATTYPE_SMARTHEAP:
	{
		umdsd_dstat_smartheap_msg_t *smartheap_msg;
		smartheap_msg = (umdsd_dstat_smartheap_msg_t *)aligned_msg_buffer;
		umdsd_dstat_smartheap_record_t * record;
		record = &smartheap_msg->record;
		if (msg_length < sizeof(umdsd_dstat_smartheap_msg_t)) {
			printf("undersized umdsd_DSTATTYPE_SMARTHEAP message: %d\n", size);
			return;
		}
		printf("\n==============Smartheap Info (Version: %d)==============\n%s Sent\n", msg_version, time_buff_sent);
		printf("       version: %d.%d.%d\n", COND_SWAP32(msg_swap,record->maj_ver), COND_SWAP32(msg_swap,record->min_ver), COND_SWAP32(msg_swap,record->upd_ver));
		printf("      poolsize: %"PRIu64"\n", COND_SWAP64(msg_swap, record->poolsize));
		printf("     poolcount: %"PRIu64"\n", COND_SWAP64(msg_swap, record->poolcount));
		printf("smallblocksize: %"PRIu64"\n", COND_SWAP64(msg_swap, record->smallblocksize));
		printf("      pagesize: %"PRIu64"\n\n", COND_SWAP64(msg_swap, record->pagesize));
		break;
	}

	case UMDS_DSTATTYPE_CFG:
		{
			char * cfgstart;
			cfgstart = (char *) aligned_msg_buffer+sizeof(umdsd_dstat_msg_hdr_t);
			printf("\n======================UMDS CFG (Version: %d)======\n%s Sent\n", msg_version, time_buff_sent);
			printf("%s\n\n", cfgstart);
		}
		break;
	case UMDS_DSTATTYPE_CONNSUMMARY:
		{
			umdsd_dstat_connection_summary_msg_t * msg;
			msg = (umdsd_dstat_connection_summary_msg_t *) aligned_msg_buffer;
			umdsd_dstat_connection_summary_record_t *record;
			record = &msg->record;
			printf("\n=================Connection (%d.%d) Summary Stats(Version: %d)=================\n%s Sent\n", msg_workerId, msg_connId, msg_version, time_buff_sent);
			printf("                            ID:  %d\n",COND_SWAP32(msg_swap, record->id));
			printf("                     User Name:  %s\n",record->user_name);
			printf("              Application Name:  %s\n",record->appl_name);
			printf("                   Client Host:  %s\n",record->net_ip);
			printf("        Data messages received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_msgs_rcvd));
			printf("     Request messages received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_msgs_rcvd));
			printf("    Response messages received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_msgs_rcvd));
			printf("     Control messages received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_msgs_rcvd));
			printf("       Total messages received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_rcvd));
			printf("           Data bytes received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_bytes_rcvd));
			printf("        Request bytes received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_bytes_rcvd));
			printf("       Response bytes received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_bytes_rcvd));
			printf("        Control bytes received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_bytes_rcvd));
			printf("          Total bytes received:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_bytes_rcvd));
			printf("            Data messages sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_msgs_sent));
			printf("         Request messages sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_msgs_sent));
			printf("        Response messages sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_msgs_sent));
			printf("         Control messages sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_msgs_sent));
			printf("           Total messages sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_sent));
			printf("               Data bytes sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_bytes_sent));
			printf("            Request bytes sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_bytes_sent));
			printf("           Response bytes sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_bytes_sent));
			printf("            Control bytes sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_bytes_sent));
			printf("              Total bytes sent:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_bytes_sent));
			printf("  User messages tossed for age:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->user_msgs_tossed_for_age));
			printf(" User messages tossed for size:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->user_msgs_tossed_for_size));
			printf("                 Messages lost:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->msgs_lost));

		}
		break;
	case UMDS_DSTATTYPE_CLIENTPERMS:
		{
			umdsd_dstat_connection_permission_msg_t * msg;
			msg = (umdsd_dstat_connection_permission_msg_t *) aligned_msg_buffer;
			int numpads, i, j;
			printf("\n=================Client Permissions for Connection (%d.%d) (Version: %d)====================\n%s Sent\n", COND_SWAP32(msg_swap,msg->hdr.workerId), COND_SWAP32(msg_swap,msg->hdr.connId), msg_version,time_buff_sent);
			printf("               Permission Name : Permission Value\n");
			for(i = 0; i < UMDS_DSTAT_NUM_CLIENT_PERMS; i++){
				numpads = (strlen(msg->perms[i].permission_name) > 30? 0 : (30 - strlen(msg->perms[i].permission_name)));
				for(j = 0; j < numpads; j++) printf(" ");
				printf("%s : %"PRIu64"\n", msg->perms[i].permission_name, COND_SWAP64(msg_swap,msg->perms[i].val));
			}
		}
		break;
	case UMDS_DSTATTYPE_CLIENTATTRS:
		{
			umdsd_dstat_connection_attribute_msg_t * msg;
			msg = (umdsd_dstat_connection_attribute_msg_t *) aligned_msg_buffer;
			int numpads, i, j;
			printf("\n====================Client Attributes for Connection (%d.%d) (Version: %d)====================\n%s Sent\n", COND_SWAP32(msg_swap,msg->hdr.workerId), COND_SWAP32(msg_swap,msg->hdr.connId), msg_version,time_buff_sent);
			printf("                Attribute Name : Attribute Value\n");
			for(i = 0; i < UMDS_DSTAT_NUM_CLIENT_ATTRS; i++){
				numpads = (strlen(msg->attr[i].attribute_name) > 30? 0 : (30 - strlen(msg->attr[i].attribute_name)));
				for(j = 0; j < numpads; j++) printf(" ");
				printf("%s : %"PRIu64"\n", msg->attr[i].attribute_name, COND_SWAP64(msg_swap,msg->attr[i].val));
			}
		}
		break;
	case UMDS_DSTATTYPE_WORKER:
		{
			umdsd_dstat_worker_msg_t *msg;
			msg = (umdsd_dstat_worker_msg_t *) aligned_msg_buffer;
			umdsd_dstat_worker_record_t * record;
			record = &msg->record;
			printf("\n================Worker %d Summary (Version: %d)===============\n%s Sent\n", COND_SWAP32(msg_swap, record->workerId), msg_version, time_buff_sent);	
			printf("           Number of connections: %d\n", COND_SWAP32(msg_swap,record->num_connections));
		}
		break;
	
	case UMDS_DSTATTYPE_PERTOPIC:
		{

			umdsd_dstat_connection_pertopic_msg_t * msg;
			msg = (umdsd_dstat_connection_pertopic_msg_t *) aligned_msg_buffer;
			umdsd_dstat_connection_pertopic_record_t * record;
			char *topic_name;
			record = &msg->record;
			topic_name = (strlen(record->topic_name) == 0 ) ? "Default" : record->topic_name;
			printf("\n================Topic Data for Connection (%d.%d)  (Version: %d)===============\n%s Sent\n", COND_SWAP32(msg_swap,msg->hdr.workerId), COND_SWAP32(msg_swap,msg->hdr.connId),  msg_version, time_buff_sent);
			printf("                              Topic Name:  %s\n", topic_name);
			printf("                Topic Queue quota(bytes):  %d\n", COND_SWAP32(msg_swap, record->quota));
			printf("             Data messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_msgs_ever_enq));
			printf("             Loss messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->loss_msgs_ever_enq));
			printf("          Request messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_msgs_ever_enq));
			printf("         Response messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_msgs_ever_enq));
			printf("          Control messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_msgs_ever_enq));
			printf("            Total messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_ever_enq));
			printf("               Data bytes ever  in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_bytes_ever_enq));
			printf("             Request bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_bytes_ever_enq));
			printf("            Response bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_bytes_ever_enq));
			printf("             Control bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_bytes_ever_enq));
			printf("               Total bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_bytes_ever_enq));
			printf("        Data messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_msgs_cur_enq));
			printf("        Loss messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->loss_msgs_cur_enq));
			printf("     Request messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_msgs_cur_enq));
			printf("    Response messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_msgs_cur_enq));
			printf("     Control messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_msgs_cur_enq));
			printf("       Total messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_cur_enq));
			printf("           Data bytes currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->data_bytes_cur_enq));
			printf("        Request bytes currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->req_bytes_cur_enq));
			printf("       Response bytes currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->resp_bytes_cur_enq));
			printf("        Control bytes currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->control_bytes_cur_enq));
			printf("          Total bytes currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_bytes_cur_enq));
			printf("            User messages tossed for age:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->user_msgs_tossed_for_age));
			printf("           User messages tossed for size:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->user_msgs_tossed_for_size));
			printf("                           Messages lost:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->msgs_lost));
		}
		break;	
	case UMDS_DSTATTYPE_TOPICTOTALS:
		{

			umdsd_dstat_connection_totaltopic_msg_t * msg;
			msg = (umdsd_dstat_connection_totaltopic_msg_t *) aligned_msg_buffer;
			umdsd_dstat_connection_totaltopic_record_t * record;
			record = &msg->record;
			printf("\n================Data for All Topics on Connection(%d.%d) (Version: %d)===============\n%s Sent\n", COND_SWAP32(msg_swap,msg->hdr.workerId), COND_SWAP32(msg_swap,msg->hdr.connId),  msg_version, time_buff_sent);
			printf("             Data messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_data_msgs_ever_enq));
			printf("             Loss messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_loss_msgs_ever_enq));
			printf("          Request messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_req_msgs_ever_enq));
			printf("         Response messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_resp_msgs_ever_enq));
			printf("          Control messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_control_msgs_ever_enq));
			printf("            Total messages ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_ever_enq));
			printf("                Data bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_data_bytes_ever_enq));
			printf("             Request bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_req_bytes_ever_enq));
			printf("            Response bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_resp_bytes_ever_enq));
			printf("             Control bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_control_bytes_ever_enq));
			printf("               Total bytes ever in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_bytes_ever_enq));
			printf("        Data messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_data_msgs_cur_enq));
			printf("        Loss messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_loss_msgs_cur_enq));
			printf("     Request messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_req_msgs_cur_enq));
			printf("    Response messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_resp_msgs_cur_enq));
			printf("     Control messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->tot_control_msgs_cur_enq));
			printf("       Total messages currently in queue:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_cur_enq));
			printf("            User messages tossed for age:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_user_msgs_tossed_for_age));
			printf("           User messages tossed for size:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_user_msgs_tossed_for_size));
			printf("                           Messages lost:  %"PRIu64"\n",COND_SWAP64(msg_swap, record->total_msgs_lost));
		}
		break;
	case UMDS_DSTATTYPE_SOURCE:
		{
			umdsd_dstat_connection_source_msg_t * msg;
			msg = (umdsd_dstat_connection_source_msg_t *) aligned_msg_buffer;
			umdsd_dstat_connection_source_record_t * record;
			record = &msg->record;
			printf("\n================Source Info for Connection (%d.%d) (Version: %d)===============\n%s Sent\n", COND_SWAP32(msg_swap, msg->hdr.workerId), COND_SWAP32(msg_swap, msg->hdr.connId),  msg_version, time_buff_sent);
			printf("                              Topic Name: %s\n", record->topic);
			printf("                          Topic Queue ID: %d\n", COND_SWAP32(msg_swap, record->topicQ_id));
		}
		break;
	case UMDS_DSTATTYPE_RECEIVER:
		{
			umdsd_dstat_connection_receiver_msg_t * msg;
			msg = (umdsd_dstat_connection_receiver_msg_t *) aligned_msg_buffer;
			umdsd_dstat_connection_receiver_record_t * record;
			record = &msg->record;
			printf("\n================Receiver Info for Connection (%d.%d) (Version: %d)===============\n%s Sent\n", COND_SWAP32(msg_swap, msg->hdr.workerId), COND_SWAP32(msg_swap, msg->hdr.connId),  msg_version, time_buff_sent);
			printf("                              Topic Name: %s\n", record->topic);
			printf("                          Topic Queue ID: %d\n", COND_SWAP32(msg_swap, record->topicQ_id));
		}
		break;
	default:
		printf("unknown message type 0x%x\n", msg_hdr->type);
		return;
	}
	free(aligned_msg_buffer);
}

/* Received message handler (passed into lbm_rcv_create()) */
int rcv_handle_msg(lbm_rcv_t *rcv, lbm_msg_t *msg, void *clientd)
{
	struct Options *opts = &options;

	if (close_recv)
		return 0; /* skip any new messages if we're just waiting to exit */

	switch (msg->type) {
	case LBM_MSG_DATA:
		umdsd_dmon_msg_handler(msg->data, msg->len);
		if (opts->verbose)
		{
			printf("[@%ld.%06ld]", (long int)msg->tsp.tv_sec, (long int)msg->tsp.tv_usec);
			printf("[%s][%s][%u]%s%s%s%s, %lu bytes\n",
				msg->topic_name, msg->source, msg->sequence_number,
				((msg->flags & LBM_MSG_FLAG_RETRANSMIT) ? "-RX-" : ""),
				((msg->flags & LBM_MSG_FLAG_HF_DUPLICATE) ? "-HFDUP-" : ""),
				((msg->flags & LBM_MSG_FLAG_HF_PASS_THROUGH) ? "-PASS-" : ""),
				((msg->flags & LBM_MSG_FLAG_OTR) ? "-OTR-" : ""),
				(unsigned long)msg->len);

			if (opts->verbose > 1)
				dump(msg->data, msg->len);
		}
		break;
	case LBM_MSG_UNRECOVERABLE_LOSS:
		if (opts->verbose) {
			printf("[%s][%s][%u], LOST\n",
				msg->topic_name, msg->source, msg->sequence_number);
		}
		break;
	case LBM_MSG_UNRECOVERABLE_LOSS_BURST:
		if (opts->verbose) {
			printf("[%s][%s][%u], LOSS BURST\n",
				msg->topic_name, msg->source, msg->sequence_number);
		}
		break;
	case LBM_MSG_REQUEST:
		/* Request message received (no response processed here) */
		if (opts->verbose) {
			printf("[%s][%s][%u], Request\n",
				msg->topic_name, msg->source, msg->sequence_number);
		}
		break;
	case LBM_MSG_BOS:
		printf("[%s][%s], Beginning of Transport Session\n", msg->topic_name, msg->source);
		break;
	case LBM_MSG_EOS:
		printf("[%s][%s], End of Transport Session\n", msg->topic_name, msg->source);
		/* When verifying sequence numbers, multiple sources or EOS and new sources will cause
		* the verification to fail as we don't track the numbers on a per source basis.
		*/
		if (opts->end_on_end)
			close_recv = 1;
		break;
	case LBM_MSG_NO_SOURCE_NOTIFICATION:
		printf("[%s], no sources found for topic\n", msg->topic_name);
		break;
	default:
		printf("Unknown lbm_msg_t type %x [%s][%s]\n", msg->type, msg->topic_name, msg->source);
		break;
	}
	/* LBM automatically deletes the lbm_msg_t object unless we retain it. */
	return 0;
}

void process_cmdline(int argc, char **argv, struct Options *opts)
{
	int c, errflag = 0;

	memset(opts, 0, sizeof(*opts));

	while ((c = getopt_long(argc, argv, OptionString, OptionTable, NULL)) != EOF) {
		switch (c) {
		case 'c':
			/* Initialize configuration parameters from a file. */
			if (lbm_config(optarg) == LBM_FAILURE) {
				fprintf(stderr, "lbm_config: %s\n", lbm_errmsg());
				exit(1);
			}
			break;
		case 'E':
			opts->end_on_end = 1;
			break;
		case 'h':
			fprintf(stderr, "%s\n%s\n%s\n%s",
				argv[0], lbm_version(), purpose, usage);
			exit(0);
		case 'v':
			opts->verbose++;
			verbose = 1;
			break;
		default:
			errflag++;
			break;
		}
	}

	if (errflag || (optind == argc)) {
		/* An error occurred processing the command line - dump the LBM version, usage and exit */
		fprintf(stderr, "%s\n%s\n%s", argv[0], lbm_version(), usage);
		exit(1);
	}

	opts->topic = argv[optind];
}

int main(int argc, char **argv)
{
	struct Options *opts = &options;
	lbm_context_attr_t * ctx_attr;	/* ptr to attributes for creating context */
	lbm_topic_t *topic;				/* ptr to topic info structure for creating receiver */
	lbm_rcv_t *rcv;					/* ptr to a LBM receiver object */
	size_t optlen;					/* to be set to length of retrieved data in LBM getopt calls */

#if defined(_WIN32)
	{
		WSADATA wsadata;
		int status;

		/* Windows socket setup code */
		if ((status = WSAStartup(MAKEWORD(2, 2), &wsadata)) != 0) {
			fprintf(stderr, "%s: WSA startup error - %d\n", argv[0], status);
			exit(1);
		}
	}
#else
	/*
	* Ignore SIGPIPE on UNIXes which can occur when writing to a socket
	* with only one open end point.
	*/
	signal(SIGPIPE, SIG_IGN);
#endif /* _WIN32 */

	/* Process command line options */
	process_cmdline(argc, argv, opts);

	/* Initialize logging callback */
	if (lbm_log(lbm_log_msg, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_log: %s\n", lbm_errmsg());
		exit(1);
	}

	/* Retrieve default / configuration-modified context settings */
	if (lbm_context_attr_create(&ctx_attr) == LBM_FAILURE) {
		fprintf(stderr, "lbm_context_attr_create: %s\n", lbm_errmsg());
		exit(1);
	}
	{
		/*
		* Since we are manually validating attributes, retrieve any XML configuration
		* attributes set for this context.
		*/
		char ctx_name[256];
		size_t ctx_name_len = sizeof(ctx_name);
		if (lbm_context_attr_str_getopt(ctx_attr, "context_name", ctx_name, &ctx_name_len) == LBM_FAILURE) {
			fprintf(stderr, "lbm_context_attr_str_getopt - context_name: %s\n", lbm_errmsg());
			exit(1);
		}
		if (lbm_context_attr_set_from_xml(ctx_attr, ctx_name) == LBM_FAILURE) {
			fprintf(stderr, "lbm_context_attr_set_from_xml - context_name: %s\n", lbm_errmsg());
			exit(1);
		}
	}
	/*
	* Check if operational mode is set to "sequential" meaning that all
	* LBM processing will be done on this thread rather than on a separate
	* thread (see while loop below).
	*/
	optlen = sizeof(opmode);
	if (lbm_context_attr_getopt(ctx_attr, "operational_mode", &opmode, &optlen) == LBM_FAILURE) {
		fprintf(stderr, "lbm_context_attr_getopt - operational mode: %s\n", lbm_errmsg());
		exit(1);
	}

	/* Create LBM context according to given attribute structure */
	if (lbm_context_create(&ctx, ctx_attr, NULL, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_context_create: %s\n", lbm_errmsg());
		exit(1);
	}
	lbm_context_attr_delete(ctx_attr); /* attributes can be discarded after context creation */

	/* Look up desired topic */
	if (lbm_rcv_topic_lookup(&topic, ctx, opts->topic, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_rcv_topic_lookup: %s\n", lbm_errmsg());
		exit(1);
	}

	/* Create receiver */
	printf("Receiving on topic %s\n", opts->topic);
	if (lbm_rcv_create(&rcv, ctx, topic, rcv_handle_msg, NULL, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_rcv_create: %s\n", lbm_errmsg());
		exit(1);
	}

	while (1) {
		/*
		* Just sleep for 1 second. LBM processing is
		* done in its own thread.
		*/
		SLEEP_SEC(1);
		/* Check if we should exit */
		if (close_recv) {
			break;
		}
	}

	SLEEP_SEC(5);

	/* Clean up LBM objects */
	lbm_rcv_delete(rcv);
	lbm_context_delete(ctx);
	return 0;
}

