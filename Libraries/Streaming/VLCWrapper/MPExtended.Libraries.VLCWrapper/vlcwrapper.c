// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.

// This is a wrapper around libvlc that does almost the same as vlc, but
// also prints the progress of streaming to stdout, so that we can return
// it to the client.
//
// Usage of libvlc on Windows:
// - You need a full VLC installation with the sdk folder from the
//   standalone .zip file
// - Create .lib file (http://wiki.videolan.org/GenerateLibFromDll)
// - Change include and library directories
//
// There is some preprocessor magic to make it usable on Linux too.
// There is probably a bith too much preprocessor magic here anyway,
// but it works. 

#ifdef __unix__
#define PLATFORM_LINUX
#endif
#ifdef _MSC_VER
#define PLATFORM_WIN32
#endif

#include <vlc/libvlc.h>
#include <vlc/libvlc_media.h>
#include <vlc/libvlc_events.h>
#include <vlc/libvlc_vlm.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef PLATFORM_LINUX
#include <unistd.h>
#include <stdbool.h>
#endif

#ifdef PLATFORM_WIN32
#include <Windows.h>
#define true 1
#define false 0
#endif

#define ENDLN(x) (x "\n")

#define USER_AGENT "VLC Wrapper for MPExtended"
#define HTTP_USER_AGENT "VLCWrapper/0.1 MPExtended/0.3.9"

#define MEDIA_NAME "Stream"

#define LOG_INTERVAL 500

#define STATE_NULL 0
#define STATE_STARTED 1
#define STATE_PLAYING 2
#define STATE_FINISHED 3
#define STATE_ERROR 4

const char *state_name[] = {
   "null",
   "started",
   "playing",
   "finished",
   "error"
};

// maybe this var should be locked when writing it
int global_state = STATE_NULL;

void event_callback(const libvlc_event_t *event_data, void *data_void) {
   int data = *(int*)data_void;
   global_state = data;
   fprintf(stdout, ENDLN("S %s"), state_name[global_state]);
	fflush(stdout);
}

void register_event(libvlc_event_manager_t *eventManager, libvlc_event_type_t event_type, int type) {
   int *data = (int*)malloc(sizeof(int));
   *data = type;
   libvlc_event_attach(eventManager, event_type, event_callback, (void*)data);
}

void handle_messages(libvlc_log_t *log) {
   libvlc_log_iterator_t *log_iter = libvlc_log_get_iterator(log);
   libvlc_log_message_t messageBuffer;
   while(libvlc_log_iterator_has_next(log_iter)) {
      libvlc_log_message_t *message = libvlc_log_iterator_next(log_iter, &messageBuffer);
      const char *header = message->psz_header == NULL ? "[null]" : message->psz_header;
      fprintf(stderr, ENDLN("%d %s %s %s %s"), message->i_severity, header, message->psz_type, message->psz_name, message->psz_message);
   }
   libvlc_log_clear(log);
   libvlc_log_iterator_free(log_iter);
}

void millisleep(int millisecs) {
#if defined(PLATFORM_LINUX)
   usleep(millisecs * 1000);
#elif defined(PLATFORM_WIN32)
    Sleep(millisecs);
#endif
}

int main(int argc, char **argv) {
   // MSVC wants variable declarations at start of function
	int i;
	char **vlc_argv;
	int nr;
	libvlc_instance_t *vlc;
	libvlc_log_t *log;
	libvlc_event_manager_t *eventManager;

   // init arguments
   if(argc < 3) {
      printf(ENDLN("Usage: vlcwrapper <input> <soutstring> [optional vlc arguments]"));
      return 1;
   }
   
   // init state machine
   global_state = STATE_NULL;

   // arguments (you shouldn't need these in normal usage; but we don't support all arguments by ourself yet)
   nr = argc - 3;
   vlc_argv = (char**)malloc(sizeof(char)*nr);
   for(i = 3; i < argc; i++) {
      vlc_argv[i-3] = (char*)malloc(sizeof(char)*strlen(argv[i]));
      strcpy(vlc_argv[i-3], argv[i]);
   }
   for(i = 0; i < nr; i++)
      fprintf(stdout, ENDLN("A %d %s"), i, vlc_argv[i]);

   // init vlc
   vlc = libvlc_new(0, NULL);
   libvlc_set_user_agent(vlc, USER_AGENT, HTTP_USER_AGENT);
   
   // open log
   libvlc_set_log_verbosity(vlc, 3);
   log = libvlc_log_open(vlc);
    
   // register for some events
   eventManager = libvlc_vlm_get_event_manager(vlc);
   register_event(eventManager, libvlc_VlmMediaInstanceStarted, STATE_STARTED);
   register_event(eventManager, libvlc_VlmMediaInstanceStatusPlaying, STATE_PLAYING);
   register_event(eventManager, libvlc_VlmMediaInstanceStatusError, STATE_ERROR);
   register_event(eventManager, libvlc_VlmMediaInstanceStatusEnd, STATE_FINISHED);

   // start broadcast    
   libvlc_vlm_add_broadcast(vlc, MEDIA_NAME, argv[1], argv[2], 0, NULL, true, false);
   libvlc_vlm_play_media(vlc, MEDIA_NAME);
   
   // wait till it's started or errored
   while(global_state != STATE_ERROR && global_state != STATE_PLAYING)
      millisleep(100);
   
   // let it play till it's ended
   while(global_state == STATE_PLAYING) {
      handle_messages(log);
      fprintf(stdout, ENDLN("P %d, %.9f"), 
             libvlc_vlm_get_media_instance_time(vlc, MEDIA_NAME, 0),
             libvlc_vlm_get_media_instance_position(vlc, MEDIA_NAME, 0));
		fflush(stdout);
      millisleep(LOG_INTERVAL);
   }

   // and stop
   libvlc_vlm_stop_media(vlc, MEDIA_NAME);
   libvlc_vlm_release(vlc);
   libvlc_release(vlc);
   return 0;
}