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

#include "VLCNative.h"

VLCNative::VLCNative() {
	this->argc = 0;
	this->argv = NULL;
	this->input = '\0';
	this->sout = '\0';
	this->medianame = '\0';
	this->seekPosition = 0.0f;
	this->state = NEW;
}

void VLCNative::setArguments(int argc, char **argv) {
	this->argc = argc;
	this->argv = argv;
}

void VLCNative::setInput(const char *input) {
	this->input = input;
}

void VLCNative::setSout(const char *sout) {
	this->sout = sout;
}

void VLCNative::setMediaName(const char *medianame) {
	this->medianame = medianame;
}

void VLCNative::startTranscoding() {
	DEBUG("VLCNative::startTranscoding called");

	// init vlc
	vlc = libvlc_new(this->argc, this->argv);
	libvlc_set_user_agent(vlc, USER_AGENT, HTTP_USER_AGENT);

	// open log
   libvlc_set_log_verbosity(vlc, 3);
   log = libvlc_log_open(vlc);

	// register for events
	eventManager = libvlc_vlm_get_event_manager(vlc);
	libvlc_event_attach(eventManager, libvlc_VlmMediaInstanceStarted, event_handler_started, (void*)this);
	libvlc_event_attach(eventManager, libvlc_VlmMediaInstanceStatusPlaying, event_handler_status_playing, (void*)this);
	libvlc_event_attach(eventManager, libvlc_VlmMediaInstanceStatusError, event_handler_status_error, (void*)this);
	libvlc_event_attach(eventManager, libvlc_VlmMediaInstanceStatusEnd, event_handler_status_end, (void*)this);

	// start broadcast
	libvlc_vlm_add_broadcast(vlc, medianame, input, sout, 0, NULL, true, false);
	this->state = CREATED;
	DEBUG("VLCNative: created media");

	// and start playing
   libvlc_vlm_play_media(vlc, medianame);
	DEBUG("VLCNative: started media")
	handleMessages();
}

void VLCNative::seek(float position) {
	DEBUG("VLCNative::seek called with pos={0}", position);
	handleMessages();
	libvlc_vlm_seek_media(vlc, medianame, position);
}

VLCNativeState VLCNative::getState() {
	return this->state;
}

float VLCNative::getPosition() {
	handleMessages();
	float ret = libvlc_vlm_get_media_instance_position(vlc, medianame, 0);
	DEBUG("VLCNative::getPosition = {0}", ret);
	return ret;
}

int VLCNative::getTime() {
	handleMessages();
	int ret = libvlc_vlm_get_media_instance_time(vlc, medianame, 0);
	DEBUG("VLCNative::getTime = {0}", ret);
	return ret;
}

void VLCNative::stopTranscoding() {
	handleMessages();
	DEBUG("VLCNative: stop transcoding");
   libvlc_vlm_stop_media(vlc, medianame);
	DEBUG("VLCNative: stopped media");
   libvlc_vlm_release(vlc);
	DEBUG("VLCNative: released vlm");
   libvlc_release(vlc);
	DEBUG("VLCNative: released libvlc");
}

void VLCNative::mediaInstanceStarted(const libvlc_event_t *event_data) {
	DEBUG("VLCNative: event started");
	this->state = STARTED;

	// seek if needed
	if(this->seekPosition > 0.01f) {
		int ret = libvlc_vlm_seek_media(vlc, medianame, this->seekPosition);
		DEBUG("VLCNative: seeked to {0}, return value {1}", this->seekPosition, ret)
	}
}

void VLCNative::mediaInstanceStatusPlaying(const libvlc_event_t *event_data) {
	DEBUG("VLCNative: event status playing");
	this->state = PLAYING;
}

void VLCNative::mediaInstanceStatusError(const libvlc_event_t *event_data) {
	DEBUG("VLCNative: event status error");
	this->state = ERROR;
}

void VLCNative::mediaInstanceStatusEnd(const libvlc_event_t *event_datas) {
	DEBUG("VLCNative: event status end");
	this->state = FINISHED;
}

void VLCNative::handleMessages() {
#ifdef DEBUG_CPPCLI_CONSOLE
	if(this->state != NEW) {
		libvlc_log_iterator_t *log_iter = libvlc_log_get_iterator(log);
		libvlc_log_message_t messageBuffer;
		while(libvlc_log_iterator_has_next(log_iter)) {
	      libvlc_log_message_t *message = libvlc_log_iterator_next(log_iter, &messageBuffer);
			const char *header = message->psz_header == NULL ? "[null]" : message->psz_header;
			DEBUG("VLCMSG: {0} {1} {2} {3} {4}", message->i_severity, gcstring(header), gcstring(message->psz_type), gcstring(message->psz_name), gcstring(message->psz_message));
		}
		libvlc_log_clear(log);
	   libvlc_log_iterator_free(log_iter);
	}
#endif
}

void event_handler_started(const libvlc_event_t *event_data, void *object_void) {
	VLCNative *object = (VLCNative*)object_void;
	object->mediaInstanceStarted(event_data);
}

void event_handler_status_playing(const libvlc_event_t *event_data, void *object_void) {
	VLCNative *object = (VLCNative*)object_void;
	object->mediaInstanceStatusPlaying(event_data);
}

void event_handler_status_error(const libvlc_event_t *event_data, void *object_void) {
	VLCNative *object = (VLCNative*)object_void;
	object->mediaInstanceStatusError(event_data);
}

void event_handler_status_end(const libvlc_event_t *event_data, void *object_void) {
	VLCNative *object = (VLCNative*)object_void;
	object->mediaInstanceStatusEnd(event_data);
}