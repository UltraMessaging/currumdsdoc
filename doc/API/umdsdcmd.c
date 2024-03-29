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
#else
#include <unistd.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/time.h>
#include <signal.h>
#endif
#include "replgetopt.h"
#include "lbm/lbm.h"

#define MAX_CMDLEN 256
#define MAX_TARGETLEN 256
#define DEFAULT_DELAY_B4CLOSE 1

#if defined(_WIN32)
extern int optind;
extern char *optarg;
#   define SLEEP_SEC(x) Sleep((x)*1000)
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

typedef struct lbm_req_info_t_stct {
	lbm_request_t *req;
	lbm_context_t *ctx;
	int timer_id;
	char req_msg[MAX_CMDLEN];
} lbm_req_info_t;

#define RESPONSE_TIMEOUT 5000

/* Lines starting with double quote are extracted for UM documentation. */

char purpose[] = "Purpose: "
"application sends unicast immediate command messages to a umds publishing daemon."
;

char usage[] =
"Usage: umdsdcmd -T target_string -c config_file [command_string]\n"
"Available options:\n"
"  -c filename = Use LBM configuration file filename.\n"
"                Multiple config files are allowed.\n"
"                Example:  '-c file1.cfg -c file2.cfg'\n"
"  -h = help\n"
"  -L linger = linger for linger seconds before closing context\n"
"  -T target = target for unicast immediate messages (mandatory)\n"
;

const char * OptionString = "c:hL:T:";
const struct option OptionTable[] = {
	{ "config", required_argument, NULL, 'c' },
	{ "help", no_argument, NULL, 'h' },
	{ "linger", required_argument, NULL, 'L' },
	{ "target", required_argument, NULL, 'T' },
	{ NULL, 0, NULL, 0 }
};

char help_msg[] =
"*******************************************************************\n"
"* help (print this message): h                                    *\n"
"*   quit (exit application): q                                    *\n"
"*   set publishing interval: (0-N = interval in seconds)          *\n"
"*          [\"worker ID\"]   worksum 0-N  (worker summary)          *\n"
"*          [\"worker ID\"]   workdet 0-N  (worker details)          *\n"
"*                          mallinfo 0-N (malloc info)             *\n"
"*                                                                 *\n"
"*   snapshot all groups : snap                                    *\n"
"*   snapshot single group: snap (worksum|workdet|mallinfo|cfg)    *\n"
"*   snapshot single worker: \"worker ID\" snap worksum|workdet      *\n"
"*   snapshot single connection: \"conn ID\" snap connsum|conndet    *\n"
"*   Print the current version of the monitor: version             *\n"
"*******************************************************************\n"
;

char my_request_port_str[256] = "";
size_t port_str_sz = sizeof(my_request_port_str);

/* Logging callback function (given as an argument to lbm_log()) */
int lbm_log_msg(int level, const char *message, void *clientd)
{
	fprintf(stdout, "LOG Level %d: %s\n", level, message);
	return 0;
}

void print_help_exit(char **argv, int exit_value){
	fprintf(stderr, "%s\n%s\n%s\n%s",
		argv[0], lbm_version(), purpose, usage);
	exit(exit_value);
}

int done_sending = 0;

int handle_response(lbm_request_t *req, lbm_msg_t *msg, void *clientd) {
	lbm_req_info_t *req_info = (lbm_req_info_t *)clientd;

	char text[256] = "";
	if (msg->len <= 255) {
		strncpy(text, msg->data, msg->len);
		text[msg->len] = '\0';
	}

	switch (msg->type) {
	case LBM_MSG_RESPONSE:
		fprintf(stdout, "Response received [%s] from [%s][%u], %lu bytes\n", text, msg->source, msg->sequence_number, (unsigned long)msg->len);
		break;
	default:
		fprintf(stderr, "Unknown (unsupported) lbm_msg_t type 0x%x [%s]\n", msg->type, msg->source);
		break;
	}
	lbm_cancel_timer(req_info->ctx, req_info->timer_id, NULL);
	lbm_request_delete(req_info->req);
	free(req_info);
	return 0;
}

int handle_timer(lbm_context_t *ctx, const void *clientd) {
	lbm_req_info_t *req_info = (lbm_req_info_t *)clientd;

	fprintf(stderr,"Command [%s] timed out!\n", req_info->req_msg);
	lbm_request_delete(req_info->req);
	free(req_info);
	return 0;
}

void send_request(lbm_context_t *ctx, char *target, char *command) {
	lbm_req_info_t *req_info = malloc(sizeof(lbm_req_info_t));
	memset(req_info, 0, sizeof(lbm_req_info_t));
	memcpy(req_info->req_msg, command, strlen(command));
	req_info->ctx = ctx;
	if ((req_info->timer_id = lbm_schedule_timer(ctx, handle_timer, req_info, NULL, RESPONSE_TIMEOUT)) == -1) {
		fprintf(stderr, "lbm_schedule_timer: %s\n", lbm_errmsg());
		exit(1);
	}
	if (lbm_unicast_immediate_request(&(req_info->req), ctx, target, NULL, command, strlen(command), handle_response, req_info, NULL, 0) == LBM_FAILURE) {
		fprintf(stderr, "lbm_unicast_immediate_request: %s\n", lbm_errmsg());
		exit(1);
	}
}

void print_help() {
	fprintf(stdout, "%s", help_msg);
}

int main(int argc, char **argv) {
	lbm_context_t *ctx;
	lbm_context_attr_t * cattr;
	char command_string[MAX_CMDLEN];
	char *command = NULL;
	char targetname[MAX_TARGETLEN] = "";
	char *target = NULL;
	int c, errflag = 0;
	int linger = DEFAULT_DELAY_B4CLOSE;

#if defined(_WIN32)
	{
		WSADATA wsadata;
		int status;

		/* Code to initialize socket interface on Windows */
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

	while ((c = getopt_long(argc, argv, OptionString, OptionTable, NULL)) != EOF) {
		switch (c) {
		case 'c':
			/* Initialize configuration parameters from a file. */
			if (lbm_config(optarg) == LBM_FAILURE) {
				fprintf(stderr, "lbm_config: %s\n", lbm_errmsg());
				exit(1);
			}
			break;
		case 'h':
			print_help_exit(argv, 0);
		case 'L':
			linger = atoi(optarg);
			break;
		case 'T':
			strncpy(targetname, optarg, sizeof(targetname));
			target = targetname;
			break;
		default:
			errflag++;
			break;
		}
	}

	if (errflag != 0) {
		print_help_exit(argv, 1);
	}
	if (argc > optind) {
		memset(command_string, 0, MAX_CMDLEN);
		strncpy(command_string, argv[optind], sizeof(command_string));
		command = command_string;
		fprintf(stdout, "command_string: %s\n", command_string);
	}
	if (target == NULL) {
		fprintf(stderr, "Command line error: the '-T target_string' option is mandatory\n");
		print_help_exit(argv, 1);
	}
	/* Initialize logging callback */
	if (lbm_log(lbm_log_msg, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_log: %s\n", lbm_errmsg());
		exit(1);
	}
	/* Retrieve current context settings */
	if (lbm_context_attr_create(&cattr) == LBM_FAILURE) {
		fprintf(stderr, "lbm_context_attr_create: %s\n", lbm_errmsg());
		exit(1);
	}
	/* Create LBM context passing in any new context level attributes */
	if (lbm_context_create(&ctx, cattr, NULL, NULL) == LBM_FAILURE) {
		fprintf(stderr, "lbm_context_create: %s\n", lbm_errmsg());
		exit(1);
	}
	lbm_context_attr_delete(cattr);
	fprintf(stdout, "Sending unicast immediate messages to target: <%s>\n", target);
	{
		size_t optlen = sizeof(my_request_port_str);
		if (lbm_context_str_getopt(ctx, "request_tcp_port", my_request_port_str, &optlen) == LBM_FAILURE) {
			fprintf(stderr, "lbm_context_str_getopt(request_tcp_port): %s\n", lbm_errmsg());
			exit(1);
		}
	}
	if (command != NULL) {
		send_request(ctx, target, command_string);
	}
	else {
		print_help();
		while (1) {
			if (fgets(command_string, MAX_CMDLEN - 1, stdin) == NULL)
				break;
			if (command_string[0] == 'q')
				break;
			if (command_string[0] == 'h') {
				print_help();
			}
			else {
				if(strlen(command_string) == 1) continue;
				command_string[strlen(command_string) - 1] = 0;
				send_request(ctx, target, command_string);
			}
		}
	}
	fprintf(stdout, "Lingering for %d seconds...", linger);
	fflush(stdout);
	SLEEP_SEC(linger);
	fprintf(stdout, "\n");

	lbm_context_delete(ctx);
	ctx = NULL;
	return 0;
}

