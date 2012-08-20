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

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "common.h"

extern "C" {
	#include <vlc/libvlc.h>
	#include <vlc/libvlc_media.h>
	#include <vlc/libvlc_events.h>
	#include <vlc/libvlc_vlm.h>
}

enum VLCNativeState {
	NEW = 0,
	CREATED = 1,
	STARTED = 2,
	PLAYING = 3,
	ERROR = 4,
	FINISHED = 5
};

public class VLCNative {
public:
	VLCNative();

	void setArguments(int argc, char **argv);
	void setInput(const char *input);
	void setSout(const char *sout);
	void setMediaName(const char *medianame);
	void startTranscoding();

	VLCNativeState getState();
	void seek(float position);

	float getPosition();
	int getTime();

	void stopTranscoding();

	void mediaInstanceStarted(const libvlc_event_t *event_data);
	void mediaInstanceStatusPlaying(const libvlc_event_t *event_data);
	void mediaInstanceStatusError(const libvlc_event_t *event_data);
	void mediaInstanceStatusEnd(const libvlc_event_t *event_data);

private:
	void handleMessages();

	const char *medianame;
	const char *input;
	const char *sout;
	int argc;
	char **argv;
	float seekPosition;
	VLCNativeState state;

	libvlc_instance_t *vlc;
	libvlc_log_t *log;
	libvlc_event_manager_t *eventManager;
};

void event_handler_started(const libvlc_event_t *event_data, void *data_void);
void event_handler_status_playing(const libvlc_event_t *event_data, void *data_void);
void event_handler_status_error(const libvlc_event_t *event_data, void *data_void);
void event_handler_status_end(const libvlc_event_t *event_data, void *data_void);