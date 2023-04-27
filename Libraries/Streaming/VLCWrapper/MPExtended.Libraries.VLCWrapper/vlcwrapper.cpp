// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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

#if defined(_MSC_VER)
#include <BaseTsd.h>
typedef SSIZE_T ssize_t;
#endif

#include <vlc/libvlc.h>
#include <vlc/libvlc_media.h>
#include <vlc/libvlc_events.h>
#include <vlc/libvlc_media_player.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <Windows.h>

// missing C99 support...
#define snwprintf _snwprintf_s

// missing stdbool.h
#define bool _Bool
#define true 1
#define false 0

#define ENDLN(x) (x L"\n")

#define LOG_INTERVAL 500

#define VERSION L"0.2.1"
#define USER_AGENT L"VLC Wrapper for MPExtended"
#define HTTP_USER_AGENT L"VLCWrapper/" VERSION

#define STATE_NULL 0
#define STATE_STARTED 1
#define STATE_PLAYING 2
#define STATE_FINISHED 3
#define STATE_ERROR 4

const wchar_t *state_name[] = {
   L"null",
   L"started",
   L"playing",
   L"finished",
   L"error"
};

// maybe this var should be locked when writing to it...
int global_state = STATE_NULL;

char* to_utf8(wchar_t *buffer) {
   int chars = WideCharToMultiByte(CP_UTF8, 0, buffer, -1, NULL, 0, NULL, NULL);
   if (chars == 0) return "";
	char* newbuffer = (char*)malloc(chars * sizeof(char));
   WideCharToMultiByte(CP_UTF8, 0, buffer, -1, newbuffer, chars, NULL, NULL); 
	return newbuffer;
}

void event_callback(const libvlc_event_t *event_data, void *data_void) {
   int data = *(int*)data_void;
   global_state = data;
   fwprintf(stdout, ENDLN(L"S %s"), state_name[global_state]);
   fflush(stdout);
}

void register_event(libvlc_event_manager_t *eventManager, libvlc_event_type_t event_type, int type) {
   int *data = (int*)malloc(sizeof(int));
   *data = type;
   libvlc_event_attach(eventManager, event_type, event_callback, (void*)data);
}

void millisleep(int millisecs) {
   Sleep(millisecs);
}

int wmain(int argc, wchar_t **argv) {
   // having these declarations at the start of the function is only needed for C compatibility
   int i;
   int soutBufferSize;
   int nr;
   char **vlc_argv; 
   wchar_t *soutOption; 
   wchar_t *config; 
   wchar_t *path; 
   libvlc_instance_t *vlc;
   libvlc_media_t *media;
   libvlc_media_player_t *player;
   libvlc_event_manager_t *eventManager;

   // init arguments
   if(argc < 3) {
      fwprintf(stdout, ENDLN(L"Usage: vlcwrapper <input> <soutstring> [optional vlc arguments]"));
      return 1;
   }

	// print some information
	path = argv[1];
   config = argv[2];
   fwprintf(stdout, ENDLN(L"I VLCWrapper version %s, using VLC %hs"), VERSION, libvlc_get_version());
   fwprintf(stdout, ENDLN(L"A path %s"), path);
   fwprintf(stdout, ENDLN(L"A config %s"), config);
   
   // init state machine  
   global_state = STATE_NULL;

   // arguments (you shouldn't need these in normal usage; but we don't support all arguments by ourself yet)
   nr = argc - 3;
   vlc_argv = (char**)malloc(nr * sizeof(char*));
   for(i = 3; i < argc; i++) {
      fwprintf(stdout, ENDLN(L"A cmd %d %s"), i - 3, argv[i]);
	  vlc_argv[i - 3] = to_utf8(argv[i]);
   }

   // init vlc
   vlc = libvlc_new(nr, vlc_argv);
   libvlc_set_user_agent(vlc, to_utf8(USER_AGENT), to_utf8(HTTP_USER_AGENT));
   
   // create media and set sout string
   media = libvlc_media_new_path(vlc, to_utf8(path));
   soutBufferSize = wcslen(config) + 6;
   soutOption = (wchar_t*)malloc(soutBufferSize * sizeof(wchar_t));
   snwprintf(soutOption, soutBufferSize, soutBufferSize, L"sout=%s", config);
   libvlc_media_add_option(media, to_utf8(soutOption));
   
   // create player and listen for events
   player = libvlc_media_player_new_from_media(media);
   eventManager = libvlc_media_player_event_manager(player);
   register_event(eventManager, libvlc_MediaPlayerPlaying, STATE_PLAYING);
   register_event(eventManager, libvlc_MediaPlayerEncounteredError, STATE_ERROR);
   register_event(eventManager, libvlc_MediaPlayerEndReached, STATE_FINISHED);
   
   // start playing it
   libvlc_media_player_play(player);
   
   // wait till it's started or stopped because of an error
   while(global_state != STATE_ERROR && global_state != STATE_PLAYING)
      millisleep(100);

   // let it play till it has finished, while printing status information
   while(global_state == STATE_PLAYING) {
      fwprintf(stdout, ENDLN(L"P %d"), libvlc_media_player_get_time(player));
      fflush(stdout);
      millisleep(LOG_INTERVAL);
   }
   
   // stop playing
   libvlc_media_player_stop(player);
   
   // release objects in memory
   libvlc_media_release(media);
   libvlc_media_player_release(player);
   libvlc_release(vlc);
   return 0;
}