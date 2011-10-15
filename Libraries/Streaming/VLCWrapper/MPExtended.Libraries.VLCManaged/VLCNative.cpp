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
   libvlc_vlm_play_media(vlc, medianame);
}

void VLCNative::seek(float position) {
	libvlc_vlm_seek_media(vlc, medianame, position);
}

float VLCNative::getPosition() {
	return libvlc_vlm_get_media_instance_position(vlc, medianame, 0);
}

int VLCNative::getTime() {
	return libvlc_vlm_get_media_instance_time(vlc, medianame, 0);
}

void VLCNative::stopTranscoding() {
   libvlc_vlm_stop_media(vlc, medianame);
   libvlc_vlm_release(vlc);
   libvlc_release(vlc);
}

void VLCNative::mediaInstanceStarted(const libvlc_event_t *event_data) {
}

void VLCNative::mediaInstanceStatusPlaying(const libvlc_event_t *event_data) {
}

void VLCNative::mediaInstanceStatusError(const libvlc_event_t *event_data) {
}

void VLCNative::mediaInstanceStatusEnd(const libvlc_event_t *event_datas) {
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